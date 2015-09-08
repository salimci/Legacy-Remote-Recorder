/*
 * Application Parser Base
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Log;
using CustomTools;
using DAL;

namespace Parser
{
    public class AppParser : CustomBase
    {
        RegistryKey reg;

        protected String Dir;
        protected CLogger Log;
        protected String LogName;
        protected Int64 Position;
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
        private bool startFromEndOnLoss;
        protected Int32 threadSleepTime;
        protected Int32 maxReadLineCount;
        private String lastKeywords;
        private String LastRecDate;
        protected String remoteHost;
        protected String user;
        protected String password;
        protected Int32 zone;

        Timer checkTimer;

        public AppParser()
        {
            usingRegistry = true;
            usingKeywords = true;
            keywordsFound = false;
            startFromEndOnLoss = false;
            Id = 0;

            checkTimer = new Timer(2000);
            checkTimer.Elapsed += new ElapsedEventHandler(checkTimer_Elapsed);

            Log = new CLogger();
            Log.SetLogLevel(LogLevel.NONE);
        }

        public override void Start()
        {
            checkTimer.Start();
            checkTimer.Stop();
            ValidateMe();
            Parse();
            ReleaseMe();
            checkTimer.Start();
        }

        public virtual void Parse()
        {
            throw new Exception("You havent implemented Parse() method, please implement it. If you added this please remove \"base.Parse();\"");
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

        private string lastUser = string.Empty;
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
            if (wic != null && lastUser == user)
                return;

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
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
                                lastUser = user;
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

        void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            checkTimer.Stop();
            Parse();
            checkTimer.Start();
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
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
            Log.SetLogLevel((LogLevel)TraceLevel);
            Virtualhost = virtualhost;
            Dal = dal;

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
                reg.SetValue("LastPosition", Position);
                reg.SetValue("LastLine", lastLine);
                reg.SetValue("LastFile", lastFile);
            }
            else
            {
                CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                s.SetReg(Id, Position.ToString(), lastLine, lastFile, lastKeywords, LastRecDate);
            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Registry");
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
                if (!string.IsNullOrEmpty(password))
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
                    Log.SetLogLevel(LogLevel.NONE);
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
                    Log.SetLogLevel(LogLevel.NONE);
                    return;
                }
            }
            Log.SetLogFile(home + "log\\" + LogName + Id.ToString() + ".log");
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
