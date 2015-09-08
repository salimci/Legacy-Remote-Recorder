// Onur Sarıkaya

using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Parser;

namespace SquidSyslogRecorder
{
    public class SquidSyslogRecorder : CustomBase
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(SlogSquidSyslogRecorder);

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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SquidSyslogRecorder" + Id + ".log";
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

        public SquidSyslogRecorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        public void SlogSquidSyslogRecorder(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);

                try
                {
                    rec.LogName = "SquidSyslogRecorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();
                    string[] arr = args.Message.Split(' ');
                    //for (int i = 0; i < arr.Length; i++)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, " Line Spliteed. Onur - " + arr[i]);
                    //}
                    rec.CustomStr1 = arr[15];

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].StartsWith("http:") || arr[i].StartsWith("https:"))
                        {
                            rec.CustomStr4 = arr[i];
                            rec.CustomStr2 = arr[i + 1];
//                            rec.CustomStr5 = arr[i - 1];
                            rec.EventCategory = arr[i - 1];
                        }

                        else if (arr[i].StartsWith("CONNECT"))
                        {
                            rec.CustomStr4 = arr[i + 1];
                            rec.CustomStr2 = arr[i + 2];
                        }

                        else if (arr[i].StartsWith("TCP"))
                        {
                            rec.CustomStr1 = arr[i - 1];
                        }
                        else if (arr[i].StartsWith("squid"))
                        {
                            rec.CustomStr3 = arr[i];
                            if (rec.CustomStr3.EndsWith(":"))
                            {
                                rec.CustomStr3 = rec.CustomStr3.Replace(':', ' ');
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(rec.CustomStr4))
                    {
                        if (rec.CustomStr4.Contains("://"))
                        {
                            string b = After(rec.CustomStr4, "://");
                            string c = b.Split('/')[0];
                            string d = "";// b.Split('/')[1];
                            for (int i = 1; i < b.Split('/').Length; i++)
                            {
                                if (i == 1)
                                {
                                    d += b.Split('/')[i];
                                }
                                else
                                {
                                    d += "/" + b.Split('/')[i];
                                }
                            }
                            rec.CustomStr4 = c;
                            rec.CustomStr5 = d;
                        }
                    }

                    if (args.Message.Length > 895)
                        rec.Description = args.Message.Substring(0, 890);
                    else
                        rec.Description = args.Message;

                    rec.Description = args.Message.Replace("'", "|");

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
                    rec.SourceName = args.Source;
                    L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
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
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoBBSwitchSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoBBSwitchSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoBBSwitchSyslogRecorder").GetValue("Trace Level"));
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
