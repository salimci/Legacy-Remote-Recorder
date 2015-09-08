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

namespace AixRecorder
{
    public class IBMAixSyslogRecorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoPixSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoPixSyslog Recorder functions may not be running");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing CiscoPixSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\AixRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public IBMAixSyslogRecorder()
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
                L.Log(LogType.FILE, LogLevel.DEBUG, "Aix_SyslogEvent() | Start preparing record");

                rec.LogName = "AixRecorder";

                string sLogLine = args.Message;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Aix_SyslogEvent() | start line=" + sLogLine);

                if (sLogLine.Contains("Aritech_Isg1000") == true)
                    rec = LogFormat_Aritech_Isg1000(sLogLine, rec);
                else
                    if (sLogLine.Contains("Message") == true)
                        rec = LogFormat_Message(sLogLine, rec);




                L.Log(LogType.FILE, LogLevel.DEBUG, "Aix_SyslogEvent() | Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Aix_SyslogEvent() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Aix_SyslogEvent() |  Finish Sending Data");
                }


            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Aix_SyslogEvent() | " + er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Aix_SyslogEvent() | " + args.Message);
            }
        }

        private Rec LogFormat_Message(string sLogLine, Rec rec)
        {

            try
            {
                string[] arrLogs = SpaceSplit(sLogLine, true);
                rec = getLog_date_type_src(arrLogs, rec);

                for (int i = 0; i < arrLogs.Length; i++)
                {
                    if (arrLogs[i].Contains("Message") == true)
                    {
                        rec.CustomStr5 = arrLogs[i - 3] + " " + arrLogs[i - 2] + " " + arrLogs[i - 1];//mesajtarih
                        rec.CustomStr8 = arrLogs[i] + " " + arrLogs[i + 1];
                        rec.CustomStr6 = arrLogs[i + 3].Trim(':');
                        sLogLine = sLogLine.Substring(sLogLine.IndexOf(arrLogs[i + 3]) + arrLogs[i + 3].Length, sLogLine.Length - sLogLine.IndexOf(arrLogs[i + 3]) - arrLogs[i + 3].Length);
                        break;
                    }
                }

                arrLogs = sLogLine.Trim().Split(' ');


                if (arrLogs[0].Contains("[") && arrLogs[0].Contains("]"))
                {
                    rec.CustomStr4 = arrLogs[0];

                    if (sLogLine.Contains(" for ") && arrLogs[0].Contains("]"))
                        rec.CustomStr7 = sLogLine.Substring(sLogLine.IndexOf(']') + 1, sLogLine.IndexOf(" for ") - sLogLine.IndexOf(']') - 1);
                    else
                        if (sLogLine.Contains(" from ") && arrLogs[0].Contains("]"))
                            rec.CustomStr7 = sLogLine.Substring(sLogLine.IndexOf(']') + 1, sLogLine.IndexOf(" from ") - sLogLine.IndexOf(']') - 1);

                }

                for (int i = 0; i < arrLogs.Length; i++)
                {

                    if (arrLogs[i].Contains("from="))
                        rec.CustomStr1 = arrLogs[i].Split('=')[1].Trim('<').Trim('>');

                    if (arrLogs[0] != ("su:"))
                        if (arrLogs[i] == " from ")
                            rec.CustomStr1 = arrLogs[i + 1].Trim().Trim(':');

                    if (arrLogs[i].Contains("to="))
                        rec.CustomStr3 = arrLogs[i].Split('=')[1].Trim(',').Trim('<').Trim('>');

                    if (arrLogs[i] == " to ")
                        rec.CustomStr3 = arrLogs[i + 1].Trim();

                    if (arrLogs[i].Trim() == "for")
                        rec.UserName = arrLogs[i + 1].Trim();

                    if (arrLogs[0] == ("su:"))
                        if (arrLogs[i].Trim() == "from")
                            rec.UserName = arrLogs[i + 1].Trim();

                }

                rec.Description = sLogLine;

                return rec;

            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogFormat_Aritech_Isg1000() | Line " + sLogLine);
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoPixSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoPixSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoPixSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private Rec LogFormat_Aritech_Isg1000(string sLine, Rec rec)
        {

            try
            {
                string[] arrLogs = SpaceSplit(sLine, true);
                rec = getLog_date_type_src(arrLogs, rec);

                if (sLine.Contains("[") && sLine.Contains("]"))
                {
                    rec.UserName = sLine.Substring(sLine.IndexOf('[') + 1, sLine.IndexOf(']') - sLine.IndexOf('[') - 1);
                    sLine = sLine.Substring(sLine.IndexOf(':', sLine.IndexOf('[')) + 1, sLine.Length - sLine.IndexOf(':', sLine.IndexOf('[')) - 1).Trim();
                }


                for (int i = 0; i < arrLogs.Length; i++)
                {
                    if (arrLogs[i].Trim() == ("From"))
                    {

                        rec.EventCategory = sLine.Substring(0, sLine.IndexOf(" From "));
                        
                        if (arrLogs[i + 1].Contains(":"))
                        {
                            rec.CustomStr1 = arrLogs[i + 1].Split(':')[0];
                            rec.CustomInt2 = Convert.ToInt32(arrLogs[i + 1].Split(':')[1]);
                        }
                        else
                            rec.CustomStr1 = arrLogs[i + 1];
                    }

                    else if (arrLogs[i] == "to")
                    {
                        if (arrLogs[i + 1].Contains(":"))
                        {
                            rec.CustomStr3 = arrLogs[i + 1].Split(':')[0];
                            rec.CustomInt3 = Convert.ToInt32(arrLogs[i + 1].Split(':')[1].Trim(','));
                        }
                        else
                            rec.CustomStr3 = arrLogs[i + 1].Trim(',');
                    }

                    else if (arrLogs[i].Contains("IKE"))
                        rec.CustomStr2 = arrLogs[i + 1];
                    else if (arrLogs[i].Contains("start_time="))
                        rec.CustomStr9 = arrLogs[i].Split('=')[1].Trim('\"');
                    else if (arrLogs[i].Contains("duration="))
                        rec.CustomStr10 = arrLogs[i].Split('=')[1].Trim();
                    else if (arrLogs[i].Contains("policy_id="))
                        rec.CustomInt4 = Convert.ToInt32(arrLogs[i].Split('=')[1].Trim());
                    //else if (arrLogs[i].Contains("service="))
                    //    rec.CustomStr7 = arrLogs[i].Split('=')[1].Trim();                 

                }

                rec.Description = sLine;
                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogFormat_Aritech_Isg1000() | Line " + sLine);
                rec.Description = sLine;
                return rec;
            }

        }

        private Rec getLog_date_type_src(string[] arrLogs, Rec rec)
        {

            try
            {

                // rec.Datetime = arrLogs[0] + " " + arrLogs[1];
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = arrLogs[2];
                if (arrLogs[0].Contains(":"))
                {
                    rec.CustomStr2 = arrLogs[0].Trim().Split(':')[0];
                    rec.CustomInt1 = Convert.ToInt32(arrLogs[0].Trim().Split(':')[1]);
                }
                else
                    rec.CustomStr2 = arrLogs[0];

                return rec;

            }
            catch (Exception exp)
            {
                throw exp;
            }

        }


    }


}

