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

namespace GKS_Recorder
{
    public class GKS_Recorder : CustomBase
    {
        
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        //wsdbnameleri kontrol et
        private string err_log, wsdb_name, wsdb_name_last, db_name, location, user, password, remote_host, last_recdate="";
        //using registry true olcak en son reg flag false
        private bool reg_flag = false;
        
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost,Dal;
        private CLogger L;
        public GKS_Recorder()
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating Websense DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        //Database.CreateDatabase();
                        //Database.Fast = true;
                        db_name = location;
                        if (Database.AddConnection(db_name, Database.Provider.MySQL, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create Websense DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating Websense DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\Patent_GKS_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                //Console.WriteLine("error on getlog dir");
                EventLog.WriteEntry("Security Manager Patent_GKS_Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("WebsenseRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\Patent_GKS_Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("Patent_GKS_Recorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Patent_GKS_Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            Console.WriteLine("last position");

            DbCommand cmd = null;
            string readQuery = null;
            IDataReader readReader = null;
            try
            {

                //wsdb_name_last = Get_Ws_Dbname();
               L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);

                //if (wsdb_name_last == null)
                   //emr  wsdb_name_last = wsdb_name;
                    //SELECT GID FROM `test`.`personnelgatelog`
                // emr  readQuery = "select MAX(RECORD_NUMBER) FROM " + wsdb_name_last + "..LOG_DETAILS(nolock)";
                    Console.WriteLine("null degil");
                Console.WriteLine("wsdb_name_last " + wsdb_name_last);
                    readQuery = "select MAX(GID) FROM " + wsdb_name_last + ".PERSONNELGATELOG";
               L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                  // foreach (string z in Database.GetConnectionNames())
                    //    Console.WriteLine(z);

                    //Console.ReadLine();
                   
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                Console.WriteLine("2");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnum = readReader.GetInt64(0);
                    if (usingRegistry)
                        Set_Registry(last_recordnum);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        Console.WriteLine("last_recordnum" + last_recordnum);
                        s.SetReg(Id, last_recordnum.ToString(), "", "","",last_recdate);
                        
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,"last err"+ er.ToString());
                Console.WriteLine("error on last position"+er.ToString());
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

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("func");
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            string readQuery = null;
            IDataReader readReader = null;
            DbCommand cmd = null;
            Console.WriteLine("azeneme1");
            try
            {
                // Fill the record fileds with necessary parameters 
                //readQuery = "SELECT UPPER(HOST_NAME) AS HOST_NAME FROM NODE WHERE LAST_UPDATED < (getdate() - CONVERT(datetime,'" + respond_hour + ":" + respond_time + ":0',108)) ORDER BY LAST_UPDATED DESC";
                Console.WriteLine("azeneme2");
                if (!reg_flag)
                {
                    Console.WriteLine("azeneme3");
                    if (!Read_Registry())
                    {
                        Console.WriteLine("azeneme4");
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                         L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Patent_GKS_Recorder functions may not be running");
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

                wsdb_name_last = Get_Ws_Dbname();
                // emr
                         L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);

                if (wsdb_name_last == null)
                   wsdb_name_last = wsdb_name;

                int i = 0;

                //readQuery = "select D.RECORD_NUMBER,D.DATE_TIME,D.USER_ID,D.CATEGORY,D.DISPOSITION_CODE,D.PROTOCOL_ID,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString();//+" ORDER BY D.RECORD_NUMBER";                
                // read new 
                //emrr   readQuery = "select TOP(" + max_record_send + ") D.RECORD_NUMBER,D.DATE_TIME,US.USER_LOGIN_NAME,C.NAME AS CATEGORY,D.DISPOSITION_CODE,P.NAME AS PROTOCOL,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock)," + wsdb_name + "..USERS AS US(nolock)," + wsdb_name + "..CATEGORY AS C(nolock)," + wsdb_name + "..PROTOCOLS AS P(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND US.USER_ID=D.USER_ID AND C.CATEGORY=D.CATEGORY AND P.ID =D.PROTOCOL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString() + " ORDER BY RECORD_NUMBER";
                //readQuery = "SELECT A.GID,A.EVENTDATE,A.EVENTTIME,B.NAME,B.SURNAME,C.NAME,D.NAME,E.NAME,F.NAME,G1.NAME,G2.NAME FROM " + wsdb_name + ".personnelgatelog AS A," + wsdb_name + ".personnel AS B," + wsdb_name + ".eventtype AS C," + wsdb_name + ".department AS D," + wsdb_name + ".groups AS E," + wsdb_name + ".positions as F," + wsdb_name + ".zone as G1," + wsdb_name + ".zone as G2 WHERE A.LU_PERSONNEL_ID=B.ID AND A.LU_EVENTTYPE_ID=C.ID AND B.LU_DEPARTMENT_ID=D.ID AND B.LU_GROUPS_ID=E.ID AND B.LU_POSITIONS_ID=F.ID AND A.LU_ZONE_FROMZONEID=G1.ID AND A.LU_ZONE_TOZONEID=G2.ID AND A.GID>" + last_recordnum.ToString() + " ORDER BY A.GID LIMIT " + max_record_send;
                readQuery = "SELECT A.GID,A.EVENTDATE,A.EVENTTIME,B.NAME,B.SURNAME,C.NAME,D.NAME,E.NAME,F.NAME,G1.NAME,G2.NAME FROM PERSONNELGATELOG AS A,PERSONNEL AS B,EVENTTYPE AS C,DEPARTMENT AS D,GROUPS AS E,POSITIONS as F,ZONE as G1,ZONE as G2 WHERE A.LU_PERSONNEL_ID=B.ID AND A.LU_EVENTTYPE_ID=C.ID AND B.LU_DEPARTMENT_ID=D.ID AND B.LU_GROUPS_ID=E.ID AND B.LU_POSITIONS_ID=F.ID AND A.LU_ZONE_FROMZONEID=G1.ID AND A.LU_ZONE_TOZONEID=G2.ID AND A.GID>" + last_recordnum.ToString() + " ORDER BY A.GID LIMIT " + max_record_send;

                //readQuery = "SELECT A.GID,A.EVENTDATE,A.EVENTTIME,B.NAME,B.SURNAME FROM " + wsdb_name + ".personnelgatelog AS A," + wsdb_name + ".personnel AS B WHERE A.LU_PERSONNEL_ID=B.ID LIMIT " + max_record_send;
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                //Console.WriteLine("func 2");
                Console.WriteLine("emrah dbname read query "+db_name +" "+readQuery );
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                   

                   rec.LogName = "GKS_Recorder";
                   //Console.WriteLine("rec.LogName " + rec.LogName);
                   rec.CustomInt6 = Convert.ToInt64(readReader.GetInt64(0));
                   //Console.WriteLine("rec.CustomInt6 " + rec.CustomInt6);
                   //emr rec.Datetime = readReader.GetDateTime(1).ToString("yyyy/MM/dd HH:mm:ss.fff");
                   string d1 = readReader.GetDateTime(1).ToString("yyyy/MM/dd");
                   string[] temp = d1.Split('.');                   
                   d1 = temp[0] + "/" + temp[1] + "/" + temp[2];
                    //Console.WriteLine("D1 " + d1);
                   //readReader.GetValue
                   L.Log(LogType.FILE, LogLevel.DEBUG, "d1" + d1);
                   string d2 = readReader.GetValue(2).ToString();
                   //Console.WriteLine("D2 " + d2);
                   L.Log(LogType.FILE, LogLevel.DEBUG, "d2" + d2);
                   string d3= d1+" "+d2;
                   //Console.WriteLine("D3 " + d3);
                   L.Log(LogType.FILE, LogLevel.DEBUG, "d3" + d3);
                   rec.Datetime = Convert.ToDateTime(d3).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                   L.Log(LogType.FILE, LogLevel.DEBUG, "datetime->" + rec.Datetime);
                   rec.CustomStr1 = readReader.GetString(3).ToString();
                   Console.WriteLine("3 USER " +rec.CustomStr1);
                   L.Log(LogType.FILE, LogLevel.DEBUG, "name " + rec.CustomStr1);
                   rec.CustomStr2 = readReader.GetString(4).ToString();
                   //Console.WriteLine("4 SURNAME " + rec.CustomStr2);
                   rec.CustomStr3 = readReader.GetString(5).ToString();
                   Console.WriteLine("5 gecis tipi " + rec.CustomStr3);
                   rec.CustomStr4 = readReader.GetString(6).ToString();
                   //Console.WriteLine("6 departman" + rec.CustomStr4);
                   rec.CustomStr5 = readReader.GetString(7).ToString();
                   //Console.WriteLine("7 group" + rec.CustomStr5);
                   rec.CustomStr6 = readReader.GetString(8).ToString();
                   //Console.WriteLine("8 posizyon" + rec.CustomStr6);
                   rec.CustomStr7 = readReader.GetString(9).ToString();
                   //Console.WriteLine("9 nerden" + rec.CustomStr7);
                   rec.CustomStr8 = readReader.GetString(10).ToString();
                   Console.WriteLine("9 nereye" + rec.CustomStr8);
                 
                   L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                   //Console.WriteLine("SENDING DATA ");
                    if (usingRegistry)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "uses registry");
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");                        
                        s.SetData(rec);
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "dal virtualhost"+Dal+" "+virtualhost);
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        last_recdate = rec.Datetime;
                        s.SetData(Dal, virtualhost, rec);
                    }
                     L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                     last_recordnum = rec.CustomInt6;
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
                        L.Log(LogType.FILE, LogLevel.DEBUG, "id lastrecordnum" + Id+ " " + last_recordnum);
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnum.ToString(), "", "","",last_recdate);
                    }

                }
               L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");

            }
            catch (Exception er)
            {
                Console.WriteLine("ERRROROROROO "+er.Message);
             L.Log(LogType.FILE, LogLevel.ERROR, "timer"+er.ToString());
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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("Patent_GKS_Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
             //   L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Patent_GKS_Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public string Get_Ws_Dbname()
        {
            string dbname = null;
            /*
            IDataReader readReader = null;
            DbCommand cmd = null;
            string dbname = null;
            try
            {
                //readReader = Database.ExecuteReader(db_name,"select MAX(NAME) from master..sysdatabases(nolock) where name like '"+wsdb_name+"%'", out cmd);
                readReader = Database.ExecuteReader(db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MAX(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%')", out cmd);
                while (readReader.Read())
                {
                    dbname = readReader.GetString(0).ToString();
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
            */
            dbname = location;
            return dbname;
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
                EventLog.WriteEntry("Security Manager Patent_GKS_Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
