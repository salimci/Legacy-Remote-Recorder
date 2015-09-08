//Name:Juniper SSL VPN Recorder
//Writer: Ali Yıldırım
//Date: 24/02/2011

using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;


namespace JuniperSSLVPNRecorder
{

    public class JuniperSSLVPNRecorder : CustomBase
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
                                           String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait,
                                           String User,
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
            try
            {
                if (slog != null)
                    slog.Stop();
            }
            finally
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "SetConfigData => Clear Ends");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on JuniperSSLVPNRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on JuniperSSLVPNRecorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SetConfigData => Parse Location [" + location + "]");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing JuniperSSLVPN Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSSLVPNRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\JuniperSSLVPNRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSSLVPNRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public JuniperSSLVPNRecorder()
        {
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            /*
            02-24-2011	10:29:12	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:56 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Network Connect: Session ended for user with IP 192.168.1.192
            02-24-2011	10:29:12	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:56 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Network Connect: ACL count = 0.
            02-24-2011	10:29:12	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:56 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Closed connection to TUN-VPN port 443 after 45 seconds, with 65554 bytes read (in 208 chunks) and 64940 bytes written (in 269 chunks)
            02-24-2011	10:29:08	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:51 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Logout from 192.168.1.1
            02-24-2011	10:28:47	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:30 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Transport mode failed over to SSL for user with NCIP 192.168.1.192
            02-24-2011	10:28:32	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:16 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Key Exchange number 1 occured for user with NCIP 192.168.1.192
            02-24-2011	10:28:28	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:11 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Connected to TUN-VPN port 443
            02-24-2011	10:28:28	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:11 - ive - [192.168.1.1] natekuser(Natek)[natek] - Network Connect: Session started for user with IP 192.168.1.192
            02-24-2011	10:28:28	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:11 - ive - [192.168.1.1] natekuser(Natek)[natek] - Network Connect: ACL count = 16.
            02-24-2011	10:28:24	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:07 - ive - [192.168.1.1] natekuser(Natek)[natek] - *Login succeeded for natekuser/Natek from 192.168.1.1.
            02-24-2011	10:28:24	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:54:07 - ive - [192.168.1.1] natekuser(Natek)[] - *Primary authentication successful for natekuser/System Local from 192.168.1.1
            02-24-2011	10:28:16	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:53:59 - ive - [192.168.1.1] System()[] -* Connection from IP 192.168.1.1 not authenticated yet (URL=/dana-na/auth/lastauthserverused.js)
            02-24-2011	10:28:16	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:53:59 - ive - [192.168.1.1] System()[] -* Connection from IP 192.168.1.1 not authenticated yet (URL=/)
            02-24-2011	10:23:03	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:48:46 - ive - [192.168.1.1] natekuser(Natek)[natek] - Logout from 192.168.1.1
            02-24-2011	10:21:36	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:47:19 - ive - [192.168.1.1] natekuser(Natek)[natek] - Login succeeded for natekuser/Natek from 192.168.1.1.
            02-24-2011	10:21:36	Local0.Info	88.255.50.190	Juniper: 2011-02-24 10:47:19 - ive - [192.168.1.1] natekuser(Natek)[] - Primary authentication successful for natekuser/System Local from 192.168.1.1
            02-24-2011	10:21:19	Local0.Warning	88.255.50.190	Juniper: 2011-02-24 10:47:02 - ive - [192.168.1.1] System()[] - SSL negotiation failed while client at source IP '192.168.1.1' was trying to connect to '192.168.1.190'. Reason: 'tlsv1 alert unknown ca'

            */
            //Real log format
            //88.255.50.190:514 : local0.info Juniper: 2011-02-24 16:59:44 - ive - [192.168.1.1] natek(Natek)[] - Primary authentication failed for natek/System Local from 192.168.1.1

            CustomBase.Rec rec = new CustomBase.Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Log : " + args.Message);

                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Start preparing record");

                rec.LogName = "JuniperSSLVPNRecorder";
                String[] parts = args.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                rec.Description = args.Message; //All Message
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> All Message : " + args.Message);

                rec.SourceName = args.Source;  //Source Name
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Datetime  : " + rec.Datetime);
                try
                {
                    if (parts.Length > 12)
                    {
                        rec.CustomStr1 = parts[0].Split(':')[0]; //Host Name
                        rec.CustomInt1 = Convert.ToInt32(parts[0].Split(':')[1]);//Port
                        //2011-02-24 16:59:44
                        try
                        {
                            string[] tarih = parts[4].Split('-');
                            string date = tarih[0] + "/" + tarih[1] + "/" + tarih[2] + " " + parts[5];
                            rec.Datetime = Convert.ToDateTime(date, CultureInfo.InvariantCulture).ToString("yyyy/MM/dd HH:mm:ss"); //Datetime     
                            L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Datetime1 : " + rec.Datetime);
                        }
                        catch (Exception ex)
                        {
                            rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            L.Log(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() --> DateTime conversion error, please check JuniperSSLVPN log format...");
                            L.Log(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() --> args.Message : " + args.Message);
                        }
                        rec.UserName = parts[10]; //User Name

                        rec.CustomStr10 = "";
                        for (int i = 12; i < parts.Length; i++)
                        {
                            rec.CustomStr10 += parts[i] + " "; //Action

                            if (parts[i] == "with")
                            {
                                if (parts[i + 1] == "IP" || parts[i + 1] == "NCIP")
                                {
                                    rec.CustomStr2 = parts[i + 2]; //User IP
                                }
                            }
                        }
                        rec.CustomStr10 = rec.CustomStr10.Trim();
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Log format is not proper. So it is sent to Description column.");
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> ERROR on parsing...");
                    L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> " + e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> " + e.StackTrace);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.DEBUG, Dal + " " + virtualhost + " " + rec.Description);
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() --> Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() --> " + er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, " slog_SyslogEvent() --> " + args.EventLogEntType + " " + args.Message);
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\JuniperSSLVPNRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSSLVPNRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSSLVPNRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSSLVPNRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager JuniperSSLVPNRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }
}
