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

namespace JuniperSyslogRecorder
{
    public class JuniperSyslogV2Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514,zone;
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
                    EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Read Registry", "JuniperSyslogRecorder may not working properly ", EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\JuniperSyslogRecorder"+identity+".log";
            }
            catch (Exception ess) {
                L.Log(LogType.FILE, LogLevel.ERROR, "at get_logdir" + ess.ToString());
            }

        }


        public override void SetConfigData(Int32 Identity, String Location, String LastLine, string LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost,String dal,Int32 Zone)
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
                virtualHost = virtualhost;
                Dal = dal;
                identity = Identity;
                remote_host = RemoteHost;
                zone = Zone;
            }
            catch (Exception err) {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error at setConfigData "+err.ToString());
            }


        }// end of setconfigdata

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
        public override void Start()
        {
            try
            {

                // TODO: Add any initialization after the Init call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  JuniperSyslogRecorder");

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening JuniperSyslog on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                ProtocolType pro;
                if (protocol.ToLower() == "udp")
                    pro = ProtocolType.Udp;
                else
                    pro = ProtocolType.Tcp;

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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Juniper_Syslog);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Juniper Syslog Event");

               
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }

        }

        public JuniperSyslogV2Recorder()
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

        void parseInfo(String line, ref CustomBase.Rec r)
        {
            bool parseble = true;
            String[] ip1 = null;
            String[] ip2 = null;
            string dst = " ";
            string category = " ";
            string reason = " ";
            String tarih = " ";
            string ufmgr = " ";

            line = line.Replace('\0', ' ').TrimEnd(' ');
            String[] pars1 = line.Split(' ');

            //parsing date 
            int parsLen = pars1.Length;
            tarih = pars1[parsLen - 2].TrimStart('(').Replace("-", "/");
            if (tarih.Length != 10)
            {
                String[] t = tarih.Split('/');
                if (t[1].Length == 1)
                {
                    t[1] = "0" + t[1];
                }
                if (t[2].Length == 1)
                {
                    t[2] = "0" + t[2];
                }
                tarih = t[0] + "/" + t[1] + "/" + t[2];

            }
            String[] tar = pars1[parsLen - 1].Split(')');
            tarih += " " + tar[0];
            String[] line5;

            if (pars1.Length == 31 || pars1.Length == 15)
            {
                string msgid = " ";
                string spi = " ";
                string descrip = " ";
                string lifetime = " ";


                line5 = pars1[8].TrimEnd('>').Split('<'); //ike - ip}

                if (pars1.Length > 30)
                {
                    msgid = pars1[13].TrimStart('<').TrimEnd(':').TrimEnd('>');
                    spi = pars1[18].TrimStart('<').TrimEnd(',').TrimEnd('>');
                    descrip = pars1[21];
                    lifetime = pars1[25].TrimStart('<').TrimEnd('>');
                }
                //hepsini descriptiona atilacak 
                r.Description = line;
            }// 31---15
            else
                if (pars1.Length == 35)
                {
                    String[] iport1 = pars1[15].Split(':');
                    String[] iport2 = pars1[17].Split(':');

                    r.Description = line;
                }// == 35
                else
                    if (pars1[13].ToLower().Contains("category"))
                    {
                        //parsing uf-mgr
                        ufmgr = pars1[9] + " " + pars1[10].TrimEnd(':');

                        //parsing ips and ports
                        String[] ip = pars1[11].Replace("->", "-").TrimEnd(',').Split('-');
                        if (pars1[11].Contains("("))
                        {
                            ip1 = ip[0].TrimEnd(')').Split('(');
                            ip2 = ip[1].TrimEnd(')').Split('(');
                        }
                        else
                            if (pars1[11].Contains(":"))
                            {
                                ip1 = ip[0].Split(':');
                                ip2 = ip[1].Split(':');
                            }
                        //parsing dest
                        if (pars1.Length > 20)
                        {
                            dst = pars1[12];
                            if (pars1.Length > 22)
                            {
                                category = pars1[14] + " & " + pars1[16];
                                reason = pars1[18];
                            }//23
                            else
                                if (pars1.Length > 21)
                                {
                                    category = pars1[14] + " " + pars1[15];
                                    reason = pars1[17];
                                }//22
                                else// =21
                                {
                                    category = pars1[14];
                                    reason = pars1[16];
                                }
                        }//>20
                    }//else
                    else
                    {
                        r.Description = line;
                        parseble = false;
                    }
            if (parseble)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "preparing data ");
                r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                r.CustomStr1 = ufmgr;                //uf-mgr

                if (ip1.Length > 1)
                {
                    r.CustomInt1 = Convert.ToInt32(ip1[1]);   //port
                    if (ip2.Length > 1)
                        r.CustomInt2 = Convert.ToInt32(ip2[1]);   //port
                    r.CustomStr5 = ip1[0];                  //src-xlated ip
                    r.CustomStr6 = ip2[0];                  //dst-xlated ip
                }

                r.CustomStr10 = reason;                   //reason
                r.CustomStr4 = dst;                   //dst
                //r.EventCategory = category;
                L.Log(LogType.FILE, LogLevel.DEBUG, "preparing finished ");

            }

        }

        void parseWarning(String line, ref CustomBase.Rec r)
        {
            
            String[] ip1 = null;
            String[] ip2 = null;
            string dst = " ";
            string category = " ";
            string action = " ";
            string reason = " ";
            String tarih = " ";

            line = line.Replace('\0', ' ').TrimEnd(' ');
            String[] pars1 = line.Split(' ');

            //parsing uf-mgr
            string ufmgr = pars1[9] + " " + pars1[10].TrimEnd(':');

            //parsing ips and ports
            String[] ip = pars1[11].Replace("->", "-").TrimEnd(',').Split('-');

            if (pars1[11].Contains("("))
            {
                ip1 = ip[0].TrimEnd(')').Split('(');
                ip2 = ip[1].TrimEnd(')').Split('(');

            }
            else
                if (pars1[11].Contains(":"))
                {
                    ip1 = ip[0].Split(':');
                    ip2 = ip[1].Split(':');

                }
            //parsing date 
            int parsLen = pars1.Length;
            tarih = pars1[parsLen - 2].TrimStart('(').Replace("-", "/");
            if (tarih.Length != 10)
            {
                String[] t = tarih.Split('/');
                if (t[1].Length == 1)
                {
                    t[1] = "0" + t[1];
                }
                if (t[2].Length == 1)
                {
                    t[2] = "0" + t[2];
                }
                tarih = t[0] + "/" + t[1] + "/" + t[2];

            }
            String[] tar = pars1[parsLen - 1].Split(')');
            tarih += " " + tar[0];

            if (pars1[0].Contains(":"))
            {
                if (pars1[17].Contains(")"))
                {
                    ip2 = pars1[17].Split(')');     //dst port
                    ip2[1] = ip2[0];
                    ip2[0] = "0";
                    if (pars1[19].Contains(":"))
                        ip1 = pars1[19].Split(':');
                }
                dst = pars1[12];
                r.Description = pars1[8] + pars1[9] + pars1[10] + pars1[11] + pars1[12] + pars1[13] + pars1[14] + pars1[15];

            }
            else
            if (pars1.Length < 23 && line.ToLower().Contains("adult"))
            {
                dst = pars1[12];
                category = pars1[14] + pars1[15];
                reason = pars1[17];
            }
            else
                if (pars1.Length == 21 && pars1[13].ToLower().Contains("category"))  //parsing dest
                {
                    dst = pars1[12];
                    category = pars1[14];
                    reason = pars1[16];
                }//21
                else
                {
                    //parsing dest
                    dst = pars1[14] + pars1[15];

                    //parsing category -action -reason
                    if (pars1.Length > 30)
                    {
                        if (pars1.Length > 34)
                        {
                            if (pars1.Length > 38)
                            {
                                category = pars1[33];
                                action = pars1[31];
                                reason = pars1[36];
                            }
                            else //34<x<38
                            {
                                if (pars1.Length == 37)
                                    reason = pars1[34];
                                category = pars1[32];
                                action = pars1[30];
                            }
                        }//30<x<34
                        else
                            category = pars1[28];
                    }//if >30
                }//else
            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing data ");
            r.Datetime = tarih;
            r.CustomStr1 = ufmgr;                //uf-mgr
            r.CustomInt1 = Convert.ToInt32(ip1[1]);   //port
            if (ip2.Length > 1)
                r.CustomInt2 = Convert.ToInt32(ip2[1]);   //port
            r.CustomStr5 = ip1[0];                  //src-xlated ip
            r.CustomStr6 = ip2[0];                  //dst-xlated ip
            r.CustomStr10 = reason;                   //reason
            //r.CustomStr4 = action;                //action
            r.CustomStr4 = dst;                   //dst
            //r.EventCategory = category;
            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing finished ");
        }
        void Allocate(ref CustomBase.Rec r, String key, String value, ref bool check)
        {
            switch (key)
            {
                case "start_time":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting start time");
                        String[] yil = value.Split(' ');
                        String[] tar = yil[0].Split('-');
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting year: ");
                        r.Datetime = tar[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting month: ");
                        if (tar[1].Length == 1)
                        {
                            tar[1] = "0" + tar[1];
                        }
                        r.Datetime += "/" + tar[1];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting day: ");
                        if (tar[2].Length == 1)
                        {
                            tar[2] = "0" + tar[2];
                        }
                        r.Datetime += "/" + tar[2];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting time: ");
                        r.Datetime += " " + yil[1];
                    }
                    break;
                case "policy_id":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting policy id");
                        r.CustomInt2 = Convert.ToInt32(value);
                    }
                    break;
                case "service":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting service");
                        r.CustomStr1 = value;
                    }
                    break;
                case "proto":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting proto");
                        r.CustomInt3 = Convert.ToInt32(value);
                    }
                    break;
                case "src zone":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting src zone");

                    }
                    break;
                case "dst zone":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting dst zone");

                    }
                    break;
                case "action":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "setting action");
                        r.EventCategory = value;
                    }
                    break;
                case "sent":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting sent");
                        if (value.Contains("?"))
                            value = value.TrimStart('?');
                        //r.CustomInt2 = Convert.ToInt32(value);
                    }
                    break;
                case "rcvd":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting rcvd");
                        if (value.Contains("<"))
                            value = value.TrimStart('<');
                        //r.CustomInt4 =Convert.ToInt32(value);
                    }
                    break;
                case "src":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting src");
                        r.CustomStr3 = value;
                    }
                    break;
                case "dst":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting dst");
                        r.CustomStr4 = value;
                    }
                    break;
                case "src_port":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting src port");
                        r.CustomInt1 = Convert.ToInt32(value);
                    }
                    break;
                case "dst_port":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting dst port");
                        r.CustomInt2 = Convert.ToInt32(value);
                    }
                    break;
                case "src-xlated ip":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting src xlated ip");
                        r.CustomStr5 = value;
                    }
                    break;
                case "dst-xlated ip":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting dst xlated ip");
                        r.CustomStr6 = value;
                    }
                    break;
                case "port":
                    if (check)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting port-1");
                        r.CustomInt9 = Convert.ToInt32(value);
                        check = false;
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting port2");
                        r.CustomStr9 = value;
                    }
                    break;
                case "session_id":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting session id");
                        r.CustomInt10 = Convert.ToInt32(value);
                    }
                    break;
                case "reason":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting reason");
                        r.CustomStr10 = value;
                    }
                    break;

                //Other
                case "UF-MGR":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting uf-mgr");
                        r.CustomStr1 = value;
                    }
                    break;
                case "BLOCKED":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting blocked");
                        String[] temp = value.Split(' ');
                        String[] tempLeft = temp[0].Split('-', '>');
                        String[] ipLeft = tempLeft[0].Split('(');
                        String[] ipRight = tempLeft[2].Split('(');
                        r.CustomStr5 = ipLeft[0];
                        r.CustomStr6 = ipRight[0];
                        if (temp.Length > 1)
                            r.CustomStr10 = temp[1];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "finis setting bloacked");
                    } break;
                case "CATEGORY":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting category inside");
                        //r.EventCategory= value;
                    }
                    break;
                case "REASON":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting REASON");
                        r.CustomStr10 = value;
                    }
                    break;
                case "PROFILE":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting PROFILE");
                        String[] temp = value.Split('(');
                        r.CustomStr9 = temp[0].Trim();
                        String[] tempRight = temp[1].Split(')');
                       // r.CustomStr10 = tempRight[0];
                      //  L.Log(LogType.FILE, LogLevel.DEBUG, "finish setting PROFILE ");
                    } break;
            };
        }
        void Juniper_Syslog(LogMgrEventArgs args)
        {
            CustomBase.Rec r = new CustomBase.Rec();
            CustomServiceBase s;
            r.LogName = "Juniper Syslog Recorder";
            if (usingRegistry)
            {
                 s= base.GetInstanceService("Security Manager Sender");
            }
            else
            {
                s = base.GetInstanceService("Security Manager Remote Recorder");
            }
            try
            {
                bool check = true;

                String[] arr = SpaceSplit(args.Message.Replace('\0', ' '), true);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Log is :" + args.Message);
                L.Log(LogType.FILE, LogLevel.DEBUG, "setting eventcategory");
                //r.EventCategory = arr[2];
                String[] device_id = arr[5].Split('=');

                L.Log(LogType.FILE, LogLevel.DEBUG, "setting computername");
                r.ComputerName = device_id[1];
                r.EventType = arr[6];

                String[] cat = arr[2].Split('.');

                if (cat[1].ToLower() == "warning")
                    parseWarning(args.Message.Replace('\0', ' '), ref r);
                else
                    if (cat[1].ToLower() == "Warning")
                        parseWarning(args.Message.Replace('\0', ' '), ref r);
                    else
                        if (cat[1] == "Info")
                            parseInfo(args.Message.Replace('\0', ' '), ref r);
                        else
                            if (cat[1] == "info")
                                parseInfo(args.Message.Replace('\0', ' '), ref r);
                            else
                                if (cat[1].ToLower() == "notice")
                                {

                                    if (arr.Length < 30 && arr.Length != 25)
                                    {
                                        String lastKey = "";
                                        String lastValue = "";
                                        for (Int32 i = 7; i < arr.Length; i++)
                                        {
                                            lastKey = arr[i].Trim(':');
                                            for (Int32 j = i + 1; j < arr.Length; j++)
                                            {
                                                if (arr[j].Contains(":") && lastKey != "PROFILE")
                                                    break;

                                                lastValue += " " + arr[j];
                                                i++;
                                            }
                                            Allocate(ref r, lastKey, lastValue.Trim(), ref check);
                                            lastValue = "";
                                        }//for outher
                                    }//if
                                    else
                                    {
                                        String lastKey = "";
                                        String lastValue = "";
                                        for (Int32 i = 7; i < arr.Length; i++)
                                        {
                                            String[] temp = arr[i].Split('=');
                                            if (temp.Length < 2)
                                            {
                                                if (temp[0] == "src" || temp[0] == "src-xlated" || temp[0] == "dst" || temp[0] == "dst-xlated")
                                                {
                                                    lastKey = temp[0];
                                                }
                                                else
                                                {
                                                    //err
                                                }
                                            }//if
                                            else
                                            {
                                                if (lastKey == "src" || lastKey == "src-xlated" || lastKey == "dst" || lastKey == "dst-xlated")
                                                {
                                                    if (lastKey != "src" || temp[0] != "dst")
                                                        if (lastKey != "dst" || temp[0] != "src_port")
                                                        {
                                                            lastKey += " " + temp[0];
                                                            lastValue = temp[1];
                                                        }
                                                    Allocate(ref r, lastKey, lastValue, ref check);
                                                    lastKey = temp[0];
                                                    lastValue = temp[1];
                                                    continue;
                                                }
                                                Allocate(ref r, lastKey, lastValue, ref check);
                                                lastKey = temp[0];
                                                lastValue = temp[1];
                                                if (lastKey == "start_time")
                                                {
                                                    i++;
                                                    lastValue += " " + arr[i];
                                                    lastValue = lastValue.Trim('"');
                                                }
                                                else if (lastKey == "reason")
                                                {
                                                    for (Int32 j = i + 1; j < arr.Length; j++)
                                                    {
                                                        lastValue += " " + arr[j];
                                                    }
                                                    Allocate(ref r, lastKey, lastValue, ref check);
                                                    break;
                                                }//end of else if
                                            }//end of inner else
                                        }//end of for
                                    }//end of else
                                }//if-1
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "invalid category :" + cat[1]);
                                }
                //sending data
                r.SourceName = args.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                if (!usingRegistry)
                {
                    s.SetData(Dal,virtualHost, r);
                    s.SetReg(identity, r.Datetime, "","","",r.Datetime);
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
                r.LogName = "Juniper Syslog Recorder";
                r.Description = args.Message.Replace('\0', ' ');
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                s.SetData(r);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\JuniperSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogRecorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogRecorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }


}


