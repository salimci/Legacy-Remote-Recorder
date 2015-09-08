/*
 * Modified from WebsenceRecorder
 * Modified by Emin Karaca
 * 27/08/2008
 * 
 * */
using System;
using System.Collections.Generic;
using System.Text;
using CustomTools;
//using System.ServiceProcess;
using System.Threading;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;

namespace IsaWebDbRecorder
{
    public class IsaWebDbRecorder : CustomBase
    {
        private System.Timers.Timer timer1;         //belirlenen zaman araliklari ile db ye baglanip almak icin
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private string lastRecordDate = "";     //son recordu bilebilmek icin 
        private int repeat_recordnum=0;        //tarihin tekrarlanma sayisi (hangi rekordda kaldigini bulmak icin)
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, isaDb_name = "isalogdb", isaTable_name = "WebProxyLog",location, user, password, remote_host = "";  //>>>>>
        protected string Virtualhost = "",Dal;
        //>>>> isadb name = isa database in ismi, isatable name = web proxy logunun oldugu table
        
        String[] lastrecord = null;         //en son gonderilen datanin tarihi
            
        private int x = 0;
        private string dates = "2000/08/19 12:03:48.867";
        private bool reg_flag = false;
        private CLogger L;
        int repeat=0;
        private bool begining = true;
        private int max_rec_send2 = 100;
        int identity;
        private bool usingRegistery = true;
        private bool fromEnd = false;
        //databse reader
        string readQuery = null;
        IDataReader readReader = null;
        DbCommand cmd = null; 

        public override void Init()
        {
            if (usingRegistery)
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Websense Recorder functions may not be running");
                            return;
                        }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start creating IsaWebDb DAL");

                    reg_flag = true;
                    //Database.CreateDatabase();

                    isaDb_name = "IsaWebDb" + identity.ToString();
                    if (Database.AddConnection(isaDb_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create IsaWebDb DAL");
                    else
                        L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating IsaWebDb DAL");
                }
            }
        }
        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        public bool get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\IsaWebDbRecorder" + identity + ".log";
                return true;
            }
            catch (Exception ess)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "" + ess.ToString());
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
            String CustomVar1, Int32 CustomVar2, String virtualhost,String dal,Int32 Zone)
        {
            trc_level = TraceLevel;
            timer_interval = SleepTime;
            max_record_send = MaxLineToWait;
            lastRecordDate = LastPosition;
            identity = Identity;
            location = Location;
            usingRegistery = false;
            //err_log = Location;
            //isaDb_name = User;

            Virtualhost = virtualhost;
            Dal = dal;
            //logging_interval = Convert.ToUInt32(CustomVar2);
            //log_size = Convert.ToUInt32(LastKeywords);
            isaTable_name = CustomVar1;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            fromEnd = FromEndOnLoss;
            zone = Zone;
            //>>>> isadb name = isa database in ismi, isatable name = web proxy logunun oldugu table
        }
        
        public IsaWebDbRecorder()
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\IsaWebDbRecorder.log";
                isaDb_name = rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("DBName").ToString();
                isaTable_name = rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("DTNAME").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("MaxRecordSend"));
                //en son hangi recordda kaldigimizi bulmak icin
                lastRecordDate = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("IsaWebDbRecorder").GetValue("LastRecordDate"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Isa Web Database Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            String datet;
            if (usingRegistery)
            {
                s = base.GetInstanceService("Security Manager Sender");
            }
            else
            {
                s = base.GetInstanceService("Security Manager Remote Recorder");
            }

            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
                       
            try
            {
                // Fill the record fileds with necessary parameters 
                //readQuery = "SELECT UPPER(HOST_NAME) AS HOST_NAME FROM NODE WHERE LAST_UPDATED < (getdate() - CONVERT(datetime,'" + respond_hour + ":" + respond_time + ":0',108)) ORDER BY LAST_UPDATED DESC";
                L.Log(LogType.FILE, LogLevel.DEBUG, "Starting the timer");
                if (!reg_flag)
                {
                    if (usingRegistery)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                    }
                    
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Isa Web Database Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + isaDb_name);
                
                
                if (begining)
                {
                    max_rec_send2 = max_record_send;
                    begining = false;
                }

                int i = 0;      //okunan row sayisi
                
                //get date and repeat number separetly
                splitDate();

                /*
                * date max_record_send sayisindan fazla repeat ederse sonsuz loopa giriyor 
                * onu onlemek icin repeat sayisi max_recordu gecerse 
                * max record sayisina repeat sayisini ekliyorum
                * daha sonra eski haline tekrar getiriyorum
                * */
                if (repeat_recordnum >= max_record_send)
                {
                    max_record_send += repeat_recordnum;
                }
                else
                {
                    max_record_send = max_rec_send2;

                }

                //[AuthenticationServer] query'den kaldýrýldý
                //database query si = belli tarihten sonra ilk max_record_send tane recordu getiriyor
                readQuery = "select TOP(" + max_record_send + ")";
                        readQuery += "[ClientIP],[ClientUserName],[ClientAgent],";
                        readQuery += "[ClientAuthenticate],[logTime],[service],[servername],[referredserver],";
                        readQuery += "[DestHost],[DestHostIP],[DestHostPort],[processingtime],[bytesrecvd],";
                        readQuery += "[bytessent],[protocol],[transport],[operation],[uri],[mimetype],[objectsource],";
                        readQuery += "[resultcode],[CacheInfo],[rule],[FilterInfo],[SrcNetwork],[DstNetwork],[ErrorInfo],";
                        readQuery += "[Action],[GmtLogTime]";
                        readQuery += "FROM " + isaTable_name + "(nolock) ";
                        readQuery += "WHERE logTime >= CONVERT(datetime, '" + dates + "',102) ORDER BY logTime";
                
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);

                try
                {
                    readReader = Database.ExecuteReader(isaDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                    cmd.CommandTimeout = 1200;
                }
                catch (Exception rr) {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, "Readreader is not working properly" + rr.ToString());
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                //en son tarihten kac kez okunmus
                repeat = repeat_recordnum;

                while (readReader.Read())
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "read reader");
                    if (repeat == 0)
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rec's");
                            rec.LogName = "Isa Web Db Recorder";
                            rec.ComputerName = System.Net.IPAddress.Parse(readReader.GetInt64(0).ToString()).ToString(); //client ip
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting client ip");
                            rec.UserName = readReader.GetString(1).ToString();                           //client user name
                            L.Log(LogType.FILE, LogLevel.DEBUG, "setting c user name");
                            rec.CustomStr1 = readReader.GetString(2).ToString();                              //client agent
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting c. agent");
                            rec.CustomInt1 = Convert.ToInt32(readReader.GetInt16(3));                         //client authentication
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting c. autentication");
                            datet = readReader.GetDateTime(4).ToString("yyyy/MM/dd HH:mm:ss.fff"); 
                            rec.Datetime = readReader.GetDateTime(4).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");               //date                                         
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting date");
                            rec.CustomInt2 = Convert.ToInt32(readReader.GetInt16(5));                               //service
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting service");
                            rec.SourceName = readReader.GetString(6).ToString();                                    //server name
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting server name");
                            //rec.CustomStr5 = readReader.GetString(7).ToString();                                    //referedserver         
                            //L.Log(LogType.FILE, LogLevel.DEBUG, "Setting refferred");
                            rec.CustomStr2 = readReader.GetString(8).ToString();                           //desthost
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst host");
                            rec.CustomStr10 = System.Net.IPAddress.Parse(readReader.GetInt64(9).ToString()).ToString();//desthostip
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst hostip");
                            rec.CustomInt3 = Convert.ToInt32(readReader.GetInt32(10));                                 // desthost port 
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dsthost port");
                            rec.CustomInt4 = Convert.ToInt32(readReader.GetInt32(11));                              //processing time
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting processing time");
                            rec.CustomInt7 = Convert.ToInt64(readReader.GetInt64(12));                              //bytesrcvd
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bytesrcvd");
                            rec.CustomInt6 = Convert.ToInt64(readReader.GetInt64(13));                              //bytessend
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bytessend");
                            rec.CustomStr3 = readReader.GetString(14).ToString();                                      //protocol//
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting protocol");
                            rec.CustomStr4 = readReader.GetString(15).ToString();                               //transport
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting transport");
                            rec.EventCategory = readReader.GetString(16).ToString();                               //operation
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting operation");
                            rec.Description = readReader.GetString(17).ToString();                               //URI
                            if (rec.Description.Length > 899)
                            {
                                rec.Description = rec.Description.Remove(900);
                            }

                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting uri");
                            rec.CustomStr6 = readReader.GetString(18).ToString();                               //mimetype
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting mimetype");

                            rec.CustomInt5 = Convert.ToInt32(readReader.GetInt16(19));                          //objectsourse
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting objectserve");
                            rec.EventId = Convert.ToInt32(readReader.GetInt32(20));                          //resultcode
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting resultcode");
                            rec.EventType = readReader.GetString(22).ToString();                          //rule
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rule");

                            rec.CustomStr7 = readReader.GetString(23).ToString();                               //Filterinfo
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting filterinfo");
                            rec.CustomStr8 = readReader.GetString(24).ToString();                               //src network
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting src network");
                            rec.CustomStr9 = readReader.GetString(25).ToString();                               //dst network
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst network");

                            rec.CustomInt8 = Convert.ToInt32(readReader.GetInt32(26));                          //error info
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting err info");
                            rec.CustomInt9 = Convert.ToInt32(readReader.GetString(27));                         //action
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting action");

                            rec.CustomInt10 = Convert.ToInt32(readReader.GetInt32(21));                          //cacheinfo
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting cache info");

                            switch (rec.CustomInt9)
                            {
                                case 6:
                                    rec.CustomStr5 = "Establish";
                                    break;
                                case 7:
                                    rec.CustomStr5 = "Terminate";
                                    break;
                                case 8:
                                    rec.CustomStr5 = "Denied";
                                    break;
                                case 9:
                                    rec.CustomStr5 = "Allowed";
                                    break;
                                case 10:
                                    rec.CustomStr5 = "Failed";
                                    break;
                                case 11:
                                    rec.CustomStr5 = "Intermediate";
                                    break;
                                case 12:
                                    rec.CustomStr5 = "Succesfull Connection";
                                    break;
                                case 13:
                                    rec.CustomStr5 = "Unsuccesfull Connection";
                                    break;
                                case 14:
                                    rec.CustomStr5 = "Disconnect";
                                    break;
                                default:
                                    rec.CustomStr5 = "";
                                    break;
                            }
                        }
                        catch (Exception bd)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Bad data recieved" + bd.ToString());
                            continue;
                        }
                        
                        //eyer tarih bir onceki tarih ile ayni ise repeat numberi artir
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Lastrecord : " + dates + " " + datet.ToString());
                        if (dates == datet)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Repeat the date");
                            repeat_recordnum++;
                        }
                        else //deilse last recor olarak date i set et
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Go for last record");
                            dates = datet;
                            repeat_recordnum = 0;
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                        if (usingRegistery)
                        {
                            s.SetData(rec);     //datayi gonder
                        }
                        else
                        {
                            s.SetData(Dal,Virtualhost,rec);     //datayi gonder
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                        // repeat_recordnum = rec.CustomInt6;
                        //bakilan en son record                        
                        lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + lastRecordDate);
                        i++;
                        if (i > max_record_send ||i%100 == 0)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting last date");
                            //son bakilan recordu registerye kaydet
                            Set_Registry(lastRecordDate,rec.Datetime);                            
                            //  lastRecordDate += "%" + repeat_recordnum.ToString();                            
                        }
                    }//end of if
                    else
                    {
                        repeat -= 1;
                    }
                }
                cmd.Cancel();
                ////son bakilan recordu registerye kaydet
                //lastRecordDate += "%" + repeat_recordnum.ToString();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Set last date to registery");
                Set_Registry(lastRecordDate,rec.Datetime);


                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");

            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
                Database.Drop(ref cmd);
                s.Dispose();
            }
        }
        public bool Set_Registry(String status,string lastrecdate)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistery)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("IsaWebDbRecorder");
                    rk.SetValue("lastRecordDate", status);
                    rk.Close();
                    return true;
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(identity, status, "","","", lastrecdate);
                    return true;
                }

                
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Isa Web Db Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

       
        /*
        * ////////////////////
        * database den belli tarihten sonrasini aliyor
        * repeat_number tarihin sonuna eklendigi icin ilk once onu ayiriyoruz
        * /////////////////////
        * */
        public void splitDate()
        {
            try
            {
                if (fromEnd)
                {
                    try
                    {
                        readQuery = "select MAX(logTime)";
                        readQuery += "FROM " + isaTable_name;

                        readReader = Database.ExecuteReader(isaDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                        cmd.CommandTimeout = 1200;

                        dates = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        repeat_recordnum = 0;
                        lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                    }
                    catch (Exception DE)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR at reading from end" + DE.ToString());
                    }
                }
                else
                {
                    if (lastRecordDate.Equals("no data"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " initialize the date");                        
                        lastRecordDate = "2000/08/19 12:03:48.867%0";
                    }
                    bool check = lastRecordDate.ToString().Contains("%");
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Split date");

                    if (check)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "splitting");
                        lastrecord = lastRecordDate.Split('%');

                        if (lastrecord[1].Equals(""))
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Repeat record number was null setting it to 0");
                            repeat_recordnum = 0;
                            dates = lastrecord[0];
                            lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                        }//inner if
                        else
                        {
                            repeat_recordnum = Convert.ToInt32(lastrecord[1]);
                            //lastRecordDate = lastrecord[0];
                            dates = lastrecord[0];
                            //lastRecordDate = dates + "%" + repeat_recordnum;
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Split succesfull");
                        }//else
                    }//if
                    else
                    {
                        if (lastRecordDate.Equals("0") | lastRecordDate.Equals(""))
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "repeat record number is null setting it to 0");
                            lastRecordDate = "2000/08/19 12:03:48.867%0";
                            splitDate();
                        }//if
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on lastrecord registry starts from the beginning");
                            repeat_recordnum = 0;
                            dates = lastRecordDate;
                            lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                        }//else

                    }//else

                }//else === fromend=false
            }
            catch (Exception sp)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at spliting the date " + sp.ToString());
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
                EventLog.WriteEntry("Security Manager Isa Web Database Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
