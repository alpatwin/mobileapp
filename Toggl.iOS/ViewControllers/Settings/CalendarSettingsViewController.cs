using MvvmCross.Platforms.Ios.Presenters.Attributes;
using Toggl.Core.UI.ViewModels.Settings;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.iOS.ViewSources;
using Toggl.Shared.Extensions;
using Colors = Toggl.Core.UI.Helper.Colors;
using FoundationResources = Toggl.Shared.Resources;

namespace Toggl.iOS.ViewControllers.Settings
{
    [MvxChildPresentation]
    public sealed partial class CalendarSettingsViewController : ReactiveViewController<CalendarSettingsViewModel>
    {
        private const int tableViewHeaderHeight = 106;

        public CalendarSettingsViewController()
            : base(nameof(CalendarSettingsViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.Title = FoundationResources.CalendarSettingsTitle;

            var header = CalendarSettingsTableViewHeader.Create();
            UserCalendarsTableView.TableHeaderView = header;
            header.TranslatesAutoresizingMaskIntoConstraints = false;
            header.HeightAnchor.ConstraintEqualTo(tableViewHeaderHeight).Active = true;
            header.WidthAnchor.ConstraintEqualTo(UserCalendarsTableView.WidthAnchor).Active = true;
            header.SetCalendarPermissionStatus(ViewModel.PermissionGranted);

            var source = new SelectUserCalendarsTableViewSource(UserCalendarsTableView);
            source.SectionHeaderBackgroundColor = Colors.Settings.Background.ToNativeColor();
            UserCalendarsTableView.Source = source;

            ViewModel.Calendars
                .Subscribe(UserCalendarsTableView.Rx().ReloadSections(source))
                .DisposedBy(DisposeBag);

            header.EnableCalendarAccessTapped
                .Subscribe(ViewModel.RequestAccess.Inputs)
                .DisposedBy(DisposeBag);

            source.Rx().ModelSelected()
                .Subscribe(ViewModel.SelectCalendar.Inputs)
                .DisposedBy(DisposeBag);
        }
    }
}

