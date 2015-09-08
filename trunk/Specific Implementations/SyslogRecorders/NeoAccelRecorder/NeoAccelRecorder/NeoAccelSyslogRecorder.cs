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

namespace NeoAccelSyslogRecorder
{
    public class NeoAccelSyslogRecorder : CustomBase
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

        #region Properties of Logs
        private string Level;
        private string Date_of_Request;
        private string Time_of_Request;
        private string Protocol;
        private string User_ID;
        private string First_Name;
        private string Last_Name;
        private string Source_IP_Address;
        private string Destination_IP_Address;
        private string Type;
        private string Session_Id;
        private string Response_Status;
        private string Session_Start_Date;
        private string Session_Start_Time;
        private string Date_of_Response;
        private string Time_of_Response;
        private int Destination_Port;
        private int Source_Port;
        private long Total_TCP_Bytes_Recvd;
        private long Total_TCP_Bytes_Sent;
        private string URL_Accessed;
        private string Method_Name;
        private string User_Agent;
        private string Name_of_Policy_Denied;
        private string Zone_Name;
        private string Secondary_user_name;
        private string Primary_authentication_server;
        private string Secondary_authentication_server;
        private string Groups_extracted;
        private string Failure_Description;
        private string Virtual_IP;
        private string Reserved1;
        private string Reserved2;
        #endregion

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
                    L.Log(LogType.FILE, LogLevel.INFORM, " NeoAccelSyslogRecorder In Init() -->> Start Listening Syslogs On Ip : " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " NeoAccelSyslogRecorder In Init() -->> Start Listening Syslogs On Ip : " + remote_host + " Port: " + Syslog_Port.ToString());
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, " NeoAccelSyslogRecorder In Init() -->> Finish initializing Syslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry(" NeoAccelSyslogRecorder In Init() -->> Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NeoAccelSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("NeoAccelSyslogRecorder In Get_logDir() -->> Exception is : ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public NeoAccelSyslogRecorder()
        {
            Level = null;
            Date_of_Request = null;
            Time_of_Request = null;
            Protocol = null;
            User_ID = null;
            First_Name = null;
            Last_Name = null;
            Source_IP_Address = null;
            Destination_IP_Address = null;
            Type = null;
            Session_Id = null;
            Response_Status = null;
            Session_Start_Date = null;
            Session_Start_Time = null;
            Date_of_Response = null;
            Time_of_Response = null;
            Destination_Port = 0;
            Source_Port = 0;
            Total_TCP_Bytes_Recvd = 0;
            Total_TCP_Bytes_Sent = 0;
            URL_Accessed = null;
            Method_Name = null;
            User_Agent = null;
            Name_of_Policy_Denied = null;
            Zone_Name = null;
            Secondary_user_name = null;
            Primary_authentication_server = null;
            Secondary_authentication_server = null;
            Groups_extracted = null;
            Failure_Description = null;
            Virtual_IP = null;
            Reserved1 = null;
            Reserved2 = null;
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            Rec rec = new Rec();
            bool control = true;

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In slog_SyslogEvent -->> Start Preparing Record");
                control = parsingProcess(args);
                L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In slog_SyslogEvent -->> Finish Preparing Record");

                if (control)
                {
                    rec = createRec();
                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In slog_SyslogEvent 111-->>" + rec.LogName);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In slog_SyslogEvent 222-->>" + rec.CustomStr1);
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In slog_SyslogEvent -->> Finish Sending Data");
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "  NeoAccelSyslogRecorder In slog_SyslogEvent -->> Exception is " + er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "  NeoAccelSyslogRecorder In slog_SyslogEvent -->> Args Message is " + args.EventLogEntType + " " + args.Message);
            }
            finally
            {
                clearproperties();
            }
        }

        public bool parsingProcess(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "  NeoAccelSyslogRecorder In parsingProcess -->> Enter The Function");

            string tempsyslogmessage = "";
            string syslogmessage = "";

            tempsyslogmessage = args.Message.Replace('\0', ' ');
            syslogmessage = tempsyslogmessage.Replace('\t', ',');
            L.Log(LogType.FILE, LogLevel.DEBUG, syslogmessage);
            int indexofLOG = syslogmessage.IndexOf("LOG");

            if (indexofLOG != -1)
            {
                string permanentline = syslogmessage.Substring(indexofLOG);

                string[] fields = permanentline.Split(',');

                L.Log(LogType.FILE, LogLevel.DEBUG, " Line Is : " + syslogmessage);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Length Is : " + fields.Length.ToString());

                if (fields.Length == 33)
                {
                    try
                    {
                        if (fields[0].Contains("_"))
                        {
                            Level = fields[0].Split('_')[1];
                        }
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "An Error Occured While Assign Level");
                        L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                    }

                    Date_of_Request = fields[1];
                    Time_of_Request = fields[2];
                    Protocol = fields[3];
                    User_ID = fields[4];
                    First_Name = fields[5];
                    Last_Name = fields[6];
                    Source_IP_Address = fields[7];
                    Destination_IP_Address = fields[8];
                    Type = fields[9];
                    Session_Id = fields[10];
                    Response_Status = fields[11];
                    Session_Start_Date = fields[12];
                    Session_Start_Time = fields[13];
                    Date_of_Response = fields[14];
                    Time_of_Response = fields[15];

                    if (!String.IsNullOrEmpty(fields[16]) && fields[16] != "-")
                    {
                        Destination_Port = Convert.ToInt32(fields[16]);
                    }

                    if (!String.IsNullOrEmpty(fields[17]) && fields[17] != "-")
                    {
                        Source_Port = Convert.ToInt32(fields[17]);
                    }

                    if (!String.IsNullOrEmpty(fields[18]) && fields[18] != "-")
                    {
                        Total_TCP_Bytes_Recvd = Convert.ToInt64(fields[18]);
                    }

                    if (!String.IsNullOrEmpty(fields[19]) && fields[19] != "-")
                    {
                        Total_TCP_Bytes_Sent = Convert.ToInt64(fields[19]);
                    }

                    URL_Accessed = fields[20];

                    Method_Name = fields[21];
                    User_Agent = fields[22];
                    Name_of_Policy_Denied = fields[23];
                    Zone_Name = fields[24];
                    Secondary_user_name = fields[25];
                    Primary_authentication_server = fields[26];
                    Secondary_authentication_server = fields[27];
                    Groups_extracted = fields[28];
                    Failure_Description = fields[29];
                    Virtual_IP = fields[30];
                    return true;

                }
                else
                {
                    if (fields[3].ToLower() == "admin")
                    {
                        try
                        {
                            if (fields[0].Contains("_"))
                            {
                                Level = fields[0].Split('_')[1];
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "An Error Occured While Assign Level");
                            L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                        }

                        Session_Start_Date = fields[1];
                        Session_Start_Time = fields[2];
                        User_ID = fields[3];
                        Source_IP_Address = fields[4];
                        Protocol = fields[5];
                        Type = fields[6];
                        string des = "";
                    
                        if (fields.Length > 7)
                        {
                            for (int i = 7; i < fields.Length; i++)
                            {
                                des += fields[i] + " , ";
                            }
                        }
                        des = des.Trim(' ').Trim(',');
                        URL_Accessed = des;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }   
            }       
            else { return false; }
        }

        private void clearproperties()
        {
            Level = null;
            Date_of_Request = null;
            Time_of_Request = null;
            Protocol = null;
            User_ID = null;
            First_Name = null;
            Last_Name = null;
            Source_IP_Address = null;
            Destination_IP_Address = null;
            Type = null;
            Session_Id = null;
            Response_Status = null;
            Session_Start_Date = null;
            Session_Start_Time = null;
            Date_of_Response = null;
            Time_of_Response = null;
            Destination_Port = 0;
            Source_Port = 0;
            Total_TCP_Bytes_Recvd = 0;
            Total_TCP_Bytes_Sent = 0;
            URL_Accessed = null;
            Method_Name = null;
            User_Agent = null;
            Name_of_Policy_Denied = null;
            Zone_Name = null;
            Secondary_user_name = null;
            Primary_authentication_server = null;
            Secondary_authentication_server = null;
            Groups_extracted = null;
            Failure_Description = null;
            Virtual_IP = null;
            Reserved1 = null;
            Reserved2 = null;
        }

        public Rec createRec()
        {
            Rec rec = new Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "In createRec ---->> Date Time Is " + Session_Start_Date + " " + Session_Start_Time);

                if (!String.IsNullOrEmpty(Session_Start_Date) && !String.IsNullOrEmpty(Session_Start_Time))
                {
                    string[] sessiondate = Session_Start_Date.Split('-');
                    Session_Start_Date = sessiondate[1] + "." + sessiondate[0] + "." + sessiondate[2];
                    L.Log(LogType.FILE, LogLevel.ERROR, "In createRec ---->> Date Time Is " + Session_Start_Date + " " + Session_Start_Time);
                    rec.Datetime = Convert.ToDateTime(Session_Start_Date + " " + Session_Start_Time).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    rec.Datetime = DateTime.Now.ToString();
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "In createRec ---->> An Occured While Assign Date Time");
                L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
            }

            rec.LogName = "Neo Accel Recorder";
            rec.EventCategory = Protocol;
            rec.EventType = Type;
            rec.UserName = User_ID;

            rec.CustomInt1 = Destination_Port;
            rec.CustomInt2 = Source_Port;
            rec.CustomInt6 = Total_TCP_Bytes_Recvd;
            rec.CustomInt7 = Total_TCP_Bytes_Sent;

            rec.CustomStr1 = Source_IP_Address;
            rec.CustomStr2 = Destination_IP_Address;
            rec.CustomStr3 = Response_Status;
            rec.CustomStr4 = Session_Id;
            rec.CustomStr5 = Groups_extracted;
            rec.CustomStr6 = Primary_authentication_server;
            rec.CustomStr7 = Virtual_IP;
            rec.CustomStr8 = Name_of_Policy_Denied;
            rec.CustomStr9 = Level;
            rec.CustomStr10 = First_Name + " " + Last_Name;

            rec.Description = URL_Accessed;

            return rec;
        }

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
