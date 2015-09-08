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

namespace McaffeeEpoRecorder
{
    public class McaffeeEpoRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, mcdb_name = "ePO4_HMEPO", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
          
        public McaffeeEpoRecorder()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            /*
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;

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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
                        return;
                    }
                reg_flag = true;
            }

            Database.CreateDatabase();
             */
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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
                            //L.Log(LogType.FILE, LogLevel.ERROR, " Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating McaffeeEpo DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        db_name = "McaffeeEpodb" + Id.ToString();
                        L.Log(LogType.FILE, LogLevel.INFORM, "db_name : " + db_name);
                        L.Log(LogType.FILE, LogLevel.INFORM, "location : " + location);

                        L.Log(LogType.FILE, LogLevel.INFORM, "Log Baþlangýcý");
                        L.Log(LogType.FILE, LogLevel.INFORM, "db_name : " + db_name);
                        L.Log(LogType.FILE, LogLevel.INFORM, "remote_host : " + remote_host);
                        L.Log(LogType.FILE, LogLevel.INFORM, "user : " + user);
                        L.Log(LogType.FILE, LogLevel.INFORM, "password : " + password);
                        L.Log(LogType.FILE, LogLevel.INFORM, "location : " + location);
                        L.Log(LogType.FILE, LogLevel.INFORM, "Log Bitiþi");

                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create McaffeeEpo DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating McaffeeEpo DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\McaffeeEpo_7_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            location = Location;
            last_recordnum = Convert.ToInt64(LastPosition);
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            mcdb_name = "[" + Location + "]";
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\McaffeeEpoRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Interval"));
                mcdb_name= rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("MCDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        
        public bool Set_LastPosition()
        {
            DbCommand cmd = null;
            string readQuery = null;
            IDataReader readReader = null;
            try
            {
               
                L.Log(LogType.FILE, LogLevel.INFORM, "dbname is: " + mcdb_name);

                readQuery = "select MAX(AutoID) FROM " + mcdb_name + "..EPOEvents(nolock)";

                L.Log(LogType.FILE, LogLevel.INFORM, " Query is " + readQuery);

                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnum =Convert.ToInt64(readReader.GetInt32(0));
                    if (usingRegistry)
                        Set_Registry(last_recordnum);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnum.ToString(), "", "","","");
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
            finally
            {
                readReader.Close();
                Database.Drop(ref cmd);
            }
        }
        
        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        public bool tableControl(string tablename, string dbname) 
        {
            bool control = false;

            IDataReader readReader = null;
            DbCommand cmd = null;
            string readQuery = "IF EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + "EPOEvents" + "') BEGIN select TOP 1 * from " + dbname + ".."+tablename+" END";
            L.Log(LogType.FILE, LogLevel.DEBUG, " Query is in control" + readQuery);
            readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
            cmd.CommandTimeout = 1200;
            
            while (readReader.Read())
            {
                control = true;
            }
            return control;
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
                // Fill the record fileds with necessary parameters 
                //readQuery = "SELECT UPPER(HOST_NAME) AS HOST_NAME FROM NODE WHERE LAST_UPDATED < (getdate() - CONVERT(datetime,'" + respond_hour + ":" + respond_time + ":0',108)) ORDER BY LAST_UPDATED DESC";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }

                if (fromend)
                {
                    if (!Set_LastPosition())
                        L.Log(LogType.FILE, LogLevel.INFORM, "Error on setting last position see eventlog for more details");
                    fromend = false;
                }

                /*
                wsdb_name_last= Get_Ws_Dbname();
                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);

                if (wsdb_name_last == null)
                    wsdb_name_last = wsdb_name;

                */


                int i = 0;
                L.Log(LogType.FILE, LogLevel.INFORM, "Timer :  ");
                
                //readQuery = "select D.RECORD_NUMBER,D.DATE_TIME,D.USER_ID,D.CATEGORY,D.DISPOSITION_CODE,D.PROTOCOL_ID,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString();//+" ORDER BY D.RECORD_NUMBER";                
                readQuery = "select TOP " + max_record_send + " E.AutoID, E.ServerID, E.ReceivedUTC, E.DetectedUTC, E.Analyzer, E.AnalyzerVersion, E.AnalyzerHostName, E.AnalyzerDetectionMethod, E.TargetHostName, E.TargetUserName, E.TargetPort, E.TargetFileName, E.ThreatCategory, E.ThreatEventID, E.ThreatSeverity, E.ThreatName, E.ThreatType, E.ThreatActionTaken from " + mcdb_name + "..EPOEvents AS E(nolock) WHERE E.AutoID >" + last_recordnum + " ORDER BY E.AutoID";
                L.Log(LogType.FILE, LogLevel.INFORM, " Query is in if" + readQuery);

                L.Log(LogType.FILE, LogLevel.INFORM, " Query is " + readQuery);

                readReader = Database.ExecuteReader(db_name, readQuery,CommandBehavior.CloseConnection,out cmd);
                cmd.CommandTimeout = 1200;
                
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                
                while (readReader.Read())
                {
                    rec.LogName = "McaffeeEpo Recorder";

                    if (!Convert.IsDBNull(readReader["DetectedUTC"]))
                    {
                        rec.CustomStr1 = readReader.GetDateTime(3).ToString("yyyy/MM/dd HH:mm:ss"); // DetectedUTC
                    }
                  

                    if (!Convert.IsDBNull(readReader["ServerID"]))
                    {
                       rec.CustomStr2 = readReader.GetString(1).ToString(); //  ServerID
                    }
                   

                    if (!Convert.IsDBNull(readReader["TargetHostName"]))
                    {
                       rec.CustomStr3 = readReader.GetString(8).ToString(); //  TargetHostName
                    }
                   
                    if (!Convert.IsDBNull(readReader["TargetUserName"]))
                    {
                        rec.CustomStr4 = readReader.GetString(9).ToString(); // TargetUserName
                    }

                    if (!Convert.IsDBNull(readReader["TargetFileName"]))
                    {
                       rec.CustomStr5 = readReader.GetString(11).ToString(); // TargetFileName 
                    }
                    
                    if (!Convert.IsDBNull(readReader["ReceivedUTC"]))
                    {
                        rec.CustomStr6 = readReader.GetDateTime(2).ToString("yyyy/MM/dd HH:mm:ss"); // ReceivedUTC
                    }
                    
                    if (!Convert.IsDBNull(readReader["ThreatName"]))
                    {
                        rec.CustomStr7 = readReader.GetString(15).ToString(); // ThreatName
                    }
                    
                    if (!Convert.IsDBNull(readReader["ThreatType"]))
                    {
                        rec.CustomStr8 = readReader.GetString(16).ToString(); // ThreatType
                    }
                    
                    if (!Convert.IsDBNull(readReader["ThreatActionTaken"]))
                    {
                        rec.CustomStr9 = readReader.GetString(17).ToString(); // ThreatActionTaken
                    }
                   
                    string analyzer = "empty";
                    string analyzerVersion = "empty";
                    string analyzerHostName = "empty";
                    string analyzerDetectionMethod = "empty";
                    string str10 = "";

                    if (!Convert.IsDBNull(readReader["Analyzer"]))
                    {
                        analyzer = readReader.GetString(4).ToString(); // Analyzer
                    }
                  
                    if (!Convert.IsDBNull(readReader["AnalyzerVersion"]))
                    {
                        analyzerVersion = readReader.GetString(5).ToString(); // AnalyzerVersion
                    }
                    
                    if (!Convert.IsDBNull(readReader["AnalyzerHostName"]))
                    {
                        analyzerHostName = readReader.GetString(6).ToString(); // AnalyzerHostName
                    }
                    
                    if (!Convert.IsDBNull(readReader["AnalyzerDetectionMethod"]))
                    {
                        analyzerDetectionMethod = readReader.GetString(7).ToString(); // AnalyzerDetectionMethod
                    }
                    
                    str10 = "Analyzer is : " + analyzer + " | " + "Analyzer Version is : " + analyzerVersion + " | " + "Analyzer Host Name is :" + analyzerHostName + " | " + "Analyzer Detection Method is :" + analyzerDetectionMethod;  //

                    rec.CustomStr10 = str10;
                    if (!Convert.IsDBNull(readReader["TargetPort"])) //int
                    {
                        rec.CustomInt1 = readReader.GetInt32(10); // TargetPort
                    }
                   
                    if (!Convert.IsDBNull(readReader["ThreatEventID"])) //int
                    {
                        rec.CustomInt2 = readReader.GetInt32(13); // ThreatEventID
                    }
                   
                    if (!Convert.IsDBNull(readReader["ThreatSeverity"])) //tinyint
                    {
                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(readReader.GetByte(14)); // ThreatSeverity
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Threat Severity is :" + rec.CustomInt3);
                        }
                        catch (Exception)
                        {
                                rec.CustomInt3 = 0;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Threat Severity Alýnýrken Hata Oluþtu ve 0 a atandý");
                        }
                    }
                   
                    if (!Convert.IsDBNull(readReader["AutoID"]))
                    {
                        rec.CustomInt6 = Convert.ToInt64(readReader.GetInt32(0));  //  AutoID int
                    }
                    
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"); //

                    if (!Convert.IsDBNull(readReader["ThreatCategory"]))
                    {
                        rec.EventCategory = readReader.GetString(12).ToString();    //ThreatCategory
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
                        s.SetData(Dal,virtualhost, rec);
                    }
                    
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                    last_recordnum = rec.CustomInt6;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + last_recordnum.ToString());
                    i++;
                    if (i > max_record_send)
                    {
                        cmd.Cancel();
                        return;
                    }
                    
                    if (usingRegistry)
                        Set_Registry(last_recordnum);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnum.ToString(), "","", "", last_recdate);
                    }   
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
        
        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("McaffeeEpoRecorder");
                rk.SetValue("LastRecordNum",status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        
        /*
        public string Get_Ws_Dbname()
        {
            IDataReader readReader = null;
            DbCommand cmd = null;
            string dbname=null;
            try
            {
                //readReader = Database.ExecuteReader(db_name,"select MAX(NAME) from master..sysdatabases(nolock) where name like '"+wsdb_name+"%'", out cmd);
                readReader = Database.ExecuteReader(db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MAX(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%')", out cmd);
                while (readReader.Read())
                {
                    dbname= readReader.GetString(0).ToString();
                    //dbname="wslogdb70_55";
                }
                //dbname = "wslogdb63_20";
                return dbname;
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                return dbname;
            }
            finally
            {
                if (readReader != null)
                    readReader.Dispose();
                Database.Drop(ref cmd);
            }
        }
        */
        
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
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
