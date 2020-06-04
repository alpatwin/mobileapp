﻿using Realms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Networking.Sync.Pull;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Sync
{
    public class ProcessPullResultQuery : IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;
        private Func<DateTimeOffset> currentTimeProvider;
        private IResponse response;

        private IPreferences preferences = null;
        private IUser user = null;
        private ImmutableList<IWorkspace> workspaces;
        private ImmutableList<ITag> tags;
        private ImmutableList<IClient> clients;
        private ImmutableList<IProject> projects;
        private ImmutableList<ITask> tasks;
        private ImmutableList<ITimeEntry> timeEntries;

        public ProcessPullResultQuery(Func<RealmDb> realmProvider, Func<DateTimeOffset> currentTimeProvider, IResponse response)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(response, nameof(response));
            Ensure.Argument.IsNotNull(currentTimeProvider, nameof(currentTimeProvider));

            this.realmProvider = realmProvider;
            this.response = response;
            this.currentTimeProvider = currentTimeProvider;
        }

        private void unpackResponse()
        {
            user = response.User;
            preferences = response.Preferences;

            workspaces = response.Workspaces;
            tags = response.Tags;
            clients = response.Clients;
            projects = response.Projects;
            tasks = response.Tasks;

            timeEntries = response.TimeEntries;
        }

        public Unit Execute()
        {
            unpackResponse();

            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                processUser(realm);
                processPreferences(realm);

                processEntities<RealmWorkspace, IWorkspace>(realm, workspaces, customDeletionProcess: makeWorkspaceInaccessible);
                processEntities<RealmTag, ITag>(realm, tags);
                processEntities<RealmClient, IClient>(realm, clients);
                processEntities<RealmProject, IProject>(realm, projects, customDeletionProcess: removeRelatedTasks);
                processEntities<RealmTask, ITask>(realm, tasks);

                processTimeEntries(realm);

                transaction.Commit();
            }

            return Unit.Default;
        }

        private void processTimeEntries(RealmDb realm)
        {
            var serverRunningTimeEntry = (ITimeEntry)null;

            foreach (var timeEntry in timeEntries)
            {
                if (timeEntry.IsRunning())
                    serverRunningTimeEntry = timeEntry;

                var dbTimeEntry = realm.GetById<RealmTimeEntry>(timeEntry.Id);

                if (dbTimeEntry == null)
                {
                    if (timeEntry.ServerDeletedAt.HasValue)
                        continue;

                    addTimeEntry(timeEntry, realm);
                    continue;
                }

                if (timeEntry.ServerDeletedAt.HasValue)
                    realm.Remove(dbTimeEntry);
                else
                    updateTimeEntry(timeEntry, dbTimeEntry, realm);
            }

            preventMultipleRunningTimeEntries(serverRunningTimeEntry, realm);
        }

        private void preventMultipleRunningTimeEntries(ITimeEntry serverRunningTimeEntry, RealmDb realm)
        {
            var time = currentTimeProvider();

            if (serverRunningTimeEntry != null)
            {
                var otherLocallyRunningTimeEntries = realm.All<RealmTimeEntry>()
                    .Where(te => te.Duration == null && te.Id != serverRunningTimeEntry.Id)
                    .ToList();

                foreach (var runningTimeEntry in otherLocallyRunningTimeEntries)
                {
                    var duration = (int)(time - runningTimeEntry.Start).TotalSeconds;
                    runningTimeEntry.DurationBackup = runningTimeEntry.Duration = duration;
                    runningTimeEntry.SyncStatus = SyncStatus.SyncNeeded;
                }
            }
        }

        private void addTimeEntry(ITimeEntry timeEntry, RealmDb realm)
        {
            var dbTimeEntry = new RealmTimeEntry();
            dbTimeEntry.SaveSyncResult(timeEntry, realm);
            realm.Add(dbTimeEntry);
        }

        private void processUser(RealmDb realm)
        {
            var dbUser = realm.All<RealmUser>().SingleOrDefault();

            dbUser.ApiToken = user.ApiToken;
            dbUser.At = user.At;
            dbUser.Email = user.Email;
            dbUser.Fullname = user.Fullname;
            dbUser.ImageUrl = user.ImageUrl;
            dbUser.Language = user.Language;
            dbUser.Timezone = user.Timezone;

            var wasDirty = dbUser.SyncStatus == SyncStatus.SyncNeeded;
            var shouldStayDirty = false;

            // Default Workspace Id
            var commonDefaultWorkspaceId = dbUser.ContainsBackup
                ? dbUser.DefaultWorkspaceIdBackup
                : dbUser.DefaultWorkspaceId;

            dbUser.DefaultWorkspaceIdBackup = dbUser.DefaultWorkspaceId =
                ThreeWayMerge.Merge(commonDefaultWorkspaceId, dbUser.DefaultWorkspaceId, user.DefaultWorkspaceId, identifierComparison);

            shouldStayDirty |= user.DefaultWorkspaceId != dbUser.DefaultWorkspaceId;

            // Beginning of week
            var commonBeginningOfWeek = dbUser.ContainsBackup
                ? dbUser.BeginningOfWeekBackup
                : dbUser.BeginningOfWeek;

            dbUser.BeginningOfWeek = dbUser.BeginningOfWeekBackup =
                ThreeWayMerge.Merge(commonBeginningOfWeek, dbUser.BeginningOfWeek, user.BeginningOfWeek, beginningOfWeekComparison);

            shouldStayDirty |= user.BeginningOfWeek != dbUser.BeginningOfWeek;

            // the conflict is resolved, the backup is no longer needed until next local change
            dbUser.ContainsBackup = false;
            dbUser.LastSyncErrorMessage = null;

            // Update sync status depending on the way the user has changed during the 3-way merge
            dbUser.SyncStatus = wasDirty && shouldStayDirty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        private void processPreferences(RealmDb realm)
        {
            var dbPreferences = realm.All<RealmPreferences>().SingleOrDefault();

            if (dbPreferences == null)
            {
                dbPreferences = new RealmPreferences();
                dbPreferences.SaveSyncResult(preferences, realm);
                realm.Add(dbPreferences);
                return;
            }

            var wasDirty = dbPreferences.SyncStatus == SyncStatus.SyncNeeded;
            var shouldStayDirty = false;

            // Time of day format
            var commonTimeOfDayFormat = dbPreferences.ContainsBackup
                ? dbPreferences.TimeOfDayFormatBackup
                : dbPreferences.TimeOfDayFormat;

            dbPreferences.TimeOfDayFormatBackup = dbPreferences.TimeOfDayFormat =
                ThreeWayMerge.Merge(commonTimeOfDayFormat, dbPreferences.TimeOfDayFormat, preferences.TimeOfDayFormat, timeFormatComparison);

            shouldStayDirty |= !preferences.TimeOfDayFormat.Equals(dbPreferences.TimeOfDayFormat);

            // Date format
            var commonDateFormat = dbPreferences.ContainsBackup
                ? dbPreferences.DateFormatBackup
                : dbPreferences.DateFormat;

            dbPreferences.DateFormatBackup = dbPreferences.DateFormat =
                ThreeWayMerge.Merge(commonDateFormat, dbPreferences.DateFormat, preferences.DateFormat, dateformatComparison);

            shouldStayDirty |= !preferences.DateFormat.Equals(dbPreferences.DateFormat);

            // Duration format backup
            var commonDurationFormat = dbPreferences.ContainsBackup
               ? dbPreferences.DurationFormatBackup
               : dbPreferences.DurationFormat;

            dbPreferences.DurationFormatBackup = dbPreferences.DurationFormat =
                ThreeWayMerge.Merge(commonDurationFormat, dbPreferences.DurationFormat, preferences.DurationFormat, durationFormatComparison);

            shouldStayDirty |= !preferences.DurationFormat.Equals(dbPreferences.DurationFormat);

            // Collapse time entries backup
            var commonCollapseTimeEntries = dbPreferences.ContainsBackup
              ? dbPreferences.CollapseTimeEntriesBackup
              : dbPreferences.CollapseTimeEntries;

            dbPreferences.CollapseTimeEntriesBackup = dbPreferences.CollapseTimeEntries =
                ThreeWayMerge.Merge(commonCollapseTimeEntries, dbPreferences.CollapseTimeEntries, preferences.CollapseTimeEntries);

            shouldStayDirty |= preferences.CollapseTimeEntries != dbPreferences.CollapseTimeEntries;

            // the conflict is resolved, the backup is no longer needed until next local change
            dbPreferences.ContainsBackup = false;
            dbPreferences.LastSyncErrorMessage = null;

            // Update sync status depending on the way the preferences have changed during the 3-way merge
            dbPreferences.SyncStatus = wasDirty && shouldStayDirty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        private void updateTimeEntry(ITimeEntry timeEntry, RealmTimeEntry dbTimeEntry, RealmDb realm)
        {
            // Temporary hack: the list of entities from the server always contains the currently running TE
            // even if this entity hasn't change since the `since` timestamp. We need to filter it out manually.
            if (isIrrelevantForSyncing(timeEntry, realm)) return;

            var wasDirty = dbTimeEntry.SyncStatus == SyncStatus.SyncNeeded;
            var shouldStayDirty = false;

            dbTimeEntry.At = timeEntry.At;
            dbTimeEntry.ServerDeletedAt = timeEntry.ServerDeletedAt;
            dbTimeEntry.RealmUser = realm.GetById<RealmUser>(timeEntry.UserId);
            dbTimeEntry.RealmWorkspace = realm.GetById<RealmWorkspace>(timeEntry.WorkspaceId);

            // Description
            var commonDescription = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.DescriptionBackup
                : dbTimeEntry.Description;

            dbTimeEntry.DescriptionBackup = dbTimeEntry.Description =
                ThreeWayMerge.Merge(commonDescription, dbTimeEntry.Description, timeEntry.Description);

            shouldStayDirty |= timeEntry.Description != dbTimeEntry.Description;

            // ProjectId
            var commonProjectId = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.ProjectIdBackup
                : dbTimeEntry.ProjectId;

            var projectId = ThreeWayMerge.Merge(commonProjectId, dbTimeEntry.ProjectId, timeEntry.ProjectId, identifierComparison);

            dbTimeEntry.RealmProject = projectId.HasValue
                ? realm.GetById<RealmProject>(projectId.Value)
                : null;

            shouldStayDirty |= timeEntry.ProjectId != projectId;

            // Billable
            var commonBillable = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.BillableBackup
                : dbTimeEntry.Billable;

            dbTimeEntry.BillableBackup = dbTimeEntry.Billable =
                ThreeWayMerge.Merge(commonBillable, dbTimeEntry.Billable, timeEntry.Billable);

            shouldStayDirty |= timeEntry.Billable != dbTimeEntry.Billable;

            // Start
            var commonStart = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.StartBackup
                : dbTimeEntry.Start;

            dbTimeEntry.Start = ThreeWayMerge.Merge(commonStart, dbTimeEntry.Start, timeEntry.Start);

            shouldStayDirty |= timeEntry.Start != dbTimeEntry.Start;

            // Duration
            var commonDuration = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.DurationBackup
                : dbTimeEntry.Duration;

            dbTimeEntry.Duration = ThreeWayMerge.Merge(commonDuration, dbTimeEntry.Duration, timeEntry.Duration, identifierComparison);

            shouldStayDirty |= timeEntry.Duration != dbTimeEntry.Duration;

            // Task
            var commonTaskId = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.TaskIdBackup
                : dbTimeEntry.TaskId;

            var taskId = ThreeWayMerge.Merge(commonTaskId, dbTimeEntry.TaskId, timeEntry.TaskId, identifierComparison);

            dbTimeEntry.RealmTask = taskId.HasValue
                ? realm.GetById<RealmTask>(taskId.Value)
                : null;

            shouldStayDirty |= timeEntry.TaskId != taskId;

            // Tag Ids
            var commonTagIds = dbTimeEntry.ContainsBackup
                ? Arrays.NotNullOrEmpty(dbTimeEntry.TagIdsBackup)
                : Arrays.NotNullOrEmpty(dbTimeEntry.TagIds);

            var localTagIds = Arrays.NotNullOrEmpty(dbTimeEntry.TagIds);
            var serverTagIds = Arrays.NotNullOrEmpty(timeEntry.TagIds);

            var tagsIds = ThreeWayMerge.Merge(commonTagIds, localTagIds, serverTagIds, collectionEnumerableComparison);
            shouldStayDirty |= !tagsIds.SetEquals(localTagIds);

            dbTimeEntry.RealmTags.Clear();
            tagsIds
                .Select(tagId => realm.GetById<RealmTag>(tagId))
                .AddTo(dbTimeEntry.RealmTags);

            // the conflict is resolved, the backup is no longer needed until next local change
            dbTimeEntry.ContainsBackup = false;
            dbTimeEntry.LastSyncErrorMessage = null;

            // Update sync status depending on the way the time entry has changed during the 3-way merge
            dbTimeEntry.SyncStatus = wasDirty && shouldStayDirty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        private void makeWorkspaceInaccessible(RealmDb realm, RealmWorkspace workspace)
        {
            workspace.IsInaccessible = false;
        }

        private void removeRelatedTasks(RealmDb realm, RealmProject project)
        {
            project.RealmTasks
                .ToList()
                .ForEach(realm.Remove);

            realm.Remove(project);
        }

        private void processEntities<TRealmEntity, TEntity>(RealmDb realm, IEnumerable<TEntity> serverEntities, Action<RealmDb, TRealmEntity> customDeletionProcess = null)
            where TRealmEntity : RealmObject, TEntity, ISyncable<TEntity>, new()
            where TEntity : IIdentifiable, IDeletable
        {
            foreach (var entity in serverEntities)
            {
                var dbEntity = realm.GetById<TRealmEntity>(entity.Id);

                var isServerDeleted = entity.ServerDeletedAt.HasValue;

                if (dbEntity == null)
                {
                    if (isServerDeleted)
                        continue;

                    dbEntity = new TRealmEntity();
                    dbEntity.SaveSyncResult(entity, realm);
                    realm.Add(dbEntity);
                }
                else
                {
                    if (isServerDeleted)
                    {
                        if (customDeletionProcess != null)
                        {
                            customDeletionProcess?.Invoke(realm, dbEntity);
                        }
                        else
                        {
                            realm.Remove(dbEntity);
                        }
                    }
                    else
                    {
                        dbEntity.SaveSyncResult(entity, realm);
                    }
                }
            }
        }

        private bool identifierComparison(long? a, long? b)
            => a == b;

        private bool collectionEnumerableComparison<T>(T[] a, T[] b)
            => a.SetEquals(b);

        private bool beginningOfWeekComparison(BeginningOfWeek a, BeginningOfWeek b)
            => (int)a == (int)b;

        private bool timeFormatComparison(TimeFormat a, TimeFormat b)
            => a.Equals(b);

        private bool dateformatComparison(DateFormat a, DateFormat b)
            => a.Equals(b);

        private bool durationFormatComparison(DurationFormat a, DurationFormat b)
            => (int)a == (int)b;

        private bool isIrrelevantForSyncing(ITimeEntry timeEntry, RealmDb realm)
        {
            var timeEntriesSinceId = SinceParameterStorage.IdFor<ITimeEntry>();
            if (!timeEntriesSinceId.HasValue)
                throw new Exception("Time entries since parameter ID is not defined.");

            var since = realm.GetById<RealmSinceParameter>(timeEntriesSinceId.Value);
            return since != null && since.Since != null && timeEntry.At < since.Since.Value;
        }
    }
}
