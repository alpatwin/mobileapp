namespace Toggl.Core
{
    public struct Announcement
    {
        public string Id { get; }
        public string Title { get; }
        public string Message { get; }
        public string Url { get; }
        public string CallToAction { get; set; }

        public Announcement(string id, string title, string message, string url, string callToAction)
        {
            Id = id;
            Url = url;
            Title = title;
            Message = message;
            CallToAction = callToAction;
        }
    }
}
