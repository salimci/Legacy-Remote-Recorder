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

namespace SonicWallSyslogRecorder
{
    public class SonicWallSyslogRecorder : CustomBase
    {
        #region Deðiþkenler
                    private string sourceName; 
                    private int sourceportNumber;
                    private string id;
                    private string sn; 
                    private string time;
                    private string fw;
                    private int pri; 
                    private int c;
                    private int m; 
                    private string msg;
                    private int n; 
                    private string dst;
                    private string proto;
                    private string src;
                    private int sent; 
                    private int rcvd; 
                    private string vpnpolicy;
                    private string op; 
                    private int result; 
                    private string dstname;
                    private int code; 
                    private string Category;
                    private string log_Name;
                    private string event_Type;
                    private string arg;
                    private string info;
                    private string description;
                    private string unexpectedDescription;
        #endregion
        
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
        
        public SonicWallSyslogRecorder()
        {
                    id = null;
                    sn = null; 
                    time = null;
                    fw = null;
                    pri = 0; 
                    c = 0;
                    m = 0; 
                    msg = null;
                    n = 0; 
                    dst = null;
                    proto = null;
                    src = null;
                    sent = 0; 
                    rcvd = 0; 
                    vpnpolicy = null;
                    op = null; 
                    result = 0; 
                    dstname = null;
                    code = 0; 
                    Category = null;
                    log_Name = null;
                    event_Type = null;
                    sourceName = null; 
                    sourceportNumber = 0;
                    arg = null;
                    info = null;
                    description = null;
                    unexpectedDescription = null;
        }
        
        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try 
                {      
                    string[] logproperties = {"id","sn","time","fw","pri","c","m","msg","n","dst","proto","src",
                                                "sent","rcvd","vpnpolicy","op","result","dstname","code","Category","arg"};

                    this.log_Name = "SonicWallSyslog Recorder";
                    this.event_Type = args.EventLogEntType.ToString();
                    L.Log(LogType.FILE, LogLevel.INFORM, "args.Message" + args.Message);                    
                    string[] tempfields = args.Message.Split(' ');
                    info = tempfields[2];
                    description = args.Message;
                    
                    for (int k = 3; k < tempfields.Length; k++)
                    {
                        if (!tempfields[k].Contains("="))
                        {
                            for (int j = k; j < tempfields.Length; j++)
                            {
                                if (tempfields[j].Contains("="))
                                {
                                    k = j;
                                    break;
                                }
                                tempfields[k - 1] += " " + tempfields[j];
                                tempfields[j] = "";
                            }
                            tempfields[k - 1] = tempfields[k - 1].Trim();
                        }
                    }


                    int controlindex = 0;

                    for (int h = 0; h < tempfields.Length; h++)
                    {
                        if (tempfields[h] == "") 
                        {
                            controlindex++;
                        }
                    }

                    string[] fields = new string[tempfields.Length - controlindex];
                    int xyz = 0;

                    for (int i = 0; i < tempfields.Length; i++)
                    {
                        if (tempfields[i] != "")
                        {
                            fields[xyz] = tempfields[i];
                            xyz++;
                        }
                    }

                    for (int i = 3; i < fields.Length; i++)
                    {   
                        int index = -1;
                        string property = "";
                        property = fields[i].Split('=')[0];
                        index = Array.IndexOf(logproperties, property);

                        if (index != -1)
                        {
                            assignpropertyvalue(index, fields[i].Split('=')[1]);
                        }
                        else
                        {
                            assignundefinedvalue(fields[i].Split('=')[0], fields[i].Split('=')[1]);
                        }
                    }

                    string controltype = "";
                    for (int i = 0; i < fields.Length; i++)
			        {
        			          if(fields[i].Contains("dstname"))
                              {
                                controltype ="web";
                              }
                              if(fields[i].Contains("msg"))
                              {
                                controltype ="fw";
                              }
			        }

                    if (controltype == "web")
                    {
                        rec = createRec("web");
                    }
                    else
                    {
                        rec = createRec("fw");
                    }
                }
                catch (Exception e)
                {
                    clearProperties();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.DEBUG, Dal + " " + virtualhost + " " + rec.Description);
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "","","",rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                clearProperties();
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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SonicWallSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SonicWallSyslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SonicWallSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
        
        private void assignundefinedvalue(string property, string value)
        {
            if (unexpectedDescription == null) 
            {
                unexpectedDescription = "";
            }

            if (unexpectedDescription == "")
            {
                unexpectedDescription += property + "=" + value;
            }
            else
            {
                unexpectedDescription += "," + property + "=" + value +",";
            }
        }
        
        private void assignpropertyvalue(int i, string virtualsyslogMessageArr) 
        {                
            switch (i)
            {
                case 0:
                    {
                        id = virtualsyslogMessageArr;
                    } break;
                case 1:
                    {
                        sn = virtualsyslogMessageArr;
                    } break;
                case 2:
                    {
                        time = virtualsyslogMessageArr;
                    } break;
                case 3:
                    {
                        fw = virtualsyslogMessageArr;
                    } break;
                case 4:
                    {
                        pri = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 5:
                    {
                        c= Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 6:
                    {
                        m = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 7:
                    {
                        msg = virtualsyslogMessageArr;
                    } break;
                case 8:
                    {
                        n = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 9:
                    {
                        dst = virtualsyslogMessageArr;
                    } break;
                case 10:
                    {
                        proto = virtualsyslogMessageArr;
                    } break;
                case 11:
                    {
                        src = virtualsyslogMessageArr;
                    } break;
                case 12:
                    {
                        sent = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 13:
                    {
                        rcvd = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 14:
                    {
                        vpnpolicy = virtualsyslogMessageArr;
                    } break;
                case 15:
                    {
                        op = virtualsyslogMessageArr;
                    } break;
                case 16:
                    {
                        result = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 17:
                    {
                        dstname = virtualsyslogMessageArr;
                    } break;
                case 18:
                    {
                        code = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
                case 19:
                    {
                        Category = virtualsyslogMessageArr;
                    } break;
                case 20: 
                    {
                        arg = virtualsyslogMessageArr;
                    } break;
            }
        }

        public Rec createRec(string type) 
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            
            if (type == "web")
            {
                if (time != null)
                {
                    //time = time.Trim('"');
                    //rec.Datetime = Convert.ToDateTime(time).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    rec.Datetime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    rec.Datetime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }

                rec.LogName = "Sonicwall.Web";
                rec.EventType = op;
                rec.EventCategory = Category;

                rec.CustomInt1 = sent;
                rec.CustomInt2 = rcvd;
                rec.CustomInt3 = result;
                rec.CustomInt4 = code;

                rec.CustomStr1 = src.Split(':')[0];
                rec.CustomStr2 = dst.Split(':')[0];
                rec.CustomStr3 = proto;
                rec.CustomStr4 = src.Split(':')[1];
                rec.CustomStr5 = dst.Split(':')[1];
                rec.CustomStr6 = dstname;
                rec.CustomStr7 = arg;

                string[] srcname = src.Split(':');
                if (srcname.Length > 2)
                {
                    rec.CustomStr8 = srcname[3];
                }
                else 
                {
                    rec.CustomStr8 = "";
                }
                
                rec.CustomStr10 = info;
                
                rec.Description = description +" "+ createDescription();
            }
            if (type == "fw")
            {
                if (time != null)
                {
                    //time = time.Trim('"');
                    //rec.Datetime = Convert.ToDateTime(time).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    rec.Datetime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    rec.Datetime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }

                rec.LogName = "Sonicwall.FW";


                rec.CustomStr1 = src.Split(':')[0];
                rec.CustomStr2 = dst.Split(':')[0];
                rec.CustomStr3 = proto;
                rec.CustomStr4 = src.Split(':')[1];
                rec.CustomStr5 = dst.Split(':')[1];

                string[] srcname = src.Split(':');
                if (srcname.Length > 2)
                {
                    rec.CustomStr8 = srcname[3];
                }
                else
                {
                    rec.CustomStr8 = "";
                }

                rec.CustomStr9 = msg;
                rec.CustomStr10 = info;
               
                rec.Description = description +" "+ createDescription();

                
            }

            return rec;
        }

        private string createDescription() 
        {
            string desc = "";
            if (unexpectedDescription != null && unexpectedDescription != "") 
            {
                unexpectedDescription = unexpectedDescription.TrimEnd(',');
                desc += "UNEXPECTED PROPERTY NAMES & VALUES = " + unexpectedDescription; 
            }

            unexpectedDescription = "";
            return desc;
        }

        private void clearProperties() 
        {
            id = null;
            sn = null;
            time = null;
            fw = null;
            pri = 0;
            c = 0;
            m = 0;
            msg = null;
            n = 0;
            dst = null;
            proto = null;
            src = null;
            sent = 0;
            rcvd = 0;
            vpnpolicy = null;
            op = null;
            result = 0;
            dstname = null;
            code = 0;
            Category = null;
            log_Name = null;
            event_Type = null;
            sourceName = null;
            sourceportNumber = 0;
            arg = null;
            info = null;
            description = null;
            unexpectedDescription = null;
        }
    }
}
