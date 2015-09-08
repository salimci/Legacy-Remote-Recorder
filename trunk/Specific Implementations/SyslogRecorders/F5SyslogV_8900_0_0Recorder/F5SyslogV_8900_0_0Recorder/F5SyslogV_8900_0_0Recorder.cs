using System;
using System.Globalization;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace F5SyslogV_8900_0_0Recorder
{
    public class F5SyslogV_8900_0_0Recorder : CustomBase
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
        private string guestLanguageId;
        private string cookieSupport;

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on F5SyslogV_8900_0_0Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on F5SyslogV_8900_0_0Recorder functions may not be running");
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
                slog.SyslogEvent += slog_SyslogEvent;

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager F5SyslogV_8900_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\F5SyslogV_8900_0_0Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager F5SyslogV_8900_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            var rec = new Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "F5SyslogV_8900_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    string line = args.Message;
                    string[] lineArr = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (line.Contains("<") && line.Contains(">"))
                    {
                        rec.EventCategory = Between(line, "<", ">");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                    }

                    if (rec.EventCategory == "HTTP_REQUEST")
                    {

                        for (int j = 0; j < lineArr.Length; j++)
                        {
                            if (lineArr[j] == "Host:")
                            {
                                rec.SourceName = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                            }

                            if (lineArr[j] == "Connection:")
                            {
                                rec.CustomStr2 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                            }

                            if (lineArr[j] == "X-Forwarded-For:")
                            {
                                rec.CustomStr3 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }

                            if (lineArr[j] == "Accept:")
                            {
                                rec.CustomStr4 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }

                            if (lineArr[j] == "Referer:")
                            {
                                rec.CustomStr6 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                            }

                            if (lineArr[j] == "Accept-Encoding:")
                            {
                                rec.CustomStr7 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                            }

                            if (lineArr[j] == "Accept-Language:")
                            {
                                rec.CustomStr8 = lineArr[j + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                            }

                            if (lineArr[j].Contains("JSESSIONID="))
                            {
                                if (lineArr[j].Contains("="))
                                {
                                    rec.CustomStr9 = lineArr[j].Split('=')[1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                                }
                            }

                            if (lineArr[j].StartsWith("GUEST_LANGUAGE_ID="))
                            {
                                guestLanguageId = lineArr[j].Split('=')[1];
                            }

                            if (lineArr[j].StartsWith("COOKIE_SUPPORT="))
                            {
                                cookieSupport = lineArr[j].Split('=')[1];
                            }
                        }

                        rec.CustomStr10 = guestLanguageId + " " + cookieSupport;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);

                        rec.CustomStr5 = Between(line, "User-Agent", "Accept-Encoding:");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);



                        if (lineArr.Length > 12)
                        {
                            rec.EventType = lineArr[12];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                        }

                        if (lineArr.Length > 14)
                        {
                            rec.CustomStr1 = lineArr[13] + " " + lineArr[14];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                        }
                    }

                    else if (rec.EventCategory == "LB_SELECTED")
                    {

                        rec.CustomStr3 = Between(line, "ClientIP:", "ClientPort:").Replace("***", " ").Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);

                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(Between(line, "ClientPort:", "Server:").Replace("***", " ").Trim());
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 type casting Error. ");
                        }

                        string[] str4Arr = After(line, "Server:").Split(' ');

                        if (str4Arr.Length > 1)
                        {
                            rec.CustomStr4 = str4Arr[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                        }

                        try
                        {
                            if (str4Arr.Length > 2)
                            {
                                rec.CustomInt4 = Convert.ToInt32(str4Arr[2]);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 type casting Error. ");
                        }
                    }

                    if (lineArr[0].Contains(":"))
                    {
                        rec.ComputerName = lineArr[0].Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                    }


                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;
                    rec.Description = args.Message.Replace("'", "|");
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
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
        }// After

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// baþlangýç string
        /// <param name="b"></param>
        /// bitiþ string
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\F5SyslogV_8900_0_0Recorder.log";
                var openSubKey = rk.OpenSubKey("Recorder");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("F5SyslogV_8900_0_0Recorder");
                    if (registryKey != null)
                        Syslog_Port = Convert.ToInt32(registryKey.GetValue("Syslog Port"));
                }
                var subKey = rk.OpenSubKey("Recorder");
                if (subKey != null)
                {
                    var key = subKey.OpenSubKey("F5SyslogV_8900_0_0Recorder");
                    if (key != null)
                        trc_level = Convert.ToInt32(key.GetValue("Trace Level"));
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager F5SyslogV_8900_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager F5SyslogV_8900_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
