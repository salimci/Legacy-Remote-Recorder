﻿/*
 * Writer: Onur sarıkaya
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
        public string[] tempArray;
    }

    public class WatchguardV_5_0_0_WinFileRecorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        public WatchguardV_5_0_0_WinFileRecorder()//recorderName
            : base()
        {
            LogName = "WatchguardV_5_0_0_WinFileRecorder";//recorderName***
            RecordFields = new Fields();
        } // Exchange2010Recorder

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

        public WatchguardV_5_0_0_WinFileRecorder(String fileName)//recorderName***
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

            if (line == "")
                return true;

            String[] SpaceArray = SpaceSplit(line, true);
            String[] quotationArray = line.Split('"');
            String[] eventArr = quotationArray[0].Split('_');
            String[] eventArr2 = eventArr[0].Split(' ');

            {
                try
                {
                    Rec r = new Rec();
                    r.LogName = LogName;
                    r.CustomStr9 = FileName;
                    #region DateTime
                    try
                    {
                        DateTime dt;
                        dt = Convert.ToDateTime(SpaceArray[0] + " " + SpaceArray[1]);
                        string myDateTimeString = dt.ToString(dateFormat);
                        r.Datetime = myDateTimeString;
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "DateTime Error." + exception.Message);
                    }
                    #endregion

                    #region Description
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
                    #endregion


                    if (line.Contains("CONTEXT:"))
                    {
                        if (eventArr2[eventArr2.Length - 1] == "app")
                        {
                            //Tip 1
                            #region EventType
                            try
                            {
                                r.EventType = eventArr2[eventArr2.Length - 1];
                                if (!string.IsNullOrEmpty(r.EventType))
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType. " + r.EventType);
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "EventType Error." + exception.Message);
                            }
                            #endregion

                            #region STRParse
                            try
                            {
                                r.SourceName = quotationArray[1].Split('#')[1];
                                r.UserName = quotationArray[1].Split('#')[4];
                                r.CustomStr1 = quotationArray[1].Split('#')[0];
                                r.CustomStr3 = quotationArray[1].Split('#')[2];
                                r.CustomStr4 = quotationArray[1].Split('#')[3];
                                r.CustomStr5 = quotationArray[1].Split('#')[5];
                                r.CustomStr6 = quotationArray[1].Split('#')[quotationArray[1].Split('#').Length - 1];

                                try
                                {
                                    if (quotationArray[1].Split('#').Length == 8)
                                    {
                                        if (quotationArray[1].Split('#').Length > 6)
                                        {
                                            if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[6]))
                                            {
                                                r.CustomInt1 = Convert.ToInt32(quotationArray[1].Split('#')[6]);
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + r.CustomInt1);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = 0;
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 is null or string. CustomInt1 = 0 ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        r.CustomInt1 = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 is null or string. CustomInt1 = 0 ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 cast error. " + quotationArray[1].Split('#')[6]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 cast error. " + exception.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 1 Parsing Error." + ex.Message);

                            }
                            #endregion
                        }
                        else if (eventArr2[eventArr2.Length - 1] == "by")
                        {
                            //Tip 2
                            #region EventType
                            try
                            {
                                r.EventType = eventArr2[eventArr2.Length - 1] + "_" + eventArr[1];
                                if (!string.IsNullOrEmpty(r.EventType))
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType. " + r.EventType);
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "EventType Error." + exception.Message);
                            }
                            #endregion

                            #region STRParse
                            try
                            {
                                r.CustomStr1 = quotationArray[1].Split('#')[0];
                                r.CustomStr2 = quotationArray[1].Split('#')[2];
                                r.CustomStr3 = quotationArray[1].Split('#')[6];
                                r.CustomStr4 = quotationArray[1].Split('#')[4];

                                try
                                {
                                    if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[1]))
                                    {
                                        r.CustomInt1 = Convert.ToInt32(quotationArray[1].Split('#')[1]);
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + r.CustomInt1);
                                    }
                                    else
                                    {
                                        r.CustomInt1 = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 is null or string. CustomInt1 = 0 ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 cast error. " + quotationArray[1].Split('#')[1]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 cast error. " + exception.Message);
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[3]))
                                    {
                                        r.CustomInt2 = Convert.ToInt32(quotationArray[1].Split('#')[3]);
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + r.CustomInt2);
                                    }
                                    else
                                    {
                                        r.CustomInt2 = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 is null or string. CustomInt2 = 0 ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 cast error. " + quotationArray[1].Split('#')[3]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2 cast error. " + exception.Message);
                                }

                                try
                                {
                                    if (quotationArray[1].Split('#').Length > 5)
                                    {
                                        if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[5]))
                                        {
                                            r.CustomInt3 = Convert.ToInt32(quotationArray[1].Split('#')[5]);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + r.CustomInt3);
                                        }
                                        else
                                        {
                                            r.CustomInt3 = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 is null or string. CustomInt3 = 0 ");
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt3 is null. ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 cast error. " + quotationArray[1].Split('#')[5]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 cast error. " + exception.Message);
                                }

                                try
                                {
                                    if (quotationArray[1].Split('#').Length > 7)
                                    {
                                        if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[7]))
                                        {

                                            r.CustomInt4 = Convert.ToInt32(quotationArray[1].Split('#')[7]);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + r.CustomInt4);
                                        }
                                        else
                                        {
                                            r.CustomInt4 = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 is null or string. CustomInt4 = 0 ");
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt4 is null. ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 cast error. " + quotationArray[1].Split('#')[7]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 cast error. " + exception.Message);
                                }

                                try
                                {
                                    if (quotationArray[1].Split('#').Length > 8)
                                    {
                                        if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[8]))
                                        {
                                            r.CustomInt5 = Convert.ToInt32(quotationArray[1].Split('#')[8]);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + r.CustomInt5);
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5 is null or string. CustomInt5 = 0 ");
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt5 is null. ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 cast error. " + quotationArray[1].Split('#')[8]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 cast error. " + exception.Message);
                                }

                                try
                                {
                                    if (quotationArray[1].Split('#').Length > 9)
                                    {
                                        if (!string.IsNullOrEmpty(quotationArray[1].Split('#')[9]))
                                        {

                                            r.CustomInt6 = Convert.ToInt32(quotationArray[1].Split('#')[9]);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + r.CustomInt6);
                                        }
                                        else
                                        {
                                            r.CustomInt6 = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 is null or string. CustomInt6 = 0 ");
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt9 is null. ");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6 cast error. " + quotationArray[1].Split('#')[9]);
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6 cast error. " + exception.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 2 Error. " + ex.Message);

                            }
                            #endregion
                        }


                        Log.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + r.SourceName);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + r.UserName);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + r.CustomStr1);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + r.CustomStr3);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + r.CustomStr4);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + r.CustomStr5);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + r.CustomStr6);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + r.CustomInt1);
                    }

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
            }

            return true;
        } // ParseSpecific


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