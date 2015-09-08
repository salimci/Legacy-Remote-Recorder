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
using System.Data.SqlClient;

namespace SharePointRecorder
{
    public class SharePointRecorder : CustomBase
    {

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "Admin_Log", wsdb_name_last = "Admin_Log", db_name, location, user, password, remote_host = "", table_name = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private Int32 lineCount = 0;
        public SharePointRecorder()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
        }

        public override void Init()
        {
            //Useing build dal for db connection information and use natek database.dll to connect db.

            //Database.AddProviderToRegister(Database.Provider.Oracle, "DalDeneme", "txtHost.Text", "txtDB.Text", "txtUser.Text", "natekadmin");

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SharePoint Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SharePoint Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating SharePoint DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();

                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SharePoint Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SharePointRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SharePoint Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            location = Location;//Database Name
            //last_recordnum = Convert.ToInt64(LastPosition); //Datetime as last record.
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;
            timer_interval = SleepTime; //Timer intarval. 
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            wsdb_name = Location;
            LastRecordDate = LastPosition;
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;

            //DAL
            //if (string.IsNullOrEmpty(Dal))
            //    db_name = "SPDAL";
            //else
            //    db_name = Dal;

            //Database.AddProviderToRegister(Database.Provider.SQLServer, db_name, remote_host, location, user, password);

        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SharePointRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("SharePointRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SQLServer Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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

            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Service Started");
            //string readQuery = null;
            SqlDataReader readReader = null;
            DbCommand cmd = null;
            int dataCnt = 0;
            try
            {
                //DAL
                //IDataReader readReader = Database.ExecuteReader(db_name,ref cmd);

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Before executing the query, max_record_send : " + max_record_send + " LastRecordDate : " + LastRecordDate);
                readReader = Get_Sql_Data(remote_host, location, user, password, max_record_send, LastRecordDate);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Finish executing the query");

                while (readReader.Read())
                {
                    SaveData(readReader);
                    //    dataCnt++;
                    //    lineCount++;
                    //    lineCount2++;
                    //    if (DateTime.Equals(LastRecordDate, readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss")))
                    //    {
                    //        if (lineCount > 4 && !continueRead)
                    //        {
                    //            queryMultiplier++;
                    //            max_record_send_temp *= queryMultiplier;
                    //            readReader = Get_Sql_Data(remote_host, location, user, password, max_record_send_temp, LastRecordDate);
                    //            lineCountTemp = lineCount;
                    //            lineCount2 = 0;
                    //            continueRead = true;
                    //            continue;
                    //        }
                    //        if (max_record_send_temp == max_record_send)
                    //        {
                    //            SaveData(readReader);
                    //        }
                    //        else
                    //        {
                    //            if (lineCount2 >= lineCountTemp)
                    //            {
                    //                SaveData(readReader);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        SaveData(readReader);
                    //    }

                }
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Total get data amount : " + dataCnt);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Finish getting the data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Service Stopped");
                Database.Drop(ref cmd);
            }
        }

        private void SaveData(SqlDataReader readReader)
        {
            string errorMsg = "";
            // A.SiteId, customstr5
            //A.ItemId customstr4
            //,a.MachineName,computername
            //A.MachineIp,customstr3
            //a.DocLocation,customstr10
            //a.Occurred,datetime
            //a.LocationType, customint1
            //b.tp_Login, customstr1
            //b.tp_Title, usersID
            //b.tp_Email, customstr2
            //Event eventType
            CustomBase.Rec rec = new CustomBase.Rec();

            rec.LogName = "SharePoint Recorder";
            rec.CustomStr5 = readReader.GetGuid(0).ToString();
            rec.CustomStr4 = readReader.GetGuid(1).ToString();

            try
            {
                rec.ComputerName = readReader.GetString(2);
            }
            catch (Exception ex)
            {
                errorMsg += " | readReader.GetString(2) = " + readReader[2].ToString(); rec.ComputerName = "";
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->>  : " + ex.ToString() + "  " + rec.ComputerName);
            }
            try
            {
                rec.CustomStr3 = readReader.GetString(3);
            }
            catch (Exception ex)
            {
                errorMsg += " | readReader.GetString(3) = " + readReader[3].ToString(); rec.CustomStr3 = "";
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> : " + ex.ToString() + "  " + rec.CustomStr3);
            }
            try
            {
                rec.CustomStr10 = readReader.GetString(4);
            }
            catch (Exception ex) { errorMsg += " | readReader.GetString(4) = " + readReader[4].ToString(); rec.CustomStr10 = ""; }
            try
            {
                rec.Datetime = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> DateTime : " + rec.Datetime.ToString());
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> There is a dateTime Error : " + ex.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> readReader[5] : " + readReader[5].ToString());
                goto End;
            }
            try
            {
                rec.CustomInt1 = readReader.GetInt32(6);
            }
            catch (Exception ex)
            {
                errorMsg += " | readReader.GetString(6) = " + readReader[6].ToString(); rec.CustomInt1 = 0;
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> : " + ex.ToString() + "  " + rec.CustomInt1.ToString());
            }
            try
            {
                rec.CustomStr1 = readReader.GetString(7);
            }
            catch (Exception ex) { errorMsg += " | readReader.GetString(7) = " + readReader[7].ToString(); rec.CustomStr1 = ""; }
            try
            {
                rec.UserName = readReader.GetString(8);
            }
            catch (Exception ex) { errorMsg += " | readReader.GetString(8) = " + readReader[8].ToString(); rec.UserName = ""; }
            try
            {
                rec.CustomStr2 = readReader.GetString(9);
            }
            catch (Exception ex) { errorMsg += " | readReader.GetString(9) = " + readReader[9].ToString(); rec.CustomStr2 = ""; }
            try
            {
                rec.EventType = readReader.GetString(10);
            }
            catch (Exception ex) { errorMsg += " | readReader.GetString(10) = " + readReader[10].ToString(); rec.EventType = ""; }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Karşılaşılan Kolon Hataları : " + errorMsg);
            }

            errorMsg = "";

            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Start sending Data");

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

            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Finish Sending Data");
            LastRecordDate = rec.Datetime;
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Last Position is setted. Last rec date : " + LastRecordDate.ToString());

            //SetConfigData(Id, location, "", LastRecordDate, "", "", false, max_record_send, user, password, remote_host, sleeptime, trc_level, LastRecordDate, 0, virtualhost, Dal, zone);

            if (usingRegistry)
                Set_Registry(last_recordnum);
            else
            {
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetReg(Id, LastRecordDate, "", "", "", LastRecordDate);
            }

        End: ;

        }

        private SqlDataReader Get_Sql_Data(string Remote_Host, string DbName, string UserName, string Password, int SelectAmount, string lastRecordeDate)
        {
            string connStr = "Data Source=" + Remote_Host + ",1433;Network Library=DBMSSOCN;Initial Catalog=" + DbName + ";User ID=" + UserName + ";Password=" + Password + ";";
            SqlDataReader reader = null;
            string readQuery = null;
            try
            {
                readQuery = "SELECT top " + SelectAmount.ToString() +
                            " A.SiteId,A.ItemId,A.MachineName,A.MachineIp,A.DocLocation,A.Occurred,A.LocationType,B.tp_Login,B.tp_Title,B.tp_Email," +
                            " case [Event] when 1 then 'CheckOut' when 2 then 'CheckIn' when 3 then 'View' when 4 then 'Delete' when 5 then 'Update' when 6 then 'ProfileChange' when 7 then 'ChildDelete' when 8 then 'SchemaChange' when 9 then 'Undelete' when 10 then 'Workflow' when 11 then 'Copy' when 12 then 'Move' when 13 then 'AuditMaskChange' when 14 then 'Search' when 15 then 'ChildMove' when 16 then 'FileFragmentWrite' when 17 then 'SecGroupCreate' when 18 then 'SecGroupDelete' when 19 then 'SecGroupMemberAdd' when 20 then 'SecGroupMemberDel' when 21 then 'SecRoleDefCreate' when 22 then 'SecRoleDefDelete' when 23 then 'SecRoleDefModify' when 24 then 'SecRoleDefBreakInherit' when 25 then 'SecRoleBindUpdate' when 26 then 'SecRoleBindInherit' when 27 then 'SecRoleBindBreakInherit' when 28 then 'EventsDeleted' when 29 then 'Custom'" +
                            " end as EventName " +
                            " FROM AuditData A join UserInfo B " +
                            " on A.UserId=B.tp_ID " +
                            " AND A.Occurred BETWEEN CONVERT(VARCHAR(23), '" + LastRecordDate + "',126)" +
                            " AND CONVERT(VARCHAR(23), A.Occurred, 126) ORDER BY A.Occurred ASC";
                L.Log(LogType.FILE, LogLevel.DEBUG, " Get_Sql_Data() -->> readQuery : " + readQuery);

                SqlConnection conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand command = new SqlCommand();
                command.CommandText = readQuery;
                command.CommandTimeout = 5000;
                command.CommandType = CommandType.Text;
                command.Connection = conn;
                reader = command.ExecuteReader();
                L.Log(LogType.FILE, LogLevel.DEBUG, " Get_Sql_Data() -->> Query Executed.");
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Get_Sql_Data() -->> Error : " + ex.ToString());
            }

            return reader;
        }

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("SharePointRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SQLServer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager SQLServer Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
