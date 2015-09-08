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
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace NtEventLog2008TRV1_1_1_Recorder
{
    public class NtEventLog2008TRV1_1_1_Recorder : CustomBase
    {

        #region "Global Variables"

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0, zone = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position = "0", first_position = "0", remote_host = "", location, user, password, last_recdate = "";
        private bool reg_flag = false, start_state = true;
        protected bool usingRegistry = true;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private ConnectionOptions co;
        private ManagementScope scope;
        private string EventIDToFilter = "";

        #endregion

        #region "Impersonation Parameters"

        //Validation
        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

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

        #endregion

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

        public NtEventLog2008TRV1_1_1_Recorder()
        {
            InitializeComponent();
        }

        public virtual void Stop()
        {
            timer1.Stop();

            L.Log(LogType.FILE, LogLevel.DEBUG, "Recorder has been Stopped");
        }
        public override void Init()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;
            timer1.AutoReset = false;

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog 2008 Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
            }
            else
            {
                if (!reg_flag)
                {
                    if (!Get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog 2008 Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "Exiting Init method");
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Trace Level"));
                remote_host = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NtEventlog2008Recorder" + remote_host + ".log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("MaxLineToWait"));
                fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("FromEndOnLoss"));
                last_position = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("LastPosition").ToString();
                location = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Location").ToString();
                user = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlog2008Recorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NtEventlog 2008 Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NtEventLog2008TRV1_1_1_Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NtEventlog 2008 Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Evenlog Yeni");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Enter timer_tick1 method");
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");

                if (!reg_flag & usingRegistry)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog 2008 Recorder functions may not be running");
                            return;
                        }

                    reg_flag = true;

                }
                else
                {
                    if (!Get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log directory");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog 2008 Recorder functions may not be running");
                            return;
                        }
                }

                if (start_state & fromend == 1)
                {
                    if (!Set_LastPosition())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on setting the last position see log for more details");
                    }
                    start_state = false;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start Connecting host:");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Remote Host :" + remote_host);
                if (remote_host == "")
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Localden Okuyor. ");
                    co = new ConnectionOptions();
                    co.Timeout = new TimeSpan(0, 10, 0);
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.PacketPrivacy;
                    scope = new ManagementScope(@"\\localhost\root\cimv2", co);
                }
                else
                {
                    co = new ConnectionOptions();
                    co.Username = user;
                    co.Password = password;
                    co.Timeout = new TimeSpan(0, 10, 0);
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.PacketPrivacy;
                    scope = new ManagementScope(@"\\" + remote_host + @"\root\cimv2", co);
                }

                scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                scope.Connect();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");

                // default blocksize = 1, larger value may increase network throughput
                EnumerationOptions opt = new EnumerationOptions();
                opt.BlockSize = 1000;

                first_position = last_position;

                if (Convert.ToInt64(first_position) <= 0)
                {
                    first_position = setNewLastPosition(scope, opt);
                    last_position = first_position;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "first_position is :" + first_position);

                SelectQuery query = null;
                if (EventIDToFilter == "")
                {
                    query = new SelectQuery("select CategoryString,ComputerName, EventIdentifier,Type,Message,RecordNumber,SourceName," +
                        "TimeGenerated,User from Win32_NtLogEvent where Logfile ='" + location + "' and RecordNumber >=" + first_position +
                        " and RecordNumber <" + Convert.ToString(Convert.ToInt64(first_position) + max_line_towait));

                }
                else
                {
                    string[] EventIDArr = EventIDToFilter.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string FilterText = "";

                    if (EventIDArr.Length < 2)
                    {
                        FilterText = "EventIdentifier <> " + EventIDArr[0];
                    }
                    else
                    {
                        FilterText = "EventIdentifier <> " + EventIDArr[0] + " and ";

                        for (int f = 0; f < EventIDArr.Length - 2; f++)
                        {
                            FilterText += "EventIdentifier <> " + EventIDArr[f] + " and ";
                        }
                        FilterText += "EventIdentifier <> " + EventIDArr[EventIDArr.Length - 1];

                        query = new SelectQuery("select CategoryString,ComputerName, EventIdentifier,Type,Message,RecordNumber,SourceName," +
                            "TimeGenerated,User from Win32_NtLogEvent where " + FilterText + " and Logfile ='" +
                            location + "' and RecordNumber >=" + first_position + " and RecordNumber <" +
                            Convert.ToString(Convert.ToInt64(first_position) + max_line_towait));
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "query is :" + query.QueryString);
                L.Log(LogType.FILE, LogLevel.DEBUG, "last position :" + last_position);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start collection event logs of :" + location);

                bool resetposition = true;

                List<ManagementObject> moList = new List<ManagementObject>();

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "using ManagementObjectSearcher");

                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        moList.Add(mObject);
                    }
                }
                L.Log(LogType.FILE, LogLevel.INFORM, "Time passed to retrieve " + moList.Count + " records through WMI: ");

                foreach (ManagementObject mo in moList)
                {
                    resetposition = false;

                    if (Convert.ToInt64(mo["RecordNumber"]) == Convert.ToInt64(first_position))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "first_position==mo[RecordNumber]:" + first_position);
                    }
                    else
                    {
                        if (!Send_Record(mo))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Error on sending record with record number" + mo["RecordNumber"].ToString());
                        }
                        if (Convert.ToInt64(last_position) < Convert.ToInt64(mo["RecordNumber"]))
                        {
                            last_position = mo["RecordNumber"].ToString();
                            L.Log(LogType.FILE, LogLevel.DEBUG, "new last_position : " + last_position.ToString());
                            string dat = mo["TimeGenerated"].ToString().Split('.')[0];
                            L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated1:" + dat);
                            last_recdate = dat.Substring(0, 4) + "/" + dat.Substring(4, 2) + "/" + dat.Substring(6, 2) + " " + dat.Substring(8, 2) + ":" + dat.Substring(10, 2) + ":" + dat.Substring(12, 2); //+ "." + dat.Substring(14, 2);                                 
                            last_recdate = Convert.ToDateTime(last_recdate).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                        }
                    }
                }

                DateTime afterRecordsSent = DateTime.Now;
                L.Log(LogType.FILE, LogLevel.INFORM, "Time passed to send " + moList.Count + " records to memory: ");

                if (resetposition)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "resetposition:No data come Start Reseting the position of the log files");
                    //Set_LastPosition();
                    setNewLastPosition(scope, opt);
                }

                Set_Registry(last_position, last_recdate);

            }

            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;

                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Exiting timer_tick1 method");

            }
        }

        private string setNewLastPosition(ManagementScope scope, EnumerationOptions opt)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "setNewLastPosition()| Start: Oldfirst_position:" + first_position);
            Int64 lPosition = 0;
            SelectQuery query = null;
            try
            {

                string sWhereClause = "";
                ValidateMe();
                EventLog ev;
                if (remote_host == "")
                    ev = new EventLog(location);
                else
                    ev = new EventLog(location, remote_host);

                string sTarih = ManagementDateTimeConverter.ToDmtfDateTime(DateTime.Now.AddHours(-2));

                if (EventIDToFilter == "")
                {
                    query = new SelectQuery("select CategoryString,ComputerName, EventIdentifier,Type,Message,RecordNumber,SourceName," +
                        "TimeGenerated,User from Win32_NtLogEvent where Logfile ='" + location + "' AND TimeGenerated>'" + sTarih + "'");

                }
                else
                {
                    string[] EventIDArr = EventIDToFilter.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string FilterText = "";

                    if (EventIDArr.Length < 2)
                    {
                        FilterText = "EventIdentifier <> " + EventIDArr[0];
                    }
                    else
                    {
                        FilterText = "EventIdentifier <> " + EventIDArr[0] + " and ";
                        for (int f = 0; f < EventIDArr.Length - 2; f++)
                        {
                            FilterText += "EventIdentifier <> " + EventIDArr[f] + " and ";
                        }
                        FilterText += "EventIdentifier <> " + EventIDArr[EventIDArr.Length - 1];
                    }
                    query = new SelectQuery("select CategoryString,ComputerName, EventIdentifier,Type,Message,RecordNumber,SourceName," +
                        "TimeGenerated,User from Win32_NtLogEvent where " + FilterText + " and Logfile ='" +
                        location + "' AND TimeGenerated>'" + sTarih + "'");
                }


                L.Log(LogType.FILE, LogLevel.DEBUG, "setNewLastPosition()| query:" + query.QueryString);

                List<ManagementObject> moList = new List<ManagementObject>();
                opt.Timeout = System.TimeSpan.MaxValue;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                {

                    //Int32 count = ev.Entries.Count;
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        lPosition = Convert.ToInt64(mObject["RecordNumber"]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "setNewLastPosition()| :lPosition" + lPosition);
                        if (fromend == 0)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "setNewLastPosition()| fromend==1:lPosition" + lPosition);
                            lPosition = lPosition - ev.Entries.Count;
                        }
                        break;
                    }
                }

            }
            catch (Exception exp)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "setNewLastPosition()| " + exp.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "setNewLastPosition()| query:" + query.QueryString);
                return "0";
            }

            L.Log(LogType.FILE, LogLevel.DEBUG, "setNewLastPosition()| End :lPosition:" + lPosition.ToString());
            return lPosition.ToString();
        }

        public bool Set_Registry(string status, string lastrecdate)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistry)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NtEventlog2008Recorder");
                    rk.SetValue("LastPosition", status);
                    rk.Close();
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, status, "", "", "", lastrecdate);
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager NtEventlog 2008 Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
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

        public bool Set_LastPosition()
        {
            try
            {
                ValidateMe();
                EventLog ev;
                if (remote_host == "")
                    ev = new EventLog(location);
                else
                    ev = new EventLog(location, remote_host);

                //EventLog ev = new EventLog(location, remote_host);
                if (fromend == 1)
                {

                    L.Log(LogType.FILE, LogLevel.WARN, "Fromend Position is set to: " + last_position.ToString());
                    last_position = ev.Entries[ev.Entries.Count - 1].Index.ToString();

                    last_recdate = ev.Entries[ev.Entries.Count - 1].TimeGenerated.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm.ss");
                }
                else
                {
                    /*
                    if(start_state) 
                    {
                        if (Convert.ToInt32(last_position) < ev.Entries[1].Index)
                            last_position = ev.Entries[1].Index.ToString();
                    }
                    else*/
                    L.Log(LogType.FILE, LogLevel.WARN, "Set_lastPosition() --> ev.Entries[1].Index.ToString() = " + ev.Entries[1].Index.ToString());

                    L.Log(LogType.FILE, LogLevel.WARN, "Fromend Position is set to: " + last_position.ToString());

                    L.Log(LogType.FILE, LogLevel.WARN, "Position is set to: " + last_position.ToString());
                    last_position = ev.Entries[1].Index.ToString();
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }

            /*
            int first_rec=0, second_rec=0, record_count=0;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start Connecting host:");
                co = new ConnectionOptions();
                co.Username = user;
                co.Password = password;
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.PacketPrivacy;
                //co.Authority = "ntlmdomain:" +remote_host;

                if (remote_host == "")
                    remote_host = "localhost";

                scope = new ManagementScope(@"\\" + remote_host + @"\root\cimv2", co);
                scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                scope.Connect();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");

                // default blocksize = 1, larger value may increase network throughput
                EnumerationOptions opt = new EnumerationOptions();
                opt.BlockSize = 1000;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start executing query for getting the last position");

                SelectQuery query = new SelectQuery("select RecordNumber from Win32_NtLogEvent where Logfile ='" + location + "'");

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing query for getting the last position");

                int i = 1;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                    foreach (ManagementObject mc in searcher.Get())
                    {
                        if (i == 2)
                        {
                            second_rec = Convert.ToInt32(mc["RecordNumber"]);
                            break;
                        }
                        if (i == 1)
                        {
                            record_count = searcher.Get().Count;
                            first_rec = Convert.ToInt32(mc["RecordNumber"]);
                            i++;
                        }
                    }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);
                L.Log(LogType.FILE, LogLevel.DEBUG, "First record is: "+first_rec.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Second record is: "+second_rec.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Record Count: "+record_count.ToString());

                if (first_rec > second_rec)
                {
                    if (fromend == 1)
                        last_position = Convert.ToString(first_rec);
                    else
                    {
                        if (Convert.ToInt32(last_position) < first_rec - record_count)
                            last_position = Convert.ToString(first_rec - record_count);
                    }
                }
                else
                {
                    if (fromend == 1)
                        last_position = Convert.ToString(first_rec + record_count);
                    else
                    {
                        if (Convert.ToInt32(last_position) < first_rec)
                            last_position = Convert.ToString(first_rec);
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
            */
        }

        public override void Clear()
        {
            if (timer1 != null)
            {
                timer1.Enabled = false;
                timer1.Dispose();
            }
        }

        public bool Send_Record(ManagementObject mo)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {

                #region "Parse Log:"

                rec.LogName = "NT-" + location;
                rec.CustomStr8 = remote_host;
                if (mo["ComputerName"] != null)
                {
                    rec.ComputerName = mo["ComputerName"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Computername:" + rec.ComputerName);
                }
                if (mo["CategoryString"] != null)
                {
                    rec.EventCategory = mo["CategoryString"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory:" + rec.EventCategory);
                }

                if (mo["EventIdentifier"] != null)
                {
                    rec.EventId = Convert.ToInt64(mo["EventIdentifier"]);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventIdentifier:" + rec.EventId);
                }

                if (mo["Type"] != null)
                {
                    //Audit Failure => FailureAudit
                    //Audit Success => SuccessAudit
                    string evt = mo["Type"].ToString();

                    if (evt == "Audit Failure")
                        evt = "FailureAudit";
                    if (evt == "Denetim Başarısız")
                        evt = "FailureAudit";
                    if (evt == "Audit Success")
                        evt = "SuccessAudit";
                    if (evt == "Denetim Başarılı")
                        evt = "SuccessAudit";

                    rec.EventType = evt;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType:" + rec.EventType);

                }
                if (mo["RecordNumber"] != null)
                {
                    rec.CustomInt10 = Convert.ToInt64(mo["RecordNumber"]);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "RecordNumber:" + rec.Recordnum);
                }
                if (mo["SourceName"] != null)
                {
                    rec.SourceName = mo["SourceName"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName:" + rec.SourceName);
                }
                if (mo["User"] != null)
                {
                    rec.UserName = mo["User"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "User:" + rec.UserName);
                }
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "mo[Message] :" + mo["Message"].ToString());
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "mo[Message] :" + "");
                }


                if (mo["Message"] != null)
                {

                    if (mo["Message"].ToString().Length>4000)
                    {
                        rec.Description = mo["Message"].ToString().Substring(0, 3999);
                    }
                    else
                    {
                        rec.Description = mo["Message"].ToString();    
                    }

                    rec.Description = mo["Message"].ToString();
                    //L.Log(LogType.FILE, LogLevel.DEBUG, "Message:" + rec.Description);
                    string[] DescArr = rec.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    bool subjectMode = false;
                    bool objectMode = false;
                    bool targetMode = false;
                    bool accessMode = false;
                    bool processMode = false;
                    bool applMode = false;
                    bool networkMode = false;
                    bool authenMode = false;
                    bool dummyAccessControl = false;
                    bool newAccountMode = false;

                    for (int i = 0; i < DescArr.Length; i++)
                    {
                        if (!DescArr[i].Contains(":"))
                        {
                            if (accessMode)
                            {
                                rec.CustomStr7 += " " + DescArr[i].Trim();
                                if (rec.CustomStr7.Length > 900)
                                {
                                    rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                                }
                            }
                        }
                        else
                        {
                            string[] lineArr = DescArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "DescArr[" + i + "]:" + DescArr[i]);


                            if (DescArr[i].Contains("Logon Type"))
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Logon Type Bulundu:" + DescArr[i]);
                                string logontypestr = DescArr[i].Split(':')[1].Trim();
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Logon Type Değeri:" + logontypestr);
                                if (logontypestr != "")
                                {
                                    rec.CustomInt3 = Convert.ToInt32(logontypestr);
                                }
                            }

                            if (lineArr[lineArr.Length - 1].Trim() == "")
                            {
                                #region Mode
                                if (lineArr[0].Trim() == "Application Information")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = true;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Network Information")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = true;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Subject"
                                      || lineArr[0].Trim() == "New Logon"
                                      || lineArr[0].Trim() == "Account Whose Credentials Were Used"
                                      || lineArr[0].Trim() == "Credentials Which Were Replayed"
                                      || lineArr[0].Trim() == "Account That Was Locked Out"
                                      || lineArr[0].Trim() == "New Computer Account"
                                      || lineArr[0].Trim() == "Computer Account That Was Changed"
                                      || lineArr[0].Trim() == "Source Account")
                                {
                                    subjectMode = true;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Target"
                                    || lineArr[0].Trim() == "Target Account"
                                    || lineArr[0].Trim() == "Target Computer"
                                    || lineArr[0].Trim() == "Target Server")
                                {
                                    subjectMode = true;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Object")
                                {
                                    subjectMode = false;
                                    objectMode = true;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Process Information" || lineArr[0].Trim() == "Process")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = true;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Access Request Information")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = true;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "Detailed Authentication Information")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = true;
                                    newAccountMode = false;
                                }
                                else if (lineArr[0].Trim() == "New Account")
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = true;
                                }
                                else
                                {
                                    subjectMode = false;
                                    objectMode = false;
                                    targetMode = false;
                                    accessMode = false;
                                    processMode = false;
                                    applMode = false;
                                    networkMode = false;
                                    authenMode = false;
                                    newAccountMode = false;
                                }
                                #endregion
                            }
                            else
                            {
                                if (subjectMode)
                                {
                                    #region SubjectMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "User Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;

                                        //case "Security ID":
                                        //    if (rec.CustomStr2 == null)
                                        //    {
                                        //        rec.CustomStr2 = appendArrayElements(lineArr);
                                        //    }
                                        //    break;
                                        case "Logon ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt6 = 0;
                                            }
                                            break;
                                        case "Client Context ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt6 = 0;
                                            }
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (targetMode)
                                {

                                    #region TargetMode==true

                                    switch (lineArr[0].Trim())
                                    {
                                        case "User Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        //case "Target Server Name":
                                        //    rec.CustomStr2 = appendArrayElements(lineArr);
                                        //    break;
                                        case "Account Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Old Account Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "New Account Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;

                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (objectMode)
                                {
                                    #region ObjectMode=True
                                    switch (lineArr[0].Trim())
                                    {

                                        case "Object Name":
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;
                                        case "Object Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Operation Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Handle ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt7 = 0;
                                            }
                                            break;
                                        case "Primary User Name":
                                            if (rec.CustomStr1 == null)
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client User Name":
                                            if (rec.CustomStr2 == null)
                                            {
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (accessMode)
                                {
                                    #region AccessMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Accesses":
                                            if (rec.CustomStr7 == null)
                                            {
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                if (rec.CustomStr7.Length > 900)
                                                {
                                                    rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                                                }
                                                dummyAccessControl = true;
                                            }
                                            break;
                                        case "Access Mask":
                                            if (dummyAccessControl)
                                            {
                                                rec.CustomStr7 += " " + appendArrayElements(lineArr);
                                                if (rec.CustomStr7.Length > 900)
                                                {
                                                    rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                                                }
                                            }
                                            break;
                                        case "Operation Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (processMode)
                                {
                                    #region ProcessMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Duration":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "PID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (applMode)
                                {
                                    #region ApplMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Logon Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Duration":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Application Instance ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Application Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (networkMode)
                                {

                                    //L.Log(LogType.FILE, LogLevel.DEBUG, "lineArr[0]:" + lineArr[0]);

                                    #region NetworkMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Client Address":
                                            rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                            break;
                                        case "Source Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Workstation Name":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        //case "ffff":
                                        //    rec.CustomStr3 = appendArrayElements(lineArr);
                                        //    break;

                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (authenMode)
                                {
                                    #region AuthenMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Authentication Package":
                                            string authenPack = appendArrayElements(lineArr);
                                            if (authenPack.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Pre-Authentication Type":
                                            string authenPack3 = appendArrayElements(lineArr);
                                            if (authenPack3.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack3.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack3.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Logon Process":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Account":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (newAccountMode)
                                {
                                    #region NewAccountMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Account Name":
                                            if (rec.CustomStr1 != null)
                                            {
                                                rec.CustomStr2 = rec.CustomStr1;
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            else
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Other

                                    switch (lineArr[0].Trim())
                                    {
                                        case "Logon Type":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt3 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt3 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt3 = 0;
                                            }
                                            break;
                                        case "Error Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Status Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Failure Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Caller Workstation":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Workstation Name":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Workstation":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "User Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Name":
                                            if (rec.CustomStr1 != null)
                                            {
                                                rec.CustomStr2 = rec.CustomStr1;
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            else
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Account":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Caller User Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Caller Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Target Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Target User Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Address":
                                            rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                            //rec.CustomStr3 = appendArrayElements(lineArr);dali
                                            break;
                                        case "Source Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Authentication Package":
                                            string authenPack = appendArrayElements(lineArr);
                                            if (authenPack.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack.Contains("Kerberos") || authenPack.Contains("KDS"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Pre-Authentication Type":
                                            string authenPack2 = appendArrayElements(lineArr);
                                            if (authenPack2.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack2.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack2.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Caller Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "PID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Logon Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Process":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Duration":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Object Name":
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;
                                        case "Object Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Operation Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Handle ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt7 = 0;
                                            }
                                            break;
                                        case "Primary User Name":
                                            if (rec.CustomStr1 == null)
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client User Name":
                                            if (rec.CustomStr2 == null)
                                            {
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        //case "ffff":
                                        //    rec.CustomStr3 = appendArrayElements(lineArr);
                                        //    break;


                                        //D.Ali Türkce Gelen Loglar İçin
                                        case "Kullanıcı Adı":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "İş İstasyonu Adı":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma işlemi":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma Türü":
                                            if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                            else
                                                rec.CustomInt5 = -1;
                                            break;
                                        case "Etki Alanı":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak Ağ Adresi":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Hesabı":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak İş İstasyonu":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Share Name":
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;

                                        // 23.07.2012 Tigem için Onur sarıkaya Kapattı.
    
                                        //case "Hesap Adı":
                                        //    if (string.IsNullOrEmpty(rec.CustomStr1))
                                        //        rec.CustomStr1 = appendArrayElements(lineArr);
                                        //    else
                                        //        rec.CustomStr2 = appendArrayElements(lineArr);
                                        //    break;
                                        /////////

                                        case "Hesap Adı":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Paylaşım Adı":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak Adresi":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Hesap Etki Alanı":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma Kimliği":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Türü":
                                            if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                            else
                                                rec.CustomInt5 = -1;
                                            break;

                                        case "İşlem Kimliği":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "İşlem Adı":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak Bağlantı Noktası":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Kimlik Doğrulama Paketi":
                                            string authenPack4 = appendArrayElements(lineArr);
                                            if (authenPack4.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack4.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack4.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Paket Adı (yalnızca NTLM)":
                                            string authenPack3 = appendArrayElements(lineArr);
                                            if (authenPack3.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack3.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack3.Contains("Kerberos") || authenPack3.Contains("KDS"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;



                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }

                    //if (rec.Description.Length > 900)
                    //{
                    //    if (rec.Description.Length > 1800)
                    //    {
                    //        rec.CustomStr10 = rec.Description.Substring(900, 900);
                    //    }
                    //    else
                    //    {
                    //        rec.CustomStr10 = rec.Description.Substring(900, rec.Description.Length - 900 - 2);
                    //    }

                    //    rec.Description = rec.Description.Substring(0, 900);
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
                    //}
                }

                #endregion

                if ((mo["TimeGenerated"] != null))
                {
                    string dat = mo["TimeGenerated"].ToString().Split('.')[0];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated1:" + dat);
                    rec.Datetime = dat.Substring(0, 4) + "/" + dat.Substring(4, 2) + "/" + dat.Substring(6, 2) + " " + dat.Substring(8, 2) + ":" + dat.Substring(10, 2) + ":" + dat.Substring(12, 2); //+ "." + dat.Substring(14, 2);
                    rec.Datetime = Convert.ToDateTime(rec.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated:" + rec.Datetime);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "rec.customstr3:" + rec.CustomStr3);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                sendData(rec);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        private void sendData(Rec rec)
        {
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
        }

        private string appendArrayElements(string[] arr)
        {
            string totalString = "";
            for (int i = 1; i < arr.Length; i++)
            {
                totalString += ":" + arr[i].Trim();
            }
            //return totalString.TrimStart(":".ToCharArray()).TrimEnd(":".ToCharArray());
            return totalString.Trim(':').Trim('f').Trim(':').Trim('f');
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
                EventLog.WriteEntry("Security Manager NtEventlog 2008 Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, int Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            last_position = LastPosition;
            if (FromEndOnLoss)
                fromend = 1;
            else
                fromend = 0;
            max_line_towait = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
            EventIDToFilter = CustomVar1;
        }

    }
}
