using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CustomTools;
using Log;
using LogMgr;
using Microsoft.Win32;

namespace Natek.Recorders.Remote
{
    public class WebsenseV_7_7_2SyslogRecorder : CustomBase
    {
        public static readonly Regex REG_FIELD = new Regex(@"([^\\= ]+)=", RegexOptions.Compiled);
        public static readonly char[] WHITESPACE = new[] { ' ', '\t' };

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

        private Encoding SyslogEncoding = Encoding.ASCII;
        public string SysllogLogFile;
        public Int32 SyslogLogSize;

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


            if (!string.IsNullOrEmpty(CustomVar1))
            {
                string[] customArr = CustomVar1.Split(';');
                for (int i = 0; i < customArr.Length; i++)
                {
                    if (customArr[i].StartsWith("E="))
                    {
                        SyslogEncoding = Encoding.GetEncoding(customArr[i].Split('=')[1]);
                    }

                    if (customArr[i].StartsWith("Lf="))
                    {
                        SysllogLogFile = customArr[i].Split('=')[1];
                    }

                    if (customArr[i].StartsWith("Ls="))
                    {
                        SyslogLogSize = int.Parse(customArr[i].Split('=')[1]);
                    }
                }
            }
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WebsenseV_7_7_2SyslogRecorder Recorder functions may not be running");
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
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WebsenseV_7_7_2SyslogRecorder  functions may not be running");
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

                    if (!string.IsNullOrEmpty(SysllogLogFile))
                    {
                        slog.EnablePacketLog = true;
                        slog.LogFile = SysllogLogFile;
                        slog.LogSize = SyslogLogSize;
                    }
                    slog.Encoding = SyslogEncoding;
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WebsenseV_7_7_2SyslogRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool GetLogDir()
        {
            try
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    err_log = reg.GetValue("Home Directory") + @"log\WebsenseV_7_7_2SyslogRecorder" + Id + ".log";
                    return true;
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WebsenseV_7_7_2SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
            }
            return false;
        }

        public void slog_SyslogEvent(LogMgrEventArgs args)
        {
            try
            {
                var rec = new CustomBase.Rec();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "WebsenseV_7_7_2SyslogRecorder";

                    L.Log(LogType.FILE, LogLevel.DEBUG, "args.Message: " + args.Message);

                    var line = args.Message ?? string.Empty;
                    var fields = line.Split('|');
                    var record = ParseFields(GetField(fields, 7));

                    rec.Description = GetRecordField(record, "dhost", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);

                    try
                    {
                        var dateArr = GetField(fields, 0).Split(WHITESPACE);
                        DateTime dateTime;
                        if (DateTime.TryParseExact(dateArr[0] + " " + dateArr[1], "M-d-yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            rec.Datetime = dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Date error: " + exception.Message);
                    }

                    rec.ComputerName = GetRecordField(record, "dvc", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Computer Name: " + rec.ComputerName);

                    rec.EventCategory = GetField(fields, 4);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                    rec.EventType = GetRecordField(record, "app", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);

                    rec.UserName = GetRecordField(record, "suser", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);

                    rec.CustomStr1 = GetRecordField(record, "act", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                    rec.CustomStr2 = GetRecordField(record, "requestMethod", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);

                    rec.CustomStr3 = GetRecordField(record, "src", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);

                    rec.CustomStr4 = GetRecordField(record, "dst", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);

                    rec.CustomStr6 = GetRecordField(record, "request", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);

                    rec.CustomStr8 = GetRecordField(record, "requestClientApplication", string.Empty, ToString);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);

                    rec.CustomInt3 = GetRecordField(record, "dpt", 0, ToInt32);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);

                    rec.CustomInt4 = GetRecordField(record, "spt", 0, ToInt32);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);

                    rec.CustomInt7 = GetRecordField(record, "in", 0, ToInt32);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7);

                    rec.CustomInt8 = GetRecordField(record, "out", 0, ToInt32);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8);
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

        private string GetField(string[] fields, int index)
        {
            return fields != null && fields.Length > index ? fields[index] : string.Empty;
        }

        static string ToString(string value)
        {
            return value != null ? value.Trim() : value;
        }

        static int ToInt32(string value)
        {
            return int.Parse(value);
        }

        private T GetRecordField<T>(Dictionary<string, string> record, string field, T defaultValue, Converter<string, T> converter)
        {
            string value;
            if (record.TryGetValue(field, out value))
            {
                try
                {
                    return converter(value);
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        private Dictionary<string, string> ParseFields(string field)
        {
            var record = new Dictionary<string, string>();
            if (field != null)
            {
                var m = REG_FIELD.Match(field);
                if (m.Success)
                {
                    do
                    {
                        var keyword = m.Groups[1].Value;
                        var i = m.Index + m.Length;
                        var next = m.NextMatch();
                        record[keyword] = next.Success ? field.Substring(i, next.Index - i) : field.Substring(i); ;
                        m = next;
                    } while (m.Success);
                }
            }
            return record;
        }

        public bool Read_Registry()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager"))
                {
                    using (var agent = key.OpenSubKey("Agent"))
                    {
                        err_log = Path.Combine(Path.Combine(agent.GetValue("Home Directory") as string, "log"),
                                               "WebsenseV_7_7_2SyslogRecorder.log");
                    }
                    using (var recorder = key.OpenSubKey(@"Recorder\WebsenseV_7_7_2SyslogRecorder"))
                    {
                        Syslog_Port = Convert.ToInt32(recorder.GetValue("Syslog Port"));

                        trc_level = Convert.ToInt32(recorder.GetValue("Trace Level"));
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WebsenseV_7_7_2SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
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
