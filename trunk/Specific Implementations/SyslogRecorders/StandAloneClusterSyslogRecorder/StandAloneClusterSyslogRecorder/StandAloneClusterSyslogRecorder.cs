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

namespace StandAloneClusterSyslogRecorder
{
    public class StandAloneClusterSyslogRecorder : CustomBase
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
        Dictionary<String, Int32> dictHash;

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on StandAloneClusterSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on StandAloneClusterSyslog Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CiscoPixSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sgs_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing StandAloneClusterSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager StandAloneClusterSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\StandAloneClusterSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager StandAloneClusterSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public StandAloneClusterSyslogRecorder()
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

        public String[] SpaceSplit(String line, bool useTabs)
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
        }

        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {

                L.Log(LogType.FILE, LogLevel.DEBUG, "StandAloneClusterSyslogRecorder() | Start preparing record");

                string sLogLine = args.Message;

                L.Log(LogType.FILE, LogLevel.DEBUG, "StandAloneClusterSyslogRecorder() | start line=" + sLogLine);


                if (string.IsNullOrEmpty(sLogLine) == true)
                {
                    L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Line Ýs Null or Empty");
                    return;
                }

                if (sLogLine.Contains("INTEGER") || sLogLine.Contains("cdrRecordType"))
                    return;

                rec.LogName = "StandAloneClusterSyslogRecorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                
                rec = LogFormat_Message(sLogLine, rec);

                L.Log(LogType.FILE, LogLevel.DEBUG, "StandAloneClusterSyslogRecorder() | Start sending Data");
                
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "StandAloneClusterSyslogRecorder() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "StandAloneClusterSyslogRecorder() |  Finish Sending Data");
                }


            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "StandAloneClusterSyslogRecorder() | " + er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "StandAloneClusterSyslogRecorder() | " + args.Message);
            }
        }

        private Rec LogFormat_Message(string sLogLine, Rec rec)
        {

            try
            {
                L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
                L.LogTimed(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + sLogLine);


                string sKeyWord = "cdrRecordType	globalCallID_callManagerId	globalCallID_callId	origLegCallIdentifier	dateTimeOrigination	origNodeId	origSpan	origIpAddr	callingPartyNumber	callingPartyUnicodeLoginUserID	origCause_location	origCause_value	origPrecedenceLevel	origMediaTransportAddress_IP	origMediaTransportAddress_Port	origMediaCap_payloadCapability	origMediaCap_maxFramesPerPacket	origMediaCap_g723BitRate	origVideoCap_Codec	origVideoCap_Bandwidth	origVideoCap_Resolution	origVideoTransportAddress_IP	origVideoTransportAddress_Port	origRSVPAudioStat	origRSVPVideoStat	destLegIdentifier	destNodeId	destSpan	destIpAddr	originalCalledPartyNumber	finalCalledPartyNumber	finalCalledPartyUnicodeLoginUserID	destCause_location	destCause_value	destPrecedenceLevel	destMediaTransportAddress_IP	destMediaTransportAddress_Port	destMediaCap_payloadCapability	destMediaCap_maxFramesPerPacket	destMediaCap_g723BitRate	destVideoCap_Codec	destVideoCap_Bandwidth	destVideoCap_Resolution	destVideoTransportAddress_IP	destVideoTransportAddress_Port	destRSVPAudioStat	destRSVPVideoStat	dateTimeConnect	dateTimeDisconnect	lastRedirectDn	pkid	originalCalledPartyNumberPartition	callingPartyNumberPartition	finalCalledPartyNumberPartition	lastRedirectDnPartition	duration	origDeviceName	destDeviceName	origCallTerminationOnBehalfOf	destCallTerminationOnBehalfOf	origCalledPartyRedirectOnBehalfOf	lastRedirectRedirectOnBehalfOf	origCalledPartyRedirectReason	lastRedirectRedirectReason	destConversationId	globalCallId_ClusterID	joinOnBehalfOf	comment	authCodeDescription	authorizationLevel	clientMatterCode	origDTMFMethod	destDTMFMethod	callSecuredStatus	origConversationId	origMediaCap_Bandwidth	destMediaCap_Bandwidth	authorizationCodeValue";
                
                if (dictHash != null)
                    dictHash.Clear();

                dictHash = new Dictionary<String, Int32>();

                String[] fields = SpaceSplit(sKeyWord, true);
                Int32 count = 0;

                foreach (String field in fields)
                {
                    dictHash.Add(field, count);
                    count++;
                }

                String[] arr = sLogLine.Split('\t');

                rec.CustomStr1 = arr[dictHash["finalCalledPartyNumberPartition"]];
                rec.CustomStr2 = arr[dictHash["lastRedirectDnPartition"]];
                rec.CustomInt1 = ObjectToInt32(arr[dictHash["duration"]], 0);
                rec.CustomStr3 = arr[dictHash["origDeviceName"]];
                rec.CustomStr4 = arr[dictHash["destDeviceName"]];
                rec.CustomInt6 = ObjectToInt64(arr[dictHash["dateTimeOrigination"]], 0);

                return rec;

            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() | Line " + sLogLine);
                rec.Description = sLogLine;
                return rec;

            }

        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\StandAloneClusterSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("StandAloneClusterSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("StandAloneClusterSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager StandAloneClusterSyslog Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager StandAloneClusterSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);
            }
            catch
            {
                return iReturn;
            }

        }
        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }

        }


    }


}

