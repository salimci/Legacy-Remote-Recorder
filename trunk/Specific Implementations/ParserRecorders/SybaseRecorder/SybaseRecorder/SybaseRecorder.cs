/*
 * Sybase Recorder
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
    public class SybaseRecorder : Parser
    {
        public SybaseRecorder()
            : base()
        {
            LogName = "SybaseRecorder";
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
                String[] arr = SpaceSplit(line, false);

                try
                {
                    Rec r = new Rec();

                    String [] arrIn = arr[0].Split(':');

                    if (arrIn.Length != 4 || arr.Length < 4)
                        return true;

                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arrIn[0]);
                    }
                    catch
                    {
                        r.CustomInt1 = 0;
                    }
                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arrIn[1]);
                    }
                    catch
                    {
                        r.CustomInt2 = 0;
                    }
                    r.SourceName = arrIn[2];
                    r.Datetime = arrIn[3] + " " + arr[1];
                    r.EventCategory = arr[2];

                    for (Int32 i = 3; i < arr.Length; i++)
                    {
                        r.Description += arr[i] + " ";
                    }

                    r.Description = r.Description.TrimEnd(' ');

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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Close();
        }

        protected override void ParseFileNameLocal()
        {
            FileName = Dir;
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is " + FileName);
            /*Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
            foreach (String file in Directory.GetFiles(Dir))
            {
                String filename = Path.GetFileName(file);
                if (filename == "TMODBSRV.log")
                {
                    FileName = file;
                    break;
                }
            }*/
        }

        protected override void ParseFileNameRemote()
        {
            String stdOut = "";
            String stdErr = "";
            se = new SshExec(remoteHost, user);
            se.Password = password;
            FileName = Dir;
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is " + FileName);
            /*se.Connect();
            se.SetTimeout(Int32.MaxValue);
            String command = "ls -lt " + Dir + " | grep TMODBSRV";
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
                if (arr[arr.Length - 1].Contains("TMODBSRV.log"))
                {
                    FileName = Dir + arr[arr.Length - 1];
                    break;
                }
            }
            stdOut = "";
            stdErr = "";
            se.Close();*/
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
