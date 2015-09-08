/*
 * Teamcenter Recorder
 * Copyright (C) 2008-2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;
using Microsoft.Win32;
using CustomTools;
using SharpSSH.SharpSsh;
using Log;

namespace Parser
{
    public class TeamcenterFlexRecorder : Parser
    {
        public TeamcenterFlexRecorder()
            : base()
        {
            LogName = "TeamcenterFlexRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        protected override void dayChangeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
                if (oldFile == FileName)
                {
                    if (FileName == null)
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find file to parse, please check your path!");
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Day changed, new file is, " + FileName);
                        Start();
                    }
                }
                else
                {
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Day changed, new file is, " + FileName);
                }
            }
            dayChangeTimer.Start();
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            String[] arr = SpaceSplit(line, false, '"');
            if (arr.Length < 3)
                return true;

            try
            {
                if (!dontSend)
                {
                    Rec r = new Rec();
                    r.Description = "";

                    String eventType = arr[2].TrimEnd(':');

                    arr[1] = arr[1].TrimStart('(');
                    arr[1] = arr[1].TrimEnd(')');
                    
                    DateTime currentDate = DateTime.Now;

                    r.Datetime = currentDate.Year + "/" + currentDate.Month + "/" + currentDate.Day + " " + arr[0];

                    Int32 runItr = 2;

                    switch (eventType)
                    {
                        case "IN":
                        case "OUT":
                            {
                                r.EventType = eventType;
                                r.EventCategory = arr[3].TrimEnd('"').TrimStart('"');
                                r.UserName = arr[4];
                                runItr = 5;
                            } break;
                        case "UNSUPPORTED":
                            {
                                r.EventType = eventType;
                                r.EventCategory = arr[3].TrimEnd('"').TrimStart('"');
                                r.UserName = arr[5];
                                runItr = 6;
                            } break;
                    };

                    for (Int32 i = runItr; i < arr.Length; i++)
                    {
                        r.Description = arr[i] + " ";
                    }

                    r.Description = r.Description.Trim();

                    r.LogName = LogName;

                    SetRecordData(r);
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                return true;
            }
            return true;
        }

        public override void Start()
        {
            base.Start();
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                if (FileName != null)
                    System.Threading.Thread.Sleep(60000);

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains("ugflexlm.log"))
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
                se.RunCommand("ls -lt " + Dir + " | grep ^-", ref stdOut, ref stdErr);
                StringReader sr = new StringReader(stdOut);
                String line = sr.ReadLine();
                String[] arr = line.Split(' ');

                FileName = Dir + arr[12];
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
