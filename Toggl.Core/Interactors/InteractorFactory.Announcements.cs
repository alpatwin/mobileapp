using System.Threading.Tasks;

namespace Toggl.Core.Interactors
{
    public partial class InteractorFactory
    {
        public IInteractor<Announcement?> FetchAnnouncement()
            => new FetchAnnouncementInteractor(timeService, onboardingStorage);
    }
}
