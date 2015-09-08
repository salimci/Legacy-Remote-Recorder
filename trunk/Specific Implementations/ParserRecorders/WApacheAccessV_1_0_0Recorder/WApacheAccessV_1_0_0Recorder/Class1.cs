/*
 Tamam
 */

using System;
using System.Collections.Generic;
using System.Net;
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
        public int num3;
        public long lineNumber;
        public string currentFile;
        public long totalLineCountinFile;
        public string[] tempArray;
    }

    public class WApacheAccessV_1_0_0Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private Fields RecordFields;
        public WApacheAccessV_1_0_0Recorder()
            : base()
        {
            LogName = "WApacheAccessV_1_0_0Recorder";
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

        public WApacheAccessV_1_0_0Recorder(String fileName)//recorderName***
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

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            //if (string.IsNullOrEmpty(line))
            //{
            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
            //    return true;
            //}

            if (!dontSend)
            {
                string[] strArray = line.Split('-');
                ArrayList list = new ArrayList();
                try
                {
                    int num;
                    CustomBase.Rec rec = new CustomBase.Rec();

                    for (num = 0; num < strArray.Length; num++)
                    {
                        if (!string.IsNullOrEmpty(strArray[num].Trim()))
                        {
                            list.Add(strArray[num]);
                        }
                    }

                    rec.CustomStr3 = list[0].ToString().Trim();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                    IPAddress ip_adres = null;
                    bool check = IPAddress.TryParse(rec.CustomStr3, out ip_adres);

                    if (!check)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is not begin with a valid ip address");
                        return true;
                    }

                    if (string.IsNullOrEmpty(remoteHost) == false)
                        rec.ComputerName = remoteHost;

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                    rec.LogName = LogName;
                    rec.CustomStr10 = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);

                    if (list.Count > 1)
                    {
                        string[] strArray2 = list[1].ToString().Trim().Split('[');
                        if (!string.IsNullOrEmpty(strArray2[0]))
                        {
                            rec.UserName = strArray2[0];
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                        strArray2 = strArray2[1].Split(']');
                        string[] strArray3 = strArray2[0].Trim('[').Split(':');
                        rec.Datetime = Convert.ToDateTime(strArray3[0] + " " + strArray3[1] + ":" + strArray3[2] + ":" + strArray3[3]).ToString("yyyy/MM/dd HH:mm:ss");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                        strArray2 = strArray2[1].Trim().Split(' ');
                        rec.EventType = strArray2[0].Trim('"');
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                        bool flag = false;
                        string str = "";
                        for (num = 1; num < strArray2.Length; num++)
                        {
                            if (!strArray2[num].StartsWith("HTT"))
                            {
                                if (!flag)
                                {
                                    flag = true;
                                    rec.Description = rec.Description + " " + strArray2[num];
                                }
                                else
                                {
                                    str = str + " " + strArray2[num];
                                }
                            }
                            else
                            {
                                str = str + " " + strArray2[num];
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);
                        string[] strArray4 = str.Replace('-', ' ').Trim().Split(' ');
                        rec.CustomStr1 = strArray4[0];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                        if (strArray4.Length > 1)
                        {
                            rec.CustomInt1 = Convert.ToInt32(strArray4[1]);
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                        if (strArray4.Length > 2)
                        {
                            rec.CustomInt2 = Convert.ToInt32(strArray4[2]);
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                    }

                    try
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data: ");
                        SetRecordData(rec);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Finish sending Data: ");
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Sending Data Error: " + exception.Message);
                    }

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }
            }
            return true;
        }

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

        //Utils
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
        //private void SetLastFile(List<string> fileNameList)
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> lastFile : " + lastFile);
        //        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        //        ArrayList list = new ArrayList();

        //        for (int num = 0; num < fileNameList.Count; num++)
        //        {
        //            dictionary.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture), num);
        //            list.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture));
        //        }

        //        if (list.Count > 0)
        //        {
        //            if (!string.IsNullOrEmpty(lastFile))
        //            {
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile, new int[0]);
        //                string item = lastFile.Replace(Dir, "");
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> item is " + item);

        //                if (fileNameList.Contains(item))
        //                {
        //                    if (IsFileFinished(lastFile))
        //                    {
        //                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
        //                        string key = item;
        //                        if (dictionary.ContainsKey(key))
        //                        {
        //                            RecordFields.num3 = dictionary[key];
        //                            Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                    " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
        //                                    FileName, new int[0]);
        //                            Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                    " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
        //                                    RecordFields.num3.ToString(CultureInfo.InvariantCulture));

        //                            if ((RecordFields.num3 + 1) != list.Count)
        //                            {
        //                                FileName = Dir + list[RecordFields.num3 + 1].ToString();
        //                                Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.num3 + 1].ToString(), " Value ", RecordFields.num3 }), new int[0]);
        //                                lastFile = FileName;
        //                                Position = 0;
        //                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName, new int[0]);
        //                                RecordFields.num3 = RecordFields.num3 + 1;
        //                                RecordFields.lineNumber = 0;
        //                                RecordFields.currentFile = lastFile;
        //                            }
        //                            else
        //                            {
        //                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
        //                            }
        //                        }
        //                        RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
        //                    }
        //                    else
        //                    {
        //                        FileName = lastFile;
        //                        //RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
        //                    }
        //                }
        //                else
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
        //                    FileName = Dir + fileNameList[fileNameList.Count + 1];
        //                    lastFile = FileName;
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
        //                }
        //            }
        //            else
        //            {
        //                FileName = Dir + fileNameList[0];
        //                lastFile = FileName;
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
        //                RecordFields.currentFile = lastFile;
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile, new int[0]);
        //            }
        //        }
        //        else
        //        {
        //            Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
        //            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
        //    }
        //} // SetLastFile

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
                if (fileNameList[i].StartsWith("access_"))
                {
                    _fileNameList.Add(fileNameList[i]);
                }
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

//
