/*
 * TKB Syslog Recorder
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
    public class HPUXSyslogRecorder : Parser
    {
        public HPUXSyslogRecorder()
            : base()
        {
            LogName = "HPUXSyslogRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public HPUXSyslogRecorder(String fileName)
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

                        r.SourceName = arr[3];
                        r.EventCategory = arr[4];

                        for (Int32 i = 5; i < arr.Length; i++)
                            r.Description += arr[i] + " ";
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

        protected override void ParseFileNameRemote()
        {
            /*String stdOut = "";
            String stdErr = "";*/
            se = new SshExec(remoteHost, user);
            se.Password = password;
            /*se.Connect();
            se.SetTimeout(Int32.MaxValue);
            String command = "ls " + Dir + " -lt | grep ^-";
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
                if (arr[arr.Length - 1].Contains("syslog"))
                {
                    FileName = Dir + arr[arr.Length - 1];
                    break;
                }
            }
            stdOut = "";
            stdErr = "";
            se.Close();*/

            FileName = Dir + "syslog.log";
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

        public override void Start()
        {
            base.Start();
        }
    }
}
