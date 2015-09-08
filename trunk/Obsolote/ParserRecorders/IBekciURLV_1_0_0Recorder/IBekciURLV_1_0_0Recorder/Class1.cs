using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;
using System.Globalization;
using System.Threading;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        //public long lineNumber;
        public string currentFile;
        Dictionary<String, Int32> dictHash;
    }

    public class IBekciURLV_1_0_0Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private int num;
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        object syncRoot = new object();

        public IBekciURLV_1_0_0Recorder()
            : base()
        {
            LogName = "IBekciURLV_1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = true;
        } // IBekciURLV_1_0_0Recorder

        public override void Init()
        {
            try
            {
                GetFiles();
                //Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");//
            }
            catch (Exception exception)
            {

            }
        }

        public override void Start()
        {
            base.Start();
        } // Start


        public IBekciURLV_1_0_0Recorder(String fileName)
            : base(fileName)
        {

        } // IBekciURLV_1_0_0Recorder

        protected override void ParseFileNameLocal()
        {
            //if (Monitor.TryEnter(syncRoot))
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED " + Dir);
                try
                {


                    if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList = SortFileNames(fileNameList);
                        for (int i = 0; i < fileNameList.Count; i++)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Sorting Files. " + fileNameList[i]);
                        }
                        SetLastFile(fileNameList);
                    }
                    else
                    {
                        FileName = Dir;//
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                }

                finally
                {
                    //Monitor.Exit(syncRoot);
                }
            }
        } // ParseFileNameLocal

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnRemote();
                    fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> An eror occurred : " + ex.ToString());
            }
        } // ParseFileNameRemote


        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line İs Null or Empty");
                return true;
            }

            string sKeyWord = "zaman kaynakip hedefip tür kuralno izin kategori url";

            dictHash = new Dictionary<String, Int32>();

            String[] fields = sKeyWord.Split(' ');
            Int32 count = 0;

            foreach (String field in fields)
            {
                dictHash.Add(field, count);
                count++;
            }


            if (!dontSend)
            {

                String[] arr = SpaceSplit(line, false); ;

                try
                {

                    Rec r = new Rec();
                    string tempDescription = arr[dictHash["url"]];
                    r.CustomStr3 = arr[dictHash["kaynakip"]];
                    r.CustomStr4 = arr[dictHash["hedefip"]];

                    
                    try
                    {
                        r.CustomStr6 = tempDescription.Substring(0, tempDescription.IndexOf('/'));
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 Error: "  + exception.Message);
                    }

                    r.EventCategory = arr[dictHash["kategori"]];
                    r.LogName = LogName;
                    DateTime dtFile = new DateTime(1970, 1, 1, 0, 0, 0);
                    r.Datetime = dtFile.AddSeconds(ObjectToDouble(arr[dictHash["zaman"]], 0)).ToString();
                    r.CustomStr2 = arr[dictHash["tür"]];
                    r.CustomInt1 = ObjectToInt32(arr[dictHash["kuralno"]], 0);
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["izin"]], 0);
                    r.CustomStr5 = getIzin(ObjectToInt32(arr[dictHash["izin"]], -1));
                    r.CustomStr9 = FileName;
                    
                    try
                    {
                        if (tempDescription.Length > 899)
                        {
                            r.Description = tempDescription.Substring(0, 899);
                            r.CustomStr10 = tempDescription.Substring(899, tempDescription.Length - 899);
                        }
                        else
                        {
                            r.Description = tempDescription;
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Description Error: "  +exception.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.INFORM, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Finish Record Data");

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                    return true;
                }

            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParsingEnds");

            return true;
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
        /// Check the given file is finished or not 
        /// </summary>
        /// <param name="file">File full path which will check</param>
        /// <returns>if file finished return true</returns>
        private Boolean IsFileFinished(String file)
        {
            int lineCount = 0;
            String stdOut = "";
            String stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                            " IsFileFinished() -->> reading local file! " + file);

                    fileStream.Seek(Position, SeekOrigin.Begin);
                    FileInfo fileInfo = new FileInfo(file);
                    Int64 fileLength = fileInfo.Length;
                    Byte[] byteArray = new Byte[3];
                    fileStream.Read(byteArray, 0, 3);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> filelength : " + fileLength);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> Position : " + Position);

                    //if (Position + 1 == fileLength)
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, "IsFileFinished() -->> " + file + " finished.");

                    //    return true;
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                    using (StreamReader sr = new StreamReader(file))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
                //WriteMessage("IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                //WriteMessage("IsFileFinished() -->> " + ex.StackTrace);
                return false;
            }
        } // IsFileFinished

        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception e)
            {
                //if (reg == null)
                //{
                //    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                //    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Mesaj: " + e.StackTrace);
                //}
                //else
                //{
                //    Log.Log(LogType.FILE, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                //    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                //}
            }
        } // GetFiles

        protected override void dayChangeTimer_Elapsed(Object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        } // dayChangeTimer_Elapsed

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                List<String> fileNameList = new List<String>();
                foreach (String fileName in Directory.GetFiles(Dir))
                {
                    String fileShortName = Path.GetFileName(fileName).ToString();
                    fileNameList.Add(fileShortName);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnLocal

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is STARTED ");

                String line = "";
                String stdOut = "";
                String stdErr = "";

                String command = "ls -lt " + Dir;//FileNames contains what.*** fileNameFilter
                Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> SSH command : " + command);

                se.Connect();
                se.RunCommand(command, ref stdOut, ref stdErr);
                se.Close();

                StringReader stringReader = new StringReader(stdOut);
                List<String> fileNameList = new List<String>();
                Boolean foundAnyFile = false;

                while ((line = stringReader.ReadLine()) != null)
                {
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameList.Add(arr[arr.Length - 1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> File name is read: " + arr[arr.Length - 1]);
                    foundAnyFile = true;
                }

                if (!foundAnyFile)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFileNamesOnRemote() -->> There is no proper file in directory");
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnRemote() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnRemote

        /// <summary>
        /// Select the suitable last file in the file name list
        /// </summary>
        /// <param name="fileNameList">The file names are in the stored directory</param>
        private void SetLastFile(List<string> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);

                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                ArrayList list = new ArrayList();

                for (int num = 0; num < fileNameList.Count; num++)
                {
                    dictionary.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture), num);
                    list.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture));
                }

                if (list.Count > 0)
                {
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile, new int[0]);
                        string item = lastFile.Replace(Dir, "");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> item is " + item);

                        if (fileNameList.Contains(item))
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> num3: " + RecordFields.num3 + "list.count: " + list.Count);
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> fName: " + list[RecordFields.num3]);

                            if ((RecordFields.num3 + 1) != list.Count)
                            {
                                if (IsFileFinished(lastFile))
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
                                    string key = item;
                                    if (dictionary.ContainsKey(key))
                                    {
                                        RecordFields.num3 = dictionary[key];
                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                                FileName, new int[0]);
                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                                RecordFields.num3.ToString(CultureInfo.InvariantCulture));

                                        if ((RecordFields.num3 + 1) != list.Count)
                                        {
                                            FileName = Dir + list[RecordFields.num3 + 1].ToString();
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.num3 + 1].ToString(), " Value ", RecordFields.num3 }), new int[0]);
                                            lastFile = FileName;
                                            Position = 0;
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName, new int[0]);
                                            RecordFields.num3 = RecordFields.num3 + 1;
                                            RecordFields.currentFile = lastFile;
                                        }
                                        else
                                        {
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
                                        }
                                    }
                                }
                                else
                                {
                                    FileName = lastFile;
                                }
                            }

                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
                            FileName = Dir + fileNameList[fileNameList.Count + 1];
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
                        }
                    }
                    else
                    {
                        FileName = Dir + fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
                        RecordFields.currentFile = lastFile;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile, new int[0]);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
                //WriteMessage(" SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile



        public bool FileFinishControl(long position, long fileLength)
        {
            if (position == fileLength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Control giving line will reading or skipping
        /// </summary>
        /// <param name="line">Will checking line</param>
        /// <returns>Returns line will reading or skipping</returns>
        private Boolean IsSkipKeyWord(String line)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is STARTED ");
                if (null != _skipKeyWords && _skipKeyWords.Length > 0)
                {
                    foreach (String item in _skipKeyWords)
                    {
                        if (line.StartsWith(item))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned True ");
                            return true;
                        }
                    }
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned False");
                return false;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " IsSkipKeyWord() -->> An error occured" + ex.ToString());
                return false;
            }
        } // IsSkipKeyWord

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        private List<String> SortFileNames(List<String> fileNameList)
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> is STARTED ");

            List<string> sortedFileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                if (fileNameList[i].ToString().StartsWith("web"))
                {
                    sortedFileNameList.Add(fileNameList[i]);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->>  " + fileNameList[i].ToString());
                }
            }
            sortedFileNameList.Sort();
            return sortedFileNameList;
        } // SortFileNames

        /// <summary>
        /// Create file number given file properties
        /// </summary>
        /// <param name="fileNameProp">File date properties</param>
        private void SetFileNumber(ref FileNameProp fileNameProp)
        {
            String _fileNumber = "";
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is STARTED");

                _fileNumber = fileNameProp.FNYearPart +
                                        fileNameProp.FNMonthPart +
                                        fileNameProp.FNDayPart +
                                        fileNameProp.FileNumber;

                fileNameProp.FileNumber = Convert.ToInt64(_fileNumber);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is successfully FINISHED");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetFileNumber() -->> An error occurred" + ex.ToString());
            }

        } // SetFileNumber

        class FileNameProp : IComparable
        {
            public String FileName = "0";
            public Int64 FileNumber = 0;
            public String FNYearPart = "0";
            public String FNMonthPart = "0";
            public String FNDayPart = "0";
            public String FNNumberPart = "0";
            public String FNGenericPart = "0";

            public int CompareTo(object obj)
            {
                FileNameProp sampleFNNo = (FileNameProp)obj;
                return Convert.ToInt32(this.FileNumber - sampleFNNo.FileNumber);
            }
        }

        class FileNamePropComparerByGenericPart : IComparer
        {
            public int Compare(object x, object y)
            {
                FileNameProp fnp1 = (FileNameProp)x;
                FileNameProp fnp2 = (FileNameProp)y;
                return String.Compare(fnp1.FNGenericPart, fnp2.FNGenericPart);
            }
        }

        public struct newFields
        {
            public string datetime;
            public string clientIp;
            public string clientHostname;
            public string serverIp;
            public string serverHostname;
            public string sourceContext;
            public string connectorId;
            public string source;
            public string eventId;
            public string internalMessageId;
            public string messageId;
            public string recipientAddress;
            public string recipientStatus;
            public string totalBytes;
            public string recipientCount;
            public string relatedRecipientAddress;
            public string reference;
            public string messageSubject;
            public string senderAddress;
            public string returnPath;
            public string messageInfo;
            public string directionality;
            public string tenantId;
            public string originalClientIp;
            public string originalServerIp;
            public string customData;
        }
    }
}

//
