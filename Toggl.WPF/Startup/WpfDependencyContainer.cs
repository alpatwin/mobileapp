using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using NSubstitute;
using Toggl.Core;
using Toggl.Core.Analytics;
using Toggl.Core.Services;
using Toggl.Core.Shortcuts;
using Toggl.Core.UI;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Services;
using Toggl.Networking;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Queries;
using Toggl.Storage.Settings;
using Toggl.WPF.Analytics;
using Toggl.WPF.Database;
using Toggl.WPF.Services;

namespace Toggl.WPF.Startup
{
    public class WpfDependencyContainer : UIDependencyContainer
    {
        private const ApiEnvironment environment =
#if USE_PRODUCTION_API
            ApiEnvironment.Production;
#else
            ApiEnvironment.Staging;
#endif

        private readonly CompositePresenter presenter;
        private static readonly UserAgent userAgent = new UserAgent("wpf-nativeapp", "1.0.0.0");
        public WpfDependencyContainer(ApiEnvironment apiEnvironment, UserAgent userAgent) : base(apiEnvironment, userAgent)
        {
            presenter = new CompositePresenter();
        }

        public new static WpfDependencyContainer Instance { get; private set; }

        public static void EnsureInitialized()
        {
            Instance = new WpfDependencyContainer(ApiEnvironment.Staging, userAgent);
            UIDependencyContainer.Instance = Instance;
        }

        protected override ITogglDatabase CreateDatabase()
         => new FakeTogglDatabase();

        protected override IPlatformInfo CreatePlatformInfo()
            => Substitute.For<IPlatformInfo>();

        protected override IQueryFactory CreateQueryFactory()
            => Substitute.For<IQueryFactory>();

        protected override IRatingService CreateRatingService()
            => Substitute.For<IRatingService>();

        protected override ICalendarService CreateCalendarService()
            => Substitute.For<ICalendarService>();

        protected override IKeyValueStorage CreateKeyValueStorage()
            => Substitute.For<IKeyValueStorage>();

        protected override ILicenseProvider CreateLicenseProvider()
            => Substitute.For<ILicenseProvider>();

        protected override IUserPreferences CreateUserPreferences()
            => Substitute.For<IUserPreferences>();

        protected override IAnalyticsService CreateAnalyticsService()
         => new FakeAnalyticsService();

        protected override IOnboardingStorage CreateOnboardingStorage()
            => Substitute.For<IOnboardingStorage>();

        protected override ISchedulerProvider CreateSchedulerProvider()
         => new WpfSchedulerProvider();

        protected override INotificationService CreateNotificationService()
         => new FakeNotificationService();

        protected override IAccessibilityService CreateAccessibilityService()
            => Substitute.For<IAccessibilityService>();

        protected override ILastTimeUsageStorage CreateLastTimeUsageStorage()
            => Substitute.For<ILastTimeUsageStorage>();

        protected override IApplicationShortcutCreator CreateShortcutCreator()
            => Substitute.For<IApplicationShortcutCreator>();

        protected override IBackgroundSyncService CreateBackgroundSyncService()
            => Substitute.For<IBackgroundSyncService>();

        protected override IFetchRemoteConfigService CreateFetchRemoteConfigService()
            => Substitute.For<IFetchRemoteConfigService>();

        protected override IAccessRestrictionStorage CreateAccessRestrictionStorage()
            => Substitute.For<IAccessRestrictionStorage>();

        protected override IPrivateSharedStorageService CreatePrivateSharedStorageService()
            => Substitute.For<IPrivateSharedStorageService>();

        protected override IPushNotificationsTokenService CreatePushNotificationsTokenService()
            => Substitute.For<IPushNotificationsTokenService>();

        protected override HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var header = new ProductHeaderValue(userAgent.Name, userAgent.Version);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(header));
            return client;
        }

        protected override INavigationService CreateNavigationService()
         => new NavigationService(presenter, ViewModelLoader, AnalyticsService);

        protected override IPermissionsChecker CreatePermissionsChecker()
         => new FakePermissionsChecker();

        protected override IWidgetsService CreateWidgetsService()
            => Substitute.For<IWidgetsService>();
    }
}
