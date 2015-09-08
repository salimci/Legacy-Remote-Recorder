//#define HARD_DEBUG_MODE
//#define HARD_LOG_MODE
#define CUSTOM_DEBUG_MODE
#define STAYCONNECTED
//#define USEZIPLIBRARYFORGZ

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Security.Principal;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Log;
using DAL;
using CustomTools;
using SharpSSH.SharpSsh;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
//
//

namespace Parser
{
    public class Parser : CustomBase, IDisposable
    {
        protected String FileName;
        protected String Dir;
        protected CLogger Log;
        protected String LogName;
        protected Int64 Position;
        protected Int64 lastTempPosition;
        protected Int32 Id;
        protected String Virtualhost;
        protected String Dal;
        protected String home;
        protected LogLevel logLevel;
        protected String lastLine;
        protected String lastFile;
        protected Int64 startLineCheckCount;
        protected bool usingRegistry;
        protected bool usingKeywords;
        protected bool keywordsFound;
        protected bool checkLineMismatch;
        private FileSystemWatcher fsw;
        private bool started;
        private bool startFromEndOnLoss;
        private Int32 threadSleepTime;
        private Int32 maxReadLineCount;
        private String lastKeywords;
        private String LastRecDate;
        protected bool parsing;
        protected Mutex parseLock;
        private bool disposeCheck;
        private bool loadWatcher;
        private int zone;
        protected string tempCustomVar1;

        protected RegistryKey reg = null;
        protected DateTime Today;
        protected System.Timers.Timer dayChangeTimer;

        //Ssh and remote connection
        protected SshExec se;
        protected String remoteHost;
        protected String user;
        protected String password;
        protected System.Timers.Timer checkTimer;
        protected bool usingCheckTimer;
        protected System.Timers.Timer checkTimerSSH;
        protected Int32 lineLimit;
        protected String readMethod;

#if STAYCONNECTED
        protected bool stayConnected;
#endif

        //Temporary Storage
        protected Rec sRec;
        protected Rec sRecv;
        protected bool storage;
        protected Encoding enc;

        public Parser()
        {
            started = true;
            usingRegistry = true;
            usingKeywords = true;
            keywordsFound = false;
            startFromEndOnLoss = false;
            checkLineMismatch = true;
            lineLimit = 100;
            startLineCheckCount = 10;
            parsing = false;
            parseLock = new Mutex();
            disposeCheck = false;
            loadWatcher = false;
            readMethod = "sed";
            enc = Encoding.Default;

#if STAYCONNECTED
            stayConnected = false;
#endif

            Id = 0;

            checkTimer = new System.Timers.Timer(60000);
            checkTimer.Enabled = false;
            checkTimer.Elapsed += new System.Timers.ElapsedEventHandler(checkTimer_Elapsed);
            usingCheckTimer = false;

            checkTimerSSH = new System.Timers.Timer(15000);
            checkTimerSSH.Enabled = false;
            checkTimerSSH.Elapsed += new System.Timers.ElapsedEventHandler(checkTimerSSH_Elapsed);

            Log = new CLogger();
            Log.SetLogLevel(LogLevel.ERROR);
        }

        public Parser(String file)
        {
            FileName = file;

            LoadWatcher();

            started = true;

            Log = new CLogger();

            Log.SetLogLevel(LogLevel.ERROR);
        }

        //protected  virtual Parser (int aI)
        //{
            
        //}

        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
#if HARD_LOG_MODE
                   Log.Log(LogType.FILE, LogLevel.DEBUG, "fsw_Changed");
#endif
            Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In fsw_Changed -->> Entered Function");
            Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In fsw_Changed -->> " + e.FullPath.ToString());
            if (fsw == null)
                return;
            fsw.EnableRaisingEvents = false;
            if (!parsing)
            {//
                if (e.FullPath == Path.GetFullPath(FileName))
                    Parse();
            }
            try
            {
                if (fsw != null)
                    fsw.EnableRaisingEvents = true;
            }
            catch
            {
            }
        }

        private void fsw_Deleted(object sender, FileSystemEventArgs e)
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "fsw_Deleted");
#endif
            Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In fsw_Deleted -->> Entered Function");
            Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In fsw_Deleted -->> " + e.FullPath.ToString());

            if (fsw == null)
                return;
            if (e.FullPath == Path.GetFullPath(FileName))
            {
                fsw.EnableRaisingEvents = false;
                Stop();
                Position = 0;
                lastLine = "";
                ParseFileName();
                Start();
                try
                {
                    if (fsw != null)
                        fsw.EnableRaisingEvents = true;
                }
                catch
                {
                }
            }
        }

        void fsw_Disposed(object sender, EventArgs e)
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "fsw_Disposed");
#endif

            if (!checkTimer.Enabled && !disposeCheck)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In fsw_Disposed -->> File Watcher Disposed, Trying To Recover");
                checkTimer.Start();
            }
        }

        protected virtual void dayChangeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
#if HARD_LOG_MODE
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "dayChangeTimer_Elapsed");
#endif

            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In dayChangeTimer_Elapsed() -->> dayChangeTimer Is Started");

            dayChangeTimer.Stop();

            if (Today.Day != DateTime.Now.Day || FileName == null)
            {
                String oldFile = FileName;
                DateTime oldTime = Today;
                Today = DateTime.Now;
                Stop();
                Position = 0;
                lastLine = "";
                ParseFileName();
                if (oldFile == FileName)
                {
                    if (FileName == null)
                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In dayChangeTimer_Elapsed() -->> Cannot Find File To Parse, Please Check Your Path!");
                    else
                        Log.Log(LogType.FILE, LogLevel.WARN, "   Parser In dayChangeTimer_Elapsed() -->> Day Changed But New File Not Found, Will Try Again");
                    Today = oldTime;
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In dayChangeTimer_Elapsed() -->> Start Function Will Started");
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In dayChangeTimer_Elapsed() -->> Day Changed, New File Is  " + FileName);
                }
            }
            dayChangeTimer.Start();
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
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Release Me");
#endif

            if (wic != null)
            {
                wic.Undo();
                wic.Dispose();
                wic = null;
            }
        }

        protected void ValidateMe()
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Validate Me");
#endif

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
                            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ValidateMe() -->> Impersonation is successful");
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
                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ValidateMe() -->> LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }

        public void LoadWatcher()
        {
            try
            {
#if HARD_LOG_MODE
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Load Watcher");
#endif
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> 1. ");
                if (fsw != null)
                {
                    fsw.Dispose();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> 2. ");
                    fsw = null;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> 3. ");
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> FileName : " + FileName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> Path.GetFullPath(FileName) : " + Path.GetFullPath(FileName));
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> Path.GetDirectoryName(Path.GetFullPath(FileName)) : " + Path.GetDirectoryName(Path.GetFullPath(FileName)));
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In LoadWatcher() -->> Path.GetFileName(FileName) : " + Path.GetFileName(FileName));

                fsw = new FileSystemWatcher(Path.GetDirectoryName(Path.GetFullPath(FileName)), Path.GetFileName(FileName));
                fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes
                                   | NotifyFilters.CreationTime | NotifyFilters.Security | NotifyFilters.Size;
                fsw.Changed += new FileSystemEventHandler(fsw_Changed);
                fsw.Deleted += new FileSystemEventHandler(fsw_Deleted);
                fsw.Disposed += new EventHandler(fsw_Disposed);
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In LoadWatcher() -->> Exception : " + e.Message);
            }
        }

        void checkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "checkTimer_Elapsed");
#endif
            checkTimer.Stop();
            disposeCheck = true;

            try
            {
                FileInfo fi = new FileInfo(FileName);
                if (fi.Length > -1)
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In checkTimer_Elapsed() -->> File Is Safe Continueing Parse.");
            }
            catch
            {
                disposeCheck = false;
                loadWatcher = true;
                checkTimer.Start();
                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In checkTimer_Elapsed() -->> Cannot Reach Destination, Will Try To Connect 1 Minute Later :" + Dir);
                return;
            }

            if (loadWatcher)
            {
                LoadWatcher();
                loadWatcher = false;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In checkTimer_Elapsed() -->> Will Be Called Parse()");
                Parse();
            }
            disposeCheck = false;
        }

        public void LoadChecker()
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Load Checker");
#endif

            usingCheckTimer = true;
        }

        void checkTimerSSH_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "checkTimerSSH_Elapsed");
#endif

            checkTimerSSH.Stop();
            Parse();
            checkTimerSSH.Start();
        }

        public void LoadSSHChecker()
        {
#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Load SSH Checker");
#endif

            if (checkTimerSSH != null)
                checkTimerSSH.Start();
        }

        protected virtual bool ReadLocal()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadLocal() -->> Entered The Function");

            List<String> lst = new List<String>();
            String line = "";

            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> Filename is " + FileName);



                if (readMethod == "sed")
                {
                    if (FileName != "" && FileName != null)
                    {
                        if (File.Exists(FileName))
                        {
                            fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> " + " FileName is not exist");
                            return false;
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> FileName is empty ");
                        return false;
                    }
                }
                else if (readMethod == "gz")
                {
                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);

#if USEZIPLIBRARYFORGZ
                            GZipInputStream zipInStream = new GZipInputStream(fileStreamIn);
#else
                    GZipStream zipInStream = new GZipStream(fileStreamIn, CompressionMode.Decompress);
#endif

                    String folder = Path.GetDirectoryName(FileName);
                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
                    Int32 size;
                    Byte[] buffer = new Byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    }
                    while (size > 0);
                    zipInStream.Close();
                    fileStreamOut.Close();
                    fileStreamIn.Close();
                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                else if (readMethod == "zip")
                {
                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
                    ZipEntry entry = zipInStream.GetNextEntry();
                    String folder = Path.GetDirectoryName(FileName);
                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
                    Int32 size;
                    Byte[] buffer = new Byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    }
                    while (size > 0);
                    zipInStream.Close();
                    fileStreamOut.Close();
                    fileStreamIn.Close();
                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }

                br = new BinaryReader(fs, enc);

                bool dontSend = false;

                if ((Position == -1) || (started && startFromEndOnLoss))
                {
                    startFromEndOnLoss = false;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> Setting Parser To Last Line");
                    ReverseLastLine(br);
                    dontSend = true;
                }

                if (Position > br.BaseStream.Length)
                    return false;

                if (!dontSend)
                    br.BaseStream.Seek(Position, SeekOrigin.Begin);
                else
                    br.BaseStream.Seek(0, SeekOrigin.Begin);

                Int32 readLineCount = 0;
                FileInfo fi = new FileInfo(FileName);
                Int64 fileLength = fi.Length;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> File Length : " + fileLength);
                StringBuilder lineSb = new StringBuilder();

                while (br.BaseStream.Position != fileLength)
                {
                    Char c = ' ';
                    #region while
                    while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                    {
                        try
                        {
                            c = br.ReadChar();
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "In Catch ReadLocal" + ex.Message);
                            long tempPosition = br.BaseStream.Position;
                            br.Close();
                            fs.Close();
                            fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            br = new BinaryReader(fs, enc);
                            br.BaseStream.Seek(tempPosition + 1, SeekOrigin.Begin);
                        }

                        #region if
                        if (Environment.NewLine.Contains(c.ToString()))
                        {
                            line = lineSb.ToString();
                            line = line.Trim();
                            line = line.Replace("\0", "");
                            if (!dontSend)
                            {
                                if (started)
                                {
                                    started = false;

                                    if (checkLineMismatch && Position != 0 && line != lastLine)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadLocal() -->> Line:" + line + "::");
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadLocal() -->> LastLine:" + lastLine + "::");
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadLocal() -->> Line Mismatch Reading File From Start");
                                        return false;
                                    }
                                    else
                                        if (checkLineMismatch && Position != 0 && line == lastLine)
                                        {
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> Find The File");
                                            dontSend = true;
                                        }
                                }

                                Position = currentPosition;
                                currentPosition = br.BaseStream.Position;
                                lastLine = line;

                                if (maxReadLineCount != -1)
                                {
                                    readLineCount++;
                                    if (readLineCount > maxReadLineCount)
                                    {
                                        if (threadSleepTime <= 0)
                                            Thread.Sleep(60000);
                                        else
                                            Thread.Sleep(threadSleepTime);
                                        readLineCount = 0;
                                    }
                                }
                            }

                            bool noerr = ParseSpecific(line, dontSend);
                            while (!noerr)
                            {
                                Thread.Sleep(threadSleepTime);
                                noerr = ParseSpecific(line, dontSend);
                            }

                            dontSend = false;

                            if (!dontSend)
                            {
                                SetRegistry();
                            }
                            else
                            {
                                if (usingKeywords && keywordsFound)
                                    break;
                            }

                            lineSb.Remove(0, lineSb.Length);

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> Looking For New Line");
                        }
                        #endregion
                        #region else
                        else
                        {
                            try
                            {
                                lineSb.Append(c);
                            }
                            catch (Exception ex)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadLocal() -->> Error While Append : " + ex.Message);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                parsing = false;
                if (br != null)
                {
                    br.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);

                if (usingCheckTimer)
                {
                    if (!checkTimer.Enabled)
                        checkTimer.Start();

                    return true;
                }
                else
                    return false;
            }

            try
            {
                if (br != null)
                    br.Close();
                if (fs != null)
                    fs.Close();

                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadLocal() -->> br and fs Is Closed And Waiting Next Timer");

                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
            }
            catch
            {
                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
            }
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadLocal() -->> Exit The Function");
            return true;
        }

        private void ReverseLastLine(BinaryReader br)
        {
#if HARD_LOG_MODE
               Log.Log(LogType.FILE, LogLevel.DEBUG, "Reverse Last Line");
#endif

            String line = "";
            Int64 TempPos = 1;
            br.BaseStream.Seek(br.BaseStream.Length - TempPos, SeekOrigin.Current);
            Int64 currentPosition = br.BaseStream.Position;
            while (br.BaseStream.Position != 0)
            {
                Char c = ' ';
                c = br.ReadChar();
                if (Environment.NewLine.Contains(c.ToString()))
                {
                    Position = currentPosition;
                    currentPosition = br.BaseStream.Position;

                    lastLine = line;
                    SetRegistry();
                    line = "";
                    break;
                }
                else
                    line = c + line;
                TempPos++;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Setting last line (line: " + line + " , last position: " + TempPos);
                br.BaseStream.Seek(br.BaseStream.Length - TempPos, SeekOrigin.Begin);
            }
        }

        protected bool ReadRemote()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Enter The Function");

#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Read Remote");
#endif

            //List<String> lst = new List<String>();
            String line = "";
            StringReader sr = null;
            Int64 currentPosition = Position;

            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Position Is : " + Position.ToString());

            try
            {
                String stdOut = "";
                String stdErr = "";

                String wcArg = "";
                String wcCmd = "";

                if (readMethod == "sed")
                {
                    wcCmd = "wc";
                    wcArg = "-l";
                }
                else if (readMethod == "nread")
                {
                    wcCmd = "wc";
                    wcArg = "-c";
                }

                String lineCommand = wcCmd + " " + wcArg + " " + FileName;

                if (readMethod == "sed")
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting Line Count With Command : " + lineCommand);
                }
                else if (readMethod == "nread")
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting File Length (In Bytes) With Command : " + lineCommand);
                }

#if STAYCONNECTED
                if (stayConnected)
                {
                    if (!se.Connected)
                        se.Connect();
                    se.RunCommand(lineCommand, ref stdOut, ref stdErr);
                }
                else
                {
                    se.ConnectTimeout(900000);
                    //se.SetTimeout(900000);
                    se.RunCommand(lineCommand, ref stdOut, ref stdErr);
                    //se.SetTimeout(Int32.MaxValue);
                    se.Close();
                }
#else
                    se.ConnectTimeout(900000);
                    //se.SetTimeout(900000);
                    se.RunCommand(lineCommand, ref stdOut, ref stdErr);
                    //se.SetTimeout(Int32.MaxValue);
                    se.Close();
#endif

                if (String.IsNullOrEmpty(stdOut))
                {
                    ParseFileName();
                    return true;
                }

                String[] arr = SpaceSplit(stdOut, false);
                Int64 tempPosition = Convert.ToInt64(arr[0]);

                if (readMethod == "sed")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Command Returned : " + tempPosition + " Lines");
                }
                else if (readMethod == "nread")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Command Returned : " + tempPosition + " Bytes");
                }

                bool dontSend = false;
                if ((Position == -1) || (started && startFromEndOnLoss))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> xxx(Position == -1) || (started && startFromEndOnLoss)");
                    startFromEndOnLoss = false;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Setting Parser To Last Line");
                    ReverseLastLineRemote(tempPosition);
                    dontSend = true;
                    return true;
                }

                bool sameFileCheck = false;

                if (Position > tempPosition)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> Position (" + Position + ") > tempposition (" + tempPosition + ") possible file change");
                    return false;
                }
                else if (Position == tempPosition && !started && tempPosition > startLineCheckCount)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> (1) Same file check Pos : " + Position + ",TPos:" + tempPosition + ",started:" + started);
                    started = true;
                    sameFileCheck = true;
                }
                else if (readMethod == "nread")
                {
                    if (lastTempPosition == tempPosition)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> (2) Same file check Pos : " + Position + ",TPos : " + tempPosition + ",started:" + started);
                        started = true;
                        sameFileCheck = true;
                    }
                }

                Int32 readLineCount = 0;

                stdOut = "";
                stdErr = "";
                if (readMethod == "sed")
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting lines from " + Position + " line to " + tempPosition + " line (" + (tempPosition - Position) + ")");
                }
                if (readMethod == "nread")
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting lines from " + Position + " Byte to " + tempPosition + " Byte (" + (tempPosition - Position) + ")");
                }
                if (started)
                {
                    if (Position != 0)
                    {
                        if (readMethod == "sed")
                            Position -= ((Position < startLineCheckCount) ? Position : startLineCheckCount);
                        /*else
                            Position++;*/
                        Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Service restarted, checking lastline from position: " + Position);
                    }
                    else
                        started = false;
                }

                Int64 readLeft = 0;
                Int64 readRight = 0;
                Int64 loopLimit = 0;
                if (readMethod == "sed")
                {
                    Int64 diff = tempPosition - Position;
                    readLeft = Position;
                    readRight = lineLimit + Position;
                    if (diff <= lineLimit)
                        readRight = Position + diff;
                    loopLimit = readRight;
                }
                else if (readMethod == "nread")
                {
                    readLeft = Position;
                    readRight = lineLimit;
                    loopLimit = Position;
                }

                #region Outside While
                while (loopLimit <= tempPosition)
                {
                    if (!started && readMethod == "nread")
                    {
                        /*if (loopLimit == tempPosition)
                            break;*/
                    }
                    stdOut = "";
                    stdErr = "";
                    String commandRead;

                    if (readMethod == "nread")
                    {
                        commandRead = tempCustomVar1 + " -n " + readLeft + "," + readRight + "p " + FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting lines from " + Position + " Byte to " + tempPosition + " Byte (" + (tempPosition - Position) + ")  with command : " + commandRead);
                    }
                    else
                    {
                        commandRead = readMethod + " -n " + readLeft + "," + readRight + "p " + FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Getting " + readRight + " lines from " + readLeft + ". lines with command : " + commandRead);
                    }

#if STAYCONNECTED
                    if (stayConnected)
                    {
                        if (!se.Connected)
                            se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    }
                    else
                    {
                        se.ConnectTimeout(900000);
                        //se.SetTimeout(Int32.MaxValue);
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();
                    }
#else               
                        se.ConnectTimeout(900000);
                        //se.SetTimeout(Int32.MaxValue);
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();
#endif
                    sr = new StringReader(stdOut);
                    /*Test*/


                    if (!started && readMethod == "nread")
                    {

                        StringReader srTest = new StringReader(stdOut);
                        Int64 posTest = Position;
                        String lineTest = "";

#if NREADSELFCHECK      
                            MemoryStream mstream = new MemoryStream(Encoding.Default.GetBytes(srTest.ReadToEnd()));
                            BinaryReader br = new BinaryReader(mstream, Encoding.Default);
                                                        
                            StringBuilder lineSb = new StringBuilder();
                            while (br.BaseStream.Position != br.BaseStream.Length)
                            {
                               Char c = ' ';
                               c = br.ReadChar();
                               if (Environment.NewLine.Contains(c.ToString()))
                               {
                                  lineTest = lineSb.ToString();
                                  if (lineTest != "")
                                  {
                                       posTest = Position + br.BaseStream.Position;
                                  }
                                  lineSb.Remove(0, lineSb.Length);
                                  }
                                  else
                                  lineSb.Append(c);
                                }
                                br.Close();
                                mstream.Close();                                          
#else

                        while ((lineTest = srTest.ReadLine()) != null)
                        {
                            if (lineTest.StartsWith("~?`Position"))
                            {
                                try
                                {
                                    String[] arrIn = lineTest.Split('\t');
                                    String[] arrPos = arrIn[0].Split(':');
                                    String[] arrLast = arrIn[1].Split('`');
                                    posTest = Convert.ToInt64(arrPos[1]); // deðiþti Convert.ToUInt32s
                                }
                                catch (Exception ex)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> In Try Catch 2 " + ex.Message);
                                }
                            }
                        }
#endif
                        if (posTest == Position)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> posTest is equal to Position");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> posTest and Position Is :" + posTest.ToString() + " , " + Position.ToString());
                            //break; deðiþti fatih
                            ParseFileName();
                        }
                    }

                    bool inLoop = false;
                    bool controlendoffile = false;
                    #region Ýç While
                    while ((line = sr.ReadLine()) != null)
                    {
                        inLoop = true;

                        if (line == "" || line == "\n")
                        {
                            currentPosition++;
                            continue;
                        }
                        if (started)
                        {
                            if (Position != 0)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Checking Last Line");
                                if (readMethod == "sed")
                                {
                                    Int64 limit = readRight - readLeft;
                                    for (Int64 i = 0; i <= limit; i++)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Checking lines (Place: " + i + "):");
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "    Parser In ReadRemote() -->> Line      : " + line);
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "    Parser In ReadRemote() -->> Last Line : " + lastLine);
                                        if (line == lastLine)
                                        {
                                            currentPosition = currentPosition + i - startLineCheckCount;
                                            started = false;
                                            if (sameFileCheck)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> File Integrity check ok, found line match, position: " + currentPosition);
                                                Position = currentPosition;
                                                return true;
                                            }
                                            else
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Found line match, parse continues from position: " + currentPosition);
                                                if (currentPosition == readRight)
                                                {
                                                    Position = currentPosition;
                                                    return true;
                                                }
                                            }
                                            break;
                                        }
                                        line = sr.ReadLine();
                                        if (line == null)
                                        {
                                            if (tempPosition == readRight)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Reached to end of file at restart.");
                                                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> LastLine:" + lastLine);
                                                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> Line mismatch reading file from start");
                                                started = false;
                                                return false;
                                            }
                                            else
                                            {
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Reached to end of selection at restart.");
                                                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> Line mismatch reading file from start.");
                                                started = false;
                                                return false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (line.StartsWith("~?`Position"))
                                    {
                                        String[] arrIn = line.Split('\t');
                                        String[] arrPos = arrIn[0].Split(':');
                                        String[] arrLast = arrIn[1].Split('`');
                                        loopLimit = Convert.ToInt64(arrPos[1]);
                                        Position = loopLimit;
                                        line = arrLast[1];
                                    }
                                    if (line == lastLine)
                                    {
                                        started = false;
                                        sameFileCheck = false;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Found line match, parse continues from position: " + Position);
                                        //return true; kapatýldý....
                                        dontSend = true;
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Reached to end of file at restart.");
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> LastLine:" + lastLine);
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ReadRemote() -->> Line mismatch reading file from start");
                                        started = false;
                                        return false;
                                    }
                                }
                            }
                        }

                        if (readMethod == "sed")
                        {
                            Position = currentPosition;
                            currentPosition++;
                            lastLine = line;
                        }

                        if (readMethod == "nread")
                        {
                            if (line == lastLine)
                                continue;
                            if (line.StartsWith("~?`Position"))
                            {
                                String[] arrIn = line.Split('\t');
                                String[] arrPos = arrIn[0].Split(':');
                                String[] arrLast = arrIn[1].Split('`');
                                loopLimit = Convert.ToInt64(arrPos[1]);
                                Position = loopLimit;
                                lastLine = arrLast[1];
                                break;
                            }
                        }

                        if (maxReadLineCount != -1)
                        {
                            readLineCount++;
                            if (readLineCount > maxReadLineCount)
                            {
                                if (threadSleepTime <= 0)
                                    Thread.Sleep(60000);
                                else
                                    Thread.Sleep(threadSleepTime);
                                readLineCount = 0;
                            }
                        }

                        bool noerr = true;
                        noerr = ParseSpecific(line, dontSend);
                        dontSend = false;

                        while (!noerr)
                        {
                            Thread.Sleep(threadSleepTime);
                            noerr = ParseSpecific(line, dontSend);
                            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> In While noerr Is False");
                        }

                        if (!dontSend)
                        {
                            SetRegistry();
                        }
                        else
                        {
                            if (usingKeywords && keywordsFound)
                                break;
                        }
                        line = "";
                    }

                    if (controlendoffile)
                    {
                        ParseFileName();
                        return true;
                    }
                    #endregion
                    if (!inLoop)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Error String: " + stdErr);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> Stream returned null string returning");
                        if (Position == 0 /*&& stdErr.StartsWith("sed: -e expression #1,")*/ && readMethod == "sed")
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReadRemote() -->> " + "\"sed\" problem with position 0 (possibly you are using ubuntu), starting from position 1 on next check");
                            Position = 1;
                        }
                        return true;
                    }
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Finished Reading Lines");
                    if (readMethod == "sed")
                        Position = --currentPosition;
                    SetRegistry();
                    //Store lastTempPosition to check that file changed
                    lastTempPosition = tempPosition;
                    if (readMethod == "sed")
                    {
                        readLeft = readRight;
                        Int64 checkDiff = tempPosition - readRight;
                        if (checkDiff == 0)
                            break;
                        readRight = readRight + ((checkDiff >= lineLimit) ? lineLimit : checkDiff);
                    }
                    else
                        readLeft = loopLimit;
                }
                #endregion
            }
            catch (Exception e)
            {
                if (sr != null)
                    sr.Close();
#if STAYCONNECTED
                if (!stayConnected)
                {
                    if (se.Connected)
                        se.Close();
                }
#else
                    if (se.Connected)
                        se.Close();
#endif
                Log.Log(LogType.FILE, LogLevel.ERROR, "  " + e.Message + "\n" + e.StackTrace);
                return true;
            }
            try
            {
                if (sr != null)
                    sr.Close();
#if STAYCONNECTED
                if (!stayConnected)
                {
                    if (se.Connected)
                        se.Close();
                }
#else
                    if (se.Connected)
                        se.Close();
#endif
            }
            catch
            {
            }
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReadRemote() -->> Exit The Function");
            return true;
        }

        private void ReverseLastLineRemote(Int64 lastLineNumber)
        {
#if HARD_LOG_MODE
                 Log.Log(LogType.FILE, LogLevel.DEBUG, "Reverse Last Line Remote");
#endif

            String stdOut = "";
            String stdErr = "";
            String lastLineCommand = "tail -n 1 " + FileName;
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ReverseLastLineRemote() -->> Getting Last Line With Command: " + lastLineCommand);

#if STAYCONNECTED
            if (stayConnected)
            {
                if (!se.Connected)
                    se.Connect();
                se.RunCommand(lastLineCommand, ref stdOut, ref stdErr);
            }
            else
            {
                se.ConnectTimeout(900000);
                //se.SetTimeout(Int32.MaxValue);
                se.RunCommand(lastLineCommand, ref stdOut, ref stdErr);
                se.Close();
            }
#else
                se.ConnectTimeout(900000);
                //se.SetTimeout(Int32.MaxValue);
                se.RunCommand(lastLineCommand, ref stdOut, ref stdErr);
                se.Close();
#endif
            StringReader sr = new StringReader(stdOut);
            String line = sr.ReadLine();
            if (line != null)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ReverseLastLineRemote() -->> Setting Last Line (Line: " + line + " , Last Position: " + lastLineNumber);
                lastLine = line;
                Position = lastLineNumber;
                SetRegistry();
            }
        }

        public virtual void Parse()
        {
            if (!parseLock.WaitOne(0))
            {
                parseLock.WaitOne();
                try
                {
                    throw new Exception("[!!! WARNING: SAFE MESSAGE !!!] : Parse already been processed by another thread while this call has made");
                }
                finally
                {
                    parseLock.ReleaseMutex();
                }
            }
            try
            {
                parsing = true;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In Parse() -->> Entered The Function");

                if (remoteHost != "")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In Parse() -->> Remote Host is : " + remoteHost);

                    checkTimerSSH.Stop();
                    if (!ReadRemote())
                    {
                        Position = 0;
                        lastLine = "";
                        ReadRemote();
                    }
                    checkTimerSSH.Start();
                }
                else
                {
                    if (!ReadLocal())
                    {
                        Position = 0;
                        lastLine = "";
                        ReadLocal();
                    }
                }
            }
            finally
            {
                parsing = false;
                parseLock.ReleaseMutex();
            }
        }

        public override void Init()
        {
        }

        public override void Start()
        {
            try
            {
                if (fsw != null)
                    fsw.EnableRaisingEvents = true;

                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In Start() -->> Parse Will Start");

                Parse();
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In Start() -->> Exception: " + e.Message);
            }
        }

        public override void Clear()
        {
            Stop();
            if (checkTimer != null)
            {
                checkTimer.Stop();
                //checkTimer.Dispose();
            }
            if (dayChangeTimer != null)
            {
                dayChangeTimer.Stop();
                //dayChangeTimer.Dispose();
            }
            if (checkTimerSSH != null)
            {
                checkTimerSSH.Stop();
                //checkTimerSSH.Dispose();
            }
            disposeCheck = true;
            if (remoteHost != "")
                se.Close();
            //fsw.Dispose();
            //parseLock.ReleaseMutex();
            //parseLock.Close();
        }

        public virtual void Stop()
        {
            if (fsw != null)
                fsw.EnableRaisingEvents = false;
        }

        public virtual String GetLocation()
        {
            if (usingRegistry)
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\Security Manager\\Recorder\\" + LogName, true);
                return (String)reg.GetValue("Location");
            }
            else
                return Dir;
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
                if (!string.IsNullOrEmpty(lastLine))
                {
                    s.SetReg(Id, Position.ToString(), lastLine, lastFile, lastKeywords, LastRecDate);
                }
            }
        }

        protected void GetRegistry()
        {
            if (usingRegistry)
            {

                if (reg == null)
                {
                    reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\" + LogName, true);
                }

                logLevel = (LogLevel)Convert.ToInt32(reg.GetValue("Trace Level"));
                Log.SetLogLevel(logLevel);
                Position = Convert.ToInt32(reg.GetValue("LastPosition"));
                lastLine = (String)reg.GetValue("LastLine");
                lastFile = (String)reg.GetValue("LastFile");
                startFromEndOnLoss = Convert.ToBoolean(Convert.ToInt32(reg.GetValue("FromEndOnLoss")));
                threadSleepTime = Convert.ToInt32(reg.GetValue("SleepTime"));
                maxReadLineCount = Convert.ToInt32(reg.GetValue("MaxLineToWait"));

                if (maxReadLineCount <= 0)
                    lineLimit = 100;
                else
                    lineLimit = maxReadLineCount;

                RegistryKey regIn = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Agent");

                try
                {
                    home = (String)regIn.GetValue("Home Directory");
                }
                catch
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  Cannot Read Agent Home directory, logging is disabled");
                    Log.SetLogLevel(LogLevel.NONE);
                    return;
                }
            }
            else
            {
                RegistryKey regIn = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Remote Recorder");
                try
                {
                    home = (String)regIn.GetValue("Home Directory");
                }
                catch
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  Cannot Read Agent Home directory, logging is disabled");
                    Log.SetLogLevel(LogLevel.NONE);
                    return;
                }
            }

            Log.SetLogFile(home + "log\\" + LogName + Id.ToString() + ".log");
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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SetRecordData -->> LastRecDate: " + LastRecDate);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SetRecordData -->> SetData, Start");
                    s.SetData(Dal, Virtualhost, r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SetRecordData -->> SetData, End");
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetRecordData -->> An Error occurred in SetRecordData. " + exception.Message);
                }
            }
        } // SetRecordData

        protected void ParseFileName()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ParseFileName() -->> Enter The Function");

#if HARD_LOG_MODE
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Parse Filename");
#endif

            if (usingRegistry)
            {
                user = (String)reg.GetValue("User");
                password = (String)reg.GetValue("Password");
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);
                remoteHost = (String)reg.GetValue("Remote Host");
            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ParseFileName() -->> User is : " + user + " Password is : ***** " + " Remote Host Is : " + remoteHost);

            if (remoteHost != "")
            {
                LoadSSHChecker();
#if HARD_DEBUG_MODE
                    try
                    {             
#endif
                ParseFileNameRemote();
#if STAYCONNECTED
                if (stayConnected)
                {
                    if (!se.Connected)
                        se.Connect();
                }
#endif

#if HARD_DEBUG_MODE
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting remote files, Exception: " + e.Message);
                    }
#endif
            }
            else
            {
                ValidateMe();
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ParseFileName() -->> Continiou After ValidateMe()");
#if HARD_DEBUG_MODE
                try
                {
#endif

                ParseFileNameLocal();
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ParseFileName() -->> Continiou After ParseFileNameLocal() In Derived Class");

#if HARD_DEBUG_MODE
                }
                catch(Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting local files, Exception: " + e.Message);
                }         
#endif
                bool control = true;

                try
                {
                    LoadWatcher();
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ParseFileName() -->> Cannot Load Watcher!");
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  " + e.Message);
                    control = false;
                }

                if (control)
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ParseFileName() -->> Loaded Watcher");


                if (Dir.StartsWith("\\\\"))
                    LoadChecker();
            }

            if (FileName == null)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ParseFileName() -->> Cannot Find File To Parse, Please Check Your Path!");
                Log.Log(LogType.FILE, LogLevel.WARN, "   Parser In ParseFileName() -->> DayWatcher Will Try To Find New File Every Second.");
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ParseFileName() -->> File Name Found : " + FileName);
            }

            if (dayChangeTimer == null)
            {
                dayChangeTimer = new System.Timers.Timer(60000);
                dayChangeTimer.Elapsed += new System.Timers.ElapsedEventHandler(dayChangeTimer_Elapsed);
                dayChangeTimer.Start();
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  Parser In ParseFileName() -->> Day Change Timer Is Started in Parser");
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " Parser In ParseFileName() -->> Exit The Function");
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
            lastLine = LastLine;
            lastFile = LastFile;
            Position = Convert.ToInt64(LastPosition);
            startFromEndOnLoss = FromEndOnLoss;
            maxReadLineCount = MaxLineToWait;
            if (maxReadLineCount <= 0)
                lineLimit = 100;
            else
                lineLimit = maxReadLineCount;
            threadSleepTime = SleepTime;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
            Log.SetLogLevel((LogLevel)TraceLevel);

            tempCustomVar1 = CustomVar1;

            if (CustomVar1.Contains("nread"))
                readMethod = "nread";
            else
                readMethod = "sed";

#if STAYCONNECTED
            stayConnected = Convert.ToBoolean(CustomVar2);
#endif

            Virtualhost = virtualhost;
            Dal = dal;
            zone = Zone;
        }

        //Base functions
        public virtual bool ParseSpecific(String line, bool dontSend)
        {
            return true;
        }

        public virtual bool ParseSpecific(EventLogReader logReader, EventRecord eventRecord, bool dontSend, string currentFile)
        {
            return true;
        }
        //
        public virtual void GetFiles()
        {
            throw new Exception("You havent implemented GetFiles() method, please implement it. If you added this please remove \"base.GetFiles();\"");
        }

        protected virtual void ParseFileNameRemote()
        {
            throw new Exception("You havent implemented ParseFileNameRemote() method, please implement it. If you added this please remove \"ParseFileNameRemote();\"");
        }

        protected virtual void ParseFileNameLocal()
        {
            throw new Exception("You havent implemented ParseFileNameLocal() method, please implement it. If you added this please remove \"base.ParseFileNameLocal();\"");
        }

        //CustomBaseFunctions
        public override void SetData(Rec obj)
        {
            base.SetData(obj);
        }

        public override void SetReg(Int32 Identity, String LastPosition, String LastLine, String LastFile, String LastKeywords, String LastRecDate)
        {
            base.SetReg(Identity, LastPosition, LastLine, lastFile, lastKeywords, LastRecDate);
        }

        //Utils
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
        }

        public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreChar)
                    ignore = !ignore;
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
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreCharStart, Char ignoreCharEnd)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreCharStart)
                    ignore = true;
                else if (c == ignoreCharEnd)
                    ignore = false;
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
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar, Char seperator, bool dummy)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreChar)
                    ignore = !ignore;
                if (c != seperator && (!useTabs || c != '\t'))
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
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreChar, bool includeEmpty, Char seperator)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            Char lastChar = '`';
            foreach (Char c in line.ToCharArray())
            {
                lastChar = c;
                if (c == ignoreChar)
                    ignore = !ignore;
                if (c != seperator && (!useTabs || c != '\t'))
                {
                    if (space)
                    {
                        if (includeEmpty || sb.ToString() != "")
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
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
                else if (includeEmpty && space)
                {
                    lst.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                }
            }

            if (includeEmpty || sb.ToString() != "")
                lst.Add(sb.ToString());
            if (includeEmpty && lastChar == seperator)
                lst.Add("");

            return lst.ToArray();
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                SetRegistry();
                reg.Close();
                if (se != null)
                    se.Close();
                if (wic != null)
                {
                    wic.Undo();
                    wic.Dispose();
                }
                if (wi != null)
                {
                    wi.Dispose();
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
