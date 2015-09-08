/*
 * Bea Access Recorder
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
    public class BeaAccessRecorder : Parser
    {
        public BeaAccessRecorder() : base()
        {
            LogName = "BeaAccessRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
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

                    r.SourceName = arr[0];
                    arr[5] = arr[5].TrimStart('"').TrimEnd('"');
                    String[] eventArr = arr[5].Split(' ');
                    r.EventCategory = eventArr[0];
                    for (Int32 i = 1; i < eventArr.Length; i++)
                        r.EventType += eventArr[i] + " ";
                    r.EventType = r.EventType.TrimEnd(' ');//
                    
                    r.CustomStr1 = arr[6];
                    r.CustomStr2 = arr[7];
                    //3
                    //4
                    
                    if (arr.Length > 8)
                    {
                        r.Description = arr[8].TrimStart('"').TrimEnd('"');
                        r.ComputerName = arr[9].TrimStart('"').TrimEnd('"');
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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Close();
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    String filename = Path.GetFileName(file);
                    if (filename == "access.log")
                    {
                        FileName = file;
                        break;
                    }
                }
            }
            else
                FileName = Dir;
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
                    if (arr[arr.Length - 1].Contains("httpd-access.log"))
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
