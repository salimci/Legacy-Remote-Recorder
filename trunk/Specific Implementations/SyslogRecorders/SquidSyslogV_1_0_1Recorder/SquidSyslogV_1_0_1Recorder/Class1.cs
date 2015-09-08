
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

namespace SquidSyslogV_1_0_1Recorder
{
    public class SquidSyslogV_1_0_1Recorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SquidSyslogV_1_0_1Recorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SquidSyslogV_1_0_1Recorder Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager SquidSyslogV_1_0_1Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SquidSyslogV_1_0_1Recorder" + Id + ".log";
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

        public SquidSyslogV_1_0_1Recorder()
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
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
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
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
            L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);
            try
            {
                CustomBase.Rec rec = new CustomBase.Rec();
                try
                {
                    rec.LogName = "SquidSyslogV_1_0_1Recorder";
                    string[] lineArr = SpaceSplit(args.Message, false);

                    try
                    {
                        DateTime dt;
                        string myDateTimeString = lineArr[4] + lineArr[3] + "," + DateTime.Now.Year + "," + lineArr[5];
                        dt = Convert.ToDateTime(myDateTimeString);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Datetime Error. " + exception.ToString());
                    }

                    #region squid
                    if (lineArr.Length > 6 && lineArr[6].StartsWith("squid"))
                    {
                        try
                        {
                            if (lineArr.Length > 10)
                            {
                                if (lineArr[10].Contains("/"))
                                {
                                    rec.EventCategory = lineArr[10].Split('/')[0];
                                    rec.CustomInt1 = Convert.ToInt32(lineArr[10].Split('/')[1]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory or CustomInt1 Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                rec.EventType = lineArr[12];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 0)
                            {
                                rec.ComputerName = lineArr[0];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error. " + exception.ToString());
                        }

                        try
                        {
                            rec.CustomStr2 = lineArr[lineArr.Length - 1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 9)
                            {
                                rec.CustomStr3 = lineArr[9];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 15)
                            {
                                if (lineArr[15].Contains("/"))
                                {
                                    rec.CustomStr4 = lineArr[15].Split('/')[0];
                                    rec.CustomStr7 = lineArr[15].Split('/')[1];
                                }
                                else
                                {
                                    rec.CustomStr4 = lineArr[15];
                                }
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 or CustomStr7 Error. " + exception.ToString());
                        }

                        try
                        {
                            //dene
                            if (lineArr.Length > 13 && lineArr[13].StartsWith("http"))
                            {
                                if (lineArr[13].StartsWith("http"))
                                {
                                    string s1 = After(lineArr[13], "://");
                                    string s2 = Before(s1, "/");
                                    rec.CustomStr8 = s2;
                                    rec.CustomStr9 = After(lineArr[13], s2);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                                }
                                else
                                {
                                    if (lineArr[13].Contains(":"))
                                    {
                                        rec.CustomStr8 = lineArr[13].Split(':')[0];
                                        rec.CustomInt2 = Convert.ToInt32(lineArr[13].Split(':')[1]);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 or CustomStr9 Error. " + exception.ToString());
                        }
                    }
                    #endregion
                    #region pf
                    else if (lineArr.Length > 6 && lineArr[6].StartsWith("pf"))
                    {
                        try
                        {
                            if (lineArr.Length > 13)
                            {
                                rec.SourceName = lineArr[13];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "SourceName Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 25)
                            {
                                rec.EventCategory = lineArr[25];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 10)
                            {
                                rec.EventType = lineArr[10];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 0)
                            {
                                rec.ComputerName = lineArr[0];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + rec.ComputerName);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 29)
                            {
                                rec.CustomStr3 = Before(lineArr[29], ".", 1);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + rec.CustomStr3);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error. " + exception.ToString());
                        }

                        try
                        {
                            if (lineArr.Length > 31)
                            {
                                rec.CustomStr7 = Before(lineArr[31], ".", 1);
                                string int2 = After(lineArr[31], ".", 0).Replace(":", " ").Trim();
                                rec.CustomInt2 = Convert.ToInt32(int2);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 or CustomInt2 Error. " + exception.ToString());
                        }
                    }
                    #endregion

                    if (args.Message.Length > 899)
                    {
                        rec.Description = args.Message.Substring(0, 899);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                    }
                    else
                    {
                        rec.Description = args.Message;
                    }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Log : " + args.Message);
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetData(Dal, virtualhost, rec);
                s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);

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
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a, int type)
        {
            //type = 1 last
            //type = 0 first


            int posA = 0;

            if (type == 1)
            {
                posA = value.LastIndexOf(a, System.StringComparison.Ordinal);
            }

            if (type == 0)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
            }
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
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

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

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a, int type)
        {
            //type = 0 first
            //type = 1 last
            int posA = 0;

            if (type == 1)
            {
                posA = value.IndexOf(a);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a);
            }

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
