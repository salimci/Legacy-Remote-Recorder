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

namespace CheckpointDHCPRecorder
{
    public class CheckpointDHCPRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514,zone=0;
        private string err_log;
        private CLogger L;
        public Syslog slog = null;
        private string protocol = "udp", remote_host = "localhost";
        private bool usingRegistry = true;
        private string virtualHost,Dal;
        private int identity;
        private string location;

        private void InitializeComponent()
        {
            //Init();
        }

        public override void Init()
        {

            if (usingRegistry)
            {
                if (!Read_Registry())
                {
                    EventLog.WriteEntry("Security Manager CheckpointDHCPRecorder Read Registry", "CheckpointDHCPRecorder may not working properly ", EventLogEntryType.Error);
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
        public void get_logDir() { 
            RegistryKey rk = null;
            try
            {
                
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CheckpointDHCPRecorder"+identity+".log";
            }
            catch (Exception ess) {
                L.Log(LogType.FILE, LogLevel.ERROR, "" + ess.ToString());
            }
            
        }
        public override void SetReg(int Identity, string LastPosition, string SleepTime, string LastKeywords,string LastRecDate)
        {
            base.SetReg(Identity, LastPosition, SleepTime, LastKeywords, LastRecDate);
        }
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
                String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
                String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
                String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {

            try
            {
                trc_level = TraceLevel;

                location = Location;
                if (location.Contains(':'.ToString()))
                {
                    String[] parse = location.Split(':');
                    protocol = parse[0];
                    Syslog_Port =Convert.ToInt32(parse[1]);
                }
                else
                {
                    Syslog_Port = 514;
                }

                usingRegistry = false;
                virtualHost = Virtualhost;
                Dal = dal;
                identity = Identity;
                remote_host = RemoteHost;
                zone=Zone;
            }
            catch (Exception err) {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error at setConfigData "+err.ToString());
            }


        }// end of setconfigdata

        public override void Start()
        {
            try
            {

                // TODO: Add any initialization after the Init call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  CheckpointDHCPRecorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CheckpointDHCPRecorder on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                ProtocolType pro;
                if (protocol.ToLower() == "udp")
                    pro = ProtocolType.Udp;
                else
                    pro = ProtocolType.Tcp;

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

                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(checkpoint_DHCP);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing CheckpointDHCPRecorder Event");

               
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CheckpointDHCPRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }

        }

        public CheckpointDHCPRecorder()
        {
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
        static int ay(string ay)
        {
            try
            {

                if (ay.ToLower() == "jan" || ay.ToLower() == "january")
                    return 1;
                else
                    if (ay.ToLower() == "feb" || ay.ToLower() == "february")
                        return 2;
                    else
                        if (ay.ToLower() == "mar" || ay.ToLower() == "march")
                            return 3;
                        else
                            if (ay.ToLower() == "apr" || ay.ToLower() == "april")
                                return 4;
                            else
                                if (ay.ToLower() == "may")
                                    return 5;
                                else
                                    if (ay.ToLower() == "june")
                                        return 6;
                                    else
                                        if (ay.ToLower() == "july")
                                            return 7;
                                        else
                                            if (ay.ToLower() == "aug" || ay.ToLower() == "August")
                                                return 8;
                                            else
                                                if (ay.ToLower() == "sept" || ay.ToLower() == "september")
                                                    return 9;
                                                else
                                                    if (ay.ToLower() == "oct" || ay.ToLower() == "october")
                                                        return 10;
                                                    else
                                                        if (ay.ToLower() == "nov" || ay.ToLower() == "november")
                                                            return 11;
                                                        else
                                                            if (ay.ToLower() == "dec" || ay.ToLower() == "december")
                                                                return 12;
                                                            else {

                                                                return DateTime.Now.Month;    
                                                            }

            }
            catch
            {
                return DateTime.Now.Month; 
            }
        }
        void checkpoint_DHCP(LogMgrEventArgs args)
        {
            CustomBase.Rec r = new CustomBase.Rec();
            CustomServiceBase s;
            if (usingRegistry)
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Security Manager Sender");
                s = base.GetInstanceService("Security Manager Sender");
            }
            else
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Security Manager Remote Recorder");
                s = base.GetInstanceService("Security Manager Remote Recorder");
            }

            try
            {
                String line = args.Message.Replace('\0', ' ');

                r.LogName = "Checkpoint DHCP Recorder";
                string tarih;

                line = line.Replace('\0', ' ').TrimEnd(' ');

                String[] arr = SpaceSplit(args.Message.Replace('\0', ' '), true);
                String[] arr2 = line.Split('>');
                String[] cat = arr[2].Split('.');

                

                tarih = arr[3] + "/" + ay(arr[4]).ToString() + "/" + arr[5] + " " + arr[6];
                L.Log(LogType.FILE, LogLevel.DEBUG, "set datetime");
                r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

                //An IP conflict was detected
                if (arr[8] == "<10020>")
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set event category");
                    r.EventCategory = arr[7];
                    String[] arr3 = arr2[1].Split(':');
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set username");
                    r.UserName = arr[16];       //ip
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set description");
                    r.Description = arr2[1];    //The IP 10.33.0.1 is in use by a device with MAC address 00:09:f3:07:26:ab
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set eventtype");
                    r.EventType = arr[10] + " " + arr[11]; //iip conflict
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set event customstr1");
                    r.CustomStr1 = arr3[0];     //An IP conflict was detected
                }
                else
                    if (arr[8] == "<10016>")        //spotted
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "set event category");
                        r.EventCategory = arr[7];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "set username");
                        r.UserName = arr[arr.Length - 1];       //ip
                        L.Log(LogType.FILE, LogLevel.DEBUG, "set description");
                        r.Description = arr2[1];    //Spotted 00:16:17:4d:37:9d (TMO) using IP address 10.25.0.109
                        L.Log(LogType.FILE, LogLevel.DEBUG, "set event type");
                        r.EventType = "ADDRESS DETECTION";       //sabit
                        L.Log(LogType.FILE, LogLevel.DEBUG, "set customstr1");
                        r.CustomStr1 = arr[9];     //
                    }
                    else
                        if (arr[8] == "<10015>")        //assigned
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set event category");
                            r.EventCategory = arr[7];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set username");
                            r.UserName = arr[10];       //ip
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set description");
                            r.Description = arr2[1];    //
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set eventtype");
                            r.EventType = "DHCP";      //sabit
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set customstr1");
                            r.CustomStr1 = arr[9];      //assigned
                        }
                        else {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "set descriiption ");
                            r.Description = line;
                        }

                r.SourceName = args.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                if (!usingRegistry)
                {
                    s.SetData(Dal,virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "",r.Datetime);
                }
                else
                {
                    s.SetData(r);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");


            }//end of try
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Wrong data: " + args.Message.Replace('\0', ' '));
                r.SourceName = args.Source;
                r.LogName = "CheckpointDHCPRecorder";
                r.Description = args.Message.Replace('\0', ' ');
                L.Log(LogType.FILE, LogLevel.DEBUG, "(err) Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "(err) Start sending Data");

                if (!usingRegistry)
                {
                    s.SetData(Dal,virtualHost, r);
                    s.SetReg(identity, r.Datetime,"","", "",r.Datetime);
                }
                else
                {
                    s.SetData(r);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "(err)Finish Sending Data");
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at parsing" + ex.ToString());
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CheckpointDHCPRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CheckpointDHCPRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CheckpointDHCPRecorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("CheckpointDHCPRecorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CheckpointDHCPRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CheckpointDHCPRecorder ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }


}




