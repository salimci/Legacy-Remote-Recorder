using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;

namespace IISV_7_1_1Recorder
{
    public struct Fields
    {
        public long CurrentPosition;
        public long RowCount;
    }

    public class IISV_7_1_1Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000;
        private long last_recordnum;
        private uint logging_interval = 60000;
        private string err_log, location, lastFile = "", user = "", password = "";
        private int maxRecordSend;
        private uint logSize = 1000000;
        private bool reg_flag;
        protected bool usingRegistry = true, fromend;
        protected Int32 Id;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        readonly object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        Dictionary<String, Int32> dictHash;
        private const String LogName = "IISV_7_1_1Recorder";
        public string lastKeywords = "";
        private string machineName;
        private static string iisType = "";
        private Encoding encoding = Encoding.UTF8;

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

        public IISV_7_1_1Recorder()
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IISV_7_1_1Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IISV_7_1_1Recorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating IISV_7_1_1Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager IISV_7_1_1Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  IISV_7_1_1Recorder Init Method end.");
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
            timer_interval = SleepTime;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition);
            Dal = dal;
            lastFile = LastFile;
            lastKeywords = LastKeywords;
            user = User;
            password = Password;
            machineName = Location;

            if (!string.IsNullOrEmpty(CustomVar1))
            {
                var customArr = CustomVar1.Split(';');
                if (customArr.Length == 2)
                {
                    foreach (var s in customArr)
                    {
                        if (s.StartsWith("T="))
                        {
                            iisType = s.Split('=')[1];
                        }
                        else if (s.StartsWith("E="))
                        {
                            encoding = Encoding.GetEncoding(s.Split('=')[1]);
                        }
                    }
                }
                else
                {
                    if (CustomVar1.StartsWith("T="))
                    {
                        iisType = CustomVar1.Split('=')[1];
                    }
                    else if (CustomVar1.StartsWith("E="))
                    {
                        encoding = Encoding.GetEncoding(CustomVar1.Split('=')[1]);
                    }
                }
            }
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\IISV_7_1_1Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IISV_7_1_1Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager IISV_7_1_1Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
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
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }

                logSize = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\IISV_7_1_1Recorder.log";
                timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("Interval"));
                maxRecordSend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("IISV_7_1_1Recorder").GetValue("LastRecordNum"));

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
                    GC.SuppressFinalize(rk);
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
                            " IISV_7_1_1Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In timer1_Tick() --> lastFile: " + lastFile);
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

                        if (fileNameList.Count == 0)
                        {
                            return;
                        }

                        foreach (string t in fileNameList)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  " ParseFileNameLocal() -->> Sorting Files. " + t);
                        }

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
                                    foreach (var v in fileNameList)
                                    {
                                        if (String.CompareOrdinal(lastFile, v) < 0)
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
                    String userName = user;
                    String domain = "";
                    if (user.Contains("\\"))
                    {
                        String[] arr = user.Split('\\');
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

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "IISV_7_1_1Recorder SetLastFile update error:" + exception.Message);
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
                var streamReader = new StreamReader(fileStream, encoding);

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

            CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
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
                      " IISV_7_1_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "IISV_7_1_1Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " IISV_7_1_1Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " IISV_7_1_1Recorder In SetLastFile() -->> Record sending Error." +
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
                foreach (String fileName in Directory.GetFiles(location))
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

        private List<String> SortFileNames(List<String> fileNameList)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");

            var _fileNameList = new List<string>();
            foreach (string t in fileNameList)
            {
                var f = new FileInfo(t);
                string[] fn = f.FullName.Split('.');
                L.Log(LogType.FILE, LogLevel.DEBUG, "FullName: " + f.FullName);

                var shortFileName = f.Name;

                if (fn[fn.Length - 1] == "log")
                {
                    if (shortFileName.StartsWith("u_ex") || shortFileName.StartsWith("ex") || shortFileName.StartsWith("extend") || shortFileName.StartsWith("IP_"))
                        _fileNameList.Add(t);
                }
            }
            _fileNameList.Sort();
            foreach (string t in _fileNameList)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames

        public bool CoderParse(string line, String fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "CoderParse" + line);
            if (line == "")
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line is empty");
                return true;
            }
            string add = "";
            if (line.StartsWith("#"))
            {
                if (line.Trim().StartsWith("#Fields:"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Line starts with #Fields: ");

                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    var delimiters = new char[] { ' ' };
                    String[] fields = line.Split((char[])delimiters, StringSplitOptions.RemoveEmptyEntries);
                    Int32 count = 0;
                    foreach (string t in fields)
                    {
                        if (t != "#Fields:")
                        {
                            dictHash.Add(t, count);
                            count++;
                        }
                    }

                    foreach (KeyValuePair<String, Int32> kvp in dictHash)
                    {
                        lastKeywords += kvp.Key + ",";
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Keys : " + kvp.Key);
                    }
                    lastKeywords = lastKeywords.Substring(0, lastKeywords.Length - 1);
                    add = lastKeywords;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "lastKeywords: " + lastKeywords);

                }
                return true;
            }

            var csVer_cUserAgentBuilder = new StringBuilder();
            String[] arr = line.Split(' ');
            //
            foreach (string t in arr)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "arr : " + t);
            }

            L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.5");
            try
            {
                var r = new Rec();
                Int32 dateIndex = dictHash["date"];
                arr[dateIndex] = arr[dateIndex].Replace('-', '/');
                r.Datetime = arr[dateIndex] + " " + arr[dictHash["time"]];
                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.6 " + r.Datetime);
                try
                {
                    //r.SourceName = arr[dictHash["s-sitename"]];
                    int value;
                    if (dictHash.TryGetValue("s-sitename", out value))
                    {
                        r.SourceName = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + r.SourceName);
                    }

                }
                catch (Exception e) { L.Log(LogType.FILE, LogLevel.ERROR, "1" + e.Message); }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.7");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("cs-method", out value))
                    {
                        r.EventType = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + r.EventType);
                    }

                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "2" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.8");

                try
                {
                    int value;
                    string tempCsuristem = null;
                    if (dictHash.TryGetValue("cs-uri-stem", out value))
                    {
                        tempCsuristem = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "tempCsuristem: " + tempCsuristem);

                        if (tempCsuristem.Length > 900)
                        {
                            r.Description = tempCsuristem.Substring(0, 900);
                            if (r.Description.Contains(";"))
                            {
                                string[] sqlInjectionStringArray = r.Description.Split(';');
                                r.Description = sqlInjectionStringArray[0];
                                r.CustomStr1 = SqlInjectionStringConcat(sqlInjectionStringArray);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + r.Description);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + r.CustomStr1);
                            }
                        }
                        else
                        {
                            r.Description = tempCsuristem;
                            if (r.Description.Contains(";"))
                            {
                                string[] sqlInjectionStringArray = r.Description.Split(';');
                                r.Description = sqlInjectionStringArray[0];
                                r.CustomStr1 = SqlInjectionStringConcat(sqlInjectionStringArray);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Description_1: " + r.Description);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1_1: " + r.CustomStr1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "3" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.9");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("cs-uri-query", out value))
                    {
                        string tempCsuriquery = arr[value];

                        if (tempCsuriquery.Length > 900)
                        {
                            if (tempCsuriquery.Length >= 1800)
                            {
                                tempCsuriquery = tempCsuriquery.Substring(0, 1800);
                                if (string.IsNullOrEmpty(r.CustomStr1))
                                {
                                    r.CustomStr1 = tempCsuriquery.Substring(0, 900);
                                    r.CustomStr10 = tempCsuriquery.Substring(901, tempCsuriquery.Length - 900);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(r.CustomStr1))
                                {
                                    r.CustomStr1 = tempCsuriquery.Substring(0, 900);
                                    r.CustomStr10 = tempCsuriquery.Substring(901, tempCsuriquery.Length - 900);
                                }
                            }
                        }
                        else
                        {
                            r.CustomStr1 = tempCsuriquery;
                        }
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.0");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("cs-username", out value))
                    {
                        r.UserName = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "UserName :" + r.UserName);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "4" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.1");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("c-ip", out value))
                    {
                        r.CustomStr3 = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3:" + r.CustomStr3);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "5" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.2");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("sc-status", out  value))
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[value]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1:" + r.CustomInt1);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "6" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.3");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("sc-substatus", out value))
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[value]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2:" + r.CustomInt2);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "7" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.4");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("sc-win32-status", out value))
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[value]);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4:" + r.CustomInt4);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "8" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.5");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("s-ip", out value))
                    {
                        r.CustomStr4 = arr[dictHash["s-ip"]];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4:" + r.CustomInt4);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "9" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.6");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("s-port", out value))
                    {
                        r.CustomStr2 = arr[value];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2:" + r.CustomInt2);
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "10" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.7");

                if (dictHash.ContainsKey("cs-version"))
                {
                    try
                    {
                        csVer_cUserAgentBuilder.Append((arr[dictHash["cs-version"]])).Append(" ");
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "11" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.8");

                try
                {
                    int value;
                    if (dictHash.TryGetValue("cs(User-Agent)", out value))
                    {
                        csVer_cUserAgentBuilder.Append((arr[value]));
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "12" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.9");

                r.CustomStr6 = csVer_cUserAgentBuilder.ToString();

                csVer_cUserAgentBuilder.Remove(0, csVer_cUserAgentBuilder.Length);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4");

                if (dictHash.ContainsKey("cs(Referer)"))
                {
                    try
                    {
                        r.CustomStr5 = arr[dictHash["cs(Referer)"]].Length > 900 ? arr[dictHash["cs(Referer)"]].Substring(0, 899) : arr[dictHash["cs(Referer)"]];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "13" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.3");

                if (dictHash.ContainsKey("sc-bytes"))
                {
                    try
                    {
                        r.CustomStr7 = arr[dictHash["sc-bytes"]];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "14" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.4");

                if (dictHash.ContainsKey("cs(Cookie)"))
                {
                    try
                    {
                        r.CustomStr8 = arr[dictHash["cs(Cookie)"]];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "15" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.5");

                if (dictHash.ContainsKey("cs-host"))
                {
                    try
                    {
                        r.CustomStr9 = arr[dictHash["cs-host"]];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "16" + e.Message);
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.6");

                if (dictHash.ContainsKey("cs-bytes"))
                {
                    try
                    {
                        r.CustomInt6 = Convert.ToInt64(arr[dictHash["cs-bytes"]]);
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "17" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.7");

                try
                {
                    if (r.CustomStr10 == null)
                    {
                        r.CustomStr10 = iisType;
                    }
                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "18" + e.Message);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.8");

                if (dictHash.ContainsKey("time-taken"))
                {
                    try
                    {
                        r.CustomInt5 = Convert.ToInt32(arr[dictHash["time-taken"]]);
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "19" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.9");

                if (dictHash.ContainsKey("s-computername"))
                {
                    try
                    {
                        r.ComputerName = arr[dictHash["s-computername"]];
                    }
                    catch (Exception e)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "20" + e.Message);
                    }
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 5.1");

                r.LogName = LogName;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 5.2");

                if (usingRegistry)
                {
                    r.ComputerName = r.CustomStr4;
                }
                else
                {
                    if (!string.IsNullOrEmpty(machineName))
                        if (machineName.Contains("\\") && machineName.Split('\\').Length > 2)
                            r.ComputerName = machineName.Split('\\')[2];
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In CoderParse() -->> Record sending." + last_recordnum + " - " + lastFile);
                    customServiceBase.SetData(Dal, virtualhost, r);
                    customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, lastFile, add, LastRecordDate);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " IISV_7_1_1Recorder In CoderParse() -->> Record sended.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " IISV_7_1_1Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                }

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish Record Data");
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                return false;
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Ends");
            return true;
        }

        public string SqlInjectionStringConcat(string[] array)
        {
            string fullString = "";
            try
            {
                if (array.Length > 1)
                {
                    for (int i = 1; i < array.Length; i++)
                    {
                        fullString += array[i];
                    }
                }
                else
                {
                    foreach (string t in array)
                    {
                        fullString = array[1];
                    }
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "SqlInjectionStringConcat() ERROR: " + ex.Message);
            }
            return fullString;
        } // SqlInjectionStringConcat

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
                                rk = registryKey1.CreateSubKey("IISV_7_1_1Recorder");
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
                    GC.SuppressFinalize(rk);
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
                L.SetLogFileSize(logSize);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // Initialize_Logger
    }
}
