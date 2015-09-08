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
using System.Timers;

namespace Websense_7_0Recorder
{
    public class Websense_7_0Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum, last_position;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "wslogdb70", wsdb_name_last = "", db_name, location, user, password, remote_host = "", last_recdate = "", lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;

        public Websense_7_0Recorder()
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
                        db_name = "Websensedb" + Id.ToString();
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\Websense_7_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            last_position = Convert.ToInt64(LastPosition);
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            wsdb_name = Location;
            wsdb_name_last = LastFile;
            lastFile = LastFile;
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\Websense_7_0Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("MaxRecordSend"));
                last_position = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("Websense_7_0Recorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                wsdb_name_last = Get_Ws_Dbname();
                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);
                if (wsdb_name_last == null)
                    wsdb_name_last = wsdb_name;

                readQuery = "IF EXISTS(SELECT TABLE_NAME FROM " + wsdb_name_last + "." + "INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + "LOG_DETAILS" + "') BEGIN select MAX(RECORD_NUMBER) FROM " + wsdb_name_last + "..LOG_DETAILS(nolock) END";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                readReader = Database.ExecuteReader(true, db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnum = readReader.GetInt64(0);
                    if (usingRegistry)
                        Set_Registry(last_position);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_position.ToString(), "", wsdb_name_last, "", "");
                        L.Log(LogType.FILE, LogLevel.INFORM, "Last Db is :" + wsdb_name_last);
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
            string readQuery = "IF EXISTS(SELECT TABLE_NAME FROM " + dbname + "." + "INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tablename + "') BEGIN select 1 AS ControlData" + " END";
            L.Log(LogType.FILE, LogLevel.DEBUG, " Query is in control" + readQuery);
            readReader = Database.ExecuteReader(true, db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
            cmd.CommandTimeout = 1200;

            while (readReader.Read())
            {
                control = true;
                break;
            }
            return control;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            string readQuery = null;
            IDataReader readReader = null;
            DbCommand cmd = null;
            string ip0 = "", ip1 = "", ip2 = "";
            string full_url = "";
            int numberofCharecter = 0;
            int numberofcharecteroverflow = 0;

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
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

                if (wsdb_name_last == null)
                    wsdb_name_last = lastFile;
                if (wsdb_name_last == null)
                    wsdb_name_last = wsdb_name;
                int i = 0;

                //readQuery = "select D.RECORD_NUMBER,D.DATE_TIME,D.USER_ID,D.CATEGORY,D.DISPOSITION_CODE,D.PROTOCOL_ID,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString();//+" ORDER BY D.RECORD_NUMBER";                

                #region if
                if (tableControl("WSE_URLS", wsdb_name) && tableControl("USERS", wsdb_name) && tableControl("CATEGORY", wsdb_name) && tableControl("PROTOCOLS", wsdb_name) && tableControl("LOG_DETAILS", wsdb_name_last))
                {
                    readQuery = "select TOP " + max_record_send + " D.RECORD_NUMBER,D.DATE_TIME,US.USER_LOGIN_NAME,C.NAME AS CATEGORY,D.DISPOSITION_CODE,P.NAME AS PROTOCOL,U.NAME,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED,C.CHILD_NAME,D.FULL_URL from " + wsdb_name + "..WSE_URLS AS U(nolock)," + wsdb_name + "..USERS AS US(nolock)," + wsdb_name + "..CATEGORY AS C(nolock)," + wsdb_name + "..PROTOCOLS AS P(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.WSE_URL_ID AND US.USER_ID=D.USER_ID AND C.CATEGORY=D.CATEGORY AND P.ID =D.PROTOCOL_ID AND D.RECORD_NUMBER>" + last_position.ToString() + " ORDER BY RECORD_NUMBER";
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

                    readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                    cmd.CommandTimeout = 1200;

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                    while (readReader.Read())
                    {
                        //dbname = readReader.GetString(0).ToString();
                        rec.LogName = "Websense_7_0 Recorder";
                        rec.CustomInt6 = Convert.ToInt64(readReader.GetInt64(0));
                        rec.Datetime = readReader.GetDateTime(1).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        if (!readReader.IsDBNull(2))
                            rec.UserName = readReader.GetString(2).ToString();
                        if (!readReader.IsDBNull(3))
                            rec.EventCategory = readReader.GetString(3).ToString();
                        rec.CustomInt1 = Convert.ToInt32(readReader.GetInt16(4));
                        if (!readReader.IsDBNull(5))
                            rec.EventType = readReader.GetString(5).ToString();
                        if (!readReader.IsDBNull(6))
                            rec.Description = readReader.GetString(6).ToString();
                        rec.CustomInt3 = Convert.ToInt32(readReader.GetInt32(7));

                        //rec.CustomInt9 = Convert.ToInt64(readReader.GetInt64(8));
                        //rec.CustomInt7 = Convert.ToInt64(readReader.GetInt64(9));
                        //rec.CustomInt8 = Convert.ToInt64(readReader.GetInt64(10));
                        try
                        {
                            if (!readReader.IsDBNull(8))
                            {
                                ip0 = readReader.GetValue(8).ToString();
                                rec.CustomStr1 = System.Net.IPAddress.Parse(ip0.ToString()).ToString();
                            }
                        }
                        catch (Exception exc)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, exc.Message + " StackTrace:  " + exc.StackTrace);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Parsed value could not convert to IP. Value : " + ip0);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Value will be saved in string format..");
                            rec.CustomStr1 = ip0.ToString();
                        }
                        try
                        {
                            if (!readReader.IsDBNull(9))
                            {
                                ip1 = readReader.GetValue(9).ToString();
                                rec.CustomStr4 = System.Net.IPAddress.Parse(ip1.ToString()).ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, ex.Message + " StackTrace:  " + ex.StackTrace);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Value will be saved in string format..");
                            rec.CustomStr4 = ip1.ToString();
                        }
                        try
                        {
                            if (!readReader.IsDBNull(10))
                            {
                                ip2 = readReader.GetValue(10).ToString();
                                rec.CustomStr3 = System.Net.IPAddress.Parse(ip2.ToString()).ToString();
                            }
                        }
                        catch (Exception ec)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, ec.Message + " StackTrace:  " + ec.StackTrace);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Value will be saved in string format..");
                            rec.CustomStr3 = ip2.ToString();
                        }

                        rec.CustomInt5 = Convert.ToInt32(readReader.GetInt32(11));
                        if (!readReader.IsDBNull(14))
                            rec.CustomStr5 = readReader.GetString(14).ToString();

                        if (!readReader.IsDBNull(12))
                            rec.CustomInt7 = Convert.ToInt64(readReader.GetValue(12).ToString().Trim()); // D.BYTES_SENT

                        if (!readReader.IsDBNull(13))
                            rec.CustomInt8 = Convert.ToInt64(readReader.GetValue(13).ToString().Trim()); // D.BYTES_RECEIVED

                        try
                        {
                            if (!readReader.IsDBNull(15))
                            {
                                full_url = readReader.GetString(15).ToString(); //FULL_URL
                                numberofCharecter = full_url.Length;

                                if (numberofCharecter > 900)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "URL Length is greter than 900 character");
                                    rec.CustomStr6 = full_url.Substring(0, 900);
                                    numberofcharecteroverflow = numberofCharecter - 900;
                                    rec.CustomStr7 = full_url.Substring(900, numberofcharecteroverflow);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Second Part is " + numberofcharecteroverflow.ToString());
                                }
                                else
                                {
                                    rec.CustomStr6 = full_url;
                                }
                            }
                        }
                        catch (Exception ec)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, ec.Message + " StackTrace:  " + ec.StackTrace);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Value will be saved in string format..");
                            rec.CustomStr6 = "";
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
                            s.SetData(Dal, virtualhost, rec);
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                        last_position = rec.CustomInt6;
                        last_recdate = rec.Datetime;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + last_position.ToString());
                        i++;
                        if (i > max_record_send)
                        {
                            cmd.Cancel();
                            return;
                        }
                        lastFile = wsdb_name_last;
                        if (usingRegistry)
                            Set_Registry(last_position);
                        else
                        {
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetReg(Id, last_position.ToString(), "", lastFile, "", last_recdate);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Last File Is -->>" + lastFile);
                        }
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");
                }
                #endregion
                #region else
                else
                {
                    bool tablecontrolWSE_URLS = tableControl("WSE_URLS", wsdb_name);
                    bool tablecontrolUSERS = tableControl("USERS", wsdb_name);
                    bool tablecontrolCATEGORY = tableControl("CATEGORY", wsdb_name);
                    bool tablecontrolPROTOCOLS = tableControl("PROTOCOLS", wsdb_name);
                    bool tablecontrolLOG_DETAILS = tableControl("LOG_DETAILS", wsdb_name_last);
                    string messageText = "";

                    if (!tablecontrolWSE_URLS)
                    {
                        messageText = messageText + " WSE_URLS Table In the " + wsdb_name + " Not Found, ";
                    }
                    if (!tablecontrolUSERS)
                    {
                        messageText = messageText + " USERS Table In the " + wsdb_name + " Not Found, ";
                    }
                    if (!tablecontrolCATEGORY)
                    {
                        messageText = messageText + " CATEGORY Table In the " + wsdb_name + " Not Found, ";
                    }
                    if (!tablecontrolPROTOCOLS)
                    {
                        messageText = messageText + " PROTOCOLS Table In the " + wsdb_name + " Not Found, ";
                    }
                    if (!tablecontrolLOG_DETAILS)
                    {
                        messageText = messageText + " LOG_DETAILS Table In the " + wsdb_name_last + " Not Found, ";
                    }

                    messageText = messageText.Trim();
                    messageText = messageText.Trim(',');
                    L.Log(LogType.FILE, LogLevel.INFORM, messageText);
                    wsdb_name_last = Get_Ws_Dbname();
                }
                #endregion
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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("Websense_7_0Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Websense Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Websense Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public string Get_Ws_Dbname()
        {
            IDataReader readReader = null;
            IDataReader readReader2 = null;
            DbCommand cmd = null;
            string dbname = null;
            IDataReader readReader22 = null;

            if (!String.IsNullOrEmpty(wsdb_name_last))
            {
                try
                {
                    string readQuery = null;

                    if (tableControl("LOG_DETAILS", wsdb_name_last))
                    {
                        readQuery = "select MAX(RECORD_NUMBER) FROM " + wsdb_name_last + "..LOG_DETAILS(nolock)";

                        readReader22 = Database.ExecuteReader(true, db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                        cmd.CommandTimeout = 1200;

                        while (readReader22.Read())
                        {
                            last_recordnum = readReader22.GetInt64(0);
                        }
                    }
                    else
                    {
                        last_recordnum = 0;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "In Get_Ws_Dbname()Function-> Table not Found Last Record Number is setted 0");
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "In Get_Ws_Dbname()Function-> catch()");
                }
                finally
                {
                    if (readReader22 != null)
                    {
                        readReader22.Close();
                        Database.Drop(ref cmd);
                    }
                }
            }

            if (wsdb_name_last == "" || wsdb_name_last == " " || wsdb_name_last == null)
            {
                try
                {
                    //readReader = Database.ExecuteReader(true, db_name,"select MAX(NAME) from master..sysdatabases(nolock) where name like '"+wsdb_name+"%'", out cmd);
                    readReader = Database.ExecuteReader(true, db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MIN(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%')", out cmd);
                    while (readReader.Read())
                    {
                        dbname = readReader.GetString(0).ToString();
                    }
                    string readQuery = "select MAX(RECORD_NUMBER) FROM " + dbname + "..LOG_DETAILS(nolock)";
                    readReader22 = Database.ExecuteReader(true, db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                    while (readReader22.Read())
                    {
                        last_recordnum = readReader22.GetInt64(0);
                    }

                    return dbname;
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                    return dbname;
                }
                finally
                {
                    readReader22.Close();
                    if (readReader != null)
                        readReader.Dispose();
                    Database.Drop(ref cmd);
                }
            }
            else
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "totalRecordNumber : " + last_recordnum);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "parsedRecordNumber : " + last_position);
                    if (last_recordnum == last_position)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last record parsed, changing Websense Db..");
                        readReader2 = Database.ExecuteReader(true, db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MIN(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%' AND crdate > (select crdate from  master..sysdatabases(nolock) where name = '" + wsdb_name_last + "'))", out cmd);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "QUERY is  ::  SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MIN(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%' AND crdate > (select crdate from  master..sysdatabases(nolock) where name = '" + wsdb_name_last + "'))");
                        while (readReader2.Read())
                        {///hata
                            dbname = readReader2.GetString(0);
                        }
                        if (String.IsNullOrEmpty(dbname))
                        {
                            dbname = wsdb_name_last;
                        }
                        if (dbname != wsdb_name_last)
                        {
                            last_position = 0;
                        }

                        return dbname;
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Recorder is not reach end of DB parsing continues");
                    return wsdb_name_last;
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    return wsdb_name_last;
                }
                finally
                {
                    if (readReader != null)
                        readReader.Dispose();
                    if (readReader2 != null)
                        readReader2.Dispose();
                    Database.Drop(ref cmd);
                }
            }
        }
    }
}
