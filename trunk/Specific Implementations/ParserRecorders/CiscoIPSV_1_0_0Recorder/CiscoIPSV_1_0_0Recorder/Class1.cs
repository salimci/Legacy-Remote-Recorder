using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using System.Collections;
using System.Globalization;
using System.Threading;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        public string currentFile;
    }
    
    public class CiscoIPSV_1_0_0Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private int num;
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        object syncRoot = new object();

        public CiscoIPSV_1_0_0Recorder()
            : base()
        {
            LogName = "CiscoIPSV_1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = true;
        } // CiscoIPSV_1_0_0Recorder

        public override void Init()
        {
            try
            {
                GetFiles();
                Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
            }
            catch (Exception exception)
            {
              
            }
        } // Init

        public override void Start()
        {
            base.Start();
        } // Start


        public CiscoIPSV_1_0_0Recorder(String fileName)
            : base(fileName)
        {

        } // CiscoIPSV_1_0_0Recorder

        protected override void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
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
                        FileName = Dir;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        } // ParseFileNameLocal

        public override bool ParseSpecific(String line, bool dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

                if (!string.IsNullOrEmpty(RecordFields.currentFile))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            "ParseSpecific | currentFile : " + RecordFields.currentFile);
                }
                else
                {
                    RecordFields.currentFile = lastFile;
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | dontSend : " + dontSend);
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Log error : " + exception.ToString());
            }

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }
            string[] lineArr = SpaceSplit(line, true);
            Rec r = new Rec();



            try
            {
                string dateString = lineArr[0] + " " + lineArr[1];
                DateTime dt;
                dt = Convert.ToDateTime(dateString);
                r.Datetime = dt.ToString(dateFormat);
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Date error, Line " + line);
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Date error: " + exception.ToString());
            }

            try
            {
                if (line.Length > 899)
                {
                    r.Description = line.Substring(0, 899);
                    r.CustomStr10 = line.Substring(900, line.Length - 900);
                }
                else
                {
                    r.Description = line;
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "desc error: " + exception.Message);
            }

            try
            {
                for (int i = 0; i < lineArr.Length; i++)
                {
                    try
                    {
                        if (lineArr[i].StartsWith("eventid="))
                        {
                            r.EventId = Convert.ToInt64(ColumnValue(lineArr[i]));
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "EventId: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("actions="))
                        {
                            r.EventCategory = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "EventCategory: " + exception.Message);
                    }

                    try
                    {

                        if (lineArr[i].StartsWith("target_value_rating="))
                        {
                            r.EventType = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "EventType: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("hostId="))
                        {
                            r.ComputerName = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("attacker=") && !lineArr[i].StartsWith("attacker_"))
                        {
                            r.CustomStr3 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3: " + exception.Message);
                    }
                    try
                    {

                        if (lineArr[i].StartsWith("target=") && !lineArr[i].StartsWith("target_"))
                        {
                            r.CustomStr4 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("interface=") && !lineArr[i].StartsWith("interface_"))
                        {
                            r.CustomStr5 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("attacker_locality="))
                        {
                            r.CustomStr6 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("target_locality="))
                        {
                            r.CustomStr7 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("protocol="))
                        {
                            r.CustomStr8 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("attack_relevance_rating="))
                        {
                            r.CustomStr9 = ColumnValue(lineArr[i]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("risk_rating="))
                        {
                            r.CustomInt1 = (int)Convert.ToInt64(ColumnValue(lineArr[i]));
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("threat_rating="))
                        {
                            r.CustomInt2 = (int)Convert.ToInt64(ColumnValue(lineArr[i]));
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR,"CustomInt2: " + exception.Message);
                    }

                    try
                    {

                        if (lineArr[i].StartsWith("attacker_port="))
                        {
                            r.CustomInt3 = (int)Convert.ToInt64(ColumnValue(lineArr[i]));
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3: " + exception.Message);
                    }

                    try
                    {
                        if (lineArr[i].StartsWith("target_port="))
                        {
                            r.CustomInt4 = (int)Convert.ToInt64(ColumnValue(lineArr[i]));
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4: " + exception.Message);
                    }
                }

                try
                {
                    r.CustomStr1 = Between(line, "description=", "sig_version").Replace('"', ' ').Trim();
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR,"CustomStr1: " + exception.Message);
                }
                try
                {
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Start sending data.");
                        SetRecordData(r);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending data.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "SetRecordData : " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "General Error: " + ex.Message);
            }
            return true;
        }

        public string ColumnValue(string value)
        {
            return After(value, "=").Replace('"', ' ').Trim();
        } //ColumnValue


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

                    if (Position + 1 == fileLength)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "IsFileFinished() -->> " + file + " finished.");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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
            }
        } // SetLastFile

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        private List<String> SortFileNames(List<String> fileNameList)
        {
            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                _fileNameList.Add(fileNameList[i]);
                _fileNameList.Sort();
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            foreach (string t in _fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames
    }
}


