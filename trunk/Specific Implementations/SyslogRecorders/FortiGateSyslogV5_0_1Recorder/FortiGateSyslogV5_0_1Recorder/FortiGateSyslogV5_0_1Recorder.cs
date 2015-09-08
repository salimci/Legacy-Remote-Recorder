using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace FortiGateSyslogV5_0_1Recorder
{
    public class FortiGateSyslogV5_0_1Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514, zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\FortiGateSyslogV5_0_1Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public FortiGateSyslogV5_0_1Recorder()
        {
            //TODO: Add any initialization after the InitComponent call          
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "SyslogLogEvent Starting.");
            L.Log(LogType.FILE, LogLevel.INFORM, "Log Line: " + args.Message);
            CustomBase.Rec rec = new CustomBase.Rec();

            string type = "";
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "FortiGateSyslogV5_0_1Recorder";
                    L.Log(LogType.FILE, LogLevel.DEBUG, "LogName: " + rec.LogName);
                    rec.Datetime = DateTime.Now.ToString(dateFormat);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + args.Source.ToString(CultureInfo.InvariantCulture));

                    if (args.Message.Length > 899)
                    {
                        rec.Description = args.Message.Substring(0, 899);
                    }

                    else
                    {
                        rec.Description = args.Message;
                    }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Description: " + rec.Description);


                    string[] lineArr = args.Message.Split(' ');
                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr[i].Contains("="))
                        {
                            if (lineArr[i].Split('=')[0] == "type")
                            {
                                type = lineArr[i].Split('=')[1];
                            }
                        }
                    }

                    rec.EventType = type;
                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (type == "traffic")
                        {
                            if (lineArr[i].Contains("="))
                            {
                                if (lineArr[i].StartsWith("level="))
                                {
                                    rec.SourceName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("subtype="))
                                {
                                    rec.EventCategory = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("user="))
                                {
                                    rec.UserName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("group="))
                                {
                                    rec.UserName += "/ " + lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("devname="))
                                {
                                    rec.ComputerName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("status="))
                                {
                                    rec.CustomStr1 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("service="))
                                {
                                    rec.CustomStr2 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("srcip="))
                                {
                                    rec.CustomStr3 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("dstip="))
                                {
                                    rec.CustomStr4 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("app="))
                                {
                                    rec.CustomStr5 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("utmaction="))
                                {
                                    rec.CustomStr6 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("utmevent="))
                                {
                                    rec.CustomStr7 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("hostname="))
                                {
                                    rec.CustomStr8 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("catdesc="))
                                {
                                    rec.CustomStr9 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("vpn="))
                                {
                                    rec.CustomStr10 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("srcport="))
                                {
                                    try
                                    {
                                        rec.CustomInt3 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("dstport="))
                                {
                                    try
                                    {
                                        rec.CustomInt4 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("sendbyte="))
                                {
                                    try
                                    {
                                        rec.CustomInt5 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("rcvdbyte="))
                                {
                                    try
                                    {
                                        rec.CustomInt6 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("policyid="))
                                {
                                    try
                                    {
                                        rec.CustomInt7 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("duration="))
                                {
                                    try
                                    {
                                        rec.CustomInt10 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt10 Error:" + exception.Message);
                                    }
                                }
                            }
                        }
                        else if (type == "event")
                        {
                            if (lineArr[i].StartsWith("level="))
                            {
                                rec.SourceName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("subtype="))
                            {
                                rec.EventCategory = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("user="))
                            {
                                rec.UserName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("group="))
                            {
                                rec.UserName += "/ " + lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("devname="))
                            {
                                rec.ComputerName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }
                            else if (lineArr[i].StartsWith("status="))
                            {
                                rec.CustomStr1 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("action="))
                            {
                                rec.CustomStr2 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("src="))
                            {
                                rec.CustomStr3 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("dst="))
                            {
                                rec.CustomStr4 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("init="))
                            {
                                rec.CustomStr5 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("mode="))
                            {
                                rec.CustomStr6 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }
                            else if (lineArr[i].StartsWith("remip="))
                            {
                                rec.CustomStr7 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }
                            else if (lineArr[i].StartsWith("locip="))
                            {
                                rec.CustomStr8 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }
                            else if (lineArr[i].StartsWith("vpntunnel="))
                            {
                                rec.CustomStr9 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                            }

                            else if (lineArr[i].StartsWith("remport="))
                            {
                                try
                                {
                                    rec.CustomInt3 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("locport="))
                            {
                                try
                                {
                                    rec.CustomInt4 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 Error:" + exception.Message);
                                }
                            }
                            else if (lineArr[i].StartsWith("policyid="))
                            {
                                try
                                {
                                    rec.CustomInt7 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7 Error:" + exception.Message);
                                }
                            }
                        }

                        else if (type == "utm")
                        {

                            if (lineArr[i].Contains("="))
                            {
                                if (lineArr[i].StartsWith("level="))
                                {
                                    rec.SourceName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("subtype="))
                                {
                                    rec.EventCategory = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("user="))
                                {
                                    rec.UserName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("group="))
                                {
                                    rec.UserName += "/ " + lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("devname="))
                                {
                                    rec.ComputerName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("eventtype="))
                                {
                                    rec.CustomStr1 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("service="))
                                {
                                    rec.CustomStr2 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("srcip="))
                                {
                                    rec.CustomStr3 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("dstip="))
                                {
                                    rec.CustomStr4 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("sensor="))
                                {
                                    rec.CustomStr5 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("severity="))
                                {
                                    rec.CustomStr6 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                //else if (lineArr[i].StartsWith("profiletype="))
                                //{
                                //    rec.CustomStr7 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                //}
                                else if (lineArr[i].StartsWith("attackname="))
                                {
                                    rec.CustomStr8 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("ref="))
                                {
                                    rec.CustomStr9 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                else if (lineArr[i].StartsWith("vpn="))
                                {
                                    rec.CustomStr10 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }

                                else if (lineArr[i].StartsWith("srcport="))
                                {
                                    try
                                    {
                                        rec.CustomInt3 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("dstport="))
                                {
                                    try
                                    {
                                        rec.CustomInt4 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 Error:" + exception.Message);
                                    }
                                }

                                else if (lineArr[i].StartsWith("count="))
                                {
                                    try
                                    {
                                        rec.CustomInt10 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt10 Error:" + exception.Message);
                                    }
                                }
                            }
                        }
                    }

                    if (rec.EventType == "event" && rec.EventCategory == "router")
                    {
                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].StartsWith("devname="))
                            {
                                try
                                {
                                    rec.ComputerName = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("dir="))
                            {
                                try
                                {
                                    rec.CustomStr1 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("hostname="))
                            {
                                try
                                {
                                    rec.CustomStr3 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("mac="))
                            {
                                try
                                {
                                    rec.CustomStr4 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("ip="))
                            {
                                try
                                {
                                    rec.CustomStr5 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("dhcp_msg="))
                            {
                                try
                                {
                                    rec.CustomStr6 = lineArr[i].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("msg="))
                            {
                                try
                                {
                                    rec.CustomStr7 = After(args.Message, "msg=").Replace('"', ' ').Trim();
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 Error:" + exception.Message);
                                }
                            }

                            else if (lineArr[i].StartsWith("lease="))
                            {
                                try
                                {
                                    rec.CustomInt10 = Convert.ToInt64(lineArr[i].Split('=')[1].Replace('"', ' ').Trim());
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt10 Error:" + exception.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.INFORM, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}


