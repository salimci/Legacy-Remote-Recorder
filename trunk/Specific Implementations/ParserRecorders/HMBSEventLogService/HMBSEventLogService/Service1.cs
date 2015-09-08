using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Parser;

namespace HMBSEventLogService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Thread threadEventLog;
        protected override void OnStart(string[] args)
        {
            threadEventLog = new Thread(new ThreadStart(EventLogService));
            threadEventLog.Start();
        }

        protected override void OnStop()
        {
            while (threadEventLog.IsAlive)
            {
                threadEventLog.Abort();
                Thread.Sleep(1000);
            }
            threadEventLog = null;
        }

        private void EventLogService()
        {
            HMBSEventLogRecorder eventLogRec = new HMBSEventLogRecorder();
            eventLogRec.Parse();
        }
    }
}
