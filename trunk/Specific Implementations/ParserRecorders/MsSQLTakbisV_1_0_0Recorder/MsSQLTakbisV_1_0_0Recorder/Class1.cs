using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Parser
{
    public struct Fields
    {
        public int Counter;
        public long currentPosition;
        public int ID;
    }

    public class MsSQLTakbisV_1_0_0Recorder : Parser
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "A2009HAR_09_08HAR_DB", wsdb_name_last = "A2009HAR_09_08HAR_DB", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private Fields RecordFields;

        public MsSQLTakbisV_1_0_0Recorder()
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on MsSQLTakbisV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on MsSQLTakbisV_1_0_0Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating MsSQLTakbisV_1_0_0Recorder DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        //db_name = "Websensedb" + Id.ToString();
                        db_name = location;

                        L.Log(LogType.FILE, LogLevel.ERROR, "DBNAME İS : " + db_name);
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create MsSQLTakbisV_1_0_0Recorder DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating MsSQLTakbisV_1_0_0Recorder DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager MsSQLTakbisV_1_0_0Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }

            RecordFields = new Fields();
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\MsSQLTakbisV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager MsSQLTakbisV_1_0_0Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            wsdb_name_last = LastPosition;
            Zone = Zone;
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\MsSQLTakbisV_1_0_0Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("WebsenseRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("MsSQLTakbisV_1_0_0Recorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager MsSQLTakbisV_1_0_0Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            L.Log(LogType.FILE, LogLevel.DEBUG, " Set Last Position.");
            DbCommand cmd = null;
            string readQuery = null;
            IDataReader readReader = null;
            try
            {
                readQuery = "select MAX(ID) as 'ID' FROM LOGLAR";
                L.Log(LogType.FILE, LogLevel.INFORM, " Query is " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish executing the query");
                while (readReader.Read())
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Set last position:  Position: " + readReader["ID"]);
                    last_recordnum = Convert_To_Int64(readReader["ID"].ToString());
                    L.Log(LogType.FILE, LogLevel.INFORM, "Set_LastPosition-->  Position is: " + Position);
                    RecordFields.currentPosition = last_recordnum;
                    //if (usingRegistry)
                    //    Set_Registry(last_recordnum);
                    //else
                    //{
                    //    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    //    s.SetReg(Id, last_recordnum.ToString(), "", "", "", "");
                    //    L.Log(LogType.FILE, LogLevel.INFORM, " Position Updated.");
                    //}
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "SetLastPosition: " + er.ToString());
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
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            string readQuery = null;
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on MsSQLTakbisV_1_0_0Recorder Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
                if (fromend)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Timer Tick Go To Set_LastPosition");
                    if (!Set_LastPosition())
                        L.Log(LogType.FILE, LogLevel.INFORM, "Error on setting last position see eventlog for more details");
                    fromend = false;
                }
                int i = 0;
                L.Log(LogType.FILE, LogLevel.DEBUG, " Position is: " + Position);
                //readQuery = "SELECT TOP " + max_record_send +"ID, KULLANICI_ID, SORGU, TARIH, IP FROM LOGLAR where ID > " + last_recordnum +" order by ID Asc";

                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick Position is 0 ");
                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick Position: " + Position);
                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick RecordFields.currentPosition is: " + RecordFields.currentPosition);
                
                readQuery = "SELECT ID, KULLANICI_ID, SORGU, TARIH, IP FROM LOGLAR where ID > " + Position +
                        " And ID < " + RecordFields.currentPosition + " order by ID Asc";

                L.Log(LogType.FILE, LogLevel.INFORM, " Query is " + readQuery);
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " db_name " + db_name);
                    readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " db_name _ 1");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " db_name " + exception.Message);
                }


                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    rec.LogName = "MsSQLTakbisV_1_0_0Recorder";
                    L.Log(LogType.FILE, LogLevel.DEBUG, " db_name _ 2");

                    //L.Log(LogType.FILE, LogLevel.DEBUG, " Tarih: " + Convert_To_String(Convert_To_String(readReader["TimeStamp"])));

                    rec.SourceName = location;

                    rec.CustomStr1 = Convert_To_String(Convert_To_String(readReader["IP"]));//
                    rec.CustomInt1 = Convert_To_Int32(Convert_To_String(readReader["KULLANICI_ID"]));//
                    rec.Description = Convert_To_String(Convert_To_String(readReader["SORGU"]));//
                    RecordFields.ID = Convert_To_Int32(Convert_To_String(readReader["ID"]));//

                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);

                    DateTime dtNow;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "tarih: " + Convert_To_String(readReader["TARIH"]));
                    dtNow = Convert.ToDateTime(Convert_To_String(Convert_To_String(readReader["TARIH"])));//
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Date Time : " + dtNow);
                    rec.Datetime = dtNow.ToString("yyyy-MM-dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Date Time : " + rec.Datetime);


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
                    RecordFields.Counter++;
                    L.Log(LogType.FILE, LogLevel.INFORM, "Counter: " + RecordFields.Counter);

                    L.Log(LogType.FILE, LogLevel.INFORM, "Finish Sending Data");
                    last_recordnum = rec.Recordnum;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.INFORM, "Record Number is " + RecordFields.currentPosition);
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
                        s.SetReg(Id, RecordFields.ID.ToString(CultureInfo.InvariantCulture), "", "", "", last_recdate);
                        L.Log(LogType.FILE, LogLevel.INFORM, "Update Table TimerTick.");
                    }
                }



                
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
                Database.Drop(ref cmd);
            }
        }


        private string Convert_To_String(object o)
        {
            string result = "";
            try
            {
                result = Convert.ToString(o);
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        private int Convert_To_Int32(string value)
        {
            int result = 0;
            if (Int32.TryParse(value, out result))
                return result;
            else
                return 0;
        }

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            else
                return 0;
        }


        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("MsSQLTakbisV_1_0_0Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager MsSQLTakbisV_1_0_0Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
            IDataReader readReader = null;
            DbCommand cmd = null;
            string dbname = null;
            try
            {
                //readReader = Database.ExecuteReader(db_name,"select MAX(NAME) from master..sysdatabases(nolock) where name like '"+wsdb_name+"%'", out cmd);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start getting dbname");
                L.Log(LogType.FILE, LogLevel.DEBUG, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MAX(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%')");
                readReader = Database.ExecuteReader(db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MAX(crdate) from master..sysdatabases(nolock) where name like '" + wsdb_name + "%')", out cmd);
                while (readReader.Read())
                {
                    dbname = readReader.GetString(0).ToString();
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Succesfully execute get dbname");
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
                EventLog.WriteEntry("Security Manager MsSQLTakbisV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}


