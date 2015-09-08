/*
 * Apache Access Recorder
 * Copyright (C) 2008-2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class ApacheAccessRecorder : Parser
    {
        public ApacheAccessRecorder()
            : base()
        {
            LogName = "ApacheAccessRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public ApacheAccessRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false, '"');

                try
                {
                    Rec r = new Rec();

                    arr[3] = arr[3].TrimStart('[');

                    StringBuilder date = new StringBuilder();
                    bool first = false;
                    foreach (Char c in arr[3])
                    {
                        if (!first && c == ':')
                        {
                            first = true;
                            date.Append(' ');
                        }
                        else
                            date.Append(c);
                    }

                    DateTime dt = DateTime.Parse(date.ToString());
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                    r.CustomStr3 = arr[0];
                    arr[5] = arr[5].TrimStart('"').TrimEnd('"');
                    String[] eventArr = arr[5].Split(' ');
                    r.EventCategory = eventArr[0];
                    r.Description = "";
                    for (Int32 i = 1; i < eventArr.Length; i++)
                        r.Description += eventArr[i] + " ";
                    r.Description = r.EventType.TrimEnd(' ');                    
                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[6]);
                        r.CustomInt2 = Convert.ToInt32(arr[7]);
                    }
                    catch 
                    { 
                    }                    
                    if (arr.Length > 8)
                    {
                        r.EventType = arr[8].TrimStart('"').TrimEnd('"');
                        r.CustomStr5 = arr[9].TrimStart('"').TrimEnd('"');
                    }

                    if (remoteHost != "")
                        r.CustomStr4 = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.CustomStr4 = arrLocation[2];
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
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                DateTime dt = DateTime.MinValue;
                String day = "access_log";
                String not = "admin";
                String ssl = "ssl";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains(day) && !(file.Contains(not) || file.Contains(ssl)))
                    {
                        String fShort = Path.GetFileName(file);

                        String[] arr = fShort.Split('.');
                        if (arr.Length != 2)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Wrong file name, " + file);
                            continue;
                        }

                        DateTime dtFile = new DateTime(1970, 1, 1, 0, 0, 0);

                        try
                        {
                            dtFile = dtFile.AddSeconds(Convert.ToDouble(arr[1]));
                        }
                        catch
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot parse unixtime, " + file);
                            continue;
                        }

                        if (dtFile > dt)
                        {
                            dt = dtFile;
                            FileName = file;
                        }
                    }
                }

                day = "access.log";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains(day))
                    {
                        FileName = file;
                    }
                }
            }
            else
                FileName = Dir;
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Enabled = false;

            if (Today.Day != DateTime.Now.Day || FileName == null)
            {
                String oldFile = FileName;
                DateTime oldTime = Today;
                Int64 oldPosition = Position;
                String oldLastLine = lastLine;
                Today = DateTime.Now;
                Stop();
                Position = 0;
                lastLine = "";
                ParseFileName();
                if (oldFile == FileName)
                {
                    if (FileName == null)
                    {
                        Today = oldTime;
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find file to parse, please check your path!");
                    }
                    else
                    {
                        Today = DateTime.Now;
                        Position = oldPosition;
                        lastLine = oldLastLine;
                        Start();
                    }
                }
                else
                {
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Day changed, new file is, " + FileName);
                }
            }
            else/* if (remoteHost == "")*/
            {
                String fileLast = FileName;
                ParseFileName();
                if (FileName != fileLast)
                {
                    Stop();
                    Position = 0;
                    lastLine = "";
                    FileName = fileLast;
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
            }

            dayChangeTimer.Enabled = true;            
        }

        protected override void ParseFileNameRemote()
        {
            String stdOut = "";
            String stdErr = "";
            se = new SshExec(remoteHost, user);
            se.Password = password;
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                se.Connect();
                se.SetTimeout(Int32.MaxValue);
                String command = "ls -lt " + Dir + " | grep access";
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
                    if (arr[arr.Length - 1].Contains("httpd-access.log")
                        || arr[arr.Length - 1].Contains("access_log"))
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
    }
}
