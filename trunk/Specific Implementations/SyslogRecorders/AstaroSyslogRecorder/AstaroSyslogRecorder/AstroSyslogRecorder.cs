/*
 Updated by Onur SARIKAYA*/
using System;
using System.Globalization;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace AstroSyslogRecorder
{
    public class AstroSyslogRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
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
            //last_position = LastPosition;
            //fromend = FromEndOnLoss;
            //max_record_send = MaxLineToWait;
            //timer_interval = SleepTime;
            //user = User;
            //password = Password;
            //remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            remote_host = RemoteHost;
            zone = Zone;
        } // SetConfigData

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on AstroSyslogRecorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on AstroSyslogRecorder Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CiscoPixSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);

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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sgs_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing AstroSyslogRecorder Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager AstroSyslogRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        } // Init 

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\AstroSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager AstroSyslogRecorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Get_logDir

        public AstroSyslogRecorder()
        {

        }

        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() | Start preparing record");
                string sLogLine = args.Message;
                L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() | start line=" + sLogLine);

                if (string.IsNullOrEmpty(sLogLine) == true)
                {
                    L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Line Ýs Null or Empty");
                    return;
                }

                rec.LogName = "AstroSyslogRecorder";
                rec = LogFormat_Message(sLogLine, rec);
                L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() | Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() |  Finish Sending Data");
                }
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "AstroSyslogRecorder() | " + er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "AstroSyslogRecorder() | " + args.Message);
            }
        } // Sgs_SyslogEvent

        private Rec LogFormat_Message(string sLogLine, Rec rec)
        {
            try
            {
                L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");

                L.LogTimed(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + sLogLine);

                String[] fields = sLogLine.Split('=');
                string[] dDate = fields[0].Split(' ')[3].Split('-');
                string[] darr = dDate[0].Split(':');

                //rec.Datetime = darr[2] + "/" + darr[1] + "/" + darr[0] + " " + dDate[1];

                rec.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                rec.Description = sLogLine;
                if (rec.Description.Length > 900)
                {
                    rec.Description = rec.Description.Substring(0, 899);
                }

                string sKey = "";
                //for (int i = 0; i < fields.Length; i++)
                //{
                //    L.Log(LogType.FILE, LogLevel.DEBUG,
                //           "fields : " + fields[i]);
                //}
                for (int i = 0; i < fields.Length - 1; i++)
                {
                    sKey = fields[i].ToString();
                    string sLog = fields[i + 1].ToString();

                    if (i + 1 == fields.Length - 1 || sLog.Contains(" ") == false)
                        sLog = sLog.Trim('"') + " ";

                    if (sKey.Contains("sub"))
                        rec.EventType = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    if (sKey.Contains("sys"))
                        rec.SourceName = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    //if (sKey.Contains("tcpflags"))
                    //    rec.UserName = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    //if (sKey.Contains("severity"))
                    //    rec.ComputerName = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');// info 

                    //if (sKey.Contains("name") == true)
                    //    rec.CustomStr1 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    //if (sKey.Contains("severity"))
                    //    rec.ComputerName = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    //if (Regex.IsMatch(sKey, "\\b" + "action" + "\\b") == true)
                    //    rec.EventCategory = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');

                    if (sKey.Contains("method"))
                    {
                        rec.CustomStr2 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                             "CustomStr2 : " + rec.CustomStr2);
                    }

                    if (sKey.Contains("srcip"))
                    {
                        rec.CustomStr3 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                            "CustomStr3 : " + rec.CustomStr3);
                    }

                    if (sKey.Contains("dstip"))
                    {
                        rec.CustomStr4 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                           "CustomStr4 : " + rec.CustomStr4);
                    }

                    if (sKey.Contains("srcmac") || sKey.Contains("profile"))
                    {
                        rec.CustomStr5 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                           "CustomStr5 : " + rec.CustomStr5);
                    }

                    if (sKey.Contains("dstmac") || sKey.Contains("filteraction"))
                    {
                        rec.CustomStr6 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                          "CustomStr6 : " + rec.CustomStr6);
                    }

                    if (sKey.Contains("outitf") || sKey.Contains("categoryname"))
                    {
                        rec.CustomStr8 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                          "CustomStr8 : " + rec.CustomStr8);
                    }

                    if (sKey.Contains("initf") || Regex.IsMatch(sKey, "\\b" + "category" + "\\b"))
                    {
                        rec.CustomStr7 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                          "CustomStr7 : " + rec.CustomStr7);
                    }

                    if (sKey.Contains("content-type"))
                    {
                        rec.CustomStr9 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                          "CustomStr9 : " + rec.CustomStr9);
                    }

                    if (sKey.Contains("url"))
                    {
                        //rec.Description = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');
                        rec.CustomStr1 = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');// ok
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                            "CustomStr1 : " + rec.CustomStr1);
                    }
                    if (sKey.Contains("srcport"))
                    {
                        rec.CustomInt1 = ObjectToInt32(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                            "CustomInt1 : " + rec.CustomInt1);
                    }

                    if (sKey.Contains("dstport"))
                    {
                        rec.CustomInt2 = ObjectToInt32(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                           "CustomInt2 : " + rec.CustomInt2);
                    }

                    if (sKey.Contains("fwrule"))
                    {
                        rec.CustomInt3 = ObjectToInt32(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                            "CustomInt3 : " + rec.CustomInt3);
                    }

                    if (sKey.Contains("proto"))
                    {
                        rec.CustomInt4 = ObjectToInt32(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                            "CustomInt4 : " + rec.CustomInt4);
                    }

                    if (sKey.Contains("length") || sKey.Contains("size"))
                    {
                        rec.CustomInt5 = ObjectToInt32(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                             "CustomInt5 : " + rec.CustomInt5);
                    }

                    if (sKey.Contains("statuscode"))
                    {
                        rec.CustomInt6 = ObjectToInt64(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                             "CustomInt6 : " + rec.CustomInt6);
                    }
                    //application 

                    if (sKey.Contains("cached"))
                    {
                        rec.CustomInt7 = ObjectToInt64(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                           "CustomInt7 : " + rec.CustomInt7);
                    }

                    if (sKey.Contains("time"))
                    {
                        rec.CustomInt8 = ObjectToInt64(sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"'), 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                             "CustomInt8 : " + rec.CustomInt8);
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "action öncesi");
                    if (sKey.Contains("action"))
                    {
                        //rec.EventCategory = sLog.Substring(0, sLog.LastIndexOf(' ')).Trim('"');//
                        string[] s = sLogLine.Split(' ');
                        if (s[7].Split('=')[1].Contains("SecureWeb"))
                        {
                            if (s.Length > 8)
                            {
                                try
                                {
                                    //rec.EventCategory = s[11].Split('=')[1].Replace('"', ' ').Trim();
                                    for (int j = 0; j < s.Length; j++)
                                    {
                                        if (s[i].Contains("action"))
                                        {
                                            rec.EventCategory = s[i].Split('=')[1].Replace('"', ' ').Trim();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    //rec.EventCategory = s[15].Split('=')[1].Replace('"', ' ').Replace('"', ' ').Trim();
                                }
                            }
                        }
                        else if (s[7].Split('=')[1].Contains("SecureNet"))
                        {
                            if (s.Length > 12)
                            {
                                try
                                {
                                    rec.EventCategory = s[11].Split('=')[1].Replace('"', ' ').Trim();
                                }
                                catch (Exception)
                                {
                                    rec.EventCategory = s[12].Split('=')[1].Replace('"', ' ').Trim();
                                }
                            }
                        }
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "action sonrasý");
                L.Log(LogType.FILE, LogLevel.DEBUG, "description : " + rec.Description);

                if (!string.IsNullOrEmpty(rec.Description))
                {
                    if (sLogLine.Length > 900)
                    {
                        rec.Description = sLogLine.Substring(0, 899).ToString(CultureInfo.InvariantCulture); // ok
                    }
                    else
                    {
                        rec.Description = sLogLine;
                    }
                }

                //if (string.IsNullOrEmpty(rec.CustomStr1) == false && rec.CustomStr1.StartsWith("http://"))
                //    rec.CustomStr10 = rec.CustomStr1.Substring(0, (rec.CustomStr1.IndexOf('/', 7) == -1 ? rec.CustomStr1.Length : rec.CustomStr1.IndexOf('/', 7)));

                L.Log(LogType.FILE, LogLevel.DEBUG, "AstroSyslogRecorder() |  Finish Sending Data");
                try
                {
                    string url = rec.CustomStr1;
                    string[] urlArray = url.Split('/');
                    rec.CustomStr10 = urlArray[1] + urlArray[2];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Onur Customstr10 : " + rec.CustomStr10);

                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Onur Catch" + ex.Message);
                }
                return rec;
            }
            catch (Exception exp)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, exp.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() | Line " + sLogLine);
                rec.Description = sLogLine;
                return rec;
            }
        } // LogFormat_Message

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        } // Clear

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\AstroSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("AstroSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("AstroSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager AstroSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Read_Registry

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
                EventLog.WriteEntry("Security Manager AstroSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // Initialize_Logger

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);
            }
            catch
            {
                return iReturn;
            }
        } // ObjectToInt32

        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }
        } // ObjectToInt64
    }
}

