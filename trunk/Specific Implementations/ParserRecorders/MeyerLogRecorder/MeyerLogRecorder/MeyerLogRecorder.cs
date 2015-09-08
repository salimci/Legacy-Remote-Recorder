using System;
using System.Data.SqlClient;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;

namespace MeyerLogRecorder
{
    public class MeyerLogRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "", wsdb_name_last = "", db_name, location, user, password, remote_host = "", last_recdate = "", table_name = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        private RecordFields recordFields;

        public struct RecordFields
        {
            public string ID;
        }

        public MeyerLogRecorder()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
        }

        public override void Init()
        {
            recordFields = new RecordFields();
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Meyer Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                        //Database.CreateDatabase();
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Meyer Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating Meyer DAL");
                        reg_flag = true;
                        //Database.CreateDatabase();
                        db_name = "master";
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create Meyer DAL");
                        }
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating Meyer DAL");
                        }
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Meyer Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "Exit Init Function.");
        }
        public bool Get_logDir()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Start Get_LogDir.");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\MeyerRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Meyer Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            wsdb_name = Location;
            table_name = CustomVar1;
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\MeyerRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("MeyerRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Meyer Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public string GetUserName(string UserId)
        {
            try
            {
                //DataTable tb = GetDataTable("Meyer", "Select TOP 1 Ad, Soyad From Sicil where USERID = " + UserId);
                //return (tb.Rows[0][0]+ " " + tb.Rows[0][1]);
                SqlConnection _SqlConnection = new SqlConnection();
                _SqlConnection.ConnectionString = "server=192.168.0.86;user=sa;pwd=Meyer1878;database=EMLAKKONUT_MEYER;";
                string sql = "select TOP 1 Ad, Soyad from Sicil where USERID = " + UserId;
                SqlCommand _SqlCommand = new SqlCommand(sql, _SqlConnection);
                SqlDataAdapter _SqlDataAdapter = new SqlDataAdapter();
                _SqlDataAdapter.SelectCommand = _SqlCommand;
                DataTable _DataTable = new DataTable();
                _SqlDataAdapter.Fill(_DataTable);
                return (_DataTable.Rows[0][0] + " " + _DataTable.Rows[0][1]);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "GetUserName: " + exception.Message);
                return null;
            }
        } // GetUserName

        private DataTable GetDataTable(String dataBaseName, String query)
        {
            try
            {
                Database.Fast = false;
                DbCommand dbCommand = null;
                DataSet dataSet = new DataSet();
                IDataAdapter idataAdapt = Database.GetDataAdapter(dataBaseName, query, out dbCommand);
                idataAdapt.Fill(dataSet);
                Database.Drop(ref dbCommand);
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "GetDataTable: " + ex.Message);
                return null;
            }
        } // GetDataTable

        public bool Set_LastPosition()
        {
            DbCommand cmd = null;
            string readQuery = null;
            IDataReader readReader = null;
            try
            {
                wsdb_name_last = wsdb_name;
                readQuery = "select Max(LogId) FROM Pool";
                L.Log(LogType.FILE, LogLevel.DEBUG, "Set_LastPosition  Query is " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnum = readReader.GetInt64(0);
                    if (usingRegistry)
                        Set_Registry(last_recordnum);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnum.ToString(), "", "", "", "");
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
        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.DEBUG, "Service Started");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on MeyerLog  Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "FromEnd " + fromend);
                if (fromend)
                {
                    if (!Set_LastPosition())
                        L.Log(LogType.FILE, LogLevel.INFORM, "Error on setting last position see eventlog for more details");
                    fromend = false;
                }
                //L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);
                if (wsdb_name_last == null)
                    wsdb_name_last = wsdb_name;
                int i = 0;

                L.Log(LogType.FILE, LogLevel.DEBUG, " max_record_send: " + max_record_send);
                L.Log(LogType.FILE, LogLevel.DEBUG, " last_recordnum: " + last_recordnum);

                readQuery = "select TOP " + max_record_send + " * from dbo." + table_name + " where ID > " + last_recordnum.ToString() + " ORDER BY ID";
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    rec.LogName = "Meyer Recorder";
                    rec.CustomStr1 = readReader["UserID"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr1 : " + rec.CustomStr1);
                    rec.CustomStr2 = readReader["SicilID"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr2 : " + rec.CustomStr2);
                    rec.CustomStr3 = readReader["TerminalID"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr3 : " + rec.CustomStr3);
                    rec.EventId = Convert_To_Int64(readReader["EventCode"].ToString());
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.EventId : " + rec.EventId);
                    rec.CustomStr4 = readReader["FuncCode"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr4 : " + rec.CustomStr4);
                    rec.CustomStr5 = readReader["Deleted"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr5 : " + rec.CustomStr5);
                    recordFields.ID = readReader["ID"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ID : " + recordFields.ID);

                    rec.CustomStr9 = GetUserName(rec.CustomStr1);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.UserName : " + rec.UserName);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.UserName : " + rec.UserName);
                    if (!string.IsNullOrEmpty(readReader["Status"].ToString()))
                    {
                        rec.CustomStr6 = readReader["Status"].ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr6 : " + rec.CustomStr6);
                    }

                    rec.CustomStr7 = readReader["PDKS"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr7 : " + rec.CustomStr7);
                    rec.CustomStr8 = readReader["ForeignID"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, " rec.CustomStr8 : " + rec.CustomStr8);


                    DateTime dt = Convert.ToDateTime(readReader["EventTime"]);
                    rec.Datetime = dt.ToString(dateFormat);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Datetime : " + rec.Datetime);
                    rec.ComputerName = remote_host;
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
                    last_recordnum = Convert.ToInt32(recordFields.ID);
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
                        s.SetReg(Id, last_recordnum.ToString(), "", "", "", last_recdate);
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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("MeyerRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Meyer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Meyer Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
