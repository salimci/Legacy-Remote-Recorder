/*
 * 
 * Event Logunun okunacağı makine Eğer İngilizceden 
 * farklı bir makine ise LangId'sini 'L=1033' şeklinde 
 * yazmak gerekecektir. 
 * LangId default 1033 olarak tanımlanmıştır hiçbirşey
 * yazılmadığı taktirde İngilizce okuyacaktır.
 *  
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using NT2008EventLogFileRecorder;
using System.Text.RegularExpressions;
using Natek.Helpers.Config;
using Natek.Helpers.Security.AccessControl;

namespace Nt2008EventLogFileV_2Recorder
{
    public class Nt2008EventLogFileV_2Recorder : CustomBase
    {
        public string LogLocation { get; set; }
        public event EventHandler RecordSent;

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, remote_host = "", lastLine, user, password, lastkeywords, lastUpdated;
        private string lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        private bool IsFileFinished;
        Dictionary<String, Int32> dictHash;
        private String LogName = "Nt2008EventLogFileV_2Recorder";
        protected bool parsing;
        protected bool isFileFinished;
        protected RegistryKey reg = null;
        private int LangId = 1033;
        private string ip;
        protected bool userData;
        Regex regFiles = new Regex("Archive.*\\.evtx$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;
        IntPtr token = IntPtr.Zero;
        IntPtr tokenDuplicate = IntPtr.Zero;
        private string lastUser = string.Empty;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr existingTokenHandle,
            int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public Nt2008EventLogFileV_2Recorder()
        {
            InitializeComponent();
            enc = Encoding.UTF8;
        }

        protected void ValidateMe()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> Begin with user:" + user);
            if (user != "" && (user != lastUser || wic == null))
            {

                L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> Try Revert");
                if (RevertToSelf())
                {
                    String userName = user;
                    String domain = "";
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> Revert Ok, prepare user data");
                    if (user.Contains("\\"))
                    {
                        String[] arr = user.Split('\\');
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> split data:" + arr.Length);
                        userName = arr[arr.Length - 1];
                        domain = arr[0];
                    }

                    var ret = LogonUser(userName, domain, password, (Int32)Parser.Parser.LogonType.LOGON32_LOGON_NEW_CREDENTIALS,
                        (Int32)Parser.Parser.LogonProvider.LOGON32_PROVIDER_DEFAULT, ref token);

                    if (ret)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> Impersonation is successful");
                            wi = new WindowsIdentity(tokenDuplicate);
                            wic = wi.Impersonate();
                            if (wic != null)
                            {
                                lastUser = user;
                                return;
                            }
                        }
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        L.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ValidateMe() -->> LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        } // ValidateMe

        private void InitializeComponent()
        {

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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nt2008EventLogFileV_2Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nt2008EventLogFileV_2Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating Nt2008EventLogFileV_2Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager Nt2008EventLogFileV_2Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  Nt2008EventLogFileV_2Recorder Init Method end.");
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
            max_record_send = MaxLineToWait; //Data amount to get per tick
            timer_interval = SleepTime; //Timer interval.
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition); //Last position
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;
            lastFile = LastFile;
            user = User;
            password = Password;
            Exception error = null;
            ConfigHelper.ParseKeywords(CustomVar1, OnKeywordParsed, null, null, OnUnhandledKeyword, ref error);
        }

        protected bool OnUnhandledKeyword(string keyword, bool quotedKeyword, string value, bool quotedValue, bool keywordValueError, ref int touchCount, ref Exception error)
        {
            if (keywordValueError && touchCount > 0)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, string.Format("Missused keyword or an error accure when parsing keyword [{0}]", keyword));
                return false;
            }
            if (keywordValueError)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, string.Format("Unknown keyword [{0}]", keyword));
                return false;
            }

            if (touchCount == 0)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, string.Format("Unhandled keyword [{0}]", keyword));
                return false;
            }

            return true;
        }

        protected bool OnKeywordParsed(string keyword, bool quotedkeyword, string value, bool quotedvalue, ref int touchcount, ref Exception error)
        {
            switch (keyword)
            {
                case "L":
                    touchcount++;
                    int.TryParse(value, out LangId);
                    break;
                case "FP":
                    touchcount++;
                    if (!string.IsNullOrEmpty(value))
                    {
                        regFiles = new Regex(value, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    }
                    break;
                case "UserData":
                    userData = true;
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
                        err_log = registryKey.GetValue("Home Directory") + @"\log\Nt2008EventLogFileV_2Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Nt2008EventLogFileV_2Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager Nt2008EventLogFileV_2Recorder Read Registry Error. " + er.Message);
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"\log\Nt2008EventLogFileV_2Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("Nt2008EventLogFileV_2Recorder").GetValue("LastRecordNum"));

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
                L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder in timer1_Tick -->> Timer is Started");

                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " Nt2008EventLogFileV_2Recorder In timer1_Tick() --> Directory | " + location);

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In timer1_Tick() --> lastFile: " + lastFile);

                    ParseFileNameLocal();
                }
                else
                {
                    ReadLocal(location);
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Nt2008EventLogFileV_2Recorder in timer1_Tick -->> Error : " + exception.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Nt2008EventLogFileV_2Recorder in timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick

        protected Mutex callable = new Mutex();
        //
        protected bool ReadLocal(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.INFORM, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Started.");

            if (!callable.WaitOne(0))
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Nt2008EventLogFileV_2Recorder In ReadLocal -- CALLED MULTIPLE TIMES STILL IN USE");
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
                L.Log(LogType.FILE, LogLevel.INFORM, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Started with lastfile: " + lastFile);
                var eventLogLocation = fileName;

                var query = last_recordnum > 0 ? "*[System/EventRecordID > " + last_recordnum + "]" : null;

                var handle = IntPtr.Zero;
                var events = new[] { IntPtr.Zero };

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
                    if (info != null && info.Length == 3)
                    {
                        string domain = string.IsNullOrEmpty(info[0]) ? null : info[0];
                        ip = string.IsNullOrEmpty(info[1]) ? null : info[1];
                        string userName = string.IsNullOrEmpty(info[2]) ? null : info[2];

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Remote Logger: " + user);

                        var login = new UnsafeNativeMethods.EvtRpcLogin()
                        {
                            Domain = domain,
                            User = userName,
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
                        session = UnsafeNativeMethods.EvtOpenSession(UnsafeNativeMethods.EvtLoginClass.EvtRpcLogin, ref login, 0, 0);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath: " + UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath);

                    }

                    /*
                         flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath: " + UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath);
                    }
                    else
                    {
                     */
                    int flags;
                    if (location.Contains("\\"))
                    {
                        flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal --EvtQueryFilePath");
                    }
                    else
                    {
                        flags = (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryChannelPath;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal --EvtQueryChannelPath");
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- " + session + " - " + eventLogLocation + " - " + query + " - " + flags);
                    handle = UnsafeNativeMethods.EvtQuery(session, eventLogLocation, query, flags);
                    var code = Marshal.GetLastWin32Error();
                    Console.WriteLine("Nt2008EventLogFileV_2Recorder In ReadLocal --  Error Opening Event File: " + code);
                    if (handle == IntPtr.Zero)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                                "Nt2008EventLogFileV_2Recorder In ReadLocal --  Error Opening Event File: " + Marshal.GetLastWin32Error());
                        return false;
                    }

                    hRenderContext = UnsafeNativeMethods.EvtCreateRenderContext(0, null,
                                                                                UnsafeNativeMethods
                                                                                    .EvtRenderContextFlags
                                                                                    .EvtRenderContextSystem);

                    var hRenderContextUser = UnsafeNativeMethods.EvtCreateRenderContext(0, null,
                                                                                UnsafeNativeMethods
                                                                                    .EvtRenderContextFlags
                                                                                    .EvtRenderContextUser);

                    if (hRenderContext == IntPtr.Zero)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                                "Nt2008EventLogFileV_2Recorder In ReadLocal --  Error Creating Render Context Failed: " +
                                Marshal.GetLastWin32Error() + ")");
                        return false;
                    }

                    var buffer = new StringBuilder();
                    var lineBuffer = new StringBuilder();
                    var tmpBuffer = new StringBuilder();
                    var domainBuffer = new StringBuilder();
                    var usernameBuffer = new StringBuilder();
                    var returned = 0;
                    var rec = new EventRecordWrapper();

                    isFileFinished = false;
                    lastLine = "-";

                    try
                    {
                        while (UnsafeNativeMethods.EvtNext(handle, 1, events, int.MaxValue, 0, ref returned))
                        {
                            try
                            {
                                rec.Reset();
                                if (userData)
                                {
                                    if (GetRenderValues(hRenderContextUser, events[0],
                                                        UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues,
                                                        ref dwBufferSize, ref pRenderedValues, ref dwBufferUsed,
                                                        ref dwPropertyCount, ref status))
                                    {
                                        buffer.Remove(0, buffer.Length);
                                        for (var i = 0; i < dwPropertyCount; i++)
                                        {
                                            var v = Marshal.PtrToStringAuto(
                                                ((UnsafeNativeMethods.EvtVariant)
                                                 (Marshal.PtrToStructure(
                                                     new IntPtr((Int32)pRenderedValues +
                                                                i *
                                                                Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                                     typeof(UnsafeNativeMethods.EvtVariant))))
                                                    .StringVal);
                                            if (v != null && (v = v.Trim()).Length > 0)
                                                buffer.AppendLine(v);
                                        }
                                        rec.Description = buffer.ToString();
                                    }
                                    buffer.Remove(0, buffer.Length);
                                }

                                if (!GetRenderValues(hRenderContext, events[0],
                                                     UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues,
                                                     ref dwBufferSize, ref pRenderedValues, ref dwBufferUsed,
                                                     ref dwPropertyCount, ref status))
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR,
                                          "Nt2008EventLogFileV_2Recorder In ReadLocal --  Error Getting Render Event Values Failed: " +
                                          status +
                                          ")");
                                    continue;
                                }
                                var meta =
                                    Marshal.PtrToStringAuto(
                                        ((UnsafeNativeMethods.EvtVariant)
                                         (Marshal.PtrToStructure(pRenderedValues,
                                                                 typeof(UnsafeNativeMethods.EvtVariant))))
                                            .StringVal);
                                if (meta == null)
                                {
                                    L.Log(LogType.FILE, LogLevel.INFORM,
                                          "Nt2008EventLogFileV_2Recorder In ReadLocal --  Event has no meta data. Skipping");
                                    continue;
                                }

                                rec.EventId =
                                    ((UnsafeNativeMethods.EvtVariant)
                                     Marshal.PtrToStructure(
                                         new IntPtr((Int32)pRenderedValues +
                                                    ((int)UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemEventID) *
                                                    Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                         typeof(UnsafeNativeMethods.EvtVariant))).UShort;
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventId: " + rec.EventId);

                                IntPtr metaPtr; 
                                if (!metaDict.TryGetValue(meta, out metaPtr))
                                {
                                    metaPtr = UnsafeNativeMethods.EvtOpenPublisherMetadata(session,
                                        meta, flags == (int)UnsafeNativeMethods.EvtQueryFlags.EvtQueryFilePath ? eventLogLocation : null, LangId, 0);
                                    if (metaPtr != IntPtr.Zero)
                                        metaDict[meta] = metaPtr;
                                }

                                if (!userData || string.IsNullOrEmpty(rec.Description))
                                {
                                    rec.Description = string.Empty;
                                    if (!GetMessageString(metaPtr, events[0],
                                                          UnsafeNativeMethods.EvtFormatMessageFlags
                                                                             .EvtFormatMessageEvent,
                                                          ref buffer,
                                                          out dwBufferUsed, ref status))
                                    {
                                        buffer.Remove(0, buffer.Length);
                                        L.Log(LogType.FILE, LogLevel.ERROR, "Get Description failed:" + status);
                                    }

                                    rec.Description = buffer.ToString();
                                }

                                if (!GetMessageString(metaPtr, events[0],
                                                      UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageTask,
                                                      ref buffer,
                                                      out dwBufferUsed, ref status))
                                {
                                    buffer.Remove(0, buffer.Length);
                                }
                                rec.TaskDisplayName = buffer.ToString();

                                if (!GetMessageString(metaPtr, events[0],
                                                      UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageLevel,
                                                      ref buffer,
                                                      out dwBufferUsed, ref status))
                                {
                                    buffer.Remove(0, buffer.Length);
                                }
                                rec.LevelDisplayName = buffer.ToString();

                                rec.MachineName =
                                    Marshal.PtrToStringAuto(
                                        ((UnsafeNativeMethods.EvtVariant)
                                         (Marshal.PtrToStructure(
                                             new IntPtr((Int32)pRenderedValues +
                                                        ((int)UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemComputer) *
                                                        Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                             typeof(UnsafeNativeMethods.EvtVariant))))
                                            .StringVal);


                                ulong timeCreated =
                                    ((UnsafeNativeMethods.EvtVariant)
                                     Marshal.PtrToStructure(
                                         new IntPtr((Int32)pRenderedValues +
                                                    ((int)UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemTimeCreated) *
                                                    Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                         typeof(UnsafeNativeMethods.EvtVariant))).FileTime;

                                rec.TimeCreated = DateTime.FromFileTime((long)timeCreated);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- TimeCreated: " + rec.TimeCreated);
                                rec.LogName =
                                    Marshal.PtrToStringAuto(
                                        ((UnsafeNativeMethods.EvtVariant)
                                         (Marshal.PtrToStructure(
                                             new IntPtr((Int32)pRenderedValues +
                                                        ((int)UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemChannel) *
                                                        Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                             typeof(UnsafeNativeMethods.EvtVariant))))
                                            .StringVal);

                                rec.RecordId =
                                    ((UnsafeNativeMethods.EvtVariant)
                                     Marshal.PtrToStructure(
                                         new IntPtr((Int32)pRenderedValues +
                                                    ((int)
                                                     UnsafeNativeMethods.EvtSystemPropertyId.EvtSystemEventRecordId) *
                                                    Marshal.SizeOf(typeof(UnsafeNativeMethods.EvtVariant))),
                                         typeof(UnsafeNativeMethods.EvtVariant))).ULong;

                                L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Getting Keywords");
                                if (!GetMessageString(metaPtr, events[0],
                                                      UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageKeyword,
                                                      ref buffer,
                                                      out dwBufferUsed, ref status))
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Getting Keywords FAILED:" + status);
                                    buffer.Remove(0, buffer.Length);
                                }
                                else
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Getting Keywords SUCCESS:[" + buffer + "]");

                                rec.KeywordsDisplayNames.Clear();
                                int s = 0, e = 0;
                                do
                                {
                                    while (e < buffer.Length && buffer[e] != '\0')
                                        ++e;
                                    if (e == s)
                                    {
                                        break;
                                    }

                                    if (e == buffer.Length)
                                    {
                                        rec.KeywordsDisplayNames.Add(buffer.ToString(s, e - s));
                                        break;
                                    }

                                    rec.KeywordsDisplayNames.Add(buffer.ToString(s, e - s));
                                    s = ++e;
                                } while (true);

                                L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In ReadLocal -- Description: " + rec.Description);

                                ParseSpecific(rec, eventLogLocation);
                                last_recordnum = (long)rec.RecordId;
                                //SetRegistry();
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
                        try
                        {
                            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In ReadLocal -->> Setting Registry.");
                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "", LastRecordDate);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In ReadLocal -->> Registry Set.");
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " Nt2008EventLogFileV_2Recorder In ReadLocal -->> Setting Registry Error." + exception.Message);
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
                        try
                        {
                            ValidateMe();
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> ValidateMe: " + exception.Message);
                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseFileNameLocal() -->> Searching files in directory : " + location);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList.Sort();
                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Sorting Files. Total Files:" + fileNameList.Count);
                        var idx = fileNameList.IndexOf(lastFile);
                        L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED " + lastFile);
                        LastRecordDate = DateTime.Now.ToString(dateFormat);

                        if (string.IsNullOrEmpty(lastFile))
                        {
                            L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED last File is null. So set to [" + fileNameList[0] + "]");
                            lastFile = fileNameList[0];
                            last_recordnum = 0;
                            LastRecordDate = DateTime.Now.ToString(dateFormat);
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
                      " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "Nt2008EventLogFileV_2Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> Record sending Error." +
                      exception.Message);
                return false;
            }
        } // SetLastFile

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "Nt2008EventLogFileV_2Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        public bool ParseSpecific(EventRecordWrapper eventInstance, string currentFile)
        {
            var r = new Rec();
            L.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific started. " + currentFile);
            r.EventId = eventInstance.EventId;

            try
            {
                r.Recordnum = (int)eventInstance.RecordId;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "RecordNum Casting Error. ");
                r.Recordnum = 0;
            }


            r.EventType = eventInstance.TaskDisplayName;
            r.Description = eventInstance.Description;
            r.EventCategory = eventInstance.LevelDisplayName;//
            r.ComputerName = eventInstance.MachineName;
            var dtCreate = Convert.ToDateTime(eventInstance.TimeCreated);
            r.Datetime = dtCreate.ToString("yyyy-MM-dd HH:mm:ss");
            r.SourceName = ip;
            L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + r.Datetime);
            L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + r.EventType);
            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + r.EventCategory);
            L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + r.ComputerName);
            L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + r.Description);

            try
            {
                #region NtEventLogRecorder 2008 Parser

                string[] descArr = r.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

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

                for (int i = 0; i < descArr.Length; i++)
                {
                    if (!descArr[i].Contains(":"))
                    {
                        if (accessMode)
                        {
                            r.CustomStr7 += " " + descArr[i].Trim();
                            if (r.CustomStr7.Length > 900)
                            {
                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                            }
                        }
                    }
                    else
                    {
                        string[] lineArr = descArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //L.Log(LogType.FILE, LogLeve//L.DEBUG, "DescArr[" + i + "]:" + DescArr[i]);


                        if (descArr[i].Contains("Logon Type"))
                        {
                            //L.Log(LogType.FILE, LogLeve//L.DEBUG, "Logon Type Bulundu:" + DescArr[i]);
                            string logontypestr = descArr[i].Split(':')[1].Trim();
                            //L.Log(LogType.FILE, LogLeve//L.DEBUG, "Logon Type Değeri:" + logontypestr);
                            if (logontypestr != "")
                            {
                                r.CustomInt3 = Convert.ToInt32(logontypestr);
                            }
                        }

                        if (lineArr[lineArr.Length - 1].Trim() == "")
                        {
                            #region Mode
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LineArr[0]:" + lineArr[0]);//
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
                        }//100000000
                        else
                        {
                            if (subjectMode)
                            {
                                #region SubjectMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "User Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;

                                    //case "Security ID":
                                    //    if ( CustomStr2 == null)
                                    //    {
                                    //         CustomStr2 = appendArrayElements(lineArr);
                                    //    }
                                    //    break;
                                    case "Logon ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt6 = 0;
                                        }
                                        break;
                                    case "Client Context ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt6 = 0;
                                        }
                                        break;
                                    case "Account Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
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
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    //case "Target Server Name":
                                    //     CustomStr2 = appendArrayElements(lineArr);
                                    //    break;
                                    case "Account Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Old Account Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "New Account Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
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
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Object Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Operation Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Handle ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt7 = 0;
                                        }
                                        break;
                                    case "Primary User Name":
                                        if (r.CustomStr1 == null)
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client User Name":
                                        if (r.CustomStr2 == null)
                                        {
                                            r.CustomStr2 = appendArrayElements(lineArr);
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
                                        if (r.CustomStr7 == null)
                                        {
                                            r.CustomStr7 = appendArrayElements(lineArr);
                                            if (r.CustomStr7.Length > 900)
                                            {
                                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                                            }
                                            dummyAccessControl = true;
                                        }
                                        break;
                                    case "Access Mask":
                                        if (dummyAccessControl)
                                        {
                                            r.CustomStr7 += " " + appendArrayElements(lineArr);
                                            if (r.CustomStr7.Length > 900)
                                            {
                                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                                            }
                                        }
                                        break;
                                    case "Operation Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
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
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "PID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
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
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Duration":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Application Instance ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Application Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (networkMode)
                            {

                                ////L.Log(LogType.FILE, LogLeve//L.DEBUG, "lineArr[0]:" + lineArr[0]);

                                #region NetworkMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Client Address":
                                        r.CustomStr3 = lineArr[lineArr.Length - 1];
                                        break;
                                    case "Source Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Workstation Name":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    //case "ffff":
                                    //     CustomStr3 = appendArrayElements(lineArr);
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
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Pre-Authentication Type":
                                        string authenPack3 = appendArrayElements(lineArr);
                                        if (authenPack3.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack3.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack3.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Logon Process":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Account":
                                        r.CustomStr1 = appendArrayElements(lineArr);
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
                                        if (r.CustomStr1 != null)
                                        {
                                            r.CustomStr2 = r.CustomStr1;
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        else
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
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
                                                r.CustomInt3 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt3 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt3 = 0;
                                        }
                                        break;
                                    case "Error Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Status Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Failure Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Caller Workstation":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Workstation Name":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Workstation":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "User Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Name":
                                        if (r.CustomStr1 != null)
                                        {
                                            r.CustomStr2 = r.CustomStr1;
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        else
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Account":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Caller User Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Caller Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Target Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Target User Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Address":
                                        r.CustomStr3 = lineArr[lineArr.Length - 1];
                                        // CustomStr3 = appendArrayElements(lineArr);dali
                                        break;
                                    case "Source Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Authentication Package":
                                        string authenPack = appendArrayElements(lineArr);
                                        if (authenPack.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack.Contains("Kerberos") || authenPack.Contains("KDS"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Pre-Authentication Type":
                                        string authenPack2 = appendArrayElements(lineArr);
                                        if (authenPack2.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack2.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack2.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Caller Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "PID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Logon Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Process":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Duration":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Object Name":
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Object Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Operation Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Handle ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt7 = 0;
                                        }
                                        break;
                                    case "Primary User Name":
                                        if (r.CustomStr1 == null)
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client User Name":
                                        if (r.CustomStr2 == null)
                                        {
                                            r.CustomStr2 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    //case "ffff":
                                    //     CustomStr3 = appendArrayElements(lineArr);
                                    //    break;


                                    //D.Ali Türkce Gelen Loglar İçin
                                    case "Kullanıcı Adı":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "İş İstasyonu Adı":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma işlemi":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma Türü":
                                        if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                            r.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                        else
                                            r.CustomInt5 = -1;
                                        break;
                                    case "Etki Alanı":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak Ağ Adresi":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Hesabı":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak İş İstasyonu":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Share Name":
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Hesap Adı":
                                        if (string.IsNullOrEmpty(r.CustomStr1))
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        else
                                            r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    /////////

                                    case "Güvenlik Kimliği":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Hesap Etki Alanı":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma Kimliği":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Türü":
                                        if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                            r.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                        else
                                            r.CustomInt5 = -1;
                                        break;

                                    case "İşlem Kimliği":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "İşlem Adı":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak Bağlantı Noktası":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Kimlik Doğrulama Paketi":
                                        string authenPack4 = appendArrayElements(lineArr);
                                        if (authenPack4.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack4.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack4.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Paket Adı (yalnızca NTLM)":
                                        string authenPack3 = appendArrayElements(lineArr);
                                        if (authenPack3.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack3.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack3.Contains("Kerberos") || authenPack3.Contains("KDS"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
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

                //Encoding.ASCII.GetByteCount(r.Description)>4000

                if (r.Description.Length > 900)
                {
                    if (r.Description.Length > 1800)
                    {
                        r.CustomStr10 = r.Description.Substring(900, 900);
                    }
                    else
                    {
                        r.CustomStr10 = r.Description.Substring(900, r.Description.Length - 900);
                    }
                    r.Description = r.Description.Substring(0, 900);
                }
                #endregion
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific, Error: " + ex.Message);
            }

            r.CustomStr9 = currentFile;
            lastFile = currentFile;

            r.EventCategory = r.EventType;

            if (eventInstance.KeywordsDisplayNames.Count > 0)
            {
                r.EventType = eventInstance.KeywordsDisplayNames[0];
            }

            if (!string.IsNullOrEmpty(r.EventType))
            {
                if (r.EventType.Contains(" "))
                {
                    r.EventType = r.EventType.Split(' ')[1];
                }
            }

            r.LogName = "NT-" + eventInstance.LogName;
            L.Log(LogType.FILE, LogLevel.DEBUG,
                  " Nt2008EventLogFileV_2Recorder In ParseSpecific -->> EventCategory: " + r.EventCategory);

            var customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
            try
            {
                LastRecordDate = r.Datetime;
                L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In ParseSpecific -->> Record sending." + last_recordnum + " - " + lastFile + " - " + LastRecordDate);
                customServiceBase.SetData(Dal, virtualhost, r);
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), "-", lastFile, "", LastRecordDate);
                L.Log(LogType.FILE, LogLevel.DEBUG, " Nt2008EventLogFileV_2Recorder In ParseSpecific -->> Record sended.");
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Nt2008EventLogFileV_2Recorder In ParseSpecific -->> Record sending Error." + exception.Message);
            }

            if (RecordSent != null)
                RecordSent(this, EventArgs.Empty);
            //SetRecordData(r);
            return true;
        }

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
                catch (Exception exception)
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
            L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In GetRenderValues Started. ");

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
                L.Log(LogType.FILE, LogLevel.ERROR, "Nt2008EventLogFileV_2Recorder In GetMessageString MultiCall. ");
                Monitor.Enter(mutex);
            }
            try
            {

                L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder In GetMessageString Started. ");

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
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Nt2008EventLogFileV_2Recorder->GetMessageString " + pBuffer);
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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("Nt2008EventLogFileV_2Recorder");
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
                    err_log = Path.Combine(LogLocation, @"Nt2008EventLogFileV_2Recorder" + Id + ".log");
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
    }
}
