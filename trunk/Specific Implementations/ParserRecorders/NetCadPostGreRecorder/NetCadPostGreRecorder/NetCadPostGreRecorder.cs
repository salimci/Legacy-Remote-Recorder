using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Text;
using CustomTools;
using System.Threading;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using DAL;
using Npgsql;

namespace NetCadPostGreRecorder
{
    class NetCadPostGreRecorder : CustomBase
    {
        private System.Timers.Timer timer;
        private int trc_level = 3, timer_interval = 60000, max_record_send = 100, zone = 0;
        private long last_position;

        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, mcdb_name = "", db_name, location, user, password, remote_host = "", last_recdate = "", lastDb = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;

        public NetCadPostGreRecorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {

        }

        public override void Init()
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Tick);
            timer.Interval = timer_interval;
            timer.Enabled = true;

            try
            {
                if (usingRegistry)
                {
                    if (!reg_flag)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, Log.LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, Log.LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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

                        //L.Log(LogType.FILE, LogLevel.INFORM, "Start creating McaffeeEpo DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        //if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create McaffeeEpo DAL");
                        //else
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating McaffeeEpo DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NetCadPostGreRecorder Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NetCadPostGreRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("DBName").ToString();

                this.timer.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("Interval"));

                mcdb_name = rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("MCDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("MaxRecordSend"));

                string position = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("NetCadPostGreRecorder").GetValue("LastRecordNum"));
                last_position = Convert.ToInt64(position);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool SetNetCadPostGre_Registry(string status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NetCadPostGreRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, Log.LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager NetCad PostGre Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NetCadPostGreRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NetCadPostGre Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            last_position = Convert.ToInt64(LastPosition);
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

        public bool Set_LastPosition()
        {
            string readQuery = null;

            PostGreConnection pgc = new PostGreConnection();
            NpgsqlDataReader readReader;
            NpgsqlCommand command;

            try
            {
                readQuery = "Select kayit_tarih,kayit_saat,kullanici_ip,kullanici_name,table_name,prev_data,current_data,url,kodu from loglar where kodu  = (select max(kodu) from loglar)";

                L.Log(LogType.FILE, Log.LogLevel.DEBUG, " Query Event is " + readQuery);

                //ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                //readReader = ac.ExecuteAccessQuery(readQueryEvent);

                pgc.OpenPostGreConnection(remote_host, user, password, mcdb_name);
                command = new NpgsqlCommand();

                readReader = pgc.ExecutePostGreQuery(readQuery, ref command);

                L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Finish executing the query");

                while (readReader.Read())
                {
                    last_position = Convert.ToInt64(readReader[8]);
                    L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Son pozisyonu ayarlariken. Son Pozisyon: " + last_position);
                }
                readReader.Close();

                if (usingRegistry)
                    SetNetCadPostGre_Registry(last_position.ToString());
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder.");
                    s.SetReg(Id, last_position.ToString(), "", "", "", last_recdate);
                }

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "Set_LastPosition() -->> catch hata yakaladý.");
                L.Log(LogType.FILE, Log.LogLevel.ERROR, er.ToString());
                return false;
            }
            finally
            {
                pgc.ClosePostGreConnection();
            }
        }

        public override void Clear()
        {
            if (timer != null)
                timer.Enabled = false;
        }

        private void timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, Log.LogLevel.INFORM, "Service Started");
            string readQuery = null;
            PostGreConnection pgc = new PostGreConnection();
            NpgsqlDataReader readReader = null;
            NpgsqlCommand command = null;

            try
            {
                if (!reg_flag)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, Log.LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }

                if (fromend)
                {
                    if (!Set_LastPosition())
                        L.Log(LogType.FILE, Log.LogLevel.INFORM, "Error on setting last position see eventlog for more details");
                    fromend = false;
                }

                readQuery = "Select kayit_tarih,kayit_saat,kullanici_ip,kullanici_name,table_name,prev_data,current_data,url,kodu from loglar where kodu > " + last_position.ToString() + " ORDER BY kodu";

                //readQuery = "Select * from loglar";
                L.Log(LogType.FILE, Log.LogLevel.DEBUG, " Query for EventLogTable is " + readQuery);

                pgc.OpenPostGreConnection(remote_host, user, password, mcdb_name);

                command = new NpgsqlCommand();

                readReader = pgc.ExecutePostGreQuery(readQuery, ref command);

                //if (readReader.Read() != false)
                //{
                //    L.Log(LogType.FILE, Log.LogLevel.INFORM, "timer_Tick() ----> Query Which Was Executed Bring Data");
                //}

                int i = 0;
                while (readReader.Read())
                {
                    rec.LogName = "NetCadPostGre Recorder";

                    string date = "";
                    string time = "";
                    string date_time = "";

                    date = readReader[0].ToString();
                    time = readReader[1].ToString();

                    string saat = time.Split(' ')[0].Trim();
                    string zamanDilimi = time.Split(' ')[1].Trim();

                    if (zamanDilimi == "AM")
                        time = saat + ":00";
                    else
                        time = (Convert.ToInt32(saat.Split(':')[0]) + 12).ToString() + ":" + saat.Split(':')[1] + ":00";


                    date = date.Split('/')[1] + "/" + date.Split('/')[0] + "/" + date.Split('/')[2];
                    date_time = date + " " + time;

                    try
                    {
                        rec.Datetime = Convert.ToDateTime(date_time).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    }
                    catch (Exception ex)
                    {
                        rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing kayit_tarih and kayit_saat " + ex.Message);
                    }

                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.Datetime is : " +rec.Datetime);

                    string kullaniciIP = "";
                    kullaniciIP = readReader[2].ToString();
                    try
                    {
                        rec.CustomStr1 = System.Net.IPAddress.Parse(kullaniciIP).ToString();
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing kullanici_ip " + ex.Message);
                        rec.CustomStr1 = kullaniciIP;
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> kullnici_ip olduðu gibi atandý. " + kullaniciIP);
                    }

                    // L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.CustomStr1 is (kullanici_ip) : " +rec.CustomStr1);

                    string kullaniciAdý = "";
                    kullaniciAdý = readReader[3].ToString();

                    try
                    {
                        rec.UserName = kullaniciAdý;
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing kullanici_name " + ex.Message);
                    }
                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.UserName is (kullanici_name) : " + rec.UserName);

                    string tableName = "";
                    tableName = readReader[4].ToString();

                    try
                    {
                        L.Log(LogType.FILE, Log.LogLevel.INFORM, "CustomStr7 Is : " + tableName);
                        rec.CustomStr7 = tableName;
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing table_name " + ex.Message);
                    }
                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.CustomStr7 (table_name) is : " + rec.CustomStr7);

                    string url = "";
                    url = readReader[7].ToString();
                    try
                    {
                        rec.CustomStr6 = url;
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing url " + ex.Message);
                    }
                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.CustomStr6 (url) is : " + rec.CustomStr6);

                    string tempCurrentData = "";
                    tempCurrentData = readReader[6].ToString();
                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, " (current_data) is : " + tempCurrentData);       
                    try
                    {
                        string[] permanentCurrentData = new string[4];
                        for (int f = 0; f < permanentCurrentData.Length; f++)
                        {
                            permanentCurrentData[f] = "";
                        }
                        
                        if (tempCurrentData != "")
                        {
                            permanentCurrentData = parseCurrentData(tempCurrentData);
                        }

                        if (permanentCurrentData[0].Length > 900)
                        {
                            rec.CustomStr9 = permanentCurrentData[0].Substring(0,850);
                            rec.CustomStr10 = permanentCurrentData[0].Substring(851, 850);
                        }
                        else 
                        {
                            rec.CustomStr9 = permanentCurrentData[0];
                        }

                        rec.CustomStr4 = permanentCurrentData[1];
                        rec.CustomStr5 = permanentCurrentData[2];
                        rec.CustomStr8 = permanentCurrentData[3];

                        L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.CustomStr4 Is : " + rec.CustomStr4);
                        L.Log(LogType.FILE, Log.LogLevel.INFORM, "rec.CustomStr5 Is : " + rec.CustomStr5);

                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, Log.LogLevel.ERROR, "In timer_Tick()-->> An Error Occured While Parsing current_data " + ex.Message);
                    }

                    string tempPrevData = "";
                    tempPrevData = readReader[5].ToString();
                    //L.Log(LogType.FILE, Log.LogLevel.INFORM, " (tempPrevData) is : " + tempPrevData);
                    string permanentPrevData = "";
                    if (tempPrevData != "")
                    {
                        permanentPrevData = parsePrevData(tempPrevData);
                    }

                    rec.CustomStr2 = permanentPrevData;

                    long recordID = Convert.ToInt64(readReader[8].ToString());

                    L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Start sending Data");

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

                    L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Finish Sending Data");

                    last_position = recordID;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Record Number is " + last_position.ToString());
                    i++;
                    if (i > max_record_send)
                    {
                        command.Cancel();
                        L.Log(LogType.FILE, Log.LogLevel.DEBUG, "max_record_send < " + i.ToString() + " and command canceled");
                        return;
                    }
                    lastDb = mcdb_name;
                    if (usingRegistry)
                        SetNetCadPostGre_Registry(last_position.ToString());
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_position.ToString(), "", lastDb, "", last_recdate);
                    }
                }
                L.Log(LogType.FILE, Log.LogLevel.DEBUG, "Finish getting the data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, Log.LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer.Enabled = true;
                L.Log(LogType.FILE, Log.LogLevel.INFORM, "Service Stopped");

                if (command != null)
                {
                    command.Dispose();
                }
                pgc.ClosePostGreConnection();
            }
        }

        private string[] parseCurrentData(string tempData)
        {
            string[] returnData = new string[4];

            string current_data = "";
            string current_data_ExceptionName = "";
            string current_data_ExceptionMessage = "";
            string current_data_ExceptionStack = "";

            try
            {
                if (tempData.Contains("EX") || tempData.Contains("<MESSAGE>") || tempData.Contains("<STACK>"))
                {
                    int ExBaslangic = tempData.IndexOf('<');
                    int ExBitis = tempData.IndexOf('>');
                    string partEx = "";
                    partEx = tempData.Substring(ExBaslangic, ExBitis - ExBaslangic + 1);
                    if (partEx.Contains("Name"))
                    {
                        current_data_ExceptionName = partEx.Split('=')[1].Trim('<').Trim('>');
                    }

                    int MessageBaslangic = tempData.IndexOf("<MESSAGE>");
                    int MessageBitis = tempData.IndexOf("</MESSAGE>");
                    string partMessage = "";

                    if (MessageBaslangic != -1 && MessageBitis != -1)
                    {
                        partMessage = tempData.Substring((MessageBaslangic + 9), (MessageBitis - (MessageBaslangic + 9)));
                        current_data_ExceptionMessage = partMessage;
                    }

                    int StackBaslangic = tempData.IndexOf("<STACK>");
                    int StackBitis = tempData.IndexOf("</STACK>");
                    string partStack = "";
                    if (StackBaslangic != -1 && StackBitis != -1)
                    {
                        partStack = tempData.Substring((StackBaslangic + 7), (StackBitis - (StackBaslangic + 7)));
                        current_data_ExceptionStack = partStack;
                    }
                }
                if (tempData.Contains("<ROW>"))
                {
                    int indexofEqual = 0;
                    int indexofLitle = 0;
                    int indexofBig = 0;
                    string propertyName = "";
                    string propertyValue = "";

                    while (indexofEqual != -1)
                    {
                        indexofEqual = tempData.IndexOf('=', indexofEqual);
                        if (indexofEqual != -1)
                        {
                            indexofLitle = tempData.IndexOf('>', indexofEqual);
                            indexofBig = tempData.IndexOf('<', indexofEqual);
                            propertyName = tempData.Substring(indexofEqual + 1, indexofLitle - indexofEqual - 1);
                            indexofEqual = indexofBig;
                            while (propertyName.Contains("\""))
                            {
                                propertyName = propertyName.Trim('\"');
                            }

                            propertyValue = tempData.Substring(indexofLitle + 1, indexofBig - indexofLitle - 1);
                            current_data += propertyName + " = " + propertyValue + "; ";
                        }
                    }
                    current_data = current_data.Trim(';');
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parsePrevData -->> CurrentData Is :" + tempData);
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parseCurrentData -->> " + ex.Message);
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parseCurrentData -->> " + ex.StackTrace);
            }

                returnData[0] = current_data;
                returnData[1] = current_data_ExceptionName;
                returnData[2] = current_data_ExceptionMessage;
                returnData[3] = current_data_ExceptionStack;
             
                return returnData;
        }

        private string parsePrevData(string tempData)
        {   

            string prev_data = "";

            try
            {
                if (tempData.Contains("<ROW>"))
                {
                    int indexofEqual = 0;
                    int indexofLitle = 0;
                    int indexofBig = 0;
                    string propertyName = "";
                    string propertyValue = "";

                    while (indexofEqual != -1)
                    {
                        indexofEqual = tempData.IndexOf('=', indexofEqual);
                        if (indexofEqual != -1)
                        {
                            indexofLitle = tempData.IndexOf('>', indexofEqual);
                            indexofBig = tempData.IndexOf('<', indexofEqual);
                            propertyName = tempData.Substring(indexofEqual + 1, indexofLitle - indexofEqual - 1);
                            indexofEqual = indexofBig;
                            while (propertyName.Contains("\""))
                            {
                                propertyName = propertyName.Trim('\"');
                            }

                            propertyValue = tempData.Substring(indexofLitle + 1, indexofBig - indexofLitle - 1);
                            prev_data += propertyName + " = " + propertyValue + "; ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parsePrevData -->> PrevData Is :" + tempData);
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parsePrevData -->> " + ex.Message);
                L.Log(LogType.FILE, Log.LogLevel.ERROR, "In parsePrevData -->> " + ex.StackTrace);
            }

            return prev_data;
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
                            L.SetLogLevel(Log.LogLevel.NONE);
                        } break;
                    case 1:
                        {
                            L.SetLogLevel(Log.LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            L.SetLogLevel(Log.LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            L.SetLogLevel(Log.LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            L.SetLogLevel(Log.LogLevel.DEBUG);
                        } break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
