//TKIOracleKGSV_1_0_0Recorder

//Name: TKI OracleKGS Recorder
//Writer: Onur Sarıkaya
//Date: 25.01.2013

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

namespace TKIOracleKGSV_1_0_0Recorder
{
    public struct Fields
    {
        public long currentPosition;
        public long rowCount;
    }

    public class TKIOracleKGSV_1_0_0Recorder : CustomBase
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

        public TKIOracleKGSV_1_0_0Recorder()
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

                //Şimdi registery manual olarak giriliyor. Fakat otomatik olarak aşağıdaki gibi yapılmalı 
                //Database.AddProviderToRegister(Database.Provider.Oracle, Dal, remote_host, db_name, user, password);

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TKIOracleKGSV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TKIOracleKGSV_1_0_0Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating TKIOracleKGSV_1_0_0Recorder DAL");

                        reg_flag = true;
                        Database.CreateDatabase();
                        db_name = location;

                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TKIOracleKGSV_1_0_0Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager TKIOracleKGSV_1_0_0Recorder Recorder Init. " + er.Message);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\TKIOracleKGSV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TKIOracleKGSV_1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager TKIOracleKGSV_1_0_0Recorder Read Registry Error. " + er.Message);
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
            //Dal = dal;
            //Dal = "OracleEnebisRecorder";
            //Dal = "Natekdb";//Database Name(Registiry Dal Name)
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;

            //user = "cryptt@testpre";
            //password = "abc123abc";
            //remote_host = "10.0.5.30";

            //L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> user: " + user);
            //L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> remote_host: " + remote_host);
            //L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> password: " + password);
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\TKIOracleKGSV_1_0_0Recorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("TKIOracleKGSV_1_0_0Recorder").GetValue("LastRecordNum"));

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
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Start executing the query");

                //readQuery = "select MAX(REC_ID) as REC_ID from natek.enbs_audit";
                //readQuery = "select MAX(REC_ID) as REC_ID from sys.aud$";
                //readQuery = "select COUNT(*) from tkb.ONAY_YETKI_VW";
                readQuery = "Select MAX(KO_ID) as ID From USR_PRO01_2013.KARTOKUTMA";
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readQuery : " + readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> db_name : " + db_name);

                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readReader sonrası.");

                cmd.CommandTimeout = 2000;
                readReader.Read();
                datacount = Convert_To_Int64(readReader[0].ToString());

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> datacount" + datacount);

                readReader.Close();
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Total data count in sys.enbs_audit table : " + datacount.ToString(CultureInfo.InvariantCulture));
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Last data number read from sys.enbs_audit table is : " + last_recordnum.ToString(CultureInfo.InvariantCulture));

                if (datacount < last_recordnum)
                {
                    last_recordnum = 0;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> sys.enbs_audit table is truncated. Started to read at the beginning.");
                }

                //long currentPosition = Convert.ToInt64(last_recordnum) + Convert.ToInt64(max_record_send);

                //Tablo sıfırlanmış ise yeni dataları okumak için başa dönmek gerekmektedir.
                //



                //if (last_recordnum == 0)
                //{
                //    RecordFields.currentPosition = max_record_send;
                //}
                //else
                //{
                //    RecordFields.currentPosition = last_recordnum + max_record_send;
                //}


                if (RecordFields.rowCount != 0)
                {
                    RecordFields.currentPosition = last_recordnum + max_record_send;
                }


                if (last_recordnum == 0)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> is Last Pozision == 0 ");
                    //readQuery = "SELECT REC_ID, SESSIONID,ENTRYID,STATEMENT,USERID,USERHOST,TERMINAL,ACTION#,RETURNCODE,OBJ$CREATOR,OBJ$NAME,CLIENTID,PRIV$USED,INSTANCE#,PROCESS#,AUDITID,SCN,SQLBIND,SQLTEXT,TO_CHAR(NTIMESTAMP#,'DD/MM/YYYY HH:MI:SS') as NTIMESTAMP# from sys.aud$ WHERE REC_ID >0 and REC_ID <= " + currentPosition + " ORDER BY REC_ID ASC";
                    //readQuery = "SELECT YETKI,ID,ADI_SOYADI from tkb.ONAY_YETKI_VW  WHERE ID >0 and ID <= " + currentPosition + " ORDER BY ID ASC";
                    readQuery = "select KO_ID, SICILNO, KAPI, ADISOYADI, TARIH, HAREKET, SAAT from USR_PRO01_2013.KARTOKUTMA WHERE KO_ID > 0 and KO_ID <= " + RecordFields.currentPosition + " ORDER BY KO_ID ASC";


                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> is Last Pozision != 0 ");
                    //readQuery = "SELECT REC_ID, SESSIONID,ENTRYID,STATEMENT,USERID,USERHOST,TERMINAL,ACTION#,RETURNCODE,OBJ$CREATOR,OBJ$NAME,CLIENTID,PRIV$USED,INSTANCE#,PROCESS#,AUDITID,SCN,SQLBIND,SQLTEXT,TO_CHAR(NTIMESTAMP#,'DD/MM/YYYY HH:MI:SS') as NTIMESTAMP# from sys.aud$ WHERE REC_ID > " + last_recordnum + " and REC_ID <= " + currentPosition + " ORDER BY REC_ID ASC";
                    readQuery = "select KO_ID, SICILNO, KAPI, ADISOYADI, TARIH, HAREKET, SAAT from USR_PRO01_2013.KARTOKUTMA  WHERE KO_ID > " + last_recordnum + " and KO_ID <= " + RecordFields.currentPosition + " ORDER BY KO_ID ASC";

                }

                //readQuery = "select * from(Select SESSIONID,ENTRYID,STATEMENT,USERID,USERHOST,TERMINAL,ACTION#,RETURNCODE,OBJ$CREATOR,OBJ$NAME,CLIENTID,PRIV$USED,INSTANCE#,PROCESS#,AUDITID,SCN,SQLBIND,SQLTEXT,TO_CHAR(NTIMESTAMP#,'DD/MM/YYYY HH:MI:SS') as NTIMESTAMP#, ROWNUM as DATANUM from sys.enbs_audit) where DATANUM >=" + last_recordnum + " and DATANUM <" + (last_recordnum + Convert_To_Int64(max_record_send.ToString())) + " ";//" ORDER BY NTIMESTAMP# ASC";

                cmd = null;

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readQuery : " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 2000;

                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Finish executing the query. Query : " + readQuery);

                
                while (readReader.Read())
                {
                    rec.LogName = "TKIOracleKGSV_1_0_0Recorder";

                    //DateTime dt;
                    //dt = DateTime.Now;
                    //rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");



                    try
                    {
                        rec.CustomStr3 = Convert_To_String(readReader["TARIH"]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr3: " + rec.CustomStr3);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr3: " + exception.ToString());
                    }

                    try
                    {
                        rec.CustomInt2 = Convert_To_Int32(readReader["SAAT"].ToString());
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt2: " + rec.CustomInt2);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt2: " + exception.ToString());
                    }

                    try
                    {

                        string date = rec.CustomStr3;//Convert_To_String(readReader["TARIH"]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> date: " + date);

                        DateTime d = new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)), Convert.ToInt32(date.Substring(6, 2)), 0, 0, 0);

                        string time = rec.CustomInt2.ToString();//readReader["SAAT"].ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> time: " + time);

                        rec.Datetime = d.AddMinutes(Convert.ToDouble(time)).ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Date_Time Error: " + exception.ToString());
                    }

                    if (!string.IsNullOrEmpty(rec.Datetime))
                    {
                        try
                        {

                            rec.UserName = Convert_To_String(readReader["ADISOYADI"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> UserName: " + rec.UserName);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> UserName: " + exception.ToString());
                        }

                        try
                        {
                            rec.CustomStr1 = Convert_To_String(readReader["KO_ID"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr1: " + rec.CustomStr1);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr1: " + exception.ToString());
                        }

                        try
                        {
                            rec.CustomStr2 = Convert_To_String(readReader["SICILNO"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr1: " + rec.CustomStr2);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr1: " + exception.ToString());
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(readReader["HAREKET"].ToString()))
                            {
                                rec.CustomInt1 = Convert_To_Int32(readReader["HAREKET"].ToString());
                                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt1: " + rec.CustomInt1);
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt1: " + exception.ToString());
                        }



                        //try
                        //{

                        //    if (!string.IsNullOrEmpty(readReader["NEDEN"].ToString()))
                        //    {
                        //        rec.CustomInt3 = Convert_To_Int32(readReader["NEDEN"].ToString());
                        //        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt3: " + rec.CustomInt3);
                        //    }
                        //}
                        //catch (Exception exception)
                        //{
                        //    L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt3: " + exception.ToString());
                        //}

                        try
                        {
                            rec.CustomInt4 = Convert_To_Int32(readReader["KAPI"].ToString());
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt4: " + rec.CustomInt4);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt4: " + exception.ToString());
                        }



                        //rec.Description = Convert_To_String(readReader["ID"]) + " " + Convert_To_String(readReader["ADI_SOYADI"]) + " " + Convert_To_String(readReader["YETKI"]);

                        last_recordnum = Convert_To_Int64(readReader["KO_ID"].ToString());
                        //

                        if (rec.CustomInt1 == 0)
                        {
                            rec.EventCategory = "LOGIN";
                        }
                        else if (rec.CustomInt1 == 1)
                        {
                            rec.EventCategory = "LOGOUT";
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Start sending Data. Last Record Number : " + last_recordnum);

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

                            /*
                             birdaha bir bugi oluştuğunda buraya rowCount==0 kontrolunu koy unutma
                             */

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
                }

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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("TKIOracleKGSV_1_0_0Recorder");
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
