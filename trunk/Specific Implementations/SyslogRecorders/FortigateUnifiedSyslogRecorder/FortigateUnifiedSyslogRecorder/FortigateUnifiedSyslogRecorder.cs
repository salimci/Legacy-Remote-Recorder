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
    public class FortigateUnifiedSyslogRecorder : CustomBase
    {
        public static readonly Regex REG_FIELD = new Regex("(\"[^\"]*\")|([^=, ]+)=", RegexOptions.Compiled);
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on /SyslogRecorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on FortigateUnifiedSyslogRecorder  functions may not be running");
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
                EventLog.WriteEntry("Security Manager FortigateUnifiedSyslogRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool GetLogDir()
        {
            try
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    err_log = reg.GetValue("Home Directory") + @"log\FortigateUnifiedSyslogRecorder" + Id + ".log";
                    return true;
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FortigateUnifiedSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                    rec.LogName = "FortigateUnifiedSyslogRecorder";

                    L.Log(LogType.FILE, LogLevel.DEBUG, "args.Message: " + args.Message);

                    var line = args.Message ?? string.Empty;
                    var record = ParseFields(line);

                    try
                    {
                        try
                        {
                            var date = GetRecordField(record, "date", string.Empty, ToString);
                            if (string.IsNullOrEmpty(date))
                            {
                                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                                L.Log(LogType.FILE, LogLevel.ERROR, "No DateTime set to Now: [" + line + "]");
                            }
                            else
                            {
                                var time = GetRecordField(record, "time", string.Empty, ToString);
                                if (string.IsNullOrEmpty(time))
                                    rec.Datetime =
                                        DateTime.ParseExact(date, "yyyy-M-d", CultureInfo.InvariantCulture)
                                                .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                                else
                                    rec.Datetime =
                                        DateTime.ParseExact(date + " " + time, "yyyy-M-d H:m:s",
                                                            CultureInfo.InvariantCulture)
                                                .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                        catch
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "Possible Date Time Format Error: Date(" +
                                  GetRecordField(record, "date", string.Empty, ToString)
                                  + ") Time(" + GetRecordField(record, "time", string.Empty, ToString));
                            return;
                        }

                        rec.SourceName = GetRecordField(record, "level", string.Empty, ToString);
                        rec.EventCategory = GetRecordField(record, "subtype", string.Empty, ToString);
                        rec.EventType = GetRecordField(record, "type", string.Empty, ToString);
                        rec.ComputerName = GetRecordField(record, "devname", string.Empty, ToString);
                        rec.CustomStr1 = GetRecordField(record, "devid", string.Empty, ToString);
                        rec.CustomStr2 = GetRecordField(record, "status", string.Empty, ToString);
                        rec.CustomStr3 = GetRecordField(record, "srcip", string.Empty, ToString);
                        rec.CustomStr4 = GetRecordField(record, "dstip", string.Empty, ToString);
                        rec.CustomStr5 = GetRecordField(record, "strintf", string.Empty, ToString);
                        rec.CustomStr6 = GetRecordField(record, "catdesc", string.Empty, ToString);
                        rec.CustomStr7 = GetRecordField(record, "service", string.Empty, ToString);
                        rec.CustomStr8 = GetRecordField(record, "hostname", string.Empty, ToString);
                        rec.CustomStr9 = GetRecordField(record, "url", string.Empty, ToString);
                        rec.UserName = GetRecordField(record, "user", string.Empty, ToString);
                        rec.CustomInt1 = GetRecordField(record, "srcport", 0, ToInt32);
                        rec.CustomInt2 = GetRecordField(record, "dstport", 0, ToInt32);
                        rec.CustomInt4 = GetRecordField(record, "duration", 0, ToInt32);
                        rec.CustomInt6 = GetRecordField(record, "rcvdbyte", 0, ToInt32);
                        rec.CustomInt9 = GetRecordField(record, "sentbyte", 0, ToInt32);
                        rec.Description = line.Length > 900 ? line.Substring(0, 900) : line;
                    }
                    catch (Exception de)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Fieldset Error: " + de);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.ToString());
                }
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
                var back = REG_FIELD.Match(field);
                while (back.Success)
                {
                    if (back.Groups[2].Success)
                        break;
                    back = back.NextMatch();
                }
                if (!back.Success)
                    return record;
                var m = back.NextMatch();
                int index;
                string value;
                while (m.Success)
                {
                    if (m.Groups[2].Success)
                    {
                        index = back.Index + back.Length;
                        value = field.Substring(index, m.Index - index);
                        AddField(record, back.Groups[2].Value, value);
                        back = m;
                    }
                    m = m.NextMatch();
                }
                index = back.Index + back.Length;
                if (index < field.Length)
                {
                    value = field.Substring(index, field.Length - index);
                    if (value[0] == '"')
                        AddField(record, back.Groups[2].Value, value);
                    else
                        record[back.Groups[2].Value] = value;
                }
                else
                    record[back.Groups[2].Value] = string.Empty;
            }
            return record;
        }

        private void AddField(Dictionary<string, string> record, string key, string value)
        {
            if (value.Length == 0)
            {
                record[key] = value;
                return;
            }
            int st;
            char lookup;
            if (value[0] == '"')
            {
                st = 1;
                lookup = '"';
            }
            else
            {
                st = 0;
                lookup = value[value.Length - 1];
            }

            var index = st - 1;
            while (++index < value.Length && value[index] != lookup)
            {
            }
            record[key] = index > st ? value.Substring(st, index - st) : string.Empty;
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
                                               "FortigateUnifiedSyslogRecorder.log");
                    }
                    using (var recorder = key.OpenSubKey(@"Recorder\FortigateUnifiedSyslogRecorder"))
                    {
                        Syslog_Port = Convert.ToInt32(recorder.GetValue("Syslog Port"));

                        trc_level = Convert.ToInt32(recorder.GetValue("Trace Level"));
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FortigateUnifiedSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
