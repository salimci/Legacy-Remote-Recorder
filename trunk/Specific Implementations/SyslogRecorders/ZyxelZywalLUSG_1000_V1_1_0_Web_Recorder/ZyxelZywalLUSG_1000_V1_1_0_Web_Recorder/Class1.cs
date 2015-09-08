// Writer: Onur Sarıkaya
// TCDD Afyon 7.bölge
// 26.02.2013


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CustomTools;
using Log;
using LogMgr;
using Microsoft.Win32;

namespace ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder
{
    public class ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder : CustomBase
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
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

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
        } // SetConfigData

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Recorder functions may not be running");
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(Sgs_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        } // Init 

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Get_logDir

        public ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder()
        {

        }

        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() | Start preparing record");
                string sLogLine = args.Message;
                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() | start line=" + sLogLine);

                if (string.IsNullOrEmpty(sLogLine) == true)
                {
                    L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Line İs Null or Empty");
                    return;
                }

                rec.LogName = "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder";
                rec = LogFormat_Message(sLogLine, rec);


                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder()  EventLogEntType: " + args.EventLogEntType.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder()  Source: " + args.Source.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder()  Message: " + args.Message.ToString());

                rec.SourceName = args.Source;

                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() | Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() |  Finish Sending Data");
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() | " + er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() | " + args.Message);
            }
        } // Sgs_SyslogEvent

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

        private Rec LogFormat_Message(string sLogLine, Rec rec)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
                L.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + sLogLine);
                rec.Description = sLogLine;

                //if (rec.Description.Length > 900)
                //{
                //    rec.Description = rec.Description.Substring(0, 899);
                //}

                char c = '"';
                string strPort1 = Between(sLogLine, "dst=", "msg=").Replace('"', ' ').Trim();
                L.Log(LogType.FILE, LogLevel.DEBUG, "strPort1:" + strPort1);

                try
                {
                    if (strPort1.Contains(':'))
                    {
                        rec.CustomStr3 = strPort1.Split(':')[0].ToString();
                        //rec.CustomInt1 = Convert.ToInt32(strPort1.Split(':')[1]);

                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3:" + rec.CustomStr3);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1:" + rec.CustomInt1);
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR1" + exception.StackTrace);
                }

                try
                {
                    rec.EventCategory = Between(sLogLine, "cat=", c.ToString()).Replace('"', ' ').Trim();
                }
                catch (Exception exception)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, "EventCategory" + exception.StackTrace);
                }

                try
                {
                    rec.EventType = Between(sLogLine, "note=", "user=").Replace('"', ' ').Trim();
                }
                catch (Exception exception)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, "EventType" + exception.StackTrace);
                }


                try
                {
                    rec.UserName = Between(sLogLine, "user=", "devID=").Replace('"', ' ').Trim();
                }
                catch (Exception exception)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, "UserName" + exception.StackTrace);
                }

                try
                {
                    string srcIp = Between(sLogLine, "src=", "dst=").Replace('"', ' ').Trim();
                    if (srcIp.Contains(":"))
                    {
                        rec.CustomStr1 = srcIp.Split(':')[0];

                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(srcIp.Split(':')[1]);
                        }
                        catch (Exception exception)
                        {
                            L.LogTimed(LogType.FILE, LogLevel.ERROR, "CustomInt1" + exception.Message);      
                        }
                    }
                    
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1" + exception.StackTrace);
                }

                try
                {
                    rec.CustomStr2 = Between(sLogLine, "devID=", "cat=").Replace('"', ' ').Trim();
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2" + exception.StackTrace);
                }


                try
                {
                    //string dateString = Before(sLogLine, "zywall-usg-300");
                    //string day = dateString.Split(' ')[4];
                    //string month = dateString.Split(' ')[3];
                    //string year = dateString.Split(' ')[6];
                    //string time = dateString.Split(' ')[5];

                    //DateTime dt;
                    //string myDateTimeString = day + month + "," + year + "," + time;
                    //dt = Convert.ToDateTime(myDateTimeString);
                    //string lastDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    //rec.Datetime = lastDate;

                    string[] lineArr = SpaceSplit(sLogLine, false);
                    DateTime dt;
                    string dateNow = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
                    string myDateTimeString = lineArr[4] + lineArr[3] + "," + dateNow + "  ," + lineArr[5];
                    dt = Convert.ToDateTime(myDateTimeString);
                    rec.Datetime = dt.ToString(dateFormat);

                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Date time error : " + exception.StackTrace);
                }

                string strPort2 = Between(sLogLine, "dst=", "msg=").Replace('"', ' ').Trim();
                L.Log(LogType.FILE, LogLevel.DEBUG, "strPort2:" + strPort2);
                try
                {
                    if (strPort2.Contains(':'))
                    {
                        rec.CustomStr4 = strPort2.Split(':')[0].Replace('"', ' ').Trim();
                        rec.CustomInt3 = Convert.ToInt32(strPort2.Split(':')[1]);
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "strPort2: " + strPort2);
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR2" + exception.StackTrace);
                }

                try
                {
                    string str = Between(sLogLine, "msg=", "note=").Replace('"', ' ').Trim();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "str : " + str);

                    if (str.Contains(":"))
                    {
                        rec.CustomStr6 = str.Split(':')[0].Trim('"');
                        rec.CustomStr5 = str.Split(':')[1].Trim('"');
                    }
                    else
                    {
                        rec.CustomStr6 = str;
                        rec.CustomStr5 = "";
                    }//
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Custom str5 ve 6 Error : " + exception.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "action sonrası");
                L.Log(LogType.FILE, LogLevel.DEBUG, "description : " + rec.Description);

                if (!string.IsNullOrEmpty(rec.Description))
                {
                    if (sLogLine.Length > 900)
                    {
                        rec.Description = sLogLine.Substring(0, 899).ToString(CultureInfo.InvariantCulture); // ok
                    }
                    else
                    {
                        rec.Description = sLogLine;
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder() |  Finish Sending Data");
                return rec;
            }
            catch (Exception exp)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, exp.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() | Line " + sLogLine);
                rec.Description = sLogLine;
                return rec;
            }
        } // LogFormat_Message

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        } // Clear

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Read_Registry


        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After

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
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);

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
                EventLog.WriteEntry("Security Manager ZyxelZywalLUSG_1000_V1_1_0_Web_Recorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // Initialize_Logger

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);
            }
            catch
            {
                return iReturn;
            }
        } // ObjectToInt32

        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }
        } // ObjectToInt64


    }
}
