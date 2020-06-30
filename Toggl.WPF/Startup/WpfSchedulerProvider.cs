using System.Reactive.Concurrency;
using ReactiveUI;
using Toggl.Shared;

namespace Toggl.WPF.Startup
{
    public class WpfSchedulerProvider : ISchedulerProvider
    {
        public IScheduler MainScheduler { get; } = RxApp.MainThreadScheduler;
        public IScheduler DefaultScheduler { get; } = Scheduler.Default;
        public IScheduler BackgroundScheduler { get; } = RxApp.TaskpoolScheduler;
    }
}
