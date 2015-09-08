using System;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace CheckPointSyslogV_1_0_0Recorder
{
    public class CheckPointSyslogV_1_0_0Recorder : CustomBase
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
        private Encoding SyslogEncoding = Encoding.ASCII;
        public string SysllogLogFile;
        public Int32 SyslogLogSize;

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
            //


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
                    slog.Encoding = SyslogEncoding;
                }

                slog.Start();
                slog.SyslogEvent += slog_SyslogEvent;

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
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CheckPointSyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
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

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            string line = args.Message;

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "CheckPointSyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    if (line.Length > 899)
                    {
                        rec.Description = line.Substring(0, 899);
                        rec.CustomStr10 = line.Substring(899, line.Length - 899);
                    }
                    else
                    {
                        rec.Description = args.Message;
                    }

                    rec.Description = args.Message.Replace("'", "|");

                    string[] lineArr = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    string[] subLineArr = line.Split(':');

                    if (lineArr.Length > 6)
                        rec.EventCategory = lineArr[6];

                    #region encrypt OK
                    if (lineArr[6] == "encrypt")
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "encrypt record started.");
                        if (lineArr.Length > 7)
                        {
                            rec.SourceName = lineArr[7];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName:" + rec.SourceName);
                        }

                        rec.CustomStr3 = Between(line, "src:", "dst:").Replace(':', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3:" + rec.CustomStr3);
                        rec.CustomStr4 = Between(line, "dst:", "proto:").Replace(':', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4:" + rec.CustomStr4);
                    }
                    #endregion

                    #region allow
                    if (lineArr[6] == "allow")
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "allow record started.");

                        if (lineArr.Length > 7)
                        {
                            rec.SourceName = lineArr[7];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName:" + rec.SourceName);
                        } //ok

                        rec.CustomStr10 = Between(line, "resource:", "product:"); //ok
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10:" + rec.CustomStr10);

                        string[] resourceArr = Between(line, "resource:", "product:").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        rec.CustomStr2 = resourceArr[0] + @"//" + resourceArr[1]; //ok
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2:" + rec.CustomStr2);

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                              StringSplitOptions.RemoveEmptyEntries);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2:" + subLineArr[i + 1]);
                                rec.UserName = userNameArr[1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2:" + rec.CustomStr2);

                                rec.CustomStr1 = userNameArr[0];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2:" + rec.CustomStr2);

                            }
                            if (subLineArr[i].Trim() == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_desc")
                            {
                                rec.CustomStr5 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_category")
                            {
                                rec.CustomStr6 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "matched_category")
                            {
                                rec.CustomStr7 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_risk")
                            {
                                rec.CustomStr8 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_rule_name")
                            {
                                rec.CustomStr9 = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region monitor OK
                    if (lineArr[6] == "monitor")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.SourceName = lineArr[7];
                        }
                        rec.EventType = Between(line, "proto:", "product:").Replace(':', ' ').Trim();
                        rec.CustomStr3 = Between(line, "src:", "dst:").Replace(':', ' ').Trim();
                        rec.CustomStr4 = Between(line, "dst:", "proto:").Replace(':', ' ').Trim();
                        rec.CustomStr5 = Between(line, "product:", "service:").Replace(':', ' ').Trim();
                        rec.CustomStr6 = Between(line, "service:", "s_port:").Replace(':', ' ').Trim();
                    }
                    #endregion

                    #region accept
                    if (lineArr[6] == "accept")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.SourceName = lineArr[7];
                        }
                        //rec.EventType = Between(line, "proto:", "xlate:");
                        //rec.CustomStr3 = Between(line, "src:", "dst:").Replace(':', ' ').Trim();
                        //rec.CustomStr4 = Between(line, "dst:", "proto:").Replace(':', ' ').Trim();
                        //rec.CustomStr5 = Between(line, "product:", "service:").Replace(':', ' ').Trim();
                        //rec.CustomStr6 = Between(line, "service:", "s_port:").Replace(':', ' ').Trim();

                        subLineArr = line.Split(':');
                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }

                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                              StringSplitOptions.RemoveEmptyEntries);
                                rec.UserName = userNameArr[1];
                                rec.CustomStr1 = userNameArr[0];
                            }
                            if (subLineArr[i].Trim() == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "lineArr: " + lineArr[i] + lineArr);

                            }
                            if (subLineArr[i].Trim() == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region https
                    if (lineArr[6] == "HTTPS")
                    {
                        if (lineArr.Length > 8)
                        {
                            rec.EventCategory = lineArr[6] + " " + lineArr[7];
                        }
                        rec.SourceName = lineArr[8];
                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }

                            //if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            if (subLineArr[i].Trim() == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];

                            }
                            if (subLineArr[i].Trim() == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_category")
                            {
                                rec.CustomStr6 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "matched_category")
                            {
                                rec.CustomStr7 = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region logout
                    if (lineArr[6] == "logout")
                    {
                        if (lineArr.Length > 8)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
                                rec.UserName = userNameArr[1];
                                rec.CustomStr1 = userNameArr[0];
                            }

                            if (subLineArr[i].Trim() == "duration")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region authcrypt
                    if (lineArr[6] == "authcrypt")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];

                        subLineArr = line.Split(':');
                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
                                rec.UserName = userNameArr[1];
                                rec.CustomStr1 = userNameArr[0];
                            }

                            if (subLineArr[i].Trim() == "roles")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }
                            if (subLineArr[i].Trim() == "auth_status")
                            {
                                rec.CustomStr2 = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region block
                    if (lineArr[6] == "block")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
                                rec.UserName = userNameArr[1];
                                rec.CustomStr1 = userNameArr[0];
                            }
                            if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "dst" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_desc")
                            {
                                rec.CustomStr5 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_category")
                            {
                                rec.CustomStr6 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "matched_category")
                            {
                                rec.CustomStr7 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_risk")
                            {
                                rec.CustomStr8 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_rule_name")
                            {
                                rec.CustomStr9 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region drop
                    if (lineArr[6] == "drop")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "dst" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                            }
                        }
                    }

                    #endregion

                    #region drop
                    if (lineArr[6] == "drop")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "dst" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion

                    #region ctl
                    if (lineArr[6] == "ctl")
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventCategory = lineArr[6];
                        }
                        rec.SourceName = lineArr[7];
                        rec.CustomStr10 = Between(line, "resource:", "product:");
                        string[] resourceArr = Between(line, "resource:", "product:").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        rec.CustomStr2 = resourceArr[0] + @"//" + resourceArr[1];

                        for (int i = 0; i < subLineArr.Length; i++)
                        {
                            if (subLineArr[i].Trim() == "src_user_name")
                            {
                                string[] userNameArr = (subLineArr[i + 1]).Split(new char[] { '(', ')' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
                                rec.UserName = userNameArr[1];
                                rec.CustomStr1 = userNameArr[0];
                            }

                            if (subLineArr[i].Trim() == "src" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "src")
                            {
                                rec.CustomStr3 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "dst" || subLineArr[i].Split(' ')[subLineArr[i].Split(' ').Length - 1] == "dst")
                            {
                                rec.CustomStr4 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_desc")
                            {
                                rec.CustomStr5 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_category")
                            {
                                rec.CustomStr6 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "matched_category")
                            {
                                rec.CustomStr7 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_risk")
                            {
                                rec.CustomStr8 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "app_rule_name")
                            {
                                rec.CustomStr9 = subLineArr[i + 1];
                            }

                            if (subLineArr[i].Trim() == "proto")
                            {
                                rec.EventType = subLineArr[i + 1];
                            }
                        }
                    }
                    #endregion


                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------" + line);
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CheckPointSyslogV_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CheckPointSyslogV_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CheckPointSyslogV_1_0_0Recorder").GetValue("Trace Level"));
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
