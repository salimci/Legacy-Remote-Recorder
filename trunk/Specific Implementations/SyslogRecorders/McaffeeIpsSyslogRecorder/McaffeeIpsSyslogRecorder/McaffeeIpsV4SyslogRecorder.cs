/*
 * AUTHOR: EMIN KARACA
 * emin.karaca@gmail.com
 * */
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

namespace McaffeeIpsV4SyslogRecorder
{
    public class McaffeeIpsV4SyslogRecorder : CustomBase
    {

        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514,zone=0;
        private string err_log, remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private string protocol;

        private bool usingRegistry = true;
        private string virtualHost,Dal;
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
                    EventLog.WriteEntry("Security Manager McaffeeIpsV4SyslogRecorder Read Registry", "McaffeeIpsV4SyslogRecorder may not working properly ", EventLogEntryType.Error);
                    return;
                }
            }
            else {
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\McaffeeIpsV4SyslogRecorder" + identity + ".log";
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
            String CustomVar1, Int32 CustomVar2, String virtualhost,String dal,Int32 Zone)
        { 
            
            trc_level = TraceLevel;
            usingRegistry = false;
            virtualHost= virtualhost;
            identity = Identity;
            Dal = dal;           
            location = Location;
            if (location.Contains(':'.ToString()))
            {
                String[] parse = location.Split(':');
                protocol = parse[0];
                Syslog_Port = Convert.ToInt32(parse[1]) ;
            }
            else {
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
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  McaffeeIpsV4SyslogRecorder");

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening McaffeeIpsV4Syslog on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

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
                EventLog.WriteEntry("Security Manager McaffeeIpsV4SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
        
        }

        public McaffeeIpsV4SyslogRecorder()
        {

		}

        void errorMessage(ref CustomBase.Rec r, String[] faultType, String[] fault)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "message is error type");
            String[] faultType2 = fault[0].Split(':');
            if (usingRegistry)
            {
                r.LogName = "Mcaffee Ips Syslog Recorder";
            }
            else
            {
                r.LogName = "Mcaffee Ips Syslog Recorder "+identity.ToString();
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "set admin domain");
           // String[] adDomain = faultType[3].Split(':');

            L.Log(LogType.FILE, LogLevel.DEBUG, "set admin domain2");
            r.ComputerName = faultType2[5].TrimStart().Trim('&', '&');    //adminDomain
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faultname");
            r.EventCategory = fault[1];                                 //faultName
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faulttype");
            r.EventType = fault[2];                                     //faultTypes 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set ownerid");
            if (fault[3].TrimEnd(' ').TrimStart(' ') == "N/A")
                r.CustomInt1 = 0;
            else
            r.CustomInt1 = Convert.ToInt32(fault[3]);                   //ownerId
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set ownername");
            r.CustomStr3 = fault[4];                                      //ownerName 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faultlvl");
            r.CustomStr1 = fault[5];                                    //faultLvl 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faulttime");
            String[] tarih = fault[6].Split(' ');
            String[] gun = tarih[0].Split('-');
            string dates = gun[0] + "/" + gun[1] + "/" + gun[2] + " " + tarih[1];
            r.Datetime = Convert.ToDateTime(dates).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");     //faultTime
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faultsource");
            r.CustomStr5 = fault[7];                                   //faultSource
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set faultcomponent");
            r.CustomStr2 = fault[8];                                   //faultComponent 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set severity");
            r.CustomStr3 = fault[9];                                   //severity 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set description");
            r.Description = fault[10];                                  ///description 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set ackinfo");
            r.CustomStr6 = fault[11];                                   //ackInformation

        }

        void alertMessage(ref CustomBase.Rec r, String[] fault)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "message is alert type");
            r.LogName = "Mcaffee Ips Syslog Recorder";
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set alertid");
            if (fault[1].TrimEnd(' ').TrimStart(' ') == "N/A")
                r.EventId = 0;
            else
            r.EventId = Convert.ToInt64(fault[1]);                              //alertId
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set alerttype");
            r.EventType = fault[2];                                             //alertType
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set alerttime");
            String[] tarih = fault[3].Split(' ');
            String[] gun = tarih[0].Split('-');
            string dates = gun[0] + "/" + gun[1] + "/" + gun[2] + " " + tarih[1]; 
            r.Datetime  = dates;                                             //alerttime
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set alertname");
            r.CustomStr10 = fault[4];                                          //alertName
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set attackid");
            r.CustomStr1 = fault[5];                                           //attackId 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set attackseverity");
            r.CustomStr2 = fault[6];                                           //attackSeverity 
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set appprotocol");
            r.CustomStr9 = fault[18];                                           //applicationProtocol
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set interface");
            r.CustomStr6 = fault[8];                                           //interfaces
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set sourceip");
            if (fault[9].Trim(' ') == "N/A")
                r.CustomStr5 = "N/A";
            else
                r.CustomStr5 = System.Net.IPAddress.Parse(fault[9]).ToString(); //sourceIp

            L.Log(LogType.FILE, LogLevel.DEBUG, "set sourceport");
            if (fault[10].TrimEnd(' ').TrimStart(' ') == "N/A")
                r.CustomInt1 = 0;
            else
            r.CustomInt1 = Convert.ToInt32(fault[10]);                          //sourcePort
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set destinationip");
            if (fault[11].Trim(' ') == "N/A")
                r.CustomStr9 = "N/A";
             else
                r.CustomStr9 = System.Net.IPAddress.Parse(fault[11]).ToString();//destinationIp    
            

            L.Log(LogType.FILE, LogLevel.DEBUG, "set destport");
            if (fault[12].TrimEnd(' ').TrimStart(' ') == "N/A")
                r.CustomInt2 = 0;
            else
            r.CustomInt2 = Convert.ToInt32(fault[12]);                          //destinationport
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set category");
            r.EventCategory = fault[13];                                        //category
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set subcategory");
            r.CustomStr4 = fault[14];                                           //subcategory
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set direction");
            r.CustomStr7 = fault[15];                                           //direction
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set resultstatus");
            r.CustomStr8 = fault[16];                                           //resultStatus


            L.Log(LogType.FILE, LogLevel.DEBUG, "set sensorname");
            r.CustomStr9 = fault[7];                                             //sensorName

            L.Log(LogType.FILE, LogLevel.DEBUG, "set ntwrkprotocol");
            r.Description = fault[19];                                           //networkProtocol
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set siplit last part");

            String[] rest = fault[20].Replace("//", "|").Replace("\0", " ").Split('|'); 
          
            
            L.Log(LogType.FILE, LogLevel.DEBUG, "set computername");
            r.ComputerName = rest[2];                                           //adminDomain
            

            L.Log(LogType.FILE, LogLevel.DEBUG, "set description");
            r.Description += "attacksignature: " + rest[0];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set attacksignature");
            r.Description += " \n attackconfidence: " + rest[1];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set attackconfidence");
            r.Description += "\n quarantineEndTime: " + rest[3];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set quarantineEndTime");
            r.Description += "\n remediationEndTime: " + rest[4];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set remediationEndTime");
            r.Description += "\n mCAFEENacForwardedStatus: " + rest[5];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set mCAFEENacForwardedStatus");
            r.Description += "\n mcaffeeNacManagedStatus" + rest[6];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set mcaffeeNacManagedStatus");
            r.Description += "\n mcaffeeNacErrorStatus" + rest[7];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set mcaffeeNacErrorStatus");
            r.Description += "\n mcaffeeNacActionStatus" + rest[8];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set mcaffeeNacActionStatus");
            r.Description += "\n detectionmechanism: " + fault[17];
            L.Log(LogType.FILE, LogLevel.DEBUG, "set detectionmechanism");
            
        }

        static String[] SpaceSplit(String line, bool useTabs)
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


        void Mcaffee_Syslog(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "method is called");
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

                

                L.Log(LogType.FILE, LogLevel.DEBUG, "log is: " + args.Message.Replace('\0', ' ') );

                String[] fault = args.Message.Replace('\0', ' ').Replace("||", "|").Split('|');
                L.Log(LogType.FILE, LogLevel.DEBUG, "split mesaage phase 1");

                String[] faultType = fault[0].Split(' ');
                L.Log(LogType.FILE, LogLevel.DEBUG, "split mesaage phase 2");

                String[] arr = SpaceSplit(args.Message, true);

                    if (faultType.Length < 3) {
                        //r.Description = args.Message;
                        L.Log(LogType.FILE, LogLevel.ERROR, "Bad string :" + args.Message);
                       
                    }
                    else
                        if (faultType[2] == "local0.critical")
                        alertMessage(ref r, fault);
                    else
                        if (faultType[2] == "security.emergency")
                            errorMessage(ref r, faultType, fault);
                        else
                            if (faultType[2] == "local0.alert")
                                alertMessage(ref r, fault);
                            else
                                if (faultType[2] == "local0.warning")
                                    alertMessage(ref r, fault);
                                else
                                    if (faultType[2].ToLower() == "mail.info")
                                        alertMailInfo(ref r, fault);
                                    else
                                        if (faultType[2].ToLower() == "user.error")
                                        {
                                            r.LogName = "Mcaffee2 Ips Syslog Recorder";
                                            r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss").Replace(".", "/");
                                            r.EventCategory = arr[2];
                                            r.Description = args.Message.Replace('\0', ' ');
                                        }
                                        else
                                            if (faultType[2].ToLower() == "user.notice")
                                            {
                                                r.LogName = "Mcaffee2 Ips Syslog Recorder";
                                                r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss").Replace(".", "/");
                                                r.EventCategory = arr[2];
                                                r.Description = args.Message.Replace('\0', ' ');
                                            }
                                            else
                                                {
                                                    r.LogName = "Mcaffee2 Ips Syslog Recorder";
                                                    r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss").Replace(".", "/");
                                                    //r.Description =  args.Message;
                                                    //unknown
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr : " + arr);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 0: " + arr[0]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 1: " + arr[1]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 2: " + arr[2]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 3: " + arr[3]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 4: " + arr[4]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 5: " + arr[5]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 6: " + arr[6]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 7: " + arr[7]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 8: " + arr[8]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 9: " + arr[9]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 10: " + arr[10]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 11: " + arr[11]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 12: " + arr[12]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 13: " + arr[13]);
                                                    //L.Log(LogType.FILE, LogLevel.INFORM, "arr 14: " + arr[14]); 
                                                    r.SourceName = arr[0];
                                                    r.EventType = arr[9] + " " + arr[10];
                                                    if (arr[11] == "Host" && arr[12] == "Sweep")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12];
                                                        //
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "host sweep : " + arr);
                                                        //
                                                        prsfunc(2, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Microsoft" && arr[12] == "SMTP" && arr[13] == "Service" && arr[14] == "DNS" && arr[15] == "resolver" && arr[16] == "overflow")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14] + " " + arr[15] + " " + arr[16];
                                                        prsfunc(6, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Mail" && arr[12] == "Relay" && arr[13] == "Attempt")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "ACK" && arr[12] == "Port" && arr[13] == "Scan")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Bad" && arr[12] == "State" && arr[13] == "Transition")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Possible" && arr[12] == "Worm" && arr[13] == "Detected" && arr[14] == "in" && arr[15] == "Attachment")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14] + " " + arr[15];
                                                        prsfunc(5, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Full-Connect" && arr[12] == "Host" && arr[13] == "Sweep")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Port" && arr[12] == "Scan")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12];
                                                        prsfunc(2, ref r, arr);
                                                    }
                                                    else if (arr[11] == "SYN" && arr[12] == "Host" && arr[13] == "Sweep")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Nachi" && arr[12] == "Worm" && arr[13] == "Host" && arr[14] == "Sweep")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
                                                        prsfunc(4, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Nachi-like" && arr[12] == "Ping")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12];
                                                        prsfunc(2, ref r, arr);
                                                    }
                                                    else if (arr[11] == "W32/Netsky@MM" && arr[12] == "Worm")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12];
                                                        prsfunc(2, ref r, arr);
                                                    }
                                                    else if (arr[11] == "XMAS" && arr[12] == "with" && arr[13] == "SYN" && arr[14] == "Probe")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
                                                        prsfunc(4, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Illegal" && arr[12] == "FIN" && arr[13] == "Probe")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Suspicious" && arr[12] == ".Lnk" && arr[13] == "Attachment" && arr[14] == "Found")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
                                                        prsfunc(4, ref r, arr);
                                                    }
                                                    else if (arr[11] == "BitTorrent" && arr[12] == "Meta-Info" && arr[13] == "Retrieving")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[10] == "ICMP:Destination")
                                                    {
                                                        r.EventCategory = "Destination Unreachable DOS";
                                                        prsfunc(2, ref r, arr);
                                                    }
                                                    else if (arr[11] == "SQL" && arr[12] == "Injection" && arr[13] == "Exploit" && arr[14] == "II")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
                                                        //
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "host sweep : " + arr);
                                                        //
                                                        prsfunc(4, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Bad" && arr[12] == "State" && arr[13] == "Transition")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13];
                                                        //
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "host sweep : " + arr);
                                                        //
                                                        prsfunc(3, ref r, arr);
                                                    }
                                                    else if (arr[11] == "Attempt" && arr[12] == "to" && arr[13] == "Read" && arr[14] == "Password" && arr[15] == "File")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14] + " " + arr[15];
                                                        //
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "host sweep : " + arr);
                                                        //
                                                        prsfunc(5, ref r, arr);
                                                    }
                                                    else if (arr[11] == "XMLRPC" && arr[12] == "Remote" && arr[13] == "Code" && arr[14] == "Execution")
                                                    {
                                                        r.EventCategory = arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
                                                        //
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "host sweep : " + arr);
                                                        //
                                                        prsfunc(4, ref r, arr);
                                                    }
                                                    else
                                                    {
                                                        L.Log(LogType.FILE, LogLevel.INFORM, "problem log is: " + args.Message.Replace('\0', ' '));
                                                        //L.Log(LogType.FILE, LogLevel.ERROR, "new message format : " + arr);
                                                        //L.Log(LogType.FILE, LogLevel.INFORM, "new message format : " + arr);
                                                        r.Description += arr;
                                                    }
                                                }

                //sending data
                //r.SourceName = args.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

                if (usingRegistry)
                {
                    s.SetData(r);
                }
                else
                {
                    s.SetData(Dal,virtualHost, r);
                    s.SetReg(identity, r.Datetime,"", "", "",r.Datetime);
                }


                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }//end of try
            catch (Exception ex)
            {

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at parsing log: " + args.Message +"\n"+ ex.ToString());
            }
            finally
            {
                s.Dispose();
            }

        }
        void prsfunc(int a, ref CustomBase.Rec r, String[] arr2)
        {
            //
            //L.Log(LogType.FILE, LogLevel.INFORM, "arr2 : " + arr2);
            //L.Log(LogType.FILE, LogLevel.INFORM, "a : " + a);
            //
            String last = "";
            r.CustomStr5 = arr2[11 + a];
            r.CustomStr1 = arr2[12 + a];
            r.CustomStr2 = arr2[13 + a];
            r.CustomStr3 = arr2[14 + a];
            r.CustomStr4 = arr2[15 + a];

            for (int i = 16 + a; i < arr2.Length; i++)
            {
                last += arr2[i] + " ";
            }

            r.Description = last;
        }
        static void alertMailInfo(ref CustomBase.Rec r, String[] fault)
        {
            r.LogName = "Mcaffee2 Ips Syslog Recorder";

            String[] pars = fault[0].Split('\'');
            String[] parss = fault[0].Split(',');
            String[] par = SpaceSplit(fault[0], true);
            r.CustomStr1 = pars[3];         //status
            r.ComputerName = par[0];
            r.EventCategory = par[2];
            r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss").Replace(".", "/");
            for (int i = 0; i < parss.Length; i++)
            {
                if (parss[i].Contains("="))
                {
                    string[] bol = parss[i].Split('=');
                    bol[0] = bol[0].Trim(' ');
                    if (bol[0].Contains("Application"))
                        bol[0] = "application";
                    bol[0] = bol[0].ToLower();
                    switch (bol[0])
                    {
                        case "application":
                            {
                                r.CustomStr2 = bol[1].Trim(',');
                            } break;
                        case "event":
                            {
                                r.EventType = bol[1];
                            } break;
                        case "source":
                            {
                                if (bol[1].Contains("("))
                                    bol[1] = bol[1].Trim('(');
                                if (bol[1].Contains(")"))
                                    bol[1] = bol[1].Trim(')');
                                if (bol[1].Contains(","))
                                    bol[1] = bol[1].Trim(',');
                                r.SourceName = bol[1];
                            } break;
                        case "msgid":
                            {
                                r.CustomStr9 = bol[1].TrimEnd(',');
                            } break;
                        case "convid":
                            {
                                r.CustomStr6 = bol[1].TrimEnd(',');
                            } break;
                        case "relay":
                            {
                                r.CustomStr5 = bol[1];
                            } break;
                        case "to":
                            {
                                if (bol[1].Contains("("))
                                    bol[1] = bol[1].Trim('(');
                                if (bol[1].Contains(")"))
                                    bol[1] = bol[1].Trim(')');
                                if (bol[1].Contains(","))
                                    bol[1] = bol[1].Trim(',');
                                r.CustomStr4 = bol[1];
                            } break;
                        case "from":
                            {
                                if (bol[1].Contains("<"))
                                    bol[1] = bol[1].Trim('<');
                                if (bol[1].Contains(">"))
                                    bol[1] = bol[1].Trim('>');
                                r.CustomStr3 = bol[1];
                            } break;
                        case "virusname":
                            {
                                r.CustomStr7 = bol[1];
                            } break;
                        case "filename":
                            {
                                r.CustomStr8 = bol[1];
                            } break;
                        default:
                            {
                                if (bol[0] != "status")
                                    r.Description += bol[0] + "=" + bol[1] + " ; ";

                            } break;

                    }//switch
                }//if

            }//for
        }


        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\McaffeeIpsV4SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeIpsV4SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeIpsV4SyslogRecorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogRecorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeIpsV4SyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McaffeeIpsV4Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }//class
}//name space