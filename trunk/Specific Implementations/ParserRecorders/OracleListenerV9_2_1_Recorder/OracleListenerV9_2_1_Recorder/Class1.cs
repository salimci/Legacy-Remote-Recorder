/*
 16.08.2012 tarihinde Ankara Ticaret Odası'nda bulunan Oracle DB'nin aktivite loglarının alımı için geliştirilmiştir.
 * İstekde bulunan : Hüseyin Sevin
 * Geliştiren : Onur SARIKAYA
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
//OracleListenerV9_2_1_Recorder
namespace Parser
{
    public class OracleListenerV9_2_1_Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords        

        public OracleListenerV9_2_1_Recorder()//recorderName
            : base()
        {
            LogName = "OracleListenerV9_2_1_Recorder";//recorderName***
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

        public OracleListenerV9_2_1_Recorder(String fileName)//recorderName***
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
                    //fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);
                    for (int i = 0; i < fileNameList.Count; i++)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> FileName : " + fileNameList[i].ToString());
                    }
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
                    //fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);
                    for (int i = 0; i < fileNameList.Count; i++)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> FileName : " + fileNameList[i].ToString());
                    }
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED : Onur : " + line + " ------ " + dontSend);
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is : '" + line + "' ");

                if (line == "")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is empty");
                    return true;
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> DontSend : " + dontSend + "");
                //if (!dontSend)
                {
                    Rec rec = new Rec();
                    rec.LogName = LogName;

                    if (!String.IsNullOrEmpty(remoteHost))
                        rec.ComputerName = remoteHost;
                    else
                        rec.ComputerName = Environment.MachineName;

                    try
                    {
                        #region Description
                        if (line.Length > 3999)
                        {
                            rec.Description = line.Substring(0, 3999);
                        }
                        else
                        {
                            rec.Description = line;
                        }
                        #endregion
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->>  Description Error : " + exception);
                    }

                    string[] arr = line.Split('*');

                    #region DateTime
                    try
                    {

                        DateTime dt = Convert.ToDateTime(arr[0]);
                        rec.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Date time rror : " + exception.Message);

                        rec.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        rec.CustomStr10 = "Date_time is null";
                    }
                    #endregion

                    try
                    {
                        if (line.Contains("CONNECT_DATA"))
                        {

                            #region Main Parts
                            if (arr.Length > 3)
                            {
                                rec.EventCategory = arr[3];
                            }

                            if (arr.Length > 4)
                            {
                                rec.CustomStr2 = arr[4];
                            }

                            if (arr.Length > 5)
                            {
                                rec.CustomInt1 = Convert.ToInt32(arr[5]);
                            }
                            #endregion

                            #region MainParsing
                            if (arr[1].Contains("CONNECT_DATA"))
                            {
                                string[] arr2 = arr[1].Split('(');
                                for (int i = 0; i < arr2.Length; i++)
                                {
                                    if (arr2[i].StartsWith("SERVICE_NAME"))
                                    {
                                        rec.CustomStr6 = arr2[i].Split('=')[1].Trim(')');
                                    }

                                    if (arr2[i].StartsWith("PROGRAM"))
                                    {
                                        rec.EventType = arr2[i].Split('=')[1].Trim(')');
                                    }

                                    if (arr2[i].StartsWith("HOST"))
                                    {
                                        rec.CustomStr1 = arr2[i].Split('=')[1].Trim(')');
                                    }

                                    if (arr2[i].StartsWith("USER"))
                                    {
                                        rec.UserName = arr2[i].Split('=')[1].Replace(")", " ").Trim(' ');
                                    }
                                }
                            }

                            string[] arr3 = arr[2].Split('(');
                            for (int i = 0; i < arr3.Length; i++)
                            {
                                if (arr3[i].StartsWith("PROTOCOL"))
                                {
                                    rec.CustomStr3 = arr3[i].Split('=')[1].Trim(')');
                                }

                                if (arr3[i].StartsWith("HOST"))
                                {
                                    rec.CustomStr4 = arr3[i].Split('=')[1].Trim(')');
                                }

                                if (arr3[i].StartsWith("PORT"))
                                {
                                    rec.CustomStr5 = arr3[i].Split('=')[1].Replace(")", " ").Trim(' ');
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->>  Line is not correct format.");
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->>  Line is : " + line);
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->>  Line is not correct format.");
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr1 : " + rec.CustomStr1);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr2 : " + rec.CustomStr2);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr3 : " + rec.CustomStr3);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr4 : " + rec.CustomStr4);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr5 : " + rec.CustomStr5);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  CustomStr6 : " + rec.CustomStr6);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  EventType : " + rec.EventType);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  EventCategory : " + rec.EventCategory);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  ComputerName : " + rec.ComputerName);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->>  UserName : " + rec.UserName);


                   

                    try
                    {
                        SetRecordData(rec);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> After SetrecordData No catch. ");
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> After SetrecordData. " + ex.Message);
                    }
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

        /// <summary>
        /// Check the given file is finished or not 
        /// </summary>
        /// <param name="file">File full path which will check</param>
        /// <returns>if file finished return true</returns>
        private Boolean IsFileFinished(String file)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> File :  " + file);

                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> reading local file! ");

                        fileStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fileInfo = new FileInfo(file);
                        Int64 fileLength = fileInfo.Length;
                        Byte[] byteArray = new Byte[3];
                        fileStream.Read(byteArray, 0, 3);
                        if (byteArray[2] == 0)
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Dir : " + Dir.ToString());

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
        private void SetLastFile(List<String> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);

                if (fileNameList != null)
                {
                    if (fileNameList.Count > 0)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile);
                        FileName = lastFile;
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + ex.ToString());
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
    }
}

//Written by  Onur Sarıkaya 
//09.02.2012- 14:30 
// Dll has been loaded BOREN Server.
// Dll is OK.
// Last Compile Date-Time 14:34
// Last Compile Size 17KB
