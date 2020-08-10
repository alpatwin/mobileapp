using System.Threading.Tasks;
using Toggl.Core.UI.Navigation;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Settings;
using Xamarin.Essentials;

namespace Toggl.Core.UI.ViewModels
{
    public sealed class AnnouncementViewModel : ViewModelWithInput<Announcement>
    {
        private readonly IOnboardingStorage onboardingStorage;

        public Announcement Announcement { get; private set; }

        public ViewAction Dismiss { get; }
        public ViewAction OpenBrowser { get; }

        public AnnouncementViewModel(
            IOnboardingStorage onboardingStorage,
            ISchedulerProvider schedulerProvider,
            INavigationService navigationService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));

            this.onboardingStorage = onboardingStorage;

            Dismiss = ViewAction.FromAction(dismiss, schedulerProvider.MainScheduler);
            OpenBrowser = ViewAction.FromAsync(openBrowser, schedulerProvider.MainScheduler);
        }

        public override Task Initialize(Announcement payload)
        {
            Announcement = payload;
            return base.Initialize(payload);

        }

        public override void ViewAppeared()
        {
            // This method will be called multiple times if the user puts the app into background
            // while this VM is being shown and then goes back to the app. We must log the event
            // only once even in this case.
            //
            // We decided to put the logic into this method and not in `Initialize` because a VM
            // can be initialized, but the view might not be shown (if the initialization or
            // presentation fails for some reason).
            if (!onboardingStorage.CheckIfAnnouncementWasShown(Announcement.Id))
            {
                onboardingStorage.MarkAnnouncementAsShown(Announcement.Id);
            }
        }

        private void dismiss()
        {
            Close();
        }

        private async Task openBrowser()
        {
            await Browser.OpenAsync(Announcement.Url, BrowserLaunchMode.External);

            Close();
        }
    }
}
