using System;
using Toggl.Shared;
using Toggl.Storage.Settings;

namespace Toggl.Core.Interactors
{
    internal class FetchAnnouncementInteractor : IInteractor<Announcement?>
    {
        private const string preLaunchId = "announcement_rebranding_pre_launch";
        private static readonly DateRange preLaunchAnnouncementDates = new DateRange(
            new DateTime(2020, 8, 31),
            new DateTime(2020, 9, 6)
        );

        private const string rebrandingId = "announcement_rebranding_live";
        private static readonly DateRange rebrandingAnnouncementDates = new DateRange(
            new DateTime(2020, 9, 7),
            new DateTime(2020, 9, 14)
        );

        private readonly ITimeService timeService;
        private readonly IOnboardingStorage onboardingStorage;

        public FetchAnnouncementInteractor(ITimeService timeService, IOnboardingStorage onboardingStorage)
        {
            this.timeService = timeService;
            this.onboardingStorage = onboardingStorage;
        }

        public Announcement? Execute()
        {
            var today = timeService.CurrentDateTime.DateTime;

            return preLaunchAnnouncement(today) ?? rebrandingAnnouncement(today);
        }

        private Announcement? preLaunchAnnouncement(DateTime today)
        {
            if (!preLaunchAnnouncementDates.Contains(today))
                return null;

            if (onboardingStorage.CheckIfAnnouncementWasShown(preLaunchId))
                return null;

            return new Announcement(
                preLaunchId,
                Resources.PreLaunchAnnouncementTitle,
                Resources.PreLaunchAnnouncementMessage,
                Resources.LearnMore,
                ""
            );
        }

        private Announcement? rebrandingAnnouncement(DateTime today)
        {
            if (!rebrandingAnnouncementDates.Contains(today))
                return null;

            if (onboardingStorage.CheckIfAnnouncementWasShown(rebrandingId))
                return null;

            return new Announcement(
                rebrandingId,
                Resources.RebrandingAnnouncementTitle,
                Resources.RebrandingAnnouncementMessage,
                Resources.CheckUsOut,
                ""
            );
        }
    }
}
