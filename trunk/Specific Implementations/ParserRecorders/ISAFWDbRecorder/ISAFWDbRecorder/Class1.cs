/*
 * 
 * 
 * 
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
using System.Globalization;

namespace ISAFWDbRecorder
{
    public class ISAFWDbRecorder : CustomBase
    {
        private System.Timers.Timer timer1;         //belirlenen zaman araliklari ile db ye baglanip almak icin
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0;
        private string lastRecordDate = "";     //son recordu bilebilmek icin 
        private int repeat_recordnum = 0;        //tarihin tekrarlanma sayisi (hangi rekordda kaldigini bulmak icin)
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, isaDb_name = "isalogdb", isaFWTable_name = "firewalllog", location, user, password, remote_host = "";  //>>>>>
        protected string Virtualhost="",Dal;
        //>>>> isadb name = isa database in ismi, isatable name = web proxy logunun oldugu table

        String[] lastrecord = null;      //registeryden alinan kayitta tarih ile tekrar sayisini ayri saklamak icin
        private string dates = "2000/08/19 12:03:48.867";
        private bool reg_flag = false;
        private CLogger L;
        int repeat = 0;
        private int max_rec_send2 = 100;
        bool begining = true;
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
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start creating IsaFWDb DAL");

                    //Database.CreateDatabase();

                    isaDb_name = "IsaWebDb" + identity.ToString();
                    if (Database.AddConnection(isaDb_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create IsaFWDb DAL");
                    else
                        L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating IsaFWDb DAL");
                    reg_flag = true;
                }
            }
        }
        public bool get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\ISAFWDbRecorder" + identity + ".log";
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
            usingRegistery = false;

            isaDb_name = User;
            Virtualhost = virtualhost;
            Dal = dal;
            //logging_interval = Convert.ToUInt32(CustomVar2);
            //log_size = Convert.ToUInt32(LastKeywords);
            location = Location;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            isaFWTable_name = CustomVar1;
            fromEnd = FromEndOnLoss;
            zone = Zone;
            //>>>> isadb name = isa database in ismi, isatable name = web proxy logunun oldugu table
        }

        public ISAFWDbRecorder()
        {

        }

        private void InitializeComponent()
        {
            //Init();
        }
        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\ISAFWDbRecorder.log";
                isaDb_name = rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("DBName").ToString();
                isaFWTable_name = rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("DTNAME").ToString();
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("MaxRecordSend"));
                //en son hangi recordda kaldigimizi bulmak icin
                lastRecordDate = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("ISAFWDbRecorder").GetValue("LastRecordDate"));
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Isa Fire wall Database Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Isa FW Database Recorder functions may not be running");
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
                /*
                 * ////////////////////
                 * database den belli tarihten sonrasini aliyor
                 * repeat_number tarihin sonuna eklendigi icin ilk once onu ayiriyoruz
                 * /////////////////////
                 * */
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
                L.Log(LogType.FILE, LogLevel.DEBUG, "Date : " + dates);
                //database query si = belli tarihten sonra ilk max_record_send tane recordu getiriyor
                readQuery = "select TOP(" + max_record_send + ") ";
                readQuery += "[servername],[logTime],[protocol],[SourceIP],[SourcePort],[DestinationIP],";
                readQuery += "[DestinationPort],[OriginalClientIP],[SourceNetwork],[DestinationNetwork],";
                readQuery += "[Action],[resultcode],[rule],[ApplicationProtocol],[Bidirectional],[bytessent],";
                readQuery += "[bytessentDelta],[bytesrecvd],[bytesrecvdDelta],[connectiontime],[connectiontimeDelta],";
                readQuery += "[SourceProxy],[DestinationProxy],[SourceName],[DestinationName],[ClientUserName],";
                readQuery += "[ClientAgent],[sessionid],[connectionid],[Interface],[IPHeader],[Payload],[GmtLogTime]";
                readQuery += " FROM " + isaFWTable_name + " (nolock) WHERE logTime >= CONVERT(datetime, '" + dates + "',102) ORDER BY logTime";
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query is " + readQuery);                
                readReader = Database.ExecuteReader(isaDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                cmd.CommandTimeout = 1200;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                //en son tarihten kac kez okunmus
                repeat = repeat_recordnum;
                L.Log(LogType.FILE, LogLevel.DEBUG,"FieldCount:"+readReader.FieldCount);                    
                while (readReader.Read())
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "read reader");
                    if (repeat == 0)
                    {
                        try
                        {
                            #region SetRecord
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rec's");
                            rec.LogName = "ISA FireWall Recorder";
                            rec.ComputerName = readReader.GetString(0).ToString();    //server name
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting sserver name");
                            rec.UserName = System.Net.IPAddress.Parse(readReader.GetInt64(7).ToString()).ToString(); //orginal client ip
                            L.Log(LogType.FILE, LogLevel.DEBUG, "setting original client ip");
                            datet = readReader.GetDateTime(1).ToString("yyyy/MM/dd HH:mm:ss.fff");               //log time
                            rec.Datetime = readReader.GetDateTime(1).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");               //log time
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting log time");
                            rec.SourceName = System.Net.IPAddress.Parse(readReader.GetInt64(3).ToString()).ToString(); //source ýp
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting source ip");
                            rec.CustomStr1 = readReader.GetString(2).ToString();                              //protocol
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting protocol");
                            rec.CustomInt1 = Convert.ToInt32(readReader.GetInt32(4));                         //sorce port
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting source port");
                            Int64 destip = 0;
                            string destString = "";
                            try
                            {                                
                                destString = readReader.GetValue(5).ToString();                            
                                destip = Convert.ToInt64(destString);
                                rec.CustomStr4 = System.Net.IPAddress.Parse(destip.ToString()).ToString();//dest ip
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst ip");
                            }
                            catch
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Could not parse dest ip sending in string format");
                                L.Log(LogType.FILE, LogLevel.DEBUG, "DestString : " + destString);
                                rec.CustomStr4 = destString;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst ip by string");
                            }

                            rec.CustomInt2 = Convert.ToInt32(readReader.GetInt32(6));                         //dest port
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dest port");

                            rec.CustomStr3 = rec.SourceName;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting source IP");
                            
                            rec.CustomStr5 = readReader.GetString(8).ToString();                            //sorce ntwrk     
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting source netwrk");

                            rec.CustomStr6 = readReader.GetString(9).ToString();                        //dest netwrk
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting dst netwrk");

                            rec.CustomInt3 = Convert.ToInt32(readReader.GetInt16(10));                         //action
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting action");

                            rec.CustomInt4 = Convert.ToInt32(readReader.GetInt32(11));                         //result code
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting result code");

                            rec.CustomStr7 = readReader.GetString(12).ToString();                    //rule
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rule");

                            rec.CustomStr8 = readReader.GetString(13).ToString();                    //application protocol
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting app. protocol");

                            rec.CustomInt5 = Convert.ToInt32(readReader.GetInt16(14));                         //bidericetional
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bidirectional");

                            rec.CustomInt6 = Convert.ToInt64(readReader.GetInt64(15));                         //bytes sent
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bytes send");

                            rec.CustomInt7 = Convert.ToInt64(readReader.GetInt64(16));                         //bytes sent delta
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bytes send delta");

                            rec.CustomInt8 = Convert.ToInt64(readReader.GetInt64(17));                         //bytes rcvd
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting rcvd send");

                            rec.EventId = Convert.ToInt64(readReader.GetInt64(18));                         //bytes rcvd delta
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting bytes rcvd delta");

                            rec.CustomInt10 = Convert.ToInt32(readReader.GetInt32(27));                         //session id
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting session id");

                            rec.CustomInt9 = Convert.ToInt32(readReader.GetInt32(28));                         //conn. id
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting conn id");

                            rec.Recordnum = Convert.ToInt32(readReader.GetInt32(19));                         //conn. time
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Setting conn. time");

                            switch (rec.CustomInt3)
                            {
                                case 6 :
                                    rec.EventCategory = "Establish";
                                    break;
                                case 7:
                                    rec.EventCategory = "Terminate";
                                    break;
                                case 8:
                                    rec.EventCategory = "Denied";
                                    break;
                                case 9:
                                    rec.EventCategory = "Allowed";
                                    break;
                                case 10:
                                    rec.EventCategory = "Failed";
                                    break;
                                case 11:
                                    rec.EventCategory = "Intermediate";
                                    break;
                                case 12:
                                    rec.EventCategory = "Succesfull Connection";
                                    break;
                                case 13:
                                    rec.EventCategory = "Unsuccesfull Connection";
                                    break;
                                case 14:
                                    rec.EventCategory = "Disconnect";
                                    break;
                                default:
                                    rec.EventCategory = "";
                                    break;
                            }
                            #endregion
                        }
                        catch (Exception bd)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Bad data" + bd.ToString());
                            continue;
                        }
                        //eyer tarih bir onceki tarih ile ayni ise repeat numberi artir
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Lastrecord : " + dates + " " + datet.ToString());
                        if (dates == datet.ToString())
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Repeat the date");
                            repeat_recordnum++;
                        }
                        else //degilse last recor olarak date i set et
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Go for last record");
                            dates = datet.ToString();
                            repeat_recordnum = 0;
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                        if (usingRegistery)
                        {
                            s.SetData(rec);     //datayi gonder
                        }
                        else
                        {
                            s.SetData(Dal,Virtualhost, rec);     //datayi gonder
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                        // repeat_recordnum = rec.CustomInt6;
                        //bakilan en son record
                        lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is " + lastRecordDate);
                        i++;
                        if (i > max_record_send || i%100==0)
                        {
                            try
                            {
                                //son bakilan recordu registerye kaydet
                                Set_Registry(lastRecordDate,rec.Datetime);                                
                            }
                            catch (Exception sr)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "couldnt send date to registery " + sr.ToString());
                            }                            
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
                try
                {
                    //son bakilan recordu registerye kaydet
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Set last date to registery");
                    Set_Registry(lastRecordDate,rec.Datetime);
                }
                catch (Exception sr2)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "couldnt send date to registery " + sr2.ToString());
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.Message);
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.StackTrace);
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
                Database.Drop(ref cmd);
                s.Dispose();
            }
        }

        public bool Set_Registry(String status,String lastrecdate)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistery)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("ISAFWDbRecorder");
                    rk.SetValue("lastRecordDate", status);
                    rk.Close();
                    return true;
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(identity, status, "","", "",lastrecdate);
                    return true;
                }
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Isa FW Db Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                        readQuery += " FROM " + isaFWTable_name;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last date time query is :" + readQuery);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "isaFWTable_name" + isaFWTable_name);
                        readReader = Database.ExecuteReader(isaDb_name, readQuery, CommandBehavior.CloseConnection, out cmd);
                        cmd.CommandTimeout = 1200;
                        dates = readReader.GetDateTime(0).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        repeat_recordnum = 0;
                        lastRecordDate = dates + "%" + repeat_recordnum.ToString();
                    }
                    catch (Exception DE)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "ERROR at reading from end" + DE.ToString());
                    }
                }
                else
                {
                    if (lastRecordDate.Equals("no data"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " initialize the date");                        
                        lastRecordDate = DateTime.Now.ToString() + "%0";
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
                            lastRecordDate = DateTime.Now.ToString() + "%0";
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
                EventLog.WriteEntry("Security Manager Isa FW Database Recorder", er.ToString(), EventLogEntryType.Error);
                return false;

            }
        }
    }
}
