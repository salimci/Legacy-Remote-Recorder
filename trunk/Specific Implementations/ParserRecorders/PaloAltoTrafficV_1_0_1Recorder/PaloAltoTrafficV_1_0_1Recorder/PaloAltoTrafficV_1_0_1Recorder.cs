using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using Natek.Helpers.GenericComparator;

namespace PaloAltoTrafficV_1_0_1Recorder
{
    public class PaloAltoTrafficV_1_0_1Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, lastFile = "", user = "", password = "";
        private bool reg_flag;
        protected bool usingRegistry = true, fromend;
        protected Int32 Id;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        readonly object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        private const String LogName = "PaloAltoTrafficV_1_0_1Recorder";
        public string _lastKeywords = "";
        private int maxRecordSend;
        private string remoteHost;

        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public PaloAltoTrafficV_1_0_1Recorder()
        {
            enc = Encoding.UTF8;
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on PaloAltoTrafficV_1_0_1Recorder functions may not be running");
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on PaloAltoTrafficV_1_0_1Recorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating PaloAltoTrafficV_1_0_1Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager PaloAltoTrafficV_1_0_1Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  PaloAltoTrafficV_1_0_1Recorder Init Method end.");
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
            last_recordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            lastFile = LastFile;
            _lastKeywords = LastKeywords;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
        }

        private long Convert_To_Int64(string value)
        {
            long result;
            if (Int64.TryParse(value, out result))
                return result;
            return 0;
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\PaloAltoTrafficV_1_0_1Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager PaloAltoTrafficV_1_0_1Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager PaloAltoTrafficV_1_0_1Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        } // Get_logDir

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    RegistryKey registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Recorder");
                    if (registryKey != null)
                    {
                        var subKey = registryKey.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (subKey != null)
                            log_size = Convert.ToUInt32(subKey.GetValue("Log Size"));
                    }
                    var key = rk.OpenSubKey("Recorder");
                    if (key != null)
                    {
                        var openSubKey1 = key.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (openSubKey1 != null)
                            logging_interval = Convert.ToUInt32(openSubKey1.GetValue("Logging Interval"));
                    }
                    var registryKey1 = rk.OpenSubKey("Recorder");
                    if (registryKey1 != null)
                    {
                        var subKey1 = registryKey1.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (subKey1 != null)
                            trc_level = Convert.ToInt32(subKey1.GetValue("Trace Level"));
                    }
                    var key1 = rk.OpenSubKey("Agent");
                    if (key1 != null)
                        err_log = key1.GetValue("Home Directory") + @"log\PaloAltoTrafficV_1_0_1Recorder.log";
                    var openSubKey2 = rk.OpenSubKey("Recorder");
                    if (openSubKey2 != null)
                    {
                        var registryKey2 = openSubKey2.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (registryKey2 != null)
                            timer1.Interval = Convert.ToInt32(registryKey2.GetValue("Interval"));
                    }
                    var subKey2 = rk.OpenSubKey("Recorder");
                    if (subKey2 != null)
                    {
                        var key2 = subKey2.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (key2 != null)
                            maxRecordSend = Convert.ToInt32(key2.GetValue("MaxRecordSend"));
                    }
                    var openSubKey3 = rk.OpenSubKey("Recorder");
                    if (openSubKey3 != null)
                    {
                        var registryKey3 = openSubKey3.OpenSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        if (registryKey3 != null)
                            last_recordnum = Convert.ToInt64(registryKey3.GetValue("LastRecordNum"));
                    }

                    rk.Close();
                }
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
        } // Read_Registry

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
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is Started");

                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " PaloAltoTrafficV_1_0_1Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In timer1_Tick() --> lastFile: " + lastFile);
                    ParseFileNameLocal();
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception);
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick

        protected void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED ");
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Position: " + last_recordnum);

                    if (location.EndsWith("/") || location.EndsWith("\\"))
                    {
                        ValidateMe();
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseFileNameLocal() -->> Searching files in directory : " + location);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList = SortFileNames(fileNameList);
                        //
                        if (fileNameList.Count == 0)
                        {
                            return;
                        }

                        foreach (string t in fileNameList)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  " ParseFileNameLocal() -->> Sorting Files. " + t);
                        }

                        //L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED " + lastFile);

                        if (string.IsNullOrEmpty(lastFile))
                        {
                            lastFile = fileNameList[0];
                            last_recordnum = 0;
                            LastRecordDate = DateTime.Now.ToString(dateFormat);
                            if (!SetNewFile())
                            {
                                return;
                            }
                            SendLine(lastFile);
                        }
                        else
                        {
                            if (fileNameList.Contains(lastFile))
                            {
                                SendLine(lastFile);
                                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> lastFile: " + lastFile);
                            }
                            else
                            {
                                try
                                {
                                    bool foundFile = false;
                                    foreach (var v in fileNameList.Where(v => String.CompareOrdinal(lastFile, v) < 0))
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
                                    if (foundFile)
                                    {
                                        SendLine(lastFile);
                                    }
                                    else
                                    {
                                        lastFile = "";
                                        last_recordnum = 0;
                                        SetNewFile();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> FileName fount exception: " + exception.Message);
                                }
                            }
                        }

                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR,
                          " ParseFileNameLocal() -->> An error occurred : " + ex);
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }// ParseFileNameLocal

        protected void ValidateMe()
        {

            if (user != "")
            {
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    var userName = user;
                    var domain = "";
                    if (user.Contains("\\"))
                    {
                        var arr = user.Split('\\');
                        userName = arr[1];
                        domain = arr[0];
                    }

                    bool ret = LogonUser(userName, domain, password, (Int32)Parser.Parser.LogonType.LOGON32_LOGON_NEW_CREDENTIALS,
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
                                CloseHandle(token);
                                CloseHandle(tokenDuplicate);
                                return;
                            }
                        }
                    }
                    else
                    {
                        var error = Marshal.GetLastWin32Error();
                        L.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ValidateMe() -->> LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        } // ValidateMe

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "PaloAltoTrafficV_1_0_1Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        public bool SendLine(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "SendLine is Started. FileName" + fileName);

            try
            {
                var stringBuilder = new StringBuilder();
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //reader = new BinaryReader(fileStream, Encoding.UTF8);
                var streamReader = new StreamReader(fileStream);

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Info: " + fileName + " - " + fileStream.Length + " - " + last_recordnum);

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Path _ if : " + fileName + " - " + fileStream.Length + " - " + last_recordnum);

                streamReader.BaseStream.Seek(last_recordnum >= 0 ? last_recordnum : 0, SeekOrigin.Begin);

                int r;
                while ((r = streamReader.Read()) >= 0)
                {
                    var ch = (char)r;
                    if (ch == '\n' || ch == '\r')
                    {
                        last_recordnum = streamReader.BaseStream.Position;
                        if (stringBuilder.Length > 0)
                        {
                            string line = stringBuilder.ToString();
                            CoderParse(line, fileName);
                        }
                        stringBuilder.Remove(0, stringBuilder.Length);
                    }
                    else
                    {
                        stringBuilder.Append(ch);
                    }
                }
                return SetLastFile(fileName);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " SendLine() -->> An error occurred : " + exception);
                return false;
            }
        } // SendLine

        public bool SetLastFile(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile is started with " + fileName);

            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
            try
            {
                List<String> fileNameList = GetFileNamesOnLocal();
                fileNameList = SortFileNames(fileNameList);
                foreach (string t in fileNameList)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          " SetLastFile() -->> Sorting Files. " + t);
                }
                int idx = fileNameList.IndexOf(lastFile);


                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "PaloAltoTrafficV_1_0_1Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " PaloAltoTrafficV_1_0_1Recorder In SetLastFile() -->> Record sending Error." +
                      exception.Message);
                return false;
            }
        } // SetLastFile

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                var fileNameList = new List<String>();
                foreach (var fileName in Directory.GetFiles(location))
                {
                    String fullFileName = Path.GetFullPath(fileName);
                    fileNameList.Add(fullFileName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> fullFileName: " + fullFileName);

                }
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex);
                return null;
            }
        } // GetFileNamesOnLocal

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a, StringComparison.Ordinal);
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

        private List<String> SortFileNames(List<String> fileNameList)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");

            var _fileNameList = new List<string>();

            var fileNameRegEx = new Regex("^.*_([0-9]+_[0-9]+_[0-9]+)[^0-9]+_([0-9]+)\\.csv$");

            foreach (var t in fileNameList)
            {
                if (fileNameRegEx.IsMatch(new FileInfo(t).Name))
                {
                    _fileNameList.Add(t);
                }
            }

            var comparator = new GenericComparator()
            {
                ComparisonType = StringComparison.InvariantCultureIgnoreCase,
                IsNullAndEmptyEqual = true,
                Pattern = new Regex("^.*_([0-9]+_[0-9]+_[0-9]+)[^0-9]+_([0-9]+)\\.csv$"),
                PatternFieldComperators = new GenericComparator.Comparer<string>[]
                        {
                            (date1, date2) =>
                            DateTime.ParseExact(date1, "yyyy_M_d", CultureInfo.InvariantCulture)
                                    .CompareTo(DateTime.ParseExact(date2, "yyyy_M_d", CultureInfo.InvariantCulture)),
                            (order1,order2)=>long.Parse(order1).CompareTo(long.Parse(order2))
                        }
            };
            _fileNameList.Sort(comparator);

            foreach (string t in _fileNameList)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a, int type)
        {
            //type = 0 first
            //type = 1 last
            int posA = 0;

            if (type == 1)
            {
                posA = value.IndexOf(a, StringComparison.Ordinal);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a, StringComparison.Ordinal);
            }

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
        }

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a, StringComparison.Ordinal);
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
            int posA = value.IndexOf(a, StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, StringComparison.Ordinal);

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

        public bool CoderParse(string line, String fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In CoderParse() -->> Started. " + line + " - " + fileName);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "CoderParse | Line : " + line);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "CoderParse | Log error : " + exception);
            }

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            var rec = new Rec();
            try
            {
                try
                {
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() | Parsing Specific line. Line : " + line);
                    if (string.IsNullOrEmpty(line))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() | Line is Null Or Empty. ");
                        return true;
                    }

                    rec.Description = line;
                    rec.LogName = LogName;
                    if (!string.IsNullOrEmpty(remoteHost))
                        rec.ComputerName = remoteHost;

                    //threath     Domain*,Receive Time*,Serial #*,Type*,Threat/Content Type*,Config Version*,Generate Time*,Source address*,Destination address*,NAT Source IP*,NAT Destination IP*,Rule*,Source User*,Destination User*,Application*,Virtual System*,Source Zone*,Destination Zone*,Inbound Interface*, Outbound Interface*, Log Action*,Time Logged*,Session ID*,Repeat Count*,Source Port*,Destination Port*,NAT Source Port*,NAT Destination Port*,Flags*,IP Protocol*,Action,URL,Threat/Content Name,Category,Severity,Direction
                    //traffic     Domain*,Receive Time*,Serial #*,Type*,Threat/Content Type*,Config Version*,Generate Time*,Source address*,Destination address*,NAT Source IP*,       NAT Destination IP*,Rule*,Source User*,Destination User*,Application*,Virtual System*,Source Zone*,Destination Zone*,Inbound Interface*,Outbound Interface*,          Log Action*,Time Logged*,Session ID*,Repeat Count*,Source Port*,Destination Port*,NAT Source Port*,NAT Destination Port*,Flags*,IP Protocol*,      Action,Bytes,Bytes Sent,Bytes Received,Packets,Start Time,Elapsed Time (sec),Category,Padding(39)
                    //1,2011/01/25 05:45:17,0004C100832,THREAT,vulnerability,2,2011/01/25 05:45:12,193.189.142.32,168.216.29.89,192.168.0.12,168.216.29.89,Dis_Web_Server_erisim,,,web-browsing,vsys1,DMZ,Internet,ethernet1/1,ethernet1/4,,2011/01/25 05:45:17,56500,1,80,4149,80,4149,0x40,tcp,alert,,HTTP Non RFC-Compliant Response Found(32880),any,informational,server-to-client

                    var parts = line.Split(',');
                    //
                    try
                    {
                        try
                        {
                            rec.CustomInt1 = Convert_to_Int32(parts[0]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt1: " + exception.Message);
                        }
                        
                        try
                        {
                            rec.Datetime = Convert.ToDateTime(parts[6]).ToString("yyyy-MM-dd HH:mm:ss");//Date time conversion requeired.
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() | DateTime Convert Error. " + ex.Message);
                            L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() | DateTime Convert Error. " + parts[4]);
                            return true;
                        }

                        if (parts.Length > 7)
                            rec.CustomStr3 = parts[7];
                        if (parts.Length > 8)
                            rec.CustomStr4 = parts[8];
                        if (parts.Length > 9)
                            rec.CustomStr5 = parts[9];
                        if (parts.Length > 10)
                            rec.CustomStr6 = parts[10];
                        if (parts.Length > 11)
                            rec.CustomStr9 = parts[11];
                        if (parts.Length > 12)
                            rec.UserName = parts[12];
                        if (parts.Length > 14)
                            rec.CustomStr10 = parts[14];
                        if (parts.Length > 18)
                            rec.CustomStr1 = parts[18];
                        if (parts.Length > 19)
                            rec.CustomStr2 = parts[19];
                        if (parts.Length > 29)
                            rec.CustomStr7 = parts[29];
                        if (parts.Length > 30)
                            rec.EventType = parts[30];
                        if (parts.Length > 37)
                            rec.EventCategory = parts[37];


                        try
                        {
                            if (parts.Length > 23)
                                rec.CustomInt2 = Convert_to_Int32(parts[23]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt2: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 24)
                                rec.CustomInt3 = Convert_to_Int32(parts[24]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt3: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 25)
                                rec.CustomInt4 = Convert_to_Int32(parts[25]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt4: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 26)
                                rec.CustomInt5 = Convert_to_Int32(parts[26]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt5: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 27)
                                rec.CustomInt6 = Convert_to_Int32(parts[27]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt6: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 22)
                                rec.CustomInt7 = Convert_to_Int32(parts[22]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt7: " + exception.Message);
                        }

                        try
                        {

                            if (parts.Length > 32)
                                rec.CustomInt9 = Convert_to_Int32(parts[32]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt9: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 33)
                                rec.CustomInt9 = Convert_to_Int32(parts[33]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt9: " + exception.Message);
                        }

                        try
                        {
                            if (parts.Length > 36)
                                rec.CustomInt10 = Convert_to_Int32(parts[36]);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Type Casting Error CustomInt10: " + exception.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse()  " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse()  " + ex.StackTrace);
                        L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse()  Line : " + line);
                        return true;
                    }
                    SendRecord(rec, line);
                    return true;
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ikinci kısım | " + ex.Message);
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | Line : " + line);
                L.Log(LogType.FILE, LogLevel.ERROR, " PaloAltoTrafficV_1_0_1Recorder In CoderParse() Error.");
                //WriteMessage("CoderParse | Parsing Error : " + e.ToString());
                //WriteMessage("CoderParse | line: " + line);
                return false;
            }
            return true;
        } // CoderParse

        private int Convert_to_Int32(string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " PaloAltoTrafficV_1_0_1Recorder In Convert_to_Int32() -->> Value cannot convert int32. " +
                      ex.StackTrace);

                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " PaloAltoTrafficV_1_0_1Recorder In Convert_to_Int32() -->> Value is: " + value);

                return 0;
            }
        }

        public bool SendRecord(Rec r, string line)
        {
            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In CoderParse() -->> Record sending." + last_recordnum + " - " + lastFile);
                customServiceBase.SetData(Dal, virtualhost, r);
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, lastFile, " ", LastRecordDate);
                L.Log(LogType.FILE, LogLevel.DEBUG, " PaloAltoTrafficV_1_0_1Recorder In CoderParse() -->> Record sended.");
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " PaloAltoTrafficV_1_0_1Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                return false;
            }
        }

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
            var lst = new List<String>();
            var sb = new StringBuilder();
            bool space = false;
            foreach (var c in line)
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
                var registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE");
                if (registryKey != null)
                {
                    var subKey = registryKey.CreateSubKey("Natek");
                    if (subKey != null)
                    {
                        var key = subKey.CreateSubKey("Security Manager");
                        if (key != null)
                        {
                            var registryKey1 = key.CreateSubKey("Recorder");
                            if (registryKey1 != null)
                                rk = registryKey1.CreateSubKey("PaloAltoTrafficV_1_0_1Recorder");
                        }
                    }
                }
                if (rk != null)
                {
                    rk.SetValue("LastRecordNum", status);
                    rk.Close();
                }
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
                    rk.Close();
            }
        } // Set_Registry

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
                EventLog.WriteEntry("Security Manager PaloAltoTraffic Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // Initialize_Logger
    }
}
