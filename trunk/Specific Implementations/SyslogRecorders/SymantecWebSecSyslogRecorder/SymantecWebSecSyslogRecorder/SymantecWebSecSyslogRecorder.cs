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

namespace SymantecWebSecSyslogRecorder
{
    public class SymantecWebSecSyslogRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514;
        private static int zone = 0;
        private string err_log, protocol = "TCP", location = "", remote_host = "localhost";
        private bool reg_flag = false;
        private CLogger L;
        public Syslog slog = null;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        protected bool usingRegistry = true;
        private ProtocolType pro;

        private void InitializeComponent()
        {
            /*
            if (!Read_Registry())
            {
                EventLog.WriteEntry("Security Manager SymantecWebSecSyslogRecorder Read Registry", "SymantecSepSyslog Recorder may not working properly ", EventLogEntryType.Error);
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

        public SymantecWebSecSyslogRecorder()
        {
            /*
            try
            {
                InitializeComponent();
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SymantecWebSecSyslogRecorder Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SymantecWebSecSyslogRecorder on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                if (protocol == "TCP")
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, System.Net.Sockets.ProtocolType.Tcp);
                else
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, System.Net.Sockets.ProtocolType.Udp);

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sep_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SepSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecWebSecSyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSepSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SymantecSepSyslog Recorder functions may not be running");
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sep_SyslogEvent);

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
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SymantecWebSecSyslogRecorder" + Id + ".log";
                rk.Close();
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

        public void Sep_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rRec = new CustomBase.Rec();

            try
            {   
                String line = "";

                L.Log(LogType.FILE, LogLevel.DEBUG, "Log is:" + args.Message);

                line = args.Message.Replace('\0', ' ');

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                
                try
                {   
                    rRec.LogName = "SYMANTECWEBSEC Sys Log Recorder";
                    rRec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    rRec.EventType = args.EventLogEntType.ToString();
                    rRec = str_Parcala(rRec, line);
                    rRec.SourceName = args.Source;
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE,LogLevel.ERROR,exception.Message);
                }
                
                L.Log(LogType.FILE,LogLevel.DEBUG,"Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                   
                if (usingRegistry)
                {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rRec);
                }
                else
                {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rRec);
                        s.SetReg(Id, rRec.Datetime, "", "", "", rRec.Datetime);
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
        }

        private Rec str_Parcala(Rec rRec, string sMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(sMessage))
                {
                    L.Log(LogType.FILE,LogLevel.DEBUG,"str_Parcala | string Null or Empty");
                    rRec.Description = "Log Null or Empty";
                    return rRec;
                }
                
                sMessage = sMessage.Replace('(', ' ');
                sMessage = sMessage.Replace(')', ' ');
                string[] strArray = sMessage.Split(new char[] { ',' });
                if (strArray.Length < 7)
                {   
                    L.Log(LogType.FILE,LogLevel.DEBUG,"Log is not correct format.");

                    rRec.Description = sMessage;
                    
                    return rRec;
                }   
                    
                string[] strArray2 = strArray[0].ToString().Split(new char[] { '[' });
                string[] strArray3 = strArray2[1].ToString().Split(new char[] { ']' });
                string[] strArray4 = strArray2[0].ToString().Trim().Split(new char[] { ':' });
                    
                rRec.SourceName = strArray4[0].ToString();
                    
                rRec.CustomInt5 = Convert.ToInt32(strArray4[1].ToString());
                    
                string str = strArray4[2].ToString().Trim();
                    
                rRec.CustomStr5 = str.Substring(str.Trim().IndexOf(' ') + 1, (str.Length - str.Trim().IndexOf(' ')) - 1).Trim();
                    
                rRec.CustomStr4 = str.Substring(0, str.Trim().IndexOf(' ')).Trim();
                    
                rRec.CustomStr6 = strArray3[0].ToString();
                    
                str = strArray3[1].ToString();
                    
                rRec.CustomInt4 = Convert.ToInt32(str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim());
                    
                str = strArray[1].ToString();
                    
                rRec.CustomStr3 = str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim();
                    
                str = strArray[2].ToString();
                    
                rRec.CustomInt2 = Convert.ToInt32(str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim());
                    
                str = strArray[3].ToString();
                    
                rRec.CustomStr1 = str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim();
                    
                str = strArray[4].ToString();
                    
                rRec.CustomStr2 = str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim();
                    
                str = strArray[5].ToString();
                    
                rRec.CustomInt1 = Convert.ToInt32(str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim());
                    
                str = strArray[6].ToString();
                    
                rRec.Description = str.Substring(str.IndexOf(':') + 1, (str.Length - str.IndexOf(':')) - 1).Trim();
                    
                return rRec;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                return rRec;
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SymantecWebSecSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecWebSecSyslogRecorder").GetValue("Syslog Port"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("SymantecWebSecSyslogRecorder").GetValue("Protocol").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SymantecWebSecSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SymantecWebSecSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SymantecWebSecSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
        
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
       
        public static String[] SpaceSplit(String line, bool useTabs)
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
         
        public static String dateandtime(String w3, String w4, String w5)
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
    }
}