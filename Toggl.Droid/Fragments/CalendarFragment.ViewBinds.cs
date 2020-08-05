using System;
using System.Linq;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Toggl.Droid.Extensions;
using Toggl.Droid.Views;
using AppBarLayout = Google.Android.Material.AppBar.AppBarLayout;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Toggl.Droid.Fragments
{
    public partial class CalendarFragment
    {
        private TextView headerTimeEntriesDurationTextView;
        private TextView headerDateTextView;
        private LockableViewPager calendarViewPager;
        private LockableViewPager calendarWeekStripePager;
        private ConstraintLayout calendarWeekStripeLabelsContainer;
        private TextView[] calendarWeekStripeHeaders;
        private AppBarLayout appBarLayout;
        private Toolbar toolbar;
        private AnimatedFloatingActionButton playButton;
        private FloatingActionButton stopButton;
        private View runningEntryCardFrame;
        private TextView timeEntryCardTimerLabel;
        private TextView timeEntryCardDescriptionLabel;
        private TextView timeEntryCardAddDescriptionLabel;
        private View timeEntryCardDotContainer;
        private View timeEntryCardDotView;
        private TextView timeEntryCardProjectClientTaskLabel;


        protected override void InitializeViews(View view)
        {
            headerDateTextView = view.FindViewById<TextView>(Resource.Id.HeaderDateTextView);
            headerTimeEntriesDurationTextView = view.FindViewById<TextView>(Resource.Id.HeaderTimeEntriesDurationTextView);
            appBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.HeaderView);
            calendarViewPager = view.FindViewById<LockableViewPager>(Resource.Id.Pager);
            calendarWeekStripePager = view.FindViewById<LockableViewPager>(Resource.Id.WeekStripePager);
            calendarWeekStripeLabelsContainer = view.FindViewById<ConstraintLayout>(Resource.Id.CalendarWeekStripeLabels);
            calendarWeekStripeHeaders = calendarWeekStripeLabelsContainer.GetChildren().Cast<TextView>().ToArray();

            if (calendarWeekStripeHeaders.Length != NumberOfDaysInTheWeek) {
                throw new ArgumentOutOfRangeException($"Week headers should contain exactly {NumberOfDaysInTheWeek} text views");
            }

            toolbar = view.FindViewById<Toolbar>(Resource.Id.Toolbar);
            runningEntryCardFrame = view.FindViewById(Resource.Id.MainRunningTimeEntrySheet);
            playButton = view.FindViewById<AnimatedFloatingActionButton>(Resource.Id.MainPlayButton);
            stopButton = view.FindViewById<FloatingActionButton>(Resource.Id.MainStopButton);
            timeEntryCardTimerLabel = view.FindViewById<TextView>(Resource.Id.MainRunningTimeEntryTimerLabel);
            timeEntryCardDescriptionLabel = view.FindViewById<TextView>(Resource.Id.MainRunningTimeEntryDescription);
            timeEntryCardAddDescriptionLabel = view.FindViewById<TextView>(Resource.Id.MainRunningTimeEntryAddDescriptionLabel);
            timeEntryCardDotContainer = view.FindViewById(Resource.Id.MainRunningTimeEntryProjectDotContainer);
            timeEntryCardDotView = view.FindViewById(Resource.Id.MainRunningTimeEntryProjectDotView);
            timeEntryCardProjectClientTaskLabel = view.FindViewById<TextView>(Resource.Id.MainRunningTimeEntryProjectClientTaskLabel);
            timeEntryCardAddDescriptionLabel.Text = Shared.Resources.AddDescription;
        }
    }
}
