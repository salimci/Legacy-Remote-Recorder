//WSenseSyslogV_1_0_1Recorder

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

namespace WSenseSyslogV_1_0_1Recorder
{
    public class WSenseSyslogV_1_0_1Recorder : CustomBase
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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
        }

        //public override void Clear()
        //{
        //    if (slog != null)
        //        slog.Stop();
        //}

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WSenseSyslogV_1_0_1Recorder Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WSenseSyslogV_1_0_1Recorder  functions may not be running");
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
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_1Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\WSenseSyslogV_1_0_1Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_1Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public WSenseSyslogV_1_0_1Recorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "WSenseSyslogV_1_0_1Recorder";
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();
                    
                    string line = args.Message;

                    L.Log(LogType.FILE, LogLevel.DEBUG, "args.Message: " + args.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "line: " + line);
                    string[] lineArr = line.Split('|');

                    try
                    {
                        if (args.Message.Length > 899)
                        {
                            rec.Description = line.Substring(0, 899);
                        }
                        else
                        {
                            rec.Description = args.Message;
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);
                    }
                    catch (Exception exception)
                    {

                        L.Log(LogType.FILE, LogLevel.ERROR, "Description Error: " + exception.Message);
                    }

                    try
                    {
                        string[] dateArr = lineArr[1].Split(' ');
                        DateTime dt;
                        string dateNow = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
                        string myDateTimeString = dateArr[0] + dateArr[1] + "," + dateNow + "  ," + dateArr[2];
                        dt = Convert.ToDateTime(myDateTimeString);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.INFORM, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Date error: " + exception.Message);
                    }

                    try
                    {
                        rec.ComputerName = lineArr[0].Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.INFORM, "ComputerName: " + rec.ComputerName);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName ERROR: " + exception.Message);
                    }

                    try
                    {

                        rec.SourceName = lineArr[10].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "SourceName: " + rec.SourceName);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "SourceName ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.EventCategory = lineArr[21].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "EventCategory: " + rec.EventCategory);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.EventType = lineArr[7].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "EventType: " + rec.EventType);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventType ERROR: " + exception.Message);
                    }

                    try
                    {
                        string user = After(lineArr[16], "suser=");
                        string[] userArray = user.Split('/');
                        rec.UserName = userArray[userArray.Length - 1];
                        rec.CustomStr5 = After(lineArr[16], "suser=");
                        //rec.UserName = After(lineArr[16], "suser=");
                        L.Log(LogType.FILE, LogLevel.INFORM, "UserName: " + rec.UserName);
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomStr5: " + rec.CustomStr5);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "UserName ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr3 = lineArr[14].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomStr3: " + rec.CustomStr3);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr4 = lineArr[11].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomStr4: " + rec.CustomStr4);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomStr6 = lineArr[28].Split('=')[1];
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomStr6: " + rec.CustomStr6);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 ERROR: " + exception.Message);
                    }

                    try
                    {
                        string[] str8Arr = lineArr[28].Split('/');
                        rec.CustomStr8 = str8Arr[2];
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomStr8: " + rec.CustomStr8);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomInt1 = Convert.ToInt32(lineArr[26].Split('=')[1]);
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomInt1: " + rec.CustomInt1);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomInt3 = Convert.ToInt32(lineArr[15].Split('=')[1]);
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomInt3: " + rec.CustomInt3);

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 ERROR: " + exception.Message);
                    }

                    try
                    {
                        rec.CustomInt4 = Convert.ToInt32(lineArr[13].Split('=')[1]);
                        L.Log(LogType.FILE, LogLevel.INFORM, "CustomInt4: " + rec.CustomInt4);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 ERROR: " + exception.Message);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
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
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

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


        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\WSenseSyslogV_1_0_1Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("WSenseSyslogV_1_0_1Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("WSenseSyslogV_1_0_1Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_1Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
