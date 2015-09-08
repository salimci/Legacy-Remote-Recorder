//HMDEBYSOracleAuditv1_0_0Recorder

//Name: HMDEBYSOracleAuditv1_0_0Recorder
//Writer: Onur Sarıkaya
//Date: 04.10.2013

using System;
using System.Globalization;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;

namespace HMDEBYSOracleAuditv1_0_0Recorder
{
    public struct Fields
    {
        public long currentPosition;
        public long rowCount;
    }

    public class HMDEBYSOracleAuditv1_0_0Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trcLevel = 3, timerInterval = 3000, maxRecordSend = 100;
        private long lastRecordnum;
        private uint loggingInterval = 60000, logSize = 1000000;
        private string errLog, dbName, location, user, password, remoteHost = "";
        private bool regFlag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualHost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Fields RecordFields;

        public HMDEBYSOracleAuditv1_0_0Recorder()
        {
            InitializeComponent();
            RecordFields = new Fields();
        }

        private void InitializeComponent()
        {
        }

        public override void Init()
        {
            try
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += Timer1Tick;
                timer1.Interval = timerInterval;
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager HMDEBYSOracleAuditv1_0_0Recorder Recorder Init_1", ex.ToString(), EventLogEntryType.Error);
            }

            try
            {
                if (usingRegistry)
                {
                    if (!regFlag)
                    {
                        if (!ReadRegistry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                        {
                            if (!InitializeLogger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on HMDEBYSOracleAuditv1_0_0Recorder functions may not be running");
                                return;
                            }
                        }
                        regFlag = true;
                        Database.CreateDatabase();
                    }
                }
                else
                {
                    if (!regFlag)
                    {
                        if (!GetLogDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                        {
                            if (!InitializeLogger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on HMDEBYSOracleAuditv1_0_0Recorder Recorder  functions may not be running");
                                return;
                            }
                        }
                            
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating HMDEBYSOracleAuditv1_0_0Recorder DAL");

                        regFlag = true;
                        Database.CreateDatabase();
                        dbName = location;
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager HMDEBYSOracleAuditv1_0_0Recorder Recorder Init", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager HMDEBYSOracleAuditv1_0_0Recorder Recorder Init. " + er.Message);
            }
        }

        public bool GetLogDir()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Remote Recorder");
                    if (registryKey != null)
                        errLog = registryKey.GetValue("Home Directory") + @"log\HMDEBYSOracleAuditv1_0_0Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager HMDEBYSOracleAuditv1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager HMDEBYSOracleAuditv1_0_0Recorder Read Registry Error. " + er.Message);
                return false;
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
            dbName = Location;
            fromend = FromEndOnLoss;
            maxRecordSend = MaxLineToWait;
            timerInterval = SleepTime;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
            trcLevel = TraceLevel;
            virtualHost = Virtualhost;
            lastRecordnum = Convert_To_Int64(LastPosition);
            Dal = dal;
        }

        public bool ReadRegistry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry is Started");
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Recorder");
                    if (registryKey != null)
                    {
                        var key = registryKey.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (key != null)
                            logSize =
                                Convert.ToUInt32(
                                    key.GetValue("Log Size"));
                    }
                    var subKey = rk.OpenSubKey("Recorder");
                    if (subKey != null)
                    {
                        var openSubKey1 = subKey.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (openSubKey1 != null)
                            loggingInterval =
                                Convert.ToUInt32(
                                    openSubKey1.GetValue(
                                        "Logging Interval"));
                    }
                    var registryKey1 = rk.OpenSubKey("Recorder");
                    if (registryKey1 != null)
                    {
                        var key1 = registryKey1.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (key1 != null)
                            trcLevel =
                                Convert.ToInt32(
                                    key1.GetValue("Trace Level"));
                    }
                    var subKey1 = rk.OpenSubKey("Agent");
                    if (subKey1 != null)
                        errLog = subKey1.GetValue("Home Directory").ToString() +
                                 @"log\HMDEBYSOracleAuditv1_0_0Recorder.log";
                    var openSubKey2 = rk.OpenSubKey("Recorder");
                    if (openSubKey2 != null)
                    {
                        var subKey2 = openSubKey2.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (subKey2 != null)
                            dbName =
                                subKey2.GetValue("DBName").ToString
                                    ();
                    }
                    var registryKey2 = rk.OpenSubKey("Recorder");
                    if (registryKey2 != null)
                    {
                        var key2 = registryKey2.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (key2 != null)
                            timer1.Interval =
                                Convert.ToInt32(
                                    key2.GetValue("Interval"));
                    }
                    var openSubKey3 = rk.OpenSubKey("Recorder");
                    if (openSubKey3 != null)
                    {
                        var subKey3 = openSubKey3.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (subKey3 != null)
                            maxRecordSend =
                                Convert.ToInt32(
                                    subKey3.GetValue(
                                        "MaxRecordSend"));
                    }
                    var registryKey3 = rk.OpenSubKey("Recorder");
                    if (registryKey3 != null)
                    {
                        var key3 = registryKey3.OpenSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        if (key3 != null)
                            lastRecordnum =
                                Convert.ToInt64(
                                    key3.GetValue(
                                        "LastRecordNum"));
                    }
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Read_Registry -->> Error : " + er.Message);
                return false;
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        private void Timer1Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            var rec = new Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
            IDataReader readReader = null;
            DbCommand cmd = null;

            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Dal: " + Dal);
            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> virtualhost: " + virtualHost);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Start executing the query");
                string readQuery = "SELECT MAX(ID) AS ID From DBA_EBYS_TEST.LOG_EBYS ";

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readQuery : " + readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> db_name : " + dbName);

                readReader = Database.ExecuteReader(dbName, readQuery, CommandBehavior.CloseConnection, out cmd);
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readReader sonrası.");

                cmd.CommandTimeout = 2000;
                readReader.Read();
                long datacount = Convert_To_Int64(readReader[0].ToString());

                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> datacount: " + datacount);

                readReader.Close();
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Total data count in DBA_EBYS_TEST.LOG_EBYS table : " + datacount.ToString(CultureInfo.InvariantCulture));

                if (datacount < lastRecordnum)
                {
                    lastRecordnum = 0;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> DBA_EBYS_TEST.LOG_EBYS table is truncated. Started to read at the beginning.");
                }

                //if (last_recordnum != 0)
                {
                    //RecordFields.currentPosition = Convert.ToInt64(lastRecordnum) + Convert.ToInt64(maxRecordSend);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "timer1_Tick -->> is Last Pozision != 0 ");
                readQuery = "SELECT ID, AKSIYON, KULLANICI_BIRIM_ID, TABLO, REFERANS_ID, ISLEM_TIME, ACIKLAMA, AKTIF, ETKILENEN, ETKILENEN_TABLO, ISTEMCI_IP_ADRES FROM DBA_EBYS_TEST.LOG_EBYS WHERE ID > " + lastRecordnum + " and ID <= " + RecordFields.currentPosition + " ORDER BY ID ASC";

                cmd = null;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> readQuery : " + readQuery);
                readReader = Database.ExecuteReader(dbName, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 2000;

                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Finish executing the query. Query : " + readQuery);

                RecordFields.rowCount = 0;
                L.Log(LogType.FILE, LogLevel.DEBUG, "timer1_Tick -->> currentPosition_0: " + RecordFields.currentPosition);

                while (readReader.Read())
                {
                    rec.LogName = "HMDEBYSOracleAuditv1_0_0Recorder";
                    try
                    {
                        string processTime = Convert_To_String(readReader["ISLEM_TIME"]).Split(',')[0];
                        DateTime dt = Convert.ToDateTime(processTime);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Date_Time: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Date_Time: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["ID"].ToString()))
                        {
                            rec.Recordnum = Convert_To_Int32(readReader["ID"].ToString());
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Recordnum: " + rec.Recordnum);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Recordnum: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["AKSIYON"].ToString()))
                        {
                            rec.EventCategory = Convert_To_String(readReader["AKSIYON"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> EventCategory: " + rec.EventCategory);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> EventCategory: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["TABLO"].ToString()))
                        {
                            rec.CustomStr1 = Convert_To_String(readReader["TABLO"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr1: " + rec.CustomStr1);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr1: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["ETKILENEN_TABLO"].ToString()))
                        {
                            rec.CustomStr2 = Convert_To_String(readReader["ETKILENEN_TABLO"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr2: " + rec.CustomStr2);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr2: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["ISTEMCI_IP_ADRES"].ToString()))
                        {
                            rec.CustomStr5 = Convert_To_String(readReader["ISTEMCI_IP_ADRES"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr5: " + rec.CustomStr5);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr5: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["ETKILENEN"].ToString()))
                        {
                            rec.CustomStr6 = Convert_To_String(readReader["ETKILENEN"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomStr6: " + rec.CustomStr6);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr6: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["REFERANS_ID"].ToString()))
                        {
                            rec.CustomInt1 = Convert_To_Int32(readReader["REFERANS_ID"].ToString());
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt1: " + rec.CustomInt1);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt1: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["KULLANICI_BIRIM_ID"].ToString()))
                        {
                            rec.CustomInt2 = Convert_To_Int32(readReader["KULLANICI_BIRIM_ID"].ToString());
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt2: " + rec.CustomInt2);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt2: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["AKTIF"].ToString()))
                        {
                            rec.CustomInt3 = Convert_To_Int32(readReader["AKTIF"].ToString());
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> CustomInt3: " + rec.CustomInt3);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomInt3: " + exception);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(readReader["ACIKLAMA"].ToString()))
                        {
                            rec.Description = Convert_To_String(readReader["ACIKLAMA"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Description: " + rec.Description);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> CustomStr5: " + exception);
                    }

                    lastRecordnum = Convert_To_Int64(readReader["ID"].ToString());

                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Start sending Data. Last Record Number : " + lastRecordnum);

                    if (usingRegistry)
                    {
                        CustomServiceBase s = GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Sender -->> : ");
                    }
                    else
                    {
                        CustomServiceBase s = GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualHost, rec);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder -->> : ");
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder DAL : -->> : " + Dal);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Security Manager Remote Recorder  virtualhost : -->> : " + virtualHost);
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Finish Sending Data");

                    LastRecordDate = rec.Datetime;

                    if (usingRegistry)
                        SetRegistry(lastRecordnum);
                    else
                    {
                        CustomServiceBase s = GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, lastRecordnum.ToString(CultureInfo.InvariantCulture), "", "", "", LastRecordDate);
                    }

                    RecordFields.rowCount++;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "timer1_Tick -->> Prms : " + RecordFields.rowCount + " - " + lastRecordnum + " - " + datacount);


                if (RecordFields.rowCount == 0 && datacount > lastRecordnum)
                {
                    RecordFields.currentPosition = RecordFields.currentPosition + maxRecordSend;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "timer1_Tick -->> currentPosition: " + RecordFields.currentPosition);

                }

            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + er);
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
            int result;
            if (Int32.TryParse(value, out result))
                return result;
            return 0;
        }

        private long Convert_To_Int64(string value)
        {
            long result;
            if (Int64.TryParse(value, out result))
                return result;
            return 0;
        }

        private string Convert_To_String(object o)
        {
            string result;
            try
            {
                result = Convert.ToString(o);
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        public bool SetRegistry(long status)
        {
            RegistryKey rk = null;
            try
            {
                var registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE");
                if (registryKey != null)
                {
                    var subKey = registryKey.CreateSubKey("Natek");
                    if (subKey != null)
                    {
                        var key = subKey.CreateSubKey("Security Manager");
                        if (key != null)
                        {
                            var registryKey1 = key.CreateSubKey("Recorder");
                            if (registryKey1 != null)
                                rk = registryKey1.CreateSubKey("HMDEBYSOracleAuditv1_0_0Recorder");
                        }
                    }
                }
                if (rk != null)
                {
                    rk.SetValue("LastRecordNum", status);
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SQLServer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public bool InitializeLogger()
        {
            try
            {
                L = new CLogger();
                L.SetLogLevel((LogLevel)((trcLevel < 0 || trcLevel > 4) ? 3 : trcLevel));
                L.SetLogFile(errLog);
                L.SetTimerInterval(LogType.FILE, loggingInterval);
                L.SetLogFileSize(logSize);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("RemoteRecorderBase->InitializeLogger() ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // Initialize_Logger
    }
}
