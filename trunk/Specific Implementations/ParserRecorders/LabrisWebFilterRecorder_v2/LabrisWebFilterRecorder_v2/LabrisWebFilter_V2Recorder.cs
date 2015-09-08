using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Text.RegularExpressions;

namespace Parser
{
    public class LabrisWebFilter_V2Recorder : Parser
    {
        public LabrisWebFilter_V2Recorder() : base()
        {
            LogName = "LabrisWebFilter_V2Recorder";
            usingKeywords = false;
            lineLimit = 50;
        }
        public override void Init()
        {
            GetFiles();
        }
        public LabrisWebFilter_V2Recorder(String fileName) : base(fileName)
        {
        }
        public override bool ParseSpecific(String line, bool dontSend)
        {
            //Nov  3 04:07:10 2009 dansguardian[29367]: \01110.10.10.10\01110.10.10.10\011http://sn111ds.snt111.mail.services.live.com/DeltaSync_v2.0.0/Sync.aspx\011*EXCEPTION* Ayricalikli kullanici adina sahipsiniz.\011POST\011527\0110\011\0112\011200\011-\011\011domainadmins\011-

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);
                try
                {
                    Rec r = new Rec();
                    StringBuilder sbRest = new StringBuilder();
                    for (int i = 6; i < arr.Length; i++)
                    {
                        sbRest.Append(arr[i]).Append(' ');
                    }
                    arr[6] = sbRest.ToString();
                    StringBuilder sbDate = new StringBuilder();
                    sbDate.Append(arr[2]).Append('/').Append(arr[0]).Append('/').Append(arr[4]).Append(' ').Append(arr[3]);                                        
                    try
                    {
                        DateTime d = Convert.ToDateTime(sbDate.ToString());                        
                    }
                    catch
                    {
                        r.CustomStr8 = arr[0];
                    }
                    String[] arrRest = Regex.Split(arr[6], "\011");                    
                    if(arrRest[0] != "")
                        r.CustomInt1 = Convert.ToInt32(arrRest[0]);
                    r.UserName = arrRest[1];
                    r.CustomStr3 = arrRest[2];
                    r.Description = arrRest[3];
                    r.CustomStr5 = arrRest[4];
                    r.EventCategory = arrRest[5];
                    if (arrRest[6] != "")
                        r.CustomInt2 = Convert.ToInt32(arrRest[6]);
                    if (arrRest[7] != "")
                        r.CustomInt3 = Convert.ToInt32(arrRest[7]);
                    if (arrRest[9] != "")
                        r.CustomInt2 = Convert.ToInt32(arrRest[9]);
                    if (arrRest[10] != "")
                        r.CustomInt3 = Convert.ToInt32(arrRest[10]);
                    r.CustomStr7 = arrRest[11];
                    if (remoteHost != "")
                        r.ComputerName = remoteHost;                    
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.ComputerName = arrLocation[2];
                    }


                    if (r.Description.Length > 900)
                    {

                        if (r.Description.Length > 1800)
                        {
                            r.CustomStr10 = r.Description.Substring(900, 900);
                        }
                        else
                        {
                            r.CustomStr10 = r.Description.Substring(900, r.Description.Length - 900 - 2);
                        }

                        r.Description = r.Description.Substring(0, 900);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
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

        protected override void  ParseFileNameLocal()
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
        protected override void ParseFileNameRemote()
        {
            String stdOut = "";
            String stdErr = "";
            se = new SshExec(remoteHost, user);
            se.Password = password;
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                se.ConnectTimeout(900000);
                se.SetTimeout(900000);
                String command = "ls -lt " + Dir + " | grep ^-";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SSH command is : " + command);
                se.RunCommand(command, ref stdOut, ref stdErr);
                StringReader sr = new StringReader(stdOut);
                String line = "";
                bool first = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (first)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Command returned : " + line);
                        first = false;
                    }
                    String[] arr = line.Split(' ');
                    if (arr[arr.Length - 1].Contains("access.log"))
                    {
                        FileName = Dir + arr[arr.Length - 1];
                        break;
                    }
                }
                stdOut = "";
                stdErr = "";
                se.Close();
            }
            else
                FileName = Dir;
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
            catch (Exception e)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (Today.Day != DateTime.Now.Day || FileName == null)
            {
                String oldFile = FileName;
                DateTime oldTime = Today;
                Today = DateTime.Now;
                Stop();
                Position = 0;
                lastLine = "";
                ParseFileName();
                if (FileName == null)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find file to parse, please check your path!");
                    Today = oldTime;
                }
                else
                {
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Day changed, new file is, " + FileName);
                }
            }
            dayChangeTimer.Start();
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
