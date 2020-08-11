namespace Toggl.Core
{
    public struct Announcement
    {
        public string Id { get; }
        public string Title { get; }
        public string Message { get; }
        public string Url { get; }
        public string CallToAction { get; }

        public Announcement(string id, string title, string message, string callToAction, string url)
        {
            Id = id;
            Url = url;
            Title = title;
            Message = message;
            CallToAction = callToAction;
        }
    }
}
