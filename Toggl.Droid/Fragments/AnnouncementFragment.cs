using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Fragments
{
    public sealed class AnnouncementFragment : ReactiveDialogFragment<AnnouncementViewModel>
    {
        private TextView title;
        private TextView message;
        private Button mainActionButton;
        private TextView dismissButton;

        public AnnouncementFragment() { }

        public AnnouncementFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            Cancelable = false;
            var view = inflater.Inflate(Resource.Layout.AnnouncementFragment, null);

            InitializeViews(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            title.Text = ViewModel.Announcement.Title;
            message.Text = ViewModel.Announcement.Message;
            mainActionButton.Text = ViewModel.Announcement.CallToAction;
            dismissButton.Text = Shared.Resources.Dismiss;

            mainActionButton.Rx().Tap()
                .Subscribe(ViewModel.OpenBrowser.Inputs)
                .DisposedBy(DisposeBag);

            dismissButton.Rx().Tap()
                .Subscribe(ViewModel.Dismiss.Inputs)
                .DisposedBy(DisposeBag);
        }

        protected override void InitializeViews(View view)
        {
            title = view.FindViewById<TextView>(Resource.Id.Title);
            message = view.FindViewById<TextView>(Resource.Id.Message);
            mainActionButton = view.FindViewById<Button>(Resource.Id.MainActionButton);
            dismissButton = view.FindViewById<TextView>(Resource.Id.DismissButton);
        }
    }
}
