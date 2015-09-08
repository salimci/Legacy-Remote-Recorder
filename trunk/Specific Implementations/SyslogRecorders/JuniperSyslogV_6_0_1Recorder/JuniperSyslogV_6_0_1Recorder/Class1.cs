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

namespace JuniperSyslogV_6_0_1Recorder
{
    public class JuniperSyslogV_6_0_1Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
        private string err_log;
        private CLogger L;
        public Syslog slog = null;
        private string protocol = "udp", remote_host = "localhost";
        private bool usingRegistry = false;
        private string virtualHost, Dal;
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
                    EventLog.WriteEntry("Security Manager JuniperSyslogV6_0_1Recorder Read Registry", "JuniperSyslogV6_0_1Recorder may not working properly ", EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\JuniperSyslogV6_0_1Recorder" + identity + ".log";
            }
            catch (Exception ess)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "at get_logdir" + ess.ToString());
            }

        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, string LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, int Zone)
        {

            try
            {


                trc_level = TraceLevel;


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

                usingRegistry = false;
                virtualHost = virtualhost;
                Dal = dal;
                identity = Identity;
                remote_host = RemoteHost;
                zone = Zone;
            }
            catch (Exception err)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error at setConfigData " + err.ToString());
            }


        }// end of setconfigdata

        public override void Start()
        {
            try
            {

                // TODO: Add any initialization after the Init call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  JuniperSyslogV6_0_1Recorder");

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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Juniper_SyslogV6_0_0);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Juniper Syslog Event");


            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogV6_0_1Recorder Constructor", er.ToString(), EventLogEntryType.Error);
            }

        }

        void Juniper_SyslogV6_0_0(LogMgrEventArgs args)
        {
            CustomBase.Rec r = new CustomBase.Rec();
            CustomServiceBase s;
            r.LogName = "JuniperSyslogV6_0_1 Recorder";
            L.Log(LogType.FILE, LogLevel.DEBUG, "data: " + args.Message);
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
                //192.0.0.110:29813 : local0.notice TUYAP_MERKEZ_FW: NetScreen device_id=TUYAP_MERKEZ_FW  [Root]system-notification-00257(traffic): start_time="2011-05-27 11:01:53" duration=0 policy_id=29 service=proto:41/port:1 proto=41 src zone=Trust dst zone=ADSL action=Deny sent=0 rcvd=0 src=192.0.0.72 dst=192.88.99.1 session_id=0 
                String[] arr = args.Message.Split(' ');
                r.SourceName = arr[0];
                r.CustomStr8 = arr[2];
                r.ComputerName = arr[3].TrimEnd(':');
                L.Log(LogType.FILE, LogLevel.DEBUG, "r.computername: " + r.ComputerName);
                L.Log(LogType.FILE, LogLevel.DEBUG, "r.CustomStr8: " + r.CustomStr8);

                Int32 i = 6;
                for (; i < arr.Length; i++)
                {
                    if (arr[i].Contains("[Root]"))
                    {
                        r.EventType = arr[i].TrimStart("[Root]".ToCharArray()).TrimEnd(':');
                        L.Log(LogType.FILE, LogLevel.DEBUG, "evettype: " + r.EventType);
                        break;
                    }
                }
                i++;

                string sLines = args.Message;

                if (sLines.Contains("Aritech_Isg1000") && sLines.Contains("]") && sLines.Contains(" From "))
                    r.CustomStr9 = sLines.Substring(sLines.IndexOf(':', sLines.IndexOf(']')) + 1,
                        sLines.IndexOf(" From ") - sLines.IndexOf(':', sLines.IndexOf(']')) - 1).Trim();

                L.Log(LogType.FILE, LogLevel.DEBUG, "Test | r.CustomStr9: " + r.CustomStr9);

                if (args.Message.Contains("start_time"))
                {
                    for (; i < arr.Length; i++)
                    {
                        if (arr[i].Contains("start_time"))
                        {
                            string[] dateArr = arr[i].TrimStart("start_time=\"".ToCharArray()).Split('-');
                            if (dateArr[0].Length == 4)
                            {
                                r.Datetime = dateArr[0] + "/" + dateArr[1] + "/" + dateArr[2];
                            }
                            else
                            {
                                r.Datetime = dateArr[2] + "/" + dateArr[1] + "/" + dateArr[0];
                            }
                            break;
                        }
                    }
                    i++;

                    r.Datetime += (" " + arr[i].TrimEnd("\"".ToCharArray()));
                    i++;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "datetime: " + r.Datetime);

                    for (; i < arr.Length; i++)
                    {
                        if (arr[i].Contains("duration"))
                        {
                            r.CustomInt7 = Convert.ToInt32(arr[i].TrimStart("duration=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("policy_id"))
                        {
                            r.CustomInt8 = Convert.ToInt32(arr[i].TrimStart("policy_id=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("proto="))//???????? 2 tane proto var
                        {
                            r.CustomInt3 = Convert.ToInt32(arr[i].TrimStart("proto=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("sent"))
                        {
                            r.CustomInt4 = Convert.ToInt32(arr[i].TrimStart("sent=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("rcvd"))
                        {
                            r.CustomInt5 = Convert.ToInt32(arr[i].TrimStart("rcvd=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("session_id"))
                        {
                            r.CustomInt6 = Convert.ToInt32(arr[i].TrimStart("session_id=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("src_port"))
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[i].TrimStart("src_port=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("dst_port"))
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[i].TrimStart("dst_port=".ToCharArray()));
                            continue;
                        }
                        else if (arr[i].Contains("service"))
                        {
                            r.CustomStr1 = arr[i].TrimStart("service=".ToCharArray());
                            if (i + 1 != arr.Length)
                            {
                                if (arr[i + 1].Contains("(") & arr[i + 1].Contains(")"))
                                {
                                    r.CustomStr1 = r.CustomStr1 + arr[i + 1];
                                    i++;
                                }
                            }
                            continue;
                        }
                        else if (arr[i].Contains("action"))
                        {
                            r.EventCategory = arr[i].TrimStart("action=".ToCharArray());
                            if (arr[i + 1].Contains("(") && arr[i + 1].Contains(")"))
                                r.CustomStr10 = arr[i + 1].Trim('(').Trim(')');
                            continue;
                        }
                        else if (arr[i].Contains("reason"))
                        {
                            r.CustomStr2 = arr[i].TrimStart("reason=".ToCharArray());
                            continue;
                        }
                        else if (i + 1 != arr.Length)
                        {
                            if (arr[i].Contains("icmp") & arr[i + 1].Contains("type"))
                            {
                                i++;
                                r.CustomInt9 = Convert.ToInt32(arr[i].TrimStart("type=".ToCharArray()));
                                continue;
                            }
                            else if (arr[i].Contains("src-xlated") & arr[i + 1].Contains("ip"))
                            {
                                i++;
                                r.CustomStr5 = arr[i].TrimStart("ip=".ToCharArray()); //src-xlated ip
                                if (i + 1 != arr.Length)
                                {
                                    if (arr[i + 1].Contains("port="))
                                    {
                                        r.CustomStr5 = r.CustomStr5 + ":" + arr[i + 1].TrimStart("port=".ToCharArray());
                                        i++;
                                    }
                                }

                                continue;
                            }
                            else if (arr[i].Contains("dst-xlated") & arr[i + 1].Contains("ip"))
                            {
                                i++;
                                r.CustomStr6 = arr[i].TrimStart("ip=".ToCharArray()); //dst-xlated ip
                                if (i + 1 != arr.Length)
                                {
                                    if (arr[i + 1].Contains("port="))
                                    {
                                        r.CustomStr6 = r.CustomStr6 + ":" + arr[i + 1].TrimStart("port=".ToCharArray());
                                        i++;
                                    }
                                }
                                continue;
                            }
                            else if (arr[i].Contains("src") & !arr[i + 1].Contains("zone"))
                            {
                                r.CustomStr3 = arr[i].TrimStart("src=".ToCharArray());
                                continue;
                            }
                            else if (arr[i].Contains("dst") & !arr[i + 1].Contains("zone"))
                            {
                                r.CustomStr4 = arr[i].TrimStart("dst=".ToCharArray());
                                continue;
                            }
                            if (arr[i].Contains("src") & arr[i + 1].Contains("zone"))
                            {
                                i++;
                                r.CustomStr7 = arr[i].TrimStart("zone=".ToCharArray()); //src zone
                                continue;
                            }
                            else if (arr[i].Contains("dst") & arr[i + 1].Contains("zone"))
                            {
                                i++;
                                r.CustomStr8 = arr[i].TrimStart("zone=".ToCharArray()); //dst zone
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    if (arr[arr.Length - 2].Contains("(") & arr[arr.Length - 1].Contains(")"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Different log format. Probably WebSec log..");
                        string[] dateArr = arr[arr.Length - 2].TrimStart('(').Split('-');
                        if (dateArr[0].Length == 4)
                        {
                            r.Datetime = dateArr[0] + "/" + dateArr[1] + "/" + dateArr[2];
                        }
                        else
                        {
                            r.Datetime = dateArr[2] + "/" + dateArr[1] + "/" + dateArr[0];
                        }
                        r.Datetime += (" " + arr[arr.Length - 1].Split(')')[0]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "datetime: " + r.Datetime);

                        bool bKontrol = false;

                        for (; i < arr.Length - 1; i++)
                        {

                            //proto ve Occurred arasındakileri okumak icin
                            if (bKontrol == true)
                                r.CustomStr7 = arr[i + 1] + " ";

                            switch (arr[i])
                            {
                                case "UF-MGR:":
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting uf-mgr");
                                        r.CustomStr1 = arr[i + 1]; //"URL"
                                        r.CustomStr2 = arr[i + 2].TrimEnd(':'); //permitted/blocked
                                        string[] addArr = arr[i + 3].Split("->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        r.CustomStr3 = addArr[0]; //src ip
                                        r.CustomStr4 = addArr[1]; // dst ip
                                        L.Log(LogType.FILE, LogLevel.DEBUG, r.CustomStr3 + " -> " + r.CustomStr4);
                                        r.Description = arr[i + 4]; //url 
                                        // UF-MGR: URL PERMITTED: 10.130.1.20(28822)->85.153.7.222(80) www.belgeselizle.com/wp-content/kopek.jpg CATEGORY: Streaming Media REASON: BY_PRE_DEFINED PROFILE: Dolmabahce_Profile (2009-10-10 20:43:13) 
                                    }
                                    break;
                                case "CATEGORY:":
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting category inside");
                                        r.EventCategory = arr[i + 1];
                                    }
                                    break;
                                case "REASON:":
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting REASON");
                                        r.CustomStr10 = arr[i + 1];
                                    }
                                    break;
                                case "PROFILE:":
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "setting PROFILE");
                                        r.CustomStr9 = arr[i + 1];
                                    } break;


                                //d.ali
                                case "From":
                                    {

                                        r.CustomStr9 = sLines.Substring(sLines.IndexOf(':', sLines.IndexOf(']')) + 1, sLines.IndexOf(" From ") - sLines.IndexOf(':', sLines.IndexOf(']')) - 1).Trim();
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "From Test | r.CustomStr9: " + r.CustomStr9);

                                        r.CustomStr3 = arr[i + 1].Split(':')[0].ToString();
                                        if (arr[i + 1].Split(':').Length > 1)
                                            if (string.IsNullOrEmpty(arr[i + 1].Split(':')[1].ToString()) == false)
                                                r.CustomInt1 = Convert.ToInt32(arr[i + 1].Split(':')[1].ToString());

                                    } break;
                                case "to":
                                    {
                                        r.CustomStr4 = arr[i + 1].Split(':')[0].ToString().Trim(',');
                                        if (arr[i + 1].Split(':').Length > 1)
                                        {
                                            string sValue = arr[i + 1].Trim(",".ToCharArray()).Split(':')[1].ToString().Trim('.');
                                            if (string.IsNullOrEmpty(sValue) == false)
                                                if (sValue.Contains("/") == true)
                                                {
                                                    r.CustomInt2 = Convert.ToInt32(sValue.Split('/')[0]);
                                                    r.CustomInt3 = Convert.ToInt32(sValue.Split('/')[1]);
                                                }
                                                else
                                                {
                                                    r.CustomInt2 = Convert.ToInt32(sValue);
                                                }
                                        }
                                    } break;



                                case "proto":
                                    {
                                        r.CustomStr4 = arr[i + 1];
                                        bKontrol = true;
                                    } break;

                                case "Occurred":
                                    {
                                        if (string.IsNullOrEmpty(arr[i + 1]) == false)
                                            r.CustomInt10 = Convert.ToInt64(arr[i + 1]);
                                        bKontrol = false;
                                    } break;

                            }

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + r.Description);
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Wrong format. All data will be written in description column.");
                        r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); // .Replace(".", "/");

                        /*     string[] strArr = SpaceSplit(args.Message, true);
                             if (strArr[2].ToString() == "info")
                             {
                                 r.EventCategory = strArr[2];
                                 r.CustomStr2 = strArr[7];
                                 r.CustomStr3 = strArr[5];
                                 r.CustomStr9 = strArr[strArr.Length - 1];
                             }
                             else
                             {
                                 r.SourceName = strArr[0];
                                 r.EventType = strArr[6];
                                 r.ComputerName = strArr[3];
                                 r.CustomStr2 = strArr[11];
                                 r.CustomStr3 = strArr[9];
                                 r.CustomStr8 = strArr[2];
                                 r.CustomStr9 = strArr[13];
                             }*/

                        for (; i < arr.Length; i++)
                        {
                            r.Description += arr[i] + " ";
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + r.Description);
                    }
                }

                if (!r.EventType.Contains("(traffic)"))
                {
                    string[] strArr = SpaceSplit(args.Message, true);
                    r.SourceName = strArr[0];
                    r.ComputerName = strArr[3];
                    r.CustomStr2 = strArr[11];
                    r.CustomStr3 = Between(args.Message, "IP address ", " is");
                    r.CustomStr8 = strArr[2];
                    r.CustomStr9 = strArr[13].Replace('.', ' ').Trim();
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                /*
                //fields are change for standartization
                r.CustomInt1 = r.CustomInt7;
                r.CustomInt2 = r.CustomInt8;
                string backup = r.backup;
                r.EventCategory = r.CustomStr4;
                r.CusomtStr4 = r.CustomStr6;
                r.CustomStr6 = r.CusomtStr8;
                r.CustomStr8 = backup;
                r.CustomStr9 = r.CusomtStr3;
                r.CusomtStr3 = r.CustomStr5;
                r.CusomtStr5 = r.CusomtStr7;
                //---------------------
                */
                r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                if (r.SourceName.Contains(":"))
                {
                    r.SourceName = r.SourceName.Split(':')[0];
                }

                r.Description = args.Message;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + r.Description);

                if (!usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set data1");
                    s.SetData(Dal, virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "", "", r.Datetime);
                }
                else
                {
                    s.SetData(r);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set data2");
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                //L.Log(LogType.FILE, LogLevel.DEBUG, "tarih3" + r.Datetime);

            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Wrong data: " + args.Message.Replace('\0', ' '));
                L.Log(LogType.FILE, LogLevel.ERROR, "\0Exception message: " + ex.Message);
                r.SourceName = args.Source;

                if (r.SourceName.Contains(":"))
                {
                    r.SourceName = r.SourceName.Split(':')[0];
                }

                r.LogName = "Juniper Syslog Recorder";
                r.Description = args.Message.Replace('\0', ' ');
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                if (!usingRegistry)
                {
                    s.SetData(Dal, virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "", "", r.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set data3");
                }
                else
                {
                    s.SetData(r);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "set data4");
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");


                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at parsing" + ex.ToString());
                //L.Log(LogType.FILE, LogLevel.DEBUG, "tarih2" + r.Datetime);
            }
            finally
            {
                s.Dispose();
            }

        }

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
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
        }// SpaceSplit

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\JuniperSyslogV6_0_1Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogV6_0_1Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogV6_0_1Recorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogV6_0_1Recorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogV6_0_1Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager JuniperSyslogV6_0_1Recorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }


}


