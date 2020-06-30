using System;
using System.Reactive;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Storage;
using Toggl.Storage.Models;

namespace Toggl.WPF.Database
{
    public class FakeTogglDatabase : ITogglDatabase
    {
        public ISingleObjectStorage<IDatabaseUser> User { get; } = new FakeSingleObjectStorage<IDatabaseUser>();
        public IRepository<IDatabaseClient> Clients { get; } = new FakeRepository<IDatabaseClient>();
        public IRepository<IDatabaseProject> Projects { get; } = new FakeRepository<IDatabaseProject>();
        public ISingleObjectStorage<IDatabasePreferences> Preferences { get; } = new FakeSingleObjectStorage<IDatabasePreferences>();
        public IRepository<IDatabaseTag> Tags { get; } = new FakeRepository<IDatabaseTag>();
        public IRepository<IDatabaseTask> Tasks { get; } = new FakeRepository<IDatabaseTask>();
        public IRepository<IDatabaseTimeEntry> TimeEntries { get; } = new FakeRepository<IDatabaseTimeEntry>();
        public IRepository<IDatabaseWorkspace> Workspaces { get; } = new FakeRepository<IDatabaseWorkspace>();
        public IRepository<IDatabaseWorkspaceFeatureCollection> WorkspaceFeatures { get; } = new FakeRepository<IDatabaseWorkspaceFeatureCollection>();
        public IIdProvider IdProvider { get; } = new FakeIdProvider();
        public ISinceParameterRepository SinceParameters { get; } = Substitute.For<ISinceParameterRepository>();
        public IObservable<Unit> Clear()
        {
            return Observable.Return(Unit.Default);
        }
    }
}
