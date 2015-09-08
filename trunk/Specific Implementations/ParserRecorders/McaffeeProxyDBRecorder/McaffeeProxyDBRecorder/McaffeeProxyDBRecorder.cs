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
using Natek.Utils;

namespace McafeeProxyDBRecorder
{
    public class McafeeProxyDBRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, mcdb_name = "", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;

        public McafeeProxyDBRecorder()
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McafeeProxyDBRecorder Recorder functions may not be running");
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
                            //L.Log(LogType.FILE, LogLevel.ERROR, " Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating McafeeProxyDBRecorder DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        db_name = "McaffeeProxy" + Id.ToString();
                        if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                            L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create McafeeProxyDBRecorder DAL");
                        else
                            L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating McafeeProxyDBRecorder DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McafeeProxyDBRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\McafeeProxyDBRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McafeeProxyDBRecorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            mcdb_name = Location;
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\McafeeProxyDBRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("DBName").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("Interval"));
                mcdb_name = rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("MCDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("McafeeProxyDBRecorder").GetValue("LastRecordNum"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McafeeProxyDBRecorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);

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

                L.Log(LogType.FILE, LogLevel.DEBUG, "Set_LastPosition()| dbname is:  scr_fct_exact_access");
                if (fromend)
                    readQuery = "select MAX(detail_record_id) as TOTAL FROM  scr_fct_exact_access";
                else
                    readQuery = "select MIN(detail_record_id) as TOTAL FROM  scr_fct_exact_access";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnum = Convert.ToInt64(readReader["TOTAL"]) - 1;

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

        public bool tableControl(string tablename, string dbname)
        {
            bool control = true;

            //IDataReader readReader = null;
            //DbCommand cmd = null;
            //string readQuery = "IF EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + "EPOEvents" + "') BEGIN select TOP 1 * from " + dbname + ".."+tablename+" END";
            //L.Log(LogType.FILE, LogLevel.DEBUG, " Query is in control" + readQuery);
            //readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
            //cmd.CommandTimeout = 1200;

            //while (readReader.Read())
            //{
            //    control = true;
            //}
            return control;
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McafeeProxyDBRecorder Recorder functions may not be running");
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

                if (last_recordnum <= 0)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "last_recordnum == 0:set_LastPosition");
                    Set_LastPosition();
                }
                /*
                wsdb_name_last= Get_Ws_Dbname();
                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + wsdb_name_last);

                if (wsdb_name_last == null)
                    wsdb_name_last = wsdb_name;

                */

                int i = 0;

                //readQuery = "select D.RECORD_NUMBER,D.DATE_TIME,D.USER_ID,D.CATEGORY,D.DISPOSITION_CODE,D.PROTOCOL_ID,U.URL,D.PORT,D.SOURCE_SERVER_IP_INT,D.DESTINATION_IP_INT,D.SOURCE_IP_INT,D.HITS,D.BYTES_SENT,D.BYTES_RECEIVED from " + wsdb_name + "..URLS AS U(nolock), " + wsdb_name_last + "..LOG_DETAILS AS D(nolock) WHERE D.URL_ID= U.URL_ID AND D.RECORD_NUMBER>" + last_recordnum.ToString();//+" ORDER BY D.RECORD_NUMBER";                
                readQuery =
                              "  SELECT TOP(" + max_record_send + ") A.* FROM  \n" +
                              "  ( \n" +
                              "  SELECT \n" +
                              "      A.detail_record_id AS RECORDNUMBER \n" +
                              "        ,B.user_name AS USERSID_USERNAME \n" +
                              "        ,A.seconds_since_epoch AS DATE_TIME \n" +
                              "        ,A.url AS DESCRIPTION_URL \n" +
                              "        ,I.log_source AS SOURCENAME \n" +
                              "        ,'McafeeProxyDBRecorder' AS [LOG_NAME] \n" +
                              "        ,A.bytes AS CUSTOMINT1_BYTES \n" +
                              "        ,M.http_status AS CUSTOMSTR1_STATUS \n" +
                              "        ,L.ipaddress AS CUSTOMSTR2_IPADRESS \n" +
                              "        ,J.category_name AS CUSTOMSTR4_CATEGORY \n" +
                              "        ,G.action_name AS CUSTOMSTR5_ACTION \n" +
                              "        ,C.protocol AS CUSTOMSTR6_PROTOCOL \n" +
                              "        ,C.method AS CUSTOMSTR7_METHOD \n" +
                              "        ,K.virus_name AS CUSTOMSTR8_VIRUSNAME \n" +
                              "        ,(M.cache_status \n" +
                              "         + '|' + M.cache_status_group_1 \n" +
                              "         + '|' + M.cache_status_group_2 \n" +
                              "         + '|' + N.agent_id_string \n" +
                              "         + '|' + N.agent_id_group_1 \n" +
                              "         + '|' + N.agent_id_group_2 \n" +
                              "         + '|' + E.reputation_name \n" +
                              "         + '|' + H.reason_name \n" +
                              "         + '|' + A.user_defined \n" +
                              "         + '|' + A.user_defined_2 \n" +
                              "         + '|' + A.user_defined_3 \n" +
                              "         + '|' + A.user_defined_4 \n" +
                              "         + '| FileExtension:' + C.file_ext \n" +
                              "         + '| ContentType:' + C.content_type \n" +
                              "        ) AS CUSTOMSTR9_STATUS_INFORMATION \n" +
                              "        ,D.site_name AS CUSTOMSTR3_SITENAME      \n" +
                              "    FROM  \n" +
                              "    scr_fct_exact_access A INNER JOIN scr_dim_user B on A.user_id = B.user_id  \n" +
                              "    INNER JOIN scr_dim_site_request C ON A.site_request_id = C.site_request_id \n" +
                              "    INNER JOIN scr_dim_site D ON C.site_id = D.site_id \n" +
                              "    INNER JOIN scr_dim_reputation E ON C.reputation_id = E.reputation_id \n" +
                              "    INNER JOIN scr_dim_action_request F ON A.action_request_id = F.action_request_id \n" +
                              "    INNER JOIN scr_dim_action G ON F.action_id = G.action_id \n" +
                              "    INNER JOIN scr_dim_reason H ON F.reason_id = H.reason_id \n" +
                              "    INNER JOIN scr_dim_log_source I ON A.log_source_id = I.log_source_id \n" +
                              "    INNER JOIN scr_dim_category J ON A.category_one_id = J.category_id \n" +
                              "    INNER JOIN scr_dim_virus K ON A.virus_id = K.virus_id \n" +
                              "    INNER JOIN scr_dim_ipaddress L ON A.user_ip_id = L.ip_id \n" +
                              "    INNER JOIN scr_dim_status M ON A.status_id = M.status_id \n" +
                              "    INNER JOIN scr_dim_agent N ON A.agent_id = N.agent_id) A \n" +
                              " WHERE A.RECORDNUMBER >" + last_recordnum + " ORDER BY A.RECORDNUMBER ";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is in if" + readQuery);

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

                readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);

                cmd.CommandTimeout = 1200;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                while (readReader.Read())
                {


                    if (!Convert.IsDBNull(readReader["RECORDNUMBER"]))
                    {
                        
                        rec.CustomInt10 = cUtility.ObjectToLong(readReader["RECORDNUMBER"], -1);
                        last_recordnum = rec.CustomInt10;
                    }
                    if (!Convert.IsDBNull(readReader["USERSID_USERNAME"]))
                    {
                        rec.UserName = cUtility.ObjectToString(readReader["USERSID_USERNAME"], "").Trim('>');
                    }
                    if (!Convert.IsDBNull(readReader["DESCRIPTION_URL"]))
                    {
                        rec.Description = cUtility.ObjectToString(readReader["DESCRIPTION_URL"], "");
                    }
                    if (!Convert.IsDBNull(readReader["SOURCENAME"]))
                    {
                        rec.SourceName = cUtility.ObjectToString(readReader["SOURCENAME"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMINT1_BYTES"]))
                    {
                        rec.CustomInt1 = cUtility.ObjectToInteger(readReader["CUSTOMINT1_BYTES"], -1);
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR1_STATUS"]))
                    {
                        rec.CustomStr1 = cUtility.ObjectToString(readReader["CUSTOMSTR1_STATUS"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR2_IPADRESS"]))
                    {
                        string dataIp = "";
                        try
                        {
                            dataIp = cUtility.ObjectToString(readReader["CUSTOMSTR2_IPADRESS"], "");
                            string[] sIp = cUtility.ObjectToString(readReader["CUSTOMSTR2_IPADRESS"], "").Split('.');
                            rec.CustomStr2 = Convert.ToInt32(sIp[0]).ToString() + "." +
                                             Convert.ToInt32(sIp[1]).ToString() + "." +
                                             Convert.ToInt32(sIp[2]).ToString() + "." +
                                             Convert.ToInt32(sIp[3]).ToString();
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " CUSTOMSTR2_IPADRESS column is not proper IP.");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " readReader[\"CUSTOMSTR2_IPADRESS\"] : " + dataIp);
                            rec.CustomStr2 = dataIp;
                        }
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR4_CATEGORY"]))
                    {
                        rec.CustomStr4 = cUtility.ObjectToString(readReader["CUSTOMSTR4_CATEGORY"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR5_ACTION"]))
                    {
                        rec.CustomStr5 = cUtility.ObjectToString(readReader["CUSTOMSTR5_ACTION"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR6_PROTOCOL"]))
                    {
                        rec.CustomStr6 = cUtility.ObjectToString(readReader["CUSTOMSTR6_PROTOCOL"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR7_METHOD"]))
                    {
                        rec.CustomStr7 = cUtility.ObjectToString(readReader["CUSTOMSTR7_METHOD"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR8_VIRUSNAME"]))
                    {
                        rec.CustomStr8 = cUtility.ObjectToString(readReader["CUSTOMSTR8_VIRUSNAME"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR9_STATUS_INFORMATION"]))
                    {
                        rec.CustomStr9 = cUtility.ObjectToString(readReader["CUSTOMSTR9_STATUS_INFORMATION"], "");
                    }
                    if (!Convert.IsDBNull(readReader["CUSTOMSTR3_SITENAME"]))
                    {
                        rec.CustomStr3 = cUtility.ObjectToString(readReader["CUSTOMSTR3_SITENAME"], "");
                    }
                    if (!Convert.IsDBNull(readReader["LOG_NAME"]))
                    {
                        rec.LogName = cUtility.ObjectToString(readReader["LOG_NAME"], "");
                    }
                    if (!Convert.IsDBNull(readReader["DATE_TIME"]))
                    {
                        DateTime d = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(cUtility.ObjectToDouble(readReader["DATE_TIME"], 0));
                        rec.Datetime = d.Year + "/" + d.Month + "/" + d.Day + " " + d.Hour + ":" + d.Minute + ":" + d.Second;
                        last_recdate = rec.Datetime;
                    }

                    if (rec.Description.Length > 900)
                    {
                        if (rec.Description.Length > 1800)
                        {
                            rec.CustomStr10 = rec.Description.Substring(900, 900);
                        }
                        else
                        {
                            rec.CustomStr10 = rec.Description.Substring(900, rec.Description.Length - 900);
                        }
                        rec.Description = rec.Description.Substring(0, 900);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
                    }



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

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("McafeeProxyDBRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager McafeeProxyDBRecorder Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McafeeProxyDBRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
