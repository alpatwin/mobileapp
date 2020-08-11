using System;
using Accord.Math;
using Foundation;
using Metal;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.ViewModels.Settings;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers.Settings
{
    public partial class YourPlanViewController : ReactiveViewController<YourPlanViewModel>
    {
        public YourPlanViewController(YourPlanViewModel viewModel) : base(viewModel, nameof(YourPlanViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = ColorAssets.TableBackground;
            Title = Resources.YourWorkspacePlan;

            //For now this view is only shown to free users, so no need to complicate things yet
            PlanNameLabel.Text = Resources.Free;
            PlanExpirationDateLabel.Text = Resources.NeverExpires;

            var rawString = Resources.LoginToYourAccountOnTogglToSeeMore;
            var togglDotCom = "toggl.com";
            var str = new NSMutableAttributedString(rawString);
            var index = rawString.IndexOf(togglDotCom);
            var length = togglDotCom.Length;
            str.AddAttributes(new UIStringAttributes
            {
                Font = UIFont.BoldSystemFontOfSize(LoginToYourAccountLabel.Font.PointSize)
            }, new NSRange(index, length));

            LoginToYourAccountLabel.AttributedText = str;

            LoginToYourAccountLabel.Rx().Tap()
                .Subscribe(ViewModel.OpenTogglWebpage.Inputs)
                .DisposedBy(DisposeBag);

            StackView.AddArrangedSubview(createFeatureView(true, Resources.TrackingTime));
            StackView.AddArrangedSubview(createFeatureView(true, Resources.UserGroups));
            StackView.AddArrangedSubview(createFeatureView(true, Resources.SummaryAndWeeklyReports));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.BillableHours));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.Exporting));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.Rounding));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.SavedReports));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.WorkspaceLogo));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.Estimates));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.Alerts));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.Tasks));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.ProjectDashboard));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.IcalFeed));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.CustomProjectColor));
            StackView.AddArrangedSubview(createFeatureView(false, Resources.ProjectTemplate));
            StackView.AddArrangedSubview(createFeatureView(true, Resources.MobileAppDesktopAppWebAppTogglButton));

        }

        private UIView createFeatureView(bool enabled, string featureName)
        {
            var container = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            var imageView = new UIImageView(UIImage.FromBundle(enabled ? "icDoneSmall" : "icProjectDot"))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleAspectFit
            };
            container.AddSubview(imageView);
            var label = new UILabel
            {
                Text = featureName,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.SystemFontOfSize(13),
                ContentMode = UIViewContentMode.Center,
                TextColor = ColorAssets.Text2
            };
            container.AddSubview(label);

            imageView.CenterXAnchor.ConstraintEqualTo(container.LeadingAnchor, 24).Active = true;
            imageView.CenterYAnchor.ConstraintEqualTo(container.CenterYAnchor).Active = true;

            label.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor, 45).Active = true;
            label.TopAnchor.ConstraintEqualTo(container.TopAnchor).Active = true;
            label.BottomAnchor.ConstraintEqualTo(container.BottomAnchor).Active = true;
            label.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor).Active = true;

            imageView.WidthAnchor.ConstraintEqualTo(enabled ? 12 : 3).Active = true;
            imageView.HeightAnchor.ConstraintEqualTo(enabled ? 9 : 3).Active = true;

            return container;
        }
    }
}
