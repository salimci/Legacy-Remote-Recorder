using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Natek.Helpers.Text;

namespace ZimbraMailSyslogV_1_0_0Recorder
{
    public class ZimbraMailSyslogV_1_0_0Recorder : CustomBase
    {

        static Regex RegInputLine = new Regex("^([^ \t]*)[ \t]+:[ \t]*([^ \t]*)[ \t]+([^ \t]*[ \t]+[^ \t]*[ \t]+[^ \t]*)[ \t]+([^ \t]*)([^:]*): (.*)$");
        static Regex RegField = new Regex("([^ \t=]+)=('([^']*)'|<([^>]*)>|\\[(\\]*)\\]|([^'<[][^, \t]*))");
        static Regex RegQuotedLine = new Regex("\\=\\?([^?]*)\\?([^?]*)\\?(([^ ?]*)(\\?\\=\\?|\\?| )?)", RegexOptions.Multiline);
        static Regex RegSubject = new Regex("Subject:(.*) from [^[;]*(\\[([^]]*)\\])?;");
        static Regex RegCategory = new Regex("^(([^/]*/)*)([^[*]*)(\\[[^]]*\\])?");
        static Regex RegCodepage = new Regex("^[^0-9]*([0-9]+)");
        static Dictionary<int, Encoding> codepageLookup = new Dictionary<int, Encoding>();

        private const uint logging_interval = 60000;
        private const uint log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id;
        protected String virtualhost, Dal;
        private List<string> encodingList;

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ZimbraMailSyslogV_1_0_0Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ZimbraMailSyslogV_1_0_0Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager ZimbraMailSyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\ZimbraMailSyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ZimbraMailSyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        private static string GetMatchValue(Match m, int from, int to, string defaultValue)
        {
            while (from <= to && from < m.Groups.Count)
            {
                if (m.Groups[from].Success)
                    return m.Groups[from].Value;
                ++from;
            }
            return defaultValue;
        }

        private static void DecodeLine(StringBuilder lineSb, string line)
        {
            var m = RegQuotedLine.Match(line);
            var lastIndex = 0;
            while (m.Success)
            {
                if (m.Index > lastIndex)
                    lineSb.Append(line.Substring(lastIndex, m.Index - lastIndex));
                var encoding = GetEncoding(m.Groups[1].Value);
                try
                {
                    if (m.Groups[2].Value.StartsWith("Q"))
                        lineSb.Append(QuotedPrintable.Decode(encoding, m.Groups[4].Value));
                    else if (m.Groups[2].Value.StartsWith("B"))
                        lineSb.Append(encoding.GetString(Convert.FromBase64String(EnsureLength(m.Groups[4].Value))));
                    else
                        lineSb.Append(m.Groups[3].Value);
                }
                catch
                {
                    lineSb.Append(m.Groups[0].Value);
                }
                lastIndex = m.Index + m.Length;
                m = m.NextMatch();
            }
            lineSb.Append(line.Substring(lastIndex));
        }

        private static string EnsureLength(string p)
        {
            if (p == null || p.Length % 4 == 0)
                return p;
            int mod = p.Length % 4;

            return mod < 2 ? p.Substring(0, p.Length - mod) : p.PadRight(p.Length + 4 - mod, '=');
        }

        static Encoding GetEncoding(string encodingName)
        {
            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch
            {
                try
                {
                    var m = RegCodepage.Match(encodingName);
                    if (m.Success)
                    {
                        var cp = int.Parse(m.Groups[1].Value);
                        Encoding encoding;
                        if (codepageLookup.TryGetValue(cp, out encoding))
                            return encoding;
                        encoding = Encoding.GetEncoding(cp);
                        codepageLookup[cp] = encoding;
                        return encoding;
                    }
                }
                catch
                {
                }
            }
            return Encoding.UTF8;
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            var rec = new Rec();
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
            L.Log(LogType.FILE, LogLevel.DEBUG, " Log : " + args.Message);

            try
            {
                try
                {
                    rec.LogName = "ZimbraMailSyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    var lineSb = new StringBuilder();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Decoding Line");
                    DecodeLine(lineSb, args.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Decode Complete. Processing Line..");

                    var line = lineSb.ToString();
                    lineSb.Remove(0, lineSb.Length);

                    rec.Description = line;
                    if (rec.Description.Length > 900)
                    {
                        rec.Description = rec.Description.Substring(0, 900);
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Check Line match");
                    var m = RegInputLine.Match(line);
                    if (m.Success)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line match Ok, split accordingly");
                        line = m.Groups[6].Value;

                        var i = line.IndexOf(": ");
                        rec.CustomStr1 = i > 0 ? line.Substring(0, i).Trim() : string.Empty;
                        rec.CustomStr2 = m.Groups[4].Value.Trim();

                        var msub = RegCategory.Match(m.Groups[5].Value);
                        rec.EventCategory = msub.Success ? msub.Groups[3].Value : m.Groups[5].Value;

                        msub = RegField.Match(line);
                        while (msub.Success)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Getting sub part value");
                            var value = GetMatchValue(msub, 3, 6, string.Empty).Trim();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Sub Part: " + value);
                            switch (msub.Groups[1].Value)
                            {
                                case "to":
                                    rec.CustomStr4 = value;
                                    break;
                                case "from":
                                    rec.CustomStr5 = value;
                                    break;
                                case "size":
                                    if (rec.EventCategory != "cleanup")
                                        rec.CustomStr6 = value;
                                    break;
                                case "status":
                                    if (rec.EventCategory != "cleanup")
                                        rec.CustomStr7 = value;
                                    break;
                                case "relay":
                                    if (rec.EventCategory != "cleanup")
                                        rec.CustomStr8 = value;
                                    break;
                                case "nrcpt":
                                    rec.CustomStr9 = value;
                                    break;
                                case "delay":
                                    rec.CustomStr10 = value;
                                    break;
                                case "proto":
                                    if (rec.EventCategory == "cleanup")
                                        rec.CustomStr6 = value;
                                    break;
                                case "helo":
                                    if (rec.EventCategory == "cleanup")
                                        rec.CustomStr7 = value;
                                    break;
                            }
                            msub = msub.NextMatch();
                        }
                        if (rec.EventCategory == "cleanup")
                        {
                            msub = RegSubject.Match(line);
                            if (msub.Success)
                            {
                                rec.CustomStr3 = GetMatchValue(msub, 1, 1, string.Empty).Trim();
                                rec.CustomStr8 = GetMatchValue(msub, 3, 3, string.Empty).Trim();
                            }
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " No match. Insert in raw");
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetData(Dal, virtualhost, rec);
                customServiceBase.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

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
                    rk.Close();
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
                EventLog.WriteEntry("Security Manager ZimbraMailSyslogV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}