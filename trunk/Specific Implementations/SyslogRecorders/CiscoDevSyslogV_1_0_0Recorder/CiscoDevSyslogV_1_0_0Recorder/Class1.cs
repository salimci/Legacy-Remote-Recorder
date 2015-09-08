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

namespace CiscoDevSyslogV_1_0_0Recorder
{
    public class CiscoDevSyslogV_1_0_0Recorder : CustomBase
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

        private string sourceName;
        private int sourceportNumber;
        private int sequenceNo;
        private string dateTime;
        private string facility;
        private int severity;
        private string mnemonic;
        private string messageText;
        private string logName;
        private string eventType;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";


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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoDevSyslogV_1_0_0Recorder" + Id + ".log";
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

        public CiscoDevSyslogV_1_0_0Recorder()
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
            CustomBase.Rec rec = new CustomBase.Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "CiscoDevSyslogV_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();

                    if (args.Message.Length > 895)
                        rec.Description = args.Message.Substring(0, 890);
                    else
                        rec.Description = args.Message;

                    rec.Description = args.Message.Replace("'", "|");

                    ParsingProcess(args);
                    rec = CreateRec();

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
                    rec.SourceName = args.Source;
                    L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
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

        public void ParsingProcess(LogMgrEventArgs args)
        {
            this.logName = "Cisco DEV Recorder";
            L.Log(LogType.FILE, LogLevel.DEBUG, "Message" + args.Message);
            this.eventType = args.EventLogEntType.ToString();
            this.messageText = args.Message.Replace('\0', ' ');
            string[] syslogMessageArr = args.Message.Split(':');

            for (int i = 0; i < syslogMessageArr.Length; i++)
            {
                syslogMessageArr[i] = syslogMessageArr[i].Trim();
            }

            if (syslogMessageArr.Length == 8)
            {
                #region parser

                try
                {
                    sourceName = syslogMessageArr[0];
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceName :" + ex.Message);
                }

                try
                {
                    sourceportNumber = Convert.ToInt32(syslogMessageArr[1]);
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceportNumber :" + ex.Message);
                }

                try
                {
                    sequenceNo = Convert.ToInt32(syslogMessageArr[2].Split(' ')[1]);
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sequence no :" + ex.Message);
                }

                try
                {
                    string[] date = { "", "", "", "" };

                    date[0] = Convert.ToString(DateTime.Now.Year);

                    string[] datepartvirtual = syslogMessageArr[3].Split(' ');
                    int count = 0;

                    for (int i = 0; i < datepartvirtual.Length; i++)
                    {
                        if (datepartvirtual[i] == "")
                        {
                            count++;
                        }
                    }

                    string[] datepart = new string[datepartvirtual.Length - count];
                    int k = 0;
                    for (int j = 0; j < datepartvirtual.Length; j++)
                    {
                        if (datepartvirtual[j] != "")
                        {
                            datepart[k] = datepartvirtual[j];
                            k++;
                        }
                    }

                    date[1] = datepart[0].TrimStart('*');
                    date[2] = datepart[1];
                    date[3] = datepart[2] + ":" + syslogMessageArr[4] + ":" + syslogMessageArr[5];

                    //string logDate = "";
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    logDate += date[i] + " ";
                    //}

                    DateTime _logDate = new DateTime();
                    _logDate = DateTime.Now;
                    dateTime = _logDate.ToString(dateFormat);
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing date time  :" + ex.Message);
                }

                try
                {
                    facility = syslogMessageArr[6].Split('-')[0].TrimStart('%');
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing facility  :" + ex.Message);
                }

                try
                {
                    severity = Convert.ToInt32(syslogMessageArr[6].Split('-')[1]);
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing severity  :" + ex.Message);
                }

                try
                {
                    mnemonic = syslogMessageArr[6].Split('-')[2];
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing mnemonic  :" + ex.Message);
                }

                try
                {
                    messageText = syslogMessageArr[7];
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing messageText  :" + ex.Message);
                }
                #endregion
            }
            else
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Unexcepted log format");
            }
        }


        public Rec CreateRec()
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            rec.SourceName = sourceName;
            rec.Datetime = dateTime;
            rec.LogName = logName;
            rec.EventType = eventType;
            rec.CustomInt1 = sequenceNo;
            rec.CustomInt2 = severity;
            rec.CustomInt9 = Convert.ToInt64(sourceportNumber);
            rec.CustomStr1 = facility;
            rec.CustomStr2 = mnemonic;
            rec.Description = messageText;
            return rec;
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Trace Level"));
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
