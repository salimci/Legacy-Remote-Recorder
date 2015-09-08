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

namespace CiscoNDRecorder
{
    public class CiscoNDRecorder:CustomBase
    {        
        private uint logging_interval = 60000, log_size = 1000000;       
        private string location = "", remote_host = "localhost";        
        private int trc_level = 3, Syslog_Port=514,zone = 0;
        private string err_log,protocol = "UDP";
        private CLogger L;
        protected bool usingRegistry = false;
        private bool reg_flag = false;
        protected Int32 Id = 0;
        private ProtocolType pro;
        public Syslog slog=null;
        protected String virtualhost, Dal;
        private void InitializeComponent()
        {                      
        }
        public CiscoNDRecorder()
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
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoNDRecorder" + Id + ".log";
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
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Cisco_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        void Cisco_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
            try
            {
                String[] message = args.Message.Split(':');
                StringBuilder sb = new StringBuilder();                                
                rec.SourceName = message[0];
                try
                {
                    rec.CustomInt1 = Convert.ToInt32(message[1]);
                }
                catch { }
                sb.Append(message[2]).Append(" ");
                rec.SourceName = args.Source;
                rec.EventCategory = message[6];
                sb.Append(message[7]);
                rec.Description = sb.ToString();
                if(message[7].Contains("MAC address"))
                {
                    String[] desc = message[7].Split(' ');
                    rec.CustomStr2 = desc[7];
                    rec.CustomStr1 = desc[9];
                }
                else if(message[7].Contains("Interface "))
                {
                    String[] desc = message[7].Split(' ');
                    for (int i = 0; i < desc.Length; i++)
                    {
                        if (desc[i] == "Interface")
                        {
                            rec.CustomStr1 = desc[i + 1].TrimEnd(',');
                        }
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");                
            }
            catch (Exception er)
            {                
                L.Log(LogType.FILE, LogLevel.ERROR, er.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, er.Source);
                rec.Description = args.Message;
            }
            finally
            {
                rec.LogName = "CiscoNDRecorder";
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
            s.SetData(rec);
            L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            s.Dispose();
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoNetworkDevicesSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoNetworkDevicesSyslogRecorder").GetValue("Syslog Port"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("CiscoNetworkDevicesSyslogRecorder").GetValue("Protocol").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoNetworkDevicesSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {                
                L.Log(LogType.FILE, LogLevel.ERROR, er.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, er.Source);
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
                EventLog.WriteEntry("Security Manager CiscoNetworkDevicesSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }
}
