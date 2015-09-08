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

namespace CiscoSwitchesRecorder
{
    public class CiscoSwitchesSyslogRecorder : CustomBase
    {
        #region Deðiþkenler
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
            private string unknownlogformat;
        #endregion
        
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSwitchesSyslogRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSwitchesSyslogRecorder functions may not be running");
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


                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SonicWallSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SonicWallSyslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoSwitchesSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {   
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoSwitchesSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoSwitchesSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }   
            
        public CiscoSwitchesSyslogRecorder()
        {
            sourceName = null;
            sourceportNumber = 0;
            sequenceNo = 0;
            dateTime = null;
            facility = null;
            severity = 0;
            mnemonic = null;
            messageText = null;
            logName = null;
            eventType = null;
            unknownlogformat = null;
        }
        
        void slog_SyslogEvent(LogMgrEventArgs args)
        {           
                Rec rec = new Rec();
                    
                try 
                {   
                    L.Log(LogType.FILE, LogLevel.DEBUG, "args.Message" + args.Message);
                    
                    logName = "CiscoSwitches Syslog Recorder";
                    dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    messageText = args.Message.Replace('\0', ' ');

                    int indexofper = 0;
                    int indexofpo = 0;

                    string messagepart1 = "";
                    string messagepart2 = "";


                    indexofper = messageText.IndexOf('%');
                    indexofpo = messageText.IndexOf(':',indexofper);

                    messagepart1 = messageText.Substring(0,indexofpo);
                    messagepart2 = messageText.Substring(indexofpo + 1);

                    messagepart1 = messagepart1.Trim();
                    messagepart2 = messagepart2.Trim();

                    string[] fields = messagepart1.Split(':');

                    for (int i = 0; i < fields.Length; i++)
                    {
                        fields[i] = fields[i].Trim();
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Fields Lenght Is : " + fields.Length.ToString());
                    bool errorCheck = false;    
                        
                        try
                        {
                            sourceName = fields[0];
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceName :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            sourceportNumber = Convert.ToInt32(fields[1]);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sourceportNumber :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            eventType = fields[2].Split(' ')[0].Split('.')[1];
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find eventType :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            sequenceNo = Convert.ToInt32(fields[2].Split(' ')[1]);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Couldnt find sequence no :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            facility = fields[fields.Length-1].Split('-')[0].TrimStart('%');
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing facility  :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            severity = Convert.ToInt32(fields[fields.Length - 1].Split('-')[1]);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing severity  :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            mnemonic = fields[fields.Length - 1].Split('-')[2];
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing mnemonic  :" + ex.Message);
                            errorCheck = true;
                        }
                        
                        try
                        {
                            messageText = messagepart2;
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "An error occured while parsing messageText  :" + ex.Message);
                            errorCheck = true;
                        }

                        if (errorCheck)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Unknown Log Format");
                            clearforError();
                            unknownlogformat = args.Message;
                        }

                    rec = createRec();
                    clearProperties();
                        
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                        
                    if (usingRegistry)
                    {   
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        s.SetReg(Id, rec.Datetime, "","","",rec.Datetime);
                    }
                     
                    L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data"); 
                }   
                catch (Exception er)
                {   
                    L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                    L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
                }   
        }       
            
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoSwitchesSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSwitchesSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSwitchesSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoSwitchesSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CiscoSwitchesSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
            
        public Rec createRec() 
        {   
            Rec rec = new Rec();
            
            rec.SourceName = sourceName;
            rec.Datetime = dateTime;
            rec.LogName = logName;
            rec.EventType = eventType;
            
            rec.CustomInt1 = sequenceNo;
            rec.CustomInt2 = severity;
            rec.CustomInt9 = Convert.ToInt64(sourceportNumber);
            
            rec.CustomStr1  = facility;
            rec.CustomStr2  = mnemonic;
            rec.CustomStr3  = messageText;
            rec.Description = unknownlogformat;
            L.Log(LogType.FILE, LogLevel.DEBUG,unknownlogformat);
            L.Log(LogType.FILE, LogLevel.DEBUG,messageText);
            return rec;
        }   

        private void clearProperties() 
        {
            sourceName = null;
            sourceportNumber = 0;
            sequenceNo = 0;
            dateTime = null;
            facility = null;
            severity = 0;
            mnemonic = null;
            messageText = null;
            logName = null;
            eventType = null;
            unknownlogformat = null;
        }

        private void clearforError()
        {
            sourceName = null;
            sourceportNumber = 0;
            sequenceNo = 0;
            facility = null;
            severity = 0;
            mnemonic = null;
            messageText = null;
            eventType = null;
        }
    }   
}       
