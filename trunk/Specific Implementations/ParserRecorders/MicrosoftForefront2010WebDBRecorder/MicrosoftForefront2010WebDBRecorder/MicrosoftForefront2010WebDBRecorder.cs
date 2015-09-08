using System;
using System.Collections.Generic;
using System.Text;
using CustomTools;
using System.Threading;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;
using System.Collections;

namespace MicrosoftForefront2010WebDBRecorder
{
    public class MicrosoftForefront2010WebDBRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 5000, max_record_send = 100, zone = 0;
        private string lastRecordDate = "";
        private int repeat_recordnum = 0, option = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, ffdb_namepart1 = "ISALOG", ffdb_namepart2 = "WEB", db_name = "", ffdb_name_last = "", forefrontDb_name = "", forefrontTable_name = "", location, user, password, remote_host = "";
        protected string Virtualhost = "", Dal;

        public bool controlofdoublerecord = true;

        private string prevDate = null;
        private string curDate = null;

        private bool reg_flag = false;
        private CLogger L;
        int repeat = 0;
        private bool begining = true;
        private int max_rec_send2 = 100;
        int identity = -1;
        private bool usingRegistery = true;
        private bool fromEnd = false;

        public override void Init()
        {
            if (usingRegistery)
            {
                if (!reg_flag)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "   Error on Reading the Registry");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "   Error on Intialize Logger functions may not be running");
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
                    if (!get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "   Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "   Error on Intialize Logger functions may not be running");
                            return;
                        }

                    reg_flag = true;
                    db_name = "ffdb" + identity.ToString();
                    if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, "master"))
                        L.Log(LogType.FILE, LogLevel.INFORM, "  Successfully create MicrosoftForefront2010WEBDBRecorder DAL");
                    else
                        L.Log(LogType.FILE, LogLevel.INFORM, "  Failed on creating MicrosoftForefront2010WEBDBRecorder DAL");
                }
            }
        }

        public bool get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\MicrosoftForefront2010WebDBRecorder" + identity + ".log";
                return true;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "" + ex.ToString());
                return false;
            }
        }

        public override void Start()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, string LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {
            trc_level = TraceLevel;
            timer_interval = SleepTime;
            max_record_send = MaxLineToWait;
            lastRecordDate = LastPosition;
            identity = Identity;
            location = Location;
            usingRegistery = false;

            if (CustomVar2 != 1)
            {
                option = 0;
            }
            else
            {
                option = 1;
            }

            ffdb_name_last = LastFile;
            forefrontDb_name = location;
            forefrontTable_name = CustomVar1;
            Virtualhost = virtualhost;
            Dal = dal;

            if (LastKeywords == null | LastKeywords == "" | LastKeywords == " ")
                repeat_recordnum = 0;
            else
                repeat_recordnum = Convert.ToInt32(LastKeywords);

            user = User;
            password = Password;
            remote_host = RemoteHost;
            fromEnd = FromEndOnLoss;
            zone = Zone;
        }   

        public MicrosoftForefront2010WebDBRecorder()
        {
           
        }

        private void InitializeComponent()
        {

        }
            
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {   
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\MicrosoftForefront2010WebDBRecorder.log";
                forefrontDb_name= rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("DBName").ToString();
                forefrontTable_name = rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("DTNAME").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("MaxRecordSend"));
                //en son hangi recordda kaldigimizi bulmak icin
                lastRecordDate = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("MicrosoftForefront2010WebDBRecorder").GetValue("LastRecordDate"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Microsoft Fore Front 2010 Web DB Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
            
        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            CustomServiceBase s;
            string readQuery2 = null;
            IDataReader readReader2 = null;
            DbCommand cmd2 = null;
            bool controlofadd = false;
            bool erroroccured = false;

            if (usingRegistery)
            {
                s = base.GetInstanceService("Security Manager Sender");
            }
            else
            {
                s = base.GetInstanceService("Security Manager Remote Recorder");
            }

            L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Service Started");
            int x = 0;
            try
            {
                if (begining)
                {
                    max_rec_send2 = max_record_send;
                    begining = false;
                }

                int i = 0;

                prevDate = curDate;
                
                ffdb_name_last = Get_Ff_Dbname();

                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> ffdb_name_last " + ffdb_name_last.ToString());
                if (checkDatabaseExists(ffdb_name_last))
                {
                    #region MyRegion
                    if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0")
                    {
                        setLastRecordDate();
                    }

                    curDate = lastRecordDate;

                    //L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> curDate " + curDate);
                    /*
                    * date max_record_send sayisindan fazla repeat ederse sonsuz loopa giriyor 
                    * onu onlemek icin repeat sayisi max_recordu gecerse 
                    * max record sayisina repeat sayisini ekliyorum
                    * daha sonra eski haline tekrar getiriyorum
                    * */

                    if (controlofdoublerecord)
                    {
                        repeat = repeat_recordnum + 1;
                        repeat_recordnum = repeat;
                        controlofadd = true;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Bir arttýrýldý");
                    }
                    else
                    {
                        repeat = repeat_recordnum;
                        controlofdoublerecord = true;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Ýlk defa girdi ayný kaldý 0");
                    }

                    if (repeat_recordnum >= max_record_send)
                    {
                        max_record_send += repeat_recordnum;

                        L.Log(LogType.FILE, LogLevel.DEBUG, " Record Number azaltýldý");

                        repeat = repeat_recordnum - 1;
                        repeat_recordnum = repeat;

                        L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> max record sent " + max_record_send);
                    }
                    else
                    {
                        max_record_send = max_rec_send2;
                    }

                    readQuery2 = "Select TOP " + max_record_send + " ";
                    readQuery2 += "[logTime],[servername],[Action],[ClientUserName],[protocol],[resultcode],ClientIP,";
                    readQuery2 += "DestHostIP,[rule],[SrcNetwork],[DstNetwork],[ClientAgent],[operation],[uri],[SrcPort],";
                    readQuery2 += "[DestHostPort],[processingtime],[bytessent],[bytesrecvd],[ClientAuthenticate],[FilterInfo],[referredserver] ";
                    readQuery2 += "FROM " + ffdb_name_last + ".." + forefrontTable_name + "(nolock) ";
                    readQuery2 += "WHERE logTime >= '" + curDate + "' ORDER BY logTime";

                    L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Query is " + readQuery2);

                    try
                    {

                        readReader2 = Database.ExecuteReader(db_name, readQuery2, CommandBehavior.CloseConnection, out cmd2);
                        cmd2.CommandTimeout = 1200;
                    }
                    catch (Exception ex)
                    {
                        erroroccured = true;
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 4 : " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 4 : " + ex.StackTrace);
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Finish executing the query");

                    L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Repeat Record Num Is : " + repeat.ToString());

                    #region while
                    if (readReader2 != null)
                    {
                        while (readReader2.Read())
                        {
                            if (repeat < 0)
                            {
                                repeat = 0;
                            }
                            if (repeat == 0)
                            {
                                rec.LogName = "Forefront 2010 WEB";

                                prevDate = curDate;

                                try
                                {
                                    if (!readReader2.IsDBNull(0))
                                    {
                                        curDate = readReader2.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff"); //lodTime
                                        rec.Datetime = curDate;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 15 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 15 " + ex.StackTrace.ToString());
                                }


                                try
                                {
                                    if (!readReader2.IsDBNull(1))
                                    {
                                        rec.ComputerName = readReader2.GetString(1);              // servername 
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 12 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 12 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>servername  IS " + rec.ComputerName);

                                try
                                {
                                    if (!readReader2.IsDBNull(2))
                                    {
                                        string actionnumber = readReader2.GetInt16(2).ToString(); //Action
                                        rec.EventType = ActionText(actionnumber);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 13 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 13 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>Action  IS " + rec.EventType);


                                try
                                {
                                    if (!readReader2.IsDBNull(3))
                                    {
                                        rec.SourceName = readReader2.GetString(3);   //ClientUserName
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 14 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 14 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>ClientUserName  IS " + rec.SourceName);

                                try
                                {
                                    if (!readReader2.IsDBNull(4))
                                    {
                                        rec.CustomStr1 = readReader2.GetString(4);     //protocol
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 16 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 16 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>protocol  IS " + rec.CustomStr1);

                                try
                                {
                                    if (!readReader2.IsDBNull(5))
                                    {
                                        rec.CustomInt5 = readReader2.GetInt32(5);   //resultcode 
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 17 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 17 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>resultcode  IS " + rec.CustomStr2);

                                try
                                {
                                    if (!readReader2.IsDBNull(6))
                                    {
                                        rec.CustomStr3 = ResolveIp(readReader2.GetValue(6).ToString());  //ClientIP 
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 18 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 18 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>ClientIP  IS " + rec.CustomStr3);

                                try
                                {
                                    if (!readReader2.IsDBNull(7))
                                    {
                                        rec.CustomStr4 = ResolveIp(readReader2.GetValue(7).ToString());  //DestHostIP  
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 19 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 19 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>DestHostIP  IS " + rec.CustomStr4);

                                try
                                {
                                    if (!readReader2.IsDBNull(8))
                                    {
                                        rec.CustomStr5 = readReader2.GetString(8).ToString();  //rule  
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 20 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 20 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>rule  IS " + rec.CustomStr5);

                                try
                                {
                                    if (!readReader2.IsDBNull(9))
                                    {
                                        rec.CustomStr6 = readReader2.GetString(9).ToString();  // SrcNetwork
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 21 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 21 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>SrcNetwork  IS " + rec.CustomStr6);

                                try
                                {
                                    if (!readReader2.IsDBNull(10))
                                    {
                                        rec.CustomStr7 = readReader2.GetString(10).ToString();  // DstNetwork
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 22 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 22 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>DstNetwork  IS " + rec.CustomStr7);

                                try
                                {
                                    if (!readReader2.IsDBNull(11))
                                    {
                                        rec.CustomStr8 = readReader2.GetString(11).ToString();  // ClientAgent
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 23 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 23 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>ClientAgent  IS " + rec.CustomStr8);

                                try
                                {
                                    if (!readReader2.IsDBNull(12))
                                    {
                                        rec.CustomStr9 = readReader2.GetString(12).ToString();  // operation
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 24 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 24 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->operation  IS " + rec.CustomStr9);


                                try
                                {
                                    if (!readReader2.IsDBNull(13))
                                    {
                                        string tempdata = readReader2.GetString(13).ToString();   // uri
                                        if (tempdata.Length > 900)
                                        {
                                            rec.CustomStr10 = tempdata.Substring(0, 900);
                                        }
                                        else
                                        {
                                            rec.CustomStr10 = tempdata;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 25 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 25 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->uri  IS " + rec.CustomStr9);

                                try
                                {
                                    if (!readReader2.IsDBNull(14))
                                    {
                                        rec.CustomInt1 = readReader2.GetInt32(14);  // SrcPort
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 26 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 26 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->SrcPort  IS " + rec.CustomInt1.ToString());

                                try
                                {
                                    if (!readReader2.IsDBNull(15))
                                    {
                                        rec.CustomInt2 = readReader2.GetInt32(15);  // DestHostPort
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 27 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 27 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->DestHostPort  IS " + rec.CustomInt2.ToString());


                                try
                                {
                                    if (!readReader2.IsDBNull(16))
                                    {
                                        rec.CustomInt4 = readReader2.GetInt32(16);  // processingTime
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 28 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 28 " + ex.StackTrace.ToString());

                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> processingTime  IS " + rec.CustomInt4.ToString());

                                try
                                {
                                    if (!readReader2.IsDBNull(17))
                                    {
                                        rec.CustomInt8 = readReader2.GetInt64(17);  // bytessent
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 29 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 29 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> bytessent  IS " + rec.CustomInt5.ToString());

                                try
                                {
                                    if (!readReader2.IsDBNull(18))
                                    {
                                        rec.CustomInt6 = readReader2.GetInt64(18);  //bytesrecvd
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 30 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 30 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> bytesrecvd IS " + rec.CustomInt6.ToString());

                                try
                                {
                                    if (!readReader2.IsDBNull(19))
                                    {
                                        rec.CustomInt7 = Convert.ToInt64(readReader2.GetInt16(19));  // ClientAuthenticate
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 31 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 31 " + ex.StackTrace.ToString());
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> ClientAuthenticate  IS " + rec.CustomInt7.ToString());

                                try
                                {
                                    if (!readReader2.IsDBNull(20))
                                    {
                                        rec.Description = readReader2.GetString(20);  // FilterInfo
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 32 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 32 " + ex.StackTrace.ToString());
                                }
                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> FilterInfo  IS " + rec.Description);

                                try
                                {
                                    if (!readReader2.IsDBNull(21))
                                    {
                                        rec.CustomStr2 = readReader2.GetString(21);  // referredserver
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 33 " + ex.Message.ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 33 " + ex.StackTrace.ToString());
                                }
                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() --> referredserver  IS " + rec.CustomStr2);

                                if (prevDate == curDate)
                                {
                                    repeat_recordnum++;
                                }
                                else
                                {
                                    repeat_recordnum = 0;
                                }

                                if (usingRegistery)
                                {
                                    s.SetData(rec);
                                    Set_Registry(curDate);
                                }
                                else
                                {
                                    s.SetData(Dal, Virtualhost, rec);
                                    Set_Registry(curDate);
                                }

                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Finish Sending Data");
                                x++;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Number of Sending Data : " + x.ToString());

                                lastRecordDate = curDate;

                                i++;
                                if (i > max_record_send)
                                {
                                    //son bakilan recordu registerye kaydet
                                    Set_Registry(lastRecordDate);
                                    cmd2.Cancel();
                                    return;
                                }
                            }
                            else
                            {
                                repeat -= 1;
                            }
                        }

                        Set_Registry(lastRecordDate);
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> Database is not exist " + ffdb_name_last.ToString());
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 6 " + er.Message.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 6 " + er.StackTrace.ToString());
                erroroccured = true;
            }
            finally
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> In Finaly " + x + " record was sent is : ");  

                if (x == 0 && controlofadd && !erroroccured)
                {
                    repeat_recordnum--;
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In timer1_Tick() -->> There Is No New Record So Waiting For New Record");
                    if (option == 0)
                    {
                        ffdb_name_last = changeDatabase(ffdb_name_last);
                    }
                }

                if (x == 0 && controlofadd && erroroccured)
                {
                    repeat_recordnum--;
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In timer1_Tick() -->> An Error Occured And Repeat Record Num -- : " + repeat_recordnum.ToString());
                }

                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "  In timer1_Tick() -->> Service Stopped");
                Database.Drop(ref cmd2);
                s.Dispose();
            }
        }

        private string ActionText(string actionnumber)
        {
            string actiontext = "";

            switch (actionnumber)
            {
                case "0": { actiontext = "fpcActionNotLogged"; } break;
                case "1": { actiontext = "fpcActionBind"; } break;
                case "2": { actiontext = "fpcActionListen"; } break;
                case "3": { actiontext = "fpcActionGHBN"; } break;
                case "4": { actiontext = "fpcActionGHBA"; } break;
                case "5": { actiontext = "fpcActionRedirectBind"; } break;
                case "6": { actiontext = "fpcActionEstablish"; } break;
                case "7": { actiontext = "fpcActionTerminate"; } break;
                case "8": { actiontext = "fpcActionDenied"; } break;
                case "9": { actiontext = "fpcActionAllowed"; } break;
                case "10": { actiontext = "fpcActionFailed"; } break;
                case "11": { actiontext = "fpcActionIntermediate"; } break;
                case "12": { actiontext = "fpcActionSuccessfulConnection"; } break;
                case "13": { actiontext = "fpcActionUnsuccessfulConnection"; } break;
                case "14": { actiontext = "fpcActionDisconnection"; } break;
                case "15": { actiontext = "fpcActionUserclearedQuarantine"; } break;
                case "16": { actiontext = "fpcActionQuarantinetimeout"; } break;
                default:
                    actiontext = "0";
                    break;
            }
            return actiontext;
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        public bool Set_Registry(String status)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistery)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("MicrosoftForefront2010FwDBRecorder");
                    rk.SetValue("lastRecordDate", status);
                    rk.Close();
                    return true;
                }
                else
                {
                    if (option == 0)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(identity, status, " ", ffdb_name_last, repeat_recordnum.ToString(), status);
                        return true;
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(identity, status, " ", "", repeat_recordnum.ToString(), status);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "In Set_Registry In Catch 1 " + ex.Message.ToString());
                EventLog.WriteEntry("In Set_Registry In Catch 1 ", ex.Message.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public void setLastRecordDate()
        {
            string readQuery = null;
            IDataReader readReader = null;
            DbCommand cmd = null;

            if (fromEnd)
            {
                if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0")
                {
                    try
                    {
                        readQuery = "select MAX(logTime)";
                        readQuery += " FROM " + ffdb_name_last + ".." + forefrontTable_name;

                        readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                        cmd.CommandTimeout = 1200;
                        while (readReader.Read())
                        {
                            lastRecordDate = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                            controlofdoublerecord = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate In Catch 1 " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate In Catch 1 " + ex.StackTrace);
                    }
                    finally
                    {
                        if (readReader != null)
                            readReader.Dispose();
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0")
                {
                    try
                    {
                        readQuery = "select MIN(logTime)";
                        readQuery += " FROM " + ffdb_name_last + ".." + forefrontTable_name;

                        L.Log(LogType.FILE, LogLevel.INFORM, "  In timer1_Tick() -->> Query33333 is " + readQuery);

                        readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                        cmd.CommandTimeout = 1200;
                        while (readReader.Read())
                        {
                            lastRecordDate = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                            controlofdoublerecord = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate In Catch 1 " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate In Catch 1 " + ex.StackTrace); ;
                    }
                    finally
                    {
                        if (readReader != null)
                            readReader.Dispose();
                    }
                }
            }

            L.Log(LogType.FILE, LogLevel.DEBUG, "   In setLastRecordDate -->> Last Record Date Is : " + lastRecordDate);

        }

        private string ResolveIp(string _ip)
        {
            string strInAddress = _ip.ToLower();
            string strOutAddress = "";

            try
            {
                if (strInAddress.Substring(9, 4) == "ffff")
                {//ipv4

                    int IsNum, ZERO, IsAlpa;

                    ZERO = (int)'0';
                    IsNum = (int)'9';
                    IsAlpa = (int)'a' - 10;

                    int intH, intL;
                    intH = (int)(strInAddress.Substring(0, 1).ToCharArray()[0]);

                    if (intH <= IsNum)
                        intH = intH - ZERO;
                    else
                        intH = intH - IsAlpa;

                    intL = (int)(strInAddress.Substring(1, 1).ToCharArray()[0]);

                    if (intL <= IsNum)
                        intL = intL - ZERO;
                    else
                        intL = intL - IsAlpa;

                    strOutAddress = Convert.ToString(intH * 16 + intL) + ".";


                    //------------------------------------------------------------------


                    intH = (int)(strInAddress.Substring(2, 1).ToCharArray()[0]);

                    if (intH <= IsNum)
                        intH = intH - ZERO;
                    else
                        intH = intH - IsAlpa;

                    intL = (int)(strInAddress.Substring(3, 1).ToCharArray()[0]);

                    if (intL <= IsNum)
                        intL = intL - ZERO;
                    else
                        intL = intL - IsAlpa;

                    strOutAddress += Convert.ToString(intH * 16 + intL) + ".";

                    //-----------------------------------------------------------------


                    intH = (int)(strInAddress.Substring(4, 1).ToCharArray()[0]);

                    if (intH <= IsNum)
                        intH = intH - ZERO;
                    else
                        intH = intH - IsAlpa;

                    intL = (int)(strInAddress.Substring(5, 1).ToCharArray()[0]);

                    if (intL <= IsNum)
                        intL = intL - ZERO;
                    else
                        intL = intL - IsAlpa;

                    strOutAddress += Convert.ToString(intH * 16 + intL) + ".";

                    //------------------------------------------------------------------

                    intH = (int)(strInAddress.Substring(6, 1).ToCharArray()[0]);

                    if (intH <= IsNum)
                        intH = intH - ZERO;
                    else
                        intH = intH - IsAlpa;

                    intL = (int)(strInAddress.Substring(7, 1).ToCharArray()[0]);

                    if (intL <= IsNum)
                        intL = intL - ZERO;
                    else
                        intL = intL - IsAlpa;

                    strOutAddress += Convert.ToString(intH * 16 + intL);

                }
                else
                {//ipv6 
                    int aa = strInAddress.Length;
                    strOutAddress = strOutAddress + strInAddress.Substring(0, 4) + ':'
                                    + strInAddress.Substring(4, 4) + ':'
                                    + strInAddress.Substring(9, 4) + ':'
                                    + strInAddress.Substring(14, 4) + ':'
                                    + strInAddress.Substring(19, 4) + ':'
                                    + strInAddress.Substring(24, 4) + ':'
                                    + strInAddress.Substring(28, 4) + ':'
                                    + strInAddress.Substring(32, 4);

                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "   In ResolveIp " + ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "   In ResolveIp " + ex.StackTrace);
            }

            return strOutAddress;

        }

        public string Get_Ff_Dbname()
        {
            IDataReader readReader = null;
            DbCommand cmd = null;
            string dbname = ffdb_name_last;

            if (option == 1)
            {
                dbname = forefrontDb_name;
            }
            else
            {
                if (ffdb_name_last == "" || ffdb_name_last == " " || ffdb_name_last == null)
                {
                    if (!string.IsNullOrEmpty(forefrontDb_name))
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM, "  In Get_Ff_Dbname Location is not null and begin with " + forefrontDb_name.ToString());
                        dbname = forefrontDb_name;
                    }
                    else
                    {
                        try
                        {
                            string cmdText = "SELECT NAME FROM master..sysdatabases(nolock) WHERE crdate =  (select MIN(crdate) from master..sysdatabases(nolock) where name like '%" + ffdb_namepart1 + "%' and name like '%" + ffdb_namepart2 + "%')";
                            readReader = Database.ExecuteReader(db_name, cmdText, out cmd);
                            while (readReader.Read())
                            {
                                dbname = readReader.GetString(0).ToString().Trim();
                                L.Log(LogType.FILE, LogLevel.INFORM, "  In Get_Ff_Dbname forefrontDb_name is null so Selected Database Which created date is minimum : " + dbname.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "   In Get_Ff_Dbname " + ex.Message);
                            L.Log(LogType.FILE, LogLevel.ERROR, "   In Get_Ff_Dbname " + ex.StackTrace);
                        }
                        finally
                        {
                            readReader.Close();
                            if (readReader != null)
                                readReader.Dispose();
                        }
                    }
                }
                else
                {
                    bool check = checkDatabaseExists(ffdb_name_last);
                    if (check)
                    {
                        dbname = ffdb_name_last;
                    }
                    else
                    {
                        dbname = changeDatabase(ffdb_name_last);
                    }
                }
            }
            return dbname;
        }

        private string changeDatabase(string ffdb_name_last)
        {
            IDataReader readReader = null;
            DbCommand cmd = null;

            ArrayList dbnames = new ArrayList();
            ArrayList dbnumbers = new ArrayList();
            string newdbName = "";
            string dbname = "";
            string lastdbname = ffdb_name_last;
            long lastdbnumber = parsedbName(lastdbname);

            try
            {
                string cmdText = "SELECT NAME FROM master..sysdatabases(nolock) WHERE name like '%" + ffdb_namepart1 + "%' and name like '%" + ffdb_namepart2 + "%' ORDER BY crdate ASC";
                readReader = Database.ExecuteReader(db_name, cmdText, out cmd);

                L.Log(LogType.FILE, LogLevel.DEBUG, "   In changeDatabase cmdText is " + cmdText);

                while (readReader.Read())
                {
                    dbname = readReader.GetString(0);
                    long db_number = parsedbName(dbname);

                    dbnumbers.Add(db_number);
                    dbnames.Add(dbname);
                }

                bool foundnewDB = false;

                for (int i = 0; i < dbnumbers.Count; i++)
                {
                    if (Convert.ToInt64(dbnumbers[i]) > lastdbnumber)
                    {
                        newdbName = dbnames[i].ToString();
                        foundnewDB = true;
                        break;
                    }
                }

                if (foundnewDB)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In changeDatabase DataBase changed");
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In changeDatabase Last Database Is " + lastdbname);
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In changeDatabase New Database Is " + newdbName);

                    lastRecordDate = "";
                    repeat_recordnum = 0;
                    return newdbName;
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "  In changeDatabase  There is no new database so continiou with sama database");
                    return lastdbname;
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "   In changeDatabase " + ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "   In changeDatabase " + ex.StackTrace);
                return lastdbname;
            }
            finally
            {
                if (readReader != null)
                {
                    readReader.Close();
                    readReader.Dispose();
                }
            }
        }

        private long parsedbName(string dbname)
        {
            long db_number = 0;

            try
            {
                string[] parts = dbname.Split('_');
                string dbdate = parts[1] + parts[3];
                db_number = Convert.ToInt64(dbdate);

            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "   In parsedbName " + ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "   In parsedbName " + ex.StackTrace);
            }

            return db_number;
        }

        public bool tableControl(string tablename, string dbname)
        {
            bool control = false;

            IDataReader readReader = null;
            DbCommand cmd = null;
            string readQuery = "IF EXISTS(SELECT TABLE_NAME FROM " + dbname + "." + "INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tablename + "') BEGIN select 1 AS ControlData" + " END";
            L.Log(LogType.FILE, LogLevel.DEBUG, " Query is in control" + readQuery);
            readReader = Database.ExecuteReader(db_name, readQuery, CommandBehavior.CloseConnection, out cmd);
            cmd.CommandTimeout = 1200;

            while (readReader.Read())
            {
                control = true;
                break;
            }
            return control;
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
                EventLog.WriteEntry("Security Manager Microsoft Fore Front 2010 Fw DB Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public bool checkDatabaseExists(string database)
        {
            IDataReader readReader = null;
            DbCommand cmd = null;

            //string connString = "Data Source=" + server + ";uid="+ user +";pwd="+ password+";Initial Catalog=master;Integrated Security=True;";
            bool bRet = false;
            try
            {
                string cmdText = "select * from master.dbo.sysdatabases where name='" + database + "'";
                readReader = Database.ExecuteReader(db_name, cmdText, out cmd);

                while (readReader.Read())
                {
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "   In checkDatabaseExists : " + ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "   In checkDatabaseExists : " + ex.StackTrace);
            }
            if (bRet)
                L.Log(LogType.FILE, LogLevel.DEBUG, "  In checkDatabaseExists Database " + database + " is exist ");

            L.Log(LogType.FILE, LogLevel.DEBUG, "  In checkDatabaseExists Database " + database + " is NOT exist ");


            return bRet;
        }

    }   
}       
