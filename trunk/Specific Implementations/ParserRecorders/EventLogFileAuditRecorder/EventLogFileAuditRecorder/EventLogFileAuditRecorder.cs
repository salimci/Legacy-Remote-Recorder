using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using CustomTools;
using DAL;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Config.Relocated;
using Natek.Helpers.Database;
using Natek.Helpers.Diagnostics;
using Natek.Helpers.Security.AccessControl;
using Natek.Helpers.Security.Logon;
using Timer = System.Timers.Timer;

namespace EventLogFileAuditRecorder
{
    public class EventLogFileAuditRecorder : CustomBase
    {
        private static readonly Dictionary<string, string[]> AccessRights = new Dictionary<string, string[]>();
        private static readonly Dictionary<uint, string> AccessMasks = new Dictionary<uint, string>();
        private static readonly Dictionary<string, string> AccessTypes = new Dictionary<string, string>();

        static EventLogFileAuditRecorder()
        {
            if (File.Exists(@"o:\tmp\system.txt"))
                using (var fs = new StreamWriter(@"o:\tmp\system.txt", false)) ;
            InitAccessRights();
            InitAccessMasks();
            InitAccessTypes();
        }

        private static void InitAccessTypes()
        {
            AccessTypes["A"] = "Access Allowed";
            AccessTypes["D"] = "Access Denied";
            AccessTypes["OA"] = "Object Access Allowd";
            AccessTypes["OD"] = "Object Access Denied";
            AccessTypes["AU"] = "Audit";
            AccessTypes["AL"] = "Alarm";
            AccessTypes["OU"] = "Object Audit";
            AccessTypes["OL"] = "Object Alarm";
            AccessTypes["ML"] = "Mandatory Label";
            AccessTypes["XA"] = "Callback Access Allowed";
            AccessTypes["XD"] = "Callback Access Denied";
            AccessTypes["RA"] = "Resource Attribute";
            AccessTypes["SP"] = "Scoped Policy Id";
            AccessTypes["XU"] = "Callback Audit";
            AccessTypes["ZA"] = "Callback Object Access Allowed";
        }

        private static void InitAccessMasks()
        {
            AccessMasks[0x1] = "Read Data (List Directory)";
            AccessMasks[0x2] = "Write Data (Add File)";
            AccessMasks[0x4] = "Append Data (Add Subdirectory)";
            AccessMasks[0x8] = "Read Extended Attributes";
            AccessMasks[0x10] = "Write Extended Attributes";
            AccessMasks[0x20] = "Execute (Traverse)";
            AccessMasks[0x40] = "Delete Child";
            AccessMasks[0x80] = "Read Attributes";
            AccessMasks[0x100] = "Write Attributes";
            AccessMasks[0x10000] = "Delete";
            AccessMasks[0x20000] = "Read Control";
            AccessMasks[0x40000] = "Write DAC";
            AccessMasks[0x80000] = "Write Owner";
            AccessMasks[0x100000] = "Synchronize";
            AccessMasks[0x1000000] = "Access SACL";
            AccessMasks[0x10000000] = "General All";
            AccessMasks[0x20000000] = "General Execute";
            AccessMasks[0x40000000] = "General Write";
            AccessMasks[0x80000000] = "General Read";
        }

        private static void InitAccessRights()
        {
            AccessRights["GA"] = new[] { "GR", "GW", "GX" };
            AccessRights["GR"] = new string[0];
            AccessRights["GW"] = new string[0];
            AccessRights["GX"] = new string[0];
            AccessRights["RC"] = new string[0];
            AccessRights["SD"] = new string[0];
            AccessRights["WD"] = new string[0];
            AccessRights["WO"] = new string[0];
            AccessRights["RP"] = new string[0];
            AccessRights["WP"] = new string[0];
            AccessRights["CC"] = new string[0];
            AccessRights["DC"] = new string[0];
            AccessRights["LC"] = new string[0];
            AccessRights["SW"] = new string[0];
            AccessRights["LO"] = new string[0];
            AccessRights["DT"] = new string[0];
            AccessRights["CR"] = new string[0];
            AccessRights["FA"] = new[] { "FR", "FW", "FX" };
            AccessRights["FR"] = new string[0];
            AccessRights["FW"] = new string[0];
            AccessRights["FX"] = new string[0];
            AccessRights["KA"] = new[] { "KR", "KW", "KX" };
            AccessRights["KR"] = new string[0];
            AccessRights["KW"] = new string[0];
            AccessRights["KX"] = new string[0];
        }

        public string LogLocation { get; set; }
        public event EventHandler RecordSent;

        private System.Timers.Timer timer1;
        private System.Timers.Timer cleanupTimer;
        private int trc_level = 3, timer_interval = 3000;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 10000000;
        private string err_log, location, user, password;
        private string lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy/MM/dd HH:mm:ss";
        private object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        protected bool parsing;
        protected bool isFileFinished;
        protected RegistryKey reg = null;
        private int LangId = 1033;

        private Regex regFiles = new Regex("Archive.*\\.evtx$",
                                           RegexOptions.Compiled | RegexOptions.CultureInvariant |
                                           RegexOptions.IgnoreCase);

        protected string domain, domainUser, ip;
        private System.Timers.Timer triggerTimer;
        private string remoteHost;
        private ImpersonationContext wic;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
                                            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle,
                                                 int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        private Dictionary<string, AuditLogonEnv> _handles = new Dictionary<string, AuditLogonEnv>();
        private Dictionary<string, AuditLogonEnv2> _handles2 = new Dictionary<string, AuditLogonEnv2>();

        public EventLogFileAuditRecorder()
        {
            _handles2["00000"] = new AuditLogonEnv2("00000");
            _handles2["00000"].ProcessLastAudit["-----"] = new AuditHandle2("12313", "-----", "test");
            cleanupTimer = new Timer { AutoReset = false, Interval = 60000 };
            cleanupTimer.Elapsed += new ElapsedEventHandler(cleanupTimer_Elapsed);
            cleanupTimer.Start();
            enc = Encoding.UTF8;
        }

        private void cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var ls = new List<string>();
                lock (_handles2)
                {
                    foreach (var h in _handles2.Values)
                    {
                        lock (h)
                        {
                            foreach (var a in h.ProcessLastAudit.Values)
                            {
                                if (DateTime.Now.Subtract(a.CreatedOn).TotalSeconds > 120)
                                    ls.Add(a.Pid);
                            }
                            foreach (var a in ls)
                            {
                                h.ProcessLastAudit.Remove(a);
                            }
                            ls.Clear();
                        }
                    }
                }
            }
            finally
            {
                cleanupTimer.Enabled = true;
            }
        }

        public override void Init()
        {
            try
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += timer1_Tick;
                timer1.Interval = timer_interval;
                timer1.AutoReset = false;
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on EventLogFileAuditRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on EventLogFileAuditRecorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating EventLogFileAuditRecorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager EventLogFileAuditRecorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  EventLogFileAuditRecorder Init Method end.");
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
         String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
         String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
         String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            fromend = FromEndOnLoss;
            timer_interval = SleepTime; //Timer interval.
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition); //Last position
            Dal = dal;
            lastFile = LastFile;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
            Exception error = null;
            ConfigHelper.ParseKeywords(CustomVar1, OnKeywordParsed, null, null, ref error);
            triggerTimer = new Timer { AutoReset = false, Interval = 10 };
            triggerTimer.Elapsed += new ElapsedEventHandler(triggerTimer_Elapsed);
            triggerTimer.Enabled = false;
        }

        void triggerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double period = 10000;
            try
            {
                if (string.IsNullOrEmpty(Dal))
                {
                    period = 0;
                    return;
                }
                using (var con = Database.GetConnection(Dal))
                {
                    if (con.State == ConnectionState.Broken || con.State == ConnectionState.Closed)
                        con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandTimeout = int.MaxValue;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT COUNT(*)
FROM SYSOBJECTS tr,SYSOBJECTS ta
where tr.name='DELETE_TRG' AND tr.type='TR'
AND TR.parent_obj=ta.id and ta.type='U' AND TA.name='RECORD_MSFILESERVER_OBJACC'";
                        using (var rs = cmd.ExecuteReader())
                        {
                            if (rs.Read() && rs.GetInt32(0) > 0)
                            {
                                period = 0;
                                return;
                            }
                        }
                        cmd.CommandText = @"CREATE trigger [dbo].[DELETE_TRG] on [dbo].[RECORD_MSFILESERVER_OBJACC]
for  insert
as
SET NOCOUNT ON
BEGIN TRY
DELETE FROM RECORD_MSFILESERVER_OBJACC
		WHERE EXISTS (SELECT RECORD_MSFILESERVER_OBJACC.ID
		FROM (SELECT D.ID,D.COMPUTERNAME,D.CUSTOMSTR1,D.DATE_TIME
		FROM INSERTED I,RECORD_MSFILESERVER_OBJACC D
		WHERE (I.EVENTCATEGORY IN ('Delete','Create') AND I.ID=D.ID
			OR I.CUSTOMSTR1=D.CUSTOMSTR1 AND I.COMPUTERNAME=D.COMPUTERNAME
			AND I.EVENTCATEGORY IN ('OpenRead','OpenWrite','Rename')
			AND D.EVENTCATEGORY IN ('Delete','Create')
			AND I.DATE_TIME BETWEEN DATEADD(SECOND,-1,D.DATE_TIME) AND DATEADD(SECOND,1,D.DATE_TIME))) D
			WHERE D.CUSTOMSTR1=RECORD_MSFILESERVER_OBJACC.CUSTOMSTR1 AND D.COMPUTERNAME=RECORD_MSFILESERVER_OBJACC.COMPUTERNAME
			AND RECORD_MSFILESERVER_OBJACC.EVENTCATEGORY IN ('OpenRead','OpenWrite','Rename')
			AND RECORD_MSFILESERVER_OBJACC.DATE_TIME BETWEEN DATEADD(SECOND,-1,D.DATE_TIME) AND DATEADD(SECOND,1,D.DATE_TIME))
END TRY
BEGIN CATCH
	print 'Error Number: '+ERROR_NUMBER()+', Severity: '+ERROR_SEVERITY()+', State: '+ERROR_STATE()+', Procedure: '+ERROR_PROCEDURE()+', Line: '+ERROR_LINE()+', Msg: '+ERROR_MESSAGE()
END CATCH";
                        cmd.ExecuteScalar();
                        period = 0;
                    }
                }
            }
            finally
            {
                if (period > 0)
                {
                    triggerTimer.Interval = period;
                    triggerTimer.Enabled = true;
                }
            }
        }

        bool OnKeywordParsed(string keyword, bool keywordError, bool quotedKeyword, string value, bool valueError, bool quotedValue, ref Exception error)
        {
            switch (keyword)
            {
                case "L":
                    int.TryParse(value, out LangId);
                    break;
                case "FP":
                    if (!string.IsNullOrEmpty(value))
                    {
                        regFiles = new Regex(value, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    }
                    break;
                case "Benchmark":
                    benchEnable = string.Equals(value, "yes", StringComparison.InvariantCultureIgnoreCase);
                    break;
            }
            return true;
        }

        private long Convert_To_Int64(string value)
        {
            long result;
            return Int64.TryParse(value, out result) ? result : 0;
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Remote Recorder");
                    if (registryKey != null)
                        err_log = registryKey.GetValue("Home Directory") + @"\log\EventLogFileAuditRecorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager EventLogFileAuditRecorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager EventLogFileAuditRecorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogFileAuditRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogFileAuditRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogFileAuditRecorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"\log\EventLogFileAuditRecorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EventLogFileAuditRecorder").GetValue("Interval"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("EventLogFileAuditRecorder").GetValue("LastRecordNum"));

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Read_Registry -->> Error : " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                //Process(@"o:\tmp\system2.txt");
                L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder in timer1_Tick -->> Timer is Started");

                var info = user == null ? null : user.Split('\\');
                if (info != null && info.Length >= 2)
                {
                    domain = string.IsNullOrEmpty(info[0]) ? null : info[0];
                    ip = info.Length == 2 ? remoteHost : (string.IsNullOrEmpty(info[1]) ? null : info[1]);
                    domainUser = string.IsNullOrEmpty(info[info.Length - 1]) ? null : info[info.Length - 1];
                    if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(domainUser))
                    {
                        Exception error = null;
                        var localWic = AccountValidator.ValidateAccount(domain, domainUser, password, ref error);
                        if (localWic != null)
                        {
                            DisposeHelper.Close(wic);
                            wic = null;
                            wic = localWic;
                        }
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  " EventLogFileAuditRecorder In timer1_Tick() --> Impersonation Failed: " + user + ". Error:" + error);
                            return;
                        }
                    }
                }
                if (location.EndsWith("/") || location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " EventLogFileAuditRecorder In timer1_Tick() --> Directory | " + location);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In timer1_Tick() --> lastFile: " + lastFile);
                    ParseFileNameLocal();
                }
                else
                {
                    ReadLocal(location);
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "EventLogFileAuditRecorder in timer1_Tick -->> Error : " + exception.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "EventLogFileAuditRecorder in timer1_Tick -->> Timer is finished.");
            }
        }

        private void Process(string file)
        {
            using (var fs = new StreamReader(file))
            {
                string description;
                var audit = new AuditInfo();
                var domainBuffer = new StringBuilder();
                var usernameBuffer = new StringBuilder();
                var buffer = new StringBuilder();
                var sb = new StringBuilder();
                var ln = string.Empty;
                int state = 1;
                while ((ln = fs.ReadLine()) != null)
                {
                    switch (state)
                    {
                        case 1:
                            if (ln == "=========== BEGIN =============")
                                state = 2;
                            break;
                        case 2:
                            if (ln == "=========== END =============")
                            {
                                description = sb.ToString();
                                sb.Remove(0, sb.Length);
                                state = 1;
                                var rec = new Rec();
                                rec.Description = description;
                                if (!ParseDescriptionForAudit(audit, description, buffer, domainBuffer, usernameBuffer))
                                    continue;
                                Console.WriteLine("{0}: {1}", description.Substring(0, description.IndexOf('\r')), audit.ObjectName);
                                var sentItems = 0;

                                ParseAuditOperations(ref rec, audit, buffer, domainBuffer, usernameBuffer, ref sentItems);
                            }
                            else
                            {
                                sb.AppendLine(ln);
                            }
                            break;
                    }
                }
            }
        } // timer1_Tick

        private bool DEBUG_BREAK = false;
        protected Mutex callable = new Mutex();
        //

        void BenchStat(TextWriter w)
        {
            foreach (var item in benchmark)
            {
                w.WriteLine("{0} = {1}ms", item.Key, item.Value.ElapsedMilliseconds);
            }
        }

        Dictionary<string, StopwatchEx> benchmark = new Dictionary<string, StopwatchEx>();
        private StopwatchEx Benchmark(string name)
        {
            lock (benchmark)
            {
                StopwatchEx w;
                if (!benchmark.TryGetValue(name, out w))
                {
                    w = new StopwatchEx();
                    benchmark[name] = w;
                }
                w.Start();
                return w;
            }
        }

        long total = 0;

        void PrintT(long t)
        {
            for (var i = 0; i < 15; i++)
                Console.Write("\b \b");
            Console.Write("Total:{0,9}", t);
        }

        protected bool ReadLocal(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "EventLogFileAuditRecorder In ReadLocal -- Started.");

            if (!callable.WaitOne(0))
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "EventLogFileAuditRecorder In ReadLocal -- CALLED MULTIPLE TIMES STILL IN USE");
                callable.WaitOne();
                try
                {
                    throw new Exception("Parse already been processed by another thread while this call has made");
                }
                finally
                {
                    callable.ReleaseMutex();
                }
            }
            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "EventLogFileAuditRecorder In ReadLocal -- Started with lastfile: " + lastFile);
                var eventLogLocation = fileName;

                var query = last_recordnum > 0 ? "*[System/EventRecordID > " + last_recordnum + "]" : null;

                var handle = IntPtr.Zero;
                var events = new IntPtr[] { IntPtr.Zero };

                var hRenderContext = IntPtr.Zero;
                var pRenderedValues = IntPtr.Zero;
                var hRenderContextEvtData = IntPtr.Zero;
                var metaDict = new Dictionary<string, IntPtr>();

                var dwBufferUsed = 0;
                var dwPropertyCount = 0;
                var dwBufferSize = 0;
                var status = UnsafeNativeMethods.ERROR_SUCCESS;

                var session = IntPtr.Zero;

                try
                {
                    var info = user == null ? null : user.Split('\\');
                    if (info != null && info.Length >= 2)
                    {

                        domain = string.IsNullOrEmpty(info[0]) ? null : info[0];
                        ip = info.Length == 2 ? remoteHost : (string.IsNullOrEmpty(info[1]) ? null : info[1]);
                        domainUser = string.IsNullOrEmpty(info[info.Length - 1]) ? null : info[info.Length - 1];

                        if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(domainUser))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  "EventLogFileAuditRecorder In ReadLocal -- Remote Logger: " + user);
                            var login = new UnsafeNativeMethods.EvtRpcLogin()
                                {
                                    Domain = domain,
                                    User = domainUser,
                                    Password = CoTaskMemUnicodeSafeHandle.Zero,
                                    Server = ip
                                };
                            var secureString = new SecureString();

                            if (!string.IsNullOrEmpty(password))
                            {
                                foreach (var ch in password)
                                {
                                    secureString.AppendChar(ch);
                                }
                            }

                            login.Password.SetMemory(Marshal.SecureStringToCoTaskMemUnicode(secureString));
                            session = UnsafeNativeMethods.EvtOpenSession(UnsafeNativeMethods.EvtLoginClass.EvtRpcLogin,
                                                                         ref login, 0, 0);
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  "EventLogFileAuditRecorder In ReadLocal -- UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath: " +
                                  UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath);
                        }
                    }

                    /*
                         flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In ReadLocal -- UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath: " + UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath);
                    }
                    else
                    {
                     */
                    int flags;
                    if (location.Contains("\\"))
                    {
                        flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In ReadLocal --EvtQueryFilePath");
                    }
                    else
                    {
                        flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In ReadLocal --EvtQueryChannelPath");
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In ReadLocal -- " + session + " - " + eventLogLocation + " - " + query + " - " + flags);

                    handle = UnsafeNativeMethods.EvtQuery(session, eventLogLocation, query, flags);

                    if (handle == IntPtr.Zero)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                                "EventLogFileAuditRecorder In ReadLocal --  Error Opening Event File: " + Marshal.GetLastWin32Error());
                        return false;
                    }

                    hRenderContext = UnsafeNativeMethods.EvtCreateRenderContext(0, null,
                                                                                UnsafeNativeMethods
                                                                                    .EvtRenderContextFlags
                                                                                    .EvtRenderContextSystem);

                    if (hRenderContext == IntPtr.Zero)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                                "EventLogFileAuditRecorder In ReadLocal --  Error Creating Render Context Failed: " +
                                Marshal.GetLastWin32Error() + ")");
                        return false;
                    }

                    var buffer = new StringBuilder();
                    var lineBuffer = new StringBuilder();
                    var tmpBuffer = new StringBuilder();
                    var domainBuffer = new StringBuilder();
                    var usernameBuffer = new StringBuilder();
                    var returned = 0;
                    var rec = new CustomBase.Rec();
                    var audit = new AuditInfo();
                    isFileFinished = false;

                    try
                    {
                        while (UnsafeNativeMethods.EvtNext(handle, 1, events, int.MaxValue, 0, ref returned))
                        {
                            PrintT(++total);
                            try
                            {
                                using (Benchmark("GetRender"))
                                {
                                    if (!GetRenderValues(hRenderContext, events[0],
                                                         UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues,
                                                         ref dwBufferSize, ref pRenderedValues, ref dwBufferUsed,
                                                         ref dwPropertyCount, ref status))
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR,
                                              "EventLogFileAuditRecorder In ReadLocal --  Error Getting Render Event Values Failed: " +
                                              status +
                                              ")");
                                        continue;
                                    }
                                }

                                string description;
                                using (Benchmark("GetFields"))
                                {
                                    string meta;
                                    using (Benchmark("GetFields P1"))
                                    {
                                        meta =
                                            Marshal.PtrToStringAuto(
                                                ((UnsafeNativeMethods.EvtVariant)
                                                 (Marshal.PtrToStructure(pRenderedValues,
                                                                         typeof(UnsafeNativeMethods.EvtVariant))))
                                                    .StringVal);
                                        if (meta == null)
                                        {
                                            L.Log(LogType.FILE, LogLevel.INFORM,
                                                  "EventLogFileAuditRecorder In ReadLocal --  Event has no meta data. Skipping");
                                            continue;
                                        }
                                    }
                                    using (Benchmark("GetFields P2"))
                                    {
                                        rec.EventId =
                                            ((UnsafeNativeMethods.EvtVariant)
                                             Marshal.PtrToStructure(
                                                 new IntPtr((Int32)pRenderedValues +
                                                            ((int)
                                                             UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemEventID) *
                                                            Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                                 typeof(UnsafeNativeMethods.EvtVariant))).UShort;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventId: " + rec.EventId);
                                    }
                                    IntPtr metaPtr;
                                    using (Benchmark("GetFields P3"))
                                    {
                                        if (!metaDict.TryGetValue(meta, out metaPtr))
                                        {
                                            metaPtr = UnsafeNativeMethods.EvtOpenPublisherMetadata(session, meta, null,
                                                                                                   LangId,
                                                                                                   0);
                                            if (metaPtr != IntPtr.Zero)
                                                metaDict[meta] = metaPtr;
                                        }
                                    }

                                    using (Benchmark("GetFields P4"))
                                    {
                                        if (!GetMessageString(metaPtr, events[0],
                                                              UnsafeNativeMethods.EvtFormatMessageFlags
                                                                                 .EvtFormatMessageEvent,
                                                              ref buffer,
                                                              out dwBufferUsed, ref status))
                                        {
                                            L.Log(LogType.FILE, LogLevel.ERROR, "Get Description failed:" + status);
                                            continue;
                                        }
                                    }
                                    using (Benchmark("GetFields P5"))
                                    {
                                        description = buffer.ToString();
                                        buffer.Remove(0, buffer.Length);

                                        //WriteLine(description);
                                        //continue;

                                        rec.Recordnum = (int)
                                                        ((UnsafeNativeMethods.EvtVariant)
                                                         Marshal.PtrToStructure(
                                                             new IntPtr((Int32)pRenderedValues +
                                                                        ((int)
                                                                         UnsafeNativeMethods.EvtSystemPropertyId
                                                                                            .EvtSystemEventRecordId) *
                                                                        Marshal.SizeOf(
                                                                            typeof(UnsafeNativeMethods.EvtVariant))),
                                                             typeof(UnsafeNativeMethods.EvtVariant))).ULong;

                                        last_recordnum = (long)rec.Recordnum;

                                        rec.ComputerName =
                                            Marshal.PtrToStringAuto(
                                                ((UnsafeNativeMethods.EvtVariant)
                                                 (Marshal.PtrToStructure(
                                                     new IntPtr((Int32)pRenderedValues +
                                                                ((int)
                                                                 UnsafeNativeMethods.EvtSystemPropertyId
                                                                                    .EvtSystemComputer) *
                                                                Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                                     typeof(UnsafeNativeMethods.EvtVariant))))
                                                    .StringVal);
                                    }
                                    using (Benchmark("GetFields P6"))
                                    {
                                        if (!GetMessageString(metaPtr, events[0],
                                                              UnsafeNativeMethods.EvtFormatMessageFlags
                                                                                 .EvtFormatMessageTask,
                                                              ref buffer,
                                                              out dwBufferUsed, ref status))
                                        {
                                            buffer.Remove(0, buffer.Length);
                                        }
                                        rec.EventType = buffer.ToString();
                                        if (!GetMessageString(metaPtr, events[0],
                                                              UnsafeNativeMethods.EvtFormatMessageFlags
                                                                                 .EvtFormatMessageLevel,
                                                              ref buffer,
                                                              out dwBufferUsed, ref status))
                                        {
                                            buffer.Remove(0, buffer.Length);
                                        }
                                        rec.EventCategory = buffer.ToString();

                                        ulong timeCreated =
                                            ((UnsafeNativeMethods.EvtVariant)
                                             Marshal.PtrToStructure(
                                                 new IntPtr((Int32)pRenderedValues +
                                                            ((int)
                                                             UnsafeNativeMethods.EvtSystemPropertyId
                                                                                .EvtSystemTimeCreated) *
                                                            Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                                 typeof(UnsafeNativeMethods.EvtVariant))).FileTime;

                                        rec.Datetime =
                                            DateTime.FromFileTime((long)timeCreated)
                                                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                                        rec.LogName =
                                            Marshal.PtrToStringAuto(
                                                ((UnsafeNativeMethods.EvtVariant)
                                                 (Marshal.PtrToStructure(
                                                     new IntPtr((Int32)pRenderedValues +
                                                                ((int)
                                                                 UnsafeNativeMethods.EvtSystemPropertyId
                                                                                    .EvtSystemChannel) *
                                                                Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                                     typeof(UnsafeNativeMethods.EvtVariant))))
                                                    .StringVal);

                                        rec.Description = description;
                                    }
                                }
                                var sentItems = 0;
                                using (Benchmark("ParseDescriptionForAudit"))
                                {
                                    if (
                                        !ParseDescriptionForAudit(audit, description, buffer, domainBuffer,
                                                                  usernameBuffer))
                                        continue;
                                }
                                using (Benchmark("ParseAuditOperations2"))
                                {
                                    ParseAuditOperations2(ref rec, audit, buffer, domainBuffer, usernameBuffer,
                                                          ref sentItems);

                                    if (sentItems > 0)
                                        continue;
                                    if (audit.Reasons.Count == 0 && audit.OriginalRights.Count == 0 &&
                                        audit.NewRights.Count == 0)
                                        continue;
                                }
                                rec.Description = description;
                                rec.CustomStr2 = audit.ObjectType;
                                rec.CustomStr3 = audit.ObjectName;
                                rec.CustomStr4 = audit.Sid;
                                rec.CustomStr5 = audit.Username;
                                rec.CustomStr6 = audit.Process;
                                rec.CustomInt6 = audit.ProcessId;
                                rec.CustomStr7 = audit.AccessMask;
                                using (Benchmark("ParseAuditOperations2"))
                                {
                                    if (audit.OriginalRights.Count > 0 && audit.NewRights.Count > 0)
                                        SendAccessRightChange(ref rec, audit);
                                    //else
                                    //   SendAudit(ref rec, audit);
                                }
                            }
                            finally
                            {
                                UnsafeNativeMethods.EvtClose(events[0]);
                                events[0] = IntPtr.Zero;
                            }
                        }
                    }
                    finally
                    {
                        foreach (var item in benchmark)
                        {
                            item.Value.Stop();
                        }
                        //BenchStat(Console.Out);
                        try
                        {
                            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In ReadLocal -->> Setting Registry.");
                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "", LastRecordDate);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In ReadLocal -->> Registry Set.");
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " EventLogFileAuditRecorder In ReadLocal -->> Setting Registry Error." + exception.Message);
                        }
                    }

                    isFileFinished = true;
                    return true;
                }
                finally
                {
                    CleanupEvtHandle(handle);
                    CleanupEvtHandle(events[0]);
                    CleanupEvtHandle(hRenderContext);
                    CleanupEvtHandle(hRenderContextEvtData);
                    CleanupEvtHandle(metaDict);
                }
            }
            catch (EventLogNotFoundException e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "EVTX Parser in ReadLocal ERROR." + e.Message);
            }
            finally
            {
                callable.ReleaseMutex();
            }
            return false;
        }


        //static readonly Regex RegLine = new Regex("(The handle to an object was closed\\.$)|(An attempt was made to duplicate a handle to an object\\.$)|(A handle to an object was requested\\.$)|(An object was deleted\\.$)", RegexOptions.Compiled);
        static readonly Regex RegLine = new Regex("(A handle to an object was requested\\.$)|(An object was deleted\\.$)", RegexOptions.Compiled);

        private static readonly object[][] RegExps =
            {
                new object[]
                    {
                        new Regex(
                            "(Security ID:[ \t]*(.*$))|(Account Name:[ \t]*(.*$))|(Account Domain:[ \t]*(.*$))|(Logon ID:[ \t]*(.*$))|(Handle ID:[ \t]*(.*$))|(Process ID:[ \t]*(.*$))|(Process Name:[ \t]*(.*$))",
                            RegexOptions.Compiled),
                            127
                    },
                new object[]
                    {
                        new Regex(
                            "(Security ID:[ \t]*(.*$))|(Account Name:[ \t]*(.*$))|(Account Domain:[ \t]*(.*$))|(Logon ID:[ \t]*(.*$))|(Source Handle ID:[ \t]*(.*$))|(Source Process ID:[ \t]*(.*$))|(Target Handle ID:[ \t]*(.*$))|(Target Process ID:[ \t]*(.*$))",
                            RegexOptions.Compiled),
                            255
                    },
                new object[]
                    {
                        new Regex(
                            "(Security ID:[ \t]*(.*$))|(Account Name:[ \t]*(.*$))|(Account Domain:[ \t]*(.*$))|(Logon ID:[ \t]*(.*$))|(Object Name:[ \t]*(.*$))|(Handle ID:[ \t]*(.*$))|(Process ID:[ \t]*(.*$))|(Access Mask:[ \t]*(.*$))|(Process Name:[ \t]*(.*$))",
                            RegexOptions.Compiled),
                            511
                    },
                    new object[]
                    {
                        new Regex(
                            "(Security ID:[ \t]*(.*$))|(Account Name:[ \t]*(.*$))|(Account Domain:[ \t]*(.*$))|(Logon ID:[ \t]*(.*$))|(Handle ID:[ \t]*(.*$))|(Process ID:[ \t]*(.*$))",
                            RegexOptions.Compiled),
                            63
                    }
            };

        private void ParseAuditOperations(ref Rec rec, AuditInfo audit, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer, ref int sentItems)
        {
            try
            {
                if (string.IsNullOrEmpty(rec.Description))
                    return;

                using (var reader = new StringReader(rec.Description))
                {
                    string line;
                    var state = -1;

                    var mask = 0;
                    var values = new string[9];

                    while ((line = reader.ReadLine()) != null)
                    {
                        Match m;
                        switch (state)
                        {
                            case -1:
                                m = RegLine.Match(line);
                                if (m.Success)
                                {
                                    for (state = 1; state < m.Groups.Count && !m.Groups[state].Success; ++state)
                                    {
                                    }
                                    if (state == m.Groups.Count)
                                        state = -1;
                                    else
                                        --state;
                                }
                                break;
                            default:
                                m = ((Regex)RegExps[state][0]).Match(line);
                                if (m.Success)
                                {
                                    //WriteLine(line);
                                    var i = 1;
                                    var ms = 1;
                                    while (i < m.Groups.Count)
                                    {
                                        if (m.Groups[i].Success)
                                        {
                                            values[i / 2] = m.Groups[i + 1].Value;
                                            mask |= ms;
                                            break;
                                        }
                                        i += 2;
                                        ms <<= 1;
                                    }
                                    if (mask == ((int)RegExps[state][1]))
                                    {
                                        var username = string.IsNullOrEmpty(values[1]) || string.IsNullOrEmpty(values[2])
                                                       ? UserWithDomain(values[0], buffer, domainBuffer, userBuffer)
                                                       : values[1] + "\\" + values[2];
                                        AuditLogonEnv env;
                                        if (!_handles.TryGetValue(values[3], out env))
                                        {
                                            env = new AuditLogonEnv(values[3]);
                                            _handles[env.LogonId] = env;
                                        }
                                        try
                                        {
                                            switch (state)
                                            {
                                                case 0:
                                                    AuditCloseHandle(ref rec, env, username, values[0], values[4], values[5], values[6], ref sentItems);
                                                    return;
                                                case 1:
                                                    AuditDuplicateHandle(ref rec, env, values[3], username, values[0], values[4], values[5],
                                                                         values[6], values[7], ref sentItems);
                                                    return;
                                                case 2:
                                                    if (audit.ObjectType == "File")
                                                        AuditObjectRequestHandle(ref rec, env, values[3], username, values[0], values[5], values[6],
                                                                                 values[8], values[4], values[7], ref sentItems);
                                                    return;
                                                case 3:
                                                    AuditDeleteHandle(ref rec, env, username, values[0], values[4], values[5], values[6], ref sentItems);
                                                    return;
                                            }
                                        }
                                        finally
                                        {
                                            if (env.Handles.Count == 0)
                                                _handles.Remove(env.LogonId);
                                            //WriteLine("===================================================");
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error while parsing audit operations:" + e);
            }
        }

        private void ParseAuditOperations2(ref Rec rec, AuditInfo audit, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer, ref int sentItems)
        {
            try
            {
                if (string.IsNullOrEmpty(rec.Description))
                    return;

                using (var reader = new StringReader(rec.Description))
                {
                    string line;
                    var state = -1;

                    var mask = 0;
                    var values = new string[9];

                    while (true)
                    {
                        using (Benchmark("ParseAuditOperations2 ReadLine"))
                        {
                            line = reader.ReadLine();
                        }
                        if (line == null)
                            break;

                        Match m;
                        switch (state)
                        {
                            case -1:
                                m = RegLine.Match(line);
                                if (m.Success)
                                {
                                    state = m.Groups[1].Success ? 2 : 3;
                                    /*
                                    for (state = 1; state < m.Groups.Count && !m.Groups[state].Success; ++state)
                                    {
                                    }
                                    if (state == m.Groups.Count)
                                        state = -1;
                                    else
                                        ++state;
                                     * */
                                }
                                break;
                            default:
                                m = ((Regex)RegExps[state][0]).Match(line);
                                if (m.Success)
                                {
                                    //WriteLine(line);
                                    var i = 1;
                                    var ms = 1;
                                    while (i < m.Groups.Count)
                                    {
                                        if (m.Groups[i].Success)
                                        {
                                            values[i / 2] = m.Groups[i + 1].Value;
                                            mask |= ms;
                                            break;
                                        }
                                        i += 2;
                                        ms <<= 1;
                                    }
                                    if (mask == ((int)RegExps[state][1]))
                                    {
                                        var username = string.IsNullOrEmpty(values[1]) || string.IsNullOrEmpty(values[2])
                                                       ? UserWithDomain(values[0], buffer, domainBuffer, userBuffer)
                                                       : values[1] + "\\" + values[2];
                                        AuditLogonEnv2 env;
                                        lock (_handles2)
                                        {
                                            if (!_handles2.TryGetValue(values[3], out env))
                                            {
                                                env = new AuditLogonEnv2(values[3]);
                                                _handles2[env.LogonId] = env;
                                            }
                                        }

                                        if (state == 3)
                                        {
                                            using (Benchmark("ParseAuditOperations2 DeleteHandle2"))
                                            {
                                                AuditDeleteHandle2(ref rec, env, username, values[0], values[4],
                                                                   values[5],
                                                                   ref sentItems);
                                            }
                                        }
                                        else if (audit.ObjectType == "File")
                                        {
                                            //Console.WriteLine(rec.Description);
                                            //var ln = Console.ReadLine();
                                            //if (ln.Equals("d"))
                                            //    DEBUG_BREAK = true;
                                            //else if (ln.Equals("q"))
                                            //    DEBUG_BREAK = false;

                                            using (Benchmark("ParseAuditOperations2 AuditObjectRequestHandle2"))
                                            {
                                                AuditObjectRequestHandle2(ref rec, env, values[3], username, values[0],
                                                                          values[5], values[6],
                                                                          values[8], values[4], values[7], ref sentItems);
                                            }

                                        }
                                        return;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error while parsing audit operations:" + e);
            }
        }

        Regex regIgnore = new Regex("^(Subject:|[ \t]*Security ID:|[ \t]*Account Name:|[ \t]*Account Domain:|[ \t]*Domain Name:|[ \t]*Logon ID:|[ \t]*$|[ \t]*Restricted SID Count|[ \t]*Transaction ID:|Object:|[ \t]*Object Server:|[ \t]*Object Type:|Process Information:|[ \t]*Process Name:|Access Request Information:)", RegexOptions.Compiled);
        Regex regFile = new Regex("Object Type:[ \t]*File", RegexOptions.Compiled);
        private bool WriteLine2(string line)
        {
            if (!regFile.IsMatch(line))
                return false;
            using (var reader = new StringReader(line))
            {
                using (var fs = new StreamWriter(@"o:\tmp\system.txt", true))
                {
                    var ln = string.Empty;
                    var i = 0;
                    while ((ln = reader.ReadLine()) != null)
                    {
                        if (!regIgnore.IsMatch(ln))
                        {
                            fs.WriteLine(ln);
                            ++i;
                        }
                    }
                    return i > 0;
                }
            }
        }


        private void WriteLine(string line)
        {
            if (!line.StartsWith("An object was deleted"))
            {
                if (!line.StartsWith("A handle to an object was requested") || !regFile.IsMatch(line))
                    return;
            }
            using (var fs = new StreamWriter(@"o:\tmp\system.txt", true))
            {
                fs.WriteLine("=========== BEGIN =============");
                fs.WriteLine(line);
                fs.WriteLine("=========== END =============");
            }
        }

        private static AccessMask DirectoryMask = AccessMask.ReadControl | AccessMask.Synchronize | AccessMask.ReadOrListDirectory | AccessMask.WriteOrAddFile;
        private static AccessMask FileMask = AccessMask.ReadControl | AccessMask.Synchronize | AccessMask.ReadOrListDirectory | AccessMask.WriteOrAddFile | AccessMask.AppendOrAddSubDir;

        private static Regex RegKernelPath = new Regex(@"^(\\[^\\]+\\[^\\]*)(\\.*$)?", RegexOptions.Compiled);
        private static Regex RegWinPath = new Regex("^[^:]+:");

        private void AuditObjectRequestHandle(ref Rec rec, AuditLogonEnv env, string logonId, string username, string userSid, string handleId, string processId, string processName, string objectName, string accessMask, ref int sentItems)
        {
            var access = (uint)GetPid(accessMask);

            AuditHandle handle;
            if (!env.Handles.TryGetValue(handleId, out handle))
            {
                handle = new AuditHandle()
                    {
                        Handle = handleId,
                        Object = new AuditObject()
                    };
                env.Handles[handleId] = handle;
            }

            if (!string.IsNullOrEmpty(objectName))
            {
                var kernel = RegKernelPath.Match(objectName);
                if (kernel.Success)
                    objectName = QueryDosPath(kernel.Groups[1].Value, kernel.Groups[2].Value);
            }
            handle.Object.LogonId = logonId;
            handle.Object.Name = objectName;
            handle.Object.Owner = user;
            handle.Object.OwnerSid = userSid;
            handle.OwnerProcess[processId] = processName;
            handle.Object.Handles[handleId] = handle;
            handle.AccessType = (AccessMask)access;
            try
            {
                AuditHandle lastHandle;
                if (env.ProcessLastAudit.TryGetValue(processId, out lastHandle))
                {
                    if (lastHandle.Object.Name == handle.Object.Name)
                    {
                        lastHandle.AccessType |= handle.AccessType;
                        env.Handles.Remove(handle.Handle);
                        handle = lastHandle;
                        return;
                    }
                    if ((lastHandle.AccessType & AccessMask.Delete) == AccessMask.Delete
                        && (handle.AccessType & (AccessMask.WriteOrAddFile | AccessMask.AppendOrAddSubDir)) != AccessMask.None)
                    {
                        string customStr2;
                        if ((handle.AccessType & AccessMask.WriteOrAddFile) == AccessMask.WriteOrAddFile)
                            customStr2 = "File";
                        else
                            customStr2 = "Directory";
                        env.Handles.Remove(lastHandle.Handle);
                        int pid = GetPid(processId);
                        var prevName = lastHandle.Object.Name;
                        var op = "MOVE";
                        if (prevName != null)
                        {
                            var currName = handle.Object.Name;
                            if (currName != null)
                            {
                                if (!currName.EndsWith("\\"))
                                    currName += "\\";
                                if (currName.Length < prevName.Length &&
                                    prevName.StartsWith(currName) &&
                                    prevName.IndexOf('\\', currName.Length) < 0)
                                    op = "RENAME";
                            }
                        }
                        SendData(ref rec, op, customStr2, lastHandle.Object.Name, userSid, username, processName,
                                 pid, accessMask, objectName);
                        sentItems++;
                    }
                }
                else if ((handle.AccessType & AccessMask.WriteDac) == AccessMask.WriteDac)
                {
                    int pid = GetPid(processId);
                    if ((handle.AccessType & FileMask) == FileMask)
                    {
                        SendData(ref rec, "NEW", "File", handle.Object.Name, userSid, user, processName, pid, accessMask,
                                 string.Empty);
                        sentItems++;
                    }
                    else if ((handle.AccessType & DirectoryMask) == DirectoryMask)
                    {
                        SendData(ref rec, "NEW", "Directory", handle.Object.Name, userSid, user, processName, pid,
                                 accessMask, string.Empty);
                        sentItems++;
                    }
                }
            }
            finally
            {
                env.ProcessLastAudit[processId] = handle;
            }
        }

        static AccessMask NewFileDirMask = (AccessMask.Synchronize | AccessMask.WriteDac | AccessMask.WriteOrAddFile | AccessMask.AppendOrAddSubDir);

        private void AuditObjectRequestHandle2(ref Rec rec, AuditLogonEnv2 env, string logonId, string username,
                                               string userSid, string handleId, string processId, string processName,
                                               string objectName, string accessMask, ref int sentItems)
        {
            if (!string.IsNullOrEmpty(objectName))
            {
                var kernel = RegKernelPath.Match(objectName);
                if (kernel.Success)
                    objectName = QueryDosPath(kernel.Groups[1].Value, kernel.Groups[2].Value);
            }
            var access = (AccessMask)GetPid(accessMask);
            //if ((access & NewFileDirMask) == NewFileDirMask)
            //{
            //    SendData(ref rec, "NEW", (access & AccessMask.AppendOrAddSubDir) == AccessMask.None ? "Directory" : "File",
            //             objectName, userSid, username, processName,
            //             GetPid(processId), accessMask, string.Empty);
            //    sentItems++;
            //    return;
            //}
            AuditHandle2 handle;
            lock (env)
            {
                if ((access & AccessMask.Synchronize) == AccessMask.Synchronize)
                {
                    if (env.ProcessLastAudit.TryGetValue(processId, out handle))
                        env.ProcessLastAudit.Remove(processId);
                    else
                        handle = null;
                    if ((access & (AccessMask.WriteOrAddFile | AccessMask.AppendOrAddSubDir)) != AccessMask.None)
                    {
                        if (handle != null)
                        {
                            if (handle.Name == objectName)
                            {
                                //handle.AccessType |= access;
                                env.ProcessLastAudit[processId] = handle;
                                ++sentItems;
                                return;
                            }

                            if (handle.Name.StartsWith(objectName))
                            {
                                if (handle.Name.Length > objectName.Length)
                                {
                                    if (objectName[objectName.Length - 1] == Path.DirectorySeparatorChar ||
                                        handle.Name[objectName.Length] == Path.DirectorySeparatorChar)
                                    {
                                        SendData(ref rec, "RENAME",
                                                 (access & AccessMask.AppendOrAddSubDir) ==
                                                 AccessMask.None
                                                     ? "Directory"
                                                     : "File",
                                                 handle.Name, userSid, username, processName,
                                                 GetPid(processId), accessMask, objectName);
                                        sentItems++;
                                        return;
                                    }
                                }
                            }
                            SendData(ref rec, "MOVE", string.Empty, handle.Name, userSid, username, processName,
                                     GetPid(processId), accessMask, objectName);
                            sentItems++;
                        }
                        else
                        {
                            SendData(ref rec, "NEW",
                                     (access & AccessMask.AppendOrAddSubDir) == AccessMask.None
                                         ? "Directory"
                                         : "File",
                                     objectName, userSid, username, processName,
                                     GetPid(processId), accessMask, string.Empty);
                            sentItems++;
                        }
                    }
                    else if ((access & AccessMask.Delete) == AccessMask.Delete)
                    {
                        if (handle == null)
                            handle = new AuditHandle2(handleId, processId, processName);

                        handle.AccessType = access;
                        handle.LogonId = logonId;
                        handle.Name = objectName;
                        handle.Owner = user;
                        handle.OwnerSid = userSid;
                        env.ProcessLastAudit[processId] = handle;
                        ++sentItems;
                    }
                }
                else if ((access & AccessMask.Delete) == AccessMask.Delete)
                {
                    env.ProcessLastAudit.Remove(processId);
                    SendData(ref rec, "DELETED", string.Empty,
                             objectName, userSid, username, processName,
                             GetPid(processId), accessMask, string.Empty);
                    sentItems++;
                }
            }
        }

        private int GetPid(string processId)
        {
            int pid;
            string str;
            NumberStyles flags;
            NumberFormatFlags(processId, out str, out flags);
            return int.TryParse(str, flags, CultureInfo.InvariantCulture, out pid) ? pid : 0;
        }

        private void NumberFormatFlags(string accessMask, out string str, out NumberStyles flags)
        {
            if (accessMask.StartsWith("0x", true, CultureInfo.InvariantCulture))
            {
                str = accessMask.Substring(2);
                flags = NumberStyles.HexNumber;
            }
            else
            {
                str = accessMask;
                flags = NumberStyles.Number;
            }

        }

        private void AuditDeleteHandle(ref Rec rec, AuditLogonEnv env, string user, string userSid,
                                          string handleId, string processId, string processName, ref int sentItems)
        {
            var pid = GetPid(processId);

            AuditHandle handle;
            if (env.Handles.TryGetValue(handleId, out handle))
            {
                if ((handle.AccessType & AccessMask.Delete) == AccessMask.Delete)
                {
                    env.Handles.Remove(handleId);
                    SendData(ref rec, "DELETED", "File", handle.Object.Name, userSid, user, processName, pid, string.Format("{0:X}", handle.AccessType), string.Empty);
                    sentItems++;
                }
            }
        }

        private void AuditDeleteHandle2(ref Rec rec, AuditLogonEnv2 env, string user, string userSid,
                                          string handleId, string processId, ref int sentItems)
        {
            AuditHandle2 handle;
            lock (env)
            {
                if (env.ProcessLastAudit.TryGetValue(processId, out handle) && handle.HandleId == handleId)
                {
                    env.ProcessLastAudit.Remove(processId);
                    SendData(ref rec, "DELETED", string.Empty, handle.Name, userSid, user, handle.ProcessName,
                             GetPid(processId), string.Format("{0:X}", handle.AccessType), string.Empty);
                    sentItems++;
                }
            }
        }

        private void SendData(ref Rec rec, string operation, string objectType, string objectName, string userSid, string user, string processName, int pid, string accessMask, string target)
        {
            try
            {
                rec.CustomStr1 = operation;
                rec.CustomStr2 = objectType;
                rec.CustomStr3 = objectName;
                rec.CustomStr4 = userSid;
                rec.CustomStr5 = user;
                rec.CustomStr6 = processName;
                rec.CustomInt6 = pid;
                rec.CustomStr7 = accessMask;
                rec.CustomStr10 = target;
                SendData(ref rec);
            }
            finally
            {
                rec.CustomStr1 = string.Empty;
                rec.CustomStr2 = string.Empty;
                rec.CustomStr3 = string.Empty;
                rec.CustomStr4 = string.Empty;
                rec.CustomStr5 = string.Empty;
                rec.CustomStr6 = string.Empty;
                rec.CustomInt6 = 0;
                rec.CustomStr7 = string.Empty;
                rec.CustomStr10 = string.Empty;
            }
        }

        private void AuditDuplicateHandle(ref Rec rec, AuditLogonEnv env, string logonId, string user, string userSid, string sourceHandleId, string sourceProcessId, string targetHandleId, string targetProcessId, ref int sentItems)
        {
            try
            {
                long pid = GetPid(targetProcessId);
                if (pid == 4)
                    targetProcessId = sourceProcessId;

                AuditCloseHandle(ref rec, env, user, userSid, targetHandleId, targetProcessId, string.Empty, ref sentItems, false);

                AuditHandle handle;
                if (!env.Handles.TryGetValue(sourceHandleId, out handle))
                {
                    handle = new AuditHandle()
                        {
                            Handle = sourceHandleId,
                            Object = new AuditObject() { LogonId = logonId, Owner = user, OwnerSid = userSid }
                        };
                    env.Handles[sourceHandleId] = handle;
                }
                env.Handles[targetHandleId] = handle;
                handle.Object.Handles[sourceHandleId] = handle;
                handle.Object.Handles[targetHandleId] = handle;
                handle.OwnerProcess[sourceProcessId] = sourceProcessId;
                if (targetProcessId != sourceProcessId)
                    handle.OwnerProcess[targetProcessId] = targetProcessId;
            }
            finally
            {
                env.ProcessLastAudit.Remove(sourceProcessId);
            }
        }

        private static AccessMask BeforeMoveMask = AccessMask.Delete & AccessMask.Synchronize &
                                                   AccessMask.ReadAttributes;
        private void AuditCloseHandle(ref Rec rec, AuditLogonEnv env, string username, string userSid, string handleId, string processId, string processName, ref int sentItems, bool removeProcessLast = true)
        {
            try
            {
                AuditHandle handle;
                if (env.Handles.TryGetValue(handleId, out handle))
                {
                    if ((handle.AccessType & BeforeMoveMask) == BeforeMoveMask)
                        return;
                    handle.OwnerProcess.Remove(processId);
                    if (handle.OwnerProcess.Keys.Count == 0)
                    {
                        if (handle.AccessType == AccessMask.Delete)
                        {
                            SendData(ref rec, "DELETED", "File", handle.Object.Name, userSid, username, processName, GetPid(processId), string.Format("{0:X}", handle.AccessType), string.Empty);
                            sentItems++;
                        }
                        env.Handles.Remove(handleId);
                        handle.Object.Handles.Remove(handleId);
                    }
                }
            }
            finally
            {
                if (removeProcessLast)
                    env.ProcessLastAudit.Remove(processId);
            }
        }

        private void SendAudit(ref Rec rec, AuditInfo audit)
        {
            foreach (var a in audit.Reasons)
            {
                rec.CustomStr1 = a.AccessRight;
                rec.CustomStr8 = a.GrantedByUser;
                rec.CustomStr9 = a.GrantedBy;
                SendData(ref rec);
            }
        }

        private void SendAccessRightChange(ref CustomBase.Rec rec, AuditInfo audit)
        {
            foreach (var o in audit.OriginalRights.Keys)
            {
                if (audit.NewRights.Remove(o))
                {
                    audit.OriginalRights[o].Valid = false;
                }
            }
            rec.CustomStr1 = "RIGHT DELETED";
            foreach (var r in audit.OriginalRights.Values)
            {
                if (r.Valid)
                {
                    rec.CustomStr8 = r.Trustee;
                    rec.CustomStr9 = r.AceType;
                    rec.CustomStr10 = r.Right;
                    SendData(ref rec);
                }
            }

            rec.CustomStr1 = "RIGHT ADDED";
            foreach (var r in audit.NewRights.Values)
            {
                rec.CustomStr8 = r.Trustee;
                rec.CustomStr9 = r.AceType;
                rec.CustomStr10 = r.Right;
                SendData(ref rec);
            }
        }

        private void SendData(ref Rec rec)
        {
            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
            L.Log(LogType.FILE, LogLevel.DEBUG, "Sending Record:" + rec.Recordnum);
            rec.Description = string.Empty;
            customServiceBase.SetData(Dal, virtualhost, rec);
            L.Log(LogType.FILE, LogLevel.DEBUG, "Set Registry:" + rec.Recordnum);
            customServiceBase.SetReg(Id, rec.Recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "", rec.Datetime);
            L.Log(LogType.FILE, LogLevel.DEBUG, "Send Complete");
        }

        private static readonly Regex RegField = new Regex("^[ \t]*([^:]*)[ \t]*:[ \t]*(.*)$", RegexOptions.Compiled);

        private bool ParseDescriptionForAudit(AuditInfo audit, string desc, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            try
            {
                audit.Reset();
                using (var reader = new StringReader(desc))
                {
                    var line = string.Empty;
                    bool consumeAgain = false;
                    while (consumeAgain || (line = reader.ReadLine()) != null)
                    {
                        consumeAgain = false;
                        var m = RegField.Match(line);
                        if (m.Success)
                        {
                            using (Benchmark("ParseDescriptionForAudit " + m.Groups[1].Value))
                            {
                                var value = m.Groups[2].Value.Trim();
                                switch (m.Groups[1].Value)
                                {
                                    case "Security ID":
                                        audit.Sid = value;
                                        break;
                                    case "Account Name":
                                        if (string.IsNullOrEmpty(audit.Username))
                                            audit.Username = value;
                                        else
                                            audit.Username += "\\" + value;
                                        break;
                                    case "Account Domain":
                                        if (string.IsNullOrEmpty(audit.Username))
                                            audit.Username = value;
                                        else
                                            audit.Username = value + "\\" + audit.Username;
                                        break;
                                    case "Object Type":
                                        audit.ObjectType = value;
                                        break;
                                    case "Object Name":
                                        var kernel = RegKernelPath.Match(value);
                                        audit.ObjectName = kernel.Success
                                                               ? QueryDosPath(kernel.Groups[1].Value,
                                                                              kernel.Groups[2].Value)
                                                               : value;
                                        break;
                                    case "Process ID":
                                        audit.ProcessId = GetPid(value);
                                        break;
                                    case "Process Name":
                                        audit.Process = value;
                                        break;
                                    case "Access Mask":
                                        audit.AccessMask = value;
                                        break;
                                    case "Access Reasons":
                                        line = ParseAccessReasons(reader, value, audit, buffer, domainBuffer, userBuffer);
                                        consumeAgain = line != null;
                                        break;
                                    case "Original Security Descriptor":
                                        audit.OriginalDaclFlags = ParseDescriptorsInto(value, audit.OriginalRights,
                                                                                       buffer, domainBuffer, userBuffer);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Original:[" + value + "]");
                                        if (audit.OriginalDaclFlags == null)
                                            return false;
                                        break;
                                    case "New Security Descriptor":
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Original:[" + value + "]");
                                        audit.NewDaclFlags = ParseDescriptorsInto(value, audit.NewRights, buffer,
                                                                                  domainBuffer, userBuffer);
                                        if (audit.NewDaclFlags == null)
                                            return false;
                                        break;
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "ParseDescriptionForAudit Error:" + e.Message);
                return false;
            }
        }

        private Dictionary<string, RemoteKernelPath> remotePathTranslation = new Dictionary<string, RemoteKernelPath>();
        Dictionary<string, string> localTranslations = new Dictionary<string, string>();
        private string QueryDosPath(string kernelRoot, string restOfPath)
        {
            try
            {
                string path;
                if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(ip))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Get LocalTranslation: " + kernelRoot + "\\" + restOfPath);
                    path = LocalTranslation(kernelRoot, restOfPath);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Get RemoteTranslation: " + kernelRoot + "\\" + restOfPath);
                    path = RemoteTranslation(kernelRoot, restOfPath);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG,
                      "Translation Result: " + kernelRoot + "\\" + restOfPath + "==>" + path);
                return path;
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Translation Error: " + kernelRoot + "\\" + restOfPath + "===>" + e);
                throw;
            }
        }

        private string RemoteTranslation(string kernelRoot, string restOfPath)
        {
            RemoteKernelPath remPath;
            if (!remotePathTranslation.TryGetValue(ip, out remPath))
            {
                Logger.Log(LogType.FILE, LogLevel.DEBUG, "Remote path: [" + ip + "," + domain + "\\" + domainUser + "]");
                remPath = new RemoteKernelPath(ip, domain + "\\" + domainUser, password) { Logger = L };
                remotePathTranslation[ip] = remPath;
            }
            var drive = remPath[kernelRoot];
            return (drive ?? kernelRoot) + restOfPath;
        }

        private string LocalTranslation(string kernelRoot, string restOfPath)
        {
            string path;
            if (!localTranslations.TryGetValue(kernelRoot, out path))
            {
                var trans = new Dictionary<string, string>();
                foreach (var drive in DriveInfo.GetDrives())
                {
                    var i = drive.Name.Length;
                    while (--i >= 0 && drive.Name[i] == Path.DirectorySeparatorChar)
                    {
                    }
                    var driveName = drive.Name.Substring(0, ++i);
                    var kernelPath = Kernel32.QueryDosDevice(driveName);
                    if (string.IsNullOrEmpty(kernelPath))
                        return kernelRoot + restOfPath;
                    trans[kernelPath] = driveName;
                }
                localTranslations = trans;
                if (!localTranslations.TryGetValue(kernelRoot, out path))
                    return kernelRoot + restOfPath;
            }
            return path + restOfPath;
        }

        static readonly Regex RegAcl = new Regex("\\(([^\\)]*)\\)", RegexOptions.Compiled);
        private string ParseDescriptorsInto(string value, Dictionary<string, AccessRightInfo> list, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            try
            {
                var raw = new RawSecurityDescriptor(value);
                if (raw.DiscretionaryAcl == null
                    || (raw.ControlFlags & ControlFlags.DiscretionaryAclPresent) != ControlFlags.DiscretionaryAclPresent)
                    return string.Empty;

                buffer.Remove(0, buffer.Length);
                var vReturn = string.Empty;
                if ((raw.ControlFlags & ControlFlags.DiscretionaryAclProtected) ==
                    ControlFlags.DiscretionaryAclProtected)
                    buffer.Append("P");
                if ((raw.ControlFlags & ControlFlags.DiscretionaryAclAutoInherited) ==
                    ControlFlags.DiscretionaryAclAutoInherited)
                    buffer.Append("AI");
                else if ((raw.ControlFlags & ControlFlags.DiscretionaryAclAutoInheritRequired) == ControlFlags.DiscretionaryAclAutoInheritRequired)
                    buffer.Append("AR");
                vReturn = buffer.ToString();
                if (ParseDacl(raw.DiscretionaryAcl, list, buffer, domainBuffer, userBuffer))
                    return vReturn;
            }
            catch
            {
            }
            return null;
        }

        private bool ParseDacl(RawAcl acl, Dictionary<string, AccessRightInfo> list, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            try
            {
                foreach (var ace in acl)
                {
                    var k = ace as KnownAce;
                    if (k == null || k.SecurityIdentifier == null)
                        continue;
                    var username = UserWithDomain(k.SecurityIdentifier, buffer, domainBuffer, userBuffer);
                    var aceType = k.AceType.ToString();
                    var aceFlags = k.AceFlags.ToString();
                    foreach (var key in AccessMasks.Keys)
                    {
                        if ((key & k.AccessMask) == key)
                        {
                            var dKey = username + "-" + aceType + "-" + AccessMasks[key];
                            if (!list.ContainsKey(dKey))
                            {
                                list[dKey] = new AccessRightInfo()
                                    {
                                        AceFlags = aceFlags,
                                        AceType = aceType,
                                        Right = AccessMasks[key],
                                        Trustee = username
                                    };
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {

            }
            return false;
        }

        private static readonly Regex RegGrantedByField = new Regex("^[ \t]*([^:]*)[ \t]*:[ \t]*(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex RegDiscretinary = new Regex("D:\\([^\\)]*\\)");
        private string ParseAccessReasons(StringReader reader, string value, AuditInfo audit,
            StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            while (!string.IsNullOrEmpty(value))
            {
                var m = RegGrantedByField.Match(value);
                if (!m.Success)
                    return value;
                value = m.Groups[2].Value.Trim();
                var index = value.IndexOf("\t", StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                    value = value.Substring(index).Trim();
                var reason = new AccessReason()
                    {
                        AccessRight = m.Groups[1].Value,
                        GrantedBy = value
                    };
                if (index >= 0)
                    reason.GrantedByUser = ParseGrantedBy(reason, buffer, domainBuffer, userBuffer);
                audit.Reasons.Add(reason);
                value = reader.ReadLine();
            }
            return null;
        }

        private string ParseGrantedBy(AccessReason reason, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            try
            {
                var raw = new RawSecurityDescriptor(reason.GrantedBy);
                if (raw.Group != null && raw.Group.AccountDomainSid != null)
                    return UserWithDomain(raw.Group.AccountDomainSid, buffer, domainBuffer, userBuffer);
                if (raw.Owner != null && raw.Owner.AccountDomainSid != null)
                    return UserWithDomain(raw.Owner.AccountDomainSid, buffer, domainBuffer, userBuffer);
                CommonAce c;
                if (raw.DiscretionaryAcl != null && raw.DiscretionaryAcl.Count > 0 && (c = raw.DiscretionaryAcl[0] as CommonAce) != null
                    && c.SecurityIdentifier != null)
                    return UserWithDomain(c.SecurityIdentifier, buffer, domainBuffer, userBuffer);
            }
            catch
            {
                return reason.GrantedBy;
            }
            return string.Empty;
        }

        private string UserWithDomain(string securityIdentifier, StringBuilder buffer, StringBuilder domainBuffer,
                                      StringBuilder userBuffer)
        {
            try
            {
                return UserWithDomain(new SecurityIdentifier(securityIdentifier), buffer, domainBuffer, userBuffer);
            }
            catch
            {
                return securityIdentifier;
            }
        }

        private string UserWithDomain(SecurityIdentifier securityIdentifier, StringBuilder buffer, StringBuilder domainBuffer, StringBuilder userBuffer)
        {
            try
            {
                using (Benchmark("UserWithDomain"))
                {
                    SsdlHelper.GetUser(securityIdentifier, buffer, domainBuffer, userBuffer);
                    if (domainBuffer.Length > 0)
                    {
                        if (userBuffer.Length > 0)
                            return domainBuffer + "\\" + userBuffer;
                        return domainBuffer.ToString();
                    }
                    return userBuffer.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static readonly Regex RegSsdl = new Regex("(([^ \t:]+):((\\([^\\)]*\\))+|([^ \t])*))", RegexOptions.Compiled);

        private string ReformatDescription(string description, StringBuilder ssdlBuffer, StringBuilder lineBuffer,
            StringBuilder tmpBuffer, StringBuilder domainBuffer, StringBuilder usernameBuffer)
        {
            try
            {
                using (var sr = new StringReader(description))
                {
                    ssdlBuffer.Remove(0, ssdlBuffer.Length);
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineBuffer.Remove(0, lineBuffer.Length);
                        var m = RegSsdl.Match(line);
                        var formatted = 0;
                        if (m.Success)
                        {
                            var last = 0;
                            do
                            {
                                lineBuffer.Append(line.Substring(last, m.Groups[1].Index - last))
                                      .Append(SsdlHelper.DecodeSsdl(m.Groups[1].Value, tmpBuffer, domainBuffer, usernameBuffer,
                                                                    ref formatted));
                                last = m.Index + m.Length;
                                m = m.NextMatch();
                            } while (m.Success);
                            if (formatted > 0)
                                lineBuffer.Append(line.Substring(last, line.Length - last));
                        }
                        if (formatted <= 0 || lineBuffer.Length == 0)
                            ssdlBuffer.AppendLine(line);
                        else
                        {
                            ssdlBuffer.AppendLine(line);
                            ssdlBuffer.Append("!!!").Append(lineBuffer).AppendLine("!!!");
                        }
                    }
                    return ssdlBuffer.ToString();
                }
            }
            catch
            {
                return description;
            }
        }

        protected void ParseFileNameLocal()
        {

            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED ");
                    //L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Position: " + last_recordnum);

                    if (location.EndsWith("/") || location.EndsWith("\\"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseFileNameLocal() -->> Searching files in directory : " + location);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList.Sort();
                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Sorting Files. Total Files:" + fileNameList.Count);
                        var idx = fileNameList.IndexOf(lastFile);
                        L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED " + lastFile);
                        LastRecordDate = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);

                        if (string.IsNullOrEmpty(lastFile))
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED last File is null. So set to [" + fileNameList[0] + "]");
                            lastFile = fileNameList[0];
                            last_recordnum = 0;
                            LastRecordDate = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> SetNewFile: " + lastFile);
                            if (!SetNewFile())
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> SetNewFile: " + lastFile + " !!FAILED!!");
                                return;
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Call ReadLocal()");
                            if (ReadLocal(lastFile))
                            {
                                SetLastFile(lastFile);
                            }
                        }

                        else
                        {
                            if (fileNameList.Contains(lastFile))
                            {
                                if (ReadLocal(lastFile))
                                {
                                    SetLastFile(lastFile);
                                }
                                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> lastFile: " + lastFile);
                            }
                            else
                            {
                                try
                                {
                                    var foundFile = false;
                                    foreach (var v in fileNameList)
                                    {
                                        if (lastFile.CompareTo(v) < 0)
                                        {
                                            lastFile = v;
                                            last_recordnum = 0;
                                            LastRecordDate = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);

                                            if (!SetNewFile())
                                            {
                                                return;
                                            }

                                            foundFile = true;
                                            break;
                                        }
                                    }
                                    if (foundFile)
                                    {
                                        if (ReadLocal(lastFile))
                                        {
                                            SetLastFile(lastFile);
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> FileName fount exception: " + exception.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        //FileName = location;
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR,
                          " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }// ParseFileNameLocal

        public bool SetLastFile(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile is started with " + fileName);

            CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
            try
            {
                var fileNameList = GetFileNamesOnLocal();
                fileNameList.Sort();
                foreach (string t in fileNameList)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          " SetLastFile() -->> Sorting Files. " + t);
                }
                var idx = fileNameList.IndexOf(lastFile);

                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " EventLogFileAuditRecorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "EventLogFileAuditRecorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " EventLogFileAuditRecorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " EventLogFileAuditRecorder In SetLastFile() -->> Record sending Error." +
                      exception.Message);
                return false;
            }
        } // SetLastFile

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " EventLogFileAuditRecorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "EventLogFileAuditRecorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        private static string appendArrayElements(string[] arr)
        {
            string totalString = "";
            for (int i = 1; i < arr.Length; i++)
            {
                totalString += ":" + arr[i].Trim();
            }
            //return totalString.TrimStart(":".ToCharArray()).TrimEnd(":".ToCharArray());
            return totalString.Trim(':').Trim('f').Trim(':').Trim('f');
        }

        protected void CleanupEvtHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                try
                {
                    UnsafeNativeMethods.EvtClose(handle);
                }
                catch
                {

                }
            }
        }

        protected void CleanupEvtHandle(Dictionary<string, IntPtr> handles)
        {
            if (handles != null)
            {
                try
                {
                    Dictionary<string, IntPtr>.Enumerator e = handles.GetEnumerator();
                    while (e.MoveNext())
                    {
                        CleanupEvtHandle(e.Current.Value);
                    }
                }
                catch
                {
                }
            }
        }

        protected bool GetRenderValues(IntPtr hContext, IntPtr hEvent, UnsafeNativeMethods.EvtRenderFlags flags,
                                            ref int dwBufferSize, ref IntPtr pRenderedValues, ref int dwBufferUsed, ref int dwPropertyCount, ref int status)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In GetRenderValues Started. ");

            try
            {
                if (!UnsafeNativeMethods.EvtRender(hContext, hEvent, flags, dwBufferSize, pRenderedValues,
                                                   out dwBufferUsed,
                                                   out dwPropertyCount))
                {
                    if (UnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER ==
                        (status = Marshal.GetLastWin32Error()))
                    {
                        dwBufferSize = dwBufferUsed;
                        pRenderedValues = pRenderedValues == IntPtr.Zero
                                              ? Marshal.AllocHGlobal((IntPtr)dwBufferSize)
                                              : Marshal.ReAllocHGlobal(pRenderedValues,
                                                                       (IntPtr)dwBufferSize);
                        if (pRenderedValues != IntPtr.Zero)
                        {
                            if (UnsafeNativeMethods.EvtRender(hContext, hEvent, flags, dwBufferSize, pRenderedValues,
                                                              out dwBufferUsed, out dwPropertyCount))
                            {
                                return true;
                            }
                        }
                    }
                    status = Marshal.GetLastWin32Error();
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                status = (int)UnsafeNativeMethods.ERROR_UNHANDLED_EXCEPTION;
            }
            return false;
        }
        private object mutex = new object();
        protected bool GetMessageString(IntPtr hMetadata, IntPtr hEvent,
                                            UnsafeNativeMethods.EvtFormatMessageFlags FormatId, ref StringBuilder pBuffer, out int dwBufferUsed, ref int status)
        {
            if (!Monitor.TryEnter(mutex))
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "EventLogFileAuditRecorder In GetMessageString MultiCall. ");
                Monitor.Enter(mutex);
            }
            try
            {

                L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder In GetMessageString Started. ");

                pBuffer.Remove(0, pBuffer.Length);
                if (
                    !UnsafeNativeMethods.EvtFormatMessage(hMetadata, hEvent, 0, 0, null, FormatId, pBuffer.Capacity, pBuffer,
                                                          out dwBufferUsed))
                {
                    status = Marshal.GetLastWin32Error();

                    if (UnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER == status)
                    {
                        pBuffer.EnsureCapacity(dwBufferUsed * 4);
                        if (UnsafeNativeMethods.EvtFormatMessage(hMetadata, hEvent, 0, 0, null, FormatId, pBuffer.Capacity,
                                                             pBuffer,
                                                             out dwBufferUsed))
                        {
                            return true;
                        }
                        status = Marshal.GetLastWin32Error();//
                    }

                    if (status == UnsafeNativeMethods.ERROR_EVT_MESSAGE_NOT_FOUND
                        || status == UnsafeNativeMethods.ERROR_EVT_MESSAGE_ID_NOT_FOUND
                        || status == UnsafeNativeMethods.ERROR_EVT_UNRESOLVED_VALUE_INSERT)
                    {
                        pBuffer.Remove(0, pBuffer.Length);
                        return true;
                    }
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventLogFileAuditRecorder->GetMessageString " + pBuffer);
                    return true;
                }
                return false;
            }
            finally
            {
                Monitor.Exit(mutex);
            }
        }
        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                List<String> fileNameList = new List<String>();
                foreach (var file in Directory.GetFiles(location))
                {
                    var fInfo = new FileInfo(file);
                    if (regFiles.IsMatch(fInfo.Name))
                        fileNameList.Add(fInfo.FullName);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnLocal


        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between



        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ' && (!useTabs || c != '\t'))
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }// SpaceSplit

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("EventLogFileAuditRecorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SQLServer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                if (!string.IsNullOrEmpty(LogLocation))
                    err_log = Path.Combine(LogLocation, @"EventLogFileAuditRecorder" + Id + ".log");
                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
        ~EventLogFileAuditRecorder()
        {
            try
            {
                if (benchEnable && !string.IsNullOrEmpty(err_log))
                {
                    using (var fs = new StreamWriter(Path.Combine(new FileInfo(err_log).Directory.FullName,
                                                          "EventLogFileAuditBenchmark.txt")))
                    {
                        BenchStat(fs);
                    }
                }
            }
            catch
            {
            }
        }

        public bool benchEnable { get; set; }
    }
}
