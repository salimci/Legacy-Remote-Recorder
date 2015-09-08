using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class RedhatMessageRecorder : Parser
    {
        public RedhatMessageRecorder()
            : base()
        {
            LogName = "RedhatMessageRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public RedhatMessageRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");


            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is : " + line);

            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);

                try
                {
                    Rec r = new Rec();

                    if (arr.Length < 4)
                    {
                        Log.Log(LogType.FILE, LogLevel.WARN, "Different message on parse, moving to description: " + line);

                        DateTime dt = DateTime.Now;
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.Description = line;
                    }
                    else
                    {
                        DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.ComputerName = arr[3];
                        r.SourceName = arr[4].TrimEnd('"');

                        for (Int32 i = 5; i < arr.Length; i++)
                            r.Description += arr[i] + " ";

                        if (remoteHost != "")
                            r.CustomStr3 = remoteHost;
                        else
                        {
                            String[] arrLocation = Dir.Split('\\');
                            if (arrLocation.Length > 1)
                                r.CustomStr3 = arrLocation[2];
                        }
                    }

                    r.LogName = LogName;

                    SetRecordData(r);
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

        protected override void ParseFileNameLocal()
        {
            /*DateTime dtLast = new DateTime(1970, 12, 12);
            String fileLast = "";
            foreach (String file in Directory.GetFiles(Dir))
            {
                DateTime dt = File.GetLastAccessTime(file);
                if (dt > dtLast)
                {
                    fileLast = file;
                    dtLast = dt;
                }
            }
            FileName = fileLast;*/
        }

        //protected override void ParseFileNameRemote()
        //{
        //    String stdOut = "";
        //    String stdErr = "";
        //    se = new SshExec(remoteHost, user);
        //    se.Password = password;
        //    if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
        //    {
        //        se.Connect();
        //        se.SetTimeout(Int32.MaxValue);
        //        String command = "ls -lt " + Dir + " | grep messages";
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "SSH command is : " + command);
        //        se.RunCommand(command, ref stdOut, ref stdErr);
        //        StringReader sr = new StringReader(stdOut);
        //        String line = "";
        //        bool first = true;
        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            if (first)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Command returned : " + line);
        //                first = false;
        //            }
        //            String[] arr = line.Split(' ');
        //            if (arr[arr.Length - 1].Contains("messages"))
        //            {
        //                FileName = Dir + arr[arr.Length - 1];
        //                break;
        //            }
        //        }
        //        stdOut = "";
        //        stdErr = "";
        //        se.Close();
        //    }
        //    else
        //        FileName = Dir;
        //}

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatMessageRecorder In ParseFileNameRemote() -->> Enter The Function");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep messages";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatMessageRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    StringReader sr = new StringReader(stdOut);

                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].Contains("messages") == true && arr[arr.Length - 1].Contains("gz") == false && arr[arr.Length - 1].Contains("bz2") == false)
                            arrFileNameList.Add(arr[arr.Length - 1]);
                    }
                    
                    String[] dFileNameList = SortFiles(arrFileNameList);
                    
                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatMessageRecorder In ParseFileNameRemote() -->> LastFile Is = " + lastFile);

                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            stdOut = "";
                            stdErr = "";
                            String commandRead;

                            if (readMethod == "nread")
                            {
                                commandRead = tempCustomVar1 + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatMessageRecorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatMessageRecorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
                            }

                            se.RunCommand(commandRead, ref stdOut, ref stdErr);
                            se.Close();

                            StringReader srTest = new StringReader(stdOut);
                            Int64 posTest = Position;
                            String lineTest = "";
                            while ((lineTest = srTest.ReadLine()) != null)
                            {
                                if (lineTest.StartsWith("~?`Position"))
                                {
                                    try
                                    {
                                        String[] arrIn = lineTest.Split('\t');
                                        String[] arrPos = arrIn[0].Split(':');
                                        String[] arrLast = arrIn[1].Split('`');
                                        posTest = Convert.ToInt64(arrPos[1]); // deðiþti Convert.ToUInt32s
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, " RedhatMessageRecorder In ParseFileNameRemote() In Try Catch -->> " + ex.Message);
                                    }
                                }
                            }

                            if (posTest > Position)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " RedhatMessageRecorder In ParseFileNameRemote() -->> posTest > Position So Continiou With Same File ");

                                FileName = lastFile;

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " RedhatMessageRecorder In ParseFileNameRemote() -->> LastFile Is " + lastFile);
                            }
                            else
                            {

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                   " RedhatMessageRecorder In ParseFileNameRemote() -->> Finished Reading The File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " RedhatMessageRecorder In ParseFileNameRemote() -->> There Is No New File And Continiou With Same File And Waiting For a New Record " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " RedhatMessageRecorder In ParseFileNameRemote() -->> Finished Reading The File And Continiou With New File " + FileName);
                                            break;

                                        }
                                    }
                                }
                            }
                        }
                        else
                            SetNextFile(dFileNameList, "ParseFileNameRemote()");

                    }
                    else
                    {
                        
                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                "  RedhatMessageRecorder In ParseFileNameRemote() -->>  Last File Is Null And Setted The File To " + FileName);

                        }
                    }
                    stdOut = "";
                    stdErr = "";
                    se.Close();
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatMessageRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatMessageRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatMessageRecorder In ParseFileNameRemote() -->>  Exit The Function");
        }

        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  RedhatMessageRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  RedhatMessageRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatMessageRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatMessageRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];
            UInt64 indexnumber = Convert.ToUInt64(arrFileNames[arrFileNames.Count-1].ToString().Split('.')[1]); ;

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                if (arrFileNames[i].ToString().Contains("."))
                {
                    dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[1]);
                    dFileNameList[i] = arrFileNames[i].ToString();
                }
                else 
                {
                    dFileNumberList[i] = indexnumber;
                    dFileNameList[i] = arrFileNames[i].ToString();
                }
            }

            Array.Sort(dFileNumberList, dFileNameList);
            
            return dFileNameList;
        }

        private void SetNextFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[1]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[1]);

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, sFunction + " | LastFile Silinmis , Dosya Bulunamadý  Yeni File : " + FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ApacheAccessRecorder In SetNextFile() Exception Message -->> " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ApacheAccessRecorder In SetNextFile() Exception StackTrace -->> " + ex.StackTrace);
            }
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
