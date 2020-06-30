using System;
using System.Collections.Generic;
using Toggl.Core.Analytics;

namespace Toggl.WPF.Analytics
{
    public class FakeAnalyticsService : BaseAnalyticsService
    {
        public override void Track(Exception exception, string message)
        {
            // throw new NotImplementedException();
        }

        public override void Track(Exception exception, IDictionary<string, string> properties)
        {
            // throw new NotImplementedException();
        }

        public override void ResetUserId()
        {
            // throw new NotImplementedException();
        }

        public override void Track(string eventName, Dictionary<string, string> parameters = null)
        {
            // throw new NotImplementedException();
        }

        protected override void TrackException(Exception exception)
        {
            // throw new NotImplementedException();
        }

        public override void SetUserId(long id)
        {
            // throw new NotImplementedException();
        }
    }
}
