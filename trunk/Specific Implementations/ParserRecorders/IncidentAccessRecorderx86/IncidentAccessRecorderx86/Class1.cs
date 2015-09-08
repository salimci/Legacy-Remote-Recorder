//
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
using System.Data.OleDb;



namespace IncidentAccessRecorder
{
    public class IncidentAccessx86Recorder : CustomBase
    {
        private System.Timers.Timer timer1;         //belirlenen zaman araliklari ile db ye baglanip almak icin
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100,zone=0;
        private string lastRecordDate = "";     //son recordu bilebilmek icin 
        private int repeat_recordnum=0;        //tarihin tekrarlanma sayisi (hangi rekordda kaldigini bulmak icin)
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, dbName = "scanresults", Table_name = "",location, user, password, remote_host = "";  //>>>>>
        protected string Virtualhost = "",Dal;       
        String[] lastrecord = null;         //en son gonderilen datanin tarihi                    
        private string dates = "2000/08/19 12:03:48";
        private bool reg_flag = false;
        private CLogger L;
        int repeat=0;
        private bool begining = true;
        private int max_rec_send2 = 100;
        int identity;
        private bool usingRegistery = false;
        private bool fromEnd = false;
        //databse reader
        string readQuery = null;
        IDataReader readReader = null;
        DbCommand cmd = null; 
        //DbCommand cmd = null; 

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IncidentAccess Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                    //Database.CreateDatabase();
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Incident Recorder functions may not be running");
                            return;
                        }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start creating IncidentAccess DAL");

                    reg_flag = true;
                    //Database.CreateDatabase();                             
                    //if (Database.AddConnection(dbName, Database.Provider.SQLServer, remote_host, user, password, location))
                    //    L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create Incident DAL");
                    //else
                    //    L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating Incident DAL");
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\IncidentAccessRecorder" + identity + ".log";
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
            Virtualhost = virtualhost;
            Dal = dal;
            //logging_interval = Convert.ToUInt32(CustomVar2);
            //log_size = Convert.ToUInt32(LastKeywords);
            Table_name = CustomVar1;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            fromEnd = FromEndOnLoss;
            zone = Zone;            
        }
        
        public IncidentAccessx86Recorder()
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
                string a = "";
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "1-rk :" + rk.ToString());
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("Log Size"));
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "2-log_size :" + log_size);                
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("Logging Interval"));
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "3-logging_interval:" + logging_interval);
                a = (rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("Trace Level").ToString());
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "4-a :" + a);                
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\IncidentAccessRecorder.log";
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "5-err_log :" + err_log);
                dbName = rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("DBName").ToString();
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "6-dbName :" + dbName);
                Int32 Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("Interval"));
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "7-Interval :" + Interval);
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("MaxRecordSend"));
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "8-max_record_send :" + max_record_send);
                user = (rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("UsernameForAccess").ToString());
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "9-user :" + user);
                password = (rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("PasswordForAccess").ToString());
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "10-password :" + password);
                location = (rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("Location").ToString());
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "11-location :" + location);
                Table_name = (rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("TableName").ToString());
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "12-Table_Name :" + Table_name);
                //en son hangi recordda kaldigimizi bulmak icin
                lastRecordDate = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("IncidentAccessRecorder").GetValue("lastRecordDate"));
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", "13-log_size :" + lastRecordDate);
                rk.Close();
                trc_level = Convert.ToInt32(a);
                logging_interval = 1000;
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Incident Database Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                L.Log(LogType.FILE, LogLevel.INFORM, "Starting the timer");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Incident Access Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, " last dbname is: " + dbName);
                
                
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
                if(dates == "" ||dates == null ||dates == " ")
                {
                    dates = DateTime.MinValue.ToString().Substring(0, 19);
                }                
                //query must be reorganize, scan id filter must be added (last scan id)
                readQuery = "SELECT TOP " + max_record_send + " [_DateTime],[Message],FileName,VirusIncident,State,ScanJobDeleted,VirusIncidentsDeleted,ID,SenderName,SenderAddress,RecipientNames,RecipientAddresses,ccNames,ccAddresses,bccNames,bccAddresses from "
                            + Table_name + " where [_DateTime] > CDATE(\"" + dates + "\") order by [_DateTime] asc";

                
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);
                string connectionString = "";
                OleDbConnection conn = null;
                OleDbCommand cmd = null;
                try
                {
                    connectionString = "Provider=Microsoft.JET.OLEDB.4.0;data source=" + location;                    
                    
                    conn = new OleDbConnection(connectionString);                    
                    cmd = new OleDbCommand(readQuery, conn);
                    conn.Open();
                    readReader = cmd.ExecuteReader();
                }
                catch (Exception rr)
                {
                    L.LogTimed(LogType.FILE, LogLevel.ERROR, "Readreader is not working properly" + rr.ToString());
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                //en son tarihten kac kez okunmus
                repeat = repeat_recordnum;
#region read Query
                while (readReader.Read())
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "read reader");
                    if (repeat == 0)
                    {
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rec's variables");
                            rec.LogName = "IncidentAccess Recorder";
                            try
                            {
                                dates = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                rec.Datetime = readReader.GetDateTime(0).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");
                            }
                            catch
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Convertion failure in DateTime : " + readReader.GetValue(0).ToString());
                            }
                            rec.CustomStr2 = readReader.GetString(1).ToString();                                                      
                            rec.Description = readReader.GetString(2).ToString();
                            rec.EventType = readReader.GetValue(3).ToString();
                            rec.EventCategory = readReader.GetString(4).ToString();
                            try
                            {
                                rec.CustomInt1 = Convert.ToInt32(readReader.GetValue(5).ToString());
                            }
                            catch (Exception e1) { L.Log(LogType.FILE, LogLevel.DEBUG, e1.Message); }
                            try
                            {
                                rec.CustomInt2 = Convert.ToInt32(readReader.GetValue(6).ToString());
                            }
                            catch (Exception e2) { L.Log(LogType.FILE, LogLevel.DEBUG, e2.Message); }
                            rec.EventId = Convert.ToInt64(readReader.GetValue(7).ToString());
                            rec.CustomStr4 = readReader.GetString(8).ToString();
                            rec.CustomStr3 = readReader.GetString(9).ToString();
                            rec.CustomStr1 = readReader.GetString(10).ToString();
                            if (readReader.GetValue(11).ToString().Length > 899 && readReader.GetValue(11).ToString().Length < 1800)
                            {
                                rec.CustomStr5 = readReader.GetString(11).ToString().Substring(0, 899);
                                rec.CustomStr7 = readReader.GetString(11).ToString().Substring(900, 1799);
                            }
                            else if (readReader.GetValue(11).ToString().Length < 899 )
                                rec.CustomStr5 = readReader.GetString(11).ToString();

                            rec.CustomStr6 = readReader.GetString(12).ToString();
                            rec.CustomStr8 = readReader.GetString(13).ToString();
                            rec.CustomStr9 = readReader.GetString(14).ToString();
                            rec.CustomStr10 = readReader.GetString(15).ToString();                            
                        }
                        catch (Exception bd)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Bad data recieved" + bd.ToString());
                            continue;
                        }                        
                        //eyer tarih bir onceki tarih ile ayni ise repeat numberi artir
                        dates = dates.Substring(0, 19);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Lastrecord : " + dates );                                                                        
                        repeat_recordnum = 0;                    
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
                        //bakilan en son record                        
                        lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + lastRecordDate);                        
                        //2007/12/18 14:54:15.000
                        dates = rec.Datetime.Substring(0, 19);                        
                        i++;
                        if (i > max_record_send)
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Reached MAX RECORD SEND number");
                            //son bakilan recordu registerye kaydet
                            cmd.Cancel();
                            dates = rec.Datetime.Substring(0, 19);
                            Set_Registry(lastRecordDate, rec.Datetime.Substring(0, 19));                            
                            //  lastRecordDate += "%" + repeat_recordnum.ToString();
                            return;
                        }
                    }//end of if
                    else
                    {
                        repeat -= 1;
                    }
                }
#endregion
                ////son bakilan recordu registerye kaydet
                //lastRecordDate += "%" + repeat_recordnum.ToString();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Set last date to registery");
                //Set_Registry(lastRecordDate,rec.Datetime);                
                conn.Close();
                readReader.Close();
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "Line could not parsed : " + readReader.GetValue(0) + " "  + readReader.GetValue(1) + " "  + readReader.GetValue(2));
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");                
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
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("IncidentAccessRecorder");
                    rk.SetValue("lastRecordDate", status);
                    rk.Close();
                    return true;
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(identity, lastRecordDate, "","","", lastrecdate);
                    return true;
                }

                
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Incident Access Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                        readQuery += "FROM " + Table_name;

                        readReader = Database.ExecuteReader(dbName, readQuery, CommandBehavior.CloseConnection, out cmd);
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
                        L.Log(LogType.FILE, LogLevel.INFORM, " initialize the date");
                        lastRecordDate = "19/08/2000 12:03:48%0";
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
                            lastRecordDate = "2000/08/19 12:03:48%0";
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
                EventLog.WriteEntry("Security Manager Incident Access Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
