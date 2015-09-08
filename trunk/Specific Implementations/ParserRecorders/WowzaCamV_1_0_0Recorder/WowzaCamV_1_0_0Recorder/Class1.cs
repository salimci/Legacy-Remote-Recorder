using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Log;
using System.Collections;
using System.Globalization;
using System.Threading;
using WowzaCamV_1_0_0Recorder;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        public string currentFile;
        public string tempDateTime;
        public string ComputerName;
        public string Str3;
        public string Str4;
        public bool flag;

        public string Description;

    }
    public class WowzaCamV_1_0_0Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private int num;
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        object syncRoot = new object();
        static Regex FilePattern = new Regex(".*wowza[0-9]{14}.txt", RegexOptions.IgnoreCase);
        private bool dontSend;

        public WowzaCamV_1_0_0Recorder()
            : base()
        {
            LogName = "WowzaCamV_1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = true;
            dontSend = false;
        } // Exchange2010SP2V1_0_2Recorder

        public override void Init()
        {
            try
            {
                GetFiles();
                Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");//
            }
            catch (Exception exception)
            {

            }
        }

        public override void Start()
        {
            base.Start();
        } // Start


        public WowzaCamV_1_0_0Recorder(String fileName)
            : base(fileName)
        {

        } // WowzaCamV_1_0_0Recorder

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
            try
            {
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnLocal();

                    fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);

                    int idx = 0;
                    idx = fileNameList.IndexOf(lastFile);

                    if (fileNameList.Count+1 == idx)
                    {
                        dontSend = true;
                    }
                    else
                    {
                        dontSend = false;
                    }
                }
                else
                {
                    FileName = Dir;
                }

                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is successfully FINISHED");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
            }
        } // ParseFileNameLocal

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> An eror occurred : " + ex.ToString());
            }
        } // ParseFileNameRemote

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | tempDateTime : " + RecordFields.tempDateTime);

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }
            Rec r = new Rec();
            try
            {
                r.LogName = LogName;
                if (line.StartsWith("Date:"))
                {
                    if (string.IsNullOrEmpty(RecordFields.tempDateTime))
                    {
                        DateTime dt = new DateTime();
                        dt = Convert.ToDateTime(After(line, "Date:"));
                        RecordFields.tempDateTime = dt.ToString(dateFormat);
                    }
                }

                if (string.IsNullOrEmpty(r.Datetime))
                {
                    r.Datetime = RecordFields.tempDateTime;
                }

                RecordFields.Description += line + " | ";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "RecordFields.Description: " + RecordFields.Description);
                r.CustomStr9 = FileName;//

                if (RecordFields.Description.Contains("ConnectionsTotal") && RecordFields.Description.Contains("ConnectionsCurrent") && RecordFields.Description.Contains("Name"))
                {
                    RecordFields.flag = true;
                }

                if (RecordFields.flag)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sending.");
                    r.Description = RecordFields.Description;
                    string[] lineArr = RecordFields.Description.Split('|');
                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr[i].Trim().StartsWith("Name"))
                        {
                            r.ComputerName = lineArr[i].Split(':')[1];
                        }

                        if (lineArr[i].Trim().StartsWith("ConnectionsTotal"))
                        {
                            r.CustomStr4 = lineArr[i].Split(':')[1];
                        }

                        if (lineArr[i].Trim().StartsWith("ConnectionsCurrent"))
                        {
                            r.CustomStr3 = lineArr[i].Split(':')[1];
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description:" + r.Description);

                    if (r.Description.EndsWith(" | "))
                    {
                        r.Description = r.Description.Substring(0, r.Description.Length - 2);
                    }
                    r.CustomStr10 = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "dontSend: " + dontSend);

                    if (!dontSend)
                    {
                        SetRecordData(r);
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
                    }

                    ClearFields();
                }
               
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | Line : " + line);

                return false;
            }
            return true;
        }

        private void ClearFields()
        {
            RecordFields.ComputerName = null;
            RecordFields.Str3 = null;
            RecordFields.Str4= null;
            RecordFields.Description= null;
            RecordFields.flag = false;
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

                    if (Position + 1 == fileLength || Position == fileLength)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "IsFileFinished() -->> " + file + " finished.");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    //using (StreamReader sr = new StreamReader(file))
                    //{
                    //    while ((line = sr.ReadLine()) != null)
                    //    {
                    //        return true;
                    //    }
                    //    return false;
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
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
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Mesaj: " + e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
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
                List<String> fileNameList = new List<String>(Directory.GetFiles(Dir, "*.txt").Where(x => FilePattern.IsMatch(x)));
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

        private void SetLastFile(List<string> fileNameList)
        {//
            try
            {
                if (fileNameList != null && fileNameList.Count > 0)
                {
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        FileInfo inf = new FileInfo(lastFile);
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                            " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile + ", LineLimit=" + lineLimit);
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> item is " + inf.FullName);

                        int idx;

                        if ((idx = fileNameList.IndexOf(inf.FullName)) >= 0)
                        {
                            if (IsFileFinished(lastFile))
                            {
                                idx++;
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                        FileName);
                                dontSend = true;

                                if (idx < fileNameList.Count)
                                {
                                    FileName = fileNameList[idx];
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Onur--- : " + FileName + " Value " + idx);
                                    lastFile = FileName;
                                    Position = 0;
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. New file is  : " + FileName);

                                }
                            }
                            else
                            {
                                FileName = lastFile;
                            }
                        }
                        else
                        {
                            idx = fileNameList.IndexOf(lastFile);
                            if (idx <= 0)
                            {
                                if (fileNameList.Count > idx++)
                                {
                                    lastFile = fileNameList[idx++];
                                    FileName = lastFile;
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
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" +
                                lastFile);
                        FileName = fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->>  Last file is assign on database : " + FileName);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->> No input file in directory:" + Dir);
                    FileName = null;
                    lastFile = FileName;
                }

            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile

        public bool FileExistControl(List<string> fileNameList, string fileName)
        {
            try
            {
                return true;
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " FileExistControl() -->> An error occurred : " + exception.ToString());
                return false;
            }
        } // FileExistControl

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

        protected FileNameComperator filenameComperator = new FileNameComperator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        /// 

        private List<string> SortFileNames(List<string> fileNameList)
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> is STARTED. ");
            List<string> filteredFileNameList = new List<string>();
            foreach (var fileName in fileNameList)
            {
                filteredFileNameList.Add(fileName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Adding file name is " + fileName);
            }
            filteredFileNameList.Sort(filenameComperator);
            foreach (var item in filteredFileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> item is: " + item);
            }
            Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> is Finished. ");
            return fileNameList;
        } // SortFileNames
    }
}

//
