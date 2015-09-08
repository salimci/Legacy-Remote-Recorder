//Name: Symantec Brightmail Recorder
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

namespace SymantecBrightmailRecorder
{
    public class SymantecBrightmailRecorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecBrightmail Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecBrightmail Recorder functions may not be running");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SymantecBrightmail Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecBrightmail Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SymantecBrightmailRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecBrightmail Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SymantecBrightmailRecorder()
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
                    L.Log(LogType.FILE, LogLevel.INFORM, "Message is null.");
                    return;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                
                
                //2011-01-03 13:27:05	Local1.Info	192.168.2.80	Jan  3 13:28:08 brightmail ecelerity: 1294054057|c0a80250-b7b7aae000007fcf-ca-4d21b2a946ae|ACCEPT|209.85.216.191:42532
                //192.168.2.80:45924 : local1.info Feb 17 14:28:25 brightmail ecelerity: 1297945705|c0a80250-b7b6bae000000e0e-f8-4d5d1460c093|DELIVERY_FAILURE|550 5.4.4 [internal] null mx domain does not accept mail|yenitur@yaho.com
                //192.168.2.80:45924 : local1.info Feb 17 14:24:12 brightmail ecelerity: 1297945452|c0a80250-b7b6bae000000e0e-a0-4d5d136b012b|ORCPTS|kocaelipasaport@egm.gov.tr

                //*192.168.2.80:48626 : local1.info Mar  1 16:51:34 brightmail ecelerity: 1298991094|c0a80250-b7b8aae000000cca-18-4d65f5d052d6|DELIVERY_FAILURE|554 5.4.7 [internal] exceeded max time without delivery|bl142@bbsb.gov.tr  
                //*192.168.2.80:48626 : local1.info Mar  1 16:53:35 brightmail bmserver: 1298991215|c0a80250-b7ce9ae000000cc1-bf-4d6d086e8648|MSGID| <64fa6ea0ddb3a8489f693babbb8e1c1e03eed8254075@dcsrv.kozel.gov>
                //*192.168.2.80:48626 : local1.info Mar  1 16:48:42 brightmail bmserver: 1298990922|c0a80250-b7ce9ae000000cc1-5e-4d6d072bac0f|ATTACH|image001.jpg|kalite y??netimi ve saha i??nceleme raporu no.12.pdf 
                //*192.168.2.80:55252 : local1.info Feb 23 14:03:48 brightmail bmserver: 1298462628|c0a80250-b7b8aae000000cca-87-4d64f73e0192|ATTACHFILTER|_bbg.exe  

                //192.168.2.80:46689 : security2.info Feb 18 10:57:05 brightmail xinetd[2225]: START: https pid=6620 from=192.168.111.66
                //192.168.2.80:54229 : security2.info Feb 18 11:58:50 brightmail xinetd[2225]: START: https pid=13836 from=192.168.111.66
               

                //192.168.2.80:47547 : local1.info May  5 12:05:33 brightmail ecelerity: 1304586333|c0a80250-b7cb8ae000003006-fb-4dc2681d68ec|DELIVERY_FAILURE|554 5.4.4 [internal] domain lookup failed|263086@polnet.inta
                string[] parts = args.Message.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                rec.LogName = "SymantecBrightmailRecorder";
                rec.SourceName = args.Source;
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                try
                {
                    if (parts.Length > 8)
                    {
                        rec.CustomStr6 = parts[0].Split(':')[1];
                        rec.CustomStr7 = parts[0].Split(':')[0];// 1[1]
                        rec.CustomStr8 = parts[3] + " " + parts[4] + " " + parts[5];
                        rec.CustomStr9 = parts[7].TrimEnd(':');//7

                        if (parts[8].Contains("|"))
                        {
                            string[] pipes = parts[8].Split('|');//8

                            rec.CustomInt1 = Convert_To_Int32(pipes[0]);
                            rec.EventCategory = "";
                            try
                            {
                                rec.CustomStr10 = pipes[1];
                                rec.EventCategory = pipes[2];
                            }
                            catch (Exception ex)
                            { }

                            if (rec.EventCategory == "ACCEPT")
                            {
                                rec.CustomStr1 = pipes[3].Split(':')[0];
                                rec.CustomInt2 = Convert_To_Int32(pipes[3].Split(':')[1]);

                            }
                            else if (rec.EventCategory == "SENDER")
                            {
                                rec.CustomStr3 = pipes[3];
                            }
                            else if (rec.EventCategory == "ORCPTS")
                            {
                                rec.CustomStr4 = pipes[3];
                            }
                            else if (rec.EventCategory == "SOURCE")
                            {
                                rec.CustomStr4 = pipes[3];
                            }
                            else if (rec.EventCategory == "SUBJECT")
                            {
                                rec.CustomStr4 = pipes[3];
                            }
                            else if (rec.EventCategory == "MSGID")
                            {
                                rec.CustomStr4 = pipes[3].Trim().TrimEnd('<').TrimStart('>');//Mail adresi.
                            }
                            else if (rec.EventCategory == "ATTACH")
                            {
                                rec.CustomStr4 = "";
                                for (int i = 3; i < pipes.Length; i++)
                                {
                                    rec.CustomStr4 += pipes[i] + "|";
                                }
                                rec.CustomStr4 = rec.CustomStr4.TrimEnd('|');
                            }
                            else if (rec.EventCategory == "UNTESTED")
                            {
                                rec.CustomStr4 = pipes[3];
                                rec.CustomStr5 = " ";
                                for (int i = 4; i < pipes.Length; i++)
                                {
                                    rec.CustomStr5 += pipes[i] + "|";
                                }
                                rec.CustomStr5 = rec.CustomStr5.Trim();
                            }
                            else if (rec.EventCategory == "VERDICT")
                            {
                                rec.CustomStr4 = pipes[3];
                                rec.CustomStr5 = " ";
                                for (int i = 4; i < pipes.Length; i++)
                                {
                                    rec.CustomStr5 += pipes[i] + "|";
                                }
                                rec.CustomStr5 = rec.CustomStr5.Trim();
                            }
                            else if (rec.EventCategory == "TRACKERID")
                            {
                                rec.CustomStr4 = pipes[3];
                                rec.CustomStr5 = " ";
                                for (int i = 4; i < pipes.Length; i++)
                                {
                                    rec.CustomStr5 += pipes[i] + "|";
                                }
                                rec.CustomStr5 = rec.CustomStr5.Trim();
                            }
                            else if (rec.EventCategory == "IRCPTACTION")
                            {
                                rec.CustomStr4 = pipes[3];
                                rec.CustomStr5 = " ";
                                for (int i = 4; i < pipes.Length; i++)
                                {
                                    rec.CustomStr5 += pipes[i] + "|";
                                }
                                rec.CustomStr5 = rec.CustomStr5.TrimEnd('|');
                            }
                            else if (rec.EventCategory == "DELIVER")
                            {
                                rec.CustomStr5 = pipes[3];
                                rec.CustomStr4 = pipes[4];
                            }
                            else if (rec.EventCategory == "DELIVERY_FAILURE")
                            {
                                rec.CustomStr5 = pipes[3];
                                rec.CustomStr4 = pipes[4];
                            }
                            else if (rec.EventCategory == "ATTACHFILTER")
                            {
                                rec.CustomStr4 = "";
                                for (int i = 3; i < pipes.Length; i++)
                                {
                                    rec.CustomStr4 += pipes[i] + "|";
                                }
                                rec.CustomStr4 = rec.CustomStr4.TrimEnd('|');
                            }
                        }
                        else
                        {
                            //192.168.2.80:46689 : security2.info Feb 18 10:57:05 brightmail xinetd[2225]: START: https pid=6620 from=192.168.111.66
                            if (parts[8].Contains("START"))
                            {
                                rec.EventCategory = parts[8].TrimEnd(':');
                                rec.CustomStr1 = parts[9];
                                rec.CustomInt3 = Convert_To_Int32(parts[10].Split('=')[1]);
                                rec.CustomStr6 = parts[11].Split('=')[1];
                            }
                        }
                    }
                    else
                    {
                        L.LogTimed(LogType.FILE, LogLevel.ERROR, " Line format is not like we want. Line : " + args.Message); 
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, ex.ToString());
                    L.Log(LogType.FILE, LogLevel.ERROR, " Error line written in description. Line : " + args.Message);
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
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, " Hataya düşen line : " + args.Message);
            }


        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SymantecBrightmailRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecBrightmailRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecBrightmailRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecBrightmailRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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

        private int Convert_To_Int32(string strValue)
        {
            int intValue = 0;
            try
            {
                intValue = Convert.ToInt32(strValue);
                return intValue;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

    }
}
