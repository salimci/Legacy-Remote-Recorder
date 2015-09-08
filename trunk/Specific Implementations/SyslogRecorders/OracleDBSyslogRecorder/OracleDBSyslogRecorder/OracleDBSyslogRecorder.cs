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

//namespace BeyazAuditRecorder
namespace OracleDBSyslogRecorder
{
    //public class BeyazAuditRecorder : CustomBase
    public class OracleDBSyslogRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private long last_position;
        private uint logging_interval = 60000, log_size = 1000000;
        private bool reg_flag = false;
        private string err_log, db_name, remote_host = "", location, user, password;
        protected bool usingRegistry = true,fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;

        private CLogger L;

        public OracleDBSyslogRecorder()
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
                            //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on OracleDB Recorder functions may not be running");
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
                            //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on OracleDB Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating Oracle DAL");

                        reg_flag = true;
                        Database.CreateDatabase();
                        //db_name = "Oracledb"+Id.ToString();
                        db_name = location;

                        /*
                        if (Database.AddConnection(db_name, Database.Provider.Oracle, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create Oracle DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating Oracle DAL");
                         */
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager OracleDB Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            last_position = Convert.ToInt64(LastPosition);
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            db_name = Location;
            zone = Zone;
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\BeyazAuditRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Oracledb Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("Trace Level"));
                err_log=rk.OpenSubKey("Agent").GetValue("Home Directory").ToString()+@"log\OracleDBRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("MaxRecordSend"));
                last_position = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("OracleDBRecorder").GetValue("LastPosition").ToString());
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager OracleDB Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                readQuery = "SELECT MAX(SERVICEID) FROM " + db_name + ".UTIL_USERLOG";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

                readReader = Database.ExecuteReader(db_name, readQuery,CommandBehavior.CloseConnection,out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_position = readReader.GetInt64(0);
                    if (usingRegistry)
                        Set_Registry(last_position.ToString());
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_position.ToString(),"","", "",last_position.ToString());
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
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on OracleDB Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }

                if (fromend)
                {
                    if(!Set_LastPosition())
                        L.Log(LogType.FILE, LogLevel.INFORM, "Error on setting last position see eventlog for more details");
                    fromend = false;
                }

                int i = 0;
                //readQuery = "select D.RECORD_NUMBER,D.DATE_TIME,US.USER_LOGIN_NAME,C.NAME AS CATEGORY,D.DISPOSITION_CODE,P.NAME AS PROTOCOL,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock)," + wsdb_name + "..USERS AS US(nolock)," + wsdb_name + "..CATEGORY AS C(nolock)," + wsdb_name + "..PROTOCOLS AS P(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND US.USER_ID=D.USER_ID AND C.CATEGORY=D.CATEGORY AND P.ID =D.PROTOCOL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString();

                //readQuery = "SELECT TOP " + max_record_send + " U.NO, O.ADI, O.SOYADI, U.LOGDATE, U.LOGTIME, U.LOGFILE, U.LOGSYSTEM, U.LOGLEVEL, U.LOGPROC, U.LOGTYPE, U.SERVICEID FROM " + location + ".UTIL_USERLOG AS U(nolock) LEFT JOIN " + location + ".UTIL_OPERATOR O ON U.NO = O.NO WHERE U.SERVICEID>" + last_position + " ORDER BY U.SERVICEID";

                long _lastposition = 0;
                _lastposition = last_position + 100;

                if (last_position == 0)
                {
                    readQuery = "SELECT * FROM ( SELECT UTIL_USERLOG.NO, UTIL_OPERATOR.ADI, UTIL_OPERATOR.SOYADI, UTIL_USERLOG.LOGDATE, UTIL_USERLOG.LOGTIME, UTIL_USERLOG.LOGFILE, UTIL_USERLOG.LOGSYSTEM, UTIL_USERLOG.LOGLEVEL, UTIL_USERLOG.LOGPROC, UTIL_USERLOG.LOGTYPE, UTIL_USERLOG.SERVICEID FROM UTIL_USERLOG LEFT JOIN UTIL_OPERATOR ON UTIL_USERLOG.NO = UTIL_OPERATOR.NO WHERE UTIL_USERLOG.SERVICEID>" + last_position + " ORDER BY UTIL_USERLOG.SERVICEID) CPRIV WHERE ROWNUM > 0 AND ROWNUM < " + max_record_send;
                }
                else 
                {
                    readQuery = "SELECT UTIL_USERLOG.NO, UTIL_OPERATOR.ADI, UTIL_OPERATOR.SOYADI, UTIL_USERLOG.LOGDATE, UTIL_USERLOG.LOGTIME, UTIL_USERLOG.LOGFILE, UTIL_USERLOG.LOGSYSTEM, UTIL_USERLOG.LOGLEVEL, UTIL_USERLOG.LOGPROC, UTIL_USERLOG.LOGTYPE, UTIL_USERLOG.SERVICEID FROM UTIL_USERLOG LEFT JOIN UTIL_OPERATOR ON UTIL_USERLOG.NO = UTIL_OPERATOR.NO WHERE UTIL_USERLOG.SERVICEID > " + last_position + " AND UTIL_USERLOG.SERVICEID < " + _lastposition +" ORDER BY UTIL_USERLOG.SERVICEID";
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " Test  Son DLL ");
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Dbname is " + db_name);

                readReader = Database.ExecuteReader(db_name, readQuery,CommandBehavior.CloseConnection,out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    //dbname = readReader.GetString(0).ToString();
                    rec.LogName = "Beyaz Audit Recorder";
                    //rec.CustomInt6 = Convert.ToInt64(readReader.GetInt64(0));

                    if (readReader.IsDBNull(0))
                        rec.CustomStr1 = "";
                    else
                        rec.CustomStr1 = readReader.GetString(0).ToString();

                    string name = "";
                    string surname = ""; 

                    if (readReader.IsDBNull(1))
                        name = "";
                    else
                        name = readReader.GetString(1).ToString();

                    if (readReader.IsDBNull(2))
                        surname = "";
                    else
                        surname = readReader.GetString(2).ToString();
                    
                    rec.UserName = name + " " + surname;

                    L.Log(LogType.FILE, LogLevel.DEBUG, " User Name " + rec.UserName);

                    string tempdate = "";
                    string temptime = "";

                    if (readReader.IsDBNull(3))
                        tempdate = DateTime.Now.ToShortDateString();
                    else
                        tempdate = readReader.GetDateTime(3).ToShortDateString();                    
                    
                    if (readReader.IsDBNull(4))
                        temptime = DateTime.Now.ToShortTimeString();
                    else
                        temptime = readReader.GetString(4).ToString();

                    string temdatetime = tempdate + " " + temptime;

                    L.Log(LogType.FILE, LogLevel.DEBUG, " temdatetime " + temdatetime);

                    DateTime _tempdatetime = Convert.ToDateTime(temdatetime).AddMinutes(zone);
                    string permanentdatetime = _tempdatetime.ToString("yyyy/MM/dd HH:mm:ss");

                    rec.Datetime = permanentdatetime;
                    
                    if (readReader.IsDBNull(5))
                        rec.CustomStr2 = "";
                    else
                        rec.CustomStr2 = readReader.GetString(5).ToString();

                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr2 " + rec.CustomStr2);

                    if (readReader.IsDBNull(6))
                        rec.CustomStr3 = "";
                    else
                        rec.CustomStr3 = readReader.GetString(6).ToString();

                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomStr3 " + rec.CustomStr3);

                    if (readReader.IsDBNull(7))
                        rec.CustomInt1 = 0;
                    else
                        rec.CustomInt1 = readReader.GetInt32(7);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt1 " + rec.CustomInt1.ToString());

                    if (readReader.IsDBNull(8))
                        rec.CustomInt2 = 0;
                    else
                        rec.CustomInt2 = readReader.GetInt32(8);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt2 " + rec.CustomInt2.ToString());

                    if (readReader.IsDBNull(9))
                        rec.CustomInt3 = 0;
                    else
                        rec.CustomInt3 = readReader.GetInt32(9);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " CustomInt3 " + rec.CustomInt3.ToString());

                    if (readReader.IsDBNull(10))
                        rec.CustomInt6 = 0;
                    else
                        rec.CustomInt6 = readReader.GetInt64(10);

                    last_position = rec.CustomInt6;

                    L.Log(LogType.FILE, LogLevel.DEBUG, " last_position " + last_position.ToString());

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

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is " + last_position.ToString());
                    
                    i++;
                    
                    if (i > max_record_send)
                    {
                            if (usingRegistry)
                                Set_Registry(last_position.ToString());
                            else
                            {
                                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                                s.SetReg(Id, last_position.ToString(), "", "","",rec.Datetime.ToString());
                            }
                            cmd.Cancel();
                            return;
                    }
                    //last_position = rec.Datetime;
                    if (usingRegistry)
                        Set_Registry(last_position.ToString());
                    else
                      {
                           CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                           s.SetReg(Id, last_position.ToString(), "","", "",rec.Datetime.ToString());
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
                readReader.Close();
                Database.Drop(ref cmd);
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        public bool Set_Registry(string status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("OracleDBRecorder");
                rk.SetValue("LastPosition", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager OracleDB Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager OracleDB Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
