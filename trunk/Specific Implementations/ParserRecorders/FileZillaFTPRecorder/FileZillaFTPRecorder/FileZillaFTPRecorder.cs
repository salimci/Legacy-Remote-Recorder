/*
 Tamam
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
        public long totalLineCountinFile;
        public string[] tempArray;
    }

    public class FileZillaFTPRecorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private Fields RecordFields;

        public FileZillaFTPRecorder()//recorderName
            : base()
        {
            LogName = "FileZillaFTPRecorder";//recorderName***
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

        public FileZillaFTPRecorder(String fileName)//recorderName***
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

            if (line == "")
                return true;

            string[] arrStrings = line.Split(' ');
            string myDatestring = (arrStrings[1] + " " + arrStrings[2]).Trim();
            Log.Log(LogType.FILE, LogLevel.DEBUG, "myDatestring: " + "'" + myDatestring + "'");

            try
            {
                Rec r = new Rec();
                try
                {
                    r.Description = line;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Description : " + r.Description);
                }
                catch (Exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Description : ");
                }

                try
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Datetime_0 : " + myDatestring);
                    string[] a = myDatestring.Split(' ')[0].Split('.');
                    string[] b = myDatestring.Split(' ')[1].Split(':');
                    string c = a[2] + "-" + a[1] + "-" + a[0] + " " + b[0] + ":" + b[1] + ":" + b[2];
                    DateTime dt = Convert.ToDateTime(c);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Datetime_1 : " + dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Datetime_2 : " + r.Datetime);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Datetime : " + exception.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Datetime : " + exception.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Datetime : " + exception.ToString());
                }


                try
                {
                    r.CustomInt10 = Convert.ToInt32(arrStrings[0].Trim('(').Trim(')'));
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomInt10 : " + r.CustomInt10);
                }
                catch (Exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | CustomInt10 : ");
                }

                try
                {
                    string s = (Between(line, "(", ")"));
                    string s1 = (Between(s, "(", ")"));
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | UserName : " + s1);
                    r.UserName = s1;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | UserName : " + r.UserName);
                }
                catch (Exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | UserName : ");
                }

                try
                {
                    r.CustomStr3 = arrStrings[7].Replace('(', ' ').Replace(')', ' ').Replace('>', ' ').Trim();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr3 : " + r.CustomStr3);
                }
                catch (Exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | CustomStr3 : ");
                }

                try
                {
                    string str4 = (After(line, ")>"));
                    if (str4.Length > 899)
                    {
                        r.CustomStr4 = str4.Substring(0, 899);
                    }
                    else
                    {
                        r.CustomStr4 = str4;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr4 : " + r.CustomStr4);
                }
                catch (Exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | CustomStr4 : ");
                }

                Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
                SetRecordData(r);
                Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");

            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific : " + exception.Message);
            }
            return true;
        } // ParseSpecific

        #region Utils
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
        #endregion


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

            if (string.IsNullOrEmpty(RecordFields.currentFile))
            {
                RecordFields.totalLineCountinFile = CountLinesInFile(file);
            }

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
            //foreach (string t in _fileNameList)
            //{
            //    Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            //}

            for (int i = 0; i < _fileNameList.Count; i++)
            {
                FileInfo fileInfo = new FileInfo(Dir + @"\\" + _fileNameList[i]);
                if (fileInfo.Length == 0)
                {
                    _fileNameList.Remove(_fileNameList[i]);
                }
            }

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
        // gdfgdfg
    }
}

//
