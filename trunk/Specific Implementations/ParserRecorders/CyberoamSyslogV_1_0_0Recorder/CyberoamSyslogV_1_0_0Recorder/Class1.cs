//CyberoamSyslogV_1_0_0Recorder
//Writer Onur SARIKAYA

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace CyberoamSyslogV_1_0_0Recorder
{
    public class CyberoamSyslogV_1_0_0Recorder : CustomBase
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
        private string dateFormat = "yyyy/MM/dd HH:mm:ss";

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public CyberoamSyslogV_1_0_0Recorder()
        {

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

        private string SubLineSplitter(string subLine)
        {
            string returnStr = "";
            if (!string.IsNullOrEmpty(subLine))
            {
                if (subLine.Contains("="))
                {
                    returnStr = subLine.Split('=')[1].Replace('"', ' ').Trim();
                }
            }
            else
            {
                return "";
            }
            return returnStr;

        } // SubLineSplitter

        /// <summary>
        /// Gelen str value int'e çevrilir.
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        private int Convert_To_Int32(string strValue)
        {
            int intValue = 0;
            try
            {
                intValue = Convert.ToInt32(strValue);
                return intValue;
            }
            catch (Exception ex)
            {
                return 0;
            }
        } // Convert_To_Int32

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            string[] lineArr = SpaceSplit(args.Message, true);
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "CyberoamSyslogV_1_0_0Recorder";
                    //rec.EventType = args.EventLogEntType.ToString();

                    #region Description
                    if (args.Message.Length > 899)
                        rec.Description = args.Message.Substring(0, 899);
                    else
                    {
                        rec.Description = args.Message;
                    }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Description: " + args.Message);
                    #endregion
                    string dateString = "";
                    string timeString = "";
                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        #region DateTime

                        
                        if (lineArr[i].StartsWith("date"))
                        {
                            dateString = SubLineSplitter(lineArr[i]);
                        }

                        if (lineArr[i].StartsWith("time") && !lineArr[i].StartsWith("timezone"))
                        {
                            timeString = SubLineSplitter(lineArr[i]);
                        }
                        #endregion
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "dateString: " + dateString + " " + timeString);
                    DateTime dt = Convert.ToDateTime(dateString + " " + timeString);
                    rec.Datetime = dt.ToString(dateFormat);

                    L.Log(LogType.FILE, LogLevel.DEBUG, "DateTime: " + rec.Datetime);
                    
                    for (int i = 0; i < lineArr.Length; i++)
                    {

                        #region SOURCENAME
                        if (lineArr[i].StartsWith("device_name"))
                        {
                            rec.SourceName = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region EVENTCATEGORY
                        if (lineArr[i].StartsWith("log_component"))
                        {
                            rec.EventCategory = SubLineSplitter(lineArr[i]);
                        }
                        #endregion
                        
                        #region EVENTTYPE
                        if (lineArr[i].StartsWith("log_type"))
                        {
                            rec.EventType = SubLineSplitter(lineArr[i]);
                        }
                        #endregion
                        
                        #region USERSID
                        if (lineArr[i].StartsWith("device_id"))
                        {
                            rec.UserName = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region COMPUTERNAME
                        if (lineArr[i].StartsWith("user_name"))
                        {
                            rec.ComputerName = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR1
                        if (lineArr[i].StartsWith("protocol"))
                        {
                            rec.CustomStr1 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR2
                        if (lineArr[i].StartsWith("user_gp"))
                        {
                            rec.CustomStr2 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR3
                        if (lineArr[i].StartsWith("src_ip"))
                        {
                            rec.CustomStr3 = SubLineSplitter(lineArr[i]);
                        }
                        
                        #endregion

                        #region CUSTOMSTR4
                        if (lineArr[i].StartsWith("dst_ip"))
                        {
                            rec.CustomStr4 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR5
                        if (lineArr[i].StartsWith("category"))
                        {
                            rec.CustomStr5 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR6
                        if (lineArr[i].StartsWith("contenttype"))
                        {
                            rec.CustomStr6 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR7
                        if (lineArr[i].StartsWith("domain"))
                        {
                            rec.CustomStr7 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR8
                        if (lineArr[i].StartsWith("status"))
                        {
                            rec.CustomStr8 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR9
                        if (lineArr[i].StartsWith("url"))
                        {
                            rec.CustomStr9 = SubLineSplitter(lineArr[i]);
                        }
                        #endregion

                        #region CUSTOMSTR10
                        /* if (lineArr[i].StartsWith("log_component"))
                        {
                            rec.EventCategory = SubLineSplitter(lineArr[i]);
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);*/
                        #endregion

                        #region CUSTOMINT1
                        if (lineArr[i].StartsWith("fw_rule_id"))
                        {
                            rec.CustomInt1 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT2
                        if (lineArr[i].StartsWith("iap"))
                        {
                            rec.CustomInt2 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT3

                        #endregion

                        #region CUSTOMINT4

                        #endregion

                        #region CUSTOMINT5
                        if (lineArr[i].StartsWith("src_port"))
                        {
                            rec.CustomInt5 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT6
                        if (lineArr[i].StartsWith("dst_port"))
                        {
                            rec.CustomInt6 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT7
                        if (lineArr[i].StartsWith("httpresponsecode"))
                        {
                            rec.CustomInt7 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT8
                        if (lineArr[i].StartsWith("sent_bytes"))
                        {
                            rec.CustomInt8 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT9
                        if (lineArr[i].StartsWith("recv_bytes"))
                        {
                            rec.CustomInt9 = Convert_To_Int32(SubLineSplitter(lineArr[i]));
                        }
                        #endregion

                        #region CUSTOMINT10

                        #endregion

                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9.ToString(CultureInfo.InvariantCulture));

                    //rec.SourceName = args.Source;
                    if (SendData(rec))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        } // slog_SyslogEvent

        private bool SendData(Rec rec)
        {
            try
            {
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
                return true;
            }
            catch (Exception exception)
            {

                return false;
            }
        } // SendData

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Trace Level"));
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
    }
}
