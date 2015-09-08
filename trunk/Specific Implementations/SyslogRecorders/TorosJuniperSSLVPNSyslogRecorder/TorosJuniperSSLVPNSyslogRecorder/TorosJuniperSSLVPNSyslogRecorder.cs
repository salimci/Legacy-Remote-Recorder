using System;
using System.Globalization;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TorosJuniperSSLVPNSyslogRecorder
{
    public class TorosJuniperSSLVPNSyslogRecorder : CustomBase
    {
        //
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
        public string dateFormat = "yyyy-MM-dd HH:mm:ss";


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
                        if (!ReadRegistry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        if (!InitializeLogger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TorosJuniperSSLVPNSyslogRecorder functions may not be running");
                            return;
                        }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!GetlogDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log dir");
                            return;
                        }
                        if (!InitializeLogger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TorosJuniperSSLVPNSyslogRecorder functions may not be running");
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
                slog.SyslogEvent += SyslogEvent;

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool GetlogDir()
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\TorosJuniperSSLVPNSyslogRecorder" + Id + ".log";
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

        void SyslogEvent(LogMgrEventArgs args)
        {
            var rec = new Rec();
            string line = args.Message;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record. " + args.Message);
                try
                {
                    string[] lineArr = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    rec.LogName = "TorosJuniperSSLVPNSyslogRecorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;
                    rec.Description = args.Message.Replace("'", "|");

                    rec.ComputerName = line.Split(':')[0];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);

                    rec.SourceName = lineArr[2];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);

                    DateTime dt = Convert.ToDateTime(lineArr[4] + " " + lineArr[5]);
                    rec.Datetime = dt.ToString(dateFormat);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);


                    rec.CustomStr3 = Between(line, "[", "]", 1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);

                    rec.UserName = Between(line, "]", "(", 1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);

                    rec.CustomStr2 = Between(line, "(", ")", 1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);

                    rec.CustomStr5 = Between(line, "[", "]", 2);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);

                    try
                    {
                        foreach (string t in lineArr)
                        {
                            if (t.StartsWith("result="))
                            {
                                try
                                {
                                    rec.CustomInt3 = Convert.ToInt32(t.Split('=')[1]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 type casting error. " + exception.Message);
                                }
                            }

                            if (t.StartsWith("sent="))
                            {
                                try
                                {
                                    rec.CustomInt4 = Convert.ToInt32(t.Split('=')[1]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 type casting error. " + exception.Message);
                                }
                            }

                            if (t.StartsWith("received="))
                            {
                                try
                                {
                                    rec.CustomInt5 = Convert.ToInt32(t.Split('=')[1]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 type casting error. " + exception.Message);
                                }
                            }
                        }
                        if (lineArr.Length > 25)
                        {
                            try
                            {
                                rec.CustomInt6 = Convert.ToInt64(lineArr[25]);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 type casting error.: " + exception.Message);

                            }
                        }

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR: " + exception.Message);

                    }

                    if (line.Contains("Number of concurrent users logged in to the device"))
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "line Contains, " + "Number of concurrent users logged in to the device");

                            rec.CustomStr1 = "Number of concurrent users logged in to the device";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "System";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            rec.CustomInt1 = Convert.ToInt32(After(line, ":", 0));
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Number of concurrent users logged in to the device' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Number of JCP connections"))
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "line Contains, " + "Number of JCP connections");

                            rec.CustomStr1 = "Number of JCP connections";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "System";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            rec.CustomInt1 = Convert.ToInt32(After(line, ":", 0));
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Number of JCP connections' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Number of NCP connections"))
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "line Contains, " + "Number of NCP connections");

                            rec.CustomStr1 = "Number of NCP connections";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "System";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            rec.CustomInt1 = Convert.ToInt32(After(line, ":", 0));
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Number of NCP connections' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("User Accounts modified."))
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "line Contains, " + "User Accounts modified");

                            rec.CustomStr1 = "User Accounts modified.";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "Login";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            string str4 = Between(line, "User Accounts modified. ", " ", 0);
                            rec.CustomStr4 = str4.Split(' ')[0];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);

                            for (var i = 0; i < lineArr.Length; i++)
                            {
                                if (lineArr[i] == "username")
                                {
                                    rec.CustomStr6 = lineArr[i + 1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'User Accounts modified' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Login succeeded"))
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "line Contains, " + "Login succedd.");

                            rec.CustomStr1 = "Login succeeded";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "Login";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Login succeeded' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Login failed"))
                    {
                        try
                        {
                            rec.CustomStr1 = "Login failed";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "Login";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            rec.CustomStr6 = After(line, "Reason:", 0);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Login failed' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Session timed out"))
                    {
                        try
                        {
                            rec.CustomStr1 = "Session timed out";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "Logout";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            string timeOutDateString = Between(line, "(", ")", 2);
                            string timeOutDate = After(timeOutDateString, "at", 0);

                            rec.CustomStr8 = timeOutDate;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'Session timed out' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("WebRequest ok"))
                    {
                        try
                        {
                            rec.CustomStr1 = "WebRequest ok";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "WebRequest";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            rec.CustomStr7 = Between(line, "Host:", ",", 1);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);

                            var request = After(line, "Request:", 0);
                            if (!string.IsNullOrEmpty(request) && request.Contains(" "))
                            {
                                var str9 = request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                rec.CustomStr9 = str9;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);

                                var str10 = After(request,
                                                     request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0
                                                         ], 0);
                                rec.CustomStr10 = str10;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'WebRequest ok' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("WebRequest completed"))
                    {
                        try
                        {
                            rec.CustomStr1 = "WebRequest completed";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                            rec.EventCategory = "WebRequest";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                            var ipLine = After(line, "from ", 0);

                            if (!string.IsNullOrEmpty(ipLine) && ipLine.Contains(" "))
                            {
                                var ip = ipLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                rec.CustomStr7 = ip;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                            }

                            if (lineArr.Length > 16)
                            {
                                rec.CustomStr9 = lineArr[16];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                            }

                            if (lineArr.Length > 18)
                            {
                                rec.CustomStr10 = lineArr[18];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                            }

                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'WebRequest ok' mode error. " + exception.Message);
                        }
                    }

                    if (line.Contains("Logout from"))
                    {
                        try
                        {
                            rec.EventCategory = "Logout";
                            rec.CustomStr1 = "Logout";

                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            if (lineArr.Length > 16)
                            {
                                rec.CustomStr3 = lineArr[16];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "'WebRequest ok' mode error. " + exception.Message);
                        }
                    }//
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
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
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a, int type)
        {
            //type = 0 first
            //type = 1 last
            int posA = 0;

            if (type == 1)
            {
                posA = value.IndexOf(a, StringComparison.Ordinal);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a, StringComparison.Ordinal);
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
        }


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
                posA = value.IndexOf(a, StringComparison.Ordinal);
                posB = value.LastIndexOf(b, StringComparison.Ordinal);
            }

            if (type == 1)
            {
                posA = value.IndexOf(a, StringComparison.Ordinal);
                posB = value.IndexOf(b, StringComparison.Ordinal);
            }

            if (type == 2)
            {
                posA = value.LastIndexOf(a, StringComparison.Ordinal);
                posB = value.LastIndexOf(b, StringComparison.Ordinal);
            }

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            var adjustedPosA = posA + a.Length;
            return adjustedPosA >= posB ? "" : value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        public bool ReadRegistry()
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
                    var registryKey = rk.OpenSubKey("Agent");
                    if (registryKey != null)
                        err_log = registryKey.GetValue("Home Directory") + @"log\TorosJuniperSSLVPNSyslogRecorder.log";
                    var subKey = rk.OpenSubKey("Recorder");
                    if (subKey != null)
                    {
                        var key = subKey.OpenSubKey("TorosJuniperSSLVPNSyslogRecorder");
                        if (key != null)
                            Syslog_Port = Convert.ToInt32(key.GetValue("Syslog Port"));
                    }
                    var openSubKey1 = rk.OpenSubKey("Recorder");
                    if (openSubKey1 != null)
                    {
                        var registryKey1 = openSubKey1.OpenSubKey("TorosJuniperSSLVPNSyslogRecorder");
                        if (registryKey1 != null)
                            trc_level = Convert.ToInt32(registryKey1.GetValue("Trace Level"));
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TorosJuniperSSLVPNSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        /// <summary>
        /// Thþs function is create new RemoteRecorder's error, warn, debug etc... log file.
        /// </summary>
        /// <returns></returns>
        public bool InitializeLogger()
        {
            try
            {
                L = new CLogger();
                L.SetLogLevel((LogLevel)((trc_level < 0 || trc_level > 4) ? 3 : trc_level));
                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("RemoteRecorderBase->InitializeLogger() ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // InitializeLogger
    }
}
