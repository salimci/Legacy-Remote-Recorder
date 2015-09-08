/*
 * Audit:: Emin Karaca 
 * Written in C# by using visual studio
 * Connects to a domain by given credentials
 * Then reads the eventLogs of the given machine
 * **credentials-log type that will be read - remotemachine ip should given
 * */

using System;
using System.Collections.Generic;
using System.Text;
using CustomTools;
using System.Collections;
using System.Data;
using System.Security;
using System.IO;
using System.Diagnostics;


using Log;
using LogMgr;
using DAL;
using Microsoft.Win32;
using System.ComponentModel;
using System.Management;
using System.Collections.Specialized;
//using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Principal;
//using System.Security.Permissions;

using System.Security.Permissions;
using System.Security.Cryptography;

using System.Threading;

namespace EventLogViewer
{
    public class EventLogViewer : CustomServiceBase
    {


       
        //credentials info
        private string userName;        //user name from the domain
        private string userPass;        //login password of the domain
        static string domain;           //domain name
        static string machineName;      //ip of the remote machine

        //log info
        private string log_type;        //log types that will be read from event log

        //logging info
        private string err_log;         
        private int trc_level = 3;
        private int logging_interval=60000;
        private int log_size = 1000000;
        private CLogger L;

        //registery value
        private int eventRecordId = 0;      //number of last log [will save in registry]
        private int eventRecordIdBk = 0;    
        static bool usingRegistry=true;     //true=caller is agent false=caller is remote recorder
        static int Identity2;               //used for remote recorder id of thread
        //temp value
        
        object mac = 5;
        Thread t;
        private int readFreq = 100;         //amounth of data that will take at 1 query
        private int sleepTime = 10000;      //thread wait time

        private ConnectionOptions co;
        private ManagementScope scope;

        private bool if_2008;
        //starts the eventlogViewer
        public override void Start()
        {

           Initialize_Logger();

            //reads eventlog
            t = new Thread(new ParameterizedThreadStart(readEvent));
            t.Start(mac);
        }

        public EventLogViewer()
		{

            if (!usingRegistry)
                Start();
            else
            {

                try
                {
                    if (usingRegistry)
                    {

                        InitializeComponent();
                        // TODO: Add any initialization after the InitComponent call          
                        L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  EventLogViewer");

                        t = new Thread(new ParameterizedThreadStart(readEvent));
                        t.Start(mac);
                    }

                    L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing EventLogViewer");

                }
                catch (Exception er)
                {
                    EventLog.WriteEntry("Security Manager EventLogViewer Constructor", er.ToString(), EventLogEntryType.Error);
                }
            }
		}
       
        //gets parameters those are needed and sets them
        public override void SetConfigData(int Identity, string Location, string LastLine,
            long LastPosition, string LastKeywords, bool FromEndOnLoss, int MaxLineToWait, 
            string User, string Password, string RemoteHost, int SleepTime,
            int TraceLevel, string CheckPointVar1, bool CheckPointVar2)
        {
            //credentials info
            userName = User;
            userPass = Password;
            domain = Location;
            machineName = RemoteHost;

            //log info
            log_type = LastKeywords;

            err_log = LastLine;
            trc_level = TraceLevel;
            logging_interval = MaxLineToWait;


            eventRecordId = Convert.ToInt32(LastPosition);
            Identity2 = Identity;
            usingRegistry = false;
            
         
        }// end of setconfigdata
        
        //set the values to register
        public override void SetReg(int Identity, string LastPosition, string SleepTime, string LastKeywords)
        {
            base.SetReg(Identity, LastPosition, SleepTime.ToString(), LastKeywords);
        }


        /*
         * below are needed for connecting to a domain with new critdentials
         * ----------
         * */
        #region
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;        //enables logging into a domain with new credentials
        const int LOGON32_LOGON_INTERACTIVE = 2;
        static private WindowsImpersonationContext wic;
        static private WindowsIdentity wi;
        #endregion
        /*
         * ----------
         * */
        public void oneSec()
        {
            Thread.Sleep(sleepTime);
        }

        //disconnect from the account [owner of the credentials]
        public void ReleaseMe()
        {
            if (wic != null)
            {
                wic.Undo();
                wic.Dispose();
            }
        }

        //connect to a domain account with givven credentials
        public void ValidateMe()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Started validating credentials " );
            L.Log(LogType.FILE, LogLevel.DEBUG, "username2: " + userName);
            if (userName != "")
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "user name is not NULL");
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "setting user name");
                    String user = userName;
                    
                    if ( user.Contains("\\"))
                    {
                        String[] arr = user.Split('\\');
                        userName = arr[1];
                        domain = arr[0];
                    }
               
                    L.Log(LogType.FILE, LogLevel.DEBUG, "connecting to domain: "+ domain);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "userPass: " + userPass);
                    bool ret = LogonUser(user, domain, userPass, 9,
                          LOGON32_PROVIDER_DEFAULT, ref token);
                //    bool ret = LogonUser(user, domain, userPass, LOGON32_LOGON_INTERACTIVE ,
                  //        LOGON32_PROVIDER_DEFAULT, ref token);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Connected to domain: "+ domain);

                    if (ret)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate))
                        {

                            wi = new WindowsIdentity(tokenDuplicate);
                            wic = wi.Impersonate();
                            if (wic != null)
                            {
                                CloseHandle(token);
                                CloseHandle(tokenDuplicate);
                                return;
                            }
                        }
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Couldnt connected to domain: "+domain);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "error : " + error);
                     
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);

            }
        }

        //reads and send the eventlogs
        public void readEvent(object mac){
            
            CustomBase.Rec r = new CustomBase.Rec();
            CustomServiceBase s;

            //set instance service
#region
            if (usingRegistry)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "InstanceService :" + "Security Manager Sender");
                s = base.GetInstanceService("Security Manager Sender");
                while (s == null)
                {
                    s = base.GetInstanceService("Security Manager Sender");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "InstanceService :" + "Security Manager Remote Recorder");
                s = base.GetInstanceService("Security Manager Remote Recorder");
                while (s == null)
                {
                    s = base.GetInstanceService("Security Manager Remote Recorder");
                    Thread.Sleep(1000);
                }
            }
#endregion
            try
            {


                L.Log(LogType.FILE, LogLevel.DEBUG, "Connecting to machine :" + machineName);
                L.Log(LogType.FILE, LogLevel.DEBUG, "setting connection options :" );
                co = new ConnectionOptions();
                co.Username = userName;
                co.Password = userPass;
                co.Impersonation = ImpersonationLevel.Default;//.Impersonate;
                co.Authentication = AuthenticationLevel.PacketPrivacy;
                co.Authority = "ntlmdomain:" + domain;

                scope = new ManagementScope(@"\\" + machineName + @"\root\cimv2", co);
                scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                scope.Connect();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Connected:");
                
                // default blocksize = 1, larger value may increase network throughput
                EnumerationOptions opt = new EnumerationOptions();
                opt.BlockSize = 1000;

                //findinf the min eventRecordId 
                if (eventRecordId == 0)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "event id is zero :" + eventRecordId);
                    SelectQuery query2 = new SelectQuery("select RecordNumber from Win32_NtLogEvent where Logfile ='" + log_type + "'");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query2, opt))

                        foreach (ManagementObject mo in searcher.Get())
                        {
                            eventRecordId = Convert.ToInt32(mo["RecordNumber"]) - searcher.Get().Count;
                            break;
                        }
                }

                //checking whether machine is win2003 or win2008
                SelectQuery query = new SelectQuery("select " +
                  "RecordNumber " +
                  "from Win32_NtLogEvent " +
                  "where Logfile ='" + log_type + "' and RecordNumber >" + eventRecordId + " and  RecordNumber < " + (eventRecordId + 2));
                int data1;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query2, opt))
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        data1 = Convert.ToInt32(mo["RecordNumber"]);
                    }
                if (data1 > eventRecordId)
                {
                    //2008
                }
                else { //2003}
                while (true)
                {   
                    eventRecordIdBk = eventRecordId;

                    SelectQuery query = new SelectQuery("select "+
                      "Category,ComputerName, EventIdentifier,EventType,Message,RecordNumber,SourceName " +
                      "from Win32_NtLogEvent " +
                      "where Logfile ='" + log_type + "' and RecordNumber >" + eventRecordId + " and  RecordNumber < " + (eventRecordId+readFreq));
                                        
                    L.Log(LogType.FILE, LogLevel.DEBUG, "eventid after query:" + eventRecordId);  
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start collection event logs of :" + log_type);

                   using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                       
                   foreach (ManagementObject mo in searcher.Get())
                       {
                           L.Log(LogType.FILE, LogLevel.DEBUG, "searcher count  :" + searcher.Get().Count);
                           L.Log(LogType.FILE, LogLevel.DEBUG, "eventid in foreach  :" + mo["RecordNumber"]);
                        try{
         //preparing data
         #region
                                
                            L.Log(LogType.FILE, LogLevel.DEBUG, "setting log name");
                            r.LogName = "EventLogViewer";

                            L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing logname");

                            r.ComputerName = mo["ComputerName"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "2-preparingComputerName :" + r.ComputerName);

                            r.EventCategory = mo["Category"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing EventCategory:" + r.EventCategory);

                            r.EventId = Convert.ToInt64(mo["EventIdentifier"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing EventId:" + r.EventId);

                            r.EventType = mo["EventType"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "3-preparing EventType:" + r.EventType);

                            r.Recordnum = Convert.ToInt32(mo["RecordNumber"]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "4-preparing Recordnum:" + r.Recordnum);

                            r.SourceName = mo["SourceName"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "5-preparing SourceName:" + r.SourceName);
                                
                            if (mo["Message"].ToString() == null)
                                break;
                            r.Description = mo["Message"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "preparing Description:" + r.Description);

                            L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                            #endregion
         //sendin data    
         #region  
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                            
                            s.SetData(r);

                            L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
        #endregion
                        }//try
                        catch (Exception ej)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, ":" + ej.Message.ToString());
                            break;
                         }//catch   
                      }//foreach

                    L.Log(LogType.FILE, LogLevel.DEBUG, "end of foreach");

                   if (!usingRegistry)
                   {
                       L.Log(LogType.FILE, LogLevel.DEBUG, "sending last position with setreg");
                       s.SetReg(Identity2, eventRecordId.ToString(), "", "");
                   }
                   else
                   {
                       RegistryKey rk = null;
                       rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("EventLogViewer");
                       rk.SetValue("Last Log", eventRecordId);
                       rk.Close();
                   }
                   oneSec();
                }//while true 

            }//try
            catch (Exception es)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "log couldnt handled error is:" + es.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "log couldnt handled event id is:" + eventRecordId);
                L.Log(LogType.FILE, LogLevel.DEBUG, "call releaseme");
            }
            finally { //s.Dispose(); 
            }
         
        }//readEvent

        private void InitializeComponent()
        {
            if (!Read_Registry())
            {
                EventLog.WriteEntry("EventLogViewer Read Registry", "EventLogViewer may not working properly ", EventLogEntryType.Error);
                return;
            }
            else
                if (!Initialize_Logger())
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on EventLogViewer Service functions may not be running");
                    return;
                }
        }//initializecomponent

        //decyrpt password
        public string Decyrpt(string password, byte f, byte s, byte t)
        {
            /******* initializing keys with he predefined values *******/
            byte[] Key = { 216, 250, 130, 174, 71, 152, 5, 160 };
            byte[] IV =  { f, s, t, 53, 74, 233, 137, 18 };
            byte[] mBytes;
            MemoryStream mMemStr = null;
            CryptoStream cStream = null;
            StreamReader sReader = null;
            DES DESalg = null;
            try
            {
                DESalg = DES.Create("DES");

                DESalg.Key = Key;
                DESalg.IV = IV;
                /******* initializing keys with he predefined values *******/
                mMemStr = new MemoryStream();
                mBytes = Convert.FromBase64String(password);
                //mBytes = Encoding.GetEncoding(1252).GetBytes(password);
                mMemStr.Write(mBytes, 0, mBytes.Length);
                mMemStr.Flush();
                mMemStr.Position = 0;

                cStream = new CryptoStream(mMemStr,
                    DESalg.CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Read the data from the stream 
                // to decrypt it.
                sReader = new StreamReader(cStream, Encoding.GetEncoding(1254));
                string data = sReader.ReadToEnd();
                return data;
            }
            catch (Exception er)
            {
                Logger.Log(LogType.EVENTLOG, LogLevel.DEBUG, "Encrypt Error " + er.Message);
                return null;
            }
            finally
            {
                sReader.Close();
                cStream.Close();
                mMemStr.Close();
                IV = null; Key = null; mBytes = null;
                DESalg.Clear();
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\EventLogViewer.log";
                userName = rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("User Name").ToString();
               
                machineName = rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Host Ip").ToString();
                domain = rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Domain Name").ToString();
                log_type = rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Log Type").ToString();
                eventRecordId = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Last Log"));
                readFreq = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Read Freq"));
                sleepTime = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Sleep Time"));
                //    date = rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("lastRecordDate").ToString();
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Trace Level"));
                log_size = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Log Size"));

                logging_interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("Logging Interval"));
                userPass = Decyrpt(rk.OpenSubKey("Recorder").OpenSubKey("EventLogViewer").GetValue("User Pass").ToString(), 12, 13, 14);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        
        //initialize logging informations
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

                L.SetLogFile("EventLogViewer.log");
                L.SetTimerInterval(LogType.FILE, Convert.ToUInt32(logging_interval));
                L.SetLogFileSize(Convert.ToUInt32(log_size));

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager JuniperSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        
    }//public class
}//namespace
