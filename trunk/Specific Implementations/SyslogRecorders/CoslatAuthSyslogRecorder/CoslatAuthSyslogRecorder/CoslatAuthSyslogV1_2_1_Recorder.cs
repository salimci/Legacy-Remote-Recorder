/*
 Haziran 2012 içerisinde syslogile gönderilen loglar alınıp parse edilmiştir.
 * İstek Hüseyin Sevin tarafından AOÇ için gelmiş olup Onur Sarıkaya tarafından hazırlanmıştır.
 * Kurumda Wireless ile bağlantı kumak isteyen misafirler'e gönderilen sms kayıtlarını almak için hazırlanmıştır.
 * Telefon numaraları ve eventtype'ları önemlidir.
 * Farklı gelen log satırı sadece Register_sms eventType'da olanlar'dır.
 * Log formatı standart olup Register_Sms 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Parser;

namespace Parser
{
    public class CoslatAuthSyslogV1_2_1_Recorder : CustomBase
    {

        public struct Fields
        {
            public string messagePi;
            public string LocalPi;
            public string line1;
            public string line2;
            public string line3;
            public string line4;
            public string fullLine;
            public bool RecordSequence;
        }

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

        private Fields RecordFields;

        private void InitializeComponent()
        {
            RecordFields = new Fields();
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CoslatAuthSyslogRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CoslatAuthSyslogRecorder functions may not be running");
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_CoslatAuthSyslogRecorder);

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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CoslatAuthSyslogV1_2_1_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CoslatAuthSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public void slog_CoslatAuthSyslogRecorder(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            string day;
            string month;
            string time;
            int year = DateTime.Now.Year;

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur  : " + args.Message);

                try
                {
                    rec.LogName = "CoslatAuthSyslogRecorder";
                    rec.EventType = args.EventLogEntType.ToString();
                    string line = args.Message;

                    string[] arr = line.Split(' ');
                    ArrayList arr1 = new ArrayList();

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(arr[i]))
                        {
                            arr1.Add(arr[i]);
                        }
                    }

                    day = (string)arr1[4];
                    month = (string)arr1[3];
                    time = (string)arr1[5];
                    DateTime dt = Convert.ToDateTime(day + "/" + month + "/" + year + " " + time);
                    rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> Datetime : " + rec.Datetime);

                    rec.EventType = (string)arr1[7].ToString().Replace(':', ' ').Trim();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> EventType : " + rec.EventType);

                    rec.SourceName = (string)arr1[2];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> SourceName : " + rec.SourceName);

                    if (arr1[7].ToString() == "REGISTER_SMS")
                    {
                        rec.UserName = arr1[8].ToString().Replace(':', ' ').Replace(',', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> UserName : " + rec.UserName);
                        rec.CustomStr1 = arr1[11].ToString().Replace(',', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr1 : " + rec.CustomStr1);
                        rec.CustomStr2 = arr1[12].ToString().Replace(',',' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr2 : " + rec.CustomStr2);
                        rec.ComputerName = arr1[0].ToString().Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> ComputerName : " + rec.ComputerName);

                        rec.CustomStr3 = arr1[0].ToString().Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr3 : " + rec.CustomStr3);

                        rec.CustomStr4 = arr1[0].ToString().Split(':')[1];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr4 : " + rec.CustomStr4);

                    }
                    else
                    {
                        rec.UserName = arr1[8].ToString().Replace(':', ' ').Replace(',', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> UserName : " + rec.UserName);
                        rec.CustomStr1 = arr1[10].ToString().Replace(',', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr1 : " + rec.CustomStr1);
                        rec.CustomStr2 = arr1[9].ToString().Replace(',', ' ').Trim();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr2 : " + rec.CustomStr2);
                        rec.ComputerName = arr1[0].ToString().Split(':')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> ComputerName : " + rec.ComputerName);
                    }
                    


                    //rec.CustomStr4 = arr1[0].ToString().Split(':')[1];
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> CustomStr4 : " + rec.CustomStr4);
                    //rec.CustomStr5 = (string)arr1[6].ToString().Replace('''');
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "Line Onur --> Description : " + rec.Description);
                    if (line.Length > 3999)
                    {
                        rec.Description = line.Substring(0, 3999);
                    }
                    else
                    {
                        rec.Description = line;
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "usingRegistry : " + usingRegistry);

                //L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                try
                {
                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                        s.SetData(rec);
                        ClearRecordFields();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.");

                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                        s.SetData(Dal, virtualhost, rec);
                        ClearRecordFields();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending Data and REcordFields cleared.");

                        s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error on sending data. " + e.Message);
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
        /// Bu fonksiyon bir string katarında belli olan iki karakter arasındaki stringi alabilmek için 
        /// yazılmıştır.
        /// </summary>
        /// <param name="value"></param> string değer
        /// <param name="a"></param> ilk string index
        /// <param name="b"></param> son string index
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

        public void ClearRecordFields()
        {
            RecordFields.RecordSequence = false;
            RecordFields.line1 = "";
            RecordFields.line2 = "";
            RecordFields.line3 = "";
            RecordFields.line4 = "";
            RecordFields.fullLine = "";
            RecordFields.messagePi = "";

            L.Log(LogType.FILE, LogLevel.DEBUG, "RecordFields cleared.");

        } // ClearRecordFields 

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CoslatAuthSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CoslatAuthSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CoslatAuthSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CoslatAuthSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CoslatAuthSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
