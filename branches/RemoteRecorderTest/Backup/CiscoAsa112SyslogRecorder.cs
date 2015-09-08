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

namespace CiscoAsa112SyslogRecorder
{
    public class CiscoAsa112SyslogRecorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoAsaSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoAsaSyslog Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CiscoAsaSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing CiscoAsaSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoAsaSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoAsa112SyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoAsaSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public CiscoAsa112SyslogRecorder()
        {
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                rec.LogName = "CiscoAsaSyslog Recorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;

                //10.10.0.254:514 : local4.info %ASA-6-305011: Built dynamic TCP translation from Inside:192.168.111.10/56298 to Outside(Inside_nat_outbound):212.156.67.62/12694

                String[] parts = args.Message.Split('%')[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " Message is null " + args.Message);
                    return;
                }

                if (parts.Length < 2)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " Message is not in proper format. Log : " + args.Message);
                    return;
                }


                string type = parts[0].Split('-')[2].TrimEnd(':');


                //Uncommon fields for all pix records. Now Parse with id
                rec.EventId = Convert.ToInt64(type);
                switch (type)
                {
                    case "106001"://Untested
                        {

                        } break;

                    //10.10.0.254:514 : local4.info %ASA-6-106015: Deny TCP (no connection) from 172.16.100.142/53916 to 83.66.140.10/80 flags RST  on interface Fabrikalar
                    case "106015":
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!parts[i].Contains("("))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                bool getRest = false;
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].ToLower().Equals("from"))
                                    {
                                        rec.CustomStr3 = parts[i + 1].Split(new char[] { '/' })[0];
                                        rec.CustomInt2 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[1]);
                                    }
                                    else if (parts[i].ToLower().Equals("to"))
                                    {
                                        rec.CustomStr4 = parts[i + 1].Split(new char[] { '/' })[0];
                                        rec.CustomInt3 = Convert_To_Int32(parts[i + 1].Split(new char[] { '/' })[1]);
                                        i++;
                                        getRest = true;
                                    }
                                    else if (getRest)
                                    {
                                        rec.CustomStr7 += parts[i] + " ";
                                    }
                                    rec.CustomStr7 = rec.CustomStr7.Trim();
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 106015. Log : " + args.Message);
                            }
                        } break;

                    //10.10.0.254:514 : local4.info %ASA-6-302020: Built outbound ICMP connection for faddr 74.55.143.146/0 gaddr 212.156.67.62/5157 laddr 172.16.140.77/512
                    //10.10.0.254:514 : local4.info %ASA-6-302021: Teardown ICMP connection for faddr 172.16.204.66/0 gaddr 10.10.0.2/0 laddr 10.10.0.2/0
                    case "302020":
                    case "302021":
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!parts[i].Contains("for"))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                for (int i = 1; i < parts.Length; i++)
                                {

                                    if (parts[i].ToLower().Equals("faddr"))
                                    {
                                        rec.CustomStr3 = parts[i + 1].Split(new char[] { '/' })[0];
                                        rec.CustomInt2 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[1]);
                                    }
                                    else if (parts[i].ToLower().Equals("gaddr"))
                                    {
                                        rec.CustomStr4 = parts[i + 1].Split(new char[] { '/' })[0];
                                        rec.CustomInt3 = Convert_To_Int32(parts[i + 1].Split(new char[] { '/' })[1]);
                                    }
                                    else if (parts[i].ToLower().Equals("laddr"))
                                    {
                                        rec.CustomStr5 = parts[i + 1].Split(new char[] { '/' })[0];
                                        rec.CustomInt4 = Convert_To_Int32(parts[i + 1].Split(new char[] { '/' })[1]);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 302020, 302021. Log : " + args.Message);
                            }

                        } break;

                    //10.10.0.254:514 : local4.alert %ASA-1-106021: Deny TCP reverse path check from 192.168.34.73 to 212.156.67.62 on interface Outside
                    case "106021":
                        {
                            rec.CustomStr2 = "";

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (!parts[i].Contains("from"))
                                    rec.CustomStr2 += parts[i] + " ";
                                else
                                    break;
                            }

                            bool continueStr2 = false;

                            rec.CustomStr8 = "";

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (parts[i].Equals("from"))
                                {
                                    rec.CustomStr3 = parts[i + 1];
                                }
                                else if (parts[i].ToLower().Equals("to"))
                                {
                                    rec.CustomStr4 = parts[i + 1];
                                }

                                if (parts[i].Contains("on"))
                                {
                                    continueStr2 = true;
                                }

                                if (continueStr2)
                                {
                                    rec.CustomStr2 += " " + parts[i];
                                }
                            }

                            rec.CustomStr2 = rec.CustomStr2.Trim();
                            rec.EventCategory = parts[1] + " " + parts[2];

                        }

                        break;
                    case "106006":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 106006. Log : " + args.Message);
                        }
                        break;
                    case "106007":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 106007. Log : " + args.Message);
                        }
                        break;

                    case "106017":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 106017. Log : " + args.Message);
                        }
                        break;

                    //10.10.0.254:514 : local4.error %ASA-3-710003: TCP access denied by ACL from 88.249.67.204/2305 to Outside:212.156.67.62/23

                    case "710003":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 710003. Log : " + args.Message);
                        }
                        break;


                    //10.10.0.254:514 : local4.warning %ASA-4-106023: Deny udp src Outside:24.101.147.41/19971 dst Fabrikalar:212.156.67.62/39772 by access-group "Outside_access_in" [0x0, 0x0]
                    case "106016":
                    case "106014":
                    case "106023":
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!parts[i].Contains(":"))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].Contains(":"))
                                    {
                                        if (parts[i].ToLower().Contains("inside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr3 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt2 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                        else if (parts[i].ToLower().Contains("outside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr4 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt3 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                        else
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr3 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt2 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                    }
                                    else
                                    {
                                        rec.CustomStr8 += parts[i] + " ";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 106016, 106014, 106023. Log : " + args.Message);
                            }

                        }
                        break;
                    case "715001":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 715001. Log : " + args.Message);
                        }
                        break;
                    case "305009":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 305009. Log : " + args.Message);
                        }
                        break;


                    //10.10.0.254:514 : local4.info %ASA-6-302015: Built inbound UDP connection 53527882 for Fabrikalar:172.16.100.73/1025 (172.16.100.73/1025) to Inside:10.30.0.7/53 (10.30.0.7/53)
                    //10.10.0.254:514 : local4.info %ASA-6-302013: Built outbound TCP connection 53527880 for Outside:212.174.187.34/80 (212.174.187.34/80) to Inside:192.168.115.13/50417 (212.156.67.62/47279)
                    case "302015":
                    case "302013":
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                long sayi;
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!Int64.TryParse(parts[i], out sayi))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        rec.CustomStr1 = parts[i];
                                        break;
                                    }
                                }

                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                bool ilkIpAlindi = false;
                                bool ilkParantezIpAlindi = false;

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].Contains(":") && parts[i].Contains("/"))
                                    {
                                        if (!ilkIpAlindi)
                                        {
                                            ilkIpAlindi = true;
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr3 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt2 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                        else if (ilkIpAlindi)
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr4 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt3 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                    }
                                    else if (parts[i].Contains("(") && parts[i].Contains("/"))
                                    {
                                        if (!ilkParantezIpAlindi)
                                        {
                                            ilkParantezIpAlindi = true;
                                            rec.CustomStr5 = parts[i].Split(new char[] { '/' })[0].TrimStart('(');
                                            rec.CustomInt4 = Convert_To_Int32(parts[i].Split(new char[] { '/' })[1].TrimEnd(')'));
                                        }
                                        else if (ilkParantezIpAlindi)
                                        {
                                            rec.CustomStr6 = parts[i].Split(new char[] { '/' })[0].TrimStart('(');
                                            rec.CustomInt5 = Convert_To_Int32(parts[i].Split(new char[] { '/' })[1].TrimEnd(')'));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 305010, 302016, 302014. Log : " + args.Message);
                            }

                        }
                        break;

                    //10.10.0.254:514 : local4.info %ASA-6-302016: Teardown UDP connection 53527868 for Outside:216.239.36.10/53 to Inside:10.30.0.7/52945 duration 0:00:00 bytes 193
                    //10.10.0.254:514 : local4.info %ASA-6-302014: Teardown TCP connection 53527230 for Outside:92.45.106.106/80 to Fabrikalar:172.16.194.52/3473 duration 0:00:05 bytes 3920 TCP FINs
                    case "305010":
                    case "302016":
                    case "302014":
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";
                                long sayi;
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!Int64.TryParse(parts[i], out sayi))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        rec.CustomStr1 = parts[i];
                                        break;
                                    }
                                }
                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].Contains(":"))
                                    {
                                        if (parts[i].ToLower().Contains("inside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr3 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt2 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                        else if (parts[i].ToLower().Contains("outside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr4 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt3 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                    }
                                    else if (parts[i].Contains("duration"))
                                    {
                                        rec.CustomStr8 = parts[i + 1];
                                    }
                                    else if (parts[i].Contains("bytes"))
                                    {
                                        rec.CustomInt7 = Convert_To_Int32(parts[i + 1]);
                                    }
                                    else if (parts[i].Contains("TCP"))
                                    {
                                        rec.CustomStr7 = parts[i] + " " + parts[i + 1];
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 305010, 302016, 302014. Log : " + args.Message);
                            }
                        }
                        break;

                    case "609001":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 609001. Log : " + args.Message);

                        } break;

                    case "609002":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 609002. Log : " + args.Message);

                        } break;

                    //10.10.0.254:514 : local4.info %ASA-6-305011: Built dynamic TCP translation from Inside:192.168.111.10/56298 to Outside(Inside_nat_outbound):212.156.67.62/12694
                    //10.10.0.254:514 : local4.info %ASA-6-305012: Teardown dynamic TCP translation from Fabrikalar:172.16.211.108/2599 to Outside(Fabrikalar_nat_outbound):212.156.67.62/13200 duration 0:00:30
                    case "305011"://Tested
                    case "305012"://Tested
                        {
                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!parts[i].Contains(":"))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].Contains(":"))
                                    {
                                        if (parts[i].ToLower().Contains("inside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr3 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt2 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                        else if (parts[i].ToLower().Contains("outside"))
                                        {
                                            rec.CustomStr7 += parts[i].Split(new char[] { ':', '/' })[0] + " ";
                                            rec.CustomStr4 = parts[i].Split(new char[] { ':', '/' })[1];
                                            rec.CustomInt3 = Convert_To_Int32(parts[i].Split(new char[] { ':', '/' })[2]);
                                        }
                                    }
                                    else if (parts[i].Contains("duration"))
                                    {
                                        rec.CustomStr8 = parts[i + 1];
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 305011, 305012. Log : " + args.Message);
                            }

                        } break;

                    //10.10.0.254:514 : local4.notice %ASA-5-304001: 172.16.120.166 Accessed URL 209.85.149.106:/news/tbn/UpaJuRRf32EJ
                    case "304001":
                        {
                            rec.CustomStr3 = parts[1];
                            rec.CustomStr2 = "";

                            for (int i = 2; i < parts.Length; i++)
                            {
                                if (!parts[i].Contains(":"))
                                {
                                    rec.CustomStr2 += parts[i] + " ";
                                }
                                else
                                {
                                    rec.CustomStr4 = parts[i].Split('/')[0].TrimEnd(':');
                                    rec.CustomStr9 = parts[i];
                                }
                            }

                            rec.CustomStr2 = rec.CustomStr2.Trim();
                            rec.EventCategory = rec.CustomStr2;

                        } break;

                    case "419001":
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Boş içi. Hazırlanması lazım : 419001. Log : " + args.Message);
                        }
                        break;

                    //10.10.0.254:514 : local4.warning %ASA-4-419002: Duplicate TCP SYN from Inside:172.16.231.99/2268 to Inside:192.168.101.7/9100 with different initial sequence number
                    case "419002":
                        {

                            try
                            {
                                rec.CustomStr2 = "";
                                rec.CustomStr7 = "";

                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (!parts[i].Contains("from"))
                                    {
                                        rec.CustomStr2 += parts[i] + " ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                rec.CustomStr2 = rec.CustomStr2.Trim();
                                rec.EventCategory = rec.CustomStr2;

                                bool getRest = false;
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    if (parts[i].Equals("from"))
                                    {

                                        rec.CustomStr3 = parts[i + 1].Split(new char[] { ':', '/' })[1];
                                        rec.CustomInt2 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[2]);
                                    }
                                    else if (parts[i].ToLower().Equals("to"))
                                    {
                                        rec.CustomStr4 = parts[i + 1].Split(new char[] { ':', '/' })[1];
                                        rec.CustomInt3 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[2]);
                                        getRest = true;
                                        i++;
                                    }
                                    else if (getRest)
                                    {
                                        rec.CustomStr7 += parts[i] + " ";
                                    }
                                    rec.CustomStr7 = rec.CustomStr7.Trim();
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error On : 419002. Log : " + args.Message);
                            }
                        }
                        break;
                    //10.10.0.254:514 : local4.warning %ASA-4-733100: [ Scanning] drop rate-1 exceeded. Current burst rate is 49 per second, max configured rate is 10; Current average rate is 63 per second, max configured rate is 5; Cumulative total count is 38337
                    case "733100":
                        {
                            rec.CustomStr2 = parts[2].TrimEnd(']');

                            rec.CustomStr8 = "";
                            for (int i = 3; i < parts.Length; i++)
                            {
                                rec.CustomStr8 += parts[i] + " ";
                            }

                            rec.CustomStr8 = rec.CustomStr8.Trim();
                            rec.CustomStr2 = rec.CustomStr2.Trim();
                            rec.EventCategory = rec.CustomStr2;

                        } break;

                    //10.10.0.254:514 : local4.error %ASA-3-313001: Denied ICMP type=3, code=1 from 195.140.196.2 on interface Outside
                    case "313001":
                        {
                            rec.CustomStr2 = "";
                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (!parts[i].Contains("="))
                                    rec.CustomStr2 += parts[i] + " ";
                                else
                                    break;
                            }

                            rec.CustomStr2 = rec.CustomStr2.Trim();
                            rec.EventCategory = rec.CustomStr2;

                            bool continueStr2 = false;
                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (parts[i].Contains("from"))
                                {
                                    rec.CustomStr3 = parts[i + 1];

                                }
                                if (parts[i].Contains("on"))
                                {
                                    continueStr2 = true;
                                }
                                if (continueStr2)
                                {
                                    rec.CustomStr2 += " " + parts[i];
                                }
                            }

                            rec.CustomStr2 = rec.CustomStr2.Trim();

                        } break;

                    //10.10.0.254:514 : local4.warning %ASA-4-313005: No matching connection for ICMP error message: icmp src Fabrikalar:172.16.210.109 dst Fabrikalar:11.11.11.33 (type 3, code 3) on Fabrikalar interface.  Original IP payload: udp src 11.11.11.33/58505 dst 224.0.0.252/5355.
                    //parts=ASA-4-313005: No matching connection for ICMP error message: icmp src Fabrikalar:172.16.210.109 dst Fabrikalar:11.11.11.33 (type 3, code 3) on Fabrikalar interface.  Original IP payload: udp src 11.11.11.33/58505 dst 224.0.0.252/5355.
                    case "313005":
                        {
                            rec.EventId = 313005;
                            rec.EventType = parts[5] + " " + parts[6];
                            rec.EventCategory = parts[1] + " " + parts[2] + " " + parts[3];
                            rec.SourceName = args.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
                            rec.CustomStr3 = parts[10].Split(new char[] { ':' })[1];
                            rec.CustomStr4 = parts[12].Split(new char[] { ':' })[1];
                            rec.CustomStr7 = parts[18] + " " + parts[19];
                            rec.CustomStr8 = parts[20] + " " + parts[21] + " " + parts[22];
                            rec.CustomStr5 = parts[25].Split(new char[] { '/' })[0];
                            rec.CustomStr6 = parts[27].Split(new char[] { '/' })[0];
                            rec.CustomInt2 = Convert_To_Int32(parts[25].Split(new char[] { '/' })[1]);
                            rec.CustomInt3 = Convert_To_Int32(parts[27].Split(new char[] { '/' })[1]);
                        } break;

                    //10.10.0.254:514 : local4.error %ASA-3-305006: portmap translation creation failed for icmp src Inside:192.168.125.15 dst Inside:192.168.2.200 (type 8, code 0)
                    //parts=ASA-3-305006: portmap translation creation failed for icmp src Inside:192.168.125.15 dst Inside:192.168.2.200 (type 8, code 0)
                    case "305006":
                        {
                            rec.EventId = 3005006;
                            rec.EventType = "";
                            rec.EventCategory = parts[1] + " " + parts[2] + " " + parts[3] + " " + parts[4];
                            rec.SourceName = args.Message.Split(new char[] { ' ' })[2];
                            rec.CustomStr3 = parts[8].Split(new char[] { ':' })[1];
                            rec.CustomStr4 = parts[10].Split(new char[] { ':' })[1];

                        } break;
                    //10.10.0.254:514 : local4.warning %ASA-4-410001: Dropped UDP DNS request from Fabrikalar:172.16.204.234/4521 to Outside:193.2.122.51/53; label length 154 bytes exceeds protocol limit of 63 bytes
                    case "410001":
                        {
                            rec.CustomStr2 = "";

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (!parts[i].Contains("from"))
                                    rec.CustomStr2 += parts[i] + " ";
                                else
                                    break;
                            }

                            bool getRest = false;
                            bool firstByteGot = false;

                            rec.CustomStr8 = "";

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (parts[i].Equals("from"))
                                {
                                    rec.CustomStr5 = parts[i + 1].Split(new char[] { ':', '/' })[0];
                                    rec.CustomStr3 = parts[i + 1].Split(new char[] { ':', '/' })[1];
                                    rec.CustomInt2 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[2]);
                                }
                                else if (parts[i].ToLower().Equals("to"))
                                {
                                    rec.CustomStr6 = parts[i + 1].Split(new char[] { ':', '/' })[1];
                                    rec.CustomStr4 = parts[i + 1].Split(new char[] { ':', '/' })[1].TrimEnd(':');
                                    rec.CustomInt3 = Convert_To_Int32(parts[i + 1].Split(new char[] { ':', '/' })[2]);
                                    getRest = true;
                                    i++;
                                    continue;
                                }

                                if (getRest)
                                {
                                    rec.CustomStr8 += parts[i] + " ";
                                }

                                if (parts[i].Equals("bytes"))
                                {
                                    if (firstByteGot)
                                    {
                                        rec.CustomInt6 = Convert_To_Int32(parts[i - 1]);
                                    }
                                    else
                                    {
                                        rec.CustomInt5 = Convert_To_Int32(parts[i - 1]);
                                        firstByteGot = true;
                                    }
                                }
                            }

                            rec.CustomStr8 = rec.CustomStr8.Trim();
                            rec.CustomStr2 = rec.CustomStr2.Trim();
                            rec.EventCategory = rec.CustomStr2;

                        } break;

                    default:
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Event tanımlanmamış. Event ID : " + type + " , Log : " + args.Message);
                        }
                        break;
                }

                rec.SourceName = args.Source;

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
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, args.Message);
            }
        }
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoAsa112SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoAsa112SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoAsa112SyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoAsa112SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CiscoAsaSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
        private int Convert_To_Int32(string input)
        {
            int output = 0;
            try
            {
                output = Convert.ToInt32(input);
            }
            catch (Exception ex)
            {
                output = 0;
            }
            return output;
        }

    }
}
