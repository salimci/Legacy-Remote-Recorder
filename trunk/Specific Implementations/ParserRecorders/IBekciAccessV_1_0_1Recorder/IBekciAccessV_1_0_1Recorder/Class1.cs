//IBekciAccessV_1_0_1Recorder

using System;
using System.Collections;
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

namespace IBekciAccessV_1_0_1Recorder
{
    public class IBekciAccessV_1_0_1Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, remote_host = "", lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        Dictionary<String, Int32> dictHash;
        private String LogName = "IBekciAccessV_1_0_1Recorder";
        private String user, password;
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

        public IBekciAccessV_1_0_1Recorder()
        {
            InitializeComponent();
            enc = Encoding.UTF8;
        }

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
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IBekciAccessV_1_0_1Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on IBekciAccessV_1_0_1Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start creating IBekciAccessV_1_0_1Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager IBekciAccessV_1_0_1Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  IBekciAccessV_1_0_1Recorder Init Method end.");
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
            max_record_send = MaxLineToWait;//Data amount to get per tick
            timer_interval = SleepTime; //Timer interval.
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;
            lastFile = LastFile;
            user = User;
            password = Password;
            tempCustomVar1 = CustomVar1;
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
            DateTime dt = DateTime.Now;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\IBekciAccessV_1_0_1Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IBekciAccessV_1_0_1Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager IBekciAccessV_1_0_1Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\IBekciAccessV_1_0_1Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("IBekciAccessV_1_0_1Recorder").GetValue("LastRecordNum"));

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
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is Started");

                String line = "";
                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " IBekciAccessV_1_0_1Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In timer1_Tick() --> lastFile: " + lastFile);
                    ParseFileNameLocal();
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick

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

        protected void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
                    L.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Position: " + last_recordnum);

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

                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED " + lastFile);

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

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                CustomServiceBase customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "IBekciAccessV_1_0_1Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        }// SetNewFile

        public bool SendLine(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "SendLine is Started. FileName" + fileName);

            try
            {
                char c;
                var stringBuilder = new StringBuilder();
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var reader = new BinaryReader(fileStream, Encoding.UTF7);

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Info: " + fileName + " - " + fileStream.Length + " - " + last_recordnum);
                char ch;

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Path _ if : " + fileName + " - " + fileStream.Length + " - " + last_recordnum);

                if (last_recordnum >= 0)
                {
                    reader.BaseStream.Seek(last_recordnum, SeekOrigin.Begin);
                }
                else
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                int r;
                while ((r = reader.Read()) >= 0)
                {
                    ch = (char)r;
                    if (ch == '\n' || ch == '\r')
                    {
                        last_recordnum = reader.BaseStream.Position;
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
                reader.Close();
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
            L.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile is started with " + fileName);

            CustomServiceBase customServiceBase = GetInstanceService("Security Manager Remote Recorder");
            try
            {
                List<String> fileNameList = GetFileNamesOnLocal();
                fileNameList = SortFileNames(fileNameList);

                int idx = fileNameList.IndexOf(lastFile);

                L.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> lastFile: " + lastFile);
                L.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> tempCustomVar1: " + tempCustomVar1);
                L.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> idx: " + idx);

                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." +
                      fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "IBekciAccessV_1_0_1Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " IBekciAccessV_1_0_1Recorder In SetLastFile() -->> Error on SetLastFile: " +
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

        private List<String> SortFileNames(List<String> fileNameList)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");

            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                FileInfo f = new FileInfo(fileNameList[i]);
                string fileFullName = After(f.FullName, "\\", 0);
                string[] fn = fileFullName.Split('.');
                if (fn.Length == 2 && fn[1] == "kyt" && !fn[0].StartsWith("aa") && !fn[0].StartsWith("web"))
                {
                    _fileNameList.Add(fileNameList[i]);
                }
            }

            _fileNameList.Sort();
            foreach (string t in _fileNameList)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames(): " + t);
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
                posA = value.IndexOf(a);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a);
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

        public bool CoderParse(string line, String fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In CoderParse() -->> Started. " + line);

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            try
            {

                if (line.Length > 10)
                {

                    String[] arr = SpaceSplit(line, false);
                    ArrayList arrList = new ArrayList();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(arr[i]))
                        {
                            arrList.Add(arr[i].Trim());
                        }
                    }

                    Rec r = new Rec();
                    DateTime dt;
                    dt = DateTime.Now;
                    string myDateTimeString = arr[0] + arr[1] + "," + dt.Year + "," + arr[2].Split(':')[0] + ":" + arr[2].Split(':')[1] + ":" + arr[2].Split(':')[2].Split('.')[0];
                    dt = Convert.ToDateTime(myDateTimeString);
                    r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);



                    for (int i = 0; i < arrList.Count; i++)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ArrList : " + arrList[i]);
                    }

                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[4].Replace("-", " ").Trim());
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + r.CustomInt1);
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 = 0 : " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + arr[4]);
                    }

                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[6].Split('/')[0].Trim());
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + arr[6]);
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 = 0 : " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + arr[6]);
                    }

                    try
                    {
                        r.CustomInt3 = Convert.ToInt32(arr[11].Split('.')[4].Trim());
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + r.CustomInt3);
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 = 0 : " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + arr[11].Split('.')[4]);
                    }

                    try
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[13].Split('.')[4].Replace(":", " ").Trim());
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + r.CustomInt4);
                    }
                    catch (Exception ex)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 = 0 : " + ex.Message);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + arr[13].Split('.')[4]);
                    }

                    r.EventType = arr[7];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                    r.CustomStr1 = arr[8];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                    r.CustomStr2 = arr[10];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
                    r.CustomStr3 = arr[11].Split('.')[0] + "." + arr[11].Split('.')[1] + "." + arr[11].Split('.')[2] + "." + arr[11].Split('.')[3];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                    r.CustomStr4 = arr[13].Split('.')[0] + "." + arr[13].Split('.')[1] + "." + arr[13].Split('.')[2] + "." + arr[13].Split('.')[3];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                    r.CustomStr7 = arr[14];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                    r.LogName = LogName;

                    r.CustomStr9 = fileName;

                    if (line.Length > 899)
                    {
                        r.Description = line.Substring(0, 899);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                    }
                    else
                    {
                        r.Description = line;
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                    }

                    CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                    try
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In CoderParse() -->> Record sending." + last_recordnum + " - " + lastFile + " - " + last_recordnum);
                        if (line.Length > 10)
                        {
                            customServiceBase.SetData(Dal, virtualhost, r);
                        }
                        customServiceBase.SetReg(Id, last_recordnum.ToString(), line, lastFile, "", LastRecordDate);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " IBekciAccessV_1_0_1Recorder In CoderParse() -->> Record sended.");
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " IBekciAccessV_1_0_1Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                    }
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Coder Parse Error: " + e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "Coder Parse Error: " + e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "Coder Parse Error: | Line : " + line);
                return false;
            }
            return true;
        } // CoderParse


        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);

            }
            catch
            {
                return iReturn;
            }

        }
        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }

        }
        private double ObjectToDouble(string sObject, double iReturn)
        {
            try
            {
                return Convert.ToDouble(sObject);
            }
            catch
            {
                return iReturn;
            }
        }

        private string getIzin(int iIzinKod)
        {
            switch (iIzinKod)
            {
                case 1: return "Geçti";
                case 0: return "Takıldı";
                default: return "-";
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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("IBekciAccessV_1_0_1Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager IBekciAccessV_1_0_1Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);
                //L.SetLogFileSize(100000000);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IBekciAccessV_1_0_1Recorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
