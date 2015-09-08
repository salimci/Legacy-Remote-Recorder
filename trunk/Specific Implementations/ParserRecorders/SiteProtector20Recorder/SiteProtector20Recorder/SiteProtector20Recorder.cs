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

namespace SiteProtector20Recorder
{
    public class SiteProtector20Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, spdb_name = "RealSecureDB", spdb_name_last = "RealSecureDB", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        public SiteProtector20Recorder()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
        }
        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SiteProtector Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SiteProtector Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating SiteProtector DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        db_name = "SiteProtectordb" + Id.ToString();
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create SiteProtector DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating SiteProtector DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SiteProtector Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SiteProtectorRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SiteProtector Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SiteProtectorRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("Interval"));
                spdb_name = rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("SPDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("SiteProtectorRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SiteProtector Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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

                spdb_name_last = Get_Sp_Dbname();
                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + spdb_name_last);

                if (spdb_name_last == null)
                    spdb_name_last = spdb_name;

                readQuery = "select MAX(ObservanceID) FROM " + spdb_name_last + "..Observances(nolock)";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

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
                        s.SetReg(Id, last_recordnum.ToString(), "", "", "", last_recdate);
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
        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SiteProtector Recorder functions may not be running");
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

                spdb_name_last = Get_Sp_Dbname();
                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + spdb_name_last);

                if (spdb_name_last == null)
                    spdb_name_last = spdb_name;

                List<CompositeLogFormat> logList = new List<CompositeLogFormat>();

                int i = 0;
                DateTime beforeDBConnection = DateTime.Now;

                string readQuery = null;
                IDataReader readReader = null;
                DbCommand cmd = null;

                readQuery = "select TOP(" + max_record_send + ") O.ObservanceID as Recordnum, O.ObservanceTime as Date_time, " +
                    "O.ObservanceCount as CustomInt1, O.ClearedCount as CustomInt2, O.LastModifiedAt as CustomStr1, S.SourceIPDisplay " +
                    "as CustomStr3, T.TargetIPDisplay as CustomStr4, OT.ObservanceTypeDesc as SourceName, SC.ConseqName as EventCategory," +
                    " SC.ChkDetailDesc as Descrption, SC.ChkName as CustomStr2, O.SensorID as CustomInt3, OB.ObjectName as SourceName, " +
                    "SE.SeverityDesc as CustomStr6, VS.VulnStatusDesc as CustomStr5, O.VLanID as CustomInt4, O.VirtualSensorID as CustomInt5," +
                    " EU.UserName as UserName, O.SensorInterfaceID as CustomInt6, O.CheckSumID as CustomInt7 from " + spdb_name +
                    "..Observances(nolock) as O left join " + spdb_name + "..SourceHost(nolock) as S On O.SourceID = S.SourceID left join " +
                    spdb_name + "..TargetHost(nolock) as T On O.TargetID = T.TargetID left join " + spdb_name + "..ObservanceType(nolock) as " +
                    "OT On O.ObservanceType = OT.ObservanceType left join " + spdb_name + "..SecurityChecks(nolock) as SC On O.SecChkID = " +
                    "SC.SecChkID left join " + spdb_name + "..[Object](nolock) as OB On O.ObjectID = OB.ObjectID left join " + spdb_name +
                    "..Severity(nolock) as SE On O.SeverityID = SE.SeverityID left join " + spdb_name + "..VulnStatus(nolock) as VS On " +
                    "O.VulnStatus = VS.VulnStatus left join " + spdb_name + "..EventUser(nolock) as EU On O.EventUserID = EU.EventUserID " +
                    "where ObservanceID>" + last_recordnum.ToString() + " Order By ObservanceID";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                try
                {
                    while (readReader.Read())
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ObservanceData Last line is not null");

                        CompositeLogFormat log = new CompositeLogFormat();

                        List<object> mainLogList = new List<object>();

                        for (int p = 0; p <= 18; p++)
                        {
                            mainLogList.Add(readReader.GetValue(p));
                        }

                        log.setMainLog(mainLogList);

                        string readQuery2 = null;
                        IDataReader readReader2 = null;
                        DbCommand cmd2 = null;

                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData Service Started");


                            readQuery2 = "select SDA.SensorDataID, SDA.AttributeName, SDA.AttributeValue from " + spdb_name + "..SensorDataAVP1 as SDA " +
                                "where SDA.SensorDataID IN (select top 1 SD.SensorDataID from  " + spdb_name + "..Observances(nolock) as O LEFT JOIN  "
                                + spdb_name + "..SensorData1(nolock) as SD on " + "O.ObservanceID = SD.ObservanceID where O.ObservanceID = " + readReader.GetInt64(0).ToString() +
                                " and SD.SensorDataID IS NOT NULL order by SD.SensorDataID desc)";

                            L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData Query is " + readQuery2);


                            readReader2 = Database.ExecuteReader(db_name, readQuery2, CommandBehavior.CloseConnection, out cmd2);
                            cmd2.CommandTimeout = 1200;

                            L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the SensorData Query");

                            List<ChildLogFormat> childLogList = new List<ChildLogFormat>();
                            while (readReader2.Read())
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData last line is not null");
                                ChildLogFormat childLog = new ChildLogFormat(readReader2.GetValue(1), readReader2.GetValue(2));
                                childLogList.Add(childLog);
                            }
                            log.setChildLog(childLogList);
                            logList.Add(log);
                        }
                        catch (Exception ex)
                        {
                            L.LogTimed(LogType.FILE, LogLevel.ERROR, ex.ToString());
                        }
                        finally
                        {
                            readReader2.Close();
                            Database.Drop(ref cmd2);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData Retrieval Service Stopped");
                        }
                        i++;
                        if (i > max_record_send)
                        {
                            cmd.Cancel();
                            return;
                        }
                    }
                }
                catch (Exception exp)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, exp.ToString());
                }
                finally
                {
                    readReader.Close();
                    Database.Drop(ref cmd);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ObservanceData Retrieval Service Stopped");
                }

                DateTime afterDBConnection = DateTime.Now;
                L.Log(LogType.FILE, LogLevel.INFORM, "Time passed to retrieve " + i + " observance logs from RealSecureDB: " + afterDBConnection.Subtract(beforeDBConnection));

                try
                {
                    foreach (CompositeLogFormat log in logList)
                    {
                        #region "Get Observance Logs"

                        //dbname = log.MainLog.GetString(0).ToString();
                        rec.LogName = "SiteProtector Recorder";
                        rec.ComputerName = remote_host;
                        rec.CustomInt8 = Convert.ToInt64(log.MainLog[0]); //Observance ID                   
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8.ToString());

                        rec.Datetime = Convert.ToDateTime(log.MainLog[1]).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime.ToString());

                        if (log.MainLog[2] == DBNull.Value)
                        {
                            rec.CustomInt1 = 0;
                        }
                        else
                        {
                            rec.CustomInt1 = Convert.ToInt32(log.MainLog[2]); // Observance Count
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1.ToString());

                        if (log.MainLog[3] == DBNull.Value)
                        {
                            rec.CustomInt2 = 0;
                        }
                        else
                        {
                            rec.CustomInt2 = Convert.ToInt32(log.MainLog[3]); // Observance Cleared Count
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2.ToString());

                        if (log.MainLog[4] == DBNull.Value)
                        {
                            rec.CustomStr1 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr1 = Convert.ToDateTime(log.MainLog[4]).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff"); // Observance Last Modified At
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);

                        if (log.MainLog[5] == DBNull.Value)
                        {
                            rec.CustomStr3 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr3 = Convert.ToString(log.MainLog[5]); // Source IP Display
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);

                        if (log.MainLog[6] == DBNull.Value)
                        {
                            rec.CustomStr4 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr4 = Convert.ToString(log.MainLog[6]); // Target IP Display
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);

                        if (log.MainLog[7] == DBNull.Value)
                        {
                            rec.SourceName = "Unknown";
                        }
                        else
                        {
                            rec.SourceName = Convert.ToString(log.MainLog[7]); //Observance Type Desc
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);

                        if (log.MainLog[8] == DBNull.Value)
                        {
                            rec.EventCategory = "Unknown";
                        }
                        else
                        {
                            rec.EventCategory = Convert.ToString(log.MainLog[8]); //Security Check Conseq Name
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                        if (log.MainLog[9] == DBNull.Value)
                        {
                            rec.Description = "Unknown";
                        }
                        else
                        {
                            rec.Description = Convert.ToString(log.MainLog[9]); //Security Check Detailed Desc
                            if (rec.Description.Length > 900)
                            {
                                rec.Description = rec.Description.Substring(0, 900);
                            }
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);

                        if (log.MainLog[10] == DBNull.Value)
                        {
                            rec.CustomStr2 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr2 = Convert.ToString(log.MainLog[10]); //Security Check Name
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);

                        if (log.MainLog[11] == DBNull.Value)
                        {
                            rec.CustomInt3 = 0;
                        }
                        else
                        {
                            rec.CustomInt3 = Convert.ToInt32(log.MainLog[11]); //Observance Sensor ID
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3.ToString());

                        if (log.MainLog[12] == DBNull.Value)
                        {
                            rec.EventType = "Unknown";
                        }
                        else
                        {
                            rec.EventType = Convert.ToString(log.MainLog[12]); //Observance Object Name
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);

                        if (log.MainLog[13] == DBNull.Value)
                        {
                            rec.CustomStr6 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr6 = Convert.ToString(log.MainLog[13]); //Severity Desc
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);

                        if (log.MainLog[14] == DBNull.Value)
                        {
                            rec.CustomStr5 = "Unknown";
                        }
                        else
                        {
                            rec.CustomStr5 = Convert.ToString(log.MainLog[14]); //Vulnerability Status Desc
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);

                        if (log.MainLog[15] == DBNull.Value)
                        {
                            rec.CustomInt4 = 0;
                        }
                        else
                        {
                            rec.CustomInt4 = Convert.ToInt32(log.MainLog[15]); //Observance VLan ID
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4.ToString());

                        if (log.MainLog[16] == DBNull.Value)
                        {
                            rec.CustomInt5 = 0;
                        }
                        else
                        {
                            rec.CustomInt5 = Convert.ToInt32(log.MainLog[16]); //Observance Virtual Sensor ID
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5.ToString());

                        if (log.MainLog[17] == DBNull.Value)
                        {
                            rec.UserName = "Unknown";
                        }
                        else
                        {
                            rec.UserName = Convert.ToString(log.MainLog[17]); //Event User Name
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);

                        if (log.MainLog[18] == DBNull.Value)
                        {
                            rec.CustomInt6 = 0;
                        }
                        else
                        {
                            rec.CustomInt6 = Convert.ToInt32(log.MainLog[18]); //Observance Sensor Interface ID
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6.ToString());

                        #endregion

                        #region "Get SecureData Logs"

                        if (log.ChildLog != null)
                        {
                            for (int k = 0; k < log.ChildLog.Count - 1; k++)
                            {
                                switch (Convert.ToString(log.ChildLog[k].AttributeType))
                                {
                                    case ":value":
                                        rec.CustomStr10 = "value"; //AttributeName
                                        if (log.ChildLog[k].Attribute != DBNull.Value)
                                        {
                                            rec.CustomStr7 = Convert.ToString(log.ChildLog[k].Attribute); // AttributeValue
                                        }
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData :value: " + rec.CustomStr7);
                                        break;
                                    case ":server":
                                        rec.CustomStr10 = "server"; //AttributeName
                                        if (log.ChildLog[k].Attribute != DBNull.Value)
                                        {
                                            rec.CustomStr8 = Convert.ToString(log.ChildLog[k].Attribute); // AttributeValue
                                        }
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData :server: " + rec.CustomStr8);
                                        break;
                                    case ":URL":
                                        rec.CustomStr10 = "URL"; //AttributeName
                                        if (log.ChildLog[k].Attribute != DBNull.Value)
                                        {
                                            rec.CustomStr9 = Convert.ToString(log.ChildLog[k].Attribute); // AttributeValue
                                        }
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "SensorData :data: " + rec.CustomStr9);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        #endregion

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Observances Data");

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
                        last_recordnum = rec.CustomInt8;
                        last_recdate = rec.Datetime;
                    }
                }
                catch (Exception exp2)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, exp2.ToString());
                }

                DateTime afterParse = DateTime.Now;
                L.Log(LogType.FILE, LogLevel.INFORM, "Time passed to parse and send " + i + " observance logs from RealSecureDB: " + afterParse.Subtract(afterDBConnection));

                L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + last_recordnum.ToString());
                if (usingRegistry)
                    Set_Registry(last_recordnum);
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, last_recordnum.ToString(), "", "", "", last_recdate);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish setting last position");
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
            }
        }

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("SiteProtectorRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SiteProtector Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public string Get_Sp_Dbname()
        {
            IDataReader readReader = null;
            DbCommand cmd = null;
            string dbname = null;
            try
            {
                //readReader = Database.ExecuteReader(db_name,"select MAX(NAME) from master..sysdatabases(nolock) where name like '"+spdb_name+"%'", out cmd);
                readReader = Database.ExecuteReader(db_name, "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MAX(crdate) from master..sysdatabases(nolock) where name like '" + spdb_name + "%')", out cmd);
                while (readReader.Read())
                {
                    dbname = readReader.GetString(0).ToString();
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
                EventLog.WriteEntry("Security Manager SiteProtector Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
