
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SquidSyslogV_1_0_0Recorder
{
    public class SquidSyslogV_1_0_0Recorder : CustomBase
    {
        private const uint logging_interval = 60000;
        private const uint log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog;
        private bool reg_flag;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id;
        protected String virtualhost, Dal;
        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";

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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SquidSyslogV_1_0_0Recorder Recorder functions may not be running");
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SquidSyslogV_1_0_0Recorder Recorder functions may not be running");
                            return;
                        }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        if (location.Contains(':'.ToString(CultureInfo.InvariantCulture)))
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
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0] + " port: " + Syslog_Port.ToString(CultureInfo.InvariantCulture));
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + Syslog_Port.ToString(CultureInfo.InvariantCulture));
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                slog.Start();
                slog.SyslogEvent += SlogSquidSyslogRecorder;

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SquidSyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Remote Recorder");
                    if (registryKey != null)
                        err_log = registryKey.GetValue("Home Directory") + @"log\SquidSyslogV_1_0_0Recorder" + Id + ".log";
                    rk.Close();
                }
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

        public SquidSyslogV_1_0_0Recorder()
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

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
        {
            var lst = new List<String>();
            var sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line)
            {
                if (c != ' ' && (!useTabs || c != '\t'))
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
                    space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }// SpaceSplit

        public void SlogSquidSyslogRecorder(LogMgrEventArgs args)
        {
            var rec = new Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);
                string line = args.Message;
                try
                {
                    rec.LogName = "SquidSyslogV_1_0_0Recorder";
                    string[] arr = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length > 13)
                    {
                        rec.EventType = arr[13];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    }

                    if (arr.Length > 11)
                    {
                        rec.EventCategory = arr[11];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                    }

                    if (arr.Length > 14)
                    {
                        rec.CustomStr1 = arr[14];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    }

                    rec.CustomStr2 = arr[arr.Length - 1];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);


                    if (arr.Length > 10)
                    {
                        rec.CustomStr3 = arr[10];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    }

                    if (arr.Length > 16)
                    {
                        rec.CustomStr4 = arr[16].Contains("/") ? arr[16].Split('/')[1] : arr[16];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                    }

                    if (arr.Length > 2)
                    {
                        rec.CustomStr5 = arr[2];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                    }

                    if (arr.Length > 8)
                    {
                        rec.CustomStr6 = arr[8];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                    }


                    if (arr.Length > 7)
                    {
                        rec.CustomStr7 = arr[7];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                    }

                    if (arr.Length > 0)
                    {
                        rec.CustomStr10 = arr[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                    }

                    try
                    {
                        if (arr.Length > 9)
                            rec.CustomInt2 = Convert.ToInt32(arr[9]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "rec.CustomInt2." + rec.CustomInt2);

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 Cast Error." + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 expected value: " + arr[8]);
                    }

                    try
                    {
                        rec.CustomInt3 = Convert.ToInt32(arr[12]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "rec.CustomInt3." + rec.CustomInt3);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 Cast Error." + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 expected value: " + arr[11]);
                    }

                    try
                    {
                        string myDateTimeString = arr[4] + arr[3] + "," + DateTime.Now.Year + "," + arr[5];
                        DateTime dt = Convert.ToDateTime(myDateTimeString);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Datetime Error " + exception.Message);
                    }

                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;

                    //string findChar = "/";
                    //int Found = (arr[13].Length - arr[13].Replace(findChar, "").Length) / findChar.Length;
                    //string s = Between(arr[13], "/", "/");
                    //if (Found > 3)
                    //{
                    //    string s1 = Between(s, "/", "/");
                    //    if (Found == 3 || Found == 4)
                    //    {
                    //        rec.CustomStr8 = s1;
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr8: " + rec.CustomStr8);
                    //    }

                    //    else if (Found > 3)
                    //    {
                    //        rec.CustomStr8 = Before(s1, "/");
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr8: " + rec.CustomStr8);
                    //    }
                    //}
                    //else if (Found > 1)
                    //{
                    //    if (s.StartsWith("/"))
                    //    {
                    //        rec.CustomStr8 = After(s, "/");
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr8: " + rec.CustomStr8);
                    //    }
                    //}//
                    //else if (Found == 0)
                    //{
                    //    if (rec.CustomStr1.Contains(":"))
                    //    {
                    //        rec.CustomStr8 = rec.CustomStr1.Split(':')[0];
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr8: " + rec.CustomStr8);
                    //    }
                    //}



                    //try
                    //{
                    //    if (rec.CustomStr1.Contains(":"))
                    //    {
                    //        rec.CustomInt5 = Convert.ToInt32(rec.CustomStr1.Split(':')[1]);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 Cast Error." + exception.Message);
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 expected value: " + arr[13]);
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


                CustomServiceBase serviceBase = GetInstanceService("Security Manager Remote Recorder");
                serviceBase.SetData(Dal, virtualhost, rec);
                serviceBase.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        } // SlogSquidSyslogRecorder

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a, StringComparison.Ordinal);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a, StringComparison.Ordinal);
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
        } // Read_Registry

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
        } // Initialize_Logger
    }
}
