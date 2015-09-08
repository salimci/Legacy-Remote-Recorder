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

namespace GenuagateSyslogRecorder
{
    public class GenuagateSyslogRecorder:CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514;
        private static int zone = 0;
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
                EventLog.WriteEntry("Security Manager GenuagateSyslogRecorder Read Registry", "GenuagateSyslog Recorder may not working properly ", EventLogEntryType.Error);
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
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost,String dal, Int32 Zone)
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GenuagateSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GenuagateSyslog Recorder functions may not be running");
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

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening GenuagateSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Genuagate_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing GenuagateSyslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GenuagateSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\GenuagateSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GenuagateSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public GenuagateSyslogRecorder()
		{
            /*
            try
            {
                //InitializeComponent();
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing GenuagateSyslogRecorder Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening GenuagateSyslogRecorder on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + GenuagateSyslog_Port.ToString());
                
                if(protocol=="TCP")
                slog = new GenuagateSyslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), GenuagateSyslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                else
                slog = new GenuagateSyslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), GenuagateSyslog_Port, System.Net.Sockets.ProtocolType.Udp);
                
                slog.Start();
                slog.SyslogEvent += new GenuagateSyslog.SyslogEventDelegate(Genuagate_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing GenuagateSyslogRecorder Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GenuagateSyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
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
            try
            {
                //Console.WriteLine("."+key);
                switch (key)
                {
                    case "caddr":
                        r.CustomStr1 = value;
                        Console.WriteLine("caddr OK" + value);
                        break;
                    case "sport":
                        r.CustomInt1 = Convert.ToInt32(value);
                        Console.WriteLine("sport OK" + value);
                        break;
                    case "url":
                        r.CustomStr10 = value;
                        Console.WriteLine("URL OK" + value);
                        break;
                    case "URL":
                        r.CustomStr10 = value;
                        Console.WriteLine("URL OK" + value);
                        break;
                    case "method":
                        r.CustomStr2 = value;
                        Console.WriteLine("method OK" + value);
                        break;
                    case "status":
                        r.CustomStr3 = value;
                        Console.WriteLine("status OK" + value);
                        break;
                    case "type":
                        r.CustomStr4 = value;
                        Console.WriteLine("type OK" + value);
                        break;
                    case "duration":
                        r.CustomStr5 = value;
                        Console.WriteLine("duration OK" + value);
                        break;
                    case "rnum":
                        r.CustomStr6 = value;
                        Console.WriteLine("rnum OK" + value);
                        break;
                    case "relay":
                        r.CustomStr9 = value;
                        Console.WriteLine("relay OK" + value);
                        break;
                    case "bytes":
                        r.CustomStr8 = value;
                        Console.WriteLine("bytes OK" + value);
                        break;
                    case "cin":
                        r.CustomInt2 = Convert.ToInt32(value);
                        Console.WriteLine("cin OK" + value);
                        break;
                    case "cout":
                        r.CustomInt3 = Convert.ToInt32(value);
                        Console.WriteLine("cout OK" + value);
                        break;
                    case "sin":
                        r.CustomInt4 = Convert.ToInt32(value);
                        Console.WriteLine("sin OK" + value);
                        break;
                    case "sout":
                        r.CustomInt5 = Convert.ToInt32(value);
                        Console.WriteLine("sout OK" + value);
                        break;
                    default:
                        r.Description += key + "= ";
                        r.Description += value + " ,";
                        //Console.WriteLine("Description" + r.Description);
                        break;

                };
            }//try
            catch 
            {
                
            }
        }
        static void deamonParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            r.CustomStr9 = arr[7];

            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = Convert.ToDateTime(dType).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
            

            if (arr[10] == "New" && arr[11] == "Request")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "status=OK")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "cleanup")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "child" && arr[11] == "exit")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else
            {
                //Console.WriteLine("******************************************** emrah *********************************");
                //Console.WriteLine(arr[11]);
                r.EventType = "info";
            }



            for (int i = 11; i < arr.Length; i++)
            {

                String[] column = arr[i].TrimStart().Split('=');

                if (column.Length > 1)
                {
                    Allocate2(ref r, column[0], column[1]);
                }
                else if (column.Length <= 1 && arr[i - 1] != "url=")
                {
                    r.Description += " " + arr[i];
                    Console.WriteLine(column[0] + "a " + arr[i]);
                }
                else if (column.Length <= 1 && arr[i - 1] == "url=")
                {
                    r.CustomStr10 = arr[i];
                    Console.WriteLine("urllll " + arr[i]);
                }
            }

        }
        static void kernelParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            r.CustomStr9 = arr[7];

            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;
                //Console.WriteLine("******************************************** emrah *********************************");
                //Console.WriteLine(arr[11]);
                r.EventType = "info";
            
            for (int i = 11; i < arr.Length; i++)
            {

                String[] column = arr[i].TrimStart().Split('=');
                
                if (column.Length > 1)
                {
                    Allocate2(ref r, column[0], column[1]);
                }
                else if (column.Length <= 1 && arr[i - 1] != "url=")
                {
                    r.Description += " " + arr[i];
                    Console.WriteLine(column[0] + "a " + arr[i]);
                }
                else if (column.Length <= 1 && arr[i - 1] == "url=")
                {
                    r.CustomStr10 = arr[i];
                    Console.WriteLine("urllll " + arr[i]);
                }
            }

        }
        static void snoticeParser(ref CustomBase.Rec r, String[] arr, String line)
        {

            r.CustomStr9 = arr[6];
            
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;

            if (arr[10] == "request")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "disconnect")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "Greylisting:")
            {
                Console.WriteLine(arr[9]);
                r.EventType = arr[9];
            }
            else if (arr[10] == "INFO" && arr[11] == "connecting" && arr[13] == "vscand")
            {
                Console.WriteLine(arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13]);
                r.EventType = arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13];
            }
            else if (arr[10] == "Global" && arr[11] == "Timeout.")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "double" && arr[11] == "close")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "ACCESS" && arr[11] == "DENIED:")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "INFO" && arr[11] != "connecting")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "closefd" && arr[11] == "on")
            {
                Console.WriteLine(arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14]);
                r.EventType = arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
            }
            else if (arr[10] == "forwarding")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "Warning:")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "rrd_acct")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "exit")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[9] == "child" && arr[10] == "exit")
            {
                Console.WriteLine(arr[9] + " " + arr[10]);
                r.EventType = arr[9] + " " + arr[10];
            }
            else if (arr[10] == "SOCKET" && arr[11] == "ERROR:")
            {
                Console.WriteLine(arr[11] + " " + arr[12]);
                r.EventType = arr[11] + " " + arr[12];
            }
            else if (arr[10] == "RBL:" && arr[11] == "denied:")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else
            {
                //Console.WriteLine("******************************************** emrah *********************************");
                //Console.WriteLine(arr[11]);
                r.EventType = "info";
            }



            for (int i = 11; i < arr.Length; i++)
            {

                String[] column = arr[i].TrimStart().Split('=');

                if (column.Length > 1)
                {
                    Allocate2(ref r, column[0], column[1]);
                }
                else if (column.Length <= 1 && arr[i - 1] != "url=")
                {
                    r.Description += " " + arr[i];
                    Console.WriteLine(column[0] + "a " + arr[i]);
                }
                else if (column.Length <= 1 && arr[i - 1] == "url=")
                {
                    r.CustomStr10 = arr[i];
                    Console.WriteLine("urllll " + arr[i]);
                }
            }

        }
        static void noticeParser(ref CustomBase.Rec r, String[] arr, String line)
        {
            r.CustomStr9 = arr[7];
            String dType = dateandtime(arr[3], arr[4], arr[5]);
            Console.Write("dateTime---)");
            Console.WriteLine(dType);
            r.Datetime = dType;

            if (arr[10] == "request")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "disconnect")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "Greylisting:")
            {
                Console.WriteLine(arr[9]);
                r.EventType = arr[9];
            }
            else if (arr[10] == "INFO" && arr[11] == "connecting" && arr[13] == "vscand")
            {
                Console.WriteLine(arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13]);
                r.EventType = arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13];
            }
            else if (arr[10] == "Global" && arr[11] == "Timeout.")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "double" && arr[11] == "close")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "ACCESS" && arr[11] == "DENIED:")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "INFO" && arr[11] != "connecting")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else if (arr[10] == "closefd" && arr[11] == "on")
            {
                Console.WriteLine(arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14]);
                r.EventType = arr[10] + " " + arr[11] + " " + arr[12] + " " + arr[13] + " " + arr[14];
            }
            else if (arr[10] == "forwarding")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "Warning:")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "rrd_acct")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[10] == "exit")
            {
                Console.WriteLine(arr[10]);
                r.EventType = arr[10];
            }
            else if (arr[9] == "child" && arr[10] == "exit")
            {
                Console.WriteLine(arr[9] + " " + arr[10]);
                r.EventType = arr[9] + " " + arr[10];
            }
            else if (arr[10] == "SOCKET" && arr[11] == "ERROR:")
            {
                Console.WriteLine(arr[11] + " " + arr[12]);
                r.EventType = arr[11] + " " + arr[12];
            }
            else if (arr[10] == "RBL:" && arr[11] == "denied:")
            {
                Console.WriteLine(arr[10] + " " + arr[11]);
                r.EventType = arr[10] + " " + arr[11];
            }
            else
            {
                //Console.WriteLine("******************************************** emrah *********************************");
                //Console.WriteLine(arr[11]);
                r.EventType = "info";
            }



            for (int i = 11; i < arr.Length; i++)
            {

                String[] column = arr[i].TrimStart().Split('=');
                
                if (column.Length > 1)
                {
                    Allocate2(ref r, column[0], column[1]);
                }
                else if (column.Length <= 1 && arr[i - 1] != "url=")
                {
                    r.Description += " " + arr[i];
                    Console.WriteLine(column[0] + "a " + arr[i]);
                }
                else if (column.Length <= 1 && arr[i - 1] == "url=")
                {
                    r.CustomStr10 = arr[i];
                    Console.WriteLine("urllll " + arr[i]);
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
        void Genuagate_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                rec.LogName = "GenuagateSyslog Recorder";
                L.Log(LogType.FILE, LogLevel.DEBUG, "args.message->" + args.Message);
                String[] arr = SpaceSplit(args.Message, true);
                // CustomBase.Rec r = new CustomBase.Rec();

                rec.ComputerName = arr[0];
                //rec.EventType = arr[1];

                DateTime dt = DateTime.Parse(arr[4] + " " + arr[3] + " " + DateTime.Now.Year + " " + arr[5]);
                rec.Datetime = dt.Day + "/" + dt.Month + "/" + dt.Year + " " + arr[5];

                rec.EventCategory = arr[10];

                Dictionary<String, String> dictTemp = new Dictionary<String, String>();

                switch (rec.EventCategory)
                {
                    case "request":
                    case "accept":
                    case "connect":
                    case "disconnect":
                        {
                            for (Int32 i = 11; i < arr.Length; i++)
                            {
                                String[] arrTemp = arr[i].Split('=');
                                if (arrTemp.Length > 1)
                                {
                                    dictTemp.Add(arrTemp[0], arrTemp[1]);
                                }
                            }

                            try
                            {
                                rec.CustomStr6 = dictTemp["laddr"];
                            }
                            catch
                            {
                                rec.CustomStr6 = "";
                            }
                            try
                            {
                                rec.CustomInt1 = Convert.ToInt32(dictTemp["lport"]);
                            }
                            catch
                            {
                                rec.CustomInt1 = -1;
                            }
                            try
                            {
                                rec.CustomStr2 = dictTemp["baddr"];
                            }
                            catch
                            {
                                rec.CustomStr2 = "";
                            }
                            try
                            {
                                rec.CustomInt2 = Convert.ToInt32(dictTemp["bport"]);
                            }
                            catch
                            {
                                rec.CustomInt2 = -1;
                            }
                            try
                            {
                                rec.CustomStr3 = dictTemp["caddr"];
                            }
                            catch
                            {
                                rec.CustomStr3 = "";
                            }
                            try
                            {
                                rec.CustomInt3 = Convert.ToInt32(dictTemp["cport"]);
                            }
                            catch
                            {
                                rec.CustomInt3 = -1;
                            }
                            try
                            {
                                rec.CustomStr4 = dictTemp["saddr"];
                            }
                            catch
                            {
                                rec.CustomStr4 = "";
                            }
                            try
                            {
                                rec.CustomInt4 = Convert.ToInt32(dictTemp["sport"]);
                            }
                            catch
                            {
                                rec.CustomInt4 = -1;
                            }
                            try
                            {
                                rec.Description = dictTemp["url"];
                            }
                            catch
                            {
                                rec.Description = "";
                            }
                            try
                            {
                                rec.CustomStr5 = dictTemp["duration"];
                            }
                            catch
                            {
                                rec.CustomStr5 = "";
                            }
                            try
                            {
                                rec.CustomStr1 = dictTemp["rnum"];
                            }
                            catch
                            {
                                rec.CustomStr1 = "";
                            }
                            try
                            {
                                rec.CustomStr7 = dictTemp["status"];
                            }
                            catch
                            {
                                rec.CustomStr7 = "";
                            }
                            try
                            {
                                rec.CustomStr8 = dictTemp["type"];
                            }
                            catch
                            {
                                rec.CustomStr8 = "";
                            }

                            dictTemp.Clear();
                        } break;
                    case "ACCESS":
                        {
                            rec.EventCategory += " " + arr[11];

                            rec.CustomStr10 = "";

                            Int32 i = 12;
                            for (i = 12; i < arr.Length; i++)
                            {
                                if (Char.IsDigit(arr[i], 0))
                                {
                                    break;
                                }
                                rec.CustomStr10 += arr[i] + " ";
                            }
                            rec.CustomStr10 = rec.CustomStr10.Trim();

                            for (; i < arr.Length; i++)
                            {
                                if (arr[i].Contains("from"))
                                    break;
                            }
                            i++;

                            String[] arrTemp = arr[i].Split(':');
                            rec.CustomStr3 = arrTemp[0];
                            try
                            {
                                rec.CustomInt3 = Convert.ToInt32(arrTemp[1]);
                            }
                            catch
                            {
                            }
                            i += 2;

                            arrTemp = arr[i].Split(':');
                            rec.CustomStr2 = arrTemp[0];
                            try
                            {
                                rec.CustomInt2 = Convert.ToInt32(arrTemp[1]);
                            }
                            catch
                            {
                            }

                        } break;
                };

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                
                L.Log(LogType.FILE, LogLevel.ERROR, "args.message->" + args.Message);
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\GenuagateSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GenuagateSyslogRecorder").GetValue("Syslog Port"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("GenuagateSyslogRecorder").GetValue("Protocol").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GenuagateSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GenuagateSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager GenuagateSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }   
    }
}
