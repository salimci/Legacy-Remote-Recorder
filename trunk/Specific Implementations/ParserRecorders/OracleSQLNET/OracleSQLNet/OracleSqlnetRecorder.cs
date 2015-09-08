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
        public string mDateTime;
        public string mEventCategory;
        public string mCustomStr1;
        public string mCustomStr3;
        public int mCustomInt1;
        public string mDescription;
        public bool mIsBlockFinished;
    }//

    public class OracleSqlnetRecorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords   
        private Fields RecordFields;

        public OracleSqlnetRecorder()//recorderName
            : base()
        {
            LogName = "OracleSQLNetRecorder";//recorderName***
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

        public OracleSqlnetRecorder(String fileName)//recorderName***
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED ");
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is : " + line);
                if (line == "")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line is empty");
                    return true;
                }

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

                line = line.Trim();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Line : " + line + "");

                if (!string.IsNullOrEmpty(line) && line.Length > 0)
                {
                    if (!line.StartsWith("*"))
                    {
                        if (line.StartsWith("Fatal"))
                        {
                            RecordFields.mIsBlockFinished = false;
                            RecordFields.mEventCategory = line;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> EventCategory : " + rec.EventCategory + "");
                        }

                        else if (line.Trim().StartsWith("Time:"))
                        {
                            DateTime date = Convert.ToDateTime(line.Replace("Time:", ""));
                            string date2 = date.ToString("yyyy-MM-dd HH:mm:ss");
                            RecordFields.mDateTime = date2;
                            rec.Datetime = date2;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Datetime : '" + rec.Datetime + "'");
                        }
                        else if (line.Trim().StartsWith("Client address:"))
                        {
                            RecordFields.mIsBlockFinished = true;
                            RecordFields.mCustomStr1 = line.Split('=')[2].ToString().Split(')')[0].ToString();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr1 : " + RecordFields.mCustomStr1 + "");
                            RecordFields.mCustomStr3 = line.Split('=')[3].ToString().Split(')')[0].ToString();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr3 : " + RecordFields.mCustomStr3 + "");
                            RecordFields.mCustomInt1 = Convert.ToInt32(line.Split('=')[4].ToString().Split(')')[0]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomInt1 : " + RecordFields.mCustomInt1 + "");
                            //RecordFields.mDescription += RecordFields.mDateTime + "|" + RecordFields.mCustomStr1 + "|" + RecordFields.mCustomStr3 + "|" + RecordFields.mCustomInt1;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Description : " + RecordFields.mDescription + "");

                            rec.EventCategory = RecordFields.mEventCategory;
                            rec.CustomStr1 = RecordFields.mCustomStr1;
                            rec.CustomStr3 = RecordFields.mCustomStr3;
                            rec.Description = RecordFields.mDescription;
                            rec.CustomInt1 = RecordFields.mCustomInt1;
                            rec.Datetime = RecordFields.mDateTime;
                        }
                        else
                        {
                            try
                            {
                                if (!line.Trim().StartsWith("*"))
                                {
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        rec.Description += line + " |";
                                    }
                                    //RecordFields.mDescription += RecordFields.mDateTime + "|" + RecordFields.mCustomStr1 + "|" + RecordFields.mCustomStr3 + "|" + RecordFields.mCustomInt1 + "|" + line;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Description : " + rec.Description + "");
                                }
                            }
                            catch (Exception exc)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> iç try" + exc.Message);
                            }
                        }
                        //rec.Description = RecordFields.mDescription;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> (Last) Description : " + RecordFields.mDescription + "");

                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> line starts with **");
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully FINISHED. setrecord üstü. ");
                    //return true;                      
                }
                else
                {
                    //return true;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Position Changed.");
                }
                //rec.Description = rec.Description.Substring(0, 899);
                //fdsfd
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully FINISHED. setrecord üstü. ");

                if (RecordFields.mIsBlockFinished)
                {
                    RecordFields.mDescription += RecordFields.mDateTime + " | " + RecordFields.mEventCategory + " | " + RecordFields.mCustomStr1 + " | " + RecordFields.mCustomStr3 + " | " + RecordFields.mCustomInt1.ToString();
                    //rec.Description = RecordFields.mDescription;
                    string newDescription = "";
                    newDescription = RecordFields.mDescription;
                    if (rec.Description.StartsWith("|"))
                    {
                        rec.Description = newDescription.Substring(1, rec.Description.Length);
                    }
                    else if (rec.Description.EndsWith("|"))
                    {
                        rec.Description = newDescription.Substring(0, rec.Description.Length - 1);
                    }
                    else
                    {
                        rec.Description = newDescription;
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> mIsBlockFinished : " + RecordFields.mIsBlockFinished.ToString() + "");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> mEventCategory : " + RecordFields.mEventCategory + "");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> mCustomStr1 : " + RecordFields.mCustomStr1 + "");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> mCustomStr3 : " + RecordFields.mCustomStr3 + "");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> mCustomInt1 : " + RecordFields.mCustomInt1.ToString() + "");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Description : " + RecordFields.mDescription + "");

                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Yeni İf Blok");
                    RecordFields.mIsBlockFinished = false;
                    RecordFields.mCustomInt1 = 0;
                    RecordFields.mCustomStr1 = "";
                    RecordFields.mCustomStr3 = "";
                    RecordFields.mDateTime = "";
                    RecordFields.mDescription = "";
                    RecordFields.mEventCategory = "";
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully FINISHED. ");

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> An error occurred : _1 " + ex.ToString());
                return false;
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> parseSpecific en son satır.");
            return true;
        } // ParseSpecific
        
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

        public void fds()
        {

        } // 

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
// 13.02.2012- 10:30 
// Dll will be load on BOREN Server 
// Dll is OK.
// Last Compile Date-Time 10:31
// Last Compile Size 17KB