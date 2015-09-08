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
using System.Text.RegularExpressions;
using System.Globalization;
using Parser;


namespace SonicwallSSLVPNSyslogRecorder
{

    public class SonicwallSSLVPNRecorder: CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        public String[] CharSplit(String line, bool useTabs, Char ignoreChar) // Splits(:)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreChar)
                    ignore = !ignore;
                if (c != ':')
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        public String[] SpaceSplit(String line, bool useTabs, Char ignoreChar) // Splits(:)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreChar)
                    ignore = !ignore;
                if (c != ' ')
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        private void InitializeComponent()
        {
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
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
            //remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            remote_host = RemoteHost;
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
                if (usingRegistry)
                {
                    if (!reg_flag)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SonicwallSSLVPNRecorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!Get_logDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log dir");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SonicwallSSLVPNRecorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        if (location.Contains(':'.ToString()))
                        {
                            protocol = location.Split(':')[0];
                            Syslog_Port = Convert.ToInt32(location.Split(':')[1]);
                            if (protocol.ToLower() == "tcp")
                                pro = ProtocolType.Tcp;
                            else
                                pro = ProtocolType.Udp;
                        }
                        else
                        {
                            protocol = location;
                            Syslog_Port = 514;
                        }
                    }
                    else
                    {
                        pro = ProtocolType.Udp;
                        Syslog_Port = 514;
                    }
                }


                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SonicWallSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);

                if (usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SonicWallSSLVPN Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicwallSSLVPNRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SonicwallSSLVPNRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicwallSSLVPNRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SonicwallSSLVPNRecorder()
        {
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
/*
           
local6.notice SSLVPN: id=sslvpn sn=0006B119CE3C time="2010-10-18 01:40:26" vp_time="2010-10-17 22:40:26 UTC" fw=10.6.4.117 pri=4 m=1 src=78.165.130.251 dst=10.6.4.117 user="admin" usr="admin" msg="User login failed" agent="Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; .NET CLR 3.0.04506)"
            
local6.notice SSLVPN: id=sslvpn sn=0006B119CE3C time="2010-10-18 01:40:36" vp_time="2010-10-17 22:40:36 UTC" fw=10.6.4.117 pri=5 m=1 src=78.165.130.251 dst=10.6.4.117 user="admin" usr="admin" msg="User login successful" agent="Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; .NET CLR 3.0.04506)"

local6.notice SSLVPN: id=sslvpn sn=0006B119CE3C time="2010-10-18 01:39:45" vp_time="2010-10-17 22:39:45 UTC" fw=10.6.4.117 pri=5 m=2 src=10.6.4.71 dst=10.6.4.117 user="admin" usr="admin" msg="NetExtender disconnected" duration=167 agent=""

*/
            CustomBase.Rec rec = new CustomBase.Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    //10.6.4.117:514 : local6.notice SSLVPN: id=sslvpn sn=0006B119CE3C time="2010-10-30 10:54:59" vp_time="2010-10-30 07:54:59 UTC" fw=10.6.4.117 pri=5 m=2 src=10.6.4.72 dst=10.6.4.117 user="admin2" usr="admin2" msg="NetExtender disconnected" duration=3430 agent=""
                    rec.LogName = "SonicwallSSLVPNRecorder";
                    String[] desc = SpaceSplit(args.Message, false, '"');
                    rec.Description = args.Message;
                    if (desc.Length >= 12)
                    {
                        rec.SourceName = args.Source;
                        //Datetime
                        String dateTime = desc[7].Split('=')[1].TrimStart('"').TrimEnd('"');
                        dateTime = dateTime.Remove(dateTime.Length - 3).Trim();
                        try
                        {
                            rec.Datetime = Convert.ToDateTime(dateTime, CultureInfo.InvariantCulture).ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        catch
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "DateTime conversion error, please check SonicWall log format...");
                            L.Log(LogType.FILE, LogLevel.ERROR, args.Message);
                        }
                      
                        try
                        {//id
                            rec.CustomStr7 = desc[4].Split('=')[1].Trim();
                        }
                        catch { }
                        try
                        {//sn
                            rec.CustomStr9 = desc[5].Split('=')[1].Trim();
                        }
                        catch { }
                        try
                        {//fw
                            rec.CustomStr8 = desc[10].Split('=')[1].Trim();
                        }
                        catch { }
                        try
                        {//src
                            rec.CustomStr3 = desc[11].Split('=')[1].Trim();
                        }
                        catch { }
                        try
                        {//dst
                            rec.CustomStr4 = desc[12].Split('=')[1].Trim();
                        }
                        catch { }
                        try
                        {//user
                            rec.CustomStr1 = desc[13].Split('=')[1].TrimStart('"').TrimEnd('"');
                        }
                        catch { }
                        try
                        {//usr
                            rec.CustomStr2 = desc[14].Split('=')[1].TrimStart('"').TrimEnd('"');
                        }
                        catch { }
                        try
                        {//msg
                            rec.EventType = desc[15].Split('=')[1].TrimStart('"').TrimEnd('"');
                        }
                        catch { }
                        try
                        {//agent or duration
                            if (desc[16].Split('=')[0] == "duration")
                            {
                                rec.CustomInt1 = Convert.ToInt32(desc[14].Split('=')[1].Trim());
                                rec.CustomStr5 = desc[17].Split('=')[1].TrimStart('"').TrimEnd('"');
                            }
                            else if (desc[16].Split('=')[0] == "agent")
                            {
                                rec.CustomInt1 = 0;
                                rec.CustomStr5 = desc[16].Split('=')[1].TrimStart('"').TrimEnd('"');
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Log is sent to DESC column");
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.DEBUG, Dal + " " + virtualhost + " " + rec.Description);
                    s.SetData(Dal, virtualhost, rec);
                    //s.SetReg(Id, rec.Datetime, "","","",rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SonicwallSSLVPNSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicwallSSLVPNSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicwallSSLVPNSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicwallSSLVPNRecorderRead Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Initialize_Logger()
        {
            try
            {
                L = new CLogger();
                switch (trc_level)
                {
                    case 0:
                        {
                            L.SetLogLevel(LogLevel.NONE);
                        } break;
                    case 1:
                        {
                            L.SetLogLevel(LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            L.SetLogLevel(LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            L.SetLogLevel(LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            L.SetLogLevel(LogLevel.DEBUG);
                        } break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicwallSSLVPNSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }


}
