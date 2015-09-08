using System;
using System.Globalization;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace IlbankF5UserV_1_0_0Recorder
{
    public class IlbankF5UserV_1_0_0Recorder : CustomBase
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
                        if (!ReadRegistry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!InitializeLogger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IlbankF5UserV_1_0_0Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!GetLogDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log dir");
                            return;
                        }
                        else
                            if (!InitializeLogger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IlbankF5UserV_1_0_0Recorder functions may not be running");
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
                slog.SyslogEvent += SlogSyslogEvent;

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing IlbankF5UserV_1_0_0Recorder Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IlbankF5UserV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool GetLogDir()
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\IlbankF5UserV_1_0_0Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IlbankF5UserV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
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
        }// Before


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

        void SlogSyslogEvent(LogMgrEventArgs args)
        {
            Rec rec = new Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Log: " + args.Message);

                string line = args.Message;

                if (string.IsNullOrEmpty(args.Message))
                    return;

                char[] separator = new char[] { ' ' };
                string[] lineArr = line.Split(separator, StringSplitOptions.None);
                try
                {
                    rec.LogName = "IlbankF5UserV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    string tmpEventCategory1 = After(line, "F5-1");
                    string tmpEventCategory2 = Before(tmpEventCategory1, ":");
                    rec.EventCategory = tmpEventCategory2;

                    if (rec.EventCategory.Contains("tmm"))
                    {
                        string subLine = After(line, tmpEventCategory2);

                        try
                        {
                            rec.CustomStr1 = After(subLine, "Server:").Split(' ')[0];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 parsing error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr3 = Between(subLine, "ClientIP:", "***", 1);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 parsing error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr4 = After(subLine, "Server:").Split(' ')[1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 parsing error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr5 = subLine.Split(' ')[subLine.Split(' ').Length - 1];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 parsing error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(Between(subLine, "ClientPort:", "***", 0));
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 type casting error: " + exception.Message);
                            rec.CustomInt3 = 0;
                        }
                    }

                    if (tmpEventCategory2.Trim() == "info logger")
                    {
                        //foreach (var v in lineArr)
                        //{
                        //    if (v == "[ssl_acc]")
                        //    {
                        //        MessageBox.Show("[ssl_acc]");
                        //    }
                        //}

                        if (Between(line, "[", "]", 1) == "ssl_acc")
                        {
                            try
                            {
                                if (lineArr.Length > 15)
                                {
                                    rec.CustomStr1 = lineArr[15].Replace('"', ' ').Trim();
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 10)
                                {
                                    rec.CustomStr3 = lineArr[10];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 11)
                                {
                                    rec.CustomStr4 = lineArr[11];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 12)
                                {
                                    rec.CustomStr5 = lineArr[12];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 16)
                                {
                                    rec.CustomStr6 = lineArr[16];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 17)
                                {
                                    rec.CustomStr7 = lineArr[17];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 parsing error: " + exception.Message);
                            }
                        }

                        if (Between(line, "[", "]", 1) == "ssl_req")
                        {
                            try
                            {
                                if (lineArr.Length > 14)
                                {
                                    rec.CustomStr1 = lineArr[14].Replace('"', ' ').Trim();
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 13)
                                {
                                    rec.CustomStr2 = lineArr[12] + " " + lineArr[13];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 11)
                                {
                                    rec.CustomStr3 = lineArr[11];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 15)
                                {
                                    rec.CustomStr6 = lineArr[15];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 parsing error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr.Length > 17)
                                {
                                    rec.CustomStr7 = lineArr[17];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 parsing error: " + exception.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Log Parsing Error. " + e.Message);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish record parsing.");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                try
                {
                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 890) : args.Message;
                    rec.Description = args.Message.Replace("'", "|");

                    if (!string.IsNullOrEmpty(rec.EventCategory) && !string.IsNullOrEmpty(rec.Description))
                    {
                        CustomServiceBase s = GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Record sending error. " + exception.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

        public bool ReadRegistry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\IlbankF5UserV_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IlbankF5UserV_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IlbankF5UserV_1_0_0Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IlbankF5UserV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool InitializeLogger()
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
                EventLog.WriteEntry("Security Manager IlbankF5UserV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
