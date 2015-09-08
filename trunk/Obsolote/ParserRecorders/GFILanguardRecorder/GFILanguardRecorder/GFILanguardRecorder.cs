using System;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Xml;

namespace GFILanguardRecorder
{

    public class GFILanguardRecorder : CustomBase
    {
        #region "Property Initializations"

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0,CustomVar2=0,zone = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position = "", last_line = "", remote_host = "", location = @"", user = "", password = "", last_recdate = "",CustomVar1 = "";
        private bool reg_flag = false, start_state = true;
        protected bool usingRegistry = false, fromEndOnLoss = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
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

        public GFILanguardRecorder()
        {
            InitializeComponent();
        }

        public bool ParseXML(string Doc, CustomBase.Rec rec)
        {   
            int ii = 0;         
            XmlTextReader reader = new XmlTextReader(Doc);
            string lastRead = "";
            while (reader.Read())
            {
                string mainNode = "";
                mainNode = reader.Name;
                if (reader.Name == "Scan")
                {
                    for (int i = 0; reader.MoveToNextAttribute(); i++)
                    {
                        //Profile == Scan Profile
                        //Created ON == DateTime of the scan
                        if (reader.Name == "Session")
                        {
                            rec.CustomInt10 = Convert.ToInt64(reader.Value);
                        }
                        else if (reader.Name == "Profile")
                        {
                            rec.EventCategory = reader.Value;
                        }
                        else if (reader.Name == "CreatedOn")
                        {
                            rec.Datetime = Convert.ToDateTime(reader.Value).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        }
                    }
                }
                else if (reader.Name == "hostname")
                {
                    //hostname
                    reader.Read();
                    rec.ComputerName = reader.Value;
                    reader.Read();
                }
                else if (reader.Name == "ip")
                {
                    //host ip
                    reader.Read();
                    rec.SourceName = reader.Value;
                    reader.Read();
                }
                else if (reader.Name == "missing_hotfixes")
                {
                    rec.EventType = reader.Name;
                    while (reader.Read())
                    {
                        mainNode = reader.Name;
                        if (reader.Name == "product")
                        {
                            if (ii % 2 == 0)
                            {
                                for (int i = 0; reader.MoveToNextAttribute(); i++)
                                {
                                    //Patch product name                                
                                    if (reader.Name == "name")
                                    {
                                        rec.CustomStr1 = reader.Value;
                                    }
                                }
                            }
                            ii++;
                        }
                        else if (reader.Name == "hotfix")
                        {
                            lastRead = "";
                            while (reader.Read())
                            {
                                lastRead = reader.Name;
                                if (reader.Name == "hotfix")
                                {
                                    Send_Record(rec);
                                    break;
                                }
                                else if (reader.Name == "title")
                                {
                                    reader.Read();
                                    rec.CustomStr2 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "severity")
                                {
                                    reader.Read();
                                    if (reader.Value == "Low")
                                        rec.CustomInt1 = 2;
                                    else if (reader.Value == "Moderate")
                                        rec.CustomInt1 = 3;
                                    else if (reader.Value == "Important")
                                        rec.CustomInt1 = 4;
                                    else if (reader.Value == "Critical")
                                        rec.CustomInt1 = 5;
                                    else
                                        rec.CustomInt1 = 1;
                                    Console.WriteLine(reader.Value);
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "date")
                                {
                                    reader.Read();
                                    rec.CustomStr3 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "filename")
                                {
                                    reader.Read();
                                    rec.CustomStr4 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "filesize")
                                {
                                    reader.Read();
                                    rec.CustomStr5 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "fileurl")
                                {
                                    reader.Read();
                                    rec.CustomStr6 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                                else if (reader.Name == "fileurl")
                                {
                                    reader.Read();
                                    rec.CustomStr6 = reader.Value;
                                    if (reader.Name != lastRead)
                                        reader.Read();
                                }
                            }
                        }
                        else if (reader.Name == "missing_hotfixes")
                        {
                            rec.CustomStr1 = "";
                            rec.CustomStr2 = "";
                            rec.CustomStr3 = "";
                            rec.CustomStr4 = "";
                            rec.CustomStr5 = "";
                            rec.CustomStr6 = "";
                            break;
                        }

                    }
                }
                else if (reader.Name == "severity")
                {
                    mainNode = reader.Name;
                    reader.MoveToNextAttribute();
                    if (reader.Name == "level")
                    {
                        rec.EventType = (mainNode + " " + reader.Name + reader.Value);
                        while (reader.Read())
                        {
                            if (reader.Name == "backdoor")
                            {
                                {
                                    reader.Read();
                                    rec.CustomStr1 = "Backdoor";
                                    rec.CustomStr2 = reader.Value;
                                    reader.Read();
                                    L.Log(LogType.FILE, LogLevel.INFORM, rec.LogName + " " + rec.CustomStr1 + " " + rec.CustomStr2);
                                    Send_Record(rec);
                                }
                            }
                            else if (reader.Name == "Miscellaneous_Alerts" || reader.Name == "Web_Alerts" || reader.Name == "FTP_Alerts" || reader.Name == "Registry_Alerts" || reader.Name == "Services_Alerts")
                            {
                                rec.CustomStr1 = reader.Name;
                                while (reader.Read())
                                {
                                    if (reader.Name == "alert")
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.Name == "name")
                                            {
                                                {
                                                    reader.Read();
                                                    rec.CustomStr2 = reader.Value;
                                                    reader.Read();
                                                }
                                            }
                                            else if (reader.Name == "descr")
                                            {
                                                {
                                                    reader.Read();
                                                    if (reader.Value.Length < 4000)
                                                        rec.Description = reader.Value;
                                                    else
                                                        rec.Description = reader.Value.Substring(0, 3995);
                                                    reader.Read();
                                                }
                                            }
                                            else if (reader.Name == "alert")
                                            {
                                                L.Log(LogType.FILE, LogLevel.INFORM, rec.LogName + " " + rec.CustomStr1 + " " + rec.CustomStr2);                                    
                                                Send_Record(rec);
                                                break;
                                            }
                                        }
                                    }
                                    else if (reader.Name == "Miscellaneous_Alerts" || reader.Name == "Web_Alerts" || reader.Name == "FTP_Alerts" || reader.Name == "Registry_Alerts" || reader.Name == "Services_Alerts")
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (reader.Name == "severity")
                                break;
                        }
                    }
                }
            }            
            return true;
        }

        public override void Init()
        {
            SetConfigData(Id, location, last_line, last_position, last_line, last_position, fromEndOnLoss, max_line_towait, user, password, remote_host, timer_interval, trc_level,CustomVar1,CustomVar2, virtualhost, Dal,zone);
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GFI Languard Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GFI Languard Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
            }
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Trace Level"));
                remote_host = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\GFILanguardRecorder" + remote_host + ".log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("MaxLineToWait"));
                fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("FromEndOnLoss"));
                last_position = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("LastPosition").ToString();
                last_line = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("LastLine").ToString();
                location = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Location").ToString();
                user = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("GFILanguardRecorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GFI Languard Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\GFILanguardRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager GFI Languard Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            Rec rec = new Rec();
            rec.LogName = "GFI Languard Recorder";
            FileStream fs = null;
            StreamReader tr = null;
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GFI Languard Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Registry read in timer tick");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on GFI Languard Recorder functions may not be running");
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
                SetConfigData(Id, location, last_line, last_position, last_line, last_position, fromEndOnLoss, max_line_towait, user, password, remote_host, timer_interval, trc_level, CustomVar1, CustomVar2, virtualhost, Dal, zone);
                DirectoryInfo GFIDirectory = null;
                FileInfo[] scanResults = null;
                string fileToRead = "";
                try
                {
                    GFIDirectory = new DirectoryInfo(location);
                    scanResults = GFIDirectory.GetFiles("*.xml");
                    if (last_line != " " && last_line != null && last_line != "")
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM, "LastFile found.. " + last_line);                    
                        FileInfo fiLastFile = new FileInfo(last_line);                        
                        int ind = 0;
                        foreach (FileInfo fis in scanResults)
                        {
                            if (fis.CreationTimeUtc > fiLastFile.CreationTimeUtc)
                            {
                                ind++;
                            }
                        }
                        FileInfo[] scanResultsUpdate = new FileInfo[ind];
                        ind = 0;
                        foreach (FileInfo fis in scanResults)
                        {
                            if (fis.CreationTimeUtc > fiLastFile.CreationTimeUtc)
                            {
                                scanResultsUpdate[ind] = fis;
                                ind++;
                            }
                        }
                        scanResults = scanResultsUpdate;
                    }
                }
                catch (Exception eee)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Files in the given location could not be listed. Finishing process..");
                    L.Log(LogType.FILE, LogLevel.ERROR, eee.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, last_line);
                    return;
                }

                if (scanResults.Length == 0)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "No files to read. Finishing process..");
                    return;
                }
                else
                {
                    while (scanResults.Length > 0)
                    {
                        int fileToDelete = 0;
                        bool aFileParsed = true;
                        try
                        {
                            DateTime a = DateTime.MaxValue;
                            if (last_line == " " || last_line == null)
                            {
                                for (int i = 0; i < scanResults.Length; i++)
                                {
                                    if (scanResults[i].CreationTimeUtc < a)
                                    {
                                        a = scanResults[i].CreationTime;
                                        fileToRead = scanResults[i].FullName;
                                        last_line = fileToRead;
                                        fileToDelete = i;
                                    }
                                }
                                if (fileToRead == " ")
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "No file found to parse");
                                    aFileParsed = false;
                                }
                                else
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Filename is null so getting new file to read : " + fileToRead);
                            }
                            else
                            {
                                a = DateTime.MaxValue;
                                DateTime b = DateTime.MinValue;
                                foreach (string file in Directory.GetFiles(location))
                                {
                                    if (file == last_line)
                                    {
                                        FileInfo fi = new FileInfo(file);
                                        b = fi.CreationTime;
                                    }
                                }
                                for (int i = 0; i < scanResults.Length; i++)
                                {
                                    if (scanResults[i].CreationTimeUtc > b && scanResults[i].CreationTimeUtc < a)
                                    {
                                        L.Log(LogType.FILE, LogLevel.INFORM, scanResults[i].CreationTimeUtc.ToString());
                                        a = scanResults[i].CreationTimeUtc;
                                        fileToRead = scanResults[i].FullName;
                                        last_line = fileToRead;
                                        fileToDelete = i;
                                    }
                                }
                                if (fileToRead == "")
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "No file found to parse");
                                    aFileParsed = false;
                                }
                                else
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Lastfile found so  getting new file to read : " + fileToRead);
                            }
                        }
                        catch (Exception eee)
                        {
                            aFileParsed = false;
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error getting Xml files");
                            L.Log(LogType.FILE, LogLevel.ERROR, eee.Message);
                            L.Log(LogType.FILE, LogLevel.ERROR, eee.StackTrace);
                        }
                        if (fileToRead != "" && aFileParsed)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Parsing file :  " + fileToRead);
                            ParseXML(fileToRead, rec);
                            Set_Registry("", fileToRead);
                            FileInfo[] backupScanResults = new FileInfo[scanResults.Length-1];
                            bool fileRemoved = false;

                            try
                            {
                                for (int j = 0; j < scanResults.Length; j++)
                                {
                                    if (j != fileToDelete && !fileRemoved)
                                        backupScanResults[j] = scanResults[j];
                                    else if (j != fileToDelete && fileRemoved)
                                        backupScanResults[j - 1] = scanResults[j];
                                    else if (j == fileToDelete)
                                        fileRemoved = true;
                                }
                                
                            }
                            catch(Exception exc)
                            {
                                L.Log(LogType.FILE, LogLevel.INFORM, exc.Message);
                                L.Log(LogType.FILE, LogLevel.INFORM, "fileInfo errors");
                            }
                            scanResults = backupScanResults;
                        }
                        SetReg(Id, "", last_line, last_line, "");
                    }
                }
            }

            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, er.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, er.Source);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    tr.Close();
                }
                timer1.Enabled = true;
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
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }

        public bool Set_Registry(string status, string lastlineStat)
        {
            RegistryKey rk = null;
            try
            {                                        
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetReg(Id, status, lastlineStat, lastlineStat, "", last_recdate);           
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager GFI Languard Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
            try
            {
                ValidateMe();

                if (remote_host == "")
                    remote_host = "localhost";

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        public bool Send_Record(Rec rec)
        {
            last_recdate = rec.Datetime;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");                
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);                    
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error sending data. Reason: " + er.ToString());
                return false;
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
                EventLog.WriteEntry("Security Manager GFI Languard Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, int CustomVar2, String Virtualhost, String dal,int Zone)
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
            zone = Zone;
        }

        public override void SetReg(Int32 Identity, String LastPosition, String LastLine, String LastFile, String LastKeywords, String LastRecDate)
        {
            base.SetReg(Identity, LastPosition, LastLine,LastLine , "", LastRecDate);
        }

    }

}
