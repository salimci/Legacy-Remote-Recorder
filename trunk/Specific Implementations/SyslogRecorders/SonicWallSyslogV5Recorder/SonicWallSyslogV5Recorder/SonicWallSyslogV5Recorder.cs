//Name: Sonic Wall Syslog Recorder, Version: 5
//Writer: Ali Yıldırım
//Date: 17.01.2011

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
using System.Text.RegularExpressions;
using System.Globalization;

namespace SonicWallSyslogV5Recorder
{
    public class SonicWallSyslogV5Recorder : CustomBase
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SonicWallSyslogRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SonicWallSyslogRecorder functions may not be running");
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


                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening SonicWallSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing SonicWallSyslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicWallSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SonicWallSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicWallSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SonicWallSyslogV5Recorder()
        {

        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        { 
            //172.19.19.19:514 : local0.alert id=firewall sn=0017C542F1CC time="2011-01-17 10:52:43" fw=88.247.6.27 pri=1 c=16 m=793 msg="Application Firewall Alert" af_polid=2 af_policy="Download_policy" af_type="HTTP Client Request" af_service="HTTP" af_action="Reset/Drop"  n=0 src=172.19.19.20:1586:X0 dst=207.46.70.174:80:X1 

            CustomBase.Rec rec = new CustomBase.Rec();

            rec.LogName = "SonicWallSyslogV5 Recorder";
            rec.EventType = args.EventLogEntType.ToString();
            rec.Description = args.Message;
            L.Log(LogType.FILE, LogLevel.INFORM, " slog_SyslogEvent() | Line : " + args.Message);

            try
            {
                string param = "";
                string value = "";
                string[] parts = args.Message.Split(' ');

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains("="))
                    {
                        param = parts[i].Split('=')[0];
                        value = parts[i].Split('=')[1];
                        i = Complete_Value(parts, ref value, i);
                        int d = Complete_Value(parts, ref value, i);
                        //Now value is ready to use.

                        //All or none parameters can be selected by if clauses.
                        if (param == "")
                        {
                            rec.EventCategory = value;
                        }
                        else if (param == "id")
                        {

                        }
                        else if (param == "sn")
                        {

                        }
                        else if (param == "time")
                        {

                        }
                        else if (param == "fw")
                        {

                        }
                        else if (param == "pri")
                        {

                        }
                        else if (param == "c")
                        {

                        }
                        else if (param == "m")
                        {

                        }
                        else if (param == "msg")
                        {

                        }
                        else if (param == "af_polid")
                        {

                        }
                        else if (param == "af_policy")
                        {

                        }
                        else if (param == "af_type")
                        {

                        }
                        else if (param == "")
                        {

                        }
                        else if (param == "")
                        {

                        }
                        else if (param == "")
                        {

                        }
                        else if (param == "")
                        {

                        }
                        else if (param == "")
                        {

                        }
                        else if (param == "")
                        {

                        }
                    }
                }

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
                L.Log(LogType.FILE, LogLevel.DEBUG, " slog_SyslogEvent() | Rec data is send. ");
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "slog_SyslogEvent() | " + ex.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "slog_SyslogEvent() | " + args.EventLogEntType + " " + args.Message);
            }
        }
        
        /// <summary>
        /// Parametrenin sahip olduğu eşitliğin karşısındaki değeri alır, temizler ve tam olarak getirir.
        /// </summary>
        /// <param name="parts">Boşluğa göre parçalanmış satırın dizisi. </param>
        /// <param name="value">Eşitliğin karşısındaki değer. </param>
        /// <param name="i">Dizideki kalınan yeri gösteren sayaç. </param>
        /// <returns>Döngünün devam edeceği yer(sayaç). </returns>
        public int Complete_Value(string[] parts, ref string value, int i)
        {
            try
            {
                int j = i + 1;
                while (j < parts.Length)
                {
                    if (parts[j].Contains("="))
                    {
                        return j - 1;
                    }
                    else
                    {
                        value += " " + parts[j];
                        j++;
                    }
                }

                value = value.TrimStart('"').TrimEnd('"').Trim();
            }
            catch (Exception ex)
            {
                return i;
            }
            return i;
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SonicWallSyslogV5Recorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallSyslogV5Recorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallSyslogV5Recorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicWallSyslogV5Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SonicWallSyslogV5Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
