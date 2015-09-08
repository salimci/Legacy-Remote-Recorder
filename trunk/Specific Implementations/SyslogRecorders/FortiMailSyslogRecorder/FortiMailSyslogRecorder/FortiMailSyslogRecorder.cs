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

namespace FortiMailSyslogRecorder
{
    public class FortiMailSyslogRecorder : CustomBase
    {   
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port=514,zone=0;
        private string err_log, protocol = "UDP", location = "", remote_host="localhost";
        private CLogger L;
        public Syslog slog=null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        public string localinfo = "", local_id = "";
        
        #region Properties of Logs
		    private string date;
            private string time;
            private string device_id;		
            private long log_id;
            private int log_part;		
            private string type;	
            private string subtype;		
            private string pri;	
            private string user;	
            private string ui;
            private string action;
            private string status;
            private string session_id;		
            private string msg;
            private string to;
            private string delay;
            private string xdelay;							
            private string mailer;		
            private string relay; 	
            private string dsn;	
            private string stat;
            private string from;
            private int size;
            private int _class;
            private int nrcpts;
            private string msgid; 
            private string proto;
            private string daemon; 
            private string client_name;	
            private string resolved;	
            private string direction;
            private int message_length;
            private string virus;
            private string disposition;
            private string dst_ip;
            private string endpoint;
            private string classifier;
            private string Spam;
            private string subject;
            private string unexpectedDescription;
	    #endregion

        private void InitializeComponent()
        {
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile,String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
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
                    L.Log(LogType.FILE, LogLevel.INFORM, " FortiMailSyslogRecorder In Init() -->> Start Listening Syslogs On Ip : " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " FortiMailSyslogRecorder In Init() -->> Start Listening Syslogs On Ip : " + remote_host + " Port: " + Syslog_Port.ToString());
                    slog = new Syslog(remote_host, Syslog_Port, pro);
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, " FortiMailSyslogRecorder In Init() -->> Finish initializing Syslog Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry(" FortiMailSyslogRecorder In Init() -->> Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\FortiMailSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("FortiMailSyslogRecorder In Get_logDir() -->> Exception is : ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public FortiMailSyslogRecorder()
		{
		    date = null;
            time = null;
            device_id = null;		
            log_id = 0;
            log_part = 0;		
            type = null;	
            subtype = null;		
            pri = null;	
            user = null;	
            ui = null;
            action = null;
            status = null;
            session_id = null;		
            msg = null;
            to = null;
            delay = null;
            xdelay = null;							
            mailer = null;		
            relay = null; 	
            dsn = null;	
            stat = null;
            from = null;
            size = 0;
            _class = 0;
            nrcpts = 0;
            msgid = null; 
            proto = null;
            daemon = null; 
            client_name = null;	
            resolved = null;	
            direction = null;
            message_length = 0;
            virus = null;
            disposition = null;
            dst_ip = null;
            endpoint = null;
            classifier = null;
            Spam = null;
            subject = null;
            unexpectedDescription = null;
		}

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Recc");
            Rec rec = new Rec();

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Start Preparing Record");
                parsingProcess(args);
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Finish Preparing Record");
                rec = createRec();
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Dal : " + Dal);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> virtualhost : " + virtualhost);
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);

                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Finish Sending Data");
                clearproperties();
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Exception is " + er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "  FortiMailSyslogRecorder In slog_SyslogEvent -->> Args Message is " + args.EventLogEntType + " " + args.Message);
            }
            finally 
            {
                clearproperties();
            }
        }

        public void parsingProcess(LogMgrEventArgs args)
        {       
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> Enter The Function");

                //string[] logproperties = {"date","time","device_id","log_id","log_part","type","subtype","pri","user","ui","action","status","session_id","msg",
                //                          "to","delay","xdelay","mailer","relay","dsn","stat","from","size","class","nrcpts","msgid","proto","daemon",
                //                          "client_name","resolved","direction","message_length","virus","disposition","classifier","Spam","subject"};

                string[] logproperties = {"date","time","device_id","log_id","type","pri","session_id","client_name","dst_ip","endpoint","from",
                                             "to","subject","mailer","resolved","direction","virus","disposition","classifier","message_length"};
                                          

                string tempsyslogmessage = "";
                
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> Enter The Function");
                
                
                tempsyslogmessage = args.Message.Replace('\0', ' ');
                
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> line Is :  "  + tempsyslogmessage);     
                
                string[] tempfields = tempsyslogmessage.Split(' ');
                string mm = tempfields[0];
                string[] mm1 = mm.Split(':');
                localinfo = mm1[0];
                local_id = tempfields[2];
                int count = 0;
                for (int i = 0; i < tempfields.Length; i++)
                {
                    if (tempfields[i] == "")
                        count++;
                }

                string[] fields = new string[tempfields.Length - count];

                int fieldsindex = 0;
                for (int i = 0; i < tempfields.Length; i++)
                {
                    if (tempfields[i].Trim() != "")
                    {
                        fields[fieldsindex] = tempfields[i];
                        fieldsindex++;
                    }
                }

                for (int g = 0; g < fields.Length; g++)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " fields[" + g + "] is " + ":" + fields[g]);
                }

                int index;
            string property = "";
            string value = "";
            string propertyandvalue = "";

            for (int i = 3; i < fields.Length; i++)
            {

                index = -1;
                property = "";
                value = "";
                propertyandvalue = "";

                if (i + 1 < fields.Length)
                {
                    if (fields[i + 1].Contains("="))
                    {
                        string[] pandv = fields[i].Split('=');

                        property = pandv[0].Trim();

                        if (pandv.Length > 2)
                        {
                            for (int j = 1; j < pandv.Length; j++)
                            {
                                value = value + "=" + pandv[j];
                            }

                            value = value.Trim('=');
                            property = value.Split('=')[0].Trim();
                            value = value.Split('=')[1].Trim();
                        }
                        else
                        {
                            value = fields[i].Split('=')[1].Trim();
                        }
                    }
                    else
                    {

                        propertyandvalue = fields[i];

                        int controlff = i;
                        while (!fields[controlff + 1].Contains("="))
                        {
                            propertyandvalue = propertyandvalue + " , " + fields[controlff + 1];
                            controlff++;
                            if (controlff == fields.Length - 1)
                            {
                                break;
                            }
                        }
                        i = controlff;
                        property = propertyandvalue.Split('=')[0].Trim();
                        value = propertyandvalue.Split('=')[1].Trim();
                    }
                }
                else
                {
                    for (int j = i; j < fields.Length; j++)
                    {
                        propertyandvalue = propertyandvalue + "," + fields[i];
                    }
                    propertyandvalue = propertyandvalue.Trim(',');
                    property = propertyandvalue.Split('=')[0].Trim();
                    value = propertyandvalue.Split('=')[1].Trim();
                }

                property = property.Trim('"');
                value = value.Trim('"');

                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> for içindeki sýra sayýsý : ---->>>" + i.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> property & value :  " + property + " " + value);

                index = Array.IndexOf(logproperties, property);
                if (index != -1)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess 1111");
                    assignpropertyvalue(index, value);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess 2222");
                    assignundefinedvalue(property, value);
                }
            }
                
            L.Log(LogType.FILE, LogLevel.DEBUG, "  FortiMailSyslogRecorder In parsingProcess -->> Exit The Function");
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
                unexpectedDescription += "," + property + "=" + value + ",";
            }
        }

        private void clearproperties() 
        { 
                date = null;
                time = null;
                device_id = null;
                log_id = 0;
                log_part = 0;
                type = null;
                subtype = null;
                pri = null;
                user = null;
                ui = null;
                action = null;
                status = null;
                session_id = null;
                msg = null;
                to = null;
                delay = null;
                xdelay = null;
                mailer = null;
                relay = null;
                dsn = null;
                stat = null;
                from = null;
                size = 0;
                _class = 0;
                nrcpts = 0;
                msgid = null;
                proto = null;
                daemon = null;
                client_name = null;
                resolved = null;
                direction = null;
                message_length = 0;
                virus = null;
                disposition = null;
                dst_ip = null;
                endpoint = null;
                classifier = null;
                Spam = null;
                subject = null;
                unexpectedDescription = null;
        }

        private void assignpropertyvalue(int i, string virtualsyslogMessageArr)
        {
            switch (i)
            {
                //case 0:
                //    {
                //        date = virtualsyslogMessageArr;
                //    } break;
                //case 1:
                //    {
                //        time = virtualsyslogMessageArr;
                //    } break;
                //case 2:
                //    {
                //        device_id = virtualsyslogMessageArr;
                //    } break;
                //case 3:
                //    {
                //        log_id =Convert.ToInt64(virtualsyslogMessageArr);
                //    } break;
                //case 4:
                //    {
                //        log_part = Convert.ToInt32(virtualsyslogMessageArr);
                //    } break;
                //case 5:
                //    {
                //        type = virtualsyslogMessageArr;
                //    } break;
                //case 6:
                //    {
                //        subtype = virtualsyslogMessageArr;
                //    } break;
                //case 7:
                //    {
                //        pri = virtualsyslogMessageArr;
                //    } break;
                //case 8:
                //    {
                //        user = virtualsyslogMessageArr;
                //    } break;
                //case 9:
                //    {
                //        ui = virtualsyslogMessageArr;
                //    } break;
                //case 10:
                //    {
                //        action = virtualsyslogMessageArr;
                //    } break;
                //case 11:
                //    {
                //        status = virtualsyslogMessageArr;
                //    } break;
                //case 12:
                //    {
                //        session_id = virtualsyslogMessageArr;
                //    } break;
                //case 13:
                //    {
                //        msg = virtualsyslogMessageArr;
                //    } break;
                //case 14:
                //    {
                //        to = virtualsyslogMessageArr;
                //    } break;
                //case 15:
                //    {
                //        delay = virtualsyslogMessageArr;
                //    } break;
                //case 16:
                //    {
                //        xdelay = virtualsyslogMessageArr;
                //    } break;
                //case 17:
                //    {
                //        mailer = virtualsyslogMessageArr;
                //    } break;
                //case 18:
                //    {
                //        relay = virtualsyslogMessageArr;
                //    } break;
                //case 19:
                //    {
                //        dsn = virtualsyslogMessageArr;
                //    } break;
                //case 20:
                //    {
                //        stat = virtualsyslogMessageArr;
                //    } break;
                //case 21:
                //    {
                //        from = virtualsyslogMessageArr;
                //    } break;
                //case 22:
                //    {
                //        size = Convert.ToInt32(virtualsyslogMessageArr);
                //    } break;
                //case 23:
                //    {
                //        _class = Convert.ToInt32(virtualsyslogMessageArr);
                //    } break;
                //case 24:
                //    {
                //        nrcpts = Convert.ToInt32(virtualsyslogMessageArr);
                //    } break;
                //case 25:
                //    {
                //        msgid = virtualsyslogMessageArr;
                //    } break;
                //case 26:
                //    {
                //        proto = virtualsyslogMessageArr;
                //    } break;
                //case 27:
                //    {
                //        daemon = virtualsyslogMessageArr;
                //    } break;
                //case 28:
                //    {
                //        client_name = virtualsyslogMessageArr;
                //    } break;
                //case 29:
                //    {
                //        resolved = virtualsyslogMessageArr;
                //    } break;
                //case 30:
                //    {
                //        direction = virtualsyslogMessageArr;
                //    } break;
                //case 31:
                //    {
                //        message_length =Convert.ToInt32(virtualsyslogMessageArr);
                //    } break;
                //case 32:
                //    {
                //        virus = virtualsyslogMessageArr;
                //    } break;
                //case 33:
                //    {
                //        disposition = virtualsyslogMessageArr;
                //    } break;
                //case 34:
                //    {
                //        classifier = virtualsyslogMessageArr;
                //    } break;
                //case 35:
                //    {
                //        Spam = virtualsyslogMessageArr;
                //    } break;	
                //case 36:
                //    {
                //        subject = virtualsyslogMessageArr;
                //    } break;

                case 0:
                    {
                        date = virtualsyslogMessageArr;
                    } break;
                case 1:
                    {
                        time = virtualsyslogMessageArr;
                    } break;
                case 2:
                    {
                        device_id = virtualsyslogMessageArr;
                    } break;
                case 3:
                    {
                        log_id = Convert.ToInt64(virtualsyslogMessageArr);
                    } break;
                case 4:
                    {
                        type = virtualsyslogMessageArr;
                    } break;
                case 5:
                    {
                        pri = virtualsyslogMessageArr;
                    } break;
                case 6:
                    {
                        session_id = virtualsyslogMessageArr;
                    } break;
                case 7:
                    {
                        client_name = virtualsyslogMessageArr;
                    } break;
                case 8:
                    {
                        dst_ip = virtualsyslogMessageArr;
                    } break;
                case 9:
                    {
                        endpoint = virtualsyslogMessageArr;
                    } break;
                case 10:
                    {
                        from = virtualsyslogMessageArr;
                    } break;
                case 11:
                    {
                        to = virtualsyslogMessageArr;
                    } break;
                case 12:
                    {
                        subject = virtualsyslogMessageArr;
                    } break;
                case 13:
                    {
                        mailer = virtualsyslogMessageArr;
                    } break;
                case 14:
                    {
                        resolved = virtualsyslogMessageArr;
                    } break;
                case 15:
                    {
                        direction = virtualsyslogMessageArr;
                    } break;
                case 16:
                    {
                        virus = virtualsyslogMessageArr;
                    } break;
                case 17:
                    {
                        disposition = virtualsyslogMessageArr;
                    } break;
                case 18:
                    {
                        classifier = virtualsyslogMessageArr;
                    } break;
                case 19:
                    {
                        message_length = Convert.ToInt32(virtualsyslogMessageArr);
                    } break;
            }
        }

        public Rec createRec()
        {   
            Rec rec = new Rec();

            if (!String.IsNullOrEmpty(date) && !String.IsNullOrEmpty(time))
            {
                rec.Datetime = Convert.ToDateTime(date + " " + time).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec datetime :" + rec.Datetime);
            }
            else 
            {
                rec.Datetime = DateTime.Now.ToString();
            }

            rec.LogName = "Forti Mail Recorder";
            rec.SourceName = local_id;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec SourceName :" + rec.SourceName);
            rec.ComputerName = localinfo;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec ComputerName :" + rec.ComputerName);
            rec.EventCategory = pri;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec pri EventCategory :" + rec.EventCategory);
            rec.EventType = type;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec type EventType :" + rec.EventType);
            rec.CustomStr1 = to;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec to CustomStr1 :" + rec.CustomStr1);
            rec.CustomStr2 = subject;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec subject CustomStr2 :" + rec.CustomStr2);
            rec.CustomStr3 = from;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec from CustomStr3 :" + rec.CustomStr3);
            rec.CustomStr5 = dst_ip;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec dst_ip CustomStr5 :" + rec.CustomStr5);
            rec.CustomStr8 = session_id;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec session_id CustomStr8 :" + rec.CustomStr8);

            string[] ss = client_name.Split('[');
            rec.CustomStr9 = ss[0];
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec client_name Customstr9 :" + rec.CustomStr9);
            string[] sl = ss[1].Split(']');
            rec.CustomStr4 = sl[0];
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec client_name Customstr4 :" + rec.CustomStr4);
            rec.CustomStr10 = device_id;
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec device_id CustomStr10 :" + rec.CustomStr10);
            rec.CustomInt1 = Convert.ToInt32(log_id);
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec log_id CustomInt1 :" + rec.CustomInt1);
            rec.CustomInt2 = Convert.ToInt32(message_length);
            L.Log(LogType.FILE, LogLevel.DEBUG, "FortiMailSyslogRecorder rec message_length CustomInt2 :" + rec.CustomInt2);





            //rec.SourceName = device_id;
            //rec.EventCategory = subtype;
            //rec.EventType = type;
            //rec.UserName = session_id;
            //rec.ComputerName = client_name;

            //rec.CustomInt6 = log_id;
            //rec.CustomInt2 = log_part;
            //rec.CustomInt4 = message_length;

            //rec.CustomStr1 = pri;
            //rec.CustomStr2 = resolved;
            //rec.CustomStr3 = from;
            //rec.CustomStr4 = to;
            //rec.CustomStr5 = direction;
            //rec.CustomStr6 = virus;
            //rec.CustomStr7 = disposition;
            //rec.CustomStr8 = mailer;
            //rec.CustomStr9 = "User : " + user +",  ui : "+ ui +", action : "+ action + ", status : "+ status;
            //rec.CustomStr10 = msg;

            rec.Description = createDescription();

            return rec;
        }

        private string createDescription()
        {
            string desc = "";

            #region type is event
            if (type == "event")
            {
                if (relay != null)
                {
                    desc += "relay=" + relay + ",";
                }
                if (dsn != null)
                {
                    desc += "dsn=" + dsn + ",";
                }
                if (stat != null)
                {
                    desc += "stat=" + stat + ",";
                }
            } 
            #endregion

            #region type is statics
            if (type == "statistics" || type == "spam")
                {
                    if (subject != null)
                    {
                        desc += "subject=" + subject + ",";
                    }
                }
            #endregion
           
            desc = desc.TrimEnd(',');

            if (unexpectedDescription != null && unexpectedDescription != "")
            {
                unexpectedDescription = unexpectedDescription.TrimEnd(',');
                desc += "UNEXPECTED PROPERTY NAMES & VALUES = " + unexpectedDescription;
            }

            return desc;
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
