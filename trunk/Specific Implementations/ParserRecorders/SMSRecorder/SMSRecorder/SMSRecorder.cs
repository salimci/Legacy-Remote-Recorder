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

namespace SMSRecorder
{
    public class SMSRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 5000, max_record_send = 100,zone=0;
        private string lastRecordDate = "2000-08-19 12:03:48.867";
        private int repeat_recordnum = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, SMSDb_name = "SMS_TMO", SMSTable_name = "Add_Remove_Programs_DATA", location, user, password, remote_host = "";
        protected string Virtualhost = "",Dal;
        
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else 
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SMS Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on SMS Recorder functions may not be running");
                            return;
                        }
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start creating SMS DAL");

                    reg_flag = true;

                    if (Database.AddConnection(SMSDb_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create SMSDb DAL");
                    else
                        L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating SMSDb DAL");
                }
            }
        }

        public bool get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SMSRecorder" + identity + ".log";
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
            String CustomVar1, Int32 CustomVar2, String virtualhost,String dal,Int32 Zone)
        {   
            trc_level = TraceLevel;
            timer_interval = SleepTime;
            max_record_send = MaxLineToWait;
            lastRecordDate = LastPosition;
            identity = Identity;
            location = Location;
            usingRegistery = false;
            SMSDb_name = location;
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
        
        public SMSRecorder()
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SMSRecorder.log";
                SMSDb_name = rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("DBName").ToString();
                SMSTable_name = rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("DTNAME").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("MaxRecordSend"));
                //en son hangi recordda kaldigimizi bulmak icin
                lastRecordDate = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("SMSRecorder").GetValue("LastRecordDate"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SMS Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> 11111111111");

            L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> Service Started");
            int x = 0;
            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> Starting the timer");
                if (!reg_flag)
                {
                    if (usingRegistery)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> Error on Reading the Registry ");
                            return;
                        }
                    }

                    if (!Initialize_Logger())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> Error on Intialize Logger on Isa Web Database Recorder functions may not be running");
                        return;
                    }
                    reg_flag = true;
                }
                 
                //L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>  Last Dbname is : " + SMSDb_name);
                
                if (begining)
                {
                    max_rec_send2 = max_record_send;
                    begining = false;
                }

                int i = 0;

                //get date and repeat number separetly

                // L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Date before splitdate : " + lastRecordDate);
                
                prevDate = curDate;
                // L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> prevDate " + prevDate);
                setLastRecordDate();

                if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0") 
                {
                    lastRecordDate = "2000-08-19 12:03:48.867";
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
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Bir arttýrýldý");
                }
                else
                {
                    repeat = repeat_recordnum;
                    controlofdoublerecord = true;
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Ýlk defa girdi ayný kaldý 0");
                }
                
                if (repeat_recordnum >= max_record_send)
                {
                    max_record_send += repeat_recordnum;

                    //L.Log(LogType.FILE, LogLevel.DEBUG, " Record Number azaltýldý");

                    repeat = repeat_recordnum - 1;
                    repeat_recordnum = repeat;
                    
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> max record sent " + max_record_send);
                }
                else
                {
                    max_record_send = max_rec_send2;
                }

                readQuery2  = "Select TOP " + max_record_send + " ";
                readQuery2 += "MachineID,InstanceKey,";
                readQuery2 += "AgentID,TimeKey,ProdID00,DisplayName00,";
                readQuery2 += "Publisher00,Version00 ";
                readQuery2 += "FROM " + SMSTable_name + "(nolock) ";
                readQuery2 += "WHERE TimeKey >= '" + curDate + "' ORDER BY TimeKey,InstanceKey";

                L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->> Query is " + readQuery2);

                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>setting  readReader");
                    readReader2 = Database.ExecuteReader(SMSDb_name, readQuery2, CommandBehavior.CloseConnection, out cmd2);
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
                            rec.LogName = "SMS Recorder";
                             
                            try
                            {
                                if (!readReader2.IsDBNull(0))
                                {
                                    rec.ComputerName = readReader2.GetInt32(0).ToString(); //ComputerName As machine id
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 11 " + ex.Message.ToString());
                            }
                             
                            try
                            {
                                if (!readReader2.IsDBNull(1))
                                {
                                    rec.CustomInt1 = readReader2.GetInt32(1);              //CustomInt1 As INSTANCE_KEY 
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 12 " + ex.Message.ToString());
                            }

                            L.Log(LogType.FILE, LogLevel.DEBUG, "  In timer1_Tick() -->>INSTANCE_KEY  IS " +  rec.CustomInt1.ToString());


                            try
                            {
                                if (!readReader2.IsDBNull(2))
                                {
                                    rec.CustomInt2 = readReader2.GetInt32(2);//AgentID 
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 13 " + ex.Message.ToString());
                            }

                            try
                            {
                                if (!readReader2.IsDBNull(3))
                                {
                                    rec.Datetime = readReader2.GetDateTime(3).AddHours(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");   //date
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 14 " + ex.Message.ToString());
                            }

                            prevDate = curDate;

                            try
                            {
                                if (!readReader2.IsDBNull(3))
                                {
                                    curDate = readReader2.GetDateTime(3).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 15 " + ex.Message.ToString());
                            }

                            try
                            {
                                if (!readReader2.IsDBNull(4))
                                {
                                    rec.UserName = readReader2.GetString(4).ToString();     //client ProdID00 
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 16 " + ex.Message.ToString());
                            }

                            try
                            {
                                if (!readReader2.IsDBNull(5))
                                {
                                    rec.CustomStr4 = readReader2.GetString(5).ToString();   //DisplayName00 
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 17 " + ex.Message.ToString());
                            }

                            try
                            {
                                if (!readReader2.IsDBNull(6))
                                {
                                    rec.CustomStr6 = readReader2.GetString(6).ToString();  //Publisher00 
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 18 " + ex.Message.ToString());
                            }

                            try
                            {
                                if (!readReader2.IsDBNull(7))
                                {
                                    rec.CustomStr7 = readReader2.GetString(7).ToString();  //Version00  
                                }
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 19 " + ex.Message.ToString());
                            }


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
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 6 " + er.Message.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "  In timer1_Tick() -->> In Catch 6 " + er.StackTrace.ToString());
                erroroccured = true;
            }
            finally
            {
                //L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> In Finaly Repeat Record Num Is : " + repeat_recordnum.ToString());   
                
                if (x == 0 && controlofadd && !erroroccured)
                {
                    repeat_recordnum--;
                    L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> There Is No New Record So Waiting For New Record");
                }
                
                if (x == 0 && controlofadd && erroroccured)
                {
                    repeat_recordnum--;
                    L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> An Error Occured And Repeat Record Num -- : " + repeat_recordnum.ToString());
                }
                
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, " In timer1_Tick() -->> Service Stopped");
                Database.Drop(ref cmd2);
                s.Dispose();
            }
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
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("IsaWebDbRecorder");
                    rk.SetValue("lastRecordDate", status);
                    rk.Close();
                    return true;
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(identity, status, " ","", repeat_recordnum.ToString(),status);
                    return true;
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
            
            try
            {
                if (fromEnd)
                {

                    if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0")
                    {
                        try
                        {
                            readQuery = "select MAX(TimeKey)";
                            readQuery += " FROM " + SMSTable_name;

                            readReader = Database.ExecuteReader(SMSDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                            cmd.CommandTimeout = 1200;
                            while (readReader.Read())
                            {
                                lastRecordDate = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                controlofdoublerecord = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate() -->> In Catch 1 " + ex.ToString());
                        }
                    }
                }
                else 
                {
                    if (String.IsNullOrEmpty(lastRecordDate) || lastRecordDate == "0")
                    {
                        try
                        {
                            readQuery = "select MIN(TimeKey)";
                            readQuery += " FROM " + SMSTable_name;
                            readReader = Database.ExecuteReader(SMSDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                            cmd.CommandTimeout = 1200;
                           
                            while (readReader.Read())
                            {
                                lastRecordDate = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                                controlofdoublerecord = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate() -->> In Catch 2 " + ex.Message.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate() -->> In Catch 2 " + ex.StackTrace.ToString());
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "  In setLastRecordDate() -->> Last Record Date Is : " + lastRecordDate); 
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate() -->> In Catch 3 " + ex.Message.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "  In setLastRecordDate() -->> In Catch 3 " + ex.StackTrace.ToString());
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
                EventLog.WriteEntry("Security Manager SMS Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
