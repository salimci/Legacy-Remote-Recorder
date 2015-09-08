using System;

namespace Natek.Helpers.Providers.Ticket
{
    public static class TimestampProvider
    {
        private static readonly object Sync = new object();
        private static long _timestamp;

        public static string Next
        {
            get
            {
                lock (Sync)
                {
                    return DateTime.Now.Ticks + "_" + (++_timestamp) + "_" + new Random((int)DateTime.Now.Ticks).Next();
                }
            }
        }
    }
}
