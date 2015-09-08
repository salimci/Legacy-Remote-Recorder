/*
 Haziran 2012 tarihinde Hüseyin Sevin tarafından istekte bulunulmuş ve Onur Sarıkaya tarafından AOÇ için geliştirilmiştir.
 * Loglar ftp ile mevcut log sunucusuna gönderilmekte ve dosyalar sıralanıp okunmaktadır.
 * 
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

    public class CoslatURLV1_1_1_Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;
        //MSGTRKM20120105-1.LOG
        public CoslatURLV1_1_1_Recorder()//recorderName
            : base()
        {
            LogName = "CoslatURLV1_1_1_Recorder";//recorderName***
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

        public CoslatURLV1_1_1_Recorder(String fileName)//recorderName***
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

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {
                se = new SshExec(remoteHost, user);
                //se. = password;
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | currentFile : " + RecordFields.currentFile);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | lineNumber : " + RecordFields.lineNumber);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Total lineNumber : " + CountLinesInFile(RecordFields.currentFile));


            if (Position != 0)
            {
                RecordFields.lineNumber++;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | lineNumber : " + RecordFields.lineNumber);
            }
            else if (Position == 0)
            {
                RecordFields.lineNumber = 0;
            }


            if (line == "")
                return true;

            String[] arr = line.Split(' ');
            ArrayList arrList = new ArrayList();
            for (int i = 0; i < arr.Length; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | arr " + i + "." + arr[i]);
            }
            //
            try
            {
                Rec r = new Rec();
                DateTime dt;
                dt = DateTime.Now;
                r.LogName = LogName;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == "GET")
                    {
                        if (arr[2].EndsWith(","))
                        {
                            string myDateTimeString = arr[2].Replace(',', ' ').Trim() + "/" + arr[0] + "/" + arr[3] + " " + arr[4];
                            dt = Convert.ToDateTime(myDateTimeString);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime 1 : " + myDateTimeString);
                            r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Datetime : " + r.Datetime);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime is not recognized. New Date is now.");
                            r.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                        }

                        r.EventType = arr[8];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                        r.CustomStr1 = arr[5];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                        r.CustomStr2 = arr[6];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
                        r.CustomStr3 = arr[7];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);


                        if (arr.Length > 8)
                        {
                            if (arr[9].Length > 899)
                            {
                                r.CustomStr8 = arr[9].Substring(0, 899);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                            }
                            else
                            {
                                r.CustomStr8 = arr[9];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr8 is null ");
                        }

                        if (arr.Length > 9)
                        {
                            if (arr[10].Length > 899)
                            {
                                r.CustomStr9 = arr[10].Substring(0, 899);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                            }
                            else
                            {
                                r.CustomStr9 = arr[10];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr9 is null ");
                        }
                    }

                    else if (arr[i] == "POST")
                    {
                        r.EventType = arr[5];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                        r.CustomStr2 = arr[1];


                        string myDateString = arr[2] + " " + arr[3];
                        dt = Convert.ToDateTime(myDateString);

                        r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);

                        r.CustomStr1 = arr[0];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);

                        r.CustomStr3 = arr[4];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                        r.CustomStr5 = RecordFields.currentFile;

                        if (arr.Length>5)
                        {
                            if (arr[6].Length > 899)
                            {
                                r.CustomStr8 = arr[6].Substring(0, 899);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                            }
                            else
                            {
                                r.CustomStr8 = arr[6];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 is null. " );
                        }

                        if (arr.Length > 6)
                        {
                            if (arr[7].Length > 899)
                            {
                                r.CustomStr9 = arr[7].Substring(0, 899);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                            }
                            else
                            {
                                r.CustomStr9 = arr[7];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr9 is null ");
                        }
                    }
                }

                if (line.Length > 899)
                {
                    r.Description = line.Substring(0, 899);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                }
                else
                {
                    r.Description = line;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                }

                if ((RecordFields.lineNumber != CountLinesInFile(RecordFields.currentFile)) && RecordFields.lineNumber <= CountLinesInFile(RecordFields.currentFile))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                return true;
            }
            
            return true;
        } // ParseSpecific

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
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> CountLinesInFile : " + CountLinesInFile(file).ToString());

                    if (RecordFields.lineNumber == CountLinesInFile(file))
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                        return true;
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                        return true;
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

                //se.Connect();
                //se.RunCommand(command, ref stdOut, ref stdErr);
                //se.Close();

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
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            foreach (string t in fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            }
            return fileNameList;
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

        static long CountLinesInFile(string f)
        {
            long count = 0;
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        } // CountLinesInFile


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

            public void newFunc(ref Parser.Rec Rec)
            {

            } // newFunc
        }
        // gdfgdfg
    }
}

//
