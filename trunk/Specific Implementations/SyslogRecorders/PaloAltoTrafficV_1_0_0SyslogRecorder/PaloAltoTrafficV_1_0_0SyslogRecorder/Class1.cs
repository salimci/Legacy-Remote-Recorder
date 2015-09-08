//PaloAltoTrafficV_1_0_0SyslogRecorder
//Writer : Onur Sarıkaya

using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PaloAltoTrafficV_1_0_0SyslogRecorder
{
    public class PaloAltoTrafficV_1_0_0SyslogRecorder : CustomBase
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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
        }

        //public override void Clear()
        //{
        //    if (slog != null)
        //        slog.Stop();
        //}

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on PaloAltoTrafficV_1_0_0SyslogRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on PaloAltoTrafficV_1_0_0SyslogRecorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager PaloAltoTrafficV_1_0_0SyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\PaloAltoTrafficV_1_0_0SyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager PaloAltoTrafficV_1_0_0SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public PaloAltoTrafficV_1_0_0SyslogRecorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = ParseSpecific(args.Message, false, args);
            L.Log(LogType.FILE, LogLevel.INFORM, "Start preparing record");
            L.Log(LogType.FILE, LogLevel.INFORM, "Start sending Data");
            try
            {
               
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    try
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        s.SetReg(Id, rec.Datetime, rec.Description, "", "", rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Data sending error." + exception.Message);
                    }
                }
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

        public Rec ParseSpecific(String line, bool dontSend, LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);
            if (string.IsNullOrEmpty(line))
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Line is Null Or Empty. ");

            }
            CustomBase.Rec rec = new CustomBase.Rec();

            L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
            rec.SourceName = args.Source;
            L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

            rec.LogName = "PaloAltoTrafficV_1_0_0Syslog Recorder";
            //rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            rec.EventType = args.EventLogEntType.ToString();

            if (!string.IsNullOrEmpty(remote_host))
            {
                rec.ComputerName = remote_host;
            }

            rec.Description = args.Message;

            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | ComputerName: " + rec.ComputerName);
            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Description: " + rec.Description);
            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | LogName: " + rec.LogName);
            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | SourceName: " + rec.SourceName);

            //if (rec.Description.Length > 899)
            //{
            //    rec.Description = rec.Description.Substring(0, 899);
            //}
            //else
            //{
            //    rec.Description = rec.Description;
            //}

            //rec.Description = args.Message.Replace("'", "|");



            if (!dontSend)
            {
                //                                                                                                                                                              10                                                                                                                                                              20                                                                                                                                      30
                //threath     Domain*,Receive Time*,Serial #*,Type*,Threat/Content Type*,Config Version*,Generate Time*,Source address*,Destination address*,NAT Source IP*,NAT Destination IP*,Rule*,Source User*,Destination User*,Application*,Virtual System*,Source Zone*,Destination Zone*,Inbound Interface*, Outbound Interface*, Log Action*,Time Logged*,Session ID*,Repeat Count*,Source Port*,Destination Port*,NAT Source Port*,NAT Destination Port*,Flags*,IP Protocol*,Action,URL,Threat/Content Name,Category,Severity,Direction
                //traffic     Domain*,Receive Time*,Serial #*,Type*,Threat/Content Type*,Config Version*,Generate Time*,Source address*,Destination address*,NAT Source IP*,       NAT Destination IP*,Rule*,Source User*,Destination User*,Application*,Virtual System*,Source Zone*,Destination Zone*,Inbound Interface*,Outbound Interface*,          Log Action*,Time Logged*,Session ID*,Repeat Count*,Source Port*,Destination Port*,NAT Source Port*,NAT Destination Port*,Flags*,IP Protocol*,      Action,Bytes,Bytes Sent,Bytes Received,Packets,Start Time,Elapsed Time (sec),Category,Padding(39)
                //1,2011/01/25 05:45:17,0004C100832,THREAT,vulnerability,2,2011/01/25 05:45:12,193.189.142.32,168.216.29.89,192.168.0.12,168.216.29.89,Dis_Web_Server_erisim,,,web-browsing,vsys1,DMZ,Internet,ethernet1/1,ethernet1/4,,2011/01/25 05:45:17,56500,1,80,4149,80,4149,0x40,tcp,alert,,HTTP Non RFC-Compliant Response Found(32880),any,informational,server-to-client



                string[] parts = line.Split(',');

                try
                {
                    try
                    {
                        rec.Datetime = Convert.ToDateTime(parts[6]).ToString("yyyy-MM-dd HH:mm:ss");//Date time conversion requeired.
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | There is a problem converting to date.  date : " + parts[4]);
                    }

                    for (int i = 0; i < parts.Length; i++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() Parts[" + i + "]: " + parts[i]);
                    }
                    try
                    {

                        rec.CustomStr1 = parts[18];
                        rec.CustomStr2 = parts[19];
                        rec.CustomStr3 = parts[7];
                        rec.CustomStr4 = parts[8];
                        rec.CustomStr5 = parts[9];
                        rec.CustomStr6 = parts[10];
                        rec.CustomStr7 = parts[29];
                        rec.CustomStr8 = parts[4];
                        /*rec.CustomStr9 = parts[11];*/
                        rec.CustomStr9 = parts[3];
                        rec.CustomStr10 = parts[14];

                        rec.UserName = parts[12];
                        rec.EventType = parts[30];
                        rec.EventCategory = parts[37];

                        rec.CustomInt1 = Convert_to_Int32(parts[0]);
                        rec.CustomInt2 = Convert_to_Int32(parts[23]);
                        rec.CustomInt3 = Convert_to_Int32(parts[24]);
                        rec.CustomInt4 = Convert_to_Int32(parts[25]);
                        rec.CustomInt5 = Convert_to_Int32(parts[26]);
                        rec.CustomInt6 = Convert_to_Int32(parts[27]);
                        rec.CustomInt7 = Convert_to_Int32(parts[22]);
                        /*rec.CustomInt9 = Convert_to_Int32(parts[32]);*/
                        rec.CustomInt8 = Convert_to_Int32(parts[32]);
                        rec.CustomInt9 = Convert_to_Int32(parts[33]);
                        rec.CustomInt10 = Convert_to_Int32(parts[36]);
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " ParseSpecific() | There is a problem parsing log.: " + ex.Message);
                    }
                    //172.16.55.55:34062 : local7.info Dec 14 11:15:31 1,2012/12/14 11:15:31,002201000312,THREAT,url,1,2012/12/14 11:15:31,10.104.3.241,2.21.90.227,194.27.49.141,2.21.90.227,TR-2-UNT,,,web-browsing,vsys1,trust,untrust,ethernet1/14,ethernet1/15,au_log_profile,2012/12/14 11:15:30,1013217,1,3868,80,34277,80,0x408000,tcp,alert,"px.owneriq.net/ep?sid[]=302333068&sid[]=302334368&rid[]=1612783&rid[]=1612784",(9999),business-and-economy,informational,client-to-server,1652635554,0x0,10.0.0.0-10.255.255.255,European Union,0,text/html

                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.StackTrace);
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                }
            }
            return rec;
        }

        private int Convert_to_Int32(string value)
        {
            int sayi = 0;
            try
            {
                sayi = Convert.ToInt32(value);
                return sayi;
            }
            catch (Exception ex)
            {

                return 0;
            }
        } // Convert_to_Int32

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\PaloAltoTrafficV_1_0_0SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("PaloAltoTrafficV_1_0_0SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("PaloAltoTrafficV_1_0_0SyslogRecorder").GetValue("Trace Level"));
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
                EventLog.WriteEntry("Security Manager PaloAltoTrafficV_1_0_0SyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
