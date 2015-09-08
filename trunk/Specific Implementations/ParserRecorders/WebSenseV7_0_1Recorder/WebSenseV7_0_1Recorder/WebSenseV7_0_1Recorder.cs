using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;
using Natek.Util;

namespace Natek.Recorders.Remote
{
    public class WebSenseV7_0_1Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 1000000, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, wsdb_name = "", db_name, location, user, password, remote_host = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private string _virtualHost, _dal;
        protected Int32 Id = 0;
        private CLogger L;

        private bool InitializeInstance()
        {
            if (!reg_flag)
            {
                if (usingRegistry)
                {
                    if (!ReadLocalRegistry())
                    {
                        L.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error on Reading the Registry ");
                        return false;
                    }
                }
                else
                {
                    if (!GetLogDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return false;
                    }
                }

                if (!InitializeLogger())
                {
                    L.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
                    return false;
                }
                if (!usingRegistry)
                {
                    db_name = "Websensedb" + Id;
                    L.Log(LogType.FILE, LogLevel.INFORM,
                          Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password,
                                                 location)
                              ? "Successfully create Websense DAL"
                              : "Failed on creating Websense DAL");

                }
                reg_flag = true;
            }
            return true;
        }

        public override void Init()
        {
            try
            {
                InitializeInstance();
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                base.Init();
            }
        }

        public override void Start()
        {
            try
            {
                base.Start();
            }
            finally
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += timer1_Tick;
                timer1.Interval = timer_interval;
                timer1.AutoReset = false;
                timer1.Enabled = true;
            }
        }

        protected bool GetLogDir()
        {
            try
            {
                using (var regRecorder = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    err_log = regRecorder.GetValue("Home Directory") + @"log\" + GetType().Name + Id + ".log";
                    var fInfo = new FileInfo(err_log);

                    if (fInfo.Directory != null && !fInfo.Directory.Exists)
                        fInfo.Directory.Create();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            if (MaxLineToWait > 0)
                max_record_send = MaxLineToWait;
            if (SleepTime > 0)
                timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            wsdb_name = LastFile;
            long.TryParse(LastPosition, out last_recordnum);
            zone = Zone;
            _virtualHost = Virtualhost;
            _dal = dal;
        }

        private bool ReadLocalRegistry()
        {
            try
            {
                using (var regManager = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager"))
                {
                    using (var regWebsense = regManager.OpenSubKey(@"Recorder\WebsenseRecorder"))
                    {
                        log_size = Convert.ToUInt32(regWebsense.GetValue("Log Size"));
                        logging_interval = Convert.ToUInt32(regWebsense.GetValue("Logging Interval"));
                        trc_level = Convert.ToInt32(regWebsense.GetValue("Trace Level"));
                        db_name = regWebsense.GetValue("DBName").ToString();
                        timer1.Interval = Convert.ToInt32(regWebsense.GetValue("Interval"));
                        wsdb_name = regWebsense.GetValue("WSDBName").ToString();
                        max_record_send = Convert.ToInt32(regWebsense.GetValue("MaxRecordSend"));
                        last_recordnum = Convert.ToInt64(regWebsense.GetValue("LastRecordNum"));
                    }
                    using (var regAgent = regManager.OpenSubKey("Agent"))
                    {
                        err_log = regAgent.GetValue("Home Directory") + @"log\" + GetType().Name + ".log";
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Websense Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Begin Processing Db(" + wsdb_name + ") LastRecord(" + last_recordnum + ")");
                if (!InitializeInstance())
                    return;

                if (string.IsNullOrEmpty(wsdb_name))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          usingRegistry
                              ? @"No websense log database found: SOFTWARE\Natek\Security Manager\Recorder\WebsenseRecorder\WSDBName does not exist or empty"
                              : @"No websense log database found: LASTFILE is empty in REMOTERECORDER table");
                }
                using (var con = GetConnection(true, db_name))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Re-capture ws dbname");
                    var rec = new Rec() { LogName = "Websense_7_0 Recorder" };
                    var svc = GetInstanceService(usingRegistry
                                                                 ? "Security Manager Sender"
                                                                 : "Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Begin process [" + wsdb_name + "]");
                    using (var cmd = con.CreateCommand())
                    {
                        prepareCommand(cmd);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + cmd.CommandText);
                        var last_recdate = string.Empty;
                        using (var rs = cmd.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Prepare Record");
                                prepareRec(rs, ref rec);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                                svc.SetData(_dal, _virtualHost, rec);

                                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                                last_recordnum = rec.CustomInt6;
                                last_recdate = rec.Datetime;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + last_recordnum.ToString());
                                if (usingRegistry)
                                    SetLocalRegistry(last_recordnum);
                                else
                                {
                                    GetInstanceService("Security Manager Remote Recorder")
                                        .SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "",
                                                wsdb_name, "", last_recdate);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Last File Is -->>" + wsdb_name);
                                }
                            }
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");
                    }
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error in timer handler:" + ex);
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "End Processing Db(" + wsdb_name + ") LastRecord(" + last_recordnum + ")");
            }
        }

        private static List<FieldInfo> fields;
        private int cnt = 0;
        static WebSenseV7_0_1Recorder()
        {
            fields = new List<FieldInfo>();
            foreach (var f in typeof(Rec).GetFields(BindingFlags.Instance | BindingFlags.Public))
                fields.Add(f);
        }

        private DbConnection GetConnection(bool fast, string db_name)
        {
            var con = Database.GetConnection(fast, db_name);
            if (con != null && (con.State == ConnectionState.Broken || con.State == ConnectionState.Closed))
                con.Open();
            return con;
        }

        private void prepareRec(DbDataReader rs, ref Rec rec)
        {
            rec.CustomInt6 = DbUtil.GetField<long>(rs, 0, 0);
            rec.Datetime = rs.GetDateTime(1)
                             .AddMinutes(zone)
                             .ToString("yyyy/MM/dd HH:mm:ss.fff");
            rec.UserName = DbUtil.GetField(rs, 2, string.Empty);
            rec.EventCategory = DbUtil.GetField(rs, 3, string.Empty);
            rec.CustomInt1 = DbUtil.GetField<short>(rs, 4, 0);
            rec.EventType = DbUtil.GetField(rs, 5, string.Empty);
            rec.Description = DbUtil.GetField(rs, 6, string.Empty);
            rec.CustomInt3 = DbUtil.GetField(rs, 7, 0);
            rec.CustomStr1 = Long2Ip(DbUtil.GetField(rs, 8, 0L));
            rec.CustomStr4 = Long2Ip(DbUtil.GetField(rs, 9, 0L));
            rec.CustomStr3 = Long2Ip(DbUtil.GetField(rs, 10, 0L));
            rec.CustomInt5 = DbUtil.GetField(rs, 11, 0);
            rec.CustomInt7 = DbUtil.GetField(rs, 12, 0);
            rec.CustomInt8 = DbUtil.GetField(rs, 13, 0);
            rec.CustomStr5 = DbUtil.GetField(rs, 14, string.Empty);
            var str = DbUtil.GetField(rs, 15, string.Empty);
            if (str.Length > 900)
            {
                rec.CustomStr6 = str.Substring(0, 900);
                rec.CustomStr7 = str.Length <= 1800 ? str.Substring(900) : str.Substring(900, 900);
                rec.CustomStr7 = str.Length <= 1800 ? str.Substring(900) : str.Substring(900, 900);
            }
            else
                rec.CustomStr6 = str;
        }

        private string Long2Ip(long p)
        {
            return string.Format("{0}.{1}.{2}.{3}", (p & 0x00000000FF000000) >> 24, (p & 0x0000000000FF0000) >> 16,
                                 (p & 0x000000000000FF00) >> 8, p & 0x00000000000000FF);
        }

        private void prepareCommand(DbCommand cmd)
        {
            cmd.CommandText = "select TOP " + max_record_send +
                                              @" d.record_number,d.date_time,us.user_login_name,
c.name as category,d.disposition_code,p.name as protocol,u.name,
d.port,d.source_server_ip_int,d.destination_ip_int,d.source_ip_int,
d.hits,d.bytes_sent,d.bytes_received,c.child_name,d.full_url
from " + wsdb_name + ".dbo.wse_urls AS u(nolock)," +
                                              wsdb_name + ".dbo.users AS us(nolock)," +
                                              wsdb_name + ".dbo.category AS c(nolock)," +
                                              wsdb_name + ".dbo.protocols AS p(nolock), " +
                                              wsdb_name + @".dbo.log_details AS d(nolock)
where d.url_id= u.wse_url_id and us.user_id=d.user_id
and c.category=d.category and p.id =d.protocol_id and d.record_number>" + last_recordnum +
                                              " order by record_number";
        }

        private bool SetLocalRegistry(long status)
        {
            try
            {
                using (var reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Natek\Security Manager\Recorder\WebsenseRecorder"))
                {
                    reg.SetValue("LastRecordNum", status);
                    return true;
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        protected bool InitializeLogger()
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
    }
}