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

namespace WatchGuardWebSyslogV_1_0_0Recorder
{
    public class WatchGuardWebSyslogV_1_0_0Recorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WatchGuardWebSyslogV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WatchGuardWebSyslogV_1_0_0Recorder functions may not be running");
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
                            pro = protocol.ToLower() == "tcp" ? ProtocolType.Tcp : ProtocolType.Udp;
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(SlogSyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WatchGuardWebSyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\WatchGuardWebSyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WatchGuardWebSyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        void SlogSyslogEvent(LogMgrEventArgs args)
        {
            var rec = new Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, " Log : " + args.Message);

                try
                {
                    rec.LogName = "WatchGuardWebSyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");


                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;
                    rec.Description = args.Message.Replace("'", "|");

                    string line = args.Message;
                    string[] lineArr = line.Split();
                    string[] subLineArr = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);


                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr[i].StartsWith("op"))
                        {
                            rec.EventType = SplitFunction(lineArr[i]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                        }

                        if (lineArr[i].StartsWith("proxy_act"))
                        {
                            rec.CustomStr2 = SplitFunction(lineArr[i]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                        }

                        if (lineArr[i].StartsWith("dstname"))
                        {
                            rec.CustomStr6 = SplitFunction(lineArr[i]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                        }

                        if (lineArr[i].StartsWith("arg"))
                        {
                            rec.CustomStr7 = SplitFunction(lineArr[i]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                        }

                        try
                        {
                            if (lineArr[i].StartsWith("sent_bytes"))
                            {
                                rec.CustomInt5 = Convert.ToInt32(SplitFunction(lineArr[i]));
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 Type Casting Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr[i].StartsWith("rcvd_bytes"))
                            {
                                rec.CustomInt6 = Convert.ToInt32(SplitFunction(lineArr[i]));
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6 Type Casting Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr[i].StartsWith("elapsed_time"))
                            {
                                rec.CustomStr8 = SplitFunction(lineArr[i]);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 Type Casting Error: " + exception.Message);
                        }

                        if (lineArr[i].ToLower() == "tcp")
                        {
                            try
                            {
                                IPAddress sourceIp = IPAddress.Parse(lineArr[i + 1]);
                                rec.CustomStr3 = sourceIp.ToString(); L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                            }

                            try
                            {
                                IPAddress destIp = IPAddress.Parse(lineArr[i + 2]);
                                rec.CustomStr4 = destIp.ToString();
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 Error: " + exception.Message);
                            }
                        }

                        if (lineArr[i].ToLower() == "udp")
                        {
                            try
                            {
                                IPAddress sourceIp = IPAddress.Parse(lineArr[i + 3]);
                                rec.CustomStr3 = sourceIp.ToString(); L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                            }

                            try
                            {
                                IPAddress destIp = IPAddress.Parse(lineArr[i + 4]);
                                rec.CustomStr4 = destIp.ToString();
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 Error: " + exception.Message);
                            }

                        }
                    }
                    rec.EventCategory = subLineArr[10];

                    try
                    {
                        string msg1 = After(line, "msg=");
                        string msg2 = Before(msg1, "\" ");
                        rec.CustomStr1 = msg2.Replace('"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 Error: " + exception.Message);
                    }

                    if (lineArr.Length > 8)
                    {
                        if (lineArr[8].Contains("-"))
                        {
                            rec.CustomStr5 = lineArr[5].Split('-')[0];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                        }
                    }

                    try
                    {
                        if (subLineArr.Length > 16)
                        {
                            rec.CustomInt3 = Convert.ToInt32(lineArr[16]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 Type Casting Error: " + exception.Message);
                    }

                    try
                    {
                        if (subLineArr.Length > 17)
                        {
                            rec.CustomInt4 = Convert.ToInt32(lineArr[17]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 Type Casting Error: " + exception.Message);
                    }

                    //try
                    //{
                    //    if (lineArr.Length > 8)
                    //    {
                    //        rec.EventCategory = lineArr[10];
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                    //}
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
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
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

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

        public string SplitFunction(string value)
        {
            if (value.Contains("="))
            {
                return value.Split('=')[1].Replace('"', ' ').Trim();
            }
            else
            {
                return "";
            }
        } // SplitFunction

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
                EventLog.WriteEntry("Security Manager WatchGuardWebSyslogV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
