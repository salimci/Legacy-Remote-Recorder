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

namespace Watchguard_V_xtm505_1_Recorder
{
    public class Watchguard_V_xtm505_1_Recorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Watchguard_V_xtm505_1_Recorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Watchguard_V_xtm505_1_Recorder Recorder functions may not be running");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Watchguard_V_xtm505_1_Recorder Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Watchguard_V_xtm505_1_Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        } // Init 

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\Watchguard_V_xtm505_1_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Watchguard_V_xtm505_1_Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Get_logDir

        public Watchguard_V_xtm505_1_Recorder()
        {

        }

        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder() | Start preparing record");
                string sLogLine = args.Message;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder() | start line=" + sLogLine);

                if (string.IsNullOrEmpty(sLogLine) == true)
                {
                    L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Line İs Null or Empty");
                    return;
                }

                rec.LogName = "Watchguard_V_xtm505_1_Recorder";
                rec = LogFormat_Message(sLogLine, rec);


                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder()  EventLogEntType: " + args.EventLogEntType.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder()  Source: " + args.Source);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder()  Message: " + args.Message);

                rec.SourceName = args.Source;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder() | Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Watchguard_V_xtm505_1_Recorder() |  Finish Sending Data");
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Watchguard_V_xtm505_1_Recorder() | " + er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "Watchguard_V_xtm505_1_Recorder() | " + args.Message);
            }
        } // Sgs_SyslogEvent

        private Rec LogFormat_Message(string sLogLine, Rec rec)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
                L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() | line : " + sLogLine);
                try
                {

                    if (sLogLine.Length > 3999)
                    {
                        rec.Description = sLogLine.Substring(0, 3999);
                    }
                    else
                    {
                        rec.Description = sLogLine;
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() Description Error : " + exception.Message);
                }

                String[] SpaceArray = SpaceSplit(sLogLine, true);

                try
                {
                    rec.EventCategory = Between(sLogLine, "op=", "dstname").Replace('"', ' ').Trim();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() EventCategory : " + rec.EventCategory);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() EventCategory Error : " + exception.Message);
                }

                try//ok
                {
                    string year = DateTime.Now.Year.ToString();
                    string month = SpaceArray[3];
                    string day = SpaceArray[4];
                    string time = SpaceArray[5];
                    string date = (year + "/" + month + "/" + day + " " + time);
                    DateTime dt;
                    dt = Convert.ToDateTime(date);
                    rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() Datetime : " + rec.Datetime);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() Datetime Error : " + exception.Message);
                }

                try//ok
                {
                    rec.EventType = SpaceArray[10];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() EventType : " + rec.EventType);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() EventType Error : " + exception.Message);
                }

                try//ok
                {
                    rec.ComputerName = SpaceArray[6];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() ComputerName: " + rec.ComputerName);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() ComputerName Error : " + exception.Message);
                }

                if (rec.EventType == "Deny")
                {
                    try
                    {
                        rec.CustomStr3 = SpaceArray[17];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr3 : " + SpaceArray[17]);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr3 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr4 = SpaceArray[18];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr4 : " + SpaceArray[18]);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr4 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        if (SpaceArray[11].Contains('-'))
                        {
                            rec.CustomStr6 = SpaceArray[11].Split('-')[1];
                        }
                        else
                        {
                            rec.CustomStr6 = SpaceArray[11];
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr6 : " + SpaceArray[11]);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr6 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr7 = SpaceArray[14];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr7 : " + SpaceArray[14]);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr7 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(SpaceArray[15]))
                        {
                            rec.CustomInt1 = Convert.ToInt32(SpaceArray[15]);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt1 Error For Deny keyword. : " + exception.Message);
                    }


                    try
                    {
                        if (!string.IsNullOrEmpty(SpaceArray[16]))
                        {
                            rec.CustomInt2 = Convert.ToInt32(SpaceArray[16]);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt2 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(SpaceArray[19]))
                        {
                            rec.CustomInt6 = Convert.ToInt32(SpaceArray[19]);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt6 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(SpaceArray[20]))
                        {
                            rec.CustomInt7 = Convert.ToInt32(SpaceArray[20]);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt7 Error For Deny keyword. : " + exception.Message);
                    }

                    try
                    {
                        char c = '"';
                        rec.CustomStr5 = Between(sLogLine, "cats=", c.ToString().Replace('"', ' ').Trim());
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr5 : " + rec.CustomStr5);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr5 Error For Deny keyword. : " + exception.Message);
                    }
                }

                else
                {

                    try//ok
                    {
                        rec.CustomStr1 = Between(sLogLine, "msg=", "proxy_act").Replace('"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr1 : " + rec.CustomStr1);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr1 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        rec.CustomStr2 = Between(sLogLine, "proxy_act=", "op").Replace('"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr2 : " + rec.CustomStr2);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr2 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        rec.CustomStr3 = SpaceArray[14];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr3: " + rec.CustomStr3);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr3 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        rec.CustomStr4 = SpaceArray[15];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr4: " + rec.CustomStr4);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr4 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        if (SpaceArray[11].Contains("-"))
                        {
                            rec.CustomStr5 = (SpaceArray[11].Split('-')[1].Replace('"', ' ').Trim());
                        }
                        else
                        {
                            rec.CustomStr5 = SpaceArray[11];
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr5: " + rec.CustomStr5);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr5 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        if (SpaceArray[12].Contains("-"))
                        {
                            rec.CustomStr6 = SpaceArray[12].Split('-')[1].Replace('"', ' ').Trim();
                        }
                        else
                        {
                            rec.CustomStr6 = "";
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() log format is not recognized. CustomStr6 setted null : ");
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr6: " + rec.CustomStr6);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr6 Error : " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr7 = SpaceArray[13];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr7: " + rec.CustomStr7);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr7 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        rec.CustomStr8 = Between(sLogLine, "dstname=", "arg").Replace('"', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr8 : " + rec.CustomStr8);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr8 Error : " + exception.Message);
                    }

                    try//ok
                    {
                        string Str9 = Between(sLogLine, "arg=", "sent_bytes").Replace('"', ' ').Trim();
                        if (Str9.Length > 899)
                        {
                            rec.CustomStr9 = Str9.Substring(0, 899);
                            rec.CustomStr10 = Str9.Substring(899, Str9.Length - 899);
                        }
                        else
                        {
                            rec.CustomStr9 = Str9;
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomStr9 : " + rec.CustomStr9);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomStr9 Error : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(SpaceArray[16]))
                        {
                            int Int1 = Convert.ToInt32(SpaceArray[16]);
                            rec.CustomInt1 = Int1;
                        }
                        else
                        {
                            rec.CustomInt1 = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt1 is not recognized. CustomInt1 = 0: ");
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt1 Error : " + exception.Message);
                    }

                    try
                    {

                        if (!string.IsNullOrEmpty(SpaceArray[17]))
                        {
                            int Int2 = Convert.ToInt32(SpaceArray[17]);
                            rec.CustomInt2 = Int2;
                        }
                        else
                        {
                            rec.CustomInt2 = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt2 is not recognized. CustomInt2 = 0: ");
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt2: " + rec.CustomInt2.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt1 Error : " + exception.Message);
                    }


                    try
                    {

                        if (!string.IsNullOrEmpty(Between(sLogLine, "sent_bytes=", "rcvd_bytes").Replace('"', ' ').Trim()))
                        {
                            rec.CustomInt6 = Convert.ToInt32(Between(sLogLine, "sent_bytes=", "rcvd_bytes").Replace('"', ' ').Trim());
                        }
                        else
                        {
                            rec.CustomInt6 = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt6 is not recognized. CustomInt6 = 0: ");
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt6 : " + rec.CustomInt6.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt6 Error : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(Between(sLogLine, "rcvd_bytes=", "elapsed_time").Replace('"', ' ').Trim()))
                        {
                            rec.CustomInt7 = Convert.ToInt32(Between(sLogLine, "rcvd_bytes=", "elapsed_time").Replace('"', ' ').Trim());
                        }
                        else
                        {
                            rec.CustomInt7 = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt7 is not recognized. CustomInt7 = 0: ");
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt7 : " + rec.CustomInt7.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt7 Error : " + exception.Message);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(Between(sLogLine, "elapsed_time=", "sec(s)").Replace('"', ' ').Trim()))
                        {
                            rec.CustomInt3 = Convert.ToInt32(Between(sLogLine, "elapsed_time=", "sec(s)").Replace('"', ' ').Trim());
                        }
                        else
                        {
                            rec.CustomInt3 = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt3 is not recognized. CustomInt3 = 0: ");
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "LogFormat_Message() CustomInt3 : " + rec.CustomInt3.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogFormat_Message() CustomInt3 Error : " + exception.Message);
                    }
                }

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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\Watchguard_V_xtm505_1_Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Watchguard_V_xtm505_1_Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Watchguard_V_xtm505_1_Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Watchguard_V_xtm505_1_Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Watchguard_V_xtm505_1_Recorder Recorder", er.ToString(), EventLogEntryType.Error);
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

