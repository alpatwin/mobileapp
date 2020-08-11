using CoreGraphics;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class AnnouncementViewController : ReactiveViewController<AnnouncementViewModel>
    {
        public AnnouncementViewController(AnnouncementViewModel viewModel)
            : base(viewModel, nameof(AnnouncementViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PreferredContentSize = new CGSize(288, 419);

            TitleLabel.Text = ViewModel.Announcement.Title;
            MessageLabel.Text = ViewModel.Announcement.Message;
            DismissButton.SetTitle(Resources.Dismiss, UIControlState.Normal);
            ActionButton.SetTitle(ViewModel.Announcement.CallToAction, UIControlState.Normal);

            DismissButton.Rx()
                .BindAction(ViewModel.Dismiss)
                .DisposedBy(DisposeBag);

            ActionButton.Rx()
                .BindAction(ViewModel.OpenBrowser)
                .DisposedBy(DisposeBag);
        }
    }
}

