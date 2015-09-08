using System;
using System.Collections.Generic;
using System.Text;
using LogMgr;
using Log;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace ArubaSysLogRecorder
{
    public class ArubaSysLogRecorder : CustomBase
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
        private string sLogName = "ArubaSysLogRecorder";

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on " + sLogName + " Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on " + sLogName + " Recorder functions may not be running");
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing " + sLogName + " Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager " + sLogName + " Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\" + sLogName + "" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager " + sLogName + " Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public ArubaSysLogRecorder()
        {

        }

        public String[] SpaceSplit(String line)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ')
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

        public String[] SpaceSplit(String line, bool useTabs)
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

        void Sgs_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {   
                rec.LogName = sLogName;
                string line = args.Message;

                L.Log(LogType.FILE, LogLevel.DEBUG, sLogName + " Sgs_SyslogEvent() | Start preparing record");

                L.Log(LogType.FILE, LogLevel.DEBUG, sLogName + " Sgs_SyslogEvent() | start line=" + line);

                if (string.IsNullOrEmpty(line) == true)
                {
                    L.LogTimed(LogType.FILE, LogLevel.DEBUG, "Line Is Null or Empty");
                    return;
                }

                string[] tempfields = line.Split(' ');
                string logtype = tempfields[2].Split('.')[1];

                bool controllogtype = false;
                #region Error Logs
                if (logtype.ToLower() == "error")
                {
                    controllogtype = true;
                    string[] fields = tempfields;
                    rec.EventCategory = tempfields[2];

                    int count = 0;
                    for (int i = 0; i < tempfields.Length; i++)
                    {
                        if (tempfields[i] == "" || tempfields[i] == ":")
                            count++;
                    }

                    string[] fields0 = new string[tempfields.Length - count];

                    int index = 0;
                    for (int j = 0; j < tempfields.Length; j++)
                    {
                        if (tempfields[j] != "" && tempfields[j] != ":")
                        {
                            fields0[index] = tempfields[j];
                            index++;
                        }
                    }

                    string tempdatetime = fields0[3] + "-" + fields0[2] + "-" + fields0[5] + " " + fields0[4];
                    string datetime = Convert.ToDateTime(tempdatetime).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    rec.Datetime = datetime;

                    string sourcename = fields0[6].TrimStart('[').TrimEnd(']');
                    rec.SourceName = sourcename;

                    string customstr6 = fields0[7].Split('[')[0];
                    rec.CustomStr6 = customstr6;

                    int customint1 = Convert.ToInt32(fields0[7].Split('[')[1].Trim(':').Trim(']'));
                    rec.CustomInt1 = customint1;

                    int customint2 = Convert.ToInt32(fields0[8].Trim('<').Trim('>'));
                    rec.CustomInt2 = customint2;

                    string[] templine = line.Split('|');
                    string tempdescription = templine[2];
                    string eventtype = "";
                    string user = "";
                    if (tempdescription.Contains("Failed Authentication"))
                    {
                        eventtype = "Failed Authentication";
                        rec.EventType = eventtype;

                        user = tempdescription.Substring(0, tempdescription.IndexOf("Failed Authentication")).Trim();
                        rec.UserName = user;
                    }
                    else
                    {
                        rec.Description = tempdescription;
                    }
                }
                #endregion
                #region Notice Logs
                else if (logtype.ToLower() == "notice")
                {
                    rec.EventCategory = tempfields[2];
                    controllogtype = true;
                    if (line.Contains("|"))
                    {
                        string[] fields = tempfields;


                        int count = 0;
                        for (int i = 0; i < tempfields.Length; i++)
                        {
                            if (tempfields[i] == "" || tempfields[i] == ":")
                                count++;
                        }

                        string[] fields0 = new string[tempfields.Length - count];

                        int index = 0;
                        for (int j = 0; j < tempfields.Length; j++)
                        {
                            if (tempfields[j] != "" && tempfields[j] != ":")
                            {
                                fields0[index] = tempfields[j];
                                index++;
                            }
                        }

                        string tempdatetime = fields0[3] + "-" + fields0[2] + "-" + fields0[5] + " " + fields0[4];
                        string datetime = Convert.ToDateTime(tempdatetime).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        rec.Datetime = datetime;


                        string sourcename = fields0[6].TrimStart('[').TrimEnd(']');
                        rec.SourceName = sourcename;

                        string customstr6 = fields0[7].Split('[')[0];
                        rec.CustomStr6 = customstr6;

                        int customint1 = Convert.ToInt32(fields0[7].Split('[')[1].Trim(':').Trim(']'));
                        rec.CustomInt1 = customint1;

                        int customint2 = Convert.ToInt32(fields0[8].Trim('<').Trim('>'));
                        rec.CustomInt2 = customint2;


                        for (int k = 0; k < fields0.Length; k++)
                        {
                            if (fields0[k].Contains("MAC"))
                            {
                                rec.ComputerName = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("Name") || fields0[k].Contains("name"))
                            {
                                rec.UserName = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("IP"))
                            {
                                rec.CustomStr1 = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("method"))
                            {
                                rec.CustomStr3 = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("server"))
                            {
                                rec.CustomStr4 = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("role"))
                            {
                                rec.CustomStr5 = fields0[k].Split('=')[1];
                            }
                            else if (fields0[k].Contains("cause"))
                            {
                                string propertyandvalue = fields0[k];

                                for (int h = k + 1; h < fields0.Length; h++)
                                {
                                    if (!fields0[h].Contains("="))
                                    {
                                        propertyandvalue = propertyandvalue + " " + fields0[h];

                                        if (h + 1 == fields0.Length)
                                        {
                                            rec.CustomStr8 = propertyandvalue.Split('=')[1];
                                            k = h;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        rec.CustomStr8 = propertyandvalue.Split('=')[1];
                                        k = h;
                                        break;
                                    }
                                }
                            }
                            else if (fields0[k].Contains("authenticated"))
                            {
                                rec.EventType = "User " + fields0[k].Trim(':');
                            }
                        }
                    }
                    else
                    {
                        int count = 0;
                        for (int i = 0; i < tempfields.Length; i++)
                        {
                            if (tempfields[i] == "" || tempfields[i] == ":")
                                count++;
                        }

                        string[] fields0 = new string[tempfields.Length - count];

                        int index = 0;
                        for (int j = 0; j < tempfields.Length; j++)
                        {
                            if (tempfields[j] != "" && tempfields[j] != ":")
                            {
                                fields0[index] = tempfields[j];
                                index++;
                            }
                        }

                        string tempdatetime = fields0[3] + "-" + fields0[2] + "-" + fields0[5] + " " + fields0[4];
                        string datetime = Convert.ToDateTime(tempdatetime).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        rec.Datetime = datetime;


                        string sourcename = fields0[6].TrimStart('[').TrimEnd(']');
                        rec.SourceName = sourcename;

                        string customstr6 = fields0[7].Split('[')[0];
                        rec.CustomStr6 = customstr6;

                        if (fields0[7].Contains("["))
                        {
                            int customint1 = Convert.ToInt32(fields0[7].Split('[')[1].Trim(':').Trim(']'));
                            rec.CustomInt1 = customint1;
                        }

                        string description = "";
                        for (int f = 8; f < fields0.Length; f++)
                        {
                            description = description + " " + fields0[f];
                        }

                        rec.Description = description;

                        if (description.Contains("add username"))
                        {
                            string[] specific = description.Trim().Split(' ');
                            rec.EventType = "ADD USER";

                            for (int f = 0; f < specific.Length; f++)
                            {
                                if (specific[f].Contains("USER"))
                                {
                                    rec.UserName = specific[f].Split('@')[0].Trim().Split(':')[1];
                                    rec.CustomStr1 = specific[f].Split('@')[1].Trim();
                                }

                                if (specific[f].Contains("username"))
                                {
                                    rec.CustomStr2 = specific[f + 1].Trim('"');
                                }

                                if (specific[f].Contains("start-time"))
                                {
                                    string[] tempdate = specific[f + 1].Trim('"').Split('/');
                                    string date = tempdate[2] + "/" + tempdate[0] + "/" + tempdate[1];
                                    string time = specific[f + 2].Trim('"');

                                    rec.CustomStr7 = Convert.ToDateTime(date + " " + time).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                }

                                if (specific[f].Contains("expiry"))
                                {

                                    string[] tempdate = specific[f + 2].Trim('"').Split('/');
                                    string date = tempdate[2] + "/" + tempdate[0] + "/" + tempdate[1];
                                    string time = specific[f + 3].Trim('"');

                                    rec.CustomStr8 = Convert.ToDateTime(date + " " + time).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                }
                            }

                        }
                    }
                }
                #endregion
                #region Info
                else if (logtype.ToLower() == "info" || logtype.ToLower() == "ýnfo")
                {
                    rec.EventCategory = tempfields[2];
                    controllogtype = true;
                    int count = 0;
                    for (int i = 0; i < tempfields.Length; i++)
                    {
                        if (tempfields[i] == "" || tempfields[i] == ":")
                            count++;
                    }

                    string[] fields0 = new string[tempfields.Length - count];

                    int index = 0;
                    for (int j = 0; j < tempfields.Length; j++)
                    {
                        if (tempfields[j] != "" && tempfields[j] != ":")
                        {
                            fields0[index] = tempfields[j];
                            index++;
                        }
                    }

                    string tempdatetime = fields0[3] + "-" + fields0[2] + "-" + fields0[5] + " " + fields0[4];
                    string datetime = Convert.ToDateTime(tempdatetime).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    rec.Datetime = datetime;

                    string sourcename = fields0[6].TrimStart('[').TrimEnd(']');
                    rec.SourceName = sourcename;

                    string description = "";
                    for (int f = 7; f < fields0.Length; f++)
                    {
                        description = description + " " + fields0[f];
                    }
                    rec.Description = description;
                }
                #endregion
                
                #region Send REcord
                if (controllogtype)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, sLogName + " Sgs_SyslogEvent() | Start sending Data");

                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                        L.Log(LogType.FILE, LogLevel.DEBUG, sLogName + " Sgs_SyslogEvent() usingRegistry |  Finish Sending Data");
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                        L.Log(LogType.FILE, LogLevel.DEBUG, sLogName + " Sgs_SyslogEvent() |  Finish Sending Data");
                    }
                } 
                #endregion
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, sLogName + " Sgs_SyslogEvent() | " + er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR, sLogName + " Sgs_SyslogEvent() | " + args.Message);
            }
        }

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\ArubaSysLogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ArubaSysLogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ArubaSysLogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager " + sLogName + " Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager " + sLogName, er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

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

        }
        
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
        }
    }   
}       

