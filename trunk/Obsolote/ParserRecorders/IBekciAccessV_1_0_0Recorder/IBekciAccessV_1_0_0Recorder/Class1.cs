
/*
IBekciAccess Recorder 
* Updated by Onur SARIKAYA
* 09.07.2012 
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

    public class IBekciAccessV_1_0_0Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;

        public IBekciAccessV_1_0_0Recorder()//recorderName
            : base()
        {
            LogName = "IBekciAccessV_1_0_0Recorder";//recorderName***
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

        public IBekciAccessV_1_0_0Recorder(String fileName)//recorderName***
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

            if (line == "")
                return true;

            String[] arr = SpaceSplit(line, false);
            ArrayList arrList = new ArrayList();
            for (int i = 0; i < arr.Length; i++)
            {
                if (!string.IsNullOrEmpty(arr[i]))
                {
                    arrList.Add(arr[i].Trim());
                }
            }

            try
            {
                Rec r = new Rec();
                DateTime dt;
                dt = DateTime.Now;
                string myDateTimeString = arr[0] + arr[1] + "," + dt.Year + "," + arr[2].Split(':')[0] + ":" +
                                          arr[2].Split(':')[1] + ":" + arr[2].Split(':')[2].Split('.')[0];
                dt = Convert.ToDateTime(myDateTimeString);
                r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                for (int i = 0; i < arrList.Count; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ArrList : " + arrList[i]);
                }
                try
                {
                    r.CustomInt1 = Convert.ToInt32(arr[4].Replace("-", " ").Trim());
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + r.CustomInt1);
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt1 = 0 : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + arr[4]);
                }

                try
                {
                    r.CustomInt2 = Convert.ToInt32(arr[6].Split('/')[0].Trim());
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + arr[6]);
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt2 = 0 : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + arr[6]);
                }

                try
                {
                    r.CustomInt3 = Convert.ToInt32(arr[11].Split('.')[4].Trim());
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + r.CustomInt3);
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt3 = 0 : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + arr[11].Split('.')[4]);
                }

                try
                {
                    r.CustomInt4 = Convert.ToInt32(arr[13].Split('.')[4].Replace(":", " ").Trim());
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + r.CustomInt4);
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomInt4 = 0 : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + arr[13].Split('.')[4]);
                }

                r.EventType = arr[7];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                r.CustomStr1 = arr[8];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                r.CustomStr2 = arr[10];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
                r.CustomStr3 = arr[11].Split('.')[0] + "." + arr[11].Split('.')[1] + "." + arr[11].Split('.')[2] + "." + arr[11].Split('.')[3];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                r.CustomStr4 = arr[13].Split('.')[0] + "." + arr[13].Split('.')[1] + "." + arr[13].Split('.')[2] + "." + arr[13].Split('.')[3];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                r.CustomStr7 = arr[14];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                r.LogName = LogName;

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

                string isBlock = "";
                string isDelete = "";

                if (tempCustomVar1.Contains(","))
                {
                    string[] tempArray = tempCustomVar1.Split(',');
                    isBlock = tempArray[0] + "";
                }
                else
                {
                    if (tempCustomVar1.ToLower() == "block")
                    {
                        isBlock = tempCustomVar1;
                    }
                }

                if (!string.IsNullOrEmpty(isBlock))
                {
                    if (isBlock == "block")
                    {
                        if (r.EventType == "block")
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Record is sending now.");
                            SetRecordData(r);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
                        }
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record is sending now.");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
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
                            //MessageBox.Show(line);
                        }

                        if ((line == sr.ReadLine()) != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
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

                                if (idx < fileNameList.Count)
                                {
                                    FileName = fileNameList[idx];
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Onur--- : " + FileName + " Value " + idx);
                                    lastFile = FileName;
                                    Position = 0;
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. New file is  : " + FileName);

                                    string isDelete = "";

                                    if (tempCustomVar1.Contains(","))
                                    {
                                        string[] tempArray = tempCustomVar1.Split(',');
                                        isDelete = tempArray[1];
                                    }
                                    else
                                    {
                                        isDelete = tempCustomVar1;
                                    }


                                    if (!string.IsNullOrEmpty(isDelete))
                                    {
                                        if (isDelete.ToLower() == "delete")
                                        {
                                            for (int i = 0; i < idx; i++)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Deleting file " + fileNameList[i]);
                                                File.Delete(fileNameList[i]);
                                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Deleted file " + fileNameList[i]);
                                            }
                                        }
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
                            idx = -1;
                            while (++idx < fileNameList.Count && lastFile.CompareTo(fileNameList[idx]) <= 0)
                            {

                            }
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

        private List<String> SortFileNames(List<String> fileNameList)
        {

            List<string> sortFileNames = new List<string>();

            for (int i = 0; i < fileNameList.Count; i++)
            {
                if (!string.IsNullOrEmpty(fileNameList[i]))
                {
                    if (fileNameList[i].Split('.')[1] == "kyt")
                    {
                        if (!fileNameList[i].ToLower().StartsWith("aa") && !fileNameList[i].ToLower().StartsWith("web"))
                        {
                            sortFileNames.Add(fileNameList[i]);
                        }
                    }
                }
            }

            foreach (string t in sortFileNames)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + t);
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            return sortFileNames;
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
