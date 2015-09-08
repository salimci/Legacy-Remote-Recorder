﻿using System;
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

namespace TMG2010WEBV1_0_0Recorder
{
    public struct Fields
    {
        public long CurrentPosition;
        public long RowCount;
    }

    public class TMG2010WEBV1_0_0Recorder : CustomBase
    {

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, remote_host = "", lastFile = "", user = "", password = "";
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
        Dictionary<String, Int32> dictHash;
        private String LogName = "TMG2010WEBV1_0_0Recorder";
        public string _lastKeywords = "";

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

        public TMG2010WEBV1_0_0Recorder()
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TMG2010WEBV1_0_0Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on TMG2010WEBV1_0_0Recorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating TMG2010WEBV1_0_0Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager TMG2010WEBV1_0_0Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  TMG2010WEBV1_0_0Recorder Init Method end.");
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
            _lastKeywords = LastKeywords;
            user = User;
            password = Password;
            tempCustomVar1 = CustomVar1;
        }

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            else
                return 0;
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\TMG2010WEBV1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager TMG2010WEBV1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager TMG2010WEBV1_0_0Recorder Read Registry Error. " + er.Message);
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
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\TMG2010WEBV1_0_0Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("TMG2010WEBV1_0_0Recorder").GetValue("LastRecordNum"));

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

                String line = "";
                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " TMG2010WEBV1_0_0Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In timer1_Tick() --> lastFile: " + lastFile);
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

                        for (int i = 0; i < fileNameList.Count; i++)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  " ParseFileNameLocal() -->> Sorting Files. " + fileNameList[i]);
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
                    else
                    {
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
                L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "TMG2010WEBV1_0_0Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        public bool SendLine(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "SendLine is Started. FileName" + fileName);

            try
            {
                char c;
                int tChar = 0;
                StringBuilder stringBuilder;
                FileStream fileStream;
                BinaryReader reader;


                stringBuilder = new StringBuilder();
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //reader = new BinaryReader(fileStream, Encoding.UTF8);
                StreamReader streamReader = new StreamReader(fileStream);
                string line = "";

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Info: " + fileName + " - " + fileStream.Length + " - " + last_recordnum);
                char ch;

                bool dontSend;

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Path _ if : " + fileName + " - " + fileStream.Length + " - " + last_recordnum);

                if (last_recordnum >= 0)
                {
                    streamReader.BaseStream.Seek(last_recordnum, SeekOrigin.Begin);
                }
                else
                {
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                int r;
                while ((r = streamReader.Read()) >= 0)
                {
                    ch = (char)r;
                    if (ch == '\n' || ch == '\r')
                    {
                        last_recordnum = streamReader.BaseStream.Position;
                        if (stringBuilder.Length > 0)
                        {
                            line = stringBuilder.ToString();
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
                L.Log(LogType.FILE, LogLevel.ERROR, " SendLine() -->> An error occurred : " + exception.ToString());
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
                for (int i = 0; i < fileNameList.Count; i++)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          " SetLastFile() -->> Sorting Files. " + fileNameList[i]);
                }
                int idx = fileNameList.IndexOf(lastFile);


                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(), null, lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "TMG2010WEBV1_0_0Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " TMG2010WEBV1_0_0Recorder In SetLastFile() -->> Record sending Error." +
                      exception.Message);
                return false;
            }
        } // SetLastFile

        public bool WriteData(string line, string fileName)
        {
            enc = new UTF8Encoding();
            string shortFileName = After(fileName, "\\");
            string path = "C:\\tmp\\" + shortFileName;
            try
            {
                StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append));
                streamWriter.Write(line);
                streamWriter.Close();
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " WriteData() -->> An error occurred : " + exception.ToString());
                return false;
            }
        } // WriteData

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

        private List<String> SortFileNames(List<String> fileNameList)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                FileInfo f = new FileInfo(fileNameList[i]);
                string[] fn = f.FullName.Split('.');
                L.Log(LogType.FILE, LogLevel.DEBUG, "FullName: " + f.FullName);
                string shortFileName = After(fileNameList[i], "\\", 0);
                L.Log(LogType.FILE, LogLevel.DEBUG, "shortFileName: " + shortFileName);
                if (fn[fn.Length - 1] == "w3c")
                {
                    if (shortFileName.StartsWith(tempCustomVar1))
                    {
                        if ( shortFileName.Contains("WEB"))
                        {
                            _fileNameList.Add(fileNameList[i]);
                        }
                    }
                }
            }
            _fileNameList.Sort();
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
        }

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
            L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In CoderParse() -->> Started. " + line + " - " + fileName);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "CoderParse | Line : " + line);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "CoderParse | Log error : " + exception.ToString());
            }

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            Rec r = new Rec();
            String add = "";
            String keyWords = "";

            try
            {
                try
                {
                    if (line.StartsWith("#"))
                    {
                        if (line.StartsWith("#Fields:"))
                        {
                            if (dictHash != null)
                                dictHash.Clear();
                            dictHash = new Dictionary<String, Int32>();
                            String[] fields = line.Split('\t');
                            String[] first = fields[0].Split(' ');
                            fields[0] = first[1];
                            Int32 count = 0;
                            foreach (String field in fields)
                            {
                                dictHash.Add(field, count);
                                count++;
                            }

                            foreach (KeyValuePair<String, Int32> kvp in dictHash)
                            {
                                keyWords += kvp.Key + ",";
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Keys : " + kvp.Key);
                            }
                            keyWords = keyWords.Substring(0, keyWords.Length - 1);

                        }
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + ex.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + line);
                }

                try
                {
                    if (!line.StartsWith("#"))
                    {
                        String[] arr = line.Split('\t');

                        try
                        {
                            if (dictHash != null)
                            {
                                foreach (KeyValuePair<String, Int32> kvp in dictHash)
                                {
                                    add += kvp.Key + ",";
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Values : " + kvp.Key);
                                }
                            }
                            else
                            {
                                if (dictHash == null)
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "lastKeywords: " + _lastKeywords);
                                    dictHash = new Dictionary<String, Int32>();
                                    String[] fields = _lastKeywords.Split(',');
                                    Int32 count = 0;
                                    foreach (String field in fields)
                                    {
                                        dictHash.Add(field, count);
                                        count++;
                                    }

                                    foreach (KeyValuePair<String, Int32> kvp in dictHash)
                                    {
                                        keyWords += kvp.Key + ",";
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Keys : " + kvp.Key);
                                    }

                                    L.Log(LogType.FILE, LogLevel.DEBUG, "keywords: " + keyWords);

                                }
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "add: " + add);

                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "foreach: " + exception.Message);
                        }

                        try
                        {
                            string dateString2 = arr[4] + " " + arr[5];
                            DateTime dt = Convert.ToDateTime(dateString2);
                            r.Datetime = dt.ToString(dateFormat);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Datetime : " + ex.Message);
                        }
                        r.LogName = LogName;

                        try
                        {
                            r.EventCategory = arr[dictHash["action"]]; //-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory : " + ex.Message);
                        }
                        //
                        try
                        {
                            r.EventType = arr[dictHash["s-operation"]]; //-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventType : " + ex.Message);
                        }

                        try
                        {
                            r.UserName = arr[dictHash["cs-username"]]; //-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "UserName : " + r.UserName);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "UserName : " + ex.Message);
                        }

                        try
                        {
                            r.SourceName = arr[dictHash["c-ip"]]; //-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName : " + r.SourceName);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "SourceName : " + ex.Message);
                        }

                        try
                        {
                            r.LogName = LogName;
                            L.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "LogName : " + ex.Message);
                        }

                        try
                        {
                            r.ComputerName = arr[dictHash["s-computername"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                        }

                        try
                        {
                            string str1 = arr[dictHash["UrlDestHost"]];//-
                            if (str1.Length > 899)
                            {
                                r.CustomStr1 = str1.Substring(0, 899);
                            }
                            else
                            {
                                r.CustomStr1 = str1;
                            }

                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr2 = arr[dictHash["cs-protocol"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr3 = arr[dictHash["NAT address"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr4 = arr[dictHash["r-ip"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr5 = arr[dictHash["cs-referred"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 : " + ex.Message);

                        }

                        try
                        {
                            r.CustomStr6 = arr[dictHash["cs-uri"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 : " + ex.Message);

                        }

                        try
                        {
                            r.CustomStr7 = arr[dictHash["rule"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 : " + ex.Message);

                        }

                        try
                        {
                            if (string.IsNullOrEmpty(arr[dictHash["cs-network"]]))
                            {
                                r.CustomStr8 = arr[dictHash["cs-network"]];//-
                                L.Log(LogType.FILE, LogLevel.DEBUG,
                                        "CustomInt1 : " + arr[dictHash["recipient-status"]]);
                            }
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr9 = arr[dictHash["sc-network"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr10 = arr[dictHash["FilterInfo"]];//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10 : " + r.CustomStr10);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[dictHash["sc-bytes"]]);//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + r.CustomInt1);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt1 : " + ex.Message);
                            r.CustomInt1 = 0;
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["cs-bytes"]]);//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + r.CustomInt2);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 : " + ex.Message);
                            r.CustomInt2 = 0;
                        }

                        try
                        {
                            r.CustomInt3 = Convert.ToInt32(arr[dictHash["s-port"]]);//-
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + r.CustomInt3);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt3 : " + ex.Message);
                            r.CustomInt3 = 0;
                        }

                        try
                        {
                            r.CustomInt4 = Convert.ToInt32(arr[dictHash["r-port"]]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + r.CustomInt4);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 : " + ex.Message);
                            r.CustomInt4 = 0;
                        }

                        try
                        {
                            r.CustomInt5 = Convert.ToInt32(arr[dictHash["sc-status"]]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5 : " + r.CustomInt5);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt5 : " + ex.Message);
                            r.CustomInt5 = 0;
                        }

                        try
                        {
                            r.CustomInt6 = Convert.ToInt32(arr[dictHash["time-taken"]]);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 : " + r.CustomInt6);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.WARN, "CustomInt6 : " + ex.Message);
                            r.CustomInt6 = 0;
                        }

                        try
                        {
                            if (line.Length > 899)
                            {
                                r.Description = line.Substring(0, 899);
                            }

                            else
                            {
                                r.Description = line;
                            }
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + line);
                        }
                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Description : " + ex.Message);
                        }

                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In CoderParse() -->> Record sending." + last_recordnum + " - " + lastFile);
                            customServiceBase.SetData(Dal, virtualhost, r);
                            customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, lastFile, add, LastRecordDate);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " TMG2010WEBV1_0_0Recorder In CoderParse() -->> Record sended.");
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, " TMG2010WEBV1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Line startswith #  ");
                    }

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
                L.Log(LogType.FILE, LogLevel.ERROR, " TMG2010WEBV1_0_0Recorder In CoderParse() Error.");
                //WriteMessage("CoderParse | Parsing Error : " + e.ToString());
                //WriteMessage("CoderParse | line: " + line);
                return false;
            }
            return true;
        } // CoderParse

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
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("TMG2010WEBV1_0_0Recorder");
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
                //L.SetLogFileSize(log_size);
                L.SetLogFileSize(100000000);

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