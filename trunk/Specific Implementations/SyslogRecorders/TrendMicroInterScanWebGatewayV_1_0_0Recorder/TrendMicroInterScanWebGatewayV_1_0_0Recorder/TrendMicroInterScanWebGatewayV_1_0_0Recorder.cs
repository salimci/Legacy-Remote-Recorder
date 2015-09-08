using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace TrendMicroInterScanWebGatewayV_1_0_0Recorder
{
    public class TrendMicroInterScanWebGatewayV_1_0_0Recorder : CustomBase
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
        public string dateFormat = "yyyy-MM-dd HH:mm:ss";


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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TrendMicroInterScanWebGatewayV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TrendMicroInterScanWebGatewayV_1_0_0Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager TrendMicroInterScanWebGatewayV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\TrendMicroInterScanWebGatewayV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TrendMicroInterScanWebGatewayV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public TrendMicroInterScanWebGatewayV_1_0_0Recorder()
        {
            // TODO: Add any initialization after the InitComponent call          
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "Log Parsing is starte. Line is: " + args.Message);

            string line = args.Message;
            Rec rec = new Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "TrendMicroInterScanWebGatewayV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    string[] lineArr = line.Split(',');
                    string[] lineArrAlternate = SpaceSplit(line, false);

                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "lineArr: " + lineArr[i]);
                    }

                    for (int i = 0; i < lineArrAlternate.Length; i++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "lineArrAlternate: " + lineArr[i]);
                    }

                    rec.EventCategory = lineArrAlternate[2];
                    rec.EventType = Between(lineArr[2], "]", "tk_username", 0);
                    try
                    {
                        DateTime dt = Convert.ToDateTime(lineArr[1]);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Datetime Convert error: " + exception.Message);
                    }

                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr[i].StartsWith("tk_username=") || lineArr[i].Contains("tk_username="))
                        {
                            rec.UserName = After(lineArr[i], "tk_username=");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                        }

                        if (lineArr[i].StartsWith("tk_protocol="))
                        {
                            rec.CustomStr5 = GetValue(lineArr[i], "tk_protocol=");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                        }

                        if (lineArr[i].StartsWith("tk_uid="))
                        {
                            rec.CustomStr9 = GetValue(lineArr[i], "tk_uid=");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                        }

                        if (lineArrAlternate[2] == "local0.info")
                        {
                            if (lineArr[i].StartsWith("tk_server="))
                            {
                                rec.ComputerName = GetValue(lineArr[i], "tk_server=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                            }

                            //10.20.1.122:34970 : local0.info iwsva1.dpt.gov.tr: <Tue, 08 Oct 2013 14:32:16,EEST> [EVT_URL_ACCESS_TRACKING|LOG_INFO] Access tracking log tk_username=10.10.11.39,tk_url=http://haber10.com/images/news/100x75/421378.jpg,tk_size=0,tk_date_field=2013-10-08 14:32:16+0300,tk_protocol=http,tk_mime_content=unknown/unknown,tk_server=iwsva1.dpt.gov.tr,tk_client_ip=10.10.11.39,tk_server_ip=176.53.59.192,tk_domain=haber10.com,tk_path=images/news/100x75/421378.jpg,tk_file_name=421378.jpg,tk_operation=GET,tk_uid=1159564668-d32bfc31cafb9b079c18,tk_category=46,tk_category_type=0

                            if (lineArr[i].StartsWith("tk_operation="))
                            {
                                rec.CustomStr1 = GetValue(lineArr[i], "tk_operation=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                            }

                            if (lineArr[i].StartsWith("tk_client_ip="))
                            {
                                rec.CustomStr3 = GetValue(lineArr[i], "tk_client_ip=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }

                            if (lineArr[i].StartsWith("tk_server_ip="))
                            {
                                rec.CustomStr4 = GetValue(lineArr[i], "tk_server_ip=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }

                            if (lineArr[i].StartsWith("tk_mime_content="))
                            {
                                rec.CustomStr6 = GetValue(lineArr[i], "tk_mime_content=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                            }

                            if (lineArr[i].StartsWith("tk_domain="))
                            {
                                rec.CustomStr7 = GetValue(lineArr[i], "tk_domain=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                            }

                            if (lineArr[i].StartsWith("tk_path="))
                            {
                                rec.CustomStr8 = GetValue(lineArr[i], "tk_path=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                            }

                            if (lineArr[i].StartsWith("tk_url="))
                            {
                                rec.CustomStr10 = GetValue(lineArr[i], "tk_url=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                            }

                            try
                            {
                                if (lineArr[i].StartsWith("tk_size="))
                                {
                                    rec.CustomInt1 = Convert.ToInt32(GetValue(lineArr[i], "tk_size="));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                                }

                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 Type Casting Error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr[i].StartsWith("tk_category="))
                                {
                                    rec.CustomInt2 = Convert.ToInt32(GetValue(lineArr[i], "tk_category="));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 Type Casting Error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr[i].StartsWith("tk_category_type="))
                                {
                                    rec.CustomInt3 = Convert.ToInt32(GetValue(lineArr[i], "tk_category_type="));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 Type Casting Error: " + exception.Message);
                            }
                        }

                        if (lineArrAlternate[2] == "local0.critical")
                        {
                            if (lineArr[i].StartsWith("tk_url="))
                            {
                                rec.CustomStr2 = GetValue(lineArr[i], "tk_url=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                            }

                            if (lineArr[i].StartsWith("tk_scan_type="))
                            {
                                rec.CustomStr6 = GetValue(lineArr[i], "tk_scan_type=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                            }

                            if (lineArr[i].StartsWith("tk_blocked_by="))
                            {
                                rec.CustomStr7 = GetValue(lineArr[i], "tk_blocked_by=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                            }

                            if (lineArr[i].StartsWith("tk_rule_name"))
                            {
                                rec.CustomStr8 = GetValue(lineArr[i], "tk_rule_name=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                            }

                            if (lineArr[i].StartsWith("tk_url="))
                            {
                                string sdf = GetValue(lineArr[i], "tk_url=");
                                string sdfg = Between(sdf, "http://", "/", 0);
                                rec.CustomStr1 = Before(sdfg, "/", 0);
                            }

                            if (lineArr[i].StartsWith("tk_category="))
                            {
                                rec.CustomStr10 = GetValue(lineArr[i], "tk_category=");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                            }

                            try
                            {
                                if (lineArr[i].StartsWith("tk_opp_id="))
                                {
                                    rec.CustomInt5 = Convert.ToInt32(GetValue(lineArr[i], "tk_opp_id="));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                                }

                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5 Type Casting Error: " + exception.Message);
                            }

                            try
                            {
                                if (lineArr[i].StartsWith("tk_filter_action="))
                                {
                                    rec.CustomInt6 = Convert.ToInt32(GetValue(lineArr[i], "tk_filter_action="));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                                }
                            }
                            catch (Exception exception)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 Type Casting Error: " + exception.Message);
                            }
                        }
                    }

                    if (line.Length > 899)
                        rec.Description = line.Substring(0, 899);

                    else
                        rec.Description = line;
                    //
                    rec.Description = rec.Description.Replace("'", "|");

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start sending Data: " + rec.UserName);
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start sending Data: " + rec.UserName);

                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);

                    L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data");

                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
               
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
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



        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a, int type)
        {
            //type = 1 last
            //type = 0 first


            int posA = 0;

            if (type == 1)
            {
                posA = value.LastIndexOf(a, System.StringComparison.Ordinal);
            }

            if (type == 0)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
            }
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eðer line içinde tab boþluk var ise ve buna göre de split yapýlmak isteniyorsa true
        /// eðer line içinde tab boþluk var ise ve buna göre  split yapýlmak istenmiyorsa false
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

        public string GetValue(string line, string value)
        {
            string returnValue = "";
            try
            {
                if (line.StartsWith(value))
                {
                    returnValue = After(line, value);
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "GetValue()" + exception.ToString());
            }
            return returnValue;
        } // GetValue

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
        } // After

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\TrendMicroInterScanWebGatewayV_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TrendMicroInterScanWebGatewayV_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TrendMicroInterScanWebGatewayV_1_0_0Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TrendMicroInterScanWebGatewayV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager TrendMicroInterScanWebGatewayV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
