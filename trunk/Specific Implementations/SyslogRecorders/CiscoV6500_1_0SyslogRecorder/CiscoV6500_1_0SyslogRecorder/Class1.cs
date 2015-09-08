//CiscoV6500_1_0SyslogRecorder


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

namespace CiscoV6500_1_0SyslogRecorder
{
    public class CiscoV6500_1_0SyslogRecorder : CustomBase
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
                EventLog.WriteEntry("Security Manager CiscoV6500_1_0SyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoV6500_1_0SyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoV6500_1_0SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public CiscoV6500_1_0SyslogRecorder()
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

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "CiscoV6500_1_0SyslogRecorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();

                    if (args.Message.Length > 899)
                        rec.Description = args.Message.Substring(0, 899);
                    else
                        rec.Description = args.Message;

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
                    rec.SourceName = args.Source;
                    L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

                    string[] lineArr = SpaceSplit(args.Message, false);

                    rec.ComputerName = lineArr[0];
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ComputerName : " + rec.ComputerName);

                    if (lineArr[2].Contains("."))
                    {
                        if (lineArr[2].Split('.')[1] == "notice")
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " !! Notice Mode !!");
                            rec.SourceName = lineArr[2].Split('.')[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " SourceName : " + rec.SourceName);

                            if (lineArr.Length > 10)
                            {
                                rec.EventType = lineArr[9] + lineArr[10];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventType : " + rec.EventType);
                            }

                            if (lineArr.Length > 8)
                            {
                                rec.CustomStr3 = lineArr[8];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3 : " + rec.CustomStr3);
                            }

                            if (lineArr.Length > 11)
                            {
                                rec.CustomStr4 = Before(lineArr[11], ":/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr4 : " + rec.CustomStr4);

                                if (After(lineArr[11], ":/").Length > 900)
                                {
                                    rec.CustomStr5 = After(lineArr[11], ":/").Substring(0, 900);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr5 : " + rec.CustomStr5);

                                    rec.CustomStr6 = After(lineArr[11], ":/").Substring(900,
                                                                                        After(lineArr[11], ":/").Length -
                                                                                        900);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr6 : " + rec.CustomStr6);
                                }
                                else
                                {
                                    rec.CustomStr5 = After(lineArr[11], ":/");
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr5 : " + rec.CustomStr5);
                                }
                            }
                        }

                        if (lineArr[2].Split('.')[1] == "debug")
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " !! Debug Mode !!");

                            rec.SourceName = lineArr[2].Split('.')[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " SourceName : " + rec.SourceName);

                            if (lineArr.Length > 8)
                            {
                                rec.EventCategory = lineArr[8];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventCategory : " + rec.EventCategory);
                            }

                            if (lineArr.Length > 11)
                            {
                                rec.EventType = lineArr[11];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventType : " + rec.EventType);
                            }

                            if (lineArr.Length > 9)
                            {
                                rec.CustomStr1 = lineArr[9];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr1 : " + rec.CustomStr1);
                            }

                            if (lineArr.Length > 12)
                            {
                                rec.CustomStr3 = Between(lineArr[12], "/", "(");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3 : " + rec.CustomStr3);

                                rec.CustomInt3 = Convert.ToInt32(Between(lineArr[12], "(", ")"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt3 : " + rec.CustomInt3);
                            }

                            if (lineArr.Length > 14)
                            {
                                rec.CustomStr4 = Between(lineArr[14], "/", "(");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr4 : " + rec.CustomStr4);

                                rec.CustomStr5 = Before(lineArr[14], "/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr5 : " + rec.CustomStr5);

                                rec.CustomInt4 = Convert.ToInt32(Between(lineArr[14], "(", ")"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt4 : " + rec.CustomInt4);
                            }
                        }

                        if (lineArr[2].Split('.')[1] == "warning")
                        {

                            L.Log(LogType.FILE, LogLevel.DEBUG, " !! Warning Mode !!");

                            rec.SourceName = lineArr[2].Split('.')[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " SourceName : " + rec.SourceName);

                            if (lineArr.Length > 8)
                            {
                                rec.EventCategory = lineArr[8];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventCategory : " + rec.EventCategory);
                            }

                            if (lineArr.Length > 9)
                            {
                                rec.EventType = lineArr[9];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventType : " + rec.EventType);
                            }

                            if (lineArr.Length > 16)
                            {
                                rec.CustomStr1 = lineArr[16].Replace('"', ' ').Trim();
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr1 : " + rec.CustomStr1);
                            }

                            if (lineArr.Length > 11)
                            {
                                rec.CustomStr2 = Before(lineArr[11], ":");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr2 : " + rec.CustomStr2);

                                rec.CustomStr3 = Between(lineArr[11], ":", "/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3 : " + rec.CustomStr3);

                                rec.CustomInt3 = Convert.ToInt32(After(lineArr[11], "/"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt3 : " + rec.CustomInt3);
                            }

                            if (lineArr.Length > 13)
                            {
                                rec.CustomStr4 = Between(lineArr[13], ":", "/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr4 : " + rec.CustomStr4);

                                rec.CustomStr5 = Before(lineArr[13], ":");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr5 : " + rec.CustomStr5);

                                rec.CustomInt4 = Convert.ToInt32(After(lineArr[13], "/"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt4 : " + rec.CustomInt4);
                            }
                        }

                        if (lineArr[2].Split('.')[1] == "error")
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " !! Error Mode !!");

                            if (args.Message.Contains("Denied ICMP"))
                            {
                                //-MessageBox.Show("error 1");

                                L.Log(LogType.FILE, LogLevel.DEBUG, " !! Error Mode 1 !!");

                                rec.SourceName = lineArr[2].Split('.')[1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " SourceName : " + rec.SourceName);

                                if (lineArr.Length > 8)
                                {
                                    rec.EventCategory = lineArr[8];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " EventCategory : " + rec.EventCategory);
                                }

                                if (lineArr.Length > 9)
                                {
                                    rec.EventType = lineArr[9];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " EventType : " + rec.EventType);
                                }

                                if (lineArr.Length > 16)
                                {
                                    rec.CustomStr2 = lineArr[16];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr2 : " + rec.CustomStr2);
                                }
                                if (lineArr.Length > 13)
                                {
                                    rec.CustomStr3 = lineArr[13];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3: " + rec.CustomStr3);
                                }
                            }
                            else
                            {
                                //MessageBox.Show("error 2");

                                L.Log(LogType.FILE, LogLevel.DEBUG, " !! Error Mode 2 !!");

                                rec.SourceName = lineArr[2].Split('.')[1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " SourceName : " + rec.SourceName);

                                rec.EventCategory = lineArr[10];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventCategory : " + rec.EventCategory);

                                rec.EventType = lineArr[8];
                                L.Log(LogType.FILE, LogLevel.DEBUG, " EventType : " + rec.EventType);

                                rec.CustomStr3 = Before(lineArr[14], "/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3 : " + rec.CustomStr3);

                                rec.CustomInt3 = Convert.ToInt32(After(lineArr[14], "/"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt3 : " + rec.CustomInt3);

                                rec.CustomStr4 = Between(lineArr[16], ":", "/");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr4 : " + rec.CustomStr4);

                                rec.CustomStr5 = Before(lineArr[16], ":");
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr5 : " + rec.CustomStr5);
                                rec.CustomInt4 = Convert.ToInt32(After(lineArr[16], "/"));
                                L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt4 : " + rec.CustomInt4);


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
