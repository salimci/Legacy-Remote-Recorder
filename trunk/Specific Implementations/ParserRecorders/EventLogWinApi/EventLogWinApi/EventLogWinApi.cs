using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomTools;
using System.Timers;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Log;
using DAL;
using System.Globalization;


namespace EventLogWinApi
{
    public class EventLogWinApi : CustomBase
    {



        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

        RegistryKey reg;

        protected String Dir;
        protected CLogger Log;
        protected String LogName;
        protected Int64 Position;
        protected Int64 lastRecordNumber;
        protected String lastLine;
        protected String lastFile;
        protected Int32 Id;
        protected String Virtualhost;
        protected String Dal;
        protected String home;
        protected LogLevel logLevel;
        protected bool usingRegistry;
        protected bool usingKeywords;
        protected bool keywordsFound;
        protected bool startFromEndOnLoss;
        protected Int32 threadSleepTime;
        protected Int32 maxReadLineCount;
        private String lastKeywords;
        private String LastRecDate;
        protected String remoteHost;
        protected String user;
        protected String password;
        protected Int32 zone;
        protected String CustomVar1;
        Timer checkTimer;

        public EventLogWinApi()
        {
            Log = new CLogger();
            checkTimer = new Timer(2000);
            checkTimer.Elapsed += new ElapsedEventHandler(checkTimer_Elapsed);
        }

        public void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "checkTimer_Elapsed");
            checkTimer.Stop();
            if (CustomVar1 == "2003")
                ParseInt();
            else
                Parse();
            checkTimer.Start();

        }

        public override void Start()
        {
            ValidateMe();
            if (CustomVar1 == "2003")
                ParseInt();
            else
                Parse();

            ReleaseMe();
        }


        public virtual void Parse()
        {
            throw new Exception("You havent implemented Parse() method, please implement it. If you added this please remove \"base.Parse();\"");
        }
        public virtual void ParseInt()
        {
            throw new Exception("You havent implemented Parse() method, please implement it. If you added this please remove \"base.Parse();\"");
        }

        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountSid(
            string lpSystemName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            StringBuilder lpName,
            ref uint cchName,
            StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SidNameUse peUse);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int GetOldestEventLogRecord(IntPtr hEventLog, ref int OldestRecord);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern long GetOldestEventLogRecord(IntPtr hEventLog, ref long OldestRecord);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "OpenEventLog")]
        public static extern IntPtr OpenEventLog(
            [MarshalAs(UnmanagedType.LPStr)] String lpUNCServerName,
            [MarshalAs(UnmanagedType.LPStr)] String lpSourceName);

        //[DllImport("advapi32.dll", SetLastError = true)]
        //public static extern IntPtr OpenEventLog(string machineName, string logName);
        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "CloseEventLog")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseEventLog(IntPtr hEventLog);

        //[DllImport("advapi32.dll", SetLastError = true, EntryPoint = "ReadEventLog")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool ReadEventLog(
        //    IntPtr hEventLog,
        //    Int32 dwReadFlags,
        //    UInt32 dwRecordOffset,
        //    [Out()] byte[] lpBuffer,
        //  Int32 nNumberOfBytesToRead,
        //  ref Int32 pnBytesRead,
        //  ref Int32 pnMinNumberOfBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int ReadEventLog(IntPtr hEventLog, ReadFlags dwReadFlags,
            Int32 dwRecordOffset, byte[] buffer, int nNumberOfBytesToRead, ref int pnBytesRead,
            ref int pnMinNumberOfBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int ReadEventLog(IntPtr hEventLog, ReadFlags dwReadFlags,
            Int64 dwRecordOffset, byte[] buffer, int nNumberOfBytesToRead, ref int pnBytesRead,
            ref int pnMinNumberOfBytesNeeded);


        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int GetNumberOfEventLogRecords(IntPtr hEventLog, ref int NumberOfRecords);


        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int GetNumberOfEventLogRecords(IntPtr hEventLog, ref long NumberOfRecords);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer,
           uint nSize, IntPtr Arguments);

        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer,
           uint nSize, String[] Arguments);

        // the version, the sample is built upon:
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer,
           uint nSize, IntPtr pArguments);

        // the parameters can also be passed as a string array:
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern uint FormatMessage(uint dwFlags, IntPtr lpSource,
           uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer,
           uint nSize, string[] Arguments);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId,
            uint dwLanguageId, ref IntPtr lpBuffer, int nSize, IntPtr[] arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadFlags dwFlags);


        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

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
        public enum LoadFlags : uint
        {
            LibraryAsDataFile = 0x002,
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001
        }

        public enum FormatMessageFlags
        {
            AllocateBuffer = 0x100,
            IgnoreInserts = 0x200,
            FromHModule = 0x0800,
            FromSystem = 0x1000,
            ArgumentArray = 0x2000
        }
        public static Int32 PRIMARYLANGID(Int32 lcid)
        {
            return ((UInt16)lcid) & 0x3ff;
        }
        public static Int32 SUBLANGID(Int32 lcid)
        {
            return ((UInt16)lcid) >> 10;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EVENTLOGRECORD
        {
            public Int32 Length;
            public Int32 Reserved;
            public Int32 RecordNumber;
            public Int32 TimeGenerated;
            public Int32 TimeWritten;
            public Int32 EventID;
            public Int16 EventType;
            public Int16 NumStrings;
            public Int16 EventCategory;
            public Int16 ReservedFlags;
            public Int32 ClosingRecordNumber;
            public Int32 StringOffset;
            public Int32 UserSidLength;
            public Int32 UserSidOffset;
            public Int32 DataLength;
            public Int32 DataOffset;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct SID_IDENTIFIER_AUTHORITY
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            Byte[] Value;
        };

        public enum ReadFlags
        {
            EVENTLOG_SEQUENTIAL_READ = 0x0001,
            EVENTLOG_SEEK_READ = 0x0002,
            EVENTLOG_FORWARDS_READ = 0x0004,
            EVENTLOG_BACKWARDS_READ = 0x0008
        };

        public enum EventType
        {
            Error = 0x0001,
            Warning = 0x0002,
            Information = 0x0004,
            Success = 0x0008,
            Failure = 0x0010
        };

        public enum SidNameUse
        {
            User = 1,
            Group,
            Domain,
            lias,
            WellKnownGroup,
            DeletedAccount,
            Invalid,
            Unknown,
            Computer
        }

        protected void ReleaseMe()
        {
            if (wic != null)
            {
                wic.Undo();
                wic.Dispose();
                wic = null;
            }
        }

        public void ValidateMe()
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Impersonation is successful");
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
                        Log.Log(LogType.FILE, LogLevel.ERROR, "LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }
        private const uint LANG_ENGLISH = 0x09;
        private const uint SUBLANG_DEFAULT = 0x01;    // user default
        public static UInt32 MAKELANGID(UInt32 primary, UInt32 sub)
        {
            return (UInt32)(((UInt16)sub) << 10) | ((UInt16)primary);
        }


        private static string FetchMessage(string msgDll, uint messageID, string[] replacementStrings)
        {
            // http://msdn.microsoft.com/library/en-us/debug/base/formatmessage.asp
            // http://msdn.microsoft.com/msdnmag/issues/02/08/CQA/
            // http://msdn.microsoft.com/netframework/programming/netcf/cffaq/default.aspx

            // FIXME: we should be using Marshal.StringToHGlobalAuto and 
            // Marshal.PtrToStringAuto => bug #79117

            IntPtr msgDllHandle = LoadLibraryEx(msgDll, IntPtr.Zero,
                LoadFlags.LibraryAsDataFile);
            if (msgDllHandle == IntPtr.Zero)
                // TODO: write warning
                return null;

            IntPtr lpMsgBuf = IntPtr.Zero;
            IntPtr[] arguments = new IntPtr[replacementStrings.Length];

            try
            {
                for (int i = 0; i < replacementStrings.Length; i++)
                {
                    // TODO: use StringToHGlobalAuto once bug #79117 is fixed
                    //arguments[i] = Marshal.StringToHGlobalUni(//dali
                    //    replacementStrings[i]);

                    //dali
                    arguments[i] = Marshal.StringToHGlobalAuto(
                       replacementStrings[i]);
                }




                int ret = FormatMessage(FormatMessageFlags.ArgumentArray |
                    FormatMessageFlags.FromHModule | FormatMessageFlags.AllocateBuffer,
                    msgDllHandle, messageID, MAKELANGID(LANG_ENGLISH, SUBLANG_DEFAULT),
                    ref lpMsgBuf, 0, arguments);
                if (ret != 0)
                {
                    // TODO: use PtrToStringAuto once bug #79117 is fixed
                   // string sRet = Marshal.PtrToStringUni(lpMsgBuf);dali

                    string sRet = Marshal.PtrToStringAuto(lpMsgBuf);//dali
                    //lpMsgBuf = PInvoke.LocalFree(lpMsgBuf);
                    // remove trailing whitespace (CRLF)
                    return sRet.TrimEnd(null);
                }
                else
                {
                    //int err = Marshal.GetLastWin32Error();
                    //if (err == MESSAGE_NOT_FOUND)
                    //{
                    //    // do not consider this a failure (or even warning) as
                    //    // multiple message resource DLLs may have been configured
                    //    // and as such we just need to try the next library if
                    //    // the current one does not contain a message for this
                    //    // ID
                    //}
                    //else
                    //{
                    //    // TODO: report warning
                    //}
                }
            }
            finally
            {
                //PInvoke.FreeLibrary(msgDllHandle);
            }
            return null;
        }
        protected string FormatMessage(string source, uint messageID, string[] replacementStrings)
        {
            string formattedMessage = null;

            string[] msgResDlls = GetMessageResourceDlls(source, "EventMessageFile");
            for (int i = 0; i < msgResDlls.Length; i++)
            {
                formattedMessage = FetchMessage(msgResDlls[i],
                    messageID, replacementStrings);
                if (formattedMessage != null)
                    break;
            }

            return formattedMessage != null ? formattedMessage : string.Join(
                ", ", replacementStrings);
        }

        private string[] GetMessageResourceDlls(string source, string valueName)
        {
            // Some event sources (such as Userenv) have multiple message
            // resource DLLs, delimited by a semicolon.

            RegistryKey sourceKey = FindSourceKeyByName(source,
                remoteHost, false);
            if (sourceKey != null)
            {
                string value = sourceKey.GetValue(valueName) as string;
                if (value != null)
                {
                    string[] msgResDlls = value.Split(';');
                    return msgResDlls;
                }
            }
            return new string[0];
        }
        private static RegistryKey FindSourceKeyByName(string source, string machineName, bool writable)
        {
            if (source == null || source.Length == 0)
                return null;

            RegistryKey eventLogKey = null;
            try
            {
                eventLogKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", writable);
                if (eventLogKey == null)
                    return null;

                string[] subKeys = eventLogKey.GetSubKeyNames();
                for (int i = 0; i < subKeys.Length; i++)
                {
                    using (RegistryKey logKey = eventLogKey.OpenSubKey(subKeys[i], writable))
                    {
                        if (logKey == null)
                            break;

                        RegistryKey sourceKey = logKey.OpenSubKey(source, writable);
                        if (sourceKey != null)
                            return sourceKey;
                    }
                }
                return null;
            }
            finally
            {
                if (eventLogKey != null)
                    eventLogKey.Close();
            }
        }
        protected int GetEventID(long instanceID)
        {
            long inst = (instanceID < 0) ? -instanceID : instanceID;

            // MSDN: eventID equals the InstanceId with the top two bits masked
            int eventID = (int)(inst & 0x3fffffff);
            return (instanceID < 0) ? -eventID : eventID;
        }

        protected static string LookupAccountSid(string machineName, byte[] sid)
        {
            // http://www.pinvoke.net/default.aspx/advapi32/LookupAccountSid.html
            // http://msdn.microsoft.com/library/en-us/secauthz/security/lookupaccountsid.asp

            // FIXME: StringBuilders should not have to be initialized with a
            // specific capacity => bug #79152

            StringBuilder name = new StringBuilder(16);
            uint cchName = (uint)name.Capacity;
            StringBuilder referencedDomainName = new StringBuilder(16);
            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
            SidNameUse sidUse;

            string accountName = null;

            while (accountName == null)
            {
                bool retOk = LookupAccountSid(machineName, sid, name, ref cchName,
                    referencedDomainName, ref cchReferencedDomainName,
                    out sidUse);
                if (!retOk)
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == ERROR_INSUFFICIENT_BUFFER)
                    {
                        name.EnsureCapacity((int)cchName);
                        referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                    }
                    else
                    {
                        // TODO: write warning ?
                        accountName = string.Empty;
                    }
                }
                else
                {
                    accountName = string.Format("{0}\\{1}", referencedDomainName.ToString(),
                        name.ToString());
                }
            }
            return accountName;
        }

        protected string FormatCategory(string source, int category)
        {
            string formattedCategory = null;

            string[] msgResDlls = GetMessageResourceDlls(source, "CategoryMessageFile");
            for (int i = 0; i < msgResDlls.Length; i++)
            {
                formattedCategory = FetchMessage(msgResDlls[i],
                    (uint)category, new string[0]);
                if (formattedCategory != null)
                    break;
            }

            return formattedCategory != null ? formattedCategory : "(" +
                category.ToString(CultureInfo.InvariantCulture) + ")";
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String pCustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            Dir = Location;
            lastKeywords = LastKeywords;
            Position = Convert.ToInt64(LastPosition);
            lastLine = LastLine;
            lastFile = LastFile;
            startFromEndOnLoss = FromEndOnLoss;
            maxReadLineCount = MaxLineToWait;
            threadSleepTime = SleepTime;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
            logLevel = (LogLevel)TraceLevel;
            Virtualhost = virtualhost;
            Dal = dal;
            CustomVar1 = pCustomVar1;

        }

        protected String GetLastKeywords()
        {
            if (usingRegistry)
                return (String)reg.GetValue("LastKeywords");
            else
                return lastKeywords;
        }

        protected void SetLastKeywords(String line)
        {
            if (usingRegistry)
                reg.SetValue("LastKeywords", line);
            else
            {
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetReg(Id, Position.ToString(), lastLine, lastFile, line, LastRecDate);
            }
            lastKeywords = line;
        }

        protected void SetRegistry()
        {
            if (usingRegistry)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Registry() | usingRegistry");
                reg.SetValue("LastPosition", Position);
                reg.SetValue("LastLine", lastLine);
                reg.SetValue("LastFile", lastFile);
            }
            else
            {
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetReg(Id, Position.ToString(), lastLine, lastFile, lastKeywords, LastRecDate);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Registry() Position:" + Position.ToString());
            }
        }
        protected void GetRegistry()
        {
            if (usingRegistry)
            {
                logLevel = (LogLevel)Convert.ToInt32(reg.GetValue("Trace Level"));
                Log.SetLogLevel(logLevel);
                Position = Convert.ToInt32(reg.GetValue("LastPosition"));
                lastLine = (String)reg.GetValue("LastLine");
                lastFile = (String)reg.GetValue("LastFile");
                startFromEndOnLoss = Convert.ToBoolean(Convert.ToInt32(reg.GetValue("FromEndOnLoss")));
                threadSleepTime = Convert.ToInt32(reg.GetValue("SleepTime"));
                maxReadLineCount = Convert.ToInt32(reg.GetValue("MaxLineToWait"));
                user = (String)reg.GetValue("User");
                password = (String)reg.GetValue("Password");
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);
                remoteHost = (String)reg.GetValue("Remote Host");

                RegistryKey regIn = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Agent");
                try
                {
                    home = (String)regIn.GetValue("Home Directory");
                }
                catch
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Cannot Read Agent Home directory, logging is disabled");
                    Log.SetLogLevel(LogLevel.DEBUG);
                    return;
                }
            }
            else
            {
                //if(password != "")
                //    password = Encrypter.Decyrpt("natek12pass", password);
                RegistryKey regIn = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Remote Recorder");
                try
                {
                    home = (String)regIn.GetValue("Home Directory");
                }
                catch
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Cannot Read Agent Home directory, logging is disabled");
                    Log.SetLogLevel(LogLevel.DEBUG);
                    return;
                }
            }

            Log.SetLogFile(home + "log\\" + LogName + Id.ToString() + ".log");
            Log.SetLogLevel(logLevel);
        }

        protected String GetLocation()
        {
            if (usingRegistry)
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\Security Manager\\Recorder\\" + LogName, true);
                return (String)reg.GetValue("Location");
            }
            else
                return Dir;
        }

        protected void SetRecordData(Rec r)
        {
            if (usingRegistry)
            {
                CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                s.SetData(r);
            }
            else
            {
                try
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    LastRecDate = r.Datetime;

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SetRecordData:" + Dal + ":" + Virtualhost + ":" + LastRecDate.ToString());
                    s.SetData(Dal, Virtualhost, r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SetRecordData:End()");
                }
                catch (Exception exp)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "SetRecordData:" + exp.Message);

                }
            }
        }

    }
}
