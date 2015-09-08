using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;

namespace JuniperSyslogSRX220V_1_0_0Recorder
{
    public class JuniperSyslogSRX220V_1_0_0Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514, zone = 0;
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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
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
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on JuniperSyslogSRX220V_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on JuniperSyslogSRX220V_1_0_0Recorder functions may not be running");
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

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogSRX220V_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\JuniperSyslogSRX220V_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogSRX220V_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\JuniperSyslogSRX220V_1_0_0Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogSRX220V_1_0_0Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("JuniperSyslogSRX220V_1_0_0Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
            L.Log(LogType.FILE, LogLevel.DEBUG, "line: " + args.Message);

            CustomBase.Rec rec = new CustomBase.Rec();

            try
            {
                try
                {
                    rec.LogName = "JuniperSyslogSRX220V_1_0_0Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    string line = args.Message;
                    string[] lineArr = SpaceSplit(line, false);
                    string[] stringSeparators = new string[] { "->" };

                    if (Before(lineArr[7], "[", 0) == "utmd")
                    {
                        try
                        {
                            if (lineArr.Length > 2)
                            {
                                rec.EventCategory = lineArr[2].Split('.')[1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 8)
                            {
                                rec.EventType = lineArr[8];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 6)
                            {
                                rec.ComputerName = lineArr[6];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr1 = GetColumnValue(line, "ACTION=\"", "\"");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr2 = GetColumnValue(line, "CATEGORY=\"", "\"");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                if (lineArr[12].Contains("->"))
                                {
                                    rec.CustomStr3 = Before(lineArr[12].Split(stringSeparators, StringSplitOptions.None)[0], "(", 0);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                if (lineArr[12].Contains("->"))
                                {
                                    rec.CustomStr4 = Before(lineArr[12].Split(stringSeparators, StringSplitOptions.None)[1], "(", 0);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr5 = GetColumnValue(line, "REASON=\"", "\"");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr6 = GetColumnValue(line, "URL=", " ");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr7 = GetColumnValue(line, "OBJ=", " ");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 Error: " + exception.Message);
                        }

                        try
                        {
                            rec.CustomStr8 = GetColumnValue(line, "PROFILE=\"", "\"");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                if (lineArr[12].Contains("->"))
                                {
                                    rec.CustomInt3 = Convert.ToInt32(Between(
                                        lineArr[12].Split(stringSeparators, StringSplitOptions.None)[0], "(", ")"));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 Type Casting Error: " + exception.Message);
                            rec.CustomInt3 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                if (lineArr[12].Contains("->"))
                                {

                                    rec.CustomInt4 = Convert.ToInt32(Between(
                                        lineArr[12].Split(stringSeparators, StringSplitOptions.None)[1], "(", ")"));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 Type Casting Error: " + exception.Message);
                            rec.CustomInt4 = 0;
                        }
                    }
                    else if (lineArr[7].Substring(0, lineArr[7].Length - 1) == "RT_FLOW")
                    {
                        try
                        {
                            if (lineArr.Length > 2)
                            {
                                rec.EventCategory = lineArr[2].Split('.')[1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 7)
                            {
                                rec.EventType = lineArr[7];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 6)
                            {
                                rec.ComputerName = lineArr[6];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 10)
                            {
                                rec.CustomStr1 = lineArr[9] + " " + lineArr[10];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1 Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 14)
                            {
                                if (lineArr.Length > 25)
                                {
                                    rec.CustomStr2 = lineArr[14];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                                }
                                else
                                {
                                    rec.CustomStr2 = lineArr[12];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 16)
                            {
                                if (lineArr.Length > 25)
                                {
                                    rec.CustomStr7 = lineArr[16];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                                else
                                {

                                    rec.CustomStr7 = lineArr[14];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                                rec.CustomStr8 = lineArr[lineArr.Length - 1];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 or CustomStr8Error: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 21)
                            {
                                if (lineArr.Length > 25)
                                {
                                    rec.CustomStr9 = lineArr[19] + " " + lineArr[20] + " " + lineArr[21];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                                }
                                else
                                {
                                    rec.CustomStr9 = lineArr[17] + " " + lineArr[18] + " " + lineArr[19];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9: " + exception.Message);
                        }

                        string str3 = "";
                        string str4 = "";
                        string str5 = "";
                        string str6 = "";

                        int int3 = 0;
                        int int4 = 0;
                        int int5 = 0;
                        int int6 = 0;

                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].ToString().Contains("->"))
                            {
                                if (string.IsNullOrEmpty(str3))
                                {
                                    str3 = Before(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[0], "/");
                                    int3 = Convert.ToInt32(After(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[0], "/"));
                                }

                                if (string.IsNullOrEmpty(str4))
                                {
                                    str4 = Before(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[1], "/");
                                    int4 = Convert.ToInt32(After(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[1], "/"));
                                }

                                str5 = Before(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[0], "/");
                                int5 = Convert.ToInt32(After(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[0], "/"));
                                str6 = Before(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[1], "/");
                                int6 = Convert.ToInt32(After(lineArr[i].Split(stringSeparators, StringSplitOptions.None)[1], "/"));
                            }
                        }

                        rec.CustomStr3 = str3;
                        rec.CustomStr4 = str4;
                        rec.CustomStr5 = str5;
                        rec.CustomStr6 = str6;

                        rec.CustomInt3 = int3;
                        rec.CustomInt4 = int4;
                        rec.CustomInt5 = int5;
                        rec.CustomInt6 = int6;
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                if (args.Message.Length > 899)
                    rec.Description = args.Message.Substring(0, 899);
                else
                    rec.Description = args.Message;

                rec.Description = args.Message.Replace("'", "|");

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
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

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

        public string GetColumnValue(string line, string startValue, string endValue)
        {
            string returnValue = "";
            string tmp = After(line, startValue);
            string tmpCont = Before(tmp, endValue, 0);
            if (string.IsNullOrEmpty(tmpCont))
                returnValue = tmp;
            else
                returnValue = tmpCont;
            return returnValue;
        }// GetColumnValue


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
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a, int type)
        {
            //type = 1 last
            //type = 0 first


            int posA = 0;

            if (type == 1)
            {
                posA = value.LastIndexOf(a, System.StringComparison.Ordinal);
            }

            if (type == 0)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
            }
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before


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


    }
}
