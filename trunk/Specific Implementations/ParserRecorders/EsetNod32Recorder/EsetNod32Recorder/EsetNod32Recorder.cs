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

namespace EsetNod32Recorder
{
    public class EsetNod32Recorder : CustomBase
    {
        private System.Timers.Timer timerEvent;
        private System.Timers.Timer timerFirewall;
        private System.Timers.Timer timerScan;
        private System.Timers.Timer timerThreat;

        private int trc_level = 3, timer_interval = 60000, max_record_send = 100, zone = 0;
        private long last_recordnumEvent;
        private long last_recordnumFirewall;
        private long last_recordnumScan;
        private long last_recordnumThreat;

        private string dblocationonServer = "ESETNOD32DB";
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, mcdb_name = "era.mdb", db_name, location, user, password, remote_host = "", last_recdate = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;

        public EsetNod32Recorder()
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

            timerFirewall = new System.Timers.Timer();
            timerFirewall.Elapsed += new System.Timers.ElapsedEventHandler(this.timerFirewall_Tick);
            timerFirewall.Interval = timer_interval;
            timerFirewall.Enabled = false;

            timerScan = new System.Timers.Timer();
            timerScan.Elapsed += new System.Timers.ElapsedEventHandler(this.timerScan_Tick);
            timerScan.Interval = timer_interval;
            timerScan.Enabled = false;

            timerThreat = new System.Timers.Timer();
            timerThreat.Elapsed += new System.Timers.ElapsedEventHandler(this.timerThreat_Tick);
            timerThreat.Interval = timer_interval;
            timerThreat.Enabled = false;

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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
                        db_name = "EsetNod32Db" + Id.ToString();

                        //if (Database.AddConnection(db_name, Database.Provider.SQLServer, remote_host, user, password, location))
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Successfully create McaffeeEpo DAL");
                        //else
                        //    L.Log(LogType.FILE, LogLevel.INFORM, "Failed on creating McaffeeEpo DAL");
                    }
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Init", er.ToString(), EventLogEntryType.Error);
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
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Impersonation is successful");
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\EsetNod32_Recorder" + Id + ".log";
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

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;

            string positions = LastPosition;
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\McaffeeEpoRecorder.log";
                db_name = rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("DBName").ToString();

                this.timerEvent.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Interval"));
                this.timerFirewall.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Interval"));
                this.timerScan.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Interval"));
                this.timerThreat.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("Interval"));

                mcdb_name = rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("MCDBName").ToString();
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("MaxRecordSend"));

                string positions = Convert.ToString(rk.OpenSubKey("Recorder").OpenSubKey("McaffeeEpoRecorder").GetValue("LastRecordNum"));
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
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Read Registry", er.ToString(), EventLogEntryType.Error);

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
            string readQueryFirewall = null;
            string readQueryScan = null;
            string readQueryThreat = null;

            AccessConnection ac = new AccessConnection(); ;
            OleDbDataReader readReader;

            try
            {
                readQueryEvent = "select MAX(ID) FROM EventLog";
                readQueryFirewall = "select MAX(ID) FROM FirewallLog";
                readQueryScan = "select MAX(ID) FROM ScanLog";
                readQueryThreat = "select MAX(ID) FROM ThreatLog";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query Event is " + readQueryEvent);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query Firewall is " + readQueryFirewall);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query Scan is " + readQueryScan);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query Threat is " + readQueryThreat);

                ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                readReader = ac.ExecuteAccessQuery(readQueryEvent);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");
                while (readReader.Read())
                {
                    last_recordnumEvent = Convert.ToInt64(readReader.GetInt32(0));
                }

                readReader = ac.ExecuteAccessQuery(readQueryEvent);

                while (readReader.Read())
                {
                    last_recordnumFirewall = Convert.ToInt64(readReader.GetInt32(0));
                }

                readReader = ac.ExecuteAccessQuery(readQueryEvent);

                while (readReader.Read())
                {
                    last_recordnumScan = Convert.ToInt64(readReader.GetInt32(0));
                }

                readReader = ac.ExecuteAccessQuery(readQueryEvent);

                while (readReader.Read())
                {
                    last_recordnumThreat = Convert.ToInt64(readReader.GetInt32(0));
                }

                string lastrecordsNumber = "Event is:" + last_recordnumEvent.ToString() + "," + "Firewall is:" + last_recordnumFirewall.ToString() + "," + "Scan is:" + last_recordnumScan.ToString() + "," + "Threat is:" + last_recordnumThreat.ToString();

                if (usingRegistry)
                    Set_Registry(lastrecordsNumber);
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, lastrecordsNumber, "", "", "", last_recdate);
                }

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Set_lastPosition() -->> Catch hata yakaldı.");

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

            if (timerFirewall != null)
                timerFirewall.Enabled = false;

            if (timerScan != null)
                timerScan.Enabled = false;

            if (timerThreat != null)
                timerThreat.Enabled = false;
        }

        private void timerEvent_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ValidateMe();

            timerEvent.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            rec.EventType = "Event";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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

                readQuery = "Select TOP " + max_record_send + " PrimaryServer,ClientName,ClientComputerName,ClientMacAddress,DateReceived,DateOccurred,UserName,PluginReportedID,PluginReportedName,LogLevel,EventText,ID from EventLog where ID > " + last_recordnumEvent + " ORDER BY ID";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query for EventLogTable is " + readQuery);

                ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                readReader = ac.ExecuteAccessQuery(readQuery);

                if (!readReader.HasRows) // hatalı access kayıtları için.  hatalı kayıtları atlar...
                {
                    //string readQuery1 = "select MAX(ID) FROM EventLog";
                    //long max_record_num = 0;
                    //max_record_num = ac.ExecuteScalarQuery(readQuery1);

                    string readQuery1 = "select MAX(ID) FROM EventLog";
                    OleDbDataReader readReader22 = ac.ExecuteAccessQuery(readQuery1);
                    long max_record_num = 0;

                    while (readReader22.Read())
                    {
                        try
                        {
                            max_record_num = Convert.ToInt64(readReader22.GetDecimal(0));//111111
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Error in max_record_num = Convert.ToInt64(readReader22.GetDecimal(0)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Value : " + readReader22.GetValue(0).ToString());
                        }
                    }

                    L.Log(LogType.FILE, LogLevel.INFORM, "Maximum Record Num for EventLog Table is : " + max_record_num);

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
                    rec.LogName = "EsetNod32 Recorder";

                    if (!Convert.IsDBNull(readReader["PrimaryServer"]))
                    {
                        rec.CustomStr1 = readReader.GetString(0).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["ClientName"]))
                    {
                        rec.UserName = readReader.GetString(1).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["ClientComputerName"]))
                    {
                        rec.ComputerName = readReader.GetString(2).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["ClientMacAddress"]))
                    {
                        rec.CustomStr2 = readReader.GetString(3).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["DateReceived"]))
                    {
                        rec.CustomStr3 = readReader.GetDateTime(4).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    }

                    if (!Convert.IsDBNull(readReader["DateOccurred"]))
                    {
                        rec.CustomStr4 = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    }

                    if (!Convert.IsDBNull(readReader["UserName"]))
                    {
                        rec.CustomStr5 = readReader.GetString(6).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["PluginReportedID"]))
                    {
                        rec.CustomInt1 = readReader.GetInt32(7);
                    }

                    if (!Convert.IsDBNull(readReader["PluginReportedName"]))
                    {
                        rec.CustomStr6 = readReader.GetString(8).ToString();
                    }

                    if (!Convert.IsDBNull(readReader["LogLevel"]))
                    {
                        try
                        {
                            rec.CustomInt8 = Convert.ToInt64(readReader.GetDecimal(9));//22222
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Error in rec.CustomInt8 = Convert.ToInt64(readReader.GetDecimal(9)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Value : " + readReader.GetValue(9).ToString());
                        }
                    }

                    if (!Convert.IsDBNull(readReader["EventText"]))
                    {
                        rec.CustomStr7 = readReader.GetString(10).ToString();
                    }

                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    try
                    {
                        rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(11));//3333333
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Error in rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(11)) : " + ex.ToString());
                        L.Log(LogType.FILE, LogLevel.ERROR, " timerEvent_Tick() -->> Value : " + readReader.GetValue(11).ToString());
                    }
                    rec.CustomInt7 = 1;

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
                    last_recordnumEvent = rec.CustomInt6;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is For Event Log " + last_recordnumEvent.ToString());

                    string lastrecordsNumber = "Event is:" + last_recordnumEvent.ToString() + "," + "Firewall is:" + last_recordnumFirewall.ToString() + "," + "Scan is:" + last_recordnumScan.ToString() + "," + "Threat is:" + last_recordnumThreat.ToString();

                    if (usingRegistry)
                        Set_Registry(lastrecordsNumber);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, lastrecordsNumber, "", "", "", last_recdate);
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
                timerEvent.Enabled = false;
                timerFirewall.Enabled = true;
                timerScan.Enabled = false;
                timerThreat.Enabled = false;

                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");

                if (command != null && connection != null)
                {
                    command.Dispose();
                    connection.Close();
                }
                ac.CloseAccessConnection();
            }
        }

        private void timerFirewall_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ValidateMe();
            timerFirewall.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            rec.EventType = "Firewall";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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

                readQuery = "Select TOP " + max_record_send + " PrimaryServer, ClientName, ClientComputerName, ClientMacAddress, DateReceived, DateOccurred, UserName, LogLevel, Event, Source, Target, Protocol, RuleName, ID from FirewallLog where ID > " + last_recordnumFirewall + " ORDER BY ID";

                L.Log(LogType.FILE, LogLevel.DEBUG, " Query for FireWallLogTable is " + readQuery);

                ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                readReader = ac.ExecuteAccessQuery(readQuery);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                if (!readReader.HasRows)
                {
                    string readQuery1 = "select MAX(ID) AS MaxId FROM FirewallLog";

                    OleDbDataReader readReader22 = ac.ExecuteAccessQuery(readQuery1);
                    long max_record_num = 0;

                    while (readReader22.Read())
                    {
                        if (readReader22["MaxId"] != DBNull.Value)
                        {
                            max_record_num = Convert.ToInt64(readReader22["MaxId"]);
                        }
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Maximum Record Num for FirewallLog Table is : " + max_record_num.ToString());

                    if (!readReader22.IsClosed)
                    {
                        readReader22.Close();
                    }

                    if (last_recordnumFirewall < max_record_num)
                    {
                        last_recordnumFirewall += 1;
                    }
                }

                while (readReader.Read())
                {
                    rec.LogName = "EsetNod32 Recorder";

                    if (!Convert.IsDBNull(readReader["PrimaryServer"]))
                    {
                        rec.CustomStr1 = readReader.GetString(0).ToString();
                    }
                    if (!Convert.IsDBNull(readReader["ClientName"]))
                    {
                        rec.UserName = readReader.GetString(1).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ClientComputerName"]))
                    {
                        rec.ComputerName = readReader.GetString(2).ToString(); //  
                    }
                    if (!Convert.IsDBNull(readReader["ClientMacAddress"]))
                    {
                        rec.CustomStr2 = readReader.GetString(3).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["DateReceived"]))
                    {
                        rec.CustomStr3 = readReader.GetDateTime(4).ToString("yyyy/MM/dd HH:mm:ss.fff");  // 
                    }
                    if (!Convert.IsDBNull(readReader["DateOccurred"]))
                    {
                        rec.CustomStr4 = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss.fff"); // 
                    }
                    if (!Convert.IsDBNull(readReader["UserName"]))
                    {
                        rec.CustomStr5 = readReader.GetString(6).ToString(); // 
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "hatadan önce");

                    //Hataya düşen yer!!!!!!!!!!!!!!
                    //if (!Convert.IsDBNull(readReader["LogLevel"]))
                    //{
                    //    rec.CustomInt8 = Convert.ToInt64(readReader.GetDecimal(7)); // 
                    //}

                    L.Log(LogType.FILE, LogLevel.DEBUG, "hatadan sonra");

                    if (!Convert.IsDBNull(readReader["Event"]))
                    {
                        rec.CustomStr6 = readReader.GetString(8).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["Source"]))
                    {
                        rec.CustomStr7 = readReader.GetString(9).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["Target"]))
                    {
                        rec.CustomStr8 = readReader.GetString(10).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["Protocol"]))
                    {
                        rec.CustomStr9 = readReader.GetString(11).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["RuleName"]))
                    {
                        rec.CustomStr10 = readReader.GetString(12).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ID"]))
                    {
                        try
                        {

                            rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(13)); // 
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerFirewall_Tick() -->> Error in rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(13)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerFirewall_Tick() -->> Value : " + readReader.GetValue(13).ToString());
                        }
                    }
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"); //
                    rec.CustomInt7 = 2;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data for Firewall");

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
                    last_recordnumFirewall = rec.CustomInt6;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is For Firewall Log " + last_recordnumFirewall.ToString());

                    string lastrecordsNumber = "Event is:" + last_recordnumEvent.ToString() + "," + "Firewall is:" + last_recordnumFirewall.ToString() + "," + "Scan is:" + last_recordnumScan.ToString() + "," + "Threat is:" + last_recordnumThreat.ToString();

                    if (usingRegistry)
                        Set_Registry(lastrecordsNumber);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, lastrecordsNumber, "", "", "", last_recdate);
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish getting the data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "bu hata alnına sürekli giriyor");
            }
            finally
            {
                timerEvent.Enabled = false;
                timerFirewall.Enabled = false;
                timerScan.Enabled = true;
                timerThreat.Enabled = false;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");

                if (command != null && connection != null)
                {
                    command.Dispose();
                    connection.Close();
                }
                ac.CloseAccessConnection();
            }
        }

        private void timerScan_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ValidateMe();
            timerScan.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            rec.EventType = "Scan";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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

                readQuery = "Select TOP " + max_record_send + " PrimaryServer, ClientName, ClientComputerName, ClientMacAddress, DateReceived, DateOccurred, UserName, ScannerReportedID, ScannerReportedName, TaskID, TaskType, Description, Status, Scanned, Infected, Cleaned, Details, ID from ScanLog where ID > " + last_recordnumScan + " ORDER BY ID";
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query for ScanLogTable is " + readQuery);

                ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                readReader = ac.ExecuteAccessQuery(readQuery);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                if (!readReader.HasRows)
                {
                    string readQuery1 = "select MAX(ID) FROM ScanLog";
                    OleDbDataReader readReader22 = ac.ExecuteAccessQuery(readQuery1);
                    long max_record_num = 0;

                    while (readReader22.Read())
                    {
                        max_record_num = Convert.ToInt64(readReader22.GetDecimal(0));
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Maximum Record Num is for ScanLog Table : " + max_record_num);

                    if (!readReader22.IsClosed)
                    {
                        readReader22.Close();
                    }

                    if (last_recordnumScan < max_record_num)
                    {
                        last_recordnumScan += 1;
                    }
                }

                while (readReader.Read())
                {
                    rec.LogName = "EsetNod32 Recorder";

                    if (!Convert.IsDBNull(readReader["PrimaryServer"]))
                    {
                        rec.CustomStr1 = readReader.GetString(0).ToString();
                    }
                    if (!Convert.IsDBNull(readReader["ClientName"]))
                    {
                        rec.UserName = readReader.GetString(1).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ClientComputerName"]))
                    {
                        rec.ComputerName = readReader.GetString(2).ToString(); //  
                    }
                    if (!Convert.IsDBNull(readReader["ClientMacAddress"]))
                    {
                        rec.CustomStr2 = readReader.GetString(3).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["DateReceived"]))
                    {
                        rec.CustomStr3 = readReader.GetDateTime(4).ToString("yyyy/MM/dd HH:mm:ss.fff");  // 
                    }
                    if (!Convert.IsDBNull(readReader["DateOccurred"]))
                    {
                        rec.CustomStr4 = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss.fff"); // 
                    }
                    if (!Convert.IsDBNull(readReader["UserName"]))
                    {
                        rec.CustomStr5 = readReader.GetString(6).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ScannerReportedID"]))
                    {
                        rec.EventId = Convert.ToInt64(readReader.GetInt32(7)); // 
                    }
                    if (!Convert.IsDBNull(readReader["ScannerReportedName"]))
                    {
                        rec.CustomStr6 = readReader.GetString(8).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["TaskID"]))
                    {
                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(readReader.GetInt64(9));// 
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 Was Setted To 0 -->> " + ex.Message);
                            rec.CustomInt1 = 0;
                        }
                    }
                    if (!Convert.IsDBNull(readReader["TaskType"]))
                    {
                        try
                        {
                            rec.CustomInt2 = Convert.ToInt32(readReader.GetInt64(10)); // 
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 Was Setted To 0 -->> " + ex.Message);
                            rec.CustomInt2 = 0;
                        }
                    }
                    if (!Convert.IsDBNull(readReader["Description"]))
                    {
                        rec.CustomStr7 = readReader.GetString(11).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["Status"]))
                    {
                        rec.CustomStr8 = readReader.GetString(12).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["Scanned"]))
                    {
                        rec.CustomInt8 = Convert.ToInt64(readReader.GetInt32(13)); // 
                    }
                    if (!Convert.IsDBNull(readReader["Infected"]))
                    {
                        rec.CustomInt9 = Convert.ToInt64(readReader.GetInt32(14)); // 
                    }
                    if (!Convert.IsDBNull(readReader["Cleaned"]))
                    {
                        rec.CustomInt10 = Convert.ToInt64(readReader.GetInt32(15)); // 
                    }
                    if (!Convert.IsDBNull(readReader["Details"]))
                    {
                        rec.CustomStr9 = readReader.GetString(16).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ID"]))
                    {
                        rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(17)); // 
                    }
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"); //
                    rec.CustomInt7 = 3;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data for Scan");

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
                    last_recordnumScan = rec.CustomInt6;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is For Scan Log " + last_recordnumScan.ToString());

                    string lastrecordsNumber = "Event is:" + last_recordnumEvent.ToString() + "," + "Firewall is:" + last_recordnumFirewall.ToString() + "," + "Scan is:" + last_recordnumScan.ToString() + "," + "Threat is:" + last_recordnumThreat.ToString();

                    if (usingRegistry)
                        Set_Registry(lastrecordsNumber);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, lastrecordsNumber, "", "", "", last_recdate);
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
                timerEvent.Enabled = false;
                timerFirewall.Enabled = false;
                timerScan.Enabled = false;
                timerThreat.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");

                if (command != null && connection != null)
                {
                    command.Dispose();
                    connection.Close();
                }
                ac.CloseAccessConnection();
            }
        }

        private void timerThreat_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            ValidateMe();
            timerThreat.Enabled = false;
            CustomBase.Rec rec = new CustomBase.Rec();
            rec.EventType = "Threat";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on McaffeeEpo Recorder functions may not be running");
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

                readQuery = "Select TOP " + max_record_send + " PrimaryServer, ClientName, ClientComputerName, ClientMacAddress, DateReceived, DateOccurred, UserName, ScannerReportedID, ScannerReportedName, LogLevel, Object, Name, Virus, ActionTaken, Info,ID from ThreatLog where ID > " + last_recordnumThreat + " ORDER BY ID";
                ac.OpenAccessConnection(remote_host, dblocationonServer, location);
                readReader = ac.ExecuteAccessQuery(readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Query for ThreatLogTable is " + readQuery);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing the query");

                if (!readReader.HasRows)
                {
                    string readQuery1 = "select MAX(ID) FROM ThreatLog";
                    OleDbDataReader readReader22 = ac.ExecuteAccessQuery(readQuery1);
                    long max_record_num = 0;
                    while (readReader22.Read())
                    {
                        try
                        {
                            max_record_num = Convert.ToInt64(readReader22.GetDecimal(0));
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Error in max_record_num = Convert.ToInt64(readReader22.GetDecimal(0)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Value : " + readReader22.GetValue(0).ToString());
                        }
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Maximum Record Num for ThreatLog Table is: " + max_record_num);
                    if (!readReader22.IsClosed)
                    {
                        readReader22.Close();
                    }

                    if (last_recordnumThreat < max_record_num)
                    {
                        last_recordnumThreat += 1;
                    }
                }

                while (readReader.Read())
                {
                    rec.LogName = "EsetNod32 Recorder";
                    if (!Convert.IsDBNull(readReader["PrimaryServer"]))
                    {
                        rec.Description = readReader.GetString(0).ToString();
                    }
                    if (!Convert.IsDBNull(readReader["ClientName"]))
                    {
                        rec.UserName = readReader.GetString(1).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ClientComputerName"]))
                    {
                        rec.ComputerName = readReader.GetString(2).ToString(); //  
                    }
                    if (!Convert.IsDBNull(readReader["ClientMacAddress"]))
                    {
                        rec.CustomStr2 = readReader.GetString(3).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["DateReceived"]))
                    {
                        rec.CustomStr3 = readReader.GetDateTime(4).ToString("yyyy/MM/dd HH:mm:ss.fff");  // 
                    }
                    if (!Convert.IsDBNull(readReader["DateOccurred"]))
                    {
                        rec.CustomStr4 = readReader.GetDateTime(5).ToString("yyyy/MM/dd HH:mm:ss.fff"); // 
                    }
                    if (!Convert.IsDBNull(readReader["UserName"]))
                    {
                        rec.CustomStr5 = readReader.GetString(6).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["ScannerReportedID"]))
                    {
                        rec.CustomInt8 = Convert.ToInt64(readReader.GetInt32(7)); // 
                    }

                    if (!Convert.IsDBNull(readReader["ScannerReportedName"]))
                    {
                        rec.CustomStr6 = readReader.GetString(8).ToString(); // 
                    }
                    if (!Convert.IsDBNull(readReader["LogLevel"]))
                    {
                        try
                        {
                            rec.CustomInt9 = Convert.ToInt64(readReader.GetDecimal(9));// 
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Error in rec.CustomInt9 = Convert.ToInt64(readReader.GetDecimal(9)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Value : " + readReader.GetValue(9).ToString());
                        }
                    }
                    if (!Convert.IsDBNull(readReader["Object"]))
                    {
                        rec.CustomStr7 = readReader.GetString(10); // 
                    }
                    if (!Convert.IsDBNull(readReader["Name"]))
                    {
                        rec.CustomStr8 = readReader.GetString(11); // 
                    }
                    if (!Convert.IsDBNull(readReader["Virus"]))
                    {
                        rec.CustomStr9 = readReader.GetString(12); // 
                    }
                    if (!Convert.IsDBNull(readReader["ActionTaken"]))
                    {
                        rec.CustomStr10 = readReader.GetString(13); // 
                    }
                    if (!Convert.IsDBNull(readReader["Info"]))
                    {
                        rec.CustomStr1 = readReader.GetString(14); // 
                    }
                    if (!Convert.IsDBNull(readReader["ID"]))
                    {
                        try
                        {
                            rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(15)); // 
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Error in rec.CustomInt6 = Convert.ToInt64(readReader.GetDecimal(15)) : " + ex.ToString());
                            L.Log(LogType.FILE, LogLevel.ERROR, " timerThreat_Tick() -->> Value : " + readReader.GetValue(15).ToString());
                        }
                    }
                    rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"); //
                    rec.CustomInt7 = 4;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data For Threat");

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
                    last_recordnumThreat = rec.CustomInt6;
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Record Number is For threatlog Log " + last_recordnumThreat.ToString());

                    string lastrecordsNumber = "Event is:" + last_recordnumEvent.ToString() + "," + "Firewall is:" + last_recordnumFirewall.ToString() + "," + "Scan is:" + last_recordnumScan.ToString() + "," + "Threat is:" + last_recordnumThreat.ToString();

                    if (usingRegistry)
                        Set_Registry(lastrecordsNumber);
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetReg(Id, lastrecordsNumber, "", "", "", last_recdate);
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
                timerFirewall.Enabled = false;
                timerScan.Enabled = false;
                timerThreat.Enabled = false;

                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");

                if (!readReader.IsClosed)
                {
                    readReader.Close();
                }

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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("McaffeeEpoRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McaffeeEpo Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
