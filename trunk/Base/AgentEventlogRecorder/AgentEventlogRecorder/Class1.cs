//==============================================
// Eventlog recorder
// Copyright 2009 NATEK
//==============================================

//Agent Recorder
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EventlogRecorder
{
    // Event Log recorder developed with EventLog .NET class
    public class EventlogRecorder : CustomBase
    {
        #region "Property Initializations"
        private int numOfTimers, trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position, remote_host = "", location, msg_lib = "msaudite.dll";//, user, password;
        private bool reg_flag = false, start_state = true, newSetupStatus = true;
        private CLogger L;
        private string[] globalLastPositionArr = { "0-0", "0-0", "0-0", "0-0", "0-0", "0-0", "0-0", "0-0",
                                                   "0-0", "0-0"};

        private object syncRoot = new object();

        [DllImport("rdevnt.dll")]
        public static extern string rdevnt(int msgnumber, string library);
        [DllImport("rdevnt.dll")]
        public static extern uint FetchUserName(String strtext, System.Text.StringBuilder DomainName, System.Text.StringBuilder UserName);

        private ExtendedTimer[] timerArr = { null, null, null, null, null, null, null, null, null, null };
        #endregion

        #region "API Function Declarations"

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

        #region "Logon-Related Enum Declarations"

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

        #endregion

        #region Translate SecurityId to object name
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupAccountSid([In, MarshalAs(UnmanagedType.LPTStr)] string systemName, IntPtr sid, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder name, ref int cbName, StringBuilder referencedDomainName, ref int cbReferencedDomainName, out int use);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountName([In, MarshalAs(UnmanagedType.LPTStr)] string systemName, [In, MarshalAs(UnmanagedType.LPTStr)] string accountName, IntPtr sid, ref int cbSid, StringBuilder referencedDomainName, ref int cbReferencedDomainName, out int use);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertSidToStringSid(IntPtr sid, [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertStringSidToSid([In, MarshalAs(UnmanagedType.LPTStr)] string pStringSid, ref IntPtr sid);
        #endregion

        //Initialize Component
        public EventlogRecorder()
        {
            InitializeComponent();
        }

        bool InitSystem()
        {
            lock (syncRoot)
            {
                if (!reg_flag)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return false;
                    }
                    if (!Initialize_Logger())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              "k on Intialize Logger on EventlogRecorder Recorder functions may not be running");
                        return false;
                    }
                    reg_flag = true;
                }
                return true;
            }
        }
        //Initialize timer and logger.
        public override void Init()
        {
            if (!InitSystem())
                return;

            try
            {
                string[] locationArray = location.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                numOfTimers = locationArray.Length;
                L.Log(LogType.FILE, LogLevel.DEBUG, "numOfTimers is " + numOfTimers);

                //ExtendedTimer timer1;

                for (int i = 0; i < numOfTimers; i++)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "locationArray[i] is " + locationArray[i]);
                    if (timerArr[i] == null)
                    {
                        timerArr[i] = new ExtendedTimer(i, locationArray[i]);
                        timerArr[i].AutoReset = false;
                        timerArr[i].Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
                        timerArr[i].Interval = timer_interval;
                        timerArr[i].Enabled = true;
                        System.Threading.Thread.Sleep(timer_interval / numOfTimers);
                    }
                    //globalLastPositionArr[i] = "0-0";
                }
            }
            catch (Exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error on initializing timers");
                return;
            }
        }

        //Unimplemented function
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// The method converts SID string (user, group) into object name.
        /// Bu fonksiyon 06.08.2012 tarihinde Hazine Müsteþarlýðýnda bulunan sistemdeki 
        /// SID(Security Id)'leri anlamlý olan name'e translate etmek amacý ile Onur Sarýkaya tarafýndan eklenmiþtir. 
        /// </summary>
        /// <param name="name">SID string.</param>
        /// <returns>Object name in form domain\object_name.</returns>
        public static String GetName(CLogger L, string sid)
        {
            try
            {
                return new System.Security.Principal.SecurityIdentifier(sid).Translate(
                        typeof(System.Security.Principal.NTAccount)).ToString();
            }
            catch (Exception exception)
            {
                if (L != null)
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error on GetName: " + exception.Message);
                //WriteMessage("GetName Error: " + exception.Message);
                return sid;
            }
        } //GetName

        public static readonly Regex RegSid = new Regex("%{(S-[^}-]+-[^}-]+-[^}]*)}|(:[ \t]*)(S-[^:]*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //Get installation options and data from the registry
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Trace Level"));
                // remote_host = rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\EventlogRecorder.log";
                timer_interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("MaxLineToWait"));

                newSetupStatus = Convert.ToBoolean(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("NewSetupStatus"));
                if (newSetupStatus)
                {
                    Set_Registry_NewSetupStatus("false");
                    newSetupStatus = false;
                    fromend = 1;
                }
                else
                {
                    fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("FromEndOnLoss"));
                }

                last_position = rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("LastPosition").ToString();
                //Split last_position and copy to globalLastPositionArr:
                last_position.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).CopyTo(globalLastPositionArr, 0);
                location = rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Location").ToString();


                /*
                user = rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("EventlogRecorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);
                */

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager EventlogRecorder Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                timer1_Tick_Original(sender, e);
            }
            finally
            {
                ((ExtendedTimer)sender).Enabled = true;
            }
        }
        //Read logs through Eventlog class at each timer tick
        private void timer1_Tick_Original(object sender, System.Timers.ElapsedEventArgs e)
        {
            string specificLocation;
            int specificIndex = -1;
            string specificLastPosition = "";
            EventLog log = null;
            int arrIndex = 0;
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            try
            {
                specificLocation = ((ExtendedTimer)sender).TimerLocation;
                L.Log(LogType.FILE, LogLevel.DEBUG, "specificLocation is " + specificLocation);

                specificIndex = ((ExtendedTimer)sender).TimerIndex;
                L.Log(LogType.FILE, LogLevel.DEBUG, "specificIndex is " + specificIndex);

                if (!InitSystem())
                    return;

                lock (syncRoot)
                {
                    if (start_state & fromend == 1)
                    {
                        if (!Set_LastPosition())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on setting the last position.");
                        }
                        start_state = false;
                    }

                    if (remote_host == "")
                        remote_host = "localhost";
                }

                specificLastPosition = globalLastPositionArr[specificIndex]; // RN1-INDEX1

                string[] specificLastPositionArr = specificLastPosition.Split("-".ToCharArray(),
                                                                StringSplitOptions.RemoveEmptyEntries);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Last position at the beginning of timer_tick is " + specificLastPosition);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last position is: " + specificLastPositionArr[0]);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last index is: " + specificLastPositionArr[1]);

                L.Log(LogType.FILE, LogLevel.INFORM, "Start collecting event logs of " + specificLocation);

                try
                {
                    log = new EventLog(specificLocation);
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error collecting event logs of " + specificLocation + ".\n" + ex.Message);
                    ((ExtendedTimer)sender).Enabled = true;
                    return;
                }

                int totalEventLogCount = log.Entries.Count;
                if (totalEventLogCount == 0)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "No logs generated or logs are cleared");
                    specificLastPosition = "0-0";
                    globalLastPositionArr[specificIndex] = specificLastPosition;
                    lock (syncRoot)
                    {
                        last_position = String.Join(",", globalLastPositionArr);
                        Set_Registry(last_position);
                    }
                    return;
                }

                int last_RecordID = Convert.ToInt32(specificLastPositionArr[0]);
                int last_Index = Convert.ToInt32(specificLastPositionArr[1]);

                int firstActualRecordNum = log.Entries[0].Index;

                if (last_RecordID == 0 & last_Index == 0)
                { //First time
                    arrIndex = 0;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "First install");
                }
                else
                {
                    try
                    {
                        int i;
                        int retentionIndex = 0;
                        if (totalEventLogCount == last_Index + 1)
                        { //End of logs
                            if (log.Entries[last_Index].Index == last_RecordID)
                            { //No new log
                                L.Log(LogType.FILE, LogLevel.INFORM, "End of log file. No new entry. Exit!");
                                ((ExtendedTimer)sender).Enabled = true;
                                return;
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.INFORM, "End of log file. Retention!");
                                retentionIndex = totalEventLogCount - 1;
                            }
                        }
                        else if (totalEventLogCount > last_Index)
                        { //Not end of logs yet
                            L.Log(LogType.FILE, LogLevel.INFORM, "Not end of logs yet. No retention!");
                            retentionIndex = last_Index;
                        }
                        else
                        { //Retention
                            L.Log(LogType.FILE, LogLevel.INFORM, "End of log file. Retention 2!");
                            retentionIndex = totalEventLogCount - 1;
                        }
                        for (i = retentionIndex; i >= 0; i--)
                        {
                            if (log.Entries[i].Index == last_RecordID)
                            { //Retention but no logs lost
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Last record ID caught: " + i);
                                arrIndex = i + 1;
                                break;
                            }
                        }
                        if (i < 0)
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, "At least " + (last_Index + 1) + " " + specificLocation + " logs overwritten or logs cleared! Starting from the first index.");
                            arrIndex = 0;
                        }
                    }
                    catch (Exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Unknown error. Starting from the first index.");
                        specificLastPosition = "0-0";
                        globalLastPositionArr[specificIndex] = specificLastPosition;
                        lock (syncRoot)
                        {
                            last_position = String.Join(",", globalLastPositionArr);
                            Set_Registry(last_position);
                            return;
                        }
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "ArrIndex is " + arrIndex);
                for (int recordCount = 0; recordCount < max_line_towait; arrIndex++)
                {
                    if (arrIndex == totalEventLogCount)
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM, "End of log file. Waiting for new logs");
                        //arrIndex--;
                        break;
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Continue sending! ArrIndex " + arrIndex);
                    if (!Send_Record(log.Entries[arrIndex], specificLocation))
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on sending record with record number " + log.Entries[arrIndex].Index.ToString());
                    }
                    recordCount++;
                }

                //Join most up-to-date last positions and set registry accordingly
                specificLastPosition = log.Entries[arrIndex - 1].Index.ToString() + "-" + (arrIndex - 1);
                globalLastPositionArr[specificIndex] = specificLastPosition;
                lock (syncRoot)
                {
                    last_position = String.Join(",", globalLastPositionArr);
                    Set_Registry(last_position);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last position at the end of timer_tick is " + specificLastPosition);
            }

            catch (Exception er)
            {
                if (specificIndex >= 0)
                {
                    specificLastPosition = log.Entries[arrIndex - 1].Index.ToString() + "-" + (arrIndex - 1);
                    globalLastPositionArr[specificIndex] = specificLastPosition;
                    lock (syncRoot)
                    {
                        last_position = String.Join(",", globalLastPositionArr);
                        Set_Registry(last_position);
                    }
                    L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                }
            }
        }

        //Write current status to registry
        public bool Set_Registry(string status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("EventlogRecorder");
                rk.SetValue("LastPosition", status);
                rk.Close();

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager EventlogRecorder Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        private bool Set_Registry_NewSetupStatus(string setupStatus)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("EventlogRecorder");
                rk.SetValue("NewSetupStatus", setupStatus);
                rk.Close();

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager EventlogRecorder Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        //Format Message
        private string FormatMessage(int msgnumber)
        {
            try
            {
                string msg = rdevnt(msgnumber, msg_lib);
                return msg;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return null;
            }
        }


        public bool Set_LastPosition()
        {
            lock (syncRoot)
            {
                return Set_LastPosition_Original();
            }
        }

        //Set last position
        bool Set_LastPosition_Original()
        {
            try
            {
                if (remote_host == "")
                    remote_host = "localhost";

                string[] locationArray = location.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                last_position.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).CopyTo(globalLastPositionArr, 0);

                for (int i = 0; i < numOfTimers; i++)
                {
                    EventLog ev = new EventLog(locationArray[i], remote_host);

                    if (fromend == 1)
                    {
                        globalLastPositionArr[i] = ev.Entries[ev.Entries.Count - 1].Index.ToString() + "-" + (ev.Entries.Count - 1);
                    }
                    else
                    {
                        globalLastPositionArr[i] = ev.Entries[0].Index.ToString() + "-0";
                    }
                }
                last_position = String.Join(",", globalLastPositionArr);

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        void PrintLog(string msg)
        {
            if (L != null)
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, ">>>>>>>>>>>>>>>>>>>>" + msg);
                }
                catch
                {
                }
            }
        }

        //Send data to sender/remote recorder service
        public bool Send_Record(EventLogEntry entry, string specificLocation)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                rec.LogName = "NT-" + specificLocation;
                rec.ComputerName = entry.MachineName;
                L.Log(LogType.FILE, LogLevel.DEBUG, "Computername:" + rec.ComputerName);

                rec.EventCategory = entry.Category;
                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory:" + rec.EventCategory.ToString());

                rec.EventId = entry.InstanceId;
                L.Log(LogType.FILE, LogLevel.DEBUG, "EventIdentifier:" + rec.EventId.ToString());

                //WriteMessage(entry.InstanceId.ToString());
                //WriteMessage(entry.Message);

                rec.EventType = Convert.ToString(entry.EntryType);
                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType:" + rec.EventType);

                rec.Recordnum = entry.Index;
                L.Log(LogType.FILE, LogLevel.DEBUG, "RecordNumber:" + rec.Recordnum.ToString());

                rec.SourceName = entry.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName:" + rec.SourceName);

                rec.UserName = entry.Source;
                L.Log(LogType.FILE, LogLevel.DEBUG, "User:" + rec.SourceName);

                if (entry.Message.ToString() != null)
                {
                    rec.Description = ReplaceSids(entry.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Message:" + rec.Description);
                    if (specificLocation == "Security")
                    {
                        //string msg = FormatMessage(entry.Index);
                        //if (msg == null)
                        //    L.Log(LogType.FILE, LogLevel.ERROR, "Cannot Format The Security Message");
                        //else
                        //    rec.Description = msg;

                        if (Environment.OSVersion.Version.Major < 6)
                        {
                            //Ýþletim sistemi Windows 6 'dan Kücükse

                            #region OSVersion.Version.Major < 6
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

                            for (int i = 0; i < DescArr.Length; i++)
                            {
                                if (!DescArr[i].Contains(":"))
                                {

                                    if (accessMode)
                                    {
                                        rec.CustomStr7 += " " + DescArr[i].Trim();
                                    }

                                }
                                else
                                {
                                    string[] lineArr = DescArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "DescArr[" + i + "]:" + DescArr[i]);


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
                                        }
                                        else if (lineArr[0].Trim() == "Subject" || lineArr[0].Trim() == "New Logon" || lineArr[0].Trim() == "Account Whose Credentials Were Used" || lineArr[0].Trim() == "Credentials Which Were Replayed" || lineArr[0].Trim() == "Account That Was Locked Out" || lineArr[0].Trim() == "New Computer Account" || lineArr[0].Trim() == "Computer Account That Was Changed" || lineArr[0].Trim() == "Source Account")
                                        {
                                            subjectMode = true;
                                            objectMode = false;
                                            targetMode = false;
                                            accessMode = false;
                                            processMode = false;
                                            applMode = false;
                                            networkMode = false;
                                            authenMode = false;
                                        }
                                        else if (lineArr[0].Trim() == "Target" || lineArr[0].Trim() == "Target Account" || lineArr[0].Trim() == "Target Computer" || lineArr[0].Trim() == "Target Server")
                                        {
                                            subjectMode = true;
                                            objectMode = false;
                                            targetMode = false;
                                            accessMode = false;
                                            processMode = false;
                                            applMode = false;
                                            networkMode = false;
                                            authenMode = false;
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
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        if (subjectMode)
                                        {
                                            #region SubjectMode
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
                                                case "Security ID":
                                                    if (rec.CustomStr2 == null)
                                                    {
                                                        rec.CustomStr2 = appendArrayElements(lineArr);
                                                    }
                                                    break;
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
                                            #region TargetMode
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
                                            #region ObjectMode
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
                                            #region AccessMode
                                            switch (lineArr[0].Trim())
                                            {
                                                case "Accesses":
                                                    if (rec.CustomStr7 == null)
                                                    {
                                                        rec.CustomStr7 = appendArrayElements(lineArr);
                                                        dummyAccessControl = true;
                                                    }
                                                    break;
                                                case "Access Mask":
                                                    if (dummyAccessControl)
                                                    {
                                                        rec.CustomStr7 += " " + appendArrayElements(lineArr);
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
                                            #region ProcessMode
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
                                            #region ApplMode
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
                                            #region NetWorkMode
                                            switch (lineArr[0].Trim())
                                            {
                                                case "Client Address":
                                                    rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                                    //rec.CustomStr3 = appendArrayElements(lineArr);
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
                                                default:
                                                    break;
                                            }
                                            #endregion
                                        }
                                        else if (authenMode)
                                        {
                                            #region AuthenMode
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
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Client Name":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Logon account":
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
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
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
                                                    //rec.CustomStr3 = lineArr[lineArr.Length - 1];
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

                                                //D.Ali Türkce Gelen Loglar Ýçin
                                                case "Kullanýcý Adý":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Ýþ Ýstasyonu Adý":
                                                    rec.CustomStr4 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma iþlemi":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma Türü":
                                                    if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                        rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                    else
                                                        rec.CustomInt5 = -1;
                                                    break;
                                                case "Etki Alaný":
                                                    rec.CustomStr5 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Að Adresi":
                                                    rec.CustomStr3 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Hesabý":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Ýþ Ýstasyonu":
                                                    rec.CustomStr4 = appendArrayElements(lineArr);
                                                    break;
                                                case "Share Name":
                                                    rec.CustomStr8 = appendArrayElements(lineArr);
                                                    break;
                                                case "Hesap Adý":
                                                    if (string.IsNullOrEmpty(rec.CustomStr1))
                                                        rec.CustomStr1 = appendArrayElements(lineArr);
                                                    else
                                                        rec.CustomStr2 = appendArrayElements(lineArr);
                                                    break;
                                                /////////////77

                                                case "Güvenlik Kimliði":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Hesap Etki Alaný":
                                                    rec.CustomStr5 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma Kimliði":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Türü":
                                                    if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                        rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                    else
                                                        rec.CustomInt5 = -1;
                                                    break;

                                                case "Ýþlem Kimliði":
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
                                                case "Ýþlem Adý":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Baðlantý Noktasý":
                                                    try
                                                    {
                                                        rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                    catch (Exception)
                                                    {
                                                        rec.CustomInt4 = 0;
                                                    }
                                                    break;
                                                case "Kimlik Doðrulama Paketi":
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
                                                case "Paket Adý (yalnýzca NTLM)":
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
                            if (rec.Description.Length > 900)
                            {
                                if (rec.Description.Length > 1800)
                                {
                                    rec.CustomStr10 = rec.Description.Substring(900, 900);
                                }
                                else
                                {
                                    rec.CustomStr10 = rec.Description.Substring(900, rec.Description.Length - 900);
                                }
                                rec.Description = rec.Description.Substring(0, 900);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
                            }
                            #endregion
                        }
                        else
                        {
                            #region Ýþletim Sistemi Windows 6 ve üstü
                            string[] DescArr = rec.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            //WriteMessage("Ýþletim Sistemi Windows 6 ve üstü");

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

                                    if (DescArr[i].Trim().Contains("Member"))
                                    {
                                        rec.CustomStr4 = DescArr[i + 1].Split(':')[1].Trim();
                                    }

                                    if (lineArr[lineArr.Length - 1].Trim() == "")
                                    {
                                        #region Mode

                                        //WriteMessage("Description text splitted to CustomStr10" + lineArr[0].Trim());
                                        rec.CustomStr3 = lineArr[0].Trim();
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
                                        else if (lineArr[0].Trim() == "Subject" || lineArr[0].Trim() == "New Logon" || lineArr[0].Trim() == "Account Whose Credentials Were Used" || lineArr[0].Trim() == "Credentials Which Were Replayed" || lineArr[0].Trim() == "Account That Was Locked Out" || lineArr[0].Trim() == "New Computer Account" || lineArr[0].Trim() == "Computer Account That Was Changed" || lineArr[0].Trim() == "Source Account")
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
                                        else if (lineArr[0].Trim() == "Target" || lineArr[0].Trim() == "Target Account" || lineArr[0].Trim() == "Target Computer" || lineArr[0].Trim() == "Target Server")
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
                                        //else if ()
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
                                            #region SubjectMode
                                            switch (lineArr[0].Trim())
                                            {
                                                case "User Name":
                                                    //rec.CustomStr1 = appendArrayElements(lineArr);
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1:" + rec.CustomStr1);
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
                                                case "Security ID":
                                                    if (rec.CustomStr2 == null)
                                                    {
                                                        //rec.CustomStr2 = appendArrayElements(lineArr);
                                                        rec.CustomStr2 = appendArrayElements(lineArr);
                                                    }
                                                    break;
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
                                            #region TargetMode
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
                                            #region ObjectMode
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
                                            #region AccessMode
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
                                            #region ProcessMode
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
                                            #region ApplMode
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
                                            #region NetWorkMode
                                            switch (lineArr[0].Trim())
                                            {
                                                case "Client Address":
                                                    //rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                                    rec.CustomStr3 = appendArrayElements(lineArr);
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
                                            #region AuthenMode
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
                                            #region NewAccountMode
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

                                            //WriteMessage("lineArr[0]: " + lineArr[0]);
                                            //WriteMessage("lineArr[1]: " + lineArr[1]);
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
                                                case "Logon account":
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
                                                    //rec.CustomStr3 = lineArr[lineArr.Length - 1];
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


                                                //D.Ali Türkce Gelen Loglar Ýçin
                                                case "Kullanýcý Adý":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Ýþ Ýstasyonu Adý":
                                                    rec.CustomStr4 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma iþlemi":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma Türü":
                                                    if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                        rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                    else
                                                        rec.CustomInt5 = -1;
                                                    break;
                                                case "Etki Alaný":
                                                    rec.CustomStr5 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Að Adresi":
                                                    rec.CustomStr3 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Hesabý":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Ýþ Ýstasyonu":
                                                    rec.CustomStr4 = appendArrayElements(lineArr);
                                                    break;
                                                case "Share Name":
                                                    rec.CustomStr8 = appendArrayElements(lineArr);
                                                    break;
                                                case "Hesap Adý":
                                                    if (string.IsNullOrEmpty(rec.CustomStr1))
                                                        rec.CustomStr1 = appendArrayElements(lineArr);
                                                    else
                                                        rec.CustomStr2 = appendArrayElements(lineArr);
                                                    break;
                                                case "Güvenlik Kimliði":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Hesap Etki Alaný":
                                                    rec.CustomStr5 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Açma Kimliði":
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                    break;
                                                case "Oturum Türü":
                                                    if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                        rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                    else
                                                        rec.CustomInt5 = -1;
                                                    break;

                                                case "Ýþlem Kimliði":
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
                                                case "Ýþlem Adý":
                                                    rec.CustomStr6 = appendArrayElements(lineArr);
                                                    break;
                                                case "Kaynak Baðlantý Noktasý":
                                                    try
                                                    {
                                                        rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                    catch (Exception)
                                                    {
                                                        rec.CustomInt4 = 0;
                                                    }
                                                    break;
                                                case "Kimlik Doðrulama Paketi":
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
                                                case "Paket Adý (yalnýzca NTLM)":
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

                                                case "Member":
                                                    rec.CustomStr4 = "Member";
                                                    break;



                                                default:
                                                    break;
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }
                            if (rec.Description.Length > 899)
                            {
                                if (rec.Description.Length > 1799)
                                {
                                    rec.CustomStr10 = rec.Description.Substring(899, 899);
                                }
                                else
                                {
                                    rec.CustomStr10 = rec.Description.Substring(899, rec.Description.Length - 899);
                                }
                                rec.Description = rec.Description.Substring(0, 899);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
                            }
                            #endregion

                            rec.CustomStr7 = DescArr[0];
                            //WriteMessage("ONURRRRRRRRRRRRRRRRRRR: ***  "+rec.CustomStr7);
                        }
                    }

                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Message:" + rec.Description);

                string[] newArray = rec.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < newArray.Length; i++)
                {
                    if (newArray[i].Contains("Logon Type") || newArray[i].Contains("Oturum Türü"))
                    {
                        rec.CustomInt3 = Convert.ToInt32(newArray[i].Split(':')[1].Trim());
                    }

                    #region Onur Sarýkaya Ekledi.
                    //19.04.2012
                    /*
                    for (int j = 0; j < newArray.Length; j++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Onur. Array Elements:" + newArray[i]);
                    }
                    */
                    try
                    {
                        if (newArray[i].Contains("Security ID"))
                        {
                            rec.CustomStr1 = newArray[i].Split(':')[1].Trim();
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + rec.CustomStr1);

                    }

                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Security id to user name translate error. ");
                    }

                    if (string.IsNullOrEmpty(rec.CustomStr1) || rec.CustomStr1.Contains("S-"))
                    {
                        if (newArray[i].Contains("Account Name") || newArray[i].Contains("Hesap Adý"))
                        {
                            rec.CustomStr1 = newArray[i].Split(':')[1].Trim();
                        }
                    }

                    if (newArray[i].Contains("Account Domain") || newArray[i].Contains("Hesap Etki Alaný"))
                    {
                        rec.CustomStr5 = newArray[i].Split(':')[1].Trim();
                    }

                    if (newArray[i].Contains("Source Network Address") || newArray[i].Contains("Kaynak Að Adresi"))
                    {
                        rec.CustomStr3 = newArray[i].Split(':')[1].Trim();
                    }
                    #endregion
                }

                L.Log(LogType.FILE, LogLevel.DEBUG,
                                             "Test : Last Values : " + " CustomInt3 : " + rec.CustomInt3 + " *** " + " CustomStr1 : " +
                                             rec.CustomStr1 + " *** " + " CustomStr5 : " + rec.CustomStr5 + " *** " + " CustomStr3 : " +
                                             rec.CustomStr3 + " *** " + " EventId : " + rec.EventId);

                rec.Datetime = entry.TimeGenerated.ToString("yyyy/MM/dd HH:mm:ss.fff");
                L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated:" + rec.Datetime);




                // Start sending data
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                //WriteMessage("Start sending Data.");
                lock (syncRoot)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());

                return false;
            }
        }

        private string ReplaceSids(string str)
        {
            if (str == null)
                return null;
            return RegSid.Replace(str, (m) =>
                m.Groups[1].Success ? GetName(L, m.Groups[1].Value) : m.Groups[2].Value + GetName(L, m.Groups[3].Value.TrimEnd()));
        }
        //

        /*public void WriteMessage(string Message)
        {
            //if (fromend == 1)
            {
                string path = @"C:\tmp\AgentTest.txt";
                try
                {
                    //File.AppendAllText(path, "***********************************************");
                    //File.AppendAllText(path, "\r\n");
                    File.AppendAllText(path, DateTime.Now + (" - " + Message));
                    //File.AppendAllText(path, "\r\n");
                    //File.AppendAllText(path, "***********************************************");
                    //File.AppendAllText(path, "\r\n");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "WriteMesssage: " + exception.Message);
                    throw;
                }
            }
        }*/

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
        //Set properties of CLogger object
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
                EventLog.WriteEntry("Security Manager EventlogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}