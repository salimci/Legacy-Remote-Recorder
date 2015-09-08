using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace NatekLogService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Thread thread;
        protected override void OnStart(string[] args)
        {
            thread = new Thread(new ThreadStart(MainService));
            thread.Start();
        }

        protected override void OnStop()
        {
            while (thread.IsAlive)
            {
                thread.Abort();
            }
            thread = null;
        }
        
        private void MainService()
        {
            FilterService filterService = new FilterService();
        }
    }
}
