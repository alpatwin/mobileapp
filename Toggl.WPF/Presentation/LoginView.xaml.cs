using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using Toggl.Core.UI.ViewModels;
using Toggl.Shared;

namespace Toggl.WPF.Presentation
{
    public partial class LoginView : ReactiveUserControl<LoginViewModel>
    {
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();
        public LoginView(LoginViewModel loginViewModel)
        {
            ViewModel = loginViewModel;
            InitializeComponent();
        }

        private void LoginView_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Email
            var emailTextChanged = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                h => emailTextBox.TextChanged += h,
                h => emailTextBox.TextChanged -= h
            ).Select(x => ((TextBox)x.Sender).Text);

            emailTextChanged
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept);

            ViewModel.Email
                .Select(x => x.ToString())
                .Subscribe(email => emailTextBox.Text = email);
        }
    }
}

