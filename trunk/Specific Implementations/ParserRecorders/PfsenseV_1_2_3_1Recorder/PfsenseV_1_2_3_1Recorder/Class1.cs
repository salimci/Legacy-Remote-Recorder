using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CustomTools;
using Log;
using LogMgr;
using Microsoft.Win32;

namespace PfsenseV_1_2_3_1Recorder
{
    public class PfsenseV_1_2_3_1Recorder : CustomBase
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

        public PfsenseV_1_2_3_1Recorder()
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
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record.0");

            CustomBase.Rec rec = new CustomBase.Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record.1");
                try
                {
                    string strRealLogLine = "";
                    //----------
                    try
                    {
                        string sLogLine = args.Message;
                        string[] strTabLine = sLogLine.Split('\t');

                        string strSourceName = strTabLine[2];
                        rec.SourceName = args.Source;

                        string strSourceNameIP = strTabLine[3];
                        rec.ComputerName = remote_host;

                        strRealLogLine = strTabLine[4];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR: " + e.Message);
                    }


                    try
                    {
                        string strDateTime = strRealLogLine.Substring(0, strRealLogLine.IndexOf("pf:", 0));
                        DateTime dt;
                        string myDateTimeString = DateTime.Now.Year + " " + strDateTime; //strDateTime; //day + month + "," + year + "," + time;
                        dt = Convert.ToDateTime(myDateTimeString);
                        string lastDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        rec.Datetime = lastDate;
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR: " + e.Message);
                    }


                    try
                    {
                        string strBreakLine = strRealLogLine.Substring(strRealLogLine.IndexOf("pf:", 0), strRealLogLine.Length - strRealLogLine.IndexOf("pf:", 0));
                        //listBox1.Items.Add(strBreakLine.Split(' ')[1]);

                        string strBreakLineOne = strRealLogLine.Substring(strRealLogLine.IndexOf(strBreakLine.Split(' ')[3], 0), strRealLogLine.Length - strRealLogLine.IndexOf(strBreakLine.Split(' ')[3], 0));
                        string[] strBreakLineTwo = strBreakLineOne.Split(':');
                        //listBox1.Items.Add(strBreakLineTwo[0].Trim());
                        //listBox1.Items.Add(strBreakLineTwo[1].Trim());
                        //listBox1.Items.Add(strBreakLineTwo[2].Trim());
                        //listBox1.Items.Add(strBreakLineTwo[3].Trim());

                        string[] strBreakLineThree = strBreakLineTwo[2].Trim().Split(',');

                        rec.CustomStr1 = strBreakLineThree[0].Trim('(').Trim(' ');
                        rec.CustomStr5 = strBreakLineThree[1].Trim(' ');
                        rec.CustomStr6 = strBreakLineThree[2].Trim(' ');
                        rec.CustomStr7 = strBreakLineThree[3].Trim(' ');
                        rec.CustomStr8 = strBreakLineThree[4].Trim(' ');
                        rec.CustomStr9 = strBreakLineThree[5].Trim(' ');
                        rec.CustomStr10 = strBreakLineThree[6].Trim(' ').Split(')')[0];

                        string strSourceDestination = strBreakLineThree[6].Split(')')[0];

                        string strBreakLineFour = strRealLogLine.Substring(strRealLogLine.IndexOf(strBreakLineThree[6], 0) + strSourceDestination.Length + 1, strRealLogLine.Length - strRealLogLine.IndexOf(strBreakLineThree[6], 0) - strSourceDestination.Length - 1);


                        string[] strSourceDestinationBreakLine = strBreakLineFour.Split(':');

                        string strSource = strSourceDestinationBreakLine[0].Trim(' ').Split('>')[0].Trim(' ');
                        rec.CustomStr3 = strSource;

                        string strDestination = strSourceDestinationBreakLine[0].Trim(' ').Split('>')[1].Trim(' ');
                        rec.CustomStr4 = strDestination;

                        string strEndParamLine = "";
                        if (strSourceDestinationBreakLine.Count() > 2)
                        {
                            strEndParamLine = strSourceDestinationBreakLine[1] + ":" + strSourceDestinationBreakLine[2];
                            rec.CustomStr2 = strEndParamLine;
                        }
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR: " + e.Message);
                    }
                    //----------

                    rec.LogName = "Pfsense SysLog Recorder";
                    //rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");                    
                    rec.EventType = args.EventLogEntType.ToString();

                    if (args.Message.Length > 3999)
                        rec.Description = args.Message.Substring(0, 3999);
                    else
                        rec.Description = args.Message;

                    //rec.Description = args.Message.Replace("'","|");
                    //L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
                    //rec.SourceName = args.Source;
                    //L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);
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

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\PfsenseV_1_2_3_1Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("PfsenseV_1_2_3_1Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("PfsenseV_1_2_3_1Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager PfsenseV_1_2_3_1Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager PfsenseV_1_2_3_1Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}

