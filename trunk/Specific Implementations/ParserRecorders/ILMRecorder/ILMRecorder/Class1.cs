﻿using System;
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
    }

    public class ILMRecorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private int num;
        private Fields RecordFields;


        //MSGTRKM20120105-1.LOG
        public ILMRecorder()//recorderName
            : base()
        {
            LogName = "ILMRecorder";//recorderName***
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

        // dsfd
        public ILMRecorder(String fileName)//recorderName***
            : base(fileName)
        {

        } // ILMRecorder

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

        public override bool ParseSpecific(String line, Boolean dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is : " + line);
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

                    Rec rec = new Rec();
                    rec.LogName = LogName;
                    if (!String.IsNullOrEmpty(remoteHost))
                        rec.ComputerName = remoteHost;
                    else
                        rec.ComputerName = Environment.MachineName;

                    if (!line.StartsWith("#"))
                    {
                        CoderParse(line, ref rec);
                        if (line.Length < 900)
                        {
                            if (!line.StartsWith("#"))
                            {
                                rec.Description = line;
                            }
                        }

                        else
                        {
                            if (!line.StartsWith("#"))
                            {
                                rec.Description = line.Substring(0, 899);
                            }

                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> coderparsedan gelen : " + rec.Datetime);
                        DateTime temp;
                        if (string.IsNullOrEmpty(rec.Datetime))
                        {
                            string[] parts = lastFile.Split('\\');
                            string filename = parts[parts.Length - 1];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> FileName : " + filename);
                            int splitIndex = filename.IndexOf("20");//valid for about 90 years
                            temp = new DateTime(Convert.ToInt32(filename.Substring(splitIndex, 4)),
                              Convert.ToInt32(filename.Substring(splitIndex + 4, 2)), Convert.ToInt32(filename.Substring(splitIndex + 6, 2)));
                            rec.Datetime = temp.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        CultureInfo p = CultureInfo.InvariantCulture;
                        if (!DateTime.TryParse(rec.Datetime, p, DateTimeStyles.AdjustToUniversal, out temp))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> unable to parse datetime format : " + rec.Datetime);

                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> temp : " + temp);
                        rec.Datetime = string.Format(temp.ToString("yyyy-MM-dd HH:mm:ss"));
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> datetime is : " + rec.Datetime);
                        SetRecordData(rec);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully FINISHED. ");
                    }

                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is unrecognized. ");
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
                if (remoteHost != "")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> remote host is " + remoteHost);

                    if (readMethod == "nread")
                    {
                        commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> CommandRead For nread Is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> CommandRead returned strOut : " + stdOut);

                        stReader = new StringReader(stdOut);
                        while ((line = stReader.ReadLine()) != null)
                        {
                            if (line.StartsWith("~?`Position"))
                            {
                                continue;
                            }
                            lineCount++;
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> Will read line count is : " + lineCount);
                    }
                    else
                    {
                        commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> CommandRead For nread is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> CommandRead returned strOut : " + stdOut);

                        stReader = new StringReader(stdOut);

                        while ((line = stReader.ReadLine()) != null)
                        {
                            lineCount++;
                        }
                    }

                    if (lineCount > 1)
                        return false;
                    else
                        return true;
                }
                else
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
                        if (byteArray[2] == 0)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                            return true;
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return false. ");
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

        /// <summary>
        /// Select the suitable last file in the file name list
        /// </summary>
        /// <param name="fileNameList">The file names are in the stored directory</param>
        //private void SetLastFile(List<String> fileNameList)
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);

        //        //Onur
        //        for (int i = 0; i < fileNameList.Count; i++)
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> Debug, FileNameList is : " + fileNameList[i].ToString());
        //        }

        //        if (fileNameList != null)
        //        {
        //            if (fileNameList.Count > 0)
        //            {
        //                if (!String.IsNullOrEmpty(lastFile))
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile);
        //                    String fileShortName = lastFile.Replace(Dir, "");
        //                    if (fileNameList.Contains(fileShortName))
        //                    {
        //                        if (IsFileFinished(lastFile))
        //                        {
        //                            Int32 lastFileIndex = fileNameList.BinarySearch(lastFile);
        //                            if (lastFileIndex + 1 == fileNameList.Count)
        //                            {
        //                                FileName = lastFile;
        //                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName);
        //                            }
        //                            else
        //                            {
        //                                FileName = Dir + fileNameList[lastFileIndex + 2].ToString();
        //                                lastFile = FileName;
        //                                Position = 0;
        //                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            FileName = lastFile;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile);
        //                        FileName = Dir + fileNameList[fileNameList.Count - 1];
        //                        lastFile = FileName;
        //                        Position = 0;
        //                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName);
        //                    }
        //                }
        //                else
        //                {
        //                    FileName = Dir + fileNameList[0];
        //                    lastFile = FileName;
        //                    Position = 0;
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile);
        //                }
        //            }
        //            else
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
        //            }
        //        }
        //        else
        //        {
        //            Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + ex.ToString());
        //    }
        //} // SetLastFile
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
                            if (this.IsFileFinished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
                                //int num2 = fileNameList.BinarySearch(lastFile);
                                //for (num = 0; num < list.Count; num++)
                                //{
                                //    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> FileName : " + list[num].ToString(), new int[0]);
                                //}

                                string key = item;
                                if (dictionary.ContainsKey(key))
                                {

                                    RecordFields.num3 = dictionary[key];
                                    //FileName = lastFile;
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
                                    }
                                    else
                                    {
                                        //FileName = lastFile;
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
                            //FileName = Dir + fileNameList[fileNameList.Count - 1];
                            FileName = Dir + fileNameList[fileNameList.Count + 1];
                            lastFile = FileName;
                            //Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
                        }
                    }
                    else
                    {
                        FileName = Dir + fileNameList[0];
                        lastFile = FileName;
                        //Position = 0;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile

        private int CoderParse(String line, ref CustomTools.CustomBase.Rec rec)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is : " + line + "");
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Arraylist fill started.");
                try
                {
                    string[] arr;
                    if (!line.StartsWith("#"))
                    {
                        //if (line.Length > 899)
                        {
                            arr = line.Split('-');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("{"))
                                {
                                    rec.EventCategory = arr[i].Replace("{", " ").Replace("}", " ");
                                }
                                if (arr[i].StartsWith("<"))
                                {
                                    rec.CustomStr2 = arr[i].Replace("<", " ").Replace(">", " ");
                                }
                                
                                if (arr[i].Contains("Sicil:"))
                                {
                                    try
                                    {
                                        rec.CustomInt6 = Convert.ToInt32(arr[i].Split(':')[1]);
                                    }
                                    catch (Exception)
                                    {
                                        rec.CustomInt6 = 0;
                                    }
                                }
                            }

                            DateTime dt = Convert.ToDateTime(arr[4]);
                            rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> datetime" + rec.Datetime);
                            rec.CustomStr1 = arr[6];
                            rec.EventType = Between(line, "->", "Sicil");
                            rec.Description = line;
                            
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime" + dt.ToString("yyyy-MM-dd HH:mm:ss"));
                            
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime" + rec.Datetime);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr1" + rec.CustomStr1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> line is unrecognized, unable to parse!" + ex.StackTrace);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
                return 1;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> " + ex.StackTrace);
                return 0;
            }
        } // CoderParse

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
            for (int i = 0; i < fileNameList.Count; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + fileNameList[i]);    
            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            //try
            //{

            //    List<String> _fileNameList = new List<String>();
            //    List<String> _fileNameListNew = new List<String>();
            //    FileNameProp fileNameProp = new FileNameProp();
            //    FileNameProp[] fileNamePropArray;
            //    long[] fileNameindexes = new long[fileNameList.Count];


            //    if (_fileNameFilters != null)
            //    {
            //        foreach (String fileName in fileNameList)
            //        {
            //            foreach (String fileNamefilter in _fileNameFilters)
            //            {
            //                fileName.Contains(fileNamefilter);
            //                _fileNameList.Add(fileName);
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        _fileNameList = fileNameList;
            //    }
            //    fileNamePropArray = new FileNameProp[_fileNameList.Count];

            //    int k = 0;
            //    foreach (String line in _fileNameList)
            //    {
            //        fileNameProp = new FileNameProp();
            //        fileNameProp.FileName = line;

            //        String[] subLine0 = line.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            //        fileNameProp.FNGenericPart = subLine0[0];

            //        int splitIndex = subLine0[0].IndexOf("20");//valid for about 90 years

            //        fileNameProp.FNYearPart = subLine0[0].Substring(splitIndex, 4);
            //        fileNameProp.FNMonthPart = subLine0[0].Substring(splitIndex + 4, 2);
            //        fileNameProp.FNDayPart = subLine0[0].Substring(splitIndex + 6, 2);
            //        fileNameProp.FNNumberPart = subLine0[1];
            //        SetFileNumber(ref fileNameProp);
            //        fileNamePropArray[k] = fileNameProp;
            //        fileNameindexes[k] = fileNameProp.FileNumber;
            //        k++;
            //    }

            //    if (fileNamePropArray[0].FileNumber == 0)
            //    {
            //        Array.Sort(fileNamePropArray, new FileNamePropComparerByGenericPart());
            //    }
            //    else
            //    {
            //        Array.Sort(fileNameindexes, fileNamePropArray);
            //    }

            //    _fileNameList.Clear();
            //    foreach (FileNameProp item in fileNamePropArray)
            //    {
            //        string fileName = item.FileName;
            //        if (string.IsNullOrEmpty(tempCustomVar1) || tempCustomVar1 == "MSGTRK")
            //        {
            //            if (item.FileName.StartsWith("MSGTRK") && !item.FileName.StartsWith("MSGTRKM"))
            //            {
            //                _fileNameList.Add(fileName);
            //                _fileNameListNew.Add(fileName);
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
            //            }
            //        }
            //        else if (tempCustomVar1 == "MSGTRKM")
            //        {
            //            if (item.FileName.StartsWith("MSGTRKM"))
            //            {
            //                _fileNameList.Add(fileName);
            //                _fileNameListNew.Add(fileName);
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
            //            }
            //        }
            //        //Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
            //    }

            //    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is successfully FINISHED.");
            //    for (int i = 0; i < _fileNameList.Count; i++)
            //    {
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> FileNames : " + _fileNameList[i].ToString());
            //    }
            //    return fileNameList;
            //}
            //catch (Exception ex)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, " SortFileNames() -->> An error occurred" + ex.ToString());
            //    return null;
            //}

            //  foreach (FileNameProp item in fileNamePropArray)
            //  {
            //    _fileNameList.Add(item.FileName);
            //    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
            //  }

            //  Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is successfully FINISHED.");
            //  return _fileNameList;
            //}
            //catch (Exception ex)
            //{
            //  Log.Log(LogType.FILE, LogLevel.ERROR, " SortFileNames() -->> An error occurred" + ex.ToString());
            //  return null;
            //}

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
    }
}

//
