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

namespace ArubaSyslogV_1_0_0Recorder
{
    public class ArubaSyslogV_1_0_0Recorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ArubaSyslogV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ArubaSyslogV_1_0_0Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager ArubaSyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\ArubaSyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ArubaSyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public ArubaSyslogV_1_0_0Recorder()
        {
            /*
            // TODO: Add any initialization after the InitComponent call          
           */
        }

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eðer line içinde tab boþluk var ise ve buna göre de split yapýlmak isteniyorsa true
        /// eðer line içinde tab boþluk var ise ve buna göre  split yapýlmak istenmiyorsa false
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

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);
                try
                {
                    try
                    {
                        rec.LogName = "ArubaSyslogV_1_0_0Recorder";
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogName: " + rec.LogName);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogName Error: " + exception.Message);
                    }

                    try
                    {
                        rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Datetime Error: " + exception.Message);
                    }

                    try
                    {
                        rec.EventType = args.EventLogEntType.ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + rec.EventType);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error: " + exception.Message);
                    }

                    try
                    {
                        if (args.Message.Length > 3999)
                        {
                            rec.Description = args.Message.Substring(0, 3999);
                        }
                        else
                        {
                            rec.Description = args.Message;
                        }
                        rec.Description = args.Message.Replace("'", "|");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Description Error: " + exception.Message);
                    }

                    string[] lineArr = SpaceSplit(args.Message, false);

                    try
                    {
                        rec.SourceName = lineArr[6];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "SourceName Error: " + exception.Message);
                    }

                    try
                    {
                        rec.EventType = lineArr[2];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr2 = lineArr[0].Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 Error: " + exception.Message);
                    }

                    //if (lineArr[9] == "<NOTI>")
                    {
                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].ToLower().StartsWith("username="))
                            {
                                try
                                {
                                    rec.UserName = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "UserName Error: " + exception.Message);
                                }

                            }

                            if (lineArr[i].ToLower().StartsWith("name="))
                            {
                                try
                                {
                                    rec.UserName = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "UserName Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("mac="))
                            {
                                try
                                {
                                    rec.CustomStr1 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("ip="))
                            {
                                try
                                {
                                    rec.CustomStr3 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("role="))
                            {
                                try
                                {
                                    rec.CustomStr6 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("profile="))
                            {
                                try
                                {
                                    rec.CustomStr7 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("vlan="))
                            {
                                try
                                {
                                    rec.CustomStr8 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("ap="))
                            {
                                try
                                {
                                    rec.CustomStr9 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("server="))
                            {
                                try
                                {
                                    rec.CustomStr10 = lineArr[i].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 Error: " + exception.Message);
                                }
                            }

                            if (lineArr[i].ToLower().StartsWith("dev_type="))
                            {
                                try
                                {
                                    rec.ComputerName = lineArr[i].Split(':')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error: " + exception.Message);
                                }
                            }
                        }
                    }

                    if (lineArr[9] == "<WARN>")
                    {
                        try
                        {
                            rec.ComputerName = Between(args.Message, "AP(", "):", 1);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr4 = Between(args.Message, "from ", " to ", 1);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 Error: " + exception.Message);
                        }

                        try
                        {
                            string s1 = After(args.Message, "to ");
                            rec.CustomStr5 = Before(s1, " ");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 Error: " + exception.Message);
                        }
                    }

                    if (lineArr[9] == "<NOTI>")
                    {
                        try
                        {
                            string s1 = After(args.Message, lineArr[11]);
                            rec.EventCategory = Before(s1, ":", 0);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                        }
                    }

                    else if (lineArr[9] == "<DBUG>")
                    {
                        try
                        {
                            rec.EventCategory = lineArr[12];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                        }

                        if (args.Message.Contains("Adding user:"))
                        {
                            rec.EventCategory = "Adding user:";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                        }
                    }

                    //else if (lineArr[9] == "<INFO>")
                    {
                        //for (int i = 0; i < lineArr.Length; i++)
                        //{
                        //    if (lineArr[i].StartsWith("username"))
                        //    {
                        //        rec.UserName = lineArr[i].Split('=')[1];
                        //    }

                        //    if (lineArr[i].StartsWith("IP"))
                        //    {
                        //        rec.CustomStr3 = lineArr[i].Split('=')[1];
                        //    }

                        //    if (lineArr[i].ToUpper().StartsWith("MAC") )
                        //    {
                        //        rec.CustomStr1 = lineArr[i].Split('=')[1];
                        //    }

                        //    if (lineArr[i].StartsWith("server"))
                        //    {
                        //        rec.CustomStr10 = lineArr[i].Split('=')[1];
                        //    }
                        //}
                    }

                    //else if (lineArr[9] == "<DBUG>")
                    {
                        //for (int i = 0; i < lineArr.Length; i++)
                        //{

                        //}
                    }

                    //else if (lineArr[9] == "<WARN>")
                    {
                        //for (int i = 0; i < lineArr.Length; i++)
                        //{

                        //}
                    }

                    rec.EventType = lineArr[9];
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
        }

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

        /// <summary
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// baþlangýç string
        /// <param name="b"></param>
        /// bitiþ string
        /// <returns></returns>
        public static string Between(string value, string a, string b, int type)
        {
            //type = 1 first index
            //type = 0 middle index
            //type = 2 last index

            int posA = 0;
            int posB = 0;

            if (type == 0)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
                posB = value.LastIndexOf(b, System.StringComparison.Ordinal);
            }

            if (type == 1)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
                posB = value.IndexOf(b, System.StringComparison.Ordinal);
            }

            if (type == 2)
            {
                posA = value.LastIndexOf(a, System.StringComparison.Ordinal);
                posB = value.LastIndexOf(b, System.StringComparison.Ordinal);
            }

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

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\ArubaSyslogV_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ArubaSyslogV_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ArubaSyslogV_1_0_0Recorder").GetValue("Trace Level"));
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
                EventLog.WriteEntry("Security Manager ArubaSyslogV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
