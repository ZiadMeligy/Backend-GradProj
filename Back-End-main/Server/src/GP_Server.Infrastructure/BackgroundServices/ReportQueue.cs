using System.Collections.Concurrent;

namespace GP_Server.Infrastructure.BackgroundServices
{
    public static class ReportQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new();
    }
}
