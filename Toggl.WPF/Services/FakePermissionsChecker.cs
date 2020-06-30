using System;
using System.Reactive.Linq;
using Toggl.Core.UI.Services;

namespace Toggl.WPF.Services
{
    public class FakePermissionsChecker : IPermissionsChecker
    {
        public IObservable<bool> CalendarPermissionGranted { get; }
            = Observable.Return(true);

        public IObservable<PermissionStatus> NotificationPermissionGranted { get; }
            = Observable.Return(PermissionStatus.Authorized);
    }
}
