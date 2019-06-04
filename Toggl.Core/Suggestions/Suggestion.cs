using System;
using System.Linq;
using Toggl.Core.Helper;
using Toggl.Core.Models;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Models;

namespace Toggl.Core.Suggestions
{
    [Preserve(AllMembers = true)]
    public sealed class Suggestion : ITimeEntryPrototype, IEquatable<Suggestion>
    {
        public string Description { get; } = "";

        public long? ProjectId { get; } = null;

        public long? TaskId { get; } = null;

        public string ProjectColor { get; } = Helper.Colors.NoProject;

        public string ProjectName { get; } = "";

        public string TaskName { get; } = "";

        public string ClientName { get; } = "";

        public bool HasProject { get; } = false;

        public long WorkspaceId { get; }

        public bool IsBillable { get; } = false;

        public long[] TagIds { get; } = Array.Empty<long>();

        public DateTimeOffset StartTime { get; }

        public TimeSpan? Duration { get; } = null;

        public SuggestionProviderType ProviderName { get; }

        internal Suggestion(IDatabaseTimeEntry timeEntry)
        {
            TaskId = timeEntry.TaskId;
            ProjectId = timeEntry.ProjectId;
            IsBillable = timeEntry.Billable;
            Description = timeEntry.Description;
            WorkspaceId = timeEntry.WorkspaceId;

            if (timeEntry.Project == null) return;

            HasProject = true;
            ProjectName = timeEntry.Project.Name;
            ProjectColor = timeEntry.Project.Color;

            ClientName = timeEntry.Project.Client?.Name ?? "";

            if (timeEntry.Task == null) return;

            TaskName = timeEntry.Task.Name;
        }

        internal Suggestion(IDatabaseTimeEntry timeEntry, SuggestionProviderType providerName)
        {
            ProviderName = providerName;

            TaskId = timeEntry.TaskId;
            ProjectId = timeEntry.ProjectId;
            IsBillable = timeEntry.Billable;
            Description = timeEntry.Description;
            WorkspaceId = timeEntry.WorkspaceId;

            if (timeEntry.Project == null) 
                return;

            HasProject = true;
            ProjectName = timeEntry.Project.Name;
            ProjectColor = timeEntry.Project.Color;

            ClientName = timeEntry.Project.Client?.Name ?? "";

            if (timeEntry.Task == null)
                return;

            TaskName = timeEntry.Task.Name;
        }
        
        public bool Equals(Suggestion other)
        {
            if (other is null)
                return false;

            return Description == other.Description
                && ProjectId == other.ProjectId
                && TaskId == other.TaskId
                && WorkspaceId == other.WorkspaceId
                && StartTime == other.StartTime
                && Duration == other.Duration
                && IsBillable == other.IsBillable
                && TagIds.SetEquals(other.TagIds);
        }
    }
}
