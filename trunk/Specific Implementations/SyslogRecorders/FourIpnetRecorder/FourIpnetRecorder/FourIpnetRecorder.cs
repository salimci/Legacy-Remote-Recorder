//Name: Four Ipnet Recorder
//Writer: Ali Yıldırım
//Date: 17/02/2011

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

namespace FourIpnetRecorder
{
    public class FourIpnetRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = false;
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

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on FourIpnet Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on FourIpnet Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SymantecSmsSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing FourIpnet Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FourIpnet Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\FourIpnetRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FourIpnet Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public FourIpnetRecorder()
        {
        }

        public String[] SpaceSplit(String line)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ')
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
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " slog_SyslogEvent() -->> Message is null.");
                    return;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() -->> Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() -->> Line: " + args.Message);

                //2/16/2011 10:37:50 Local0.Notice 92.45.16.140 http: 192.168.100.55 - - [16/Feb/2011:10:42:28 +0200] "GET http://facebook.com/ HTTP/1.1" 200 - "-" "SAMSUNG-SGH-i900/1.0 (compatible; MSIE 6.0; Windows CE; PPC) Opera 9.5"
                //2/16/2011 10:39:06 Local7.Info 92.45.16.140 session:  [New]test@local TCP MAC=00:19:d2:59:8b:99 SIP=192.168.100.81 SPort=61620 DIP=10.0.0.47 DPort=8014
                //2/16/2011 10:39:11 User.Notice 92.45.16.140 User.Login: User login Username=test@local ,IP=192.168.100.55 ,MAC=00:0B:6C:A9:87:C4
                
                //22 Nisan
                //92.45.16.140:1024 : local0.notice  http: 192.168.100.170 - - [22/Apr/2011:14:00:58 +0300] "GET http://192.168.200.37/loginpages/dns.shtml?session=vroROdk1Dc0VHfTJCdUR/dktBa3GypEGSdU5Bh06Thk5EeU5Gek6XfTqFrnG2gkRFcEZD069&ReturnUrl=sublIwX8DfKF9x6oCx6R4sqtDv6oCsKRBfE035 HTTP/1.1" 200 - "-" "Mozilla/5.0 (Linux; U; Android 2.1-update1; tr-tr; Vodafone 945 Build/VF945_V02e_TR) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/53
                //92.45.16.140:1024 : local7.info session:  [New]N.A. TCP MAC=00:23:12:e7:13:8f SIP=192.168.100.87 SPort=51256 DIP=74.125.43.109 DPort=993
                //92.45.16.140:1024 : local7.info session:  [New]N.A. TCP MAC=00:23:12:e7:13:8f SIP=192.168.100.87 SPort=51304 DIP=188.132.208.154 DPort=80
                
                string[] parts = args.Message.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                rec.LogName = "FourIpnetRecorder";
                rec.SourceName = args.Source;
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;
                rec.Datetime = DateTime.Now.ToString();

                try
                {
                    if (parts.Length > 4)
                    {
                        //rec.Datetime = Convert.ToDateTime(parts[0] + " " + parts[1]).ToString("yyyy/MM/dd HH:mm:ss");
                        rec.EventCategory = parts[3].TrimEnd(':');
                        rec.ComputerName = parts[4];
                        L.LogTimed(LogType.FILE, LogLevel.INFORM, " slog_SyslogEvent() -->> Evet category is " + rec.EventCategory +", computer name is " + rec.ComputerName);

                        if (rec.EventCategory == "http")
                        {
                            rec.CustomStr3 = parts[5];

                            try
                            {
                                string[] tarih = parts[8].TrimStart('[').Split(':');
                                rec.CustomStr7 = tarih[0] + " " + tarih[1] + ":" + tarih[2] + ":" + tarih[3]; //date will be parsed
                            }
                            catch (Exception ex)
                            { }
                            string[] line = args.Message.Split('"');
                            rec.CustomStr4 = line[1];
                            rec.CustomStr5 = line[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            rec.CustomStr6 = line[line.Length - 3].Trim();
                        }
                        else if (rec.EventCategory == "session")
                        {
                            if (parts.Length > 11)
                            {
                                rec.CustomStr2 = parts[5].Split(']')[1];
                                rec.CustomStr7 = parts[6];
                                rec.CustomStr8 = parts[7].Split('=')[1];
                                rec.CustomStr3 = parts[8].Split('=')[1];
                                rec.CustomStr4 = parts[9].Split('=')[1];
                                rec.CustomStr5 = parts[10].Split('=')[1];
                                rec.CustomStr6 = parts[11].Split('=')[1];
                            }
                        }
                        else if (rec.EventCategory == "User.Login")
                        {
                            for (int i = 5; i < parts.Length; i++)
                            {
                                if (parts[i].Contains("Username"))
                                {
                                    rec.CustomStr2 = parts[i].Split('=')[1];
                                }
                                else if (parts[i].Contains("IP"))
                                {
                                    rec.CustomStr3 = parts[i].Split('=')[1];
                                }
                                else if (parts[i].Contains("MAC"))
                                {
                                    rec.CustomStr8 = parts[i].Split('=')[1];
                                }
                            }
                        }
                        else
                        {
                            L.LogTimed(LogType.FILE, LogLevel.INFORM, " slog_SyslogEvent() -->> This event type is not defined. Please contact with Developer : Line : " + args.Message);
                        }
                    }
                    else
                    {
                        L.LogTimed(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() -->> Line format is not like we want. Line : " + args.Message);
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, ex.ToString());
                    L.Log(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() -->> Error line written in description.");
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() -->> Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() -->> Start sending Data");
                
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

                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() -->> Finish Sending Data");
            }
            catch (Exception ex)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() -->> Hata : " + ex.ToString());
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\FourIpnetRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("FourIpnetRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("FourIpnetRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FourIpnetRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SymantecSmsSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }


}
