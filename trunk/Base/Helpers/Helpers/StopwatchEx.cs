using System;
using System.Diagnostics;

namespace Natek.Helpers.Diagnostics
{
    public class StopwatchEx : Stopwatch, IDisposable
    {
        public void Dispose()
        {
            Stop();
        }
    }
}
