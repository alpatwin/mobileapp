using System;

namespace Toggl.Storage.Settings
{
    public interface IOnboardingStorage
    {
        IObservable<bool> IsNewUser { get; }
        IObservable<bool> UserSignedUpUsingTheApp { get; }
        IObservable<bool> StartButtonWasTappedBefore { get; }
        IObservable<bool> HasTappedTimeEntry { get; }
        IObservable<bool> HasEditedTimeEntry { get; }
        IObservable<bool> StopButtonWasTappedBefore { get; }
        IObservable<bool> HasSelectedProject { get; }
        IObservable<bool> ProjectOrTagWasAddedBefore { get; }
        IObservable<bool> NavigatedAwayFromMainViewAfterTappingStopButton { get; }
        IObservable<bool> HasTimeEntryBeenContinued { get; }

        void SetCompletedOnboarding();
        void SetIsNewUser(bool isNewUser);
        void SetLastOpened(DateTimeOffset dateString);
        void SetFirstOpened(DateTimeOffset dateTime);
        void SetUserSignedUp();
        void SetNavigatedAwayFromMainViewAfterStopButton();
        void SetTimeEntryContinued();

        DateTimeOffset? GetLastOpened();
        DateTimeOffset? GetFirstOpened();
        bool CompletedOnboarding();

        void StartButtonWasTapped();
        void TimeEntryWasTapped();
        void ProjectOrTagWasAdded();
        void StopButtonWasTapped();

        void EditedTimeEntry();
        void SelectsProject();

        void SetDidShowRatingView();
        int NumberOfTimesRatingViewWasShown();
        void SetRatingViewOutcome(RatingViewOutcome outcome, DateTimeOffset dateTime);
        RatingViewOutcome? RatingViewOutcome();
        DateTimeOffset? RatingViewOutcomeTime();

        bool DidShowSiriClipboardInstruction();
        void SetDidShowSiriClipboardInstruction(bool value);

        bool CalendarPermissionWasAskedBefore();
        void SetCalendarPermissionWasAskedBefore();
        bool IsFirstTimeConnectingCalendars();
        void SetIsFirstTimeConnectingCalendars();

        IObservable<OnboardingConditionKey> OnboardingConditionMet { get; }
        void SetOnboardingConditionWasMet(OnboardingConditionKey onboardingConditionKey);
        bool OnboardingConditionWasMetBefore(OnboardingConditionKey onboardingConditionKey);
        bool OnboardingTimeEntryWasCreated();
        void SetOnboardingTimeEntryWasCreated();
        bool IsRunningTheAppFirstTime();

        void Reset();

        bool CheckIfAnnouncementWasShown(string announcementId);
        void MarkAnnouncementAsShown(string id);
    }
}
