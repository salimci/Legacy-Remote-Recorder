using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log;
using System.Data.SqlClient;
using CustomTools;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data;

namespace TKDK_KGSRecorder
{
    public class TKDK_KGSRecorder : CustomBase
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

        public TKDK_KGSRecorder()
        {
            InitializeComponent();
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
                EventLog.WriteEntry(" Initialize_Logger() -->> Security Manager SQLServer Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
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
                            L.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> Error on Initialize Logger on TKDK_KGS Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> Error on Intialize Logger on TKDK_KGS Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, " Init() -->> Start creating TKDK_KGS DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();

                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry(" Init() -->> Security Manager TKDK_KGS Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\TKDK_KGS" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TKDK_KGS Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            SqlDataReader sqlDataReader = null;
            Int32 dataCnt = 0;
            String errorMsg = "";

            timer1.Enabled = false;
            try
            {
                sqlDataReader = GetDataOnDataBase(remote_host, location, user, password, max_record_send, LastRecordDate);
                if (sqlDataReader != null)
                {
                    while (sqlDataReader.Read())
                    {
                        #region Data Kayıt Kolonları

                        //Logname: TKDK_KAPIGIRIS_RECORDER
                        //Computername: Logun toplandığı makinenin adı.
                        //Datetime:  GECIS.TARIH
                        //USERSID:  KISI.AD + “ “ + KISI.SOYAD
                        //EVENTTYPE: GECIS.GIRIS  ?? Bu alanda işlemin giriş veya çıkış olduğu tutulacak değil mi?
                        //EVENTCATEGORY: GECIS.NOKTA

                        //CUSTOMSTR1: KISI.AD 
                        //CUSTOMSTR2: KISI.SOYAD 
                        //CUSTOMSTR4: GECIS.TOMNO
                        //CUSTOMSTR5: GECIS.AKTARIM
                        //CUSTOMSTR6: GECIS.ZIYARETCI
                        //CUSTOMSTR7: GECIS.MANUEL
                        //CUSTOMSTR8: GECIS.SCANPC
                        #endregion
                        errorMsg = "";
                        try
                        {
                            rec.Datetime = sqlDataReader.GetDateTime(7).ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        catch (Exception ex)
                        {
                            errorMsg += " | sqlDataReader.GetDateTime(7) = " + sqlDataReader[7].ToString() + " for DateTime #|# ";
                            continue;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.Datetime is succesfully loaded in rec");
                        try { rec.LogName = "TKDK_KAPIGIRIS_RECORDER"; }
                        catch (Exception ex) { errorMsg += " | rec.LogName = "; }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.LogName is succesfully loaded in rec");

                        try
                        { rec.UserName = sqlDataReader.GetValue(0).ToString() + " " + sqlDataReader.GetValue(1).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetString(0) + sqlDataReader.GetString(1) #|#"; }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.UserName is succesfully loaded in rec");

                        try
                        { rec.ComputerName = remote_host; }
                        catch (Exception ex) { errorMsg += " | remote_host : " + rec.ComputerName.ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.ComputerName is succesfully loaded in rec");

                        try
                        { rec.EventCategory = sqlDataReader.GetInt16(8).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt16(8) :" + sqlDataReader.GetInt16(8).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.EventCategory is succesfully loaded in rec");

                        try
                        { rec.EventType = sqlDataReader.GetValue(9).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt16(9) #|# " + sqlDataReader.GetInt16(9).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.EventType is succesfully loaded in rec");

                        try
                        { rec.CustomStr1 = sqlDataReader.GetValue(0).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetString(0) #|# " + sqlDataReader.GetString(0); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr1 is succesfully loaded in rec");

                        try
                        { rec.CustomStr2 = sqlDataReader.GetValue(1).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetString(1) #|# " + sqlDataReader.GetString(1); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr2 is succesfully loaded in rec");

                        try { rec.CustomStr4 = sqlDataReader.GetValue(2).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetString(2) #|# " + sqlDataReader.GetString(2); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr4 is succesfully loaded in rec");

                        try { rec.CustomStr5 = sqlDataReader.GetInt16(3).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt16(3) #|# " + sqlDataReader.GetInt16(3).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr5 is succesfully loaded in rec");

                        try { rec.CustomStr6 = sqlDataReader.GetInt16(4).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt16(4) #|# " + sqlDataReader.GetInt16(4).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr6 is succesfully loaded in rec");

                        try { rec.CustomStr7 = sqlDataReader.GetInt16(5).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt16(5) #|# " + sqlDataReader.GetInt16(5).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr7 is succesfully loaded in rec");

                        try { rec.CustomStr8 = sqlDataReader.GetInt32(6).ToString(); }
                        catch (Exception ex) { errorMsg += " | sqlDataReader.GetInt32(6) #|# " + sqlDataReader.GetInt32(6).ToString(); }
                        L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> rec.CustomStr8 is succesfully loaded in rec");
                        dataCnt++;

                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() -->> Columns errors : " + errorMsg);
                        }

                        errorMsg = "";

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

                        LastRecordDate = rec.Datetime;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick() -->> Last Position is setted. Last rec date : " + LastRecordDate.ToString());

                        if (usingRegistry)
                            Set_Registry(last_recordnum);
                        else
                        {
                            CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                            s.SetReg(Id, LastRecordDate, "", "", "", LastRecordDate);
                        }
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> GetDataOnDataBase() function sended NULL Data : ");
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick() -->> An error occured : " + ex.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick() -->> is successfully FINISHED ");
            }
        }

        private SqlDataReader GetDataOnDataBase(String remoteHost, String dbName, String userName, String password, int maxRecordSend, String lastRecordDate)
        {
            String connStr = "Data Source=" + remoteHost + ";Initial Catalog=" + dbName + ";Persist Security Info=True;User ID=" + userName + ";Password=" + password + ";";
            SqlConnection sqlConn = null;
            SqlCommand sqlCommand = null;
            SqlDataReader sqlDataReader = null;
            String query = null;
            try
            {
                //Logname: TKDK_KAPIGIRIS_RECORDER
                //Computername: Logun toplandığı makinenin adı.
                //Datetime:  GECIS.TARIH
                //USERSID:  KISI.AD + “ “ + KISI.SOYAD
                //EVENTTYPE: GECIS.GIRIS  ?? Bu alanda işlemin giriş veya çıkış olduğu tutulacak değil mi?
                //EVENTCATEGORY: GECIS.NOKTA

                //CUSTOMSTR1: KISI.AD 
                //CUSTOMSTR2: KISI.SOYAD 
                //CUSTOMSTR4: GECIS.TOMNO
                //CUSTOMSTR5: GECIS.AKTARIM
                //CUSTOMSTR6: GECIS.ZIYARETCI
                //CUSTOMSTR7: GECIS.MANUEL
                //CUSTOMSTR8: GECIS.SCANPC
                if (lastRecordDate == "0")
                {
                    lastRecordDate = "1.1.2000";
                }
                
                query = " SELECT TOP " + maxRecordSend + " KISI.AD,KISI.SOYAD,GECIS.TOMNO,GECIS.AKTARIM,GECIS.ZIYARETCI," +
                        " GECIS.MANUEL,GECIS.SCANPC,GECIS.TARIH,GECIS.NOKTA,YON.YONU" +
                        " FROM GECIS INNER JOIN KISI ON KISI.KARTNO=GECIS.TOMNO " +
                        " INNER JOIN YON ON GECIS.GIRIS=YON.GIRIS " +
                        " AND GECIS.TARIH BETWEEN CONVERT(VARCHAR(23), '" + lastRecordDate + "',126)" +
                        " AND CONVERT(VARCHAR(23),GECIS.TARIH, 126) ORDER BY GECIS.TARIH ASC";
                sqlConn = new SqlConnection(connStr);
                L.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDataBase() -->> will execute query : " + query);
                sqlConn.Open();
                if (sqlConn.State == ConnectionState.Open)
                {
                    using (sqlCommand = new SqlCommand())
                    {
                        sqlCommand.CommandText = query;
                        sqlCommand.CommandTimeout = 5000;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.Connection = sqlConn;
                        sqlDataReader = sqlCommand.ExecuteReader();
                        L.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDataBase() -->> is successfully FINISHED ");
                        int rowCount = 0;
                        while (sqlDataReader.Read())
                        {
                            rowCount++;
                            if (rowCount==2)
                            {
                                return sqlDataReader;
                            }    
                        }
                        return null;
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDataBase() -->> NOT connected database ");
                    return null;
                }

            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetDataOnDataBase() -->> An error occured : " + ex.ToString());
                return null;
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\TKDK_KGSRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGS").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("Interval"));
                wsdb_name = rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("WSDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("TKDK_KGSRecorder").GetValue("LastRecordNum"));
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

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("TKDK_KGSRecorder");
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

    }
}
