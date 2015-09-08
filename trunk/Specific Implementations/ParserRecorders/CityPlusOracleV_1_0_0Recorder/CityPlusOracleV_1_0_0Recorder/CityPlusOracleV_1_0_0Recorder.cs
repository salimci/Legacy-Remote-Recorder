//CityPlusOracleV_1_0_0Recorder

//Name: CityPlusOracleV_1_0_0Recorder
//Writer: Onur Sarıkaya
//Date: 27.06.2013

using System;
using System.Collections.Generic;
using System.Globalization;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;

namespace CityPlusOracleV_1_0_0Recorder
{
    public struct Fields
    {
        public long currentPosition;
        public long rowCount;
    }

    public class CityPlusOracleV_1_0_0Recorder : CustomBase
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
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        private Fields RecordFields;

        public CityPlusOracleV_1_0_0Recorder()
        {
            InitializeComponent();
            RecordFields = new Fields();
        }

        private void FillTypesDictionary()
        {
            types.Add(0, "UNKNOWN");
            types.Add(1, "CREATE");
            types.Add(2, "INSERT");
            types.Add(3, "SELECT");
            types.Add(4, "CREATE");
            types.Add(5, "ALTER");
            types.Add(6, "UPDATE");
            types.Add(7, "DELETE");
            types.Add(8, "DROP");
            types.Add(9, "CREATE");
            types.Add(10, "DROP");
            types.Add(11, "ALTER");
            types.Add(12, "DROP");
            types.Add(13, "CREATE");
            types.Add(14, "ALTER");
            types.Add(15, "ALTER");
            types.Add(16, "DROP");
            types.Add(17, "GRANT");
            types.Add(18, "REVOKE");
            types.Add(19, "CREATE");
            types.Add(20, "DROP");
            types.Add(21, "CREATE");
            types.Add(22, "DROP");
            types.Add(23, "VALIDATE");
            types.Add(24, "CREATE");
            types.Add(25, "ALTER");
            types.Add(26, "LOCK");
            types.Add(27, "NO-OP");
            types.Add(28, "RENAME");
            types.Add(29, "COMMENT");
            types.Add(30, "AUDIT");
            types.Add(31, "NOAUDIT");
            types.Add(32, "CREATE");
            types.Add(33, "DROP");
            types.Add(34, "CREATE");
            types.Add(35, "ALTER");
            types.Add(36, "CREATE");
            types.Add(37, "ALTER");
            types.Add(38, "DROP");
            types.Add(39, "CREATE");
            types.Add(40, "ALTER");
            types.Add(41, "DROP");
            types.Add(42, "ALTER");
            types.Add(43, "ALTER");
            types.Add(44, "COMMIT");
            types.Add(45, "ROLLBACK");
            types.Add(46, "SAVEPOINT");
            types.Add(47, "PL/SQL");
            types.Add(48, "SET");
            types.Add(49, "ALTER");
            types.Add(50, "EXPLAIN");
            types.Add(51, "CREATE");
            types.Add(52, "CREATE");
            types.Add(53, "DROP");
            types.Add(54, "DROP");
            types.Add(55, "SET");
            types.Add(56, "CREATE");
            types.Add(57, "CREATE");
            types.Add(59, "CREATE");
            types.Add(60, "ALTER");
            types.Add(61, "DROP");
            types.Add(62, "ANALYZE");
            types.Add(63, "ANALYZE");
            types.Add(64, "ANALYZE");
            types.Add(65, "CREATE");
            types.Add(66, "DROP");
            types.Add(67, "ALTER");
            types.Add(68, "DROP");
            types.Add(70, "ALTER");
            types.Add(71, "CREATE");
            types.Add(72, "ALTER");
            types.Add(73, "DROP");
            types.Add(74, "CREATE");
            types.Add(75, "ALTER");
            types.Add(76, "DROP");
            types.Add(77, "CREATE");
            types.Add(78, "DROP");
            types.Add(79, "ALTER");
            types.Add(80, "ALTER");
            types.Add(81, "CREATE");
            types.Add(82, "ALTER");
            types.Add(83, "DROP");
            types.Add(84, "DROP");
            types.Add(85, "TRUNCATE");
            types.Add(86, "TRUNCATE");
            types.Add(91, "CREATE");
            types.Add(92, "ALTER");
            types.Add(93, "DROP");
            types.Add(94, "CREATE");
            types.Add(95, "ALTER");
            types.Add(96, "DROP");
            types.Add(97, "CREATE");
            types.Add(98, "ALTER");
            types.Add(99, "DROP");
            types.Add(100, "LOGON");
            types.Add(101, "LOGOFF");
            types.Add(102, "LOGOFF");
            types.Add(103, "SESSION");
            types.Add(104, "SYSTEM");
            types.Add(105, "SYSTEM");
            types.Add(106, "AUDIT");
            types.Add(107, "NOAUDIT");
            types.Add(108, "SYSTEM");
            types.Add(109, "SYSTEM");
            types.Add(110, "CREATE");
            types.Add(111, "DROP");
            types.Add(112, "CREATE");
            types.Add(113, "DROP");
            types.Add(114, "GRANT");
            types.Add(115, "REVOKE");
            types.Add(116, "EXECUTE");
            types.Add(117, "USER");
            types.Add(118, "ENABLE");
            types.Add(119, "DISABLE");
            types.Add(120, "ENABLE");
            types.Add(121, "DISABLE");
            types.Add(122, "NETWORK");
            types.Add(123, "EXECUTE");
            types.Add(128, "FLASHBACK");
            types.Add(129, "CREATE");
            types.Add(157, "CREATE");
            types.Add(158, "DROP");
            types.Add(159, "CREATE");
            types.Add(160, "CREATE");
            types.Add(161, "ALTER");
            types.Add(162, "DROP");
            types.Add(163, "CREATE");
            types.Add(164, "CREATE");
            types.Add(165, "DROP");
            types.Add(167, "DROP");
            types.Add(168, "ASSOCIATE");
            types.Add(169, "DISASSOCIATE");
            types.Add(170, "CALL");
            types.Add(171, "CREATE");
            types.Add(172, "ALTER");
            types.Add(173, "DROP");
            types.Add(174, "CREATE");
            types.Add(175, "ALTER");
            types.Add(176, "DROP");
            types.Add(177, "CREATE");
            types.Add(178, "DROP");
            types.Add(179, "ALTER");
            types.Add(180, "CREATE");
            types.Add(181, "DROP");
            types.Add(182, "UPDATE");
            types.Add(183, "ALTER");
            types.Add(197, "PURGE");
            types.Add(198, "PURGE");
            types.Add(199, "PURGE");
            types.Add(200, "PURGE");
            types.Add(201, "PURGE");
            types.Add(202, "UNDROP");
            types.Add(204, "FLASHBACK");
            types.Add(205, "FLASHBACK");
            types.Add(206, "CREATE");
            types.Add(207, "DROP");
            types.Add(208, "PROXY");
            types.Add(209, "DECLARE");
            types.Add(210, "ALTER");
            types.Add(211, "DROP");
        }

        private void InitializeComponent()
        {
        }

        public override void Init()
        {
            try
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
                timer1.Interval = timer_interval;
                timer1.Enabled = true;
                FillTypesDictionary();
            }
            catch (Exception ex)
            {

            }
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CityPlusOracleV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CityPlusOracleV_1_0_0Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating CityPlusOracleV_1_0_0Recorder DAL");
                        reg_flag = true;
                        Database.CreateDatabase();
                        db_name = location;
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CityPlusOracleV_1_0_0Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager CityPlusOracleV_1_0_0Recorder Recorder Init. " + er.Message);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CityPlusOracleV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CityPlusOracleV_1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager CityPlusOracleV_1_0_0Recorder Read Registry Error. " + er.Message);
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
            db_name = Location;//Database name
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;//Data amount to get per tick
            timer_interval = SleepTime; //Timer interval.
            user = User; //User name
            password = Password; //password
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CityPlusOracleV_1_0_0Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("CityPlusOracleV_1_0_0Recorder").GetValue("LastRecordNum"));

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Read_Registry -->> Error : " + er.Message);
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

        public long GetMaxId(string idColumnName, string tableName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Started GetMaxId.");
            string readQuery = "";
            long maxId = 0;
            IDataReader readReader = null;
            DbCommand cmd = null;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> Start executing the query");
                //readQuery = "Select MAX(LOGTRANS.LOGTRANS_ID) as ID From LOGTRANS";
                readQuery = "Select MAX(" + idColumnName + ") as ID From " + tableName;
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> readQuery: " + readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> db_name: " + db_name);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> readReader sonrası.");

                cmd.CommandTimeout = 2000;
                readReader.Read();
                maxId = Convert_To_Int64(readReader[0].ToString());
                readReader.Close();
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> Error on GetMaxId: " + exception.Message);
                L.Log(LogType.FILE, LogLevel.DEBUG, "GetMaxId -->> Error on GetMaxId: " + exception.StackTrace);
                maxId = 0;
            }
            return maxId;
        } // GetMaxId

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            string readQuery = "";
            long datacount = 0;
            int actionType = 0;
            timer1.Enabled = false;
            Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
            IDataReader readReader = null;
            DbCommand cmd = null;

            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Dal: " + Dal);
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> virtualhost: " + virtualhost);

            try
            {
                datacount = GetMaxId("LOGTRANS.LOGTRANS_ID", "BEYAZ.LOGTRANS");
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> datacount" + datacount);

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Total data count in LOGTRANS table : " + datacount.ToString(CultureInfo.InvariantCulture));
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Last data number read from LOGTRANS table is : " + last_recordnum.ToString(CultureInfo.InvariantCulture));

                if (datacount < last_recordnum)
                {
                    last_recordnum = 0;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> LOGTRANS table is truncated. Started to read at the beginning.");
                }

                if (RecordFields.rowCount != 0)
                {
                    RecordFields.currentPosition = last_recordnum + max_record_send;
                }

                //if (last_recordnum == 0)
                //{
                //    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> is Last Pozision == 0 ");
                //    readQuery = "select KO_ID, SICILNO, KAPI, ADISOYADI, TARIH, HAREKET, " +
                //                "SAAT from USR_PRO01_2013.KARTOKUTMA " +
                //                "WHERE KO_ID > 0 and KO_ID <= " + RecordFields.currentPosition + " ORDER BY KO_ID ASC";
                //}
                //else
                //{
                //    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> is Last Pozision != 0 ");
                //    readQuery = "select KO_ID, SICILNO, KAPI, ADISOYADI, TARIH, HAREKET, " +
                //                "SAAT from USR_PRO01_2013.KARTOKUTMA  " +
                //                "WHERE KO_ID > " + last_recordnum + " and KO_ID <= " + RecordFields.currentPosition + " ORDER BY KO_ID ASC";
                //}

                readQuery =
                    "SELECT BEYAZ.LOGSESSION.LOGSESSION_ID, BEYAZ.LOGSESSION.LOGSESSION_USERS, " +
                    "BEYAZ.USERS.USERS_NAME, BEYAZ.USERS.USERS_FULLNAME, BEYAZ.LOGSESSION.LOGSESSION_DATE, " +
                    "BEYAZ.LOGSESSION.LOGSESSION_IP as sessionIp, BEYAZ.LOGSESSION.LOGSESSION_LOCATION, BEYAZ.LOGTRANS.LOGTRANS_ID, " +
                    "BEYAZ.LOGTRANS.LOGTRANS_DATE, BEYAZ.LOGTRANS.LOGTRANS_ELAPSED, BEYAZ.LOGTRANS.LOGTRANS_CPSNAME, " +
                    "BEYAZ.LOGTRANS.LOGTRANS_STATUS, BEYAZ.LOGTRANS.LOGTRANS_STATUSTEXT, BEYAZ.LOGTABLE.LOGTABLE_ID, " +
                    "BEYAZ.LOGTABLE.LOGTABLE_SCHEMA, BEYAZ.LOGTABLE.LOGTABLE_TABLE, " +
                    "BEYAZ.LOGTABLE.LOGTABLE_PKEY, BEYAZ.LOGTABLE.LOGTABLE_OPERATION, " +
                    "BEYAZ.LOGFIELD.LOGFIELD_ID, BEYAZ.LOGFIELD.LOGFIELD_FIELD, BEYAZ.LOGFIELD.LOGFIELD_OLD, " +
                    "BEYAZ.LOGFIELD.LOGFIELD_NEW " +
                    "FROM BEYAZ.LOGSESSION " +
                    "LEFT OUTER JOIN BEYAZ.USERS ON LOGSESSION.LOGSESSION_USERS = USERS.USERS_ID " +
                    "LEFT OUTER JOIN BEYAZ.LOGTRANS ON LOGTRANS.LOGTRANS_LOGSESSION = LOGSESSION.LOGSESSION_ID " +
                    "LEFT OUTER JOIN BEYAZ.LOGTABLE ON LOGTABLE.LOGTABLE_LOGTRANS = LOGTRANS.LOGTRANS_ID " +
                    "LEFT OUTER JOIN BEYAZ.LOGFIELD ON LOGFIELD.LOGFIELD_LOGTABLE = LOGTABLE.LOGTABLE_ID " +
                    "WHERE " + last_recordnum + " <= BEYAZ.LOGTRANS.LOGTRANS_ID AND BEYAZ.LOGTRANS.LOGTRANS_ID < " + RecordFields.currentPosition +
                    " ORDER BY BEYAZ.LOGTRANS.LOGTRANS_ID";

                cmd = null;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readQuery : " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 2000;

                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Finish executing the query. Query : " + readQuery);

                while (readReader.Read())
                {
                    rec.LogName = "CityPlusOracleV_1_0_0Recorder";

                    string test = Convert_To_String(readReader[0]);


                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> LogName: " + rec.LogName);
                    try
                    {
                        string date = Convert_To_String(readReader["LOGTRANS_DATE"]);
                        DateTime dt1 = new DateTime();
                        dt1 = Convert.ToDateTime(date.Split(',')[0]);
                        rec.Datetime = dt1.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Date_Time Error: " + exception.ToString());
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Date_Time Error: " + Convert_To_String(readReader[8]));
                    }

                    /*  try
                      {
                          rec.EventId = Convert_To_Int32(readReader["LOGSESSION.LOGSESSION_ID"].ToString());
                          L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> EventId: " + rec.EventId);
                      }
                      catch (Exception exception)
                      {
                          L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> EventId Type Casting Error: " + exception.ToString());
                          L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> EventId Type Casting Error: " + readReader["LOGSESSION.LOGSESSION_ID"].ToString());
                      }

                      try
                      {
                          rec.UserName = Convert_To_String(readReader["USERS.USERS_NAME"]);
                          L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> UserName: " + rec.UserName);
                      }
                      catch (Exception exception)
                      {
                          L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr3: " + exception.ToString());
                      }
                      */

                    rec.EventId = GetIntValue("LOGSESSION_ID", "EventId", readReader);//
                    rec.Recordnum = GetIntValue("LOGTRANS_ID", "EventId", readReader);//
                    //rec.CustomInt1 = GetIntValue("LOGSESSION.LOGSESSION_USERS", "CustomInt1", readReader);
                    //rec.CustomInt2 = GetIntValue("LOGSESSION.LOGSESSION_ID", "CustomInt2", readReader);
                    //rec.CustomInt3 = GetIntValue("LOGSESSION.LOGSESSION_ID", "CustomInt3", readReader);
                    rec.CustomInt4 = GetIntValue("LOGTRANS_ELAPSED", "CustomInt4", readReader);//
                    rec.CustomInt5 = GetIntValue("LOGTRANS_STATUS", "CustomInt5", readReader);//
                    rec.CustomInt6 = GetIntValue("LOGTABLE_ID", "CustomInt6", readReader);//
                    rec.CustomInt7 = GetIntValue("LOGFIELD_ID", "CustomInt7", readReader);//
                    //rec.CustomInt8 = GetIntValue("LOGSESSION.LOGSESSION_ID", "CustomInt8", readReader);
                    //rec.CustomInt9 = GetIntValue("LOGSESSION.LOGSESSION_ID", "CustomInt9", readReader);
                    //rec.CustomInt10 = GetIntValue("LOGSESSION.LOGSESSION_ID", "CustomInt10", readReader);

                    rec.UserName = GetStringValue("USERS_NAME", "UserName", readReader);//
                    rec.ComputerName = GetStringValue("USERS_FULLNAME", "ComputerName", readReader);//
                    rec.CustomStr1 = GetStringValue("LOGSESSION_DATE", "CustomStr1", readReader);//
                    rec.CustomStr2 = GetStringValue("sessionIp", "CustomStr2", readReader);//
                    rec.CustomStr3 = GetStringValue("LOGSESSION_LOCATION", "CustomStr3", readReader);
                    rec.CustomStr4 = GetStringValue("LOGTRANS_CPSNAME", "CustomStr4", readReader);//
                    rec.CustomStr5 = GetStringValue("LOGTRANS_STATUSTEXT", "CustomStr5", readReader);//
                    rec.CustomStr6 = GetStringValue("LOGTABLE_SCHEMA", "CustomStr6", readReader);//
                    rec.CustomStr7 = GetStringValue("LOGTABLE_TABLE", "CustomStr7", readReader);//
                    rec.CustomStr8 = GetStringValue("LOGTABLE_OPERATION", "CustomStr8", readReader);
                    rec.CustomStr9 = GetStringValue("LOGFIELD_FIELD", "CustomStr9", readReader);//
                    rec.EventCategory = GetStringValue("LOGFIELD_NEW", "EventCategory", readReader);//
                    
                    rec.EventType = GetStringValue("LOGFIELD_OLD", "EventType", readReader);//
                    rec.Description = GetStringValue("LOGTABLE_PKEY", "Description", readReader);//

                    if (!string.IsNullOrEmpty(rec.Datetime))
                    {
                        if (usingRegistry)
                        {
                            CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                            s.SetData(rec);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Sender -->> : ");
                        }
                        else
                        {
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetData(Dal, virtualhost, rec);

                            L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder -->> : ");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder DAL : -->> : " + Dal);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder  virtualhost : -->> : " + virtualhost);
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Finish Sending Data");

                        LastRecordDate = rec.Datetime;

                        if (usingRegistry)
                            Set_Registry(last_recordnum);
                        else
                        {
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetReg(Id, last_recordnum.ToString(), "", "", "", LastRecordDate);
                        }
                    }
                    RecordFields.rowCount++;
                }

                if (RecordFields.rowCount == 0 && datacount > last_recordnum)
                {
                    RecordFields.currentPosition = RecordFields.currentPosition + max_record_send;
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> (test1): " + RecordFields.currentPosition);
                }

                if (last_recordnum !=0)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> (test2): " + last_recordnum);
                    RecordFields.currentPosition = last_recordnum + max_record_send;
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> currentPosition: " + RecordFields.currentPosition);
                }

                last_recordnum = last_recordnum + RecordFields.rowCount;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> last_recordnum: " + last_recordnum);
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> datacount: " + datacount);
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> rowCount: " + RecordFields.rowCount);
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + er.ToString());
            }
            finally
            {
                readReader.Close();
                timer1.Enabled = true;
                Database.Drop(ref cmd);
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick


        public string GetStringValue(string objectName, string columnName, IDataReader reader)
        {
            string returnValue = "";
            try
            {
                returnValue = Convert_To_String(reader[objectName]);
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetStringValue -->> " + columnName + ": " + returnValue);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetStringValue -->> " + columnName + ": " + exception.ToString());
            }
            return returnValue;
        }//GetStringValue 

        public Int32 GetIntValue(string objectName, string columnName, IDataReader reader)
        {
            Int32 returnValue = 0;
            try
            {
                returnValue = Convert_To_Int32(reader[objectName].ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetIntValue -->> " + columnName + ": " + returnValue);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetIntValue -->> " + columnName + ": " + exception.ToString());
            }
            return returnValue;
        }//GetStringValue 

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

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("CityPlusOracleV_1_0_0Recorder");
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
