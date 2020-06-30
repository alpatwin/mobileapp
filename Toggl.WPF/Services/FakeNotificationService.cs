using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Core.Services;
using Notification = Toggl.Shared.Notification;

namespace Toggl.WPF.Services
{
    public class FakeNotificationService : INotificationService
    {
        public IObservable<Unit> Schedule(IImmutableList<Notification> notifications)
        {
            return Observable.Return(Unit.Default);
        }

        public IObservable<Unit> UnscheduleAllNotifications()
        {
            return Observable.Return(Unit.Default);
        }
    }
}
