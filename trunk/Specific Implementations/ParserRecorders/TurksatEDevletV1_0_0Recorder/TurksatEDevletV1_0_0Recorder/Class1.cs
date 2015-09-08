﻿//TurksatEDevletV1_0_0Recorder

//TurksatDemoF5Recorder



using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsFolderFinished;
        public string fileName;
        public long lineNumber;
        public string currentFile;
        public string[] tempArray;
        public string ErrorLine;
        public bool ErrorFlag;
        public bool InfoFlag;
    }

    public class TurksatEDevletV1_0_0Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private int num;
        private Fields RecordFields;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";


        public TurksatEDevletV1_0_0Recorder()
            : base()
        {
            LogName = "TurksatEDevletV1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = false;
            RecordFields.ErrorFlag = false;
            RecordFields.InfoFlag = false;
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

        // dsfd
        public TurksatEDevletV1_0_0Recorder(String fileName)
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
        public static byte[] ConvertToByteArray(string input)
        {
            return input.Select(Convert.ToByte).ToArray();
        }

        /// <summary>
        /// Converts a byte array to a string
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>The string</returns>
        public static string ConvertToString(byte[] bytes)
        {
            return new string(bytes.Select(Convert.ToChar).ToArray());
        }

        public override bool ParseSpecific(String line, Boolean dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : '" + line + "'");

                if (string.IsNullOrEmpty(line))
                {
                    return false;
                }

                if (line == "")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is empty");
                    return true;
                }
                if (!dontSend)
                {
                    if (IsSkipKeyWord(line))
                    {
                        return true;
                    }

                    try
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Parse will date: ");



                        string[] lineArr = SpaceSplit(line, false);
                        //DateTime dt;
                        //string myDateTimeString = lineArr[0] + " " + lineArr[1].Split(',')[0];
                        //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> myDateTimeString: " + myDateTimeString);
                        //dt = Convert.ToDateTime(myDateTimeString);

                        Rec rec = new Rec();
                        rec.LogName = LogName;
                        if (!String.IsNullOrEmpty(remoteHost))
                            rec.ComputerName = remoteHost;
                        else
                            rec.ComputerName = Environment.MachineName;

                        if (line.Length < 4000)
                        {
                            rec.Description = line;
                        }

                        else
                        {
                            rec.Description = line.Substring(0, 3999);
                            if (line.Length - 4000 > 1999)
                            {
                                rec.CustomStr10 = line.Substring(4000, line.Length - 4000);
                            }
                        }

                        rec.Datetime = DateTime.Now.ToString(dateFormat);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Datetime: " + rec.Datetime);

                        if (line.Contains("-"))
                        {
                            string[] lineAf = line.Split('-');
                            if (lineAf[0].ToString() == "2012")
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Sending data. ");
                                SetRecordData(rec);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Send data. ");
                            }
                            else
                            {
                                return false;
                            }
                        }


                        //if (lineArr[2] == "INFO")
                        //{
                        //    RecordFields.InfoFlag = true;
                        //    rec.EventCategory = lineArr[2];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> EventCategory: " + rec.EventCategory);

                        //    rec.CustomStr1 = lineArr[3];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr1: " + rec.CustomStr1);

                        //    rec.CustomStr2 = lineArr[4];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr2: " + rec.CustomStr2);

                        //    string pipeLine = Between(line, "[#", "#]");

                        //    string[] pipeLineArr = pipeLine.Split('|');

                        //    rec.EventType = pipeLineArr[0];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> EventType: " + rec.EventType);

                        //    rec.CustomStr3 = pipeLineArr[4];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr3: " + rec.CustomStr3);

                        //    rec.CustomStr4 = pipeLineArr[2];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr4: " + rec.CustomStr4);

                        //    rec.CustomStr5 = pipeLineArr[5];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr5: " + rec.CustomStr5);

                        //    rec.CustomStr6 = pipeLineArr[6];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr6: " + rec.CustomStr6);

                        //    rec.CustomStr7 = pipeLineArr[7];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr7: " + rec.CustomStr7);

                        //    rec.CustomStr8 = pipeLineArr[3];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr8: " + rec.CustomStr8);

                        //    rec.CustomStr9 = pipeLineArr[1];
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr9: " + rec.CustomStr9);

                        //    //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Sending data. ");
                        //    //SetRecordData(rec);
                        //    //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Send data. ");
                        //}

                        //else if (lineArr[2] == "ERROR")
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> Start error. ");
                        //    if (!string.IsNullOrEmpty(line))
                        //    {
                        //        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> line bos değil.");
                        //        if (line.Trim().StartsWith("at") || line.Trim().StartsWith("Caused"))
                        //        {
                        //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> Lİne starts with ' ' " );
                        //            RecordFields.ErrorLine += line;
                        //            RecordFields.ErrorFlag = false;
                        //        }
                        //        else
                        //        {
                        //            RecordFields.ErrorFlag = true;

                        //        }

                        //        if (RecordFields.ErrorFlag)
                        //        {

                        //            string[] errorLine = SpaceSplit(RecordFields.ErrorLine, false);
                        //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> ErrorLine: " + RecordFields.ErrorLine);

                        //            rec.EventCategory = errorLine[2];
                        //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> EventCategory: " + rec.EventCategory);

                        //            rec.CustomStr1 = errorLine[3];
                        //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> CustomStr1: " + rec.CustomStr1);

                        //            rec.CustomStr2 = errorLine[4];
                        //            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr2: " + rec.CustomStr2);

                        //            //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Sending data. ");
                        //            //SetRecordData(rec);
                        //            //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Send data. ");
                        //        }
                        //        #region eski
                        //        //string[] validLineControl = SpaceSplit(line, false);
                        //        //if (validLineControl.Length > 1)
                        //        {
                        //            //if (validLineControl[1].Contains(","))
                        //            {
                        //                //string tempDate = validLineControl[0] + " " + validLineControl[1].Split(',')[0];
                        //                //if (ValidDate(tempDate))
                        //                {
                        //                    RecordFields.ErrorFlag = false;
                        //                }
                        //                //else
                        //                {
                        //                    RecordFields.ErrorFlag = true;
                        //                }


                        //            }
                        //        }
                        //        #endregion
                        //    }

                        //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> ErrorLine: " + RecordFields.ErrorLine);
                        //}

                        //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Sending data. ");
                        //SetRecordData(rec);
                        //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Send data. ");
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> Send data exception. " + exception.Message);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> An error occurred : " + ex.ToString());
                return false;
            }
        } // ParseSpecific

        public bool ValidDate(string date)
        {
            try
            {
                DateTime dt1;
                dt1 = Convert.ToDateTime(date);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        } // ValidDate

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
    }
}

//