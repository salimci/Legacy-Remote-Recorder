using System;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;

namespace CiscoSecureACSRecorder
{
    /// <summary>
    /// Cisco Secure ACS Recorder developed for CSV logs
    /// </summary>
    public class CiscoSecureACSRecorder : CustomBase
    {
        #region "Property Initializations"

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0,zone=0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position = "", last_line = "", remote_host = "", location, user, password,last_recdate="";
        private bool reg_flag = false, start_state = true;
        protected bool usingRegistry = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string acsLogType;

        //Validation
        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

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

        //Initialize Component
        public CiscoSecureACSRecorder()
        {
            InitializeComponent();
        }

        //Initialize timer and logger.
        //Remote Recorder: Write log directory to registry
        //Agent: Get installation options and data from the registry
        public override void Init()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
            timer1.Enabled = true;

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSecureACS Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSecureACS Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }
        //Unimplemented function
        private void InitializeComponent()
        {
        }

        //Get installation options and data from the registry
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Trace Level"));
                remote_host = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoSecureACSRecorder" + remote_host + ".log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("MaxLineToWait"));
                fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("FromEndOnLoss"));
                last_position = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("LastPosition").ToString();
                last_line = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("LastLine").ToString();
                location = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Location").ToString();
                user = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("CiscoSecureACSRecorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoSecureACS Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        //Open relevant registry key and write log directory to it
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoSecureACSRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoSecureACS Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        //Read logs through Eventlog class at each timer tick
        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {

            #region "Timer Tick Initializations"

            timer1.Enabled = false;
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            try
            {
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSecureACS Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;

                    Console.WriteLine("Reagistry read in timer tick");
                }
                else if (!reg_flag & !usingRegistry)
                {
                    if (!Get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log directory");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoSecureACS Recorder functions may not be running");
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

                if (remote_host == "")
                    remote_host = "localhost";

                try
                {
                    ValidateMe();
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Connection to host {0} failed.", remote_host));

                    return;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);

            #endregion

                int initialSkipIndex = 0; // Used to compensate two line read operations below
                string newLocation = "";

                #region "Open file stream"
                FileStream fs;
                StreamReader tr;
                try
                {
                    fs = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Connected to file " + location);
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Connection to file {0} failed.", location));
                    return;
                }
                #endregion

                if (last_position == "")
                {
                    last_position = "0";
                }

                //last_line = "27/04/2009";
                //Set_Registry(last_position, last_line);
                //return;

                #region "Reset lastline to today if necessary"

                if (last_line != "")
                {
                    if (LastlineToDatetime(last_line) > DateTime.Now)
                    { //Reset last_line to today
                        last_line = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Lastline resetted to today");
                    }
                }

                #endregion

                try
                {
                    if (fromend == 0)
                    {
                        #region "fromend = 0"

                        if (last_line == "")
                        { //Do nothing
                            initialSkipIndex = 0;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Initialskipindex = 0");
                        }
                        else
                        {
                            string firstRecord;
                            try
                            {
                                //Skip first column line in the log file.
                                L.Log(LogType.FILE, LogLevel.DEBUG, "First line in location " + location + " is : " + tr.ReadLine());
                                // Get the first meaningful record to retrieve date text
                                firstRecord = tr.ReadLine();
                                L.Log(LogType.FILE, LogLevel.DEBUG, "First record in location " + location + " is : " + firstRecord);

                            }
                            catch (Exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error reading line in {0}.\n(Line 376)", location));
                                return;
                            }

                            if (LastlineToDatetime(last_line) != LastlineToDatetime(firstRecord.Split(',')[0]))
                            {// Day changed. Return back to previous days' log files if not finished.
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Last line " + last_line + " is not equal to first date " + firstRecord.Split(',')[0] + ". Day changed");
                                try
                                {
                                    fs.Close();
                                    tr.Close();
                              
                                    string[] dummyDateArr = last_line.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (dummyDateArr.Length != 3)
                                    {
                                        dummyDateArr = last_line.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    }
                                    if (int.Parse(dummyDateArr[1]) < 10)
                                    {
                                        dummyDateArr[1] = "0" + dummyDateArr[1];
                                    }
                                    if (int.Parse(dummyDateArr[0]) < 10)
                                    {
                                        dummyDateArr[0] = "0" + dummyDateArr[0];
                                    }
                                    newLocation = location.Replace("active.csv", dummyDateArr[2] + "-" + dummyDateArr[1] + "-" + dummyDateArr[0] + ".csv");
                                    //Reopen reader with updated location info
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "New location is " + newLocation);

                                    fs = new FileStream(newLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                                }
                                catch (Exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error connecting to file {0}.\n(Line 410)", newLocation));

                                    try
                                    {
                                        DateTime lastDate = LastlineToDatetime(last_line);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last date is " + lastDate.ToShortDateString());

                                        DateTime newDate = lastDate.AddDays(1);
                                        last_line = newDate.Day + "/" + newDate.Month + "/" + newDate.Year;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last line is " + last_line);

                                        last_position = "0";
                                        Set_Registry(last_position, last_line);
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error parsing last date {0}.\n(Line 427)", last_line));
                                        last_line = "";
                                        last_position = "0";
                                        Set_Registry(last_position, last_line);
                                        return;
                                    }
                                }
                                initialSkipIndex = 0;
                            }
                            else
                            {//Some logs have been read. No day change.
                                initialSkipIndex = 2; // (last_position == "0" ? 0 : 2);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "fromend = 1"

                        if (last_line == "")
                        { //Do nothing
                        }
                        else
                        {
                            //initialSkipIndex = 0;

                            string firstRecord;
                            try
                            {
                                //Skip first column line in the log file.
                                L.Log(LogType.FILE, LogLevel.DEBUG, "First line in location " + location + " is : " + tr.ReadLine());
                                // Get the first meaningful record to retrieve date text
                                firstRecord = tr.ReadLine();
                                L.Log(LogType.FILE, LogLevel.DEBUG, "First record in location " + location + " is : " + firstRecord);

                            }
                            catch (Exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error reading line in {0}.\n(Line 464)", location));
                                return;
                            }

                            if (LastlineToDatetime(last_line) != LastlineToDatetime(firstRecord.Split(',')[0]))
                            {// Day changed. Return to yesterday.
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Last line " + last_line + " is not equal to first date " + firstRecord.Split(',')[0] + ". Day changed");
                                try
                                {
                                    fs.Close();
                                    tr.Close();

                                    string[] dummyDateArr = last_line.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (dummyDateArr.Length != 3)
                                    {
                                        dummyDateArr = last_line.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    }
                                    if (int.Parse(dummyDateArr[1]) < 10)
                                    {
                                        dummyDateArr[1] = "0" + dummyDateArr[1];
                                    }
                                    if (int.Parse(dummyDateArr[0]) < 10)
                                    {
                                        dummyDateArr[0] = "0" + dummyDateArr[0];
                                    }
                                    newLocation = location.Replace("active.csv", dummyDateArr[2] + "-" + dummyDateArr[1] + "-" + dummyDateArr[0] + ".csv");
                                    //Reopen reader with updated location info
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "New location is " + newLocation);

                                    fs = new FileStream(newLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                                }
                                catch (Exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error connecting to file {0}.\n(Line 508)", newLocation));

                                    try
                                    {
                                        DateTime lastDate = LastlineToDatetime(last_line);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last date is " + lastDate.ToShortDateString());

                                        DateTime newDate = lastDate.AddDays(1);
                                        last_line = newDate.Day + "/" + newDate.Month + "/" + newDate.Year;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Last line is " + last_line);

                                        last_position = "0";
                                        Set_Registry(last_position, last_line);
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error parsing last date {0}.\n(Line 525)", last_line));
                                        last_line = "";
                                        last_position = "0";
                                        Set_Registry(last_position, last_line);
                                        return;
                                    }
                                }
                                initialSkipIndex = 0;
                            }
                            else
                            {//Some logs have been read. No day change.
                                initialSkipIndex = 0;
                            }
                        }  
                        try
                        {
                            while (!tr.EndOfStream)
                            {
                                tr.ReadLine();
                                initialSkipIndex += 1;
                            }
                        }
                        catch (Exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error reading all lines in file {0}.\n(Line 550)", fs.Name));
                            return;
                        }
                        last_position = initialSkipIndex.ToString(); //Set last_position to the last line in the log file

                        #endregion
                    }
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error initializing initialSkipIndex for file {0}.\n(Line 560)", fs.Name));
                    return;
                }

                #region "Skip already read lines"
                int lastSkipIndex = int.Parse(last_position) - 1;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Initial skip index is " + initialSkipIndex);
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last skip index is " + lastSkipIndex);

                try
                {
                    //Skip already read lines
                    for (int i = initialSkipIndex; i <= lastSkipIndex; i++)
                    {
                        tr.ReadLine();
                    }
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error skipping in file {0}.\n(Line 580)", fs.Name));
                    return;
                }
                #endregion

                #region "Read lines from log file"

                string currentLine = "";
                for (int j = 0; j < max_line_towait; j++)
                {
                    currentLine = tr.ReadLine();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Current line is " + currentLine);

                    if (currentLine == null) // EOF
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "End of file reached");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "fs.name is " + fs.Name);
                        if (fs.Name == location)
                        {//We are in active file
                            L.Log(LogType.FILE, LogLevel.DEBUG, "We are in active file");
                            return;
                        }
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "We are behind active file");

                            try
                            {
                                DateTime lastDate = LastlineToDatetime(last_line);
                                L.Log(LogType.FILE, LogLevel.DEBUG, String.Format("Last date is now {0}.\n(Line 609)", lastDate.ToShortDateString()));

                                DateTime newDate = lastDate.AddDays(1);
                                last_line = newDate.Day + "/" + newDate.Month + "/" + newDate.Year;

                                L.Log(LogType.FILE, LogLevel.DEBUG, String.Format("Last line is now {0}.\n(Line 614)", last_line));
                                last_position = "0";
                                Set_Registry(last_position, last_line);
                                return;
                            }
                            catch (Exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Error parsing last date {0}.\n(Line 621)", last_line));
                                last_line = "";
                                last_position = "0";
                                Set_Registry(last_position, last_line);
                                return;
                            }
                        }
                    }
                    else
                    {
                        last_position = Convert.ToString(int.Parse(last_position) + 1);
                        Set_Registry(last_position, last_line);
                        Send_Record(currentLine);
                    }
                }
                #endregion

                last_line = currentLine.Split(',')[0]; //Store last date, to track day changes

                Set_Registry(last_position, last_line);

                fs.Close();
                tr.Close();
            }

            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.Message + " \n(Line 648)");
                return;
            }
            finally
            {
                timer1.Enabled = true;
            }
        }

        private DateTime LastlineToDatetime(string last_line)
        {
            string[] lastDateArray = last_line.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (lastDateArray.Length != 3)
            {
                lastDateArray = last_line.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            return DateTime.Parse(lastDateArray[2] + "/" + lastDateArray[1] + "/" + lastDateArray[0]);
        }

        //Write current status to registry or send to remote recorder
        public bool Set_Registry(string status, string lastlineStat)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistry)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("CiscoSecureACSRecorder");
                    rk.SetValue("LastPosition", status);
                    rk.Close();
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("CiscoSecureACSRecorder");
                    rk.SetValue("LastLine", lastlineStat);
                    rk.Close();
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, status, lastlineStat, "","",last_recdate);
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager CiscoSecureACS Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        //Validate credentials to access host
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

                            Console.WriteLine("Impersonation is successful");

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

        //Set last position
        public bool Set_LastPosition()
        {
            try
            {
                ValidateMe();
                if (remote_host == "")
                    remote_host = "localhost";
                //EventLog ev = new EventLog(location, remote_host);
                //if (fromend == 1)
                //    last_position = DateTime.Now.ToString(); // ev.Entries[ev.Entries.Count - 1].Index.ToString();
                //else
                //{
                //    /*
                //    if(start_state) 
                //    {
                //        if (Convert.ToInt32(last_position) < ev.Entries[1].Index)
                //            last_position = ev.Entries[1].Index.ToString();
                //    }
                //    else*/
                //    last_position = "1700/01/01 00:00:00.000"; // ev.Entries[0].Index.ToString();
                //}
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        //Send data to sender/remote recorder service
        public bool Send_Record(string line)
        {
            string[] data = line.Split(',');

            CustomBase.Rec rec = new CustomBase.Rec();
            rec.ComputerName = "";

            string[] dateArray = data[0].Split('/');
            rec.Datetime = dateArray[2] + "/" + dateArray[1] + "/" + dateArray[0] + " " + data[1] + ".000";
            rec.Datetime = Convert.ToDateTime(rec.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
            last_recdate = rec.Datetime;


            rec.LogName = "CiscoSecureACSRecorder";

            try
            {
                switch (acsLogType)
                {
                    case "Failed Attempts":
                        rec.EventCategory = "Authentication";
                        rec.EventType = "Failed";
                        rec.UserName = data[3]; // User name
                        rec.SourceName = data[4]; //Group name
                        rec.CustomInt6 = data[5] != "" ? long.Parse(data[5]) : 0; // Caller-ID
                        rec.CustomStr1 = data[2]; // Message-Type
                        rec.CustomInt7 = data[10] != "" ? long.Parse(data[10]) : 0; // NAS-Port
                        rec.CustomStr2 = data[11]; // NAS-IP-Address
                        rec.CustomStr3 = data[6]; // Network Access Profile Name
                        rec.CustomStr4 = data[7]; // Authen-Failure-Code
                        rec.CustomStr5 = data[8]; // Author-Failure-Code
                        rec.CustomStr6 = data[9]; // Author-Data
                        rec.CustomStr7 = data[12]; // Filter Information
                        rec.CustomStr8 = data[16]; // Reason
                        rec.CustomStr9 = data[15]; // EAP Type Name
                        rec.CustomStr10 = data[13]; // PEAP/EAP-FAST-Clear-Name
                        rec.CustomInt8 = data[14] != "" ? long.Parse(data[14]) : 0; // EAP Type
                        rec.Description = data[17]; // Access Device
                        rec.ComputerName = data[18]; // Network Device Group
                        break;

                    case "Passed Authentications":
                        rec.EventCategory = "Authentication";
                        rec.EventType = "Passed";
                        rec.UserName = data[3]; // User name
                        rec.SourceName = data[4]; //Group name
                        rec.CustomInt6 = data[5] != "" ? long.Parse(data[5]) : 0; // Caller-ID
                        rec.CustomStr1 = data[2]; // Message-Type
                        rec.CustomInt7 = data[6] != "" ? long.Parse(data[6]) : 0; // NAS-Port
                        rec.CustomStr2 = data[7]; // NAS-IP-Address
                        rec.CustomStr3 = data[8]; // Network Access Profile Name
                        rec.CustomStr4 = data[9]; // Shared RAC
                        rec.CustomStr5 = data[10]; // Downloadable ACL
                        rec.CustomStr6 = data[11]; // System-Posture-Token
                        rec.CustomStr7 = data[12]; // Application-Posture-Token
                        rec.CustomStr8 = data[13]; // Reason
                        rec.CustomStr9 = data[15]; // EAP Type Name
                        rec.CustomStr10 = data[16]; // PEAP/EAP-FAST-Clear-Name
                        rec.CustomInt8 = data[14] != "" ? long.Parse(data[14]) : 0; // EAP Type
                        rec.Description = data[17]; // Access Device
                        rec.ComputerName = data[18]; // Network Device Group
                        break;

                    case "RADIUS Accounting":
                        rec.EventCategory = "Accounting";
                        rec.EventType = "RADIUS";
                        rec.UserName = data[2]; // User name
                        rec.SourceName = data[3]; //Group name
                        rec.CustomInt6 = data[4] != "" ? long.Parse(data[4]) : 0; // Calling-Station-Id
                        rec.CustomStr1 = data[8]; // Service-Type
                        rec.CustomInt7 = data[15] != "" ? long.Parse(data[15]) : 0; // NAS-Port
                        rec.CustomStr2 = data[16]; // NAS-IP-Address
                        rec.CustomStr3 = data[14]; // Framed-IP-Address
                        rec.CustomStr4 = data[5]; // Acct-Status-Type
                        rec.CustomStr5 = data[6]; // Acct-Session-Id
                        rec.CustomStr6 = data[7]; // Acct-Session-Time
                        rec.CustomStr7 = data[9]; // Framed-Protocol
                        rec.CustomInt1 = data[10] != "" ? int.Parse(data[10]) : 0; // Acct-Input-Octets
                        rec.CustomInt2 = data[11] != "" ? int.Parse(data[11]) : 0; // Acct-Output-Octets
                        rec.CustomInt3 = data[12] != "" ? int.Parse(data[12]) : 0; // Acct-Input-Packets
                        rec.CustomInt4 = data[13] != "" ? int.Parse(data[13]) : 0; // Acct-Output-Packets
                        rec.ComputerName = data[17]; // cisco-av-pair
                        break;
                    default:
                        break;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
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
                return true;
            }
            catch (Exception er)
            {
                Console.WriteLine("Error sending data. Reason: {0}", er.ToString());

                return false;
            }
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
                EventLog.WriteEntry("Security Manager CiscoSecureACS Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        //Initialize log-read-related data
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            last_position = LastPosition;
            last_line = LastLine;
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
            acsLogType = CustomVar1;
            zone = Zone;
        }

        //Set last position and line of recorder (only for remote recorder)
        public override void SetReg(Int32 Identity, String LastPosition, String LastLine, String LastFile,String LastKeywords,String LastRecDate)
        {
            base.SetReg(Identity, LastPosition, LastLine,"", "",LastRecDate);
        }
    }
}
