using System;
using System.Timers;
using Log;
using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public abstract class PeriodicRecorder : RecorderBase
    {
        protected Timer processDataTimer;

        public override void Start()
        {
            lock (SyncRoot)
            {
                if (processDataTimer != null)
                {
                    return;
                }
                processDataTimer = new Timer { AutoReset = false, Interval = 1 };
                processDataTimer.Elapsed += DoPeriodic;
                processDataTimer.Start();
            }
        }

        protected override NextInstruction ValidateGlobalParameters()
        {
            sleepTime = sleepTime <= 0 ? 60000 : sleepTime;
            return NextInstruction.Do;
        }

        protected virtual void DoPeriodic(object sender, ElapsedEventArgs e)
        {
            try
            {
                Exception error = null;
                PerformRecorderLogic(ref error);
            }
            finally
            {
                Log(LogLevel.DEBUG, "Restart Timer");
                processDataTimer.Interval = sleepTime;
                processDataTimer.Start();
            }
        }
    }
}
