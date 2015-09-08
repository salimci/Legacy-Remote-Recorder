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

namespace CiscoPixSyslogRecorder
{
    public class CiscoPixSyslogV6Recorder : CustomBase
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoPixSyslogRecorder" + Id + ".log";
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

        public CiscoPixSyslogV6Recorder()
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
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                rec.LogName = "CiscoPixSyslog Recorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();

                string sLogKod = "";
                string sLogLine = args.Message;
                if (sLogLine.Contains("%") == true)
                {
                    sLogKod = sLogLine.Substring(sLogLine.IndexOf('%') + 1, sLogLine.IndexOf(':', sLogLine.IndexOf('%')) - sLogLine.IndexOf('%') - 1);
                }

                switch (sLogKod)
                {
                    case "PIX-6-302013":
                        { rec = LogKod_PIX_6_302013(sLogLine, rec); }
                        break;
                    case "PIX-7-715053":
                        { rec = LogKod_PIX_7_715053(sLogLine, rec); }
                        break;
                    case "PIX-6-713184":
                        { rec = LogKod_PIX_6_713184(sLogLine, rec); }
                        break;
                    case "PIX-6-302014":
                        { rec = LogKod_PIX_6_302014(sLogLine, rec); }
                        break;
                    case "PIX-7-713906":
                        { rec = LogKod_PIX_7_713906(sLogLine, rec); }
                        break;
                    case "PIX-6-302021":
                        { rec = LogKod_PIX_6_302021(sLogLine, rec); }
                        break;
                    case "PIX-7-609002":
                        { rec = LogKod_PIX_7_609002(sLogLine, rec); }
                        break;
                    case "PIX-7-714003":
                        { rec = LogKod_PIX_7_714003(sLogLine, rec); }
                        break;
                    case "PIX-7-715021":
                        { rec = LogKod_PIX_7_715021(sLogLine, rec); }
                        break;
                    case "PIX-7-715022":
                        { rec = LogKod_PIX_7_715022(sLogLine, rec); }
                        break;
                    case "PIX-3-713119":
                        { rec = LogKod_PIX_3_713119(sLogLine, rec); }
                        break;
                    case "PIX-7-713121":
                        { rec = LogKod_PIX_7_713121(sLogLine, rec); }
                        break;
                    case "PIX-7-715080":
                        { rec = LogKod_PIX_7_715080(sLogLine, rec); }
                        break;
                    case "PIX-7-715046":
                        { rec = LogKod_PIX_7_715046(sLogLine, rec); }
                        break;
                    case "PIX-7-713236":
                        { rec = LogKod_PIX_7_713236(sLogLine, rec); }
                        break;
                    case "PIX-7-715047":
                        { rec = LogKod_PIX_7_715047(sLogLine, rec); }
                        break;
                    case "PIX-7-714011":
                        { rec = LogKod_PIX_7_714011(sLogLine, rec); }
                        break;
                    case "PIX-7-713025":
                        { rec = LogKod_PIX_7_713025(sLogLine, rec); }
                        break;
                    case "PIX-7-713034":
                        { rec = LogKod_PIX_7_713034(sLogLine, rec); }
                        break;
                    case "PIX-7-713221":
                        { rec = LogKod_PIX_7_713221(sLogLine, rec); }
                        break;
                    case "PIX-7-713222":
                        { rec = LogKod_PIX_7_713222(sLogLine, rec); }
                        break;
                    case "PIX-5-713049":
                        { rec = LogKod_PIX_5_713049(sLogLine, rec); }
                        break;
                    case "PIX-6-602303":
                        { rec = LogKod_PIX_6_602303(sLogLine, rec); }
                        break;
                    case "PIX-7-715007":
                        { rec = LogKod_PIX_7_715007(sLogLine, rec); }
                        break;
                    case "PIX-7-609001":
                        { rec = LogKod_PIX_7_609001(sLogLine, rec); }
                        break;
                    case "PIX-7-715077":
                        { rec = LogKod_PIX_7_715077(sLogLine, rec); }
                        break;
                    case "PIX-7-713204":
                        { rec = LogKod_PIX_7_713204(sLogLine, rec); }
                        break;
                    case "PIX-5-713120":
                        { rec = LogKod_PIX_5_713120(sLogLine, rec); }
                        break;
                    case "PIX-5-335004":
                        { rec = LogKod_PIX_5_335004(sLogLine, rec); }
                        break;
                    case "PIX-6-302015":
                        { rec = LogKod_PIX_6_302015(sLogLine, rec); }
                        break;
                    case "PIX-7-710006":
                        { rec = LogKod_PIX_7_710006(sLogLine, rec); }
                        break;
                    case "PIX-6-106015":
                        { rec = LogKod_PIX_6_106015(sLogLine, rec); }
                        break;
                    case "PIX-3-305005":
                        { rec = LogKod_PIX_3_305005(sLogLine, rec); }
                        break;
                    case "PIX-6-302016":
                        { rec = LogKod_PIX_6_302016(sLogLine, rec); }
                        break;
                    case "PIX-6-302020":
                        { rec = LogKod_PIX_6_302020(sLogLine, rec); }
                        break;
                    case "PIX-7-715075":
                        { rec = LogKod_PIX_7_715075(sLogLine, rec); }
                        break;
                    case "PIX-7-715036":
                        { rec = LogKod_PIX_7_715036(sLogLine, rec); }
                        break;
                    case "PIX-7-715028":
                        { rec = LogKod_PIX_7_715028(sLogLine, rec); }
                        break;
                    case "PIX-7-715076":
                        { rec = LogKod_PIX_7_715076(sLogLine, rec); }
                        break;
                    case "PIX-7-710005":
                        { rec = LogKod_PIX_7_710005(sLogLine, rec); }
                        break;
                    case "PIX-7-715048":
                        { rec = LogKod_PIX_7_715048(sLogLine, rec); }
                        break;
                    case "PIX-6-113009":
                        { rec = LogKod_PIX_6_113009(sLogLine, rec); }
                        break;

                    case "PIX-3-713122":
                        { rec = LogKod_PIX_3_713122(sLogLine, rec); }
                        break;
                    case "PIX-7-713035":
                        { rec = LogKod_PIX_7_713035(sLogLine, rec); }
                        break;

                    case "PIX-7-713225":
                        { rec = LogKod_PIX_7_713225(sLogLine, rec); }
                        break;
                    case "PIX-7-713066":
                        { rec = LogKod_PIX_7_713066(sLogLine, rec); }
                        break;
                    case "PIX-7-715027":
                        { rec = LogKod_PIX_7_715027(sLogLine, rec); }
                        break;
                    case "PIX-7-715006":
                        { rec = LogKod_PIX_7_715006(sLogLine, rec); }
                        break;
                    case "PIX-7-715001":
                        { rec = LogKod_PIX_7_715001(sLogLine, rec); }
                        break;
                    case "PIX-7-714005":
                        { rec = LogKod_PIX_7_714005(sLogLine, rec); }
                        break;
                    case "PIX-5-713050":
                        { rec = LogKod_PIX_5_713050(sLogLine, rec); }
                        break;
                    case "PIX-7-710002":
                        { rec = LogKod_PIX_7_710002(sLogLine, rec); }
                        break;
                    case "PIX-7-715009":
                        { rec = LogKod_PIX_7_715009(sLogLine, rec); }
                        break;
                    case "PIX-4-113019":
                        { rec = LogKod_PIX_4_113019(sLogLine, rec); }
                        break;
                    case "PIX-6-602304":
                        { rec = LogKod_PIX_6_602304(sLogLine, rec); }
                        break;
                    case "PIX-5-713904":
                        { rec = LogKod_PIX_5_713904(sLogLine, rec); }
                        break;
                    case "PIX-5-713041":
                        { rec = LogKod_PIX_5_713041(sLogLine, rec); }
                        break;
                    case "PIX-6-302010":
                        { rec = LogKod_PIX_6_302010(sLogLine, rec); }
                        break;
                    case "PIX-6-110002":
                        { rec = LogKod_PIX_6_110002(sLogLine, rec); }
                        break;
                    case "PIX-7-714004":
                        { rec = LogKod_PIX_7_714004(sLogLine, rec); }
                        break;
                    case "PIX-6-713219":
                        { rec = LogKod_PIX_6_713219(sLogLine, rec); }
                        break;
                    case "PIX-4-713903":
                        { rec = LogKod_PIX_4_713903(sLogLine, rec); }
                        break;
                    case "PIX-7-715049":
                        { rec = LogKod_PIX_7_715049(sLogLine, rec); }
                        break;
                    case "PIX-7-715064":
                        { rec = LogKod_PIX_7_715064(sLogLine, rec); }
                        break;
                    case "PIX-6-113012":
                        { rec = LogKod_PIX_6_113012(sLogLine, rec); }
                        break;
                    case "PIX-7-715019":
                        { rec = LogKod_PIX_7_715019(sLogLine, rec); }
                        break;
                    case "PIX-7-713052":
                        { rec = LogKod_PIX_7_713052(sLogLine, rec); }
                        break;
                    case "PIX-5-713130":
                        { rec = LogKod_PIX_5_713130(sLogLine, rec); }
                        break;
                    case "PIX-6-113008":
                        { rec = LogKod_PIX_6_113008(sLogLine, rec); }
                        break;
                    case "PIX-6-713228":
                        { rec = LogKod_PIX_6_713228(sLogLine, rec); }
                        break;
                    case "PIX-7-715020":
                        { rec = LogKod_PIX_7_715020(sLogLine, rec); }
                        break;
                    case "PIX-7-715055":
                        { rec = LogKod_PIX_7_715055(sLogLine, rec); }
                        break;
                    case "PIX-7-715065":
                        { rec = LogKod_PIX_7_715065(sLogLine, rec); }
                        break;
                    case "PIX-3-713902":
                        { rec = LogKod_PIX_3_713902(sLogLine, rec); }
                        break;
                    case "PIX-3-106014":
                        { rec = LogKod_PIX_3_106014(sLogLine, rec); }
                        break;

                    default:
                        L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() | Log Tanýmlanmayan formatta geldi: " + args.Message);
                        rec.Description = sLogLine;
                        break;

                }

                rec.ComputerName = args.Source;

                L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() | Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() |  Finish Sending Data");
                }



            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "slog_SyslogEvent() | " + er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "slog_SyslogEvent() | " + args.Message);
            }
        }

        private Rec LogKod_PIX_3_106014(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.error %PIX-3-106014: Deny inbound icmp src inside:10.1.1.1 dst inside:172.16.200.2 (type 8, code 0)

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-3-106014:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_106014() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_106014() | Line : " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_3_713902(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.error %PIX-3-713902: IP = 78.187.139.180, Removing peer from peer table failed, no match!

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-3-713902:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713902 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713902 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715065(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715065: IP = 78.187.139.180, IKE AM Initiator FSM error history (struct &0xa087750)  <state>, <event>:  AM_DONE, EV_ERROR-->AM_WAIT_MSG2, EV_RETRY-->AM_WAIT_MSG2, EV_TIMEOUT-->AM_WAIT_MSG2, NullEvent-->AM_SND_MSG1, EV_SND_MSG-->AM_SND_MSG1, EV_START_TMR-->AM_SND_MSG1, EV_RESEND_MSG-->AM_WAIT_MSG2, EV_RETRY

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715065:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715065 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715065 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715055(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715020: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, construct_cfg_set: default domain = yurticikargo.local

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715055:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715055 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715055 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715020(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715020: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, construct_cfg_set: default domain = yurticikargo.local

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715020:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715020 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715020 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_713228(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-713228: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, Assigned private IP address 172.16.202.3 to remote user

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-713228:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713228 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713228 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_113008(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-113008: AAA transaction status ACCEPT : user = sar_elek

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-113008:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_113008() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_113008() | Line : " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_5_713130(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.notice %PIX-5-713130: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, Received unsupported transaction mode attribute: 5

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713130:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713130 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713130 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_713052(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-713052: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, User (sar_elek) authenticated.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713052:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713052 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713052 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715019(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715019: Group = sar_elektronik, Username = sar_elek, IP = 95.15.55.222, IKEGetUserAttributes: primary DNS = 10.1.1.1

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715019:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715019 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715019 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_113012(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-113012: AAA user authentication Successful : local database : user = sar_elek

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-113012:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_113012() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_113012() | Line : " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_7_715064(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715049: IP = 95.15.55.222, Received Fragmentation VID
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715064:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715064 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715064 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715049(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug %PIX-7-715049: IP = 95.15.55.222, Received Fragmentation VID
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715049:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715049 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715049 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_4_713903(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.warning %PIX-4-713903: IP = 88.250.172.55, Information Exchange processing failed

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-4-713903:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_4_713903 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_4_713903 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_713219(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.info %PIX-6-713219: IP = 88.250.172.55, Queuing KEY-ACQUIRE messages to be processed when P1 SA is complete.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-713219:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713219 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713219 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_714004(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-714004: Group = 212.156.146.166, IP = 212.156.146.166, IKE Initiator sending 1st QM pkt: msg id = ce1b5214

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-714004:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714004 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714004 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_110002(string args, Rec rec)
        {

            //172.16.200.253:514 : local7.info %PIX-6-110002: Failed to locate egress interface for UDP from outside:172.16.200.4/10771 to 231.223.174.143/32238

            try
            {

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-110002:", "").Trim();

                if (args.Contains("from ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf("from ")).Trim();

              
                string[] sKey = args.Split(' ');

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[i].Contains("outside:") == true)
                        rec.CustomStr4 = sKey[i].Split(':')[1];//172.28.87.1/1086
                    if (sKey[i].Contains("inside:") == true)
                        rec.CustomStr3 = sKey[i].Split(':')[1];//172.28.87.1/1086
                    if (sKey[i] == "to")
                        rec.CustomStr3 = sKey[i + 1];//172.28.87.1/1086
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_110002 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_110002 () Line : | " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_6_302010(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-302010: 524 in use, 1898 most used

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-302010:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302010() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302010() | Line : " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_5_713041(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.notice %PIX-5-713041: IP = 78.188.153.20, IKE Initiator: Rekeying Phase 1, Intf outside, IKE Peer 78.188.153.20  local Proxy Address N/A, remote Proxy Address N/A,  Crypto map (N/A)

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713041:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713041 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713041 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_5_713904(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.notice %PIX-5-713904: IP = 78.186.68.163, Received encrypted packet with no matching SA, dropping

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713904:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713904 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713904 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_602304(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-602304: IPSEC: An inbound LAN-to-LAN SA (SPI= 0x6AAD4067) between 92.45.66.150 and 78.188.164.121 (user= 78.188.164.121) has been deleted.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-602304:", "").Trim();

                if (args.Contains("IPSEC:") == true)
                    if (args.Contains(" SA ") == true)
                        rec.EventType = args.Substring(args.IndexOf("IPSEC:") + 6, args.IndexOf(" SA ") - args.IndexOf("IPSEC:") - 6).Trim();

                if (args.Contains("between") == true)
                    if (args.Contains(" and ") == true)
                        rec.CustomStr4 = args.Substring(args.IndexOf("between") + 7, args.IndexOf(" and ") - args.IndexOf("between") - 7).Trim();

                if (args.Contains(" and ") == true)
                    if (args.Contains("(user=") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" and ") + 5, args.IndexOf("(user=") - args.IndexOf(" and ") - 5).Trim();

                if (args.Contains("(user=") == true)
                    if (args.Contains(")") == true)
                        rec.UserName = args.Substring(args.IndexOf("(user=") + 6, args.IndexOf(")", args.IndexOf("(user=")) - args.IndexOf("(user=") - 6).Trim();


                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_602304 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_602304 () Line : | " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_4_113019(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.warning %PIX-4-113019: Group = 78.188.164.121, Username = 78.188.164.121, IP = 78.188.164.121, Session disconnected. Session Type: IPSecLAN2LAN, Duration: 4h:19m:29s, Bytes xmt: 1591776, Bytes rcv: 65821500, Reason: Idle Timeout

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-4-113019:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_4_113019 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_4_113019 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715009(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715009: Group = 78.188.164.121, IP = 78.188.164.121, IKE Deleting SA: Remote Proxy 172.28.104.0, Local Proxy 0.0.0.0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715009:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715009 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715009 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_710002(string args, Rec rec)
        {

            //172.16.200.253:514 : local7.debug %PIX-7-710002: UDP access permitted from 10.1.37.1/2820 to inside:172.16.200.253/snmp
            try
            {
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-710002:", "").Trim();

                if (args.Contains("from ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf("from ")).Trim();

                if (args.Contains(" from ") == true)
                    if (args.Contains(" to ") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" from ") + 6, args.IndexOf(" to ") - args.IndexOf(" from ") - 6).Trim();

                string[] sKey = args.Split(' ');

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[1].Contains("outside:") == true)
                        rec.CustomStr4 = sKey[1].Split(':')[1];//172.28.87.1/1086
                    if (sKey[2].Contains("inside:") == true)
                        rec.CustomStr3 = sKey[1].Split(':')[1];//172.28.87.1/1086
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710002 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710002 () Line : | " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_5_713050(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.notice %PIX-5-713050: Group = 78.188.164.121, IP = 78.188.164.121, Connection terminated for peer 78.188.164.121.  Reason: IPSec SA Idle Timeout  Remote Proxy 172.28.104.0, Local Proxy 0.0.0.0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713050:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713050 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713050 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_714005(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715001: Group = 81.214.50.110, IP = 81.214.50.110, constructing proxy ID

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-714005:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714005 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714005 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715001(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715001: Group = 81.214.50.110, IP = 81.214.50.110, constructing proxy ID

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715001:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715001 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715001 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715006(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715006: Group = 81.214.50.110, IP = 81.214.50.110, IKE got SPI from key engine: SPI = 0x1d67616f

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715006:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715006 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715006 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715027(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715027: Group = 81.214.50.110, IP = 81.214.50.110, IPSec SA Proposal # 1, Transform # 1 acceptable  Matches global IPSec SA entry # 5

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715027:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715027 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715027 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_713066(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713066: Group = 81.214.50.110, IP = 81.214.50.110, IKE Remote Peer configured for crypto map: outside_map

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713066:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713066 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713066 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_713225(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713225: Group = 81.214.50.110, IP = 81.214.50.110, Static Crypto Map check, map outside_map, seq = 5 is a successful match

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713225:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713225 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713225 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_713035(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713035: Group = 81.214.50.110, IP = 81.214.50.110, Received remote IP Proxy Subnet data in ID Payload:   Address 172.28.2.0, Mask 255.255.255.0, Protocol 0, Port 0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713035:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713035 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713035 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_3_713122(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.error %PIX-3-713122: IP = 81.214.50.110, Keep-alives configured on but peer does not support keep-alives (type = None)
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-3-713122:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713122 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713122 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_6_113009(string args, Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-113009: AAA retrieved default group policy (DfltGrpPolicy) for user = 81.214.50.110

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-113009:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_3113009() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_113009() | Line : " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_7_715048(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715048: Group = 81.214.50.110, IP = 81.214.50.110, Send Altiga/Cisco VPN3000/Cisco ASA GW VID

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715048:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715048 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715048 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_710005(string args, Rec rec)
        {

            //172.16.200.253:514 : local7.debug %PIX-7-710005: TCP request discarded from 92.41.97.18/44252 to outside:92.45.66.150/135

            try
            {

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-710005:", "").Trim();

                if (args.Contains("from ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf("from ")).Trim();

                if (args.Contains(" from ") == true)
                    if (args.Contains(" to ") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" from ") + 6, args.IndexOf(" to ") - args.IndexOf(" from ") - 6).Trim();

                string[] sKey = args.Split(' ');

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[i].Contains("outside:") == true)
                        rec.CustomStr4 = sKey[i].Split(':')[1];//172.28.87.1/1086
                    if (sKey[i].Contains("inside:") == true)
                        rec.CustomStr3 = sKey[i].Split(':')[1];//172.28.87.1/1086
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710005 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710005 () Line : | " + args);
                return rec;
            }
        }

        private Rec LogKod_PIX_7_715076(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715076: Group = 81.214.50.110, IP = 81.214.50.110, Computing hash for ISAKMP

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715076:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715076 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715076 () | Line :  " + args);
                return rec;

            }
        }

        private Rec LogKod_PIX_7_715028(string args, Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-715028: Group = 81.214.50.110, IP = 81.214.50.110, IKE SA Proposal # 1, Transform # 1 acceptable  Matches global IKE entry # 8

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715028:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715028 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715028 () | Line :  " + args);
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

        private Rec LogKod_PIX_7_715036(string args, Rec rec)
        {
            //172.16.200.253:514 : local7.debug %PIX-7-715036: 
            //Group = hakan_mobilya, Username = hakanm, IP = 88.255.57.200, 
            //Sending keep-alive of type DPD R-U-THERE-ACK (seq number 0x47e42d70)
            try
            {

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715036:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {

                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";


                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715036 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715036 () | Line : " + args);
                return rec;
            }


        }
        private Rec LogKod_PIX_7_715075(string args, Rec rec)
        {
            //172.16.200.253:514 : local7.debug %PIX-7-715075: 
            //Group = hakan_mobilya, Username = hakanm, IP = 88.255.57.200, 
            //Received keep-alive of type DPD R-U-THERE (seq number 0x47e42d70)
            try
            {
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715075:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715075 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715075 () | Line : " + args);
                return rec;
            }


        }
        private Rec LogKod_PIX_6_302020(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.info 
                //%PIX-6-302021: 
                //Teardown ICMP connection for faddr 10.10.10.50/512 gaddr 10.1.1.57/0 laddr 10.1.1.57/0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-302020:", "").Trim();

                args = args.Replace(" for ", "*");

                rec.EventType = args.Split('*')[0];
                string[] dKey = args.Split('*')[1].Split(' ');


                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("faddr") == true)
                        rec.SourceName = dKey[i + 1];
                    if (dKey[i].Contains("gaddr") == true)
                        rec.CustomStr5 = dKey[i + 1];
                    if (dKey[i].Contains("laddr") == true)
                        rec.CustomStr6 = dKey[i + 1];
                }

                return rec;

            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302020() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302020() | " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_6_302016(string args, CustomBase.Rec rec)
        {
            try
            {


                //172.16.200.253:514 : local7.info %PIX-6-302016: 
                //Teardown UDP connection 49210428 
                //for inside:10.1.37.1/1035 to NP Identity Ifc:172.16.200.253/161 duration 0:02:01 bytes 1738

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-302016:", "").Trim();

                if (args.Contains(" for ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf(" for ")).Trim();


                string[] dKey = args.Split(' ');
                rec.EventType = dKey[0] + " " + dKey[1];
                for (int i = 0; i < dKey.Length; i++)
                {

                    if (dKey[i].Contains("inside") == true)
                    {
                        rec.CustomStr3 = dKey[i].Split(':')[1].Split('/')[0];
                        rec.CustomInt1 = Convert.ToInt32(dKey[i].Split(':')[1].Split('/')[1]);
                    }
                    if (dKey[i].Contains("outside") == true)
                    {
                        rec.CustomStr4 = dKey[i].Split(':')[1].Split('/')[0];
                        rec.CustomInt2 = Convert.ToInt32(dKey[i].Split(':')[1].Split('/')[1]);
                    }
                    if (dKey[i].Contains("Ifc:") == true)
                    {
                        rec.CustomStr7 = dKey[i].Split(':')[1].Split('/')[0];
                        rec.CustomInt5 = Convert.ToInt32(dKey[i].Split(':')[1].Split('/')[1]);
                    }
                    if (dKey[i].Contains("duration") == true)
                        rec.CustomStr10 = dKey[i + 1];
                    if (dKey[i].Contains("bytes") == true)
                        rec.CustomStr9 = dKey[i + 1];

                }

                return rec;

            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302016 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302016 () | " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_3_305005(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.error %PIX-3-305005: 
                //No translation group found for udp src outside:172.16.200.1/138 dst inside:172.16.200.255/138

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];

                rec.ComputerName = dLogParse[0];

                args = args.Split('%')[1].Replace("PIX-3-305005:", "").Trim();
                args = args.Replace(" src ", "*").Replace(" dst ", "*");

                string[] dMessage = args.Split('*');
                rec.EventType = dMessage[0];

                if (dMessage[1].Contains("outside") == true)
                {
                    rec.CustomStr4 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt2 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr6 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece
                }
                if (dMessage[2].Contains("inside") == true)
                {

                    rec.CustomStr3 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt1 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr5 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece
                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_305005() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_305005() | " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_6_106015(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-106015: 
                //Deny TCP (no connection) 
                //from 10.1.33.110/7777 to 172.16.200.25/1314 flags RST ACK  on interface inside

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-106015:", "").Trim();

                if (args.Contains("from ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf("from ")).Trim();

                if (args.Contains(" from ") == true)
                    if (args.Contains(" to ") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" from ") + 6, args.IndexOf(" to ") - args.IndexOf(" from ") - 6).Trim();

                if (args.Contains(" to ") == true)
                    if (args.Contains(" flags ") == true)
                        rec.CustomStr4 = args.Substring(args.IndexOf(" to ") + 4, args.IndexOf(" flags ") - args.IndexOf(" to ") - 4).Trim();

                if (args.Contains(" on ") == true)
                    rec.CustomStr4 = args.Substring(args.IndexOf(" on ") + 4, args.Length - args.IndexOf(" on ") - 4).Trim();

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_106015 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_106015 () | Line= " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_710006(string args, CustomBase.Rec rec)
        {
            //172.16.200.253:514 : local7.debug %PIX-7-710006: ESP request discarded from 78.188.39.118 to outside:92.45.66.150
            try
            {

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-710006:", "").Trim();

                if (args.Contains("from ") == true)
                    rec.EventType = args.Substring(0, args.IndexOf("from ")).Trim();

                if (args.Contains(" from ") == true)
                    if (args.Contains(" to ") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" from ") + 6, args.IndexOf(" to ") - args.IndexOf(" from ") - 6).Trim();

                string[] sKey = args.Split(' ');

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[1].Contains("outside:") == true)
                        rec.CustomStr4 = sKey[1].Split(':')[1];//172.28.87.1/1086
                    if (sKey[2].Contains("inside:") == true)
                        rec.CustomStr3 = sKey[1].Split(':')[1];//172.28.87.1/1086
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710006 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_710006 () Line : | " + args);
                return rec;
            }




        }
        private Rec LogKod_PIX_6_302015(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-302015: 
                //Built inbound UDP connection 49211607 for 
                //outside:10.10.10.4/54125 (10.10.10.4/54125) to 
                //inside:10.1.1.1/53 (10.1.1.1/53)



                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');

                rec.EventCategory = dLogParse[dLogParse.Length - 1];

                rec.ComputerName = dLogParse[0];

                args = args.Split('%')[1].Replace("PIX-6-302015:", "").Trim();
                args = args.Replace(" for ", "*").Replace(" to ", "*");

                string[] dMessage = args.Split('*');
                rec.EventType = dMessage[0];

                if (dMessage[1].Contains("outside") == true)
                {
                    rec.CustomStr4 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt2 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr6 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece
                }
                if (dMessage[2].Contains("inside") == true)
                {

                    rec.CustomStr3 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt1 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr5 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302015() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302015() | " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_5_335004(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-335004: NAC is disabled for host - 172.16.200.25.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-335004:", "").Trim();
                rec.Description = args;

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_335004() | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_335004() | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_5_713120(string args, CustomBase.Rec rec)
        {
            try
            {
                // 172.16.200.253:514 : local7.debug %PIX-7-713204: Group = hassan, Username = hassan, IP = 81.213.182.130, Adding static route for client address: 172.16.200.25 

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713120:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713120 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713120 () Line : | " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_713204(string args, CustomBase.Rec rec)
        {
            try
            {
                // 172.16.200.253:514 : local7.debug %PIX-7-713204: Group = hassan, Username = hassan, IP = 81.213.182.130, Adding static route for client address: 172.16.200.25 

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713204:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713204 () |" + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713204 () Line : |" + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715077(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.notice %PIX-5-713049: Group = hassan, Username = hassan, IP = 81.213.182.130,
                //Security negotiation complete for 
                //User (hassan)  Responder, Inbound SPI = 0x24d9b204, Outbound SPI = 0xf4491cfd
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715077:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715077 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715077 () Line : | " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_609001(string args, CustomBase.Rec rec)
        {
            try
            {
                // 172.16.200.253:514 : local7.debug %PIX-7-609001: Built local-host outside:172.16.200.25

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-609001:", "").Trim();

                string[] dKey = args.Split(' ');
                rec.EventType = dKey[0] + " " + dKey[1];
                for (int i = 0; i < dKey.Length; i++)
                {

                    if (dKey[i].Contains("inside") == true)
                        rec.CustomStr3 = dKey[i].Split(':')[1];
                    if (dKey[i].Contains("outside") == true)
                        rec.CustomStr4 = dKey[i].Split(':')[1];

                }

                return rec;
            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_609001 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_609001 () Line : | " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715007(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715007: Group = hassan, Username = hassan, IP = 81.213.182.130, 
                //IKE got a KEY_ADD msg for SA: SPI = 0xf4491cfd

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715007:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715007 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715007 () Line : | " + args);
                return rec;

            }
        }
        private Rec LogKod_PIX_6_602303(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info %PIX-6-602303: 
                //IPSEC: An outbound remote access SA (SPI= 0xF4491CFD) 
                //between 92.45.66.150 and 81.213.182.130 (user= hassan) has been created.
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-602303:", "").Trim();

                if (args.Contains("IPSEC:") == true)
                    if (args.Contains(" SA ") == true)
                        rec.EventType = args.Substring(args.IndexOf("IPSEC:") + 6, args.IndexOf(" SA ") - args.IndexOf("IPSEC:") - 6).Trim();

                if (args.Contains("between") == true)
                    if (args.Contains(" and ") == true)
                        rec.CustomStr4 = args.Substring(args.IndexOf("between") + 7, args.IndexOf(" and ") - args.IndexOf("between") - 7).Trim();

                if (args.Contains(" and ") == true)
                    if (args.Contains("(user=") == true)
                        rec.CustomStr3 = args.Substring(args.IndexOf(" and ") + 5, args.IndexOf("(user=") - args.IndexOf(" and ") - 5).Trim();

                if (args.Contains("(user=") == true)
                    if (args.Contains(")") == true)
                        rec.UserName = args.Substring(args.IndexOf("(user=") + 6, args.IndexOf(")", args.IndexOf("(user=")) - args.IndexOf("(user=") - 6).Trim();


                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_602303 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_602303 () Line : | " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_5_713049(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.notice %PIX-5-713049: Group = hassan, Username = hassan, IP = 81.213.182.130,
                //Security negotiation complete for 
                //User (hassan)  Responder, Inbound SPI = 0x24d9b204, Outbound SPI = 0xf4491cfd
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-5-713049:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group =") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Group (") == true)
                        rec.CustomStr8 = dKey[i].Substring(dKey[i].IndexOf('(') + 1, dKey[i].IndexOf(')') - dKey[i].IndexOf('(') - 1);
                    else
                        rec.Description += dKey[i] + " ";
                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713049 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_5_713049 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_713222(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713221: Group = hassan, Username = hassan, IP = 81.213.182.130, 
                //  Crypto Map check, checking map = outside_map, seq = 1...
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713222:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713222 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713222 () | Line : " + args);
                return rec;

            }
        }
        private Rec LogKod_PIX_7_713221(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713221: Group = hassan, Username = hassan, IP = 81.213.182.130, 
                //  Crypto Map check, checking map = outside_map, seq = 1...
                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713221:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713221 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713221 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_713034(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug %PIX-7-713034: Group = hassan, Username = hassan, IP = 81.213.182.130, Received local IP Proxy Subnet data in ID Payload:   Address 0.0.0.0, Mask 0.0.0.0, Protocol 0, Port 0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713034:", "").Trim();


                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713034 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713034 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_713025(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713236: IP = 81.213.182.130, 
                //IKE_DECODE SENDING Message (msgid=d40e0a9b) with payloads : HDR + HASH (8) + NOTIFY (11) + NONE (0) 
                //total length : 88

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713025:", "").Trim();


                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713025 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713025 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_714011(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713236: IP = 81.213.182.130, 
                //IKE_DECODE SENDING Message (msgid=d40e0a9b) with payloads : HDR + HASH (8) + NOTIFY (11) + NONE (0) 
                //total length : 88

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-714011:", "").Trim();


                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714011 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714011 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715047(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713236: IP = 81.213.182.130, 
                //IKE_DECODE SENDING Message (msgid=d40e0a9b) with payloads : HDR + HASH (8) + NOTIFY (11) + NONE (0) 
                //total length : 88

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715047:", "").Trim();


                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715047 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715047 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_713236(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713236: IP = 81.213.182.130, 
                //IKE_DECODE SENDING Message (msgid=d40e0a9b) with payloads : HDR + HASH (8) + NOTIFY (11) + NONE (0) 
                //total length : 88

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713236:", "").Trim();


                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713236 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713236 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715046(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715080: Group = hassan, Username = hassan, IP = 81.213.182.130, Starting P1 rekey timer: 82080 seconds.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715046:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715046 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715046 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715080(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715080: Group = hassan, Username = hassan, IP = 81.213.182.130, Starting P1 rekey timer: 82080 seconds.

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715080:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715080 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715080 () | Line : " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_7_713121(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713121: IP = 81.213.182.130, Keep-alive type for this connection: DPD

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713121:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713121 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713121 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_3_713119(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.error 
                //%PIX-3-713119: Group = hassan, Username = hassan, IP = 81.213.182.130, PHASE 1 COMPLETED

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-3-713119:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713119 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_3_713119 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715022(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715022: 
                //Group = hassan, Username = hassan, IP = 81.213.182.130, Resume Quick Mode processing, Cert/Trans Exch/RM DSID completed

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715022:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }
                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715022 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715022 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715021(string args, CustomBase.Rec rec)
        {

            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715021: 
                //Group = hassan, Username = hassan, IP = 81.213.182.130, Delay Quick Mode processing, Cert/Trans Exch/RM DSID in progress

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715021:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";

                }


                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715021 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715021 () | Line : " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_7_714003(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.debug 
                //%PIX-7-714003: 
                //IP = 81.213.182.130, IKE Responder starting QM: msg id = 9e623096

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-714003:", "").Trim();

                string[] dKey = args.Split(',');
                if (dKey[0].Contains("IP =") == true)
                    rec.CustomStr3 = dKey[0].Split('=')[1];

                rec.Description = dKey[1];

                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714003 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_714003 () | Line : " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_7_609002(string args, CustomBase.Rec rec)
        {
            try
            {


                //172.16.200.253:514 : local7.debug 
                //%PIX-7-609002: Teardown local-host outside:172.28.97.1 duration 0:00:15

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-609002:", "").Trim();


                string[] dKey = args.Split(' ');
                rec.EventType = dKey[0] + " " + dKey[1];
                for (int i = 0; i < dKey.Length; i++)
                {

                    if (dKey[i].Contains("inside") == true)
                        rec.CustomStr3 = dKey[i].Split(':')[1];
                    if (dKey[i].Contains("outside") == true)
                        rec.CustomStr4 = dKey[i].Split(':')[1];
                    if (dKey[i].Contains("duration") == true)
                        rec.CustomStr10 = dKey[i + 1];

                }

                return rec;
            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_609002 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_609002 () | Line : " + args);
                return rec;
            }



        }
        private Rec LogKod_PIX_6_302021(string args, CustomBase.Rec rec)
        {
            try
            {

                //172.16.200.253:514 : local7.info 
                //%PIX-6-302021: 
                //Teardown ICMP connection for faddr 10.10.10.50/512 gaddr 10.1.1.57/0 laddr 10.1.1.57/0

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-302021:", "").Trim();

                rec.EventType = args.Replace(" for ", "*").Split('*')[0];
                string[] dKey = args.Replace(" for ", "*").Split('*')[1].Split(' ');


                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("faddr") == true)
                        rec.SourceName = dKey[i + 1];
                    if (dKey[i].Contains("gaddr") == true)
                        rec.CustomStr5 = dKey[i + 1];
                    if (dKey[i].Contains("laddr") == true)
                        rec.CustomStr6 = dKey[i + 1];
                }

                return rec;

            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302021 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302021 () | Line : " + args);
                return rec;
            }

        }
        private Rec LogKod_PIX_7_713906(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-713906: 
                //Group = hassan, Username = hassan, IP = 81.213.182.130, 
                //Obtained IP addr (172.16.200.25) prior to initiating Mode Cfg (XAuth enabled)

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-713906:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[i] + " ";
                }


                return rec;

            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713906 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_713906 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_6_713184(string args, CustomBase.Rec rec)
        {

            try
            {

                //172.16.200.253:514 : local7.info 
                //%PIX-6-713184: 
                //Group = hassan, Username = hassan, IP = 81.213.182.130, Client Type: WinNT  Client Application Version: 5.0.06.0160

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-713184:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description = dKey[dKey.Length - 1]; ;
                }


                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713184 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_713184 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_7_715053(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.debug 
                //%PIX-7-715053: 
                //Group = hassan, Username = hassan, IP = 81.213.182.130, MODE_CFG: Received request for backup ip-sec peer list!

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-7-715053:", "").Trim();

                string[] dKey = args.Split(',');
                for (int i = 0; i < dKey.Length; i++)
                {
                    if (dKey[i].Contains("Group") == true)
                        rec.CustomStr8 = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("Username") == true)
                        rec.UserName = dKey[i].Split('=')[1];
                    else if (dKey[i].Contains("IP =") == true)
                        rec.CustomStr3 = dKey[i].Split('=')[1];
                    else
                        rec.Description += dKey[dKey.Length - 1] + " ";
                }

                return rec;
            }
            catch (Exception exp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715053 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_7_715053 () | Line : " + args);
                return rec;
            }



        }
        private Rec LogKod_PIX_6_302013(string args, CustomBase.Rec rec)
        {
            try
            {
                //172.16.200.253:514 : local7.info 
                //%PIX-6-302013: 
                //Built inbound TCP connection 49211668 for 
                //outside:172.28.87.1/1086 (172.28.87.1/1086) to 
                //inside:10.1.33.110/7777 (10.1.33.110/7777)



                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');

                rec.EventCategory = dLogParse[dLogParse.Length - 1];

                rec.ComputerName = dLogParse[0];

                args = args.Split('%')[1].Replace("PIX-6-302013:", "").Trim();
                args = args.Replace(" for ", "*").Replace(" to ", "*");

                string[] dMessage = args.Split('*');

                rec.EventType = dMessage[0];

                if (dMessage[1].Contains("outside") == true)
                {

                    rec.CustomStr4 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt2 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr6 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece

                }
                if (dMessage[2].Contains("inside") == true)
                {

                    rec.CustomStr3 = dMessage[1].Split(':')[1].Split('(')[0].Split('/')[0];//172.28.87.1/1086
                    rec.CustomInt1 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('(')[0].Split('/')[1]);//10.1.33.110/7777
                    rec.CustomStr5 = dMessage[1].Substring(dMessage[1].IndexOf('(') + 1, dMessage[1].IndexOf(')') - dMessage[1].IndexOf('(') - 1);//(172.28.87.1/1086) parantezi  ici sadece

                }


                return rec;
            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302013 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302013 () | Line : " + args);
                return rec;
            }
        }
        private Rec LogKod_PIX_6_302014(string args, CustomBase.Rec rec)
        {

            try
            {
                //gelen log formatý
                //172.16.200.253:514 : local7.info  
                //%PIX-6-302014:
                //Teardown TCP connection 49211250 for 
                //outside:172.28.60.1/1128 to 
                //inside:10.1.33.110/7777 duration 0:00:28 bytes 23897 TCP FINs

                string[] dLogParse = args.Split('%')[0].Trim().Split(' ');
                rec.EventCategory = dLogParse[dLogParse.Length - 1];
                rec.ComputerName = dLogParse[0];
                args = args.Split('%')[1].Replace("PIX-6-302014:", "").Trim();
                args = args.Replace(" for ", "*").Replace(" to ", "*");

                string[] dMessage = args.Split('*');

                rec.EventType = dMessage[0];

                if (dMessage[1].Contains("outside") == true)
                {
                    rec.CustomStr4 = dMessage[1].Split(':')[1].Split('/')[0];//172.28.60.1
                    rec.CustomInt2 = Convert.ToInt32(dMessage[1].Split(':')[1].Split('/')[1]);//1128
                }

                if (dMessage.Length > 2)
                {
                    string[] Tlist = dMessage[2].Split(' ');
                    for (int i = 0; i < Tlist.Length; i++)
                    {
                        if (Tlist[i].Contains("inside") == true)
                        {
                            rec.CustomStr3 = Tlist[i].Split(':')[1].Split('/')[0];
                            rec.CustomInt1 = Convert.ToInt32(Tlist[i].Split(':')[1].Split('/')[1]);
                        }
                        if (Tlist[i].Contains("duration") == true)
                            rec.CustomStr10 = Tlist[i + 1];
                        if (Tlist[i].Contains("bytes") == true)
                            rec.CustomStr9 = Tlist[i + 1];

                    }

                }
                return rec;
            }
            catch (Exception exp)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302014 () | " + exp.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "LogKod_PIX_6_302014 () | Line : " + args);
                return rec;
            }







        }

    }


}

