using System;
using System.Collections.Generic;
using System.Text;
using CustomTools;
using System.ServiceProcess;
using System.Threading;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;

namespace SMGUIRecorder
{
    public class SMGUIRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "Natekdb", wsdb_name_last = "Natekdb", db_name, location, user, password, remote_host = "", last_recdate = DateTime.MinValue.AddYears(2000).ToString(), table_name = "";        
        private bool reg_flag = false;        
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        public SMGUIRecorder()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {            
        }
        public override void Init()
        {            
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SMGuI Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                        Database.CreateDatabase();
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!Get_logDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SMGuI Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating SMGuI DAL");
                        reg_flag = true;                        
                        db_name = "Natekdb";
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create SMGuI DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating SMGuI DAL");
                        Database.CreateDatabase();
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SMGuI Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SMGUIRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SMGuI Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost,String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = "Natekdb";
            last_recordnum = Convert.ToInt64(LastPosition);
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;            
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            wsdb_name = Location;
            table_name  = CustomVar1;
            Dal = dal;
            zone = Zone;                        
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SMGUIRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("SMGUIRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SMGuI Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }        
        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }
        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            string readQuery=null;
            IDataReader readReader = null;
            DbCommand cmd = null;            
            try
            {
                if (!reg_flag)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else if (!Initialize_Logger())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SMGuI Recorder functions may not be running");
                        return;
                    }
                    reg_flag = true;
                }
                if (fromend)
                {
                    last_recdate = DateTime.Now.ToString();
                    fromend = false;
                }                                
                readQuery = "select top " + max_record_send + " Q.STATUS,Q.LOGDATE,Q.USERNAME,Q.TEXT , W.TEXT,Q.TQUERY from " +
                       "(select STATUS,LOGDATE,USERNAME,XTREE.TEXT,XTREE.PARENT,XT.TQUERY " +
                       "from SMGUI_USERLOG SMGUI LEFT OUTER JOIN XTREE ON XTREE.ID = SMGUI.ITEM LEFT OUTER JOIN XTEMPLATES XT ON XT.ID = SMGUI.ITEM WHERE SMGUI.MESSAGEID = 30) Q " +
                       "LEFT OUTER JOIN XTREE W ON W.ID = Q.PARENT WHERE Q.LOGDATE > '" + Convert.ToDateTime(last_recdate) + "' ORDER  BY LOGDATE ASC";               
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                readReader = Database.ExecuteReader("Natekdb", readQuery,CommandBehavior.CloseConnection,out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {                    
                    rec.LogName = "SMGUI Recorder";
                    rec.EventCategory = readReader.GetString(0).ToString();
                    rec.Datetime = readReader.GetDateTime(1).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");                    
                    rec.UserName = readReader.GetString(2).ToString();
                    rec.CustomStr1 = readReader.GetString(3).ToString();
                    rec.CustomStr2 = readReader.GetString(4).ToString();
                    rec.CustomStr3 = readReader.GetString(5).ToString();
                    
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                    }
                    else
                    {
                        last_recdate = rec.Datetime;
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal,virtualhost, rec);
                        s.SetReg(Id, last_recordnum.ToString(), "", "", "", last_recdate); 
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");                    
                    last_recdate = rec.Datetime;                                        
                                                           
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");

            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
                Database.Drop(ref cmd);
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
                EventLog.WriteEntry("Security Manager SMGuI Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
