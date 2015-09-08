using System;

namespace Natek.Recorders.Remote
{
    public class ExpectReachedException : Exception
    {
        public ExpectReachedException()
        {
        }

        public ExpectReachedException(string message)
            : base(message)
        {
        }
    }
}
