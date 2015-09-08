using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace ClavisterRecorder
{
    public class ClavisterRecorder : CustomBase
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\ClavisterRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ClavisterRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }   
        
        public ClavisterRecorder()
        {
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            CustomServiceBase s = null;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Start preparing record");
                rec.LogName = "Clavister Recorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;

                String[] arr = args.Message.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " ClavisterRecorder -->> Message is null " + args.Message);
                    return;
                }
                if (arr.Length < 6)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ClavisterRecorder -->> Error parsing message for lenght is small than 6 : " + args.Message);

                }

                string typeofLog = "";
                string date = "";

                try
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].StartsWith("["))
                        {
                            date = arr[i].TrimStart('[').Trim() + " " + arr[i + 1].TrimEnd(']').Trim();
                            rec.Datetime = Convert.ToDateTime(date, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

                        }
                        if ((arr[i].Contains("Local") || arr[i].Contains("local")) && arr[i].Contains("."))
                        {
                            typeofLog = arr[i].Split('.')[1].Trim();
                            rec.CustomStr1 = typeofLog;
                        }
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ClavisterRecorder -->> Type belirlenen yerde hata : " + args.Message);
                }

                try
                {
                    switch (typeofLog)
                    {
                        #region Info

                        case "Info":
                        case "info":

                            //2010-12-15 15:00:25	Local0.Info	192.168.20.254	
                            //[2010-12-15 14:56:37] EFW: CONN: prio=1 id=00600005 rev=1 event=conn_close_natsat action=close 
                            //rule=local_to_wan_http_alg conn=close connipproto=TCP connrecvif=core connsrcip=92.45.16.122 
                            //connsrcport=19920 conndestif=wan conndestip=81.8.63.21 conndestport=80 connnewsrcip=92.45.16.122 
                            //connnewsrcport=19920 connnewdestip=81.8.63.21 connnewdestport=80 origsent=1106 termsent=920

                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Type is Info Başladı. Line: " + args.Message);

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("prio"))
                                {

                                }
                                else if (arr[i].StartsWith("id"))
                                {
                                    rec.EventId = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i] == "rev")
                                {

                                }
                                else if (arr[i].StartsWith("event"))
                                {
                                    rec.EventCategory = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("action"))
                                {
                                    rec.CustomStr2 = arr[i].Split('=')[1].Trim().Trim('"');
                                }
                                else if (arr[i].StartsWith("rule"))
                                {
                                    rec.CustomStr10 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i] == "conn")
                                {
                                    rec.CustomStr7 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connipproto"))
                                {
                                    rec.CustomStr9 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connrecvif"))
                                {

                                }
                                else if (arr[i].StartsWith("connsrcip"))
                                {
                                    rec.CustomStr3 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connsrcport"))
                                {
                                    rec.CustomInt1 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("conndestif"))
                                {
                                    rec.CustomStr8 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("conndestip"))
                                {
                                    rec.CustomStr4 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("conndestport"))
                                {
                                    rec.CustomInt2 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("connnewsrcip"))
                                {
                                    rec.CustomStr5 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connnewsrcport"))
                                {
                                    rec.CustomInt5 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("connnewdestip"))
                                {
                                    rec.CustomStr6 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connnewdestport"))
                                {
                                    rec.CustomInt6 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("origsent"))
                                {
                                    rec.CustomInt3 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("termsent"))
                                {
                                    rec.CustomInt4 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Log Type is Info Bitti.");

                            break;

                        #endregion

                        #region Notice

                        case "Notice":
                        case "notice":

                            // 2010-12-15 15:00:25	Local0.Notice	192.168.20.254	[2010-12-15 14:56:37] 
                            //EFW: ALG: prio=2 id=00200125 rev=2 event=request_url action=allow categories="Business oriented"
                            //  audit=off override=no connipproto=TCP connrecvif=wan connsrcip=88.249.168.180 connsrcport=52047
                            //  conndestif=core conndestip=92.45.16.135 conndestport=80 origsent=419 termsent=84
                            // url="www.turkak.org.tr/favicon.ico" algname=local_local_alg algmod=http algsesid=100330

                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Type is Notice Başladı. Line: " + args.Message);

                            for (int i = 0; i < arr.Length; i++)
                            {

                                if (arr[i].StartsWith("prio"))
                                {

                                }
                                else if (arr[i].StartsWith("id"))
                                {
                                    rec.EventId = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i] == "rev")
                                {

                                }
                                else if (arr[i].StartsWith("event"))
                                {
                                    rec.EventCategory = arr[i].Split('=')[1].TrimStart('"');
                                }
                                else if (arr[i].StartsWith("action"))
                                {
                                    rec.CustomStr2 = arr[i].Split('=')[1].Trim();
                                    for (int j = i + 1; j < arr.Length; j++)
                                    {
                                        if (arr[j].Contains("="))
                                        {
                                            break;
                                        }
                                        rec.CustomStr2 += " " + arr[j].Trim('"');
                                    }
                                    rec.CustomStr2 = rec.CustomStr2.Trim().Trim('"');
                                }
                                else if (arr[i].StartsWith("categories"))
                                {
                                    rec.CustomStr7 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("audit"))
                                {

                                }
                                else if (arr[i].StartsWith("categories"))
                                {

                                }
                                else if (arr[i].StartsWith("override"))
                                {

                                }
                                else if (arr[i].StartsWith("connipproto"))
                                {
                                    rec.CustomStr9 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connrecvif"))
                                {

                                }
                                else if (arr[i].StartsWith("connsrcip"))
                                {
                                    rec.CustomStr3 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connsrcport"))
                                {
                                    rec.CustomInt1 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                if (arr[i].StartsWith("conndestip"))
                                {
                                    rec.CustomStr4 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("conndestport"))
                                {
                                    rec.CustomInt2 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("origsent"))
                                {
                                    rec.CustomInt3 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("termsent"))
                                {
                                    rec.CustomInt4 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("url"))
                                {
                                    rec.CustomStr8 = arr[i].Split('=')[1].TrimStart('"').TrimEnd('"');
                                }
                                else if (arr[i].StartsWith("algname"))
                                {

                                }
                                else if (arr[i].StartsWith("algmod"))
                                {

                                }
                                else if (arr[i].StartsWith("algsesid"))
                                {

                                }
                            }
                             L.Log(LogType.FILE, LogLevel.DEBUG, "Log Type is Notice Bitti.");

                            break;

                        #endregion

                        #region Warning

                        case "Warning":
                        case "warning":

                            //2010-12-15 15:00:25	Local0.Warning	192.168.20.254	[2010-12-15 14:56:36] 
                            //EFW: RULE: prio=3 id=06000051 rev=1 event=ruleset_drop_packet action=drop rule=Default_Rule 
                            //recvif=lan srcip=192.168.200.170 destip=85.111.2.7 ipproto=ICMP ipdatalen=64 icmptype=ECHO_REQUEST 
                            //echoid=2714 echoseq=19

                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Type is Warning Başladı. Line: " + args.Message);

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("prio"))
                                {

                                }
                                else if (arr[i].StartsWith("id"))
                                {
                                    rec.EventId = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i] == "rev")
                                {

                                }
                                else if (arr[i].StartsWith("event"))
                                {
                                    rec.EventCategory = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("action"))
                                {
                                    rec.CustomStr2 = arr[i].Split('=')[1].Trim().Trim('"');
                                }
                                else if (arr[i].StartsWith("rule"))
                                {

                                }
                                else if (arr[i].StartsWith("recvif"))
                                {
                                    rec.CustomStr8 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("srcip"))
                                {
                                    rec.CustomStr3 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("destip"))
                                {
                                    rec.CustomStr4 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("ipproto"))
                                {
                                    rec.CustomStr9 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("ipdatalen"))
                                {
                                    rec.CustomInt6 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("icmptype"))
                                {


                                }
                                else if (arr[i].StartsWith("echoid"))
                                {

                                }
                                else if (arr[i].StartsWith("echoseq"))
                                {

                                }
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Log Type is Warning Bitti.");

                            break;

                        #endregion

                        #region Debug

                        case "Debug":
                        case "debug":

                            //2010-12-15 15:00:17	Local0.Debug	192.168.20.254	[2010-12-15 14:56:29]
                            //EFW: TCP_FLAG: prio=0 id=03300016 rev=2 event=tcp_seqno_too_low action=drop seqno=1248733455 
                            //accstart=1248743675 accend=1248750163 rule=TCPSequenceNumbers connipproto=TCP connrecvif=core 
                            //connsrcip=92.45.16.122 connsrcport=10394 conndestif=wan conndestip=74.125.108.25 conndestport=80
                            //origsent=2630483 termsent=98502145 recvif=wan srcip=74.125.108.25 destip=92.45.16.122 ipproto=TCP 
                            //ipdatalen=1480 srcport=80 destport=10394 ack=1 psh=1

                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Type is Debug Başladı. Line: " + args.Message);

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("prio"))
                                {

                                }
                                else if (arr[i].StartsWith("id"))
                                {
                                    rec.EventId = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i] == "rev")
                                {

                                }
                                else if (arr[i].StartsWith("event"))
                                {
                                    rec.EventCategory = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("action"))
                                {
                                    rec.CustomStr2 = arr[i].Split('=')[1].Trim().Trim('"');
                                }
                                else if (arr[i].StartsWith("seqno"))
                                {

                                }
                                else if (arr[i].StartsWith("audit"))
                                {

                                }
                                else if (arr[i].StartsWith("rule"))
                                {

                                }
                                else if (arr[i].StartsWith("override"))
                                {

                                }
                                else if (arr[i].StartsWith("connipproto"))
                                {
                                    rec.CustomStr9 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connrecvif"))
                                {
                                    rec.CustomStr8 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connsrcip"))
                                {
                                    rec.CustomStr3 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("connsrcport"))
                                {
                                    rec.CustomInt1 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("conndestip"))
                                {
                                    rec.CustomStr4 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("conndestport"))
                                {
                                    rec.CustomInt2 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("origsent"))
                                {
                                    rec.CustomInt4 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("termsent"))
                                {
                                    rec.CustomInt5 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("ipdatalen"))
                                {
                                    rec.CustomInt3 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                }
                                else if (arr[i].StartsWith("recvif"))
                                {
                                    rec.CustomStr7 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("srcip"))
                                {
                                    rec.CustomStr5 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("destip"))
                                {
                                    rec.CustomStr6 = arr[i].Split('=')[1].Trim();
                                }
                                else if (arr[i].StartsWith("conndestif"))
                                {
                                    rec.CustomStr10 = arr[i].Split('=')[1].Trim();
                                }
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Log Type is Debug Bitti.");

                            break;

                        #endregion

                        #region Error

                        case "Error":
                        case "error":
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Type is Error Başladı. Line: " + args.Message);

                            //2010-12-15 15:00:35	Local0.Error	192.168.20.254	[2010-12-15 14:56:47] 
                            //EFW: REASSEMBLY: prio=4 id=04800004 rev=1 event=mismatching_data_in_overlapping_tcp_segment 
                            //action=correct the data connipproto=TCP connrecvif=core connsrcip=85.102.128.208 
                            //connsrcport=2444 conndestif=dmz conndestip=92.45.16.135 conndestport=80 origsent=5208 termsent=150742

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("prio"))
                                {

                                }
                                else
                                    if (arr[i].StartsWith("id"))
                                    {
                                        rec.EventId = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                                    else if (arr[i] == "rev")
                                    {

                                    }
                                    else if (arr[i].StartsWith("event"))
                                    {
                                        rec.EventCategory = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("action"))
                                    {
                                        rec.CustomStr2 = arr[i].Split('=')[1].Trim();
                                        for (int j = i + 1; j < arr.Length; j++)
                                        {
                                            if (arr[j].Contains("="))
                                            {
                                                break;
                                            }
                                            rec.CustomStr2 += " " + arr[j].Trim();
                                        }

                                        rec.CustomStr2 = rec.CustomStr2.Trim().Trim('"');
                                    }
                                    else if (arr[i].StartsWith("categories"))
                                    {
                                        rec.CustomStr7 = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("audit"))
                                    {

                                    }
                                    else if (arr[i].StartsWith("rule"))
                                    {

                                    }
                                    else if (arr[i].StartsWith("override"))
                                    {

                                    }
                                    else if (arr[i].StartsWith("connipproto"))
                                    {
                                        rec.CustomStr9 = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("connrecvif"))
                                    {
                                        rec.CustomStr8 = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("connsrcip"))
                                    {
                                        rec.CustomStr3 = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("connsrcport"))
                                    {
                                        rec.CustomInt1 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                                    else if (arr[i].StartsWith("conndestip"))
                                    {
                                        rec.CustomStr4 = arr[i].Split('=')[1].Trim();
                                    }
                                    else if (arr[i].StartsWith("conndestport"))
                                    {
                                        rec.CustomInt2 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                                    else if (arr[i].StartsWith("origsent"))
                                    {
                                        rec.CustomInt3 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                                    else if (arr[i].StartsWith("termsent"))
                                    {
                                        rec.CustomInt4 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                                    else if (arr[i].StartsWith("conndestif"))
                                    {
                                        rec.CustomInt10 = Convert_To_Int32(arr[i].Split('=')[1].Trim());
                                    }
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Log Type is Error Bitti.");

                            break;

                        #endregion

                        #region Default
                        default:
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Different type of message. We did not parse it. Line: " + args.Message);
                            break;
                        #endregion

                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Switch bitti.");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ClavisterRecorder -->> Switch içinde parsing hatası. Message : " + ex.ToString());
                    L.Log(LogType.FILE, LogLevel.ERROR, " ClavisterRecorder -->> Switch içinde parsing hatası. Line : " + args.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Log Parçalandı. Şimdi gönderilecek.");
                if (usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Rec gönderilemek üzere 'usingRegistry == true' içine girdi.");
                    s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> slog_SyslogEvent() usingRegistry | Finish Sending Data");
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> Rec gönderilemek üzere 'usingRegistry != true' içine girdi.");
                    s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime.TrimEnd(':'), "", "", "", rec.Datetime.TrimEnd(':'));
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ClavisterRecorder -->> slog_SyslogEvent() | Finish Sending Data");
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " ClavisterRecorder -->> En dışdaki parsing hatası.");
                L.Log(LogType.FILE, LogLevel.ERROR, ex.ToString());
            }
            finally
            {
                s.Dispose();
            }
        }

        private int Convert_To_Int32(string strValue)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Convert işlemi gerçekleşiyo.");
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

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\ClavisterRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ClavisterRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ClavisterRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ClavisterRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager ClavisterRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
