#define NEWSYNTAX

/*
 * OC4J Error Recorder
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
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
    public class OC4JErrorRecorder : Parser
    {
        public OC4JErrorRecorder()
            : base()
        {
            LogName = "OC4JErrorRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public OC4JErrorRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");

            StringBuilder sb = new StringBuilder();
            foreach (Char c in line)
            {
                if (c != '\0')
                    sb.Append(c);
            }
            line = sb.ToString();

            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);

                if (arr.Length < 3)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your OC4J Error Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r = new Rec();

                    String[] dateArr = arr[0].Split('/');
                    DateTime dt = DateTime.Parse("20" + dateArr[0] + " " + dateArr[1] + " " + dateArr[2] + " " + arr[1]);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

#if NEWSYNTAX
                    if (Char.IsNumber(arr[2][0]))
                    {
                        r.UserName = arr[2];
                        for (Int32 i = 3; i < arr.Length; i++)
                        {
                            r.EventCategory += arr[i] + " ";
                            r.EventCategory = r.EventCategory.Trim();
                        }
                    }
                    else
                    {
                        String[] arrIn = arr[arr.Length - 1].Split(':');
                        if (arrIn.Length > 1)
                        {
                            r.UserName = arrIn[1];
                            for (Int32 i = 2; i < (arr.Length - 1); i++)
                            {
                                r.EventCategory += arr[i] + " ";
                            }
                            r.EventCategory += arrIn[0];
                        }
                        else
                        {
                            for (Int32 i = 2; i < arr.Length; i++)
                            {
                                r.Description += arr[i] + " ";
                            }
                            r.Description = r.Description.Trim();
                        }
                    }
#else
                    r.SourceName = arr[2];
                    r.EventType = arr[4] + " " + arr[5];
                    try
                    {
                        r.EventId = Convert.ToInt64(arr[9].TrimStart('<').TrimEnd('>'));
                    }
                    catch
                    {
                        r.EventId = 0;
                    }
                    if (arr.Length > 10)
                    {
                        r.Description = arr[11] + " " + arr[12] + " " + arr[13];
                    }
#endif

                    if (remoteHost != "")
                        r.CustomStr8 = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.CustomStr8 = arrLocation[2];
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
                String day = "oc4j_err_out.out";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains(day))
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
                    if (arr[arr.Length - 1].Contains("oc4j_err_out.out"))
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
