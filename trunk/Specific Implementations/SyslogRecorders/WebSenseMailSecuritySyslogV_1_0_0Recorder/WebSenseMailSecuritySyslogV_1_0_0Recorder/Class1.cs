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

namespace WebSenseMailSecuritySyslogV_1_0_0Recorder
{
    public class WebSenseMailSecuritySyslogV_1_0_0Recorder : CustomBase
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
                EventLog.WriteEntry("Security Manager WebSenseMailSecuritySyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\WebSenseMailSecuritySyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WebSenseMailSecuritySyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public WebSenseMailSecuritySyslogV_1_0_0Recorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
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
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
            L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                try
                {
                    rec.LogName = "WebSenseMailSecuritySyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();
                    #region Description
                    if (args.Message.Length > 899)
                    {
                        rec.Description = args.Message.Substring(0, 899);
                    }
                    else
                    {
                        rec.Description = args.Message;
                    }
                    rec.Description = args.Message.Replace("'", "|");
                    #endregion

                    string line = args.Message;
                    string[] lineArr = SpaceSplit(line, true);
                    try
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.SourceName = lineArr[7];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "SourceName Error: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 8 && lineArr[8].Contains("|"))
                        {
                            rec.CustomStr5 = lineArr[8];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                        }
                        else
                        {
                            rec.CustomStr5 = GetStringValue(lineArr, "CustomStr5", "src=");
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 Error: " + exception.Message);
                    }

                    rec.ComputerName = GetStringValue(lineArr, "ComputerName", "dvc=");
                    rec.CustomStr1 = GetStringValue(lineArr, "CustomStr1", "üşer=");

                    if (string.IsNullOrEmpty(rec.CustomStr1))
                    {
                        rec.CustomStr1 = GetStringValue(lineArr, "CustomStr1", "duser=");
                    }

                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr.Length > 8 && lineArr[i].Contains("|"))
                        {
                            rec.EventCategory = lineArr[i].Split('|')[4];
                        }
                    }

                    rec.EventType = GetStringValue(lineArr, "EventType", "act=");
                    rec.CustomStr2 = Between(line, "msg=", "in=");
                    rec.CustomStr3 = GetStringValue(lineArr, "CustomStr3", "suser=");
                    rec.CustomStr4 = GetStringValue(lineArr, "CustomStr4", "dst=");

                    rec.CustomStr6 = GetStringValue(lineArr, "CustomStr6", "deviceDirection=");
                    rec.CustomStr7 = GetStringValue(lineArr, "CustomStr7", "deviceFacility=");

                    rec.CustomInt6 = GetIntValue(lineArr, "CustomInt6", "externalId=");
                    rec.CustomInt7 = GetIntValue(lineArr, "CustomInt7", "messageId=");
                    rec.CustomInt8 = GetIntValue(lineArr, "CustomInt8", "rt=");
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

        public string GetStringValue(string[] lineArr, string columnValue, string value)
        {
            string returnValue = "";
            for (int i = 0; i < lineArr.Length; i++)
            {
                try
                {
                    if (lineArr[i].StartsWith(value))
                    {
                        if (lineArr[i].Contains("="))
                        {
                            returnValue = lineArr[i].Split('=')[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, columnValue + ": " + returnValue);
                        }
                        else
                        {
                            returnValue = "";
                            L.Log(LogType.FILE, LogLevel.DEBUG, columnValue + ": Value is not recognized.");
                        }
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "GetStringValue()---->" + columnValue + " Error: " + exception.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, "GetStringValue()---->" + columnValue + " Error: " + exception.StackTrace);
                }
            }
            return returnValue;
        } // GetStringValue

        public long GetIntValue(string[] lineArr, string columnValue, string value)
        {
            Int64 returnValue = 0;
            for (int i = 0; i < lineArr.Length; i++)
            {
                try
                {
                    if (lineArr[i].StartsWith(value))
                    {
                        returnValue = Convert.ToInt64(lineArr[i].Split('=')[1]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, columnValue + ": " + returnValue);
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "GetIntValue()---->" + columnValue + "Type Casting Error: " + exception.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, "GetIntValue()---->" + columnValue + "Type Casting Error: " + exception.StackTrace);
                    returnValue = 0;
                    L.Log(LogType.FILE, LogLevel.ERROR, "GetIntValue()---->" + columnValue + "Type Casting Error, " + columnValue + "setted 0");

                }
            }
            return returnValue;
        } // GetStringValue

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\WebSenseMailSecuritySyslogV_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("WebSenseMailSecuritySyslogV_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("WebSenseMailSecuritySyslogV_1_0_0Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WebSenseMailSecuritySyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager WebSenseMailSecuritySyslogV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
