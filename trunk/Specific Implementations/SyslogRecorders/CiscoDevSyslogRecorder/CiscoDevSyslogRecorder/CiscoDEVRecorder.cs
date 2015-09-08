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

namespace CiscoDevSyslogRecorder
{
    public class CiscoDevSyslogRecorder : CustomBase
    {   
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port=516,zone=0;
        private string err_log, protocol = "UDP", location = "", remote_host="127.0.0.1";
        private CLogger L;
        public Syslog slog=null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        private void InitializeComponent()
        {
 
        }
        
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile,String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            //last_position = LastPosition;
            //fromend = FromEndOnLoss;
            //max_record_send = MaxLineToWait;
            //timer_interval = SleepTime;
            //user = User;
            //password = Password;
            remote_host = RemoteHost;
            if (remote_host == null || remote_host =="")
                remote_host = "127.0.0.1";
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
        }
        
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
        
        public override void Init()
        {
            try
            {
                RegistryProcess rp = new RegistryProcess();
                InitializeLogger logger = new InitializeLogger(rp.TRC_LEVEL);
                EventLog.WriteEntry("Security Manager Syslog Recorder", "Recorder init start", EventLogEntryType.Information);
                if (usingRegistry) // agent için geldi
                {
                    if (!reg_flag)
                    {
                        if (!rp.get_regisryforAgent())
                        {
                            return;
                        }
                        else
                            if (!logger.Initialize(rp))
                            {
                                return;
                            }
                        reg_flag = true;
                    }
                }
                else  // remote recorder için geldi
                {
                    if (!reg_flag)
                    {
                        if (!rp.get_registryforRemoteRecorder(Id, location, trc_level))
                        {
                            return;
                        }
                        else
                            if (!logger.Initialize(rp))
                            {
                                return;
                            }
                        reg_flag = true;
                    }
                }                
                
                if (usingRegistry)
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + rp.SYSLOG_PORT.ToString());

                    try
                    {
                        slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), rp.SYSLOG_PORT, rp.PRO);
                    }
                    catch (Exception er)
                    {
                        InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error accuered while creating a syslog object for agent :" + er.Message);
                    }
                }
                else
                {
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + rp.SYSLOG_PORT.ToString());
                    
                    try
                    {
                        slog = new Syslog(remote_host, rp.SYSLOG_PORT, rp.PRO);
                    }
                    catch (Exception er)
                    {
                        InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, "An error accuered while creating a syslog object for remote recorder : " + er.Message);
                    }
                }
                
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);
                InitializeLogger.L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
                EventLog.WriteEntry("Security Manager Syslog Recorder", "Recorder init start", EventLogEntryType.Information);
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        
        void slog_SyslogEvent(LogMgrEventArgs args)
        {   
            try
            {
                if (args.Message !=null && args.Message !="")
                {
                    CiscoDEVRecorderProcess devrecorder = new CiscoDEVRecorderProcess();

                    devrecorder.parsingProcess(args, zone);
                    CustomBase.Rec rec = new CustomBase.Rec();
                    rec = devrecorder.createRec();

                    InitializeLogger.L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");

                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        s.SetReg(Id, rec.Datetime, "","","",rec.Datetime);
                    }
                    InitializeLogger.L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data"); 
                }
            }
            catch (Exception er)
            {
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                InitializeLogger.L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }
    }   
}
