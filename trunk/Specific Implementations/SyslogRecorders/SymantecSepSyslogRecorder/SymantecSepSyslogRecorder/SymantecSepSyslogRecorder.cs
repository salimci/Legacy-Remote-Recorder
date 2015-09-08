using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SymantecSepSyslogRecorder
{
    public class SymantecSepSyslogRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514;
        private static int zone;
        private string err_log, protocol = "TCP", location = "", remote_host = "localhost";
        private bool reg_flag;
        private CLogger L;
        public Syslog slog;
        protected Int32 Id;
        protected String virtualhost, Dal;
        protected bool usingRegistry = true;
        private ProtocolType pro;

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSepSyslog Recorder functions may not be running");
                            return;
                        }
                        reg_flag = true;

                        pro = protocol.ToUpper() == "TCP" ? ProtocolType.Tcp : ProtocolType.Udp;
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSepSyslog Recorder functions may not be running");
                            return;
                        }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        if (location.Contains(':'.ToString(CultureInfo.InvariantCulture)))
                        {
                            protocol = location.Split(':')[0];
                            Syslog_Port = Convert.ToInt32(location.Split(':')[1]);
                            pro = protocol.ToLower() == "tcp" ? ProtocolType.Tcp : ProtocolType.Udp;
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

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SymantecSepSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                if (usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0] + " port: " + Syslog_Port.ToString(CultureInfo.InvariantCulture));
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + Syslog_Port.ToString(CultureInfo.InvariantCulture));
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                slog.Start();
                slog.SyslogEvent += Sep_SyslogEvent;

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SymantecSepSyslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSepSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Remote Recorder");
                    if (registryKey != null)
                        err_log = registryKey.GetValue("Home Directory") + @"log\SymantecSepSyslogRecorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSepSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        void Sep_SyslogEvent(LogMgrEventArgs args)
        {
            var r = new Rec();
            L.Log(LogType.FILE, LogLevel.DEBUG, " Sep_SyslogEvent() Started.");
            L.Log(LogType.FILE, LogLevel.DEBUG, " Sep_SyslogEvent() Line : " + args.Message);

            r.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;

            r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            int control = 0;
            try
            {
                String line = "";
                // e L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                r.LogName = "SymantecSepSyslog Recorder";
                //rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                //r.EventType = args.EventLogEntType.ToString();
                //r.EventCategory = args.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Log is:" + args.Message);

                //main main main
                line = args.Message;
                var array = new String[100];
                //emr for virus found and array[0] controll
                var array2 = new String[100];
                array2 = SpaceSplit(line, true);
                array = line.Split(',');
                var temp3 = new String[100];
                temp3 = array2[7].Split(':');
                r.ComputerName = temp3[0];
                if (array.Length != 0)
                {
                    if (line.Contains("Virus found"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Virus found");
                        r.EventCategory = "Virus found";
                        r.SourceName = array2[0];
                        virusFound(ref r, line);
                    }
                    else if (line.Contains("Forced TruScan proactive threat detected"))
                    {
                        r.EventCategory = "Forced TruScan proactive threat detected";
                        r.SourceName = array2[1];
                        forcedTruScanProactive(ref r, line);
                    }
                    else if (line.Contains("Scan ID"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Scan");
                        r.EventCategory = "scan";
                        r.SourceName = array2[0];
                        scanComplete(ref r, line);
                    }
                    else if (line.Contains("Could not scan"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Could not scan");
                        r.EventCategory = "Could not scan";
                        r.SourceName = array2[0];
                        couldnotScan(ref r, line);
                    }
                    else if (line.Contains("client has downloaded the content package"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "client has downloaded the content package");
                        r.EventCategory = "client has downloaded the content package";
                        r.SourceName = array2[0];
                        contentPackage(ref r, line);
                        if (string.IsNullOrEmpty(r.Datetime))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Colud not set DateTime, log is ignored..");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Log:" + line);
                        }
                    }
                    else if (line.Contains("LiveUpdate"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LiveUpdate");
                        r.EventCategory = "LiveUpdate";
                        r.SourceName = array2[0];
                        LiveUpdate(ref r, line);

                    }
                    else if (line.Contains("Network Threat Protection is unable to download the newest policy"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Network Threat Protection is unable to download the newest policy");
                        r.EventCategory = "Network Threat Protection is unable to download the newest policy";
                        r.SourceName = array2[0];
                        unableToDownload(ref r, line);

                    }
                    else if (line.Contains("New virus definition file loaded"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "New virus definition file loaded");
                        r.EventCategory = "New virus definition file loaded";
                        r.SourceName = array2[0];
                        definitionFileLoaded(ref r, line);

                    }
                    else if (line.Contains("services shutdown"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "services shutdown");
                        r.EventCategory = "services shutdown";
                        r.SourceName = array2[0];
                        definitionFileLoaded(ref r, line);

                    }
                    else if (line.Contains("services startup "))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "services startup ");
                        r.EventCategory = "services startup";
                        r.SourceName = array2[0];
                        definitionFileLoaded(ref r, line);

                    }
                    else if (line.Contains("Auto-Protect failed "))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Auto-Protect failed");
                        r.EventCategory = "Auto-Protect failed";
                        r.SourceName = array2[0];
                        autoProtectedFailed(ref r, line);

                    }
                    else if (line.Contains("disable"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "disable");
                        r.EventCategory = "disable";
                        r.SourceName = array2[0];
                        disable(ref r, line);

                    }
                    else if (line.Contains("Failed to contact server"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Failed to contact server");
                        r.EventCategory = "Failed to contact server";
                        r.SourceName = array2[0];
                        failedToContact(ref r, line);

                    }//bundan sonrakiler obey edilcek satýlar  , ,,
                    else if (line.Contains("Block IPv6") || line.Contains("Traffic from IP address") || line.Contains("Not in GZIP format") || line.Contains("received the client log") || line.Contains("Block all other traffic"))
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "Category: Block IPv6,Traffic from IP address,Not in GZIP format,received the client log,Block all other traffic");
                        control = 1;
                    }
                    else if (line.Contains("Block and log IP traffic") || line.Contains("Host Integrity") || line.Contains("Location has been changed to Default.") || line.Contains("has been activated."))
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "Category: Block IPv6,Traffic from IP address,Not in GZIP format,received the client log,Block all other traffic");
                        control = 1;
                    }
                    else
                    {
                        control = 1;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "Unknown or not wanted log format. All data written to description field." + args.Message);
                        r.Description = args.Message;
                    }
                }
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }

            try
            {
                if (control != 1)
                {
                    r.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    r.Description = args.Message.Length > 899 ? args.Message.Substring(0, 899) : args.Message;
                    //e L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                    //e L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, r);
                    s.SetReg(Id, r.Datetime, "", "", "", r.Datetime);
                    //e L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Record sending Error.");
            }
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SymantecSepSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSepSyslogRecorder").GetValue("Syslog Port"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("SymantecSepSyslogRecorder").GetValue("Protocol").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecSepSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecSepSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SymantecSepSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        static void scanComplete(ref CustomBase.Rec r, String line)
        {

            int file = 0;//completed scan için
            String[] arr = new String[100];
            arr = line.Split(',');
            for (int i = 1; i < arr.Length; i++)
            {
                //

                string[] temp = new String[100];
                string[] temp2 = new String[100];
                Boolean b;
                b = arr[i].Contains("Begin:");
                if (b == false)
                {
                    //tüm par için
                    temp = arr[i].Split(':');
                    // temp kontrolü : olmayan satýrlar için
                    if (temp.Length > 1)
                    {
                        Allocate2(ref r, temp[0], temp[1].TrimEnd());
                        if (temp[0] == "End")
                        {
                            file = i;
                        }
                    }
                }
                else if (b)
                {
                    //dateandtime için sadece 
                    //2009-03-09 09:00:00

                    temp2 = SpaceSplit(arr[i], false);
                    String date = temp2[1].Trim().Replace('-', '/') + " " + temp2[2].Trim();
                    DateTime dt = Convert.ToDateTime(date);
                    r.Datetime = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    CLogger L2 = new CLogger();
                    L2.Log(LogType.FILE, LogLevel.DEBUG, "date" + date);
                }
            }
            r.CustomStr9 = arr[file + 1];
        }
        static void virusFound(ref CustomBase.Rec r, String line)
        {
            int file = 0;
            String[] arr = new String[100];
            arr = line.Split(',');
            for (int i = 1; i < arr.Length; i++)
            {
                //
                string[] temp = new String[100];
                string[] temp2 = new String[100];
                Boolean b;
                b = arr[i].Contains("Event time");
                if (b == false)
                {
                    //tüm par için
                    temp = arr[i].Split(':');
                    // temp kontrolü : olmayan satýrlar için
                    if (temp.Length > 1)
                    {
                        Allocate2(ref r, temp[0], temp[1].TrimEnd());
                        if (temp[0] == "Occurrences")
                        {
                            file = i;
                        }
                    }
                }
                else if (b == true)
                {
                    //dateandtime için sadece 
                    //2009-03-09 09:00:00
                    temp2 = SpaceSplit(arr[i], false);

                    String date = temp2[2].Trim().Replace('-', '/') + " " + temp2[3].Trim();
                    DateTime dt = Convert.ToDateTime(date);
                    r.Datetime = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

                    CLogger L2 = new CLogger();
                    L2.Log(LogType.FILE, LogLevel.DEBUG, "date" + date);
                }
            }
            //virus file
            r.CustomStr10 = arr[file + 1];

        }
        static void forcedTruScanProactive(ref CustomBase.Rec r, String line)
        {
            int file = 0;
            String[] arr = new String[100];
            arr = line.Split(',');
            for (int i = 1; i < arr.Length; i++)
            {
                //
                string[] temp = new String[100];
                string[] temp2 = new String[100];
                Boolean b;
                b = arr[i].Contains("Event time");
                if (b == false)
                {
                    //tüm par için
                    temp = arr[i].Split(':');
                    // temp kontrolü : olmayan satýrlar için
                    if (temp.Length > 1)
                    {
                        Allocate2(ref r, temp[0], temp[1].TrimEnd());
                        if (temp[0] == "Occurrences")
                        {
                            file = i;
                        }
                    }
                }
                else if (b)
                {
                    //dateandtime için sadece 
                    //2009-03-09 09:00:00
                    temp2 = SpaceSplit(arr[i], false);

                    String date = temp2[2].Trim().Replace('-', '/') + " " + temp2[3].Trim();
                    DateTime dt = Convert.ToDateTime(date);
                    r.Datetime = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                }
            }
            //virus file
            r.CustomStr10 = arr[file + 1];

        }
        static void couldnotScan(ref CustomBase.Rec r, String line)
        {
            int file = 0;
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);

            for (int i = 0; i < arr2.Length; i++)
            {
                if (arr2[i].Contains("inside"))
                {
                    file = i;
                }
            }
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

            for (int j = (file + 1); j < arr2.Length; j++)
            {
                r.CustomStr10 += arr2[j];
            }
        }
        static void contentPackage(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');

            string[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            //arr2 = null;
            r.CustomStr4 = arr[4];
            r.UserName = arr[5];

            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);
        }
        static void LiveUpdate(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');

            string[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.Description = arr[2];
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void unableToDownload(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[2];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void failedToContact(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[2];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void definitionFileLoaded(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void startup(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void shutdown(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void autoProtectedFailed(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void upToDate(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

        }
        static void disable(ref CustomBase.Rec r, String line)
        {
            String[] arr = new String[100];
            arr = line.Split(',');
            String[] arr2 = new String[100];
            arr2 = SpaceSplit(arr[0], false);
            r.CustomStr4 = arr2[8];
            //arr2 = null;
            arr2 = SpaceSplit(line, false);
            r.Description = arr[3];
            //arr2 = null;
            r.Datetime = dateandtime(arr2[3], arr2[4], arr2[5]);

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
                    mounth = DateTime.Now.Month.ToString();
                    break;

            }
            String dType = mounth + "/" + day + "/" + year + " " + time;
            Boolean dtError = false;
            DateTime dt = DateTime.MinValue;
            try
            {
                dt = Convert.ToDateTime(dType);
            }
            catch
            {
                dtError = true;
            }
            if (dtError)
            {
                try
                {
                    dType = day + "/" + mounth + "/" + year + " " + time;
                    dt = Convert.ToDateTime(dType);
                }
                catch
                {
                    return "";
                }
            }
            dType = dt.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

            CLogger L2 = new CLogger();
            L2.Log(LogType.FILE, LogLevel.DEBUG, "date" + dType);

            return dType;
        }

        static void Allocate2(ref CustomBase.Rec r, String key, String value)
        {
            switch (key)
            {
                case "Computer name":
                    r.CustomStr4 = value;
                    break;
                case "Computer":
                    r.CustomStr4 = value;
                    break;
                case "Source IP":
                    r.CustomStr3 = value;
                    break;
                case "IP Address":
                    r.CustomStr3 = value;
                    break;
                case "Source":
                    r.CustomStr8 = value;
                    break;
                case "Risk name":
                    r.CustomStr2 = value;
                    break;
                case "Application type":
                    r.CustomStr2 = value;
                    break;
                case "Actual action":
                    r.CustomStr5 = value;
                    break;
                case "Requested action":
                    r.CustomStr6 = value;
                    break;
                case "Secondary action":
                    r.CustomStr7 = value;
                    break;
                case "Server":
                    r.ComputerName = value;
                    break;
                case "User":
                    r.UserName = value;
                    break;
                case "User1":
                    r.UserName = value;
                    break;
                case "Duration (seconds)":
                    r.CustomInt10 = Convert.ToInt32(value);
                    break;
                case "Risks":
                    r.CustomInt9 = Convert.ToInt32(value);
                    break;
                case "Scanned":
                    r.CustomInt8 = Convert.ToInt32(value);
                    break;
                case "Threats":
                    r.CustomInt7 = Convert.ToInt32(value);
                    break;
                case "Infected":
                    r.CustomInt6 = Convert.ToInt32(value);
                    break;
                case "Total files":
                    r.CustomInt5 = Convert.ToInt32(value);
                    break;
                case "Omitted":
                    r.CustomInt5 = Convert.ToInt32(value);
                    break;
                default:
                    r.Description += key + "= ";
                    r.Description += value + " ,";
                    break;

            };
        }

    }
}