using System.IO;
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

namespace McaffeeIpsV5SyslogRecorder
{
    public class McaffeeIpsV5SyslogRecorder : CustomBase
    {

        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514;
        private static int zone = 0;
        private string err_log, remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private string protocol;

        private bool usingRegistry = true;
        private string virtualHost, Dal;
        private int identity;

        private string location;

        private void InitializeComponent()
        {
            //Init();
        }//initializecomponent

        public override void Init()
        {
            if (usingRegistry)
            {
                if (!Read_Registry())
                {
                    EventLog.WriteEntry("Security Manager McaffeeIpsV5SyslogRecorder Read Registry", "McaffeeIpsV5SyslogRecorder may not working properly ", EventLogEntryType.Error);
                    return;
                }
            }
            else
            {
                get_logDir();
            }

            if (!Initialize_Logger())
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Recorder Service functions may not be running");
                return;
            }

        }

        public void get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\McaffeeIpsV5SyslogRecorder" + identity + ".log";
            }
            catch (Exception ess)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "" + ess.ToString());
            }

        }

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, string LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {

            trc_level = TraceLevel;
            usingRegistry = false;
            virtualHost = virtualhost;
            identity = Identity;
            Dal = dal;
            location = Location;
            if (location.Contains(':'.ToString()))
            {
                String[] parse = location.Split(':');
                protocol = parse[0];
                Syslog_Port = Convert.ToInt32(parse[1]);
            }
            else
            {
                Syslog_Port = 514;
            }
            remote_host = RemoteHost;
            zone = Zone;
        }

        public override void Start()
        {
            try
            {

                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  McaffeeIpsV5SyslogRecorder");

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening McaffeeIpsV5Syslog on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                ProtocolType pro;
                if (protocol.ToLower() == "tcp")
                    pro = ProtocolType.Tcp;
                else
                    pro = ProtocolType.Udp;

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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Mcaffee_Syslog);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Mcaffee Ips Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeIpsV5SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }

        }

        public McaffeeIpsV5SyslogRecorder()
        {

        }

        static String dateandtime(String w3, String w4, String w5)
        {
            
            String mounth, day, year, time;
            day = w4;
            time = w5;
            year = DateTime.Now.Year.ToString();
            switch (w3)
            {
                case "Jan":
                    mounth = "01";
                    break;
                case "Feb":
                    mounth = "02";
                    break;
                case "Mar":
                    mounth = "03";
                    break;
                case "Apr":
                    mounth = "04";
                    break;
                case "May":
                    mounth = "05";
                    break;
                case "Jun":
                    mounth = "06";
                    break;
                case "Jul":
                    mounth = "07";
                    break;
                case "Aug":
                    mounth = "08";
                    break;
                case "Sep":
                    mounth = "09";
                    break;
                case "Oct":
                    mounth = "10";
                    break;
                case "Nov":
                    mounth = "11";
                    break;
                case "Dec":
                    mounth = "12";
                    break;
                default:
                    mounth = DateTime.Now.Month.ToString();
                    break;

            }
            String dType = mounth + "/" + day + "/" + year + " " + time;
            DateTime dt = Convert.ToDateTime(dType);
            dType = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

            return dType;
        }

        void Mcaffee_Syslog(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "Mcaffee_Syslog method is called");
            CustomBase.Rec r = new CustomBase.Rec();
            CustomServiceBase s;
            if (usingRegistry)
            {
                s = base.GetInstanceService("Security Manager Sender");
            }
            else
            {
                s = base.GetInstanceService("Security Manager Remote Recorder");
            }

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "log is: " + args.Message.Replace('\0', ' '));
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");

                r.SourceName = args.Source;
                r.EventType = args.EventLogEntType.ToString();

                string logText = args.Message;

                string[] logTextArray = logText.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                string[] dateRaw = logTextArray[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string monthText = dateRaw[3];
                string dayText = dateRaw[4];
                string timeText = dateRaw[5];

                r.LogName = "McAfee IPS Syslog Recorder";
                L.Log(LogType.FILE, LogLevel.DEBUG, "datetime method parameters: " + monthText + ", " + dayText + ", " + timeText);
                r.Datetime = dateandtime(monthText, dayText, timeText);
                L.Log(LogType.FILE, LogLevel.DEBUG, "r.DateTime: " + r.Datetime.ToString());

                r.ComputerName = dateRaw[0]; //McAfee Server IP

                r.CustomStr2 = "Attack " + logTextArray[1].Split(':')[0].Trim(); //Attack name
                r.Description = logTextArray[1].Split(':')[1].Trim(); //Attack name description
                r.CustomStr6 = logTextArray[2].Trim(); //Attack confidence (Severity)

                string[] sourceIP_Port = logTextArray[3].Split(':');
                r.CustomStr3 = sourceIP_Port[0].Trim(); //Source IP
                try
                {
                    r.CustomInt9 = long.Parse(sourceIP_Port[1].Trim()); //Source Port
                }
                catch (Exception)
                {
                    r.CustomInt9 = 0;
                }

                string[] targetIP_Port = logTextArray[4].Split(':');
                r.CustomStr4 = targetIP_Port[0].Trim(); //Target IP
                try
                {
                    r.CustomInt10 = long.Parse(targetIP_Port[1].Trim()); //Target Port
                }
                catch (Exception)
                {
                    r.CustomInt10 = 0;
                }

                r.EventCategory = logTextArray[5].Trim(); //Category
                r.CustomStr7 = logTextArray[6].Trim(); //Sub-category
                r.CustomStr8 = logTextArray[7].Trim(); //Direction
                r.CustomStr5 = logTextArray[8].Trim(); //Result status
                r.CustomStr9 = logTextArray[9].Trim(); //Application protocol
                r.CustomStr10 = logTextArray[10].Trim(); //Network protocol

                try
                {
                    r.CustomInt1 = int.Parse(logTextArray[11].Split('<')[0].Trim()); //Attack count
                }
                catch (Exception)
                {
                    r.CustomInt1 = 0;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "r.EventCategory: " + r.EventCategory);

                //sending data
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

                if (usingRegistry)
                {
                    s.SetData(r);
                }
                else
                {
                    s.SetData(Dal, virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "", "", r.Datetime);
                }
                
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }//end of try
            catch (Exception ex)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at parsing log: " + args.Message + "\n" + ex.ToString() + "\nPLEASE CHECK MCAFEE IPS SYSLOG CONFIGURATION AND BE SURE THAT MESSAGE FORMAT IS AS FOLLOWS: $IV_ATTACK_TIME$ | $IV_ATTACK_NAME$ | $IV_ATTACK_CONFIDENCE$ | $IV_SOURCE_IP$:$IV_SOURCE_PORT$ | $IV_DESTINATION_IP$:$IV_DESTINATION_PORT$ | $IV_CATEGORY$ | $IV_SUB_CATEGORY$ | $IV_DIRECTION$ | $IV_RESULT_STATUS$ | $IV_APPLICATION_PROTOCOL$ | $IV_NETWORK_PROTOCOL$ | $IV_ATTACK_COUNT$");
            }
            finally
            {
                s.Dispose();
            }

        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\McaffeeIpsV5SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeIpsV5SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeIpsV5SyslogRecorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogRecorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeIpsV5SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McaffeeIpsV5Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }//class
}//name space