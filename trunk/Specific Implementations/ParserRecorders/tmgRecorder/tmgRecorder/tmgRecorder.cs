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
    }


    public class tmgRecorder : Parser//recorderName***
    {
        String recorderName = "tmgRecorder";
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = new String[] { "ISALOG_" };
        Dictionary<String, Int32> _dayDictionaryEn;
        Dictionary<String, Int32> _dayDictionaryTr;
        Dictionary<String, Int32> _mounthDictionaryEn;
        Dictionary<String, Int32> _mounthDictionaryTr;
        private Fields RecordFields;

        public tmgRecorder()//recorderName
            : base()
        {
            LogName = "tmgRecorder";//recorderName***
        } // tmgRecorder

        public override void Init()
        {
            GetFiles();
        } // Init

        public override void Start()
        {
            base.Start();
        }

        public tmgRecorder(String fileName)//recorderName***
            : base(fileName)
        {

        } // tmgRecorder

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
                    Rec rec = new Rec();
                    rec.LogName = LogName;
                    if (!String.IsNullOrEmpty(remoteHost))
                        rec.ComputerName = remoteHost;
                    else
                        rec.ComputerName = Environment.MachineName;

                    if (line.Length < 900)
                        rec.Description = line;
                    else
                        rec.Description = line.Substring(0, 899);

                    if (IsSkipKeyWord(line))
                    {
                        return true;
                    }
                    CoderParse(line, ref rec);

                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully FINISHED. ");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> An error occurred : " + ex.ToString());
                return false;
            }
        } // ParseSpecific

        protected override void ParseFileNameLocal()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
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
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
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
                    //using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    //{
                    //    fileStream.Seek(Position, SeekOrigin.Begin);
                    //    FileInfo fileInfo = new FileInfo(file);
                    //    Int64 fileLength = fileInfo.Length;
                    //    Byte[] byteArray = new Byte[3];
                    //    fileStream.Read(byteArray, 0, 3);
                    //    if (byteArray[3] == 0)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }

                    //}
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
                return false;
            }
        } // IsFileFinished
        // fdsfdsf

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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Masaj: " + e.StackTrace);
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

        //fdsfsdf
        /// <summary>
        /// Parse the line
        /// </summary>
        /// <param name="line">Will parsing line</param>
        /// <param name="rec">Will recording record</param>
        private void CoderParse(String line, ref CustomTools.CustomBase.Rec rec)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
                Int32 Year = 1;
                Int32 Month = 1;
                Int32 Day = 1;
                Int32 Hour = 0;
                Int32 Minute = 0;
                Int32 Second = 0;

                if (!line.StartsWith("#"))
                {
                    String[] subLine0 = line.Split(new Char[] { '	' }, StringSplitOptions.RemoveEmptyEntries);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> line : " + line);
                    for (int i = 0; i < subLine0.Length; i++)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> subline0[" + i + "] : " + subLine0[i]);
                    }

                    rec.CustomStr3 = Convert.ToString(subLine0[0]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr3 : " + rec.CustomStr3);

                    rec.UserName = Convert.ToString(subLine0[1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> UserName : " + rec.UserName);

                    rec.Datetime = Convert.ToString(subLine0[3]) + " " + Convert.ToString(subLine0[4]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime : " + rec.Datetime);

                    rec.ComputerName = Convert.ToString(subLine0[5]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> ComputerName : " + rec.ComputerName);


                    rec.Description = Convert.ToString(subLine0[6]);

                    if (rec.Description.Length > 899)
                    {
                        rec.Description = rec.Description.Substring(0, 899);
                    }
                    else
                    {
                        rec.Description = rec.Description;
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Description : " + rec.Description);

                    if (subLine0[6].Contains("/"))
                    {
                        string[] strArray = subLine0[6].Split('/');
                        rec.CustomStr5 = strArray[0] + "//" + strArray[2];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr5 : " + rec.CustomStr5);
                    }

                    rec.CustomStr4 = Convert.ToString(subLine0[8]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr4 : " + rec.CustomStr4);

                    rec.CustomInt4 = Convert.ToInt32(subLine0[9]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt4 : " + rec.CustomInt4);

                    rec.CustomInt5 = Convert.ToInt32(subLine0[11]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt5 : " + rec.CustomInt5);

                    rec.CustomInt6 = Convert.ToInt32(subLine0[12]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt6 : " + rec.CustomInt6);

                    rec.EventType = Convert.ToString(subLine0[13]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> EventType : " + rec.EventType);

                    rec.EventCategory = subLine0[14];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> EventCategory : " + rec.EventCategory);

                    rec.CustomStr1 = Convert.ToString(subLine0[15]);

                    if (rec.CustomStr1.Length > 899)
                    {
                        rec.CustomStr1 = rec.CustomStr1.Substring(0, 899);
                    }
                    else
                    {
                        rec.CustomStr1 = rec.CustomStr1;
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr1 : " + rec.CustomStr1);

                    rec.CustomStr2 = Convert.ToString(subLine0[16]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr2 : " + rec.CustomStr2);

                    try
                    {
                        rec.CustomInt7 = Convert.ToInt32(subLine0[17]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt7 : " + rec.CustomInt7);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                " CoderParse() -->> CustomInt7 is not integer : " + subLine0[17]);
                    }

                    rec.CustomStr6 = Convert.ToString(subLine0[19]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr6 : " + rec.CustomStr6);

                    rec.CustomStr7 = Convert.ToString(subLine0[20]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr7 : " + rec.CustomStr7);

                    rec.CustomStr9 = Convert.ToString(subLine0[21]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr9 : " + rec.CustomStr9);

                    rec.CustomStr10 = Convert.ToString(subLine0[22]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr10 : " + rec.CustomStr10);

                    try
                    {
                        rec.CustomInt10 = Convert.ToInt32(subLine0[24]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt10 : " + rec.CustomInt10);
                    }
                    catch (Exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt10 is not integer : " + subLine0[24]);
                    }

                    try
                    {
                        rec.CustomInt9 = Convert.ToInt32(subLine0[30]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt9 : " + rec.CustomInt9);
                    }
                    catch (Exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt9 is not integer : " + subLine0[30]);
                    }

                    try
                    {
                        rec.CustomInt8 = Convert.ToInt32(subLine0[40]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt8 : " + rec.CustomInt8);
                    }
                    catch (Exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt8 is not integer : " + subLine0[40]);
                    }

                    rec.EventCategory = Convert.ToString(subLine0[39]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> EventCategory : " + rec.EventCategory);

                    //DateTime dateTime = new DateTime(Year, Month, Day, Hour, Minute, Second);
                    //rec.Datetime = String.Format("{0:yyyy/MM/dd HH:mm:ss}", dateTime);
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime : " + rec.Datetime);

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
            }
        } // CoderParse

        // fdgf

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
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
                List<String> _fileNameList = new List<String>();
                FileNameProp fileNameProp = new FileNameProp();
                FileNameProp[] fileNamePropArray;
                ArrayList SortedList = new ArrayList();

                //CreateDateDictionarys();

                if (_fileNameFilters != null)
                {
                    foreach (String fileName in fileNameList)
                    {
                        foreach (String fileNamefilter in _fileNameFilters)
                        {
                            fileName.Contains(fileNamefilter);
                            _fileNameList.Add(fileName);
                            break;
                        }
                    }
                }
                else
                {
                    _fileNameList = fileNameList;
                }
                fileNamePropArray = new FileNameProp[_fileNameList.Count];

                int k = 0;
                foreach (String line in _fileNameList)
                {
                    fileNameProp = new FileNameProp();
                    fileNameProp.FileName = line;

                    String[] subLine0 = line.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                    String[] subLine0_3 = subLine0[3].Split(new Char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameProp.FNGenericPart = subLine0[0] + "/" + subLine0[2] + "/" + subLine0_3[1];
                    fileNameProp.FNNumberPart = subLine0[1] + "/" + subLine0_3[0];

                    SetFileNumber(ref fileNameProp);
                    fileNamePropArray[k] = fileNameProp;
                    k++;
                }

                if (fileNamePropArray[0].FileNumber == 0)
                {
                    Array.Sort(fileNamePropArray, new FileNamePropComparerByGenericPart());
                }
                else
                {
                    Array.Sort(fileNamePropArray);
                }

                _fileNameList.Clear();

                foreach (FileNameProp item in fileNamePropArray)
                {
                    _fileNameList.Add(item.FileName);
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is successfully FINISHED.");

                for (int i = 0; i < _fileNameList.Count; i++)
                {
                    SortedList.Add(_fileNameList[i]);
                }

                SortedList.Sort();

                _fileNameList.Clear();
                for (int i = 0; i < SortedList.Count; i++)
                {
                    _fileNameList.Add(SortedList[i].ToString());
                }

                for (int i = 0; i < _fileNameList.Count; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            " SortFileNames() -->> Sorting file name is " + _fileNameList[i]);
                }

                return _fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SortFileNames() -->> An error occurred" + ex.ToString());
                return null;
            }
        } // SortFileNames





        private void CreateDateDictionarys()
        {
            _dayDictionaryEn = new Dictionary<String, Int32>();
            _dayDictionaryTr = new Dictionary<String, Int32>();
            _mounthDictionaryEn = new Dictionary<String, Int32>();
            _mounthDictionaryTr = new Dictionary<String, Int32>();

            _dayDictionaryEn.Add("mon", 1);
            _dayDictionaryEn.Add("tue", 2);
            _dayDictionaryEn.Add("wed", 3);
            _dayDictionaryEn.Add("thu", 4);
            _dayDictionaryEn.Add("fri", 5);
            _dayDictionaryEn.Add("sat", 6);
            _dayDictionaryEn.Add("sun", 7);

            _dayDictionaryTr.Add("pzt", 1);
            _dayDictionaryTr.Add("sal", 2);
            _dayDictionaryTr.Add("çar", 3);
            _dayDictionaryTr.Add("per", 4);
            _dayDictionaryTr.Add("cum", 5);
            _dayDictionaryTr.Add("cmt", 6);
            _dayDictionaryTr.Add("paz", 7);

            _mounthDictionaryEn.Add("jan", 01);
            _mounthDictionaryEn.Add("feb", 02);
            _mounthDictionaryEn.Add("mar", 03);
            _mounthDictionaryEn.Add("apr", 04);
            _mounthDictionaryEn.Add("may", 05);
            _mounthDictionaryEn.Add("jun", 06);
            _mounthDictionaryEn.Add("jul", 07);
            _mounthDictionaryEn.Add("aug", 08);
            _mounthDictionaryEn.Add("sep", 09);
            _mounthDictionaryEn.Add("oct", 10);
            _mounthDictionaryEn.Add("nov", 11);
            _mounthDictionaryEn.Add("dec", 12);

            _mounthDictionaryTr.Add("oca", 01);
            _mounthDictionaryTr.Add("şub", 02);
            _mounthDictionaryTr.Add("mar", 03);
            _mounthDictionaryTr.Add("nis", 04);
            _mounthDictionaryTr.Add("may", 05);
            _mounthDictionaryTr.Add("haz", 06);
            _mounthDictionaryTr.Add("tem", 07);
            _mounthDictionaryTr.Add("ağu", 08);
            _mounthDictionaryTr.Add("eyl", 09);
            _mounthDictionaryTr.Add("eki", 10);
            _mounthDictionaryTr.Add("kas", 11);
            _mounthDictionaryTr.Add("ara", 12);
        } // CreateDateDictionarys

        /// <summary>
        /// Create file number given file properties
        /// </summary>
        /// <param name="fileNameProp">File date properties</param>
        private void SetFileNumber(ref FileNameProp fileNameProp)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is STARTED");
                if (_dayDictionaryEn.ContainsKey(fileNameProp.FNDayPart.ToLower()))
                {
                    fileNameProp.FNDayPart = _dayDictionaryEn[fileNameProp.FNDayPart.ToLower()].ToString();
                }
                else if (_dayDictionaryTr.ContainsKey(fileNameProp.FNDayPart.ToLower()))
                {
                    fileNameProp.FNDayPart = _dayDictionaryTr[fileNameProp.FNDayPart.ToLower()].ToString();
                }
                if (_mounthDictionaryEn.ContainsKey(fileNameProp.FNMonthPart.ToLower()))
                {
                    fileNameProp.FNMonthPart = _mounthDictionaryEn[fileNameProp.FNMonthPart.ToLower()].ToString();
                }
                else if (_mounthDictionaryTr.ContainsKey(fileNameProp.FNMonthPart.ToLower()))
                {
                    fileNameProp.FNMonthPart = _mounthDictionaryTr[fileNameProp.FNMonthPart.ToLower()].ToString();
                }

                String _fileNumber = fileNameProp.FNYearPart +
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
