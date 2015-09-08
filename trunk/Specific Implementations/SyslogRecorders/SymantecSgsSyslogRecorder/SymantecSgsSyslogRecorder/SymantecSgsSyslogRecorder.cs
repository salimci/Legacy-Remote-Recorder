/*
 // Emrah Balcýlar 
 // sYmantec gateway security Syslog recorder
*/


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

namespace SymantecSgsSyslogRecorder
{
    public class SymantecSgsSyslogRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514;
        public static  int zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private bool reg_flag = false;
        private CLogger L;
        public Syslog slog = null;
        protected Int32 Id = 0;
        protected String virtualhost,Dal;
        protected bool usingRegistry = true;
        private ProtocolType pro;

        private void InitializeComponent()
        {
            /*
            if (!Read_Registry())
            {
                EventLog.WriteEntry("Security Manager SymantecSgsSyslogRecorder Read Registry", "SymantecSgsSyslog Recorder  may not working properly ", EventLogEntryType.Error);
                return;
            }
            else
                if (!Initialize_Logger())
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Recorder Service functions may not be running");
                    return;
                }
             */
        }
        public SymantecSgsSyslogRecorder()
        {
            /*
            try
            {
                InitializeComponent();
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                if (protocol == "TCP")
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, System.Net.Sockets.ProtocolType.Tcp);
                else
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, System.Net.Sockets.ProtocolType.Udp);


                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sgs_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSgsSyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSgsSyslog Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;

                        if (protocol.ToUpper() == "TCP")
                            pro = ProtocolType.Tcp;
                        else
                            pro = ProtocolType.Udp;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSgsSyslog Recorder functions may not be running");
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

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SymantecSgsSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SymantecSgsSyslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSgsSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SymantecSgsSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSgsSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
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
        static void Allocate2(ref CustomBase.Rec r, String key, String value)
        {
            //CLogger all = new CLogger();
            //Console.WriteLine("."+key);
            switch (key)
            {
                case "Source IP":

                    //all.Log(LogType.FILE, LogLevel.DEBUG, "Source ip:"+value);

                    r.CustomStr3 = value;
                    Console.WriteLine("Source IP OK" + value);
                    break;
                case "Destination IP":
                    r.CustomStr4 = value;
                    Console.WriteLine("Destination IP OK" + value);
                    break;
                case "Source Name":
                    r.CustomStr3 = value;
                    Console.WriteLine("Source Name OK" + value);
                    break;
                case "Destination Name":
                    r.CustomStr4 = value;
                    Console.WriteLine("Destination Name OK" + value);
                    break;
                case "Source Port":
                    r.CustomInt1 = Convert.ToInt32(value);
                    Console.WriteLine("Source Port OK" + value);
                    break;
                case "Destination Port":
                    r.CustomInt2 = Convert.ToInt32(value);
                    Console.WriteLine("Destination Port OK" + value);
                    break;
                case "Target":
                    r.CustomStr1 = value;
                    Console.WriteLine("Target OK" + value);
                    break;
                case "Operation":
                    r.CustomStr2 = value;
                    Console.WriteLine("Operation OK" + value);
                    break;
                case "Source Interface":
                //20.07.2009    
                //r.CustomStr5 = value;
                    //Console.WriteLine("Source Interface" + value);
                    break;
                case "Destination Interface":
                    //20.07.2009     
                //r.CustomStr6 = value;
                    //Console.WriteLine("Destination Interface" + value);
                    break;
                case "Protocol":
                    r.CustomStr7 = value;
                    Console.WriteLine("Protocol" + value);
                    break;
                case "Sent":
                    r.CustomInt3 = Convert.ToInt32(value);
                    Console.WriteLine("Sent" + value);
                    break;
                case "Received":
                    r.CustomInt4 = Convert.ToInt32(value);
                    Console.WriteLine("Received" + value);
                    break;
                case "Bytes":
                    r.CustomInt5 = Convert.ToInt32(value);
                    Console.WriteLine("Bytes" + value);
                    break;
                case "Server Source":
                    r.CustomStr8 = value;
                    Console.WriteLine("Server Source" + value);
                    break;
                case "Server Source Port":
                    r.CustomInt6 = Convert.ToInt32(value);
                    Console.WriteLine("Server Source Port" + value);
                    break;
                case "Detail":
                    r.CustomStr9 = value;
                    Console.WriteLine("Detail" + value);
                    break;
                case "Rule":
                    r.CustomStr10 = value;
                    Console.WriteLine("Rule" + value);
                    break;
                case "Line number":
                    r.CustomInt7 = Convert.ToInt32(value);
                    Console.WriteLine("Line Number" + value);
                    break;
                default:
                    r.Description += key + "= ";
                    r.Description += value + " ,";
                    //Console.WriteLine("Description" + r.Description);
                    break;

            };
        }
        static void infoParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            //CLogger m;
            //L.Log(LogType.FILE, LogLevel.DEBUG, "sgs log");
            //m.Log(LogType.FILE, LogLevel.DEBUG, "sgs log");
            //event category
            r.EventType = arr[2];
            Console.Write("eventcategory---)");
            Console.WriteLine(arr[2]);

            //computer name
            r.ComputerName = arr[6];

            Console.Write("computername---)");
            Console.WriteLine(arr[6]);

            //repair    //event type
            Console.Write("eventtype---)");
            String a = line.Substring(55);
            String[] b = a.Split(':');
            if (b[1] != " Repeated")
            {
                String[] c = b[1].TrimStart().Split(',');
                String eType = c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }
            else if (b[1] == " Repeated")
            {
                String[] c = b[2].TrimStart().Split(',');
                String eType = "Repeated: " + c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }



            //repair    //date time
            //DateTime dt = DateTime.Parse("10/11/2008 20:21:22");
            //Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@" + dt);

            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = Convert.ToDateTime(dType).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
            ////////////////////////////////////////7
            String[] packet = line.Split(',');
            //Console.WriteLine("***"+packet[0]);
            //Console.WriteLine("***" + line);

            for (int i = 1; i < packet.Length; i++)
            {
                String[] column = packet[i].TrimStart().Split('=');

                if (column[0] != "Target")
                {
                    //Console.WriteLine("zaq" + column[0]);
                    if (column.Length != 1)
                    {
                        Allocate2(ref r, column[0], column[1]);
                    }
                }

                else
                {

                    String ex = column[1];
                    for (int j = 2; j < column.Length; j++)
                    {
                        ex = ex + column[j];
                    }
                    Allocate2(ref r, column[0], ex);
                }

            }
            CLogger inf = new CLogger();
            //inf.Log(LogType.FILE, LogLevel.DEBUG, "inf parser");

        }
        static void noticeParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            //event category
            r.EventType = arr[2];
            Console.Write("eventcategory---)");
            Console.WriteLine(arr[2]);

            //computer name
            r.ComputerName = arr[6];

            Console.Write("computername---)");
            Console.WriteLine(arr[6]);

            //repair    //event type
            Console.Write("eventtype---)");
            String a = line.Substring(55);
            String[] b = a.Split(':');
            if (b[1] != " Repeated")
            {
                String[] c = b[1].TrimStart().Split(',');
                String eType = c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }
            else if (b[1] == " Repeated")
            {
                String[] c = b[2].TrimStart().Split(',');
                String eType = "Repeated: " + c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }


            //repair    //date time
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;
            ////////////////////////////////////////7
            String[] packet = line.Split(',');
            //Console.WriteLine("***"+packet[0]);
            //Console.WriteLine("***" + line);

            for (int i = 1; i < packet.Length; i++)
            {
                String[] column = packet[i].TrimStart().Split('=');

                if (column[0] != "Target")
                {
                    //Console.WriteLine("zaq" + column[0]);
                    if (column.Length != 1)
                    {
                        Allocate2(ref r, column[0], column[1]);
                    }
                }

                else
                {

                    String ex = column[1];
                    for (int j = 2; j < column.Length; j++)
                    {
                        ex = ex + column[j];
                    }
                    Allocate2(ref r, column[0], ex);
                }

            }



        }
        static void warningParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            //event category
            r.EventType = arr[2];
            Console.Write("eventcategory---)");
            Console.WriteLine(arr[2]);

            //computer name
            r.ComputerName = arr[6];

            Console.Write("computername---)");
            Console.WriteLine(arr[6]);

            //repair    //event type
            Console.Write("eventtype---)");
            String a = line.Substring(55);
            String[] b = a.Split(':');
            if (b[1] != " Repeated")
            {
                String[] c = b[1].TrimStart().Split(',');
                String eType = c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }
            else if (b[1] == " Repeated")
            {
                String[] c = b[2].TrimStart().Split(',');
                String eType = "Repeated: " + c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }



            //repair    //date time
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;
            ////////////////////////////////////////7
            String[] packet = line.Split(',');
            //Console.WriteLine("***"+packet[0]);
            //Console.WriteLine("***" + line);

            for (int i = 1; i < packet.Length; i++)
            {
                String[] column = packet[i].TrimStart().Split('=');

                if (column[0] != "Target")
                {
                    //Console.WriteLine("zaq" + column[0]);
                    if (column.Length != 1)
                    {
                        Allocate2(ref r, column[0], column[1]);
                    }
                }

                else
                {

                    String ex = column[1];
                    for (int j = 2; j < column.Length; j++)
                    {
                        ex = ex + column[j];
                    }
                    Allocate2(ref r, column[0], ex);
                }

            }



        }
        static void errorParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            //event category
            r.EventType = arr[2];
            Console.Write("eventcategory---)");
            Console.WriteLine(arr[2]);

            //computer name
            r.ComputerName = arr[6];

            Console.Write("computername---)");
            Console.WriteLine(arr[6]);

            //repair    //event type
            Console.Write("eventtype---)");
            String a = line.Substring(55);
            String[] b = a.Split(':');
            if (b[1] != " Repeated")
            {
                String[] c = b[1].TrimStart().Split(',');
                String eType = c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }
            else if (b[1] == " Repeated")
            {
                String[] c = b[2].TrimStart().Split(',');
                String eType = "Repeated: " + c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }



            //repair    //date time
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;
            ////////////////////////////////////////7
            String[] packet = line.Split(',');
            //Console.WriteLine("***"+packet[0]);
            //Console.WriteLine("***" + line);

            for (int i = 1; i < packet.Length; i++)
            {
                String[] column = packet[i].TrimStart().Split('=');

                if (column[0] != "Target")
                {
                    //Console.WriteLine("zaq" + column[0]);
                    if (column.Length != 1)
                    {
                        Allocate2(ref r, column[0], column[1]);
                    }
                }

                else
                {

                    String ex = column[1];
                    for (int j = 2; j < column.Length; j++)
                    {
                        ex = ex + column[j];
                    }
                    Allocate2(ref r, column[0], ex);
                }

            }



        }
        static void criticalParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            //event category
            r.EventType = arr[2];
            Console.Write("eventcategory---)");
            Console.WriteLine(arr[2]);

            //computer name
            r.ComputerName = arr[6];

            Console.Write("computername---)");
            Console.WriteLine(arr[6]);

            //repair    //event type
            Console.Write("eventtype---)");
            String a = line.Substring(55);
            String[] b = a.Split(':');
            if (b[1] != " Repeated")
            {
                String[] c = b[1].TrimStart().Split(',');
                String eType = c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }
            else if (b[1] == " Repeated")
            {
                String[] c = b[2].TrimStart().Split(',');
                String eType = "Repeated: " + c[0];
                Console.WriteLine(eType);
                r.EventCategory = eType;
            }



            //repair    //date time
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;
            ////////////////////////////////////////7
            String[] packet = line.Split(',');
            //Console.WriteLine("***"+packet[0]);
            //Console.WriteLine("***" + line);

            for (int i = 1; i < packet.Length; i++)
            {
                String[] column = packet[i].TrimStart().Split('=');

                if (column[0] != "Target")
                {
                    //Console.WriteLine("zaq" + column[0]);
                    if (column.Length != 1)
                    {
                        Allocate2(ref r, column[0], column[1]);
                    }
                }

                else
                {

                    String ex = column[1];
                    for (int j = 2; j < column.Length; j++)
                    {
                        ex = ex + column[j];
                    }
                    Allocate2(ref r, column[0], ex);
                }

            }



        }
        static String dateandtime(String w3, String w4, String w5)
        {
            //String dType = arr[3] + " " + arr[4] + " " + arr[5];
            String mounth, day, year, time;
            day = w4;
            time = w5;
            year = DateTime.Now.Year.ToString();
            switch (w3)
            {
                case "Jan":
                    mounth = "01";
                    break;
                case "Feb":
                    mounth = "02";
                    break;
                case "Mar":
                    mounth = "03";
                    break;
                case "Apr":
                    mounth = "04";
                    break;
                case "May":
                    mounth = "05";
                    break;
                case "Jun":
                    mounth = "06";
                    break;
                case "Jul":
                    mounth = "07";
                    break;
                case "Aug":
                    mounth = "08";
                    break;
                case "Sep":
                    mounth = "09";
                    break;
                case "Oct":
                    mounth = "10";
                    break;
                case "Nov":
                    mounth = "11";
                    break;
                case "Dec":
                    mounth = "12";
                    break;
                default:
                    mounth = w3;
                    break;

            }
            String dType = year + "/" + mounth + "/" + day + " " + time;
            return dType;
        }
        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec r = new CustomBase.Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {
                /*
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                rec.LogName = "Syslog Recorder";
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;
                rec.SourceName = args.Source;
                 */
                L.Log(LogType.FILE, LogLevel.DEBUG, "args.message" + args.Message);
                r.LogName = "SgsSyslog Recorder";
                Console.WriteLine();
                String[] array = new String[100];
                String line = args.Message;

                //String[] arr = SpaceSplit(args.Message.Replace('\0', ' '), true);
                array = SpaceSplit(line, true);
                L.Log(LogType.FILE, LogLevel.DEBUG, "array[5]:" + array[5]);
                if (array.Length != 0)
                {

                    if (array[2].ToLower() == "system.info")
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "sgs system.info");
                        infoParser(ref r, array, line);
                    }
                    else if (array[2].ToLower() == "system.notice")
                    {
                        noticeParser(ref r, array, line);
                    }
                    else if (array[2].ToLower() == "system.warning")
                    {
                        warningParser(ref r, array, line);
                    }
                    else if (array[2].ToLower() == "system.error")
                    {
                        errorParser(ref r, array, line);
                    }
                    else if (array[2].ToLower() == "system.critical")
                    {
                        criticalParser(ref r, array, line);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                L.Log(LogType.FILE, LogLevel.DEBUG, "r.datetime:" + r.Datetime);
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(r);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal,virtualhost,r);
                    s.SetReg(Id, r.Datetime, "","","",r.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SymantecSgsSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSgsSyslogRecorder").GetValue("Syslog Port"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("SymantecSgsSyslogRecorder").GetValue("Protocol").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSgsSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SgsSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SymantecSgsSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
