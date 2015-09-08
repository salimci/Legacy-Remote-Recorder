/*
Ted Üniversitesi Santral log alımı
* Developped by Onur SARIKAYA
* 03.09.2012 
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;
using System.Globalization;

namespace Parser
{

    public struct Fields
    {
        public int num3;
        public long lineNumber;
        public string currentFile;
    }

    public class CiscoUnifiedCMV8_6_1CentralRecorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;

        //MSGTRKM20120105-1.LOG
        public CiscoUnifiedCMV8_6_1CentralRecorder()//recorderName
            : base()
        {
            LogName = "CiscoUnifiedCMV8_6_1CentralRecorder";//recorderName***
            RecordFields = new Fields();
        } // Exchange2010Recorder

        public override void Init()
        {
            GetFiles();
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
        } // Init

        public override void Start()
        {
            base.Start();
        } // Start

        public CiscoUnifiedCMV8_6_1CentralRecorder(String fileName)//recorderName***
            : base(fileName)
        {

        } // Exchange2010Recorder

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="dontSend"></param>
        /// <returns></returns>
        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);


            char c = '"';
            string b = c.ToString();

            Rec r = new Rec();
            if (line == "")
                return true;

            try
            {
                try
                {
                    if (line.StartsWith(b))
                    {
                        if (dictHash != null)
                            dictHash.Clear();

                        dictHash = new Dictionary<String, Int32>();
                        String[] fields = line.Split(',');

                        Int32 count = 0;
                        foreach (String field in fields)
                        {
                            dictHash.Add(field, count);
                            count++;
                        }
                        String add = "";
                        int d = 0;
                        foreach (KeyValuePair<String, Int32> kvp in dictHash)
                        {
                            add += kvp.Key + ",";
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Keys : " + d + ". " + kvp.Key.Trim('"'));
                            d++;
                        }
                        SetLastKeywords(add);
                        keywordsFound = true;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + ex.Message);
                }

                try
                {
                    if (line.StartsWith("1"))
                    {
                        String[] arr = line.Split(',');

                        for (int i = 0; i < arr.Length; i++)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "arr['" + i + "']:  " + arr[i]);
                        }

                        r.LogName = LogName;
                        string add = "";
                        foreach (KeyValuePair<String, Int32> kvp in dictHash)
                        {
                            add += kvp.Key + ",";
                            //Log.Log(LogType.FILE, LogLevel.INFORM, "Values : " + kvp.Key);
                        }

                        try
                        {
                            //string str = arr[dictHash["dateTimeOrigination"]].Trim('"');
                            string str = arr[4];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "str : " + str);
                            int unixtimestamp = int.Parse(str);
                            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixtimestamp);
                            r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Datetime : " + ex.Message);
                        }

                        try
                        {
                            //string str = arr[dictHash["dateTimeConnect"]].Trim('"');
                            string str = arr[47];
                            int unixtimestamp = int.Parse(str);
                            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixtimestamp);
                            r.CustomStr5 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 : " + ex.Message);
                        }

                        try
                        {
                            //string str = arr[dictHash["dateTimeDisconnect"]].Trim('"');
                            string str = arr[48];
                            int unixtimestamp = int.Parse(str);
                            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixtimestamp);
                            r.CustomStr6 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);

                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 : " + ex.Message);
                        }

                        try
                        {
                            //r.CustomInt1 = Convert.ToInt32(arr[dictHash["duration"]].Trim('"'));
                            r.CustomInt1 = Convert.ToInt32(arr[55]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + arr[55]);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 : " + ex.Message);
                            r.CustomInt1 = 0;
                        }

                        try
                        {
                            //r.CustomStr7 = arr[dictHash["callingPartyNumber"]].Trim('"');
                            r.CustomStr7 = arr[8].Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 : " + ex.Message);
                        }

                        try
                        {
                            //r.CustomStr8 = arr[dictHash["originalCalledPartyNumber"]].Trim('"');
                            r.CustomStr8 = arr[29].Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 : " + ex.Message);
                        }

                        try
                        {
                            //r.CustomStr9 = arr[dictHash["finalCalledPartyNumber"]].Trim('"');
                            r.CustomStr9 = arr[30].Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 : " + ex.Message);
                        }

                        try
                        {
                            //r.CustomStr3 = arr[dictHash["origIpv4v6Addr"]].Trim('"');
                            r.CustomStr3 = arr[80].Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 : " + ex.Message);
                        }

                        try
                        {
                            //r.CustomStr4 = arr[dictHash["destIpv4v6Addr"]].Trim('"');
                            r.CustomStr4 = arr[81].Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 : " + ex.Message);
                        }

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Start sending data.");
                        SetRecordData(r);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending data.");
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line startswith Unknown  ");
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ikinci kısım | " + ex.Message);
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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> reading local file! ");

                    fileStream.Seek(Position, SeekOrigin.Begin);
                    FileInfo fileInfo = new FileInfo(file);
                    Int64 fileLength = fileInfo.Length;
                    Byte[] byteArray = new Byte[3];
                    fileStream.Read(byteArray, 0, 3);
                    //if (byteArray[2] == 0)
                    //{
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

                        }

                        if ((line == sr.ReadLine()) != null)
                        {
                            RecordFields.currentFile = file;
                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " IsFileFinished() -->> " + file + " File is finished.");

                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " IsFileFinished() -->> Current File is " + file);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }//
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
                                        RecordFields.lineNumber = 0;
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
        //        /// 
        //        /// </summary>
        //        /// <param name="fileNameList"></param>
        //        /// <returns></returns>
        private List<String> SortFileNames(List<String> fileNameList)
        {
            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                _fileNameList.Add(fileNameList[i]);
                _fileNameList.Sort();
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            //foreach (string t in _fileNameList)
            //{
            //    Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            //}
            return _fileNameList;
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
    }
}
