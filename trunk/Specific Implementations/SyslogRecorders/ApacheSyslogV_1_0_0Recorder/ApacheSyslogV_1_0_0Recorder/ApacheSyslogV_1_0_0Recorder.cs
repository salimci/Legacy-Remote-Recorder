using System;
using System.Globalization;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ApacheSyslogV_1_0_0Recorder
{
    public class ApacheSyslogV_1_0_0Recorder : CustomBase
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
                        if (!InitializeLogger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\ApacheSyslogV_1_0_0Recorder" + Id + ".log";
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

        void SlogSyslogEvent(LogMgrEventArgs args)
        {
            var rec = new Rec();
            string line = args.Message;
            string[] lineArr = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record. " + line);

                try
                {
                    rec.LogName = "ApacheSyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "DateTime: " + rec.Datetime);

                    if (lineArr.Length > 0)
                    {
                        rec.SourceName = lineArr[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                    }

                    if (lineArr.Length > 13)
                    {
                        rec.EventType = lineArr[13].Replace('\"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    }

                    if (lineArr.Length > 16)
                    {
                        rec.ComputerName = lineArr[6];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                    }

                    if (lineArr.Length > 8)
                    {
                        rec.CustomStr1 = lineArr[8];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    }
                    if (lineArr.Length > 18)
                    {
                        rec.CustomStr2 = lineArr[18].Replace('\"',' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                    }
                    if (lineArr.Length > 14)
                    {
                        rec.CustomStr3 = lineArr[14];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    }
                    if (lineArr.Length > 15)
                    {
                        rec.CustomStr4 = lineArr[15].Replace('\"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                    }

                    string[] CustomStr9Arr = line.Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                    rec.CustomStr9 = CustomStr9Arr[CustomStr9Arr.Length - 1];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);

                    try
                    {
                        if (lineArr.Length > 16)
                        {
                            rec.CustomInt1 = Convert.ToInt32(lineArr[16]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.WARN, "CustomInt1 Type Casting Error. " + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 17)
                        {
                            rec.CustomInt2 = Convert.ToInt32(lineArr[17]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 Type Casting Error. " + exception.Message);
                    }

                    rec.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;
                    rec.Description = args.Message.Replace("'", "|");
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR- " + line);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = GetInstanceService("Security Manager Remote Recorder");
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

        public bool ReadRegistry()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                RegistryKey subKey = null;
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\ApacheSyslogV_1_0_0Recorder.log";
                    subKey = rk.OpenSubKey("Recorder");
                }
                if (subKey != null)
                {
                    var key = subKey.OpenSubKey("ApacheSyslogV_1_0_0Recorder");
                    if (key != null)
                        Syslog_Port = Convert.ToInt32(key.GetValue("Syslog Port"));
                }
                if (rk != null)
                {
                    var openSubKey1 = rk.OpenSubKey("Recorder");
                    if (openSubKey1 != null)
                    {
                        var registryKey1 = openSubKey1.OpenSubKey("ApacheSyslogV_1_0_0Recorder");
                        if (registryKey1 != null)
                            trc_level = Convert.ToInt32(registryKey1.GetValue("Trace Level"));
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ApacheSyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
