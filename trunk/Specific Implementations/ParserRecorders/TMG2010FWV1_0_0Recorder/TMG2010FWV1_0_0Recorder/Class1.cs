/*
 Onur sarıkaya
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Timers;
using Log;
using System.Collections;
using System.Globalization;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        public long lineNumber;
        public string currentFile;
        public long totalLineCountinFile;
        public string[] tempArray;
    }

    public class TMG2010FWV1_0_0Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        public TMG2010FWV1_0_0Recorder()
            : base()
        {
            LogName = "TMG2010FWV1_0_0Recorder";
            RecordFields = new Fields();
        }

        public override void Init()
        {
            GetFiles();
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init - LogName()" + LogName);
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
        } // Init

        public override void Start()
        {
            base.Start();
        } // Start

        public TMG2010FWV1_0_0Recorder(String fileName)//recorderName***
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

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

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

            String[] lineArr = line.Split(new Char[] { '	' }, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 0; i < lineArr.Length; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific lineArr" + i + ". " + lineArr[i]);
            }


            try
            {
                Rec r = new Rec();
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

                try
                {
                    DateTime dt;
                    string myDateTimeString = lineArr[1] + " " + lineArr[2];
                    dt = Convert.ToDateTime(myDateTimeString);
                    r.Datetime = dt.ToString(dateFormat);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "DateTime Parsing Error: " + exception.Message);
                }

                try
                {
                    r.SourceName = lineArr[6];
                    r.EventCategory = lineArr[3];
                    r.EventType = lineArr[9];
                    r.CustomStr1 = lineArr[7];
                    r.CustomStr2 = lineArr[8];

                    if (!lineArr[4].Contains("["))
                    {
                        r.CustomStr3 = lineArr[4].Split(':')[0];
                    }
                    else
                    {
                        r.CustomStr3 = Between(lineArr[4], "[", "]");
                    }

                    if (!lineArr[5].Contains("["))
                    {
                        r.CustomStr4 = lineArr[5].Split(':')[0];
                    }
                    else
                    {
                        r.CustomStr4 = Between(lineArr[5], "[", "]");
                    }

                    r.CustomStr5 = lineArr[6];
                    r.CustomStr6 = lineArr[12];



                    if (lineArr.Length > 19)
                    {
                        r.CustomStr7 = lineArr[19];
                    }
                    else
                    {
                        r.CustomStr7 = "";
                    }

                    r.CustomStr8 = lineArr[11];
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "String Parsing Error: " + exception.Message);
                }

                try
                {
                    if (!lineArr[4].Contains("["))
                    {
                        r.CustomInt3 = Convert.ToInt32(lineArr[4].Split(':')[1]);
                    }
                    else
                    {
                        r.CustomInt3 = Convert.ToInt32(After(lineArr[4], "]").Replace(':', ' ').Trim());
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 Cast Error: " + exception.Message);
                    r.CustomInt3 = 0;
                }

                try
                {
                    if (!lineArr[4].Contains("["))
                    {
                        r.CustomInt4 = Convert.ToInt32(lineArr[5].Split(':')[1]);
                    }
                    else
                    {
                        r.CustomInt4 = Convert.ToInt32(After(lineArr[5], "]").Replace(':', ' ').Trim());
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 Cast Error: " + exception.Message);
                    r.CustomInt4 = 0;
                }

                try
                {
                    r.CustomInt5 = Convert.ToInt32(lineArr[13]);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5 Cast Error: " + exception.Message);
                    r.CustomInt5 = 0;
                }

                try
                {
                    r.CustomInt6 = Convert.ToInt32(lineArr[14]);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 Cast Error: " + exception.Message);
                    r.CustomInt6 = 0;
                }

                try
                {
                    r.CustomInt7 = Convert.ToInt32(lineArr[15]);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7 Cast Error: " + exception.Message);
                    r.CustomInt7 = 0;
                }

                try
                {
                    r.CustomInt8 = Convert.ToInt32(lineArr[16]);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8 Cast Error: " + exception.Message);
                    r.CustomInt8 = 0;
                }

                #region Writelog

                if (!string.IsNullOrEmpty(r.EventType))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType" + r.EventType);
                }

                if (!string.IsNullOrEmpty(r.ComputerName))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName" + r.ComputerName);
                }

                if (!string.IsNullOrEmpty(r.CustomStr1))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1" + r.CustomStr1);
                }

                if (!string.IsNullOrEmpty(r.CustomStr2))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2" + r.CustomStr2);
                }

                if (!string.IsNullOrEmpty(r.CustomStr3))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3" + r.CustomStr3);
                }

                if (!string.IsNullOrEmpty(r.CustomStr4))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4" + r.CustomStr4);
                }

                if (!string.IsNullOrEmpty(r.CustomStr7))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7" + r.CustomStr7);
                }
                #endregion



                Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
                SetRecordData(r);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");

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

        private bool ValidIpV4(string ip)
        {
            string[] arrIP = ip.Split('.');
            if (arrIP.Length != 4)
            {
                return false;
            }
            byte a = 0;
            foreach (string item in arrIP)
            {
                if (!byte.TryParse(item, out a))
                {
                    return false;
                }
            }
            return true;
        } // 

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
        } // SpaceSplit

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

            RecordFields.currentFile = file;


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
                    //if (RecordFields.lineNumber == RecordFields.totalLineCountinFile)
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                    //    return true;
                    //}
                    //else
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                    //    return true;
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

        private bool ValidIpV6(string ipAdress)
        {
            IPAddress address;
            bool return_statement = false;
            if (IPAddress.TryParse(ipAdress, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        return_statement = true;
                        break;

                }
            }
            return return_statement;
        } // ValidIpV6

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

        private void SetLastFile(List<string> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);
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


                                        if (tempCustomVar1.Contains(","))
                                        {
                                            if (RecordFields.tempArray.Length != 0)
                                            {
                                                for (int i = 0; i < RecordFields.tempArray.Length; i++)
                                                {

                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
                                                    RecordFields.num3);
                                                    if (RecordFields.num3 == 0)
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                    }
                                                    else
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3 - 1]);
                                                    }
                                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. ");

                                        if (tempCustomVar1.Contains(","))
                                        {
                                            if (RecordFields.tempArray.Length != 0)
                                            {
                                                for (int i = 0; i < RecordFields.tempArray.Length; i++)
                                                {
                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
                                                    RecordFields.num3);
                                                    if (RecordFields.num3 == 0)
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                    }
                                                    else
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                    }
                                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                                }
                                            }
                                        }

                                    }
                                }
                                //RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
                            }
                            else
                            {
                                FileName = lastFile;
                                //RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
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