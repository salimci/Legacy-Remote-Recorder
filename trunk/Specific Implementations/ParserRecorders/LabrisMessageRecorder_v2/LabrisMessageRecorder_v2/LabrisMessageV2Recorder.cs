/*
 * Labris Message Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
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
    public class LabrisMessageV2Recorder : Parser
    {
        public LabrisMessageV2Recorder() : base()
        {
            LogName = "LabrisMessageV2Recorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public LabrisMessageV2Recorder(String fileName) : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);

                try
                {
                    Rec r = new Rec();

                    if (arr[4] != "kernel:")
                    {
                        DateTime dt = DateTime.Now;
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        Log.Log(LogType.FILE, LogLevel.WARN, "Non labris message on parse, moving to description: " + line);
                        r.Description = line;
                    }
                    else if (arr.Length < 21)
                    {
                        DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        Log.Log(LogType.FILE, LogLevel.WARN, "Different message on parse, moving to description: " + line);
                        r.Description = line;
                    }
                    else
                    {
                        DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.CustomStr1 = arr[3] + " " + arr[4].TrimEnd(':');
                        r.CustomStr2 = arr[5] + " " + arr[6];
                        r.EventCategory = arr[8] + " " + arr[9];
                        r.CustomStr6 = arr[10];
                        r.CustomStr5 = arr[11].Split('=')[1];
                        r.CustomStr3 = arr[12].Split('=')[1];
                        r.CustomStr4 = arr[13].Split('=')[1];
	                    try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[13].Split('=')[1]);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                        r.CustomStr7 = arr[14].Split('=')[1];
                        r.CustomStr8 = arr[15].Split('=')[1];
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[16].Split('=')[1]);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                        try
                        {
                            r.CustomInt10 = Convert.ToInt32(arr[17].Split('=')[1]);
                        }
                        catch
                        {
                            r.CustomInt10 = -1;
                        }
                        Int32 count = 18;
                        String[] str9 = arr[18].Split('=');
                        if (str9.Length < 2)
                            count++;
                        r.CustomStr9 = arr[count++].Split('=')[1];
                        try
                        {
                            r.CustomInt3 = Convert.ToInt32(arr[count++].Split('=')[1]);
                        }
                        catch
                        {
                            r.CustomInt3 = -1;
                        }
                        try
                        {
                            r.CustomInt4 = Convert.ToInt32(arr[count++].Split('=')[1]);
                        }
                        catch
                        {
                            r.CustomInt4 = -1;
                        }
                        for(Int32 i = count; i < arr.Length; i++)
                            r.Description += arr[i] + " ";

                        if (remoteHost != "")
                            r.ComputerName = remoteHost;
                        else
                        {
                            String[] arrLocation = Dir.Split('\\');
                            if (arrLocation.Length > 1)
                                r.ComputerName = arrLocation[2];
                        }
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
                    if (arr[arr.Length - 1].Contains("messages"))
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
