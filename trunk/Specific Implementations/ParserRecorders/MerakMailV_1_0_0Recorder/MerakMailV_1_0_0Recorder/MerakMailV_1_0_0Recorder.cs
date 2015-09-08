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

namespace Parser
{
    public struct Fields
    {
        public int index;
        public long lineNumber;
        public string currentFile;

        public bool Flag;
        public string MailId;
        public string ComputerName;
        public string CustomStr1;
        public string CustomStr2;
        public string CustomStr3;
        public string CustomStr4;
        public string CustomStr5;
        public string CustomStr6;
        public string CustomStr7;
        public string CustomStr8;
        public string CustomStr9;
        public string CustomStr10;
        public string SourceName;
        public string EventType;
        public string EventCategory;
        public string FullLine;

        public Int32 CustomInt1;
        public Int32 CustomInt2;
        public Int32 CustomInt3;

        public bool authFlag;

        private bool str1;
        private bool str3;
        private bool str4;
        private bool str7;
        private bool str8;
        private bool str9;
    }

    public class MerakMailV_1_0_0Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private string[] mainArray = new[] { "" };


        public MerakMailV_1_0_0Recorder()//recorderName
            : base()
        {
            LogName = "MerakMailV_1_0_0Recorder";//recorderName***
            RecordFields = new Fields();
        } // 

        public override void Init()
        {
            GetFiles();
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
            ////WriteMessage("Start recorder");
        } // Init

        public override void Start()
        {
            base.Start();
        } // Start

        public MerakMailV_1_0_0Recorder(String fileName)//recorderName***
            : base(fileName)
        {

        }

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
            try
            {
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                    //WriteMessage(" ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnLocal();
                    fileNameList = SortFileNames(fileNameList);

                    

                    SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is successfully FINISHED");
                //WriteMessage(" ParseFileNameLocal() -->> is successfully FINISHED");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                //WriteMessage("ERROR ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
            }
        } // ParseFileNameLocal

        public override bool ParseSpecific(String line, bool dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);

                Rec rec = new Rec();

                rec.LogName = LogName;
                rec.Description = line;

                string mailId;

                if (!dontSend)
                {
                    if (!line.StartsWith("SYSTEM"))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            RecordFields.FullLine += line + "#/#/#/";
                            string[] lineArr = SpaceSplit(line, true);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | parts : " + lineArr.Length.ToString(CultureInfo.InvariantCulture));
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() | line : " + line);

                            if (lineArr.Length > 0)
                            {
                                rec.ComputerName = lineArr[0];
                                try
                                {
                                    string year = lineArr[5];
                                    string month = lineArr[4];
                                    string day = lineArr[3];
                                    string time = lineArr[6];
                                    string date = (year + "-" + month + "-" + day + " " + time);
                                    DateTime dt;
                                    dt = Convert.ToDateTime(date);
                                    rec.Datetime = dt.ToString(dateFormat);
                                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseSpecific() Datetime: " + rec.Datetime);
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR,
                                            " ParseSpecific() Datetime Connected ERROR : " + exception.Message);
                                    Log.Log(LogType.FILE, LogLevel.ERROR,
                                            " ParseSpecific() Datetime Connected line : " + line);
                                    rec.Datetime = DateTime.Now.ToString(dateFormat);

                                }

                                mailId = Between(lineArr[1], "[", "]");
                                if (string.IsNullOrEmpty(RecordFields.MailId))
                                {
                                    if (lineArr.Length > 0)
                                    {
                                        RecordFields.MailId = Between(lineArr[1], "[", "]");
                                    }
                                }

                                else if (RecordFields.MailId != mailId)
                                {
                                    RecordFields.Flag = true;

                                    if (RecordFields.FullLine.Length > 3999)
                                    {
                                        rec.Description = RecordFields.FullLine.Substring(0, 3999);
                                    }
                                    else
                                    {
                                        rec.Description = RecordFields.FullLine;
                                    }
                                }

                                if (RecordFields.Flag)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() FullLine:  " + RecordFields.FullLine);
                                    mainArray = RecordFields.FullLine.Split(new string[] { "#/#/#/" }, StringSplitOptions.RemoveEmptyEntries);

                                    for (int i = 0; i < mainArray.Length; i++)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() mainArray " + i + " : " + mainArray[i]);
                                        rec = CoderParse(mainArray[i], rec);

                                        if (mainArray[i].Contains("AUTH") && mainArray[i].Contains("LOGIN"))
                                        {
                                            string[] spaceSplit1 = mainArray[i + 2].Split(' ');
                                            string[] spaceSplit2 = mainArray[i + 4].Split(' ');

                                            rec.CustomStr5 = "UserName: " +
                                                             base64Decode(spaceSplit1[spaceSplit1.Length - 1]);
                                            rec.CustomStr6 = "Password: " +
                                                             base64Decode(spaceSplit2[spaceSplit2.Length - 1]);
                                        }

                                    }


                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Parse Specific Description: " + rec.Description);
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Parse Specific Record is sending now.");
                                    rec.SourceName = FileName;
                                    SetRecordData(rec);
                                    SetStartPosition();
                                    mainArray = new[] { "" };
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Parse Specific Record sended.");
                                }
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                return true;
            }

            return true;
        } // ParseSpecific

        public string base64Decode(string data)
        {
            string returnString = "";
            try
            {
                UTF8Encoding encoder = new System.Text.UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                returnString = result;
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Error in base64Decode" + e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "Error in base64Decode" + data);
            }
            return returnString;
        }

        public Rec CoderParse(string line, Rec rec)
        {
            try
            {
                string[] array1 = SpaceSplit(line, true);

                if (array1.Length > 3)
                {
                    rec.ComputerName = array1[0];
                }

                try
                {
                    if (line.ToUpper().Contains("EHLO"))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() EHLO : ");
                        rec.CustomStr7 = After(line, "EHLO");
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                " ParseSpecific() CustomStr7: " + rec.CustomStr7);
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR,
                            " ParseSpecific() CustomStr7: ERROR " + exception.ToString());
                }

                for (int i = 0; i < array1.Length; i++)
                {
                    if (array1[i].Contains("220"))
                    {
                        rec.EventCategory = array1[i + 1];
                    }
                }


                if (line.Contains("Authentication") && line.Contains("failed"))
                {
                    rec.EventType = array1[array1.Length - 2] + " " + array1[array1.Length - 1];
                }


                if (line.ToLower().Contains("from"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() MAIL FROM : ");

                    rec.CustomStr3 = Between(line, "From:<", "> ");
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            " ParseSpecific() CustomStr3: " + rec.CustomStr3);
                }

                else if (line.ToLower().Contains("rcpt"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() RCPT TO : ");

                    rec.CustomStr4 = Between(line, ":<", ">");

                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            " ParseSpecific() CustomStr4: " + rec.CustomStr4);
                }

                if (line.ToLower().Contains("size"))
                {
                    rec.CustomInt2 = Convert.ToInt32(Between(line, "SIZE=", " "));
                }

                if (line.Contains("***"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() *** : ");

                    try
                    {

                        if (array1.Length > 14)
                        {
                            if (!String.IsNullOrEmpty(array1[14]))
                            {
                                rec.CustomInt1 = Convert.ToInt32(array1[14]);
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                        " ParseSpecific() CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            if (array1.Length > 12)
                            {
                                if (!String.IsNullOrEmpty(array1[12]))
                                {
                                    rec.CustomInt1 = Convert.ToInt32(array1[14]);
                                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseSpecific() CustomInt1: " + rec.CustomInt1.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                        }
                        catch (Exception exception1)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR,
                                           " ParseSpecific() CustomInt1: " + exception.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR,
                       " ParseSpecific() CustomInt1: line " + line);
                        }
                    }

                }

                if (line.ToLower().Contains("sender ok"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() sender ok ");

                    rec.CustomStr8 = After(line, "...");

                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            " ParseSpecific() CustomStr8: " + rec.CustomStr8);
                }

                if (line.ToLower().Contains("recipient ok"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() Recipient ok ");

                    rec.CustomStr9 = After(line, "...");
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            " ParseSpecific() CustomStr9: " + rec.CustomStr9);
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR,
                              " CoderParse() Error:  " + exception.Message);
            }
            return rec;
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

        public void SetStartPosition()
        {
            RecordFields.MailId = null;
            RecordFields.ComputerName = null;
            RecordFields.CustomStr1 = null;
            RecordFields.CustomStr3 = null;
            RecordFields.CustomStr4 = null;
            RecordFields.CustomStr6 = null;
            RecordFields.CustomStr7 = null;
            RecordFields.CustomStr8 = null;
            RecordFields.CustomStr9 = null;
            RecordFields.CustomStr10 = null;
            RecordFields.CustomInt1 = 0;
            RecordFields.Flag = false;
            RecordFields.FullLine = "";
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific()-----------------------------New Mail Ended.");
        }//SetStartPosition

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
                            //WriteMessage(file + "is finished.");
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
                //WriteMessage("GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                //WriteMessage("ERROR GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnLocal

        ///// <summary>
        ///// Gets the file names on the given directory
        ///// </summary>
        ///// <returns>Returned file names</returns>
        //private List<String> GetFileNamesOnRemote()
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is STARTED ");

        //        String line = "";
        //        String stdOut = "";
        //        String stdErr = "";

        //        String command = "ls -lt " + Dir;//FileNames contains what.*** fileNameFilter
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> SSH command : " + command);

        //        se.Connect();
        //        se.RunCommand(command, ref stdOut, ref stdErr);
        //        se.Close();

        //        StringReader stringReader = new StringReader(stdOut);
        //        List<String> fileNameList = new List<String>();
        //        Boolean foundAnyFile = false;

        //        while ((line = stringReader.ReadLine()) != null)
        //        {
        //            String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            fileNameList.Add(arr[arr.Length - 1]);
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> File name is read: " + arr[arr.Length - 1]);
        //            foundAnyFile = true;
        //        }

        //        if (!foundAnyFile)
        //        {
        //            Log.Log(LogType.FILE, LogLevel.ERROR, "GetFileNamesOnRemote() -->> There is no proper file in directory");
        //        }

        //        Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is successfully FINISHED");
        //        return fileNameList;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnRemote() -->> An error occurred :" + ex.ToString());
        //        return null;
        //    }
        //} // GetFileNamesOnRemote


        //public void LogFileSizeControl(string path)
        //{
        //    FileInfo fInfo = new FileInfo(path);

        //    //if (fInfo.Length > 25242880)
        //    if (fInfo.Length > 52339138)
        //    {
        //    }

        //    Log.Log(LogType.FILE, LogLevel.INFORM, " LogFileSizeControl() -->> LogFile Deleted: " + path);
        //}

        private void SetLastFile(List<string> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.index);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> lastFile : " + lastFile);
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
                                    RecordFields.index = dictionary[key];
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                            FileName, new int[0]);
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                            RecordFields.index.ToString(CultureInfo.InvariantCulture));

                                    if ((RecordFields.index + 1) != list.Count)
                                    {
                                        FileName = Dir + list[RecordFields.index + 1].ToString();
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.index + 1].ToString(), " Value ", RecordFields.index }), new int[0]);
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName);
                                        RecordFields.index = RecordFields.index + 1;
                                        RecordFields.lineNumber = 0;
                                        RecordFields.currentFile = lastFile;

                                    
                                        if (tempCustomVar1.Contains(","))
                                        {
                                            string[] tempArray = tempCustomVar1.Split(',');
                                        }
                                    }
                                    else
                                    {
                                        //Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. ");

                                        if (tempCustomVar1.Contains(","))
                                        {
                                            string[] tempArray = tempCustomVar1.Split(',');
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
                            FileName = Dir + fileNameList[0];
                            Position = 0;
                            lastLine = "";
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
                        }
                    }
                    else
                    {
                        FileName = Dir + fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
                        //RecordFields.currentFile = lastFile;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile, new int[0]);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
                    //WriteMessage("ERROR SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
                //WriteMessage("ERROR SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile

        ///<summary>
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
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);


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
        //
        private List<String> SortFileNames(List<String> fileNameList)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            List<string> sortFileNames = new List<string>();

            for (int i = 0; i < fileNameList.Count; i++)
            {
                if (!string.IsNullOrEmpty(fileNameList[i]))
                {
                    if (fileNameList[i].StartsWith("s"))
                    {
                        sortFileNames.Add(fileNameList[i]);
                    }
                }
            }

            foreach (string t in sortFileNames)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
                //WriteMessage(" SortFileNames() " + t);
            }
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
