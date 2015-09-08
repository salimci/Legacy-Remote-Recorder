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
using System.Data;
using System.Data.OleDb;
using DAL;

namespace AviraRecorder
{
    public class AviraRecorder : CustomBase
    {
        private System.Timers.Timer timerEvent;
        
        private int trc_level = 3, timer_interval = 60000, max_record_send = 100,zone=0;
        private long last_recordnumEvent;
        private long last_recordnumFirewall;
        private long last_recordnumScan;
        private long last_recordnumThreat;

        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, mcdb_name = "era.mdb", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;

        public AviraRecorder()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {

        }

        public override void Init()
        {
            timerEvent = new System.Timers.Timer();
            timerEvent.Elapsed += new System.Timers.ElapsedEventHandler(this.timerEvent_Tick);
            timerEvent.Interval = timer_interval;
            timerEvent.Enabled = true;

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Avira Recorder functions may not be running");
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
                                //L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Avira Recorder functions may not be running");
                                return;
                            }
                        
                        //L.Log(LogType.FILE, LogLevel.INFORM, "Start creating Avira DAL");

                        reg_flag = true;
                        //Database.CreateDatabase();
                        db_name = "AviraDb" + Id.ToString();

                        //if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create Avira DAL");
                        //else
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating Avira DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Avira Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        //Validation
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

        public enum LogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations;
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server.
            /// </summary>
            LOGON32_LOGON_INTERACTIVE = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_NETWORK = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_BATCH = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled.
            /// </summary>
            LOGON32_LOGON_SERVICE = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer.
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked.
            /// </summary>
            LOGON32_LOGON_UNLOCK = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections.
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        public enum LogonProvider : int
        {
            /// <summary>
            /// Use the standard logon provider for the system.
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name
            /// is not in UPN format. In this case, the default provider is NTLM.
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            LOGON32_PROVIDER_DEFAULT = 0,
        }

        private WindowsImpersonationContext wic;

        private WindowsIdentity wi;

        protected void ReleaseMe()
        {

            if (wic != null)
            {
                wic.Undo();
                wic.Dispose();
                wic = null;
            }
        }

        protected void ValidateMe()
        {
            if (user != "")
            {
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    String userName = user;
                    String domain = "";
                    if (user.Contains("\\"))
                    {
                        String[] arr = user.Split('\\');
                        userName = arr[1];
                        domain = arr[0];
                    }

                    bool ret = LogonUser(userName, domain, password, (Int32)LogonType.LOGON32_LOGON_NEW_CREDENTIALS,
                        (Int32)LogonProvider.LOGON32_PROVIDER_DEFAULT, ref token);

                    if (ret)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate))
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "Impersonation is successful");
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }
        
        public bool Get_logDir()
        {   
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\Avira_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Avira Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
        String CustomVar1, int CustomVar2, String Virtualhost,String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;

            string position = LastPosition;
            position = position.Trim();
            last_recordnumEvent = Convert.ToInt64(position);
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\AviraRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("DBName").ToString();

                this.timerEvent.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("Interval"));
            
                mcdb_name= rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("MCDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("MaxRecordSend"));
                
                string positions = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("AviraRecorder").GetValue("LastRecordNum"));
                positions = positions.Trim();

                if (positions != "0" && !String.IsNullOrEmpty(positions))
                {
                    string[] tempPosition = positions.Split(',');
                    last_recordnumEvent = Convert.ToInt64(tempPosition[0].Split(':')[1]);
                    last_recordnumFirewall = Convert.ToInt64(tempPosition[1].Split(':')[1]);
                    last_recordnumScan = Convert.ToInt64(tempPosition[2].Split(':')[1]);
                    last_recordnumThreat = Convert.ToInt64(tempPosition[3].Split(':')[1]);
                }
                else
                {
                    last_recordnumEvent = 0;
                    last_recordnumFirewall = 0;
                    last_recordnumScan = 0;
                    last_recordnumThreat = 0;
                }
                
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Avira Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                
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
            string readQueryEvent = null;

            AccessConnection ac = new AccessConnection(); ;
            OleDbDataReader readReader;

            try
            {
                readQueryEvent = "select MAX(Event_ID) FROM Events";


                L.Log(LogType.FILE, LogLevel.DEBUG, " Query Event is " + readQueryEvent);

                ac.OpenAccessConnection(mcdb_name);
                readReader = ac.ExecuteAccessQuery(readQueryEvent); 

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnumEvent = Convert.ToInt64(readReader.GetInt32(0));
                }
                
                if (usingRegistry)
                    Set_Registry(last_recordnumEvent.ToString());
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, last_recordnumEvent.ToString(), "", "", "", last_recdate);
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
                ac.CloseAccessConnection();
            }
        }
        
        public override void Clear()
        {
            if (timerEvent != null)
                timerEvent.Enabled = false;
        }

        private void timerEvent_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ValidateMe();

            timerEvent.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            string readQuery = null;
            AccessConnection ac = new AccessConnection();
            OleDbConnection connection = null;
            OleDbCommand command = null;
            OleDbDataReader readReader = null;

            try
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Avira Recorder functions may not be running");
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

                readQuery = "Select TOP " + max_record_send + " Actor_ID,Product_ID,Product_Name,Module_Name,Event_Type,Issue_Time2,Msg,Msg_Index,P1,P2,Event_ID from Events where Event_ID > " + last_recordnumEvent + " ORDER BY Event_ID";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query for EventLogTable is " + readQuery);

                ac.OpenAccessConnection(mcdb_name);
                readReader = ac.ExecuteAccessQuery(readQuery);

                if (!readReader.HasRows) // hatalı access kayıtları için.  hatalı kayıtları atlar...
                {
                    string readQuery1 = "select MAX(Event_ID) FROM Events";
                    OleDbDataReader readReader22 = ac.ExecuteAccessQuery(readQuery1);
                    long max_record_num = 0;
                    
                    while (readReader22.Read())
                    {
                        max_record_num = Convert.ToInt64(readReader22.GetDecimal(0));
                    }
                    
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Maximum Record Num for EventsLog Table is : " + max_record_num);
                    
                    if (!readReader22.IsClosed)
                    {
                        readReader22.Close();
                    }

                    if (last_recordnumEvent < max_record_num)
                    {
                        last_recordnumEvent += 1;
                    }
                }

                while (readReader.Read())
                {
                    rec.LogName = "Avira Recorder";

                    if (!Convert.IsDBNull(readReader["Actor_ID"]))
                    {
                        rec.CustomInt6 = readReader.GetInt64(0);
                    }

                    if (!Convert.IsDBNull(readReader["Product_ID"]))
                    {
                        rec.CustomInt7 = readReader.GetInt64(1);
                    }

                    if (!Convert.IsDBNull(readReader["Product_Name"]))
                    {
                        rec.CustomStr1 = readReader.GetString(2);
                    }

                    if (!Convert.IsDBNull(readReader["Module_Name"]))
                    {
                        rec.CustomStr2 = readReader.GetString(3);
                    }

                    if (!Convert.IsDBNull(readReader["Event_Type"]))
                    {
                        rec.CustomInt8 = readReader.GetInt64(4);
                    }

                    if (!Convert.IsDBNull(readReader["Issue_Time2"]))
                    {
                        rec.Datetime = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss.fff"); 
                    }

                    if (!Convert.IsDBNull(readReader["Msg"]))
                    {
                        rec.CustomStr3 = readReader.GetString(6);
                    }

                    if (!Convert.IsDBNull(readReader["Msg_Index"]))
                    {
                        rec.CustomInt9 = readReader.GetInt64(7);
                    }

                    if (!Convert.IsDBNull(readReader["P1"]))
                    {
                        rec.CustomStr4 = readReader.GetString(8);
                    }

                    if (!Convert.IsDBNull(readReader["P2"]))
                    {
                        rec.CustomStr5 = readReader.GetString(9);
                    }

                    if (!Convert.IsDBNull(readReader["Event_ID"]))
                    {
                        rec.CustomInt10 = readReader.GetInt64(10);
                    }



                   
                    
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data for EventLog");

                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(rec);
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal, virtualhost, rec);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "sendingg dataaaa");
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                    last_recordnumEvent = rec.CustomInt10;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is For Event Log " + last_recordnumEvent.ToString());

                    

                    if (usingRegistry)
                        Set_Registry(last_recordnumEvent.ToString());
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, last_recordnumEvent.ToString(), "", "", "", last_recdate);
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
                timerEvent.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");

                if (command != null && connection != null)
                {
                    command.Dispose();
                    connection.Close();
                }
                ac.CloseAccessConnection();
            }
        }

        public bool Set_Registry(string status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("AviraRecorder");
                rk.SetValue("LastRecordNum",status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Avira Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager Avira Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
