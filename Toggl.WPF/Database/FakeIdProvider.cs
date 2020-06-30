using System.Threading;
using Toggl.Storage;

namespace Toggl.WPF.Database
{
    public class FakeIdProvider : IIdProvider
    {
        private long currentId = 0;
        public long GetNextIdentifier()
        {
            return Interlocked.Increment(ref currentId);
        }
    }
}
