﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Exceptions;
using Toggl.Core.Extensions;
using Toggl.Core.Helper;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Models;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels.Extensions;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Shared.Models;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels
{
    public class OnboardingViewModel : ViewModelWithInput<OnboardingParameters>
    {
        private readonly IPlatformInfo platformInfo;
        private readonly ITimeService timeService;
        private readonly IAnalyticsService analyticsService;
        private readonly IUserAccessManager userAccessManager;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly IInteractorFactory interactorFactory;
        private readonly ISchedulerProvider schedulerProvider;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<bool> isForAccountLinking = new BehaviorSubject<bool>(false);

        private Email emailForLinking;
        private string confirmationCode;
        private ThirdPartyLoginInfo loginInfo;
        private List<bool> onboardingPagesViewed = new List<bool> { false, false, false };

        public IObservable<bool> IsLoading { get; }
        public IObservable<bool> IsForAccountLinking { get; }
        public IObservable<Unit> GoToNextPageObservable { get; }

        public ViewAction ContinueWithApple { get; }
        public ViewAction ContinueWithGoogle { get; }
        public ViewAction ContinueWithEmail { get; }
        public ViewAction SingleSignOn { get; }
        public ViewAction SingleSignOnCancel { get; }


        public InputAction<OnboardingScrollParameters> OnOnboardingScroll { get; }

        public OnboardingViewModel(
            ISchedulerProvider schedulerProvider,
            IPlatformInfo platformInfo,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            IUserAccessManager userAccessManager,
            ILastTimeUsageStorage lastTimeUsageStorage,
            IRxActionFactory rxActionFactory,
            IInteractorFactory interactorFactory,
            INavigationService navigationService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(platformInfo, nameof(platformInfo));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.platformInfo = platformInfo;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.userAccessManager = userAccessManager;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.interactorFactory = interactorFactory;
            this.schedulerProvider = schedulerProvider;

            ContinueWithApple = rxActionFactory.FromAction(continueWithApple);
            ContinueWithGoogle = rxActionFactory.FromAction(continueWithGoogle);
            ContinueWithEmail = rxActionFactory.FromAsync(continueWithEmail);
            SingleSignOn = rxActionFactory.FromAsync(singleSignOn);
            SingleSignOnCancel = rxActionFactory.FromAction(singleSignOnCancel);
            OnOnboardingScroll = rxActionFactory.FromAction<OnboardingScrollParameters>(onOnboardingScroll);

            IsLoading = isLoadingSubject
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            IsForAccountLinking = isForAccountLinking
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            GoToNextPageObservable = Observable
                .Interval(TimeSpan.FromSeconds(5), schedulerProvider.MainScheduler)
                .SelectValue(Unit.Default)
                .AsDriver(schedulerProvider);
        }

        public override Task Initialize(OnboardingParameters payload)
        {
            isForAccountLinking.OnNext(payload.IsForAccountLinking);
            emailForLinking = payload.Email;
            confirmationCode = payload.ConfirmationCode;

            return base.Initialize(payload);
        }

        public override void ViewDestroyed()
        {
            base.ViewDestroyed();
            disposeBag?.Dispose();
        }

        private void continueWithApple()
        {
            trackViewedPages();
            analyticsService.ContinueWithApple.Track();
            tryLoggingIn(ThirdPartyLoginProvider.Apple);
        }

        private void continueWithGoogle()
        {
            trackViewedPages();
            analyticsService.ContinueWithGoogle.Track();
            tryLoggingIn(ThirdPartyLoginProvider.Google);
        }

        private Task continueWithEmail()
        {
            analyticsService.ContinueWithEmail.Track();
            trackViewedPages();
            if (lastTimeUsageStorage.LastLogin == null && !isForAccountLinking.Value)
            {
                return Navigate<SignUpViewModel, CredentialsParameter>(CredentialsParameter.Empty);
            }
            else
            {
                return Navigate<LoginViewModel, CredentialsParameter>(CredentialsParameter.With(emailForLinking, Password.Empty, isForAccountLinking.Value, confirmationCode));
            }
        }

        private void singleSignOnCancel()
        {
            isForAccountLinking.OnNext(false);
        }

        private Task singleSignOn()
        {
            analyticsService.LoginWithSso.Track();
            return Navigate<SsoViewModel>();
        }

        private async void onAuthenticated(ITogglApi api)
        {
            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            interactorFactory.GetCurrentUser().Execute()
                .Select(u => u.Id)
                .Subscribe(analyticsService.SetUserId)
                .DisposedBy(disposeBag);

            await UIDependencyContainer.Instance.SyncManager.ForceFullSync();

            await interactorFactory.CreateOnboardingTimeEntryIfNeeded().Execute();

            await this.ssoLinkIfNeededAndNavigate(api, analyticsService, isForAccountLinking.Value, emailForLinking, confirmationCode);
        }

        private async void tryLoggingIn(ThirdPartyLoginProvider provider)
        {
            var authenticationMethod = provider == ThirdPartyLoginProvider.Google
                ? AuthenticationMethod.Google
                : AuthenticationMethod.Apple;

            View?.GetLoginInfo(provider)
                .Do(loginInfo => this.loginInfo = loginInfo)
                .Do(_ => isLoadingSubject.OnNext(true))
                .SelectMany(loginInfo => userAccessManager.ThirdPartyLogin(provider, loginInfo))
                .Track(analyticsService.Login, authenticationMethod)
                .Subscribe(onAuthenticated, ex => onLoginFailure(provider, ex))
                .DisposedBy(disposeBag);
        }

        private async void onLoginFailure(ThirdPartyLoginProvider provider, Exception exception)
        {
            if (exception is ThirdPartyLoginException loginException)
            {
                isLoadingSubject.OnNext(false);
                if (!loginException.LoginWasCanceled)
                {
                    await View.Alert(Resources.Oops, Resources.GenericLoginError, Resources.Ok);
                }
                return;
            }

            if (exception != null)
            {
                analyticsService.UnknownLoginFailure.Track(exception.GetType().FullName, exception.Message);
                analyticsService.TrackAnonymized(exception);
            }

            trySignUp(provider);
        }

        private async void trySignUp(ThirdPartyLoginProvider provider)
        {
            var country = await confirmCountryAndTermsOfService();

            if (country == null)
            {
                isLoadingSubject.OnNext(false);
                return;
            }

            var authenticationMethod = provider == ThirdPartyLoginProvider.Google
                ? AuthenticationMethod.Google
                : AuthenticationMethod.Apple;

            interactorFactory.GetSupportedTimezones().Execute()
                .Select(supportedTimezones =>
                    supportedTimezones.FirstOrDefault(tz => platformInfo.TimezoneIdentifier == tz)
                )
                .Select(timezone =>
                    userAccessManager.ThirdPartySignUp(provider, loginInfo, true, (int)country.Id, timezone)
                )
                .Merge()
                .Track(analyticsService.SignUp, authenticationMethod)
                .ObserveOn(schedulerProvider.MainScheduler)
                .Subscribe(_ => onAuthenticated(null), onSignUpError)
                .DisposedBy(disposeBag);
        }

        private async void onSignUpError(Exception exception)
        {
            isLoadingSubject.OnNext(false);

            analyticsService.UnknownSignUpFailure.Track(exception.GetType().FullName, exception.Message);
            analyticsService.TrackAnonymized(exception);
            await View.Alert(Resources.Oops, Resources.GenericSignUpError, Resources.Ok);
        }

        private async Task<ICountry?> confirmCountryAndTermsOfService()
            => await Navigate<TermsAndCountryViewModel, ICountry?>();

        private void onOnboardingScroll(OnboardingScrollParameters parameters)
        {
            onboardingPagesViewed[parameters.PageNumber] = true;
            analyticsService.OnboardingPageScroll.Track(parameters.Action, parameters.Direction, parameters.PageNumber);
        }

        private void trackViewedPages()
        {
            analyticsService.OnboardingPagesViewed.Track(onboardingPagesViewed[0], onboardingPagesViewed[1], onboardingPagesViewed[2]);
        }
    }
}
