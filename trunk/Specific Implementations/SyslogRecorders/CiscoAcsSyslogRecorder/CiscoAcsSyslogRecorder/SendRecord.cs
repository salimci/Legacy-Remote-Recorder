using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace CiscoACSRecorder
{
    class SendRecord : CustomBase
    {
        public void sendDataforAgent(Rec rec) 
        {
            
            CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            s.SetData(rec);
        }

        public void sendDataforRemoteRecorder(string Dal, string virtualhost, Rec rec)
        {
            try
            {
                CustomBase cb = new CustomBase();
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder"); 
                s.SetData(Dal, virtualhost, rec);  
            }
            catch(Exception e)
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
            }
            InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, "sendDataforRemoteRecorder is finished");
        }
    }
}

