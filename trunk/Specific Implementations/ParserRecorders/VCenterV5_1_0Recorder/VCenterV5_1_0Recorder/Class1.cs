//VCenterV5_1_0Recorder
using System;
using System.Globalization;
using System.Timers;
using CustomTools;
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

    public class VCenterV5_1_0Recorder : Parser
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private Fields RecordFields;

        public VCenterV5_1_0Recorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {

        }

        public override void Init()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += this.timer1_Tick;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on VCenterV5_1_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on VCenterV5_1_0Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating VCenterV5_1_0Recorder DAL");

                        reg_flag = true;
                        db_name = location;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "DBName is: " + db_name);

                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host.Trim() + ",1433", user, password, location))
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create VCenterV5_1_0Recorder DAL");
                        }
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating VCenterV5_1_0Recorder DAL");
                        }
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager VCenterV5_1_0Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\VCenterV5_1_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager VCenterV5_1_0Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            //wsdb_name_last = LastPosition;
            Zone = Zone;
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\VCenterV5_1_0Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("WebsenseRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("Interval"));
                //wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("VCenterV5_1_0Recorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager VCenterV5_1_0Recorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                readQuery = "select MAX(EVENT_ID) as 'ID' FROM dbo.VPX_EVENT";
                L.Log(LogType.FILE, LogLevel.INFORM, " Query is " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish executing the query");
                while (readReader.Read())
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Set last position:  Position: " + readReader["ID"]);
                    last_recordnum = Convert_To_Int64(readReader["ID"].ToString());
                    L.Log(LogType.FILE, LogLevel.INFORM, "Set_LastPosition-->  Position is: " + Position);
                    //RecordFields.currentPosition = last_recordnum;

                    if (usingRegistry)
                        Set_Registry(last_recordnum);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnum.ToString(), "", "", "", "");
                        L.Log(LogType.FILE, LogLevel.INFORM, " Position Updated.");
                    }
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on VCenterV5_1_0Recorder Recorder functions may not be running");
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

                readQuery = "SELECT TOP " + max_record_send + "  EVENT_ID, EVENT_TYPE, CREATE_TIME, USERNAME, CATEGORY, VM_ID, VM_NAME, " +
                            " HOST_ID ,  HOST_NAME ,  COMPUTERESOURCE_ID ,  COMPUTERESOURCE_NAME ,  DATACENTER_ID ,  DATACENTER_NAME ,  DATASTORE_ID , " +
                            " DATASTORE_NAME  FROM dbo.VPX_EVENT where EVENT_ID > " + last_recordnum + " order by EVENT_ID Asc";

                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick Position is 0 ");
                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick Position: " + Position);
                L.Log(LogType.FILE, LogLevel.INFORM, "TimerTick RecordFields.currentPosition is: " + RecordFields.currentPosition);
          

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
                    rec.LogName = "VCenterV5_1_0Recorder";
                    L.Log(LogType.FILE, LogLevel.DEBUG, " db_name _ 2");

                    DateTime dtNow;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "tarih: " + readReader["CREATE_TIME"].ToString());
                    dtNow = Convert.ToDateTime(Convert_To_String(Convert_To_String(readReader["CREATE_TIME"])));//
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Date Time : " + dtNow);
                    rec.Datetime = dtNow.ToString("yyyy-MM-dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Date Time : " + rec.Datetime);

                    try
                    {
                        rec.EventId = Convert.ToInt64(readReader["EVENT_ID"].ToString());//
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventId: " + rec.EventId);
                    }
                    catch (Exception exception)
                    {

                        L.Log(LogType.FILE, LogLevel.ERROR, "EventId: Error: " + exception.Message);
                    }

                    if (!string.IsNullOrEmpty(readReader["CATEGORY"].ToString()))
                    {
                        rec.EventCategory = readReader["CATEGORY"].ToString();//
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventId: " + rec.EventId);
                    }

                    if (!string.IsNullOrEmpty(readReader["EVENT_TYPE"].ToString()))
                    {
                        rec.EventType = readReader["EVENT_TYPE"].ToString();//   
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    }

                    if (!string.IsNullOrEmpty(Convert_To_String(readReader["USERNAME"])))
                    {
                        rec.UserName = readReader["USERNAME"].ToString();//    
                        L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                    }

                    rec.SourceName = location;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);


                    if (!string.IsNullOrEmpty(readReader["VM_NAME"].ToString()))
                    {
                        rec.CustomStr1 = readReader["VM_NAME"].ToString();//
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    }
                    if (!string.IsNullOrEmpty(readReader["HOST_NAME"].ToString()))
                    {
                        rec.CustomStr2 = readReader["HOST_NAME"].ToString();//
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                    }

                    //if (!string.IsNullOrEmpty(readReader["COMPUTERESOURCE_NAME"].ToString()))
                    //{
                    //    rec.CustomStr3 = readReader["COMPUTERESOURCE_NAME"].ToString();//
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    //}
                    //if (!string.IsNullOrEmpty(readReader["DATACENTER_NAME"].ToString()))
                    //{
                    //    rec.CustomStr4 = readReader["DATACENTER_NAME"].ToString();//
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                    //}
                    //if (!string.IsNullOrEmpty(readReader["DATASTORE_NAME"].ToString()))
                    //{
                    //    rec.CustomStr5 = readReader["DATASTORE_NAME"].ToString();//
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                    //}

                    //try
                    //{
                    //    if (!string.IsNullOrEmpty(readReader["VM_ID"].ToString()))
                    //    {
                    //        rec.CustomInt1 = Convert.ToInt32(readReader["VM_ID"].ToString());//
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{

                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1: Error: " + exception.Message);
                    //}

                    //try
                    //{
                    //    if (!string.IsNullOrEmpty(readReader["HOST_ID"].ToString()))
                    //        rec.CustomInt2 = Convert.ToInt32(readReader["HOST_ID"].ToString());//
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2: Error: " + exception.Message);
                    //}

                    //try
                    //{
                    //    if (!string.IsNullOrEmpty(readReader["COMPUTERESOURCE_ID"].ToString()))
                    //    {
                    //        rec.CustomInt3 = Convert.ToInt32(readReader["COMPUTERESOURCE_ID"].ToString());//
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3: Error: " + exception.Message);
                    //}

                    //try
                    //{
                    //    if (!string.IsNullOrEmpty(readReader["DATACENTER_ID"].ToString()))
                    //    {
                    //        rec.CustomInt4 = Convert.ToInt32(readReader["DATACENTER_ID"].ToString());//
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4: Error: " + exception.Message);
                    //}

                    //try
                    //{
                    //    if (!string.IsNullOrEmpty(readReader["DATASTORE_ID"].ToString()))
                    //    {
                    //        rec.CustomInt5 = Convert.ToInt32(readReader["DATASTORE_ID"].ToString());//
                    //        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{

                    //    L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5: Error: " + exception.Message);
                    //}

                    //RecordFields.ID = Convert_To_Int32(Convert_To_String(readReader["ID"]));//


                    //L.Log(LogType.FILE, LogLevel.DEBUG, "EventId: " + rec.EventId);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);

                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);

                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2.ToString(CultureInfo.InvariantCulture));
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3.ToString(CultureInfo.InvariantCulture));
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4.ToString(CultureInfo.InvariantCulture));
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5.ToString(CultureInfo.InvariantCulture));

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
                    //RecordFields.Counter++;
                    //L.Log(LogType.FILE, LogLevel.INFORM, "Counter: " + RecordFields.Counter);

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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("VCenterV5_1_0Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager VCenterV5_1_0Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager VCenterV5_1_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}


