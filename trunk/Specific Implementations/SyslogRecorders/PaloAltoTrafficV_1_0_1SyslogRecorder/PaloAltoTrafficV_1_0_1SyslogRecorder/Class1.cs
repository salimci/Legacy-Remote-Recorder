//PaloAltoTrafficV_1_0_1SyslogRecorder
//Writer : Onur Sarıkaya

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

namespace PaloAltoTrafficV_1_0_1SyslogRecorder
{
    public class PaloAltoTrafficV_1_0_1SyslogRecorder : CustomBase
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\PaloAltoTrafficV_1_0_1SyslogRecorder" + Id + ".log";
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

        public PaloAltoTrafficV_1_0_1SyslogRecorder()
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
            CustomBase.Rec rec = ParseSpecific(args.Message, false, args);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
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
                    s.SetReg(Id, rec.Datetime, rec.Description, "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
        }

        public Rec ParseSpecific(String line, bool dontSend, LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);
            if (string.IsNullOrEmpty(line))
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Line is Null Or Empty. ");

            }
            CustomBase.Rec rec = new CustomBase.Rec();

            rec.LogName = "PaloAltoTrafficV_1_0_1SyslogRecorder";
            //rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            rec.EventType = args.EventLogEntType.ToString();

            if (!string.IsNullOrEmpty(remote_host))
                rec.ComputerName = remote_host;

            rec.Description = args.Message;

            if (rec.Description.Length > 899)
            {
                rec.Description = rec.Description.Substring(0, 899);
            }
            else
            {
                rec.Description = rec.Description;
            }

            //rec.Description = args.Message.Replace("'", "|");

            L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
            rec.SourceName = args.Source;
            L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

            if (!dontSend)
            {
                string[] parts = line.Split(',');
                try
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() Parts[" + i + "]: " + parts[i]);
                    }
                    string type01 = parts[3];
                    if (type01 == "TRAFFIC")
                    {
                        #region TRAFFIC
                        try
                        {
                            rec.Datetime = Convert.ToDateTime(parts[6]).ToString("yyyy-MM-dd HH:mm:ss");//Date time conversion requeired.
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | There is a problem converting to date.  date : " + parts[4]);
                        }

                        rec.CustomStr1 = StringParsingOperation(parts[18], 18, "CustomStr1", parts.Length);
                        rec.CustomStr2 = StringParsingOperation(parts[19], 19, "CustomStr2", parts.Length);
                        rec.CustomStr3 = StringParsingOperation(parts[7], 7, "CustomStr3", parts.Length);
                        rec.CustomStr4 = StringParsingOperation(parts[8], 8, "CustomStr4", parts.Length);
                        rec.CustomStr5 = StringParsingOperation(parts[9], 9, "CustomStr5", parts.Length);
                        rec.CustomStr6 = StringParsingOperation(parts[10], 10, "CustomStr6", parts.Length);
                        rec.CustomStr7 = StringParsingOperation(parts[29], 29, "CustomStr7", parts.Length);
                        rec.CustomStr8 = StringParsingOperation(parts[4], 4, "CustomStr8", parts.Length);
                        rec.CustomStr9 = StringParsingOperation(parts[3], 3, "CustomStr9", parts.Length);
                        rec.CustomStr10 = StringParsingOperation(parts[14], 14, "CustomStr10", parts.Length);

                        rec.UserName = StringParsingOperation(parts[12], 12, "UserName", parts.Length);
                        rec.EventType = StringParsingOperation(parts[30], 30, "EventType", parts.Length);
                        rec.EventCategory = StringParsingOperation(parts[37], 37, "EventCategory", parts.Length);

                        rec.CustomInt1 = IntegerParsingOperation(parts[0], 0, "rec.CustomInt1", parts.Length);
                        rec.CustomInt2 = IntegerParsingOperation(parts[23], 23, "rec.CustomInt2", parts.Length);
                        rec.CustomInt3 = IntegerParsingOperation(parts[24], 24, "rec.CustomInt3", parts.Length);
                        rec.CustomInt4 = IntegerParsingOperation(parts[25], 25, "rec.CustomInt4", parts.Length);
                        rec.CustomInt5 = IntegerParsingOperation(parts[26], 26, "rec.CustomInt5", parts.Length);
                        rec.CustomInt6 = IntegerParsingOperation(parts[27], 27, "rec.CustomInt6", parts.Length);
                        rec.CustomInt7 = IntegerParsingOperation(parts[22], 22, "rec.CustomInt7", parts.Length);
                        rec.CustomInt8 = IntegerParsingOperation(parts[32], 32, "rec.CustomInt8", parts.Length);
                        rec.CustomInt9 = IntegerParsingOperation(parts[33], 33, "rec.CustomInt9", parts.Length);
                        rec.CustomInt10 = IntegerParsingOperation(parts[36], 36, "rec.CustomInt10", parts.Length);
                        #endregion TRAFFIC
                    }

                    else if (type01 == "THREAT")
                    {
                        #region THREAT
                        try
                        {
                            rec.Datetime = Convert.ToDateTime(parts[1]).ToString("yyyy-MM-dd HH:mm:ss");//Date time conversion requeired.
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -- Datetime : " + rec.Datetime);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | There is a problem converting to date.  date : " + parts[4]);
                        }

                        string eventType = parts[30];
                        if (eventType.ToLower() == "alert")
                        {
                            rec.EventCategory = StringParsingOperation(parts[4], 4, "EventCategory", parts.Length);
                            rec.EventType = StringParsingOperation(parts[30], 30, "EventType", parts.Length);
                            rec.ComputerName = StringParsingOperation(parts[0].Split(':')[0], 0, "ComputerName", parts.Length);

                            rec.CustomStr1 = StringParsingOperation(parts[31].Split('/')[0], 31, "CustomStr1", parts.Length);
                            rec.CustomStr2 = StringParsingOperation(parts[31].Split('/')[1], 31, "CustomStr2", parts.Length);
                            rec.CustomStr3 = StringParsingOperation(parts[7], 7, "CustomStr3", parts.Length);
                            rec.CustomStr4 = StringParsingOperation(parts[8], 8, "CustomStr4", parts.Length);
                            rec.CustomStr5 = StringParsingOperation(parts[9], 9, "CustomStr5", parts.Length);
                            rec.CustomStr6 = StringParsingOperation(parts[10], 10, "CustomStr6", parts.Length);
                            rec.CustomStr7 = StringParsingOperation(parts[29], 29, "CustomStr7", parts.Length);
                            rec.CustomStr8 = StringParsingOperation(parts[4], 4, "CustomStr8", parts.Length);
                            rec.CustomStr9 = StringParsingOperation(parts[3], 3, "CustomStr9", parts.Length);
                            rec.CustomStr10 = StringParsingOperation(parts[14], 14, "CustomStr10", parts.Length);

                            rec.CustomInt1 = IntegerParsingOperation(parts[40], 40, "rec.CustomInt1", parts.Length);
                            rec.CustomInt2 = IntegerParsingOperation(parts[5], 5, "rec.CustomInt2", parts.Length);
                            rec.CustomInt3 = IntegerParsingOperation(parts[24], 24, "rec.CustomInt3", parts.Length);
                            rec.CustomInt4 = IntegerParsingOperation(parts[25], 25, "rec.CustomInt4", parts.Length);
                            rec.CustomInt5 = IntegerParsingOperation(parts[22], 22, "rec.CustomInt5", parts.Length);
                            rec.CustomInt6 = IntegerParsingOperation(parts[27], 27, "rec.CustomInt6", parts.Length);
                            rec.CustomInt7 = IntegerParsingOperation(parts[26], 26, "rec.CustomInt7", parts.Length);
                            rec.CustomInt10 = IntegerParsingOperation(parts[36], 36, "rec.CustomInt10", parts.Length);
                        }
                        else if (eventType.ToLower() == "block-url")
                        {
                            rec.ComputerName = StringParsingOperation(parts[0].Split(':')[0] + ":" + parts[0].Split(':')[1], 0, "ComputerName", parts.Length);
                            rec.EventCategory = StringParsingOperation(parts[37], 37, "EventCategory", parts.Length);
                            rec.EventType = StringParsingOperation(parts[30], 30, "EventType", parts.Length);
                            rec.CustomStr1 = StringParsingOperation(parts[31].Split('/')[0], 31, "CustomStr1", parts.Length);
                            rec.CustomStr2 = StringParsingOperation(parts[31], 31, "CustomStr2", parts.Length);
                            rec.CustomStr3 = StringParsingOperation(parts[7], 7, "CustomStr3", parts.Length);
                            rec.CustomStr4 = StringParsingOperation(parts[8], 8, "CustomStr4", parts.Length);
                            rec.CustomStr5 = StringParsingOperation(parts[9], 9, "CustomStr5", parts.Length);
                            rec.CustomStr6 = StringParsingOperation(parts[10], 10, "CustomStr6", parts.Length);
                            rec.CustomStr7 = StringParsingOperation(parts[29], 29, "CustomStr7", parts.Length);
                            rec.CustomStr8 = StringParsingOperation(parts[4], 4, "CustomStr8", parts.Length);
                            rec.CustomStr9 = StringParsingOperation(parts[3], 3, "CustomStr9", parts.Length);
                            rec.CustomStr10 = StringParsingOperation(parts[14], 14, "CustomStr10", parts.Length);

                            rec.CustomInt1 = IntegerParsingOperation(parts[40], 40, "CustomInt1", parts.Length);
                            rec.CustomInt2 = IntegerParsingOperation(parts[5], 5, "CustomInt2", parts.Length);
                            rec.CustomInt3 = IntegerParsingOperation(parts[24], 24, "CustomInt3", parts.Length);
                            rec.CustomInt4 = IntegerParsingOperation(parts[25], 25, "CustomInt4", parts.Length);
                            rec.CustomInt5 = IntegerParsingOperation(parts[26], 26, "CustomInt5", parts.Length);
                            rec.CustomInt6 = IntegerParsingOperation(parts[27], 27, "CustomInt6", parts.Length);
                            rec.CustomInt10 = IntegerParsingOperation(parts[36], 36, "CustomInt10", parts.Length);
                        }
                        else if (eventType.ToLower() == "drop-all-packets")
                        {
                            rec.ComputerName = StringParsingOperation(parts[0].Split(':')[0] + ":" + parts[0].Split(':')[1], 0, "ComputerName", parts.Length);
                            rec.EventCategory = StringParsingOperation(parts[33], 33, "EventCategory", parts.Length);
                            rec.EventType = StringParsingOperation(parts[37], 37, "EventType", parts.Length);
                            rec.CustomStr1 = StringParsingOperation(parts[18], 18, "CustomStr1", parts.Length);
                            rec.CustomStr2 = StringParsingOperation(parts[19], 19, "CustomStr2", parts.Length);
                            rec.CustomStr3 = StringParsingOperation(parts[7], 7, "CustomStr3", parts.Length);
                            rec.CustomStr4 = StringParsingOperation(parts[8], 8, "CustomStr4", parts.Length);
                            rec.CustomStr5 = StringParsingOperation(parts[9], 9, "CustomStr5", parts.Length);
                            rec.CustomStr6 = StringParsingOperation(parts[10], 10, "CustomStr6", parts.Length);
                            rec.CustomStr7 = StringParsingOperation(parts[29], 29, "CustomStr7", parts.Length);
                            rec.CustomStr8 = StringParsingOperation(parts[33], 33, "CustomStr8", parts.Length);
                            rec.CustomStr9 = StringParsingOperation(parts[3], 3, "CustomStr9", parts.Length);
                            rec.CustomStr10 = StringParsingOperation(parts[14], 14, "CustomStr10", parts.Length);

                            rec.CustomInt1 = IntegerParsingOperation(parts[40], 40, "CustomInt1", parts.Length);
                            rec.CustomInt2 = IntegerParsingOperation(parts[5], 5, "CustomInt2", parts.Length);
                            rec.CustomInt3 = IntegerParsingOperation(parts[22], 22, "CustomInt3", parts.Length);
                            rec.CustomInt4 = IntegerParsingOperation(parts[23], 23, "CustomInt4", parts.Length);
                            rec.CustomInt5 = IntegerParsingOperation(parts[24], 24, "CustomInt5", parts.Length);
                            rec.CustomInt6 = IntegerParsingOperation(parts[25], 25, "CustomInt6", parts.Length);
                            rec.CustomInt7 = IntegerParsingOperation(parts[26], 26, "CustomInt7", parts.Length);
                            rec.CustomInt8 = IntegerParsingOperation(parts[27], 27, "CustomInt8", parts.Length);
                            rec.CustomInt10 = IntegerParsingOperation(parts[36], 36, "CustomInt10", parts.Length);
                        }
                        #endregion THREAT
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.StackTrace);
                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                }
            }
            return rec;
        }

        public int IntegerParsingOperation(string value, int index, string column, int lengthValue)
        {
            int return_value = 0;
            if (lengthValue > index)
            {
                try
                {
                    return_value = Convert.ToInt32(value);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -- '" + column + "' : " + value);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -- '" + column + "' Error : " + exception.Message);
                }
            }
            return return_value;
        } // IntegerParsingOperation

        public string StringParsingOperation(string value, int index, string column, int lengthValue)
        {
            string return_value = "";
            if (lengthValue > index)
            {
                try
                {
                    return_value = value;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -- '" + column + "' : " + value);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -- '" + column + "' Error : " + exception.Message);
                }
            }
            return return_value.Replace('"', ' ').Trim();
        } // StringParsingOperation

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
