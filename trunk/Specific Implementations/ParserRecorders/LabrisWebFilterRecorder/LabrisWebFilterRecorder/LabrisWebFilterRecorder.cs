/*
 * Labris Web Filter Recorder
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
    public class LabrisWebFilterRecorder : Parser
    {
        public LabrisWebFilterRecorder() : base()
        {
            LogName = "LabrisWebFilterRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public LabrisWebFilterRecorder(String fileName) : base(fileName)
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

                    String [] dateArr = arr[0].Split('.');
                    try
                    {
                        DateTime d = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(dateArr[0]));
                        r.Datetime = d.Year + "/" + d.Month + "/" + d.Day + " " + d.Hour + ":" + d.Minute + ":" + d.Second + "." + dateArr[1];
                    }
                    catch
                    {
                        r.CustomStr8 = arr[0];
                    }
                    r.CustomInt1 = Convert.ToInt32(arr[1]);
                    r.CustomStr3 = arr[2];
                    r.CustomStr2 = arr[3];
                    r.CustomInt2 = Convert.ToInt32(arr[4]);
                    r.CustomStr4 = arr[5];
                    r.Description = arr[6];
                    r.UserName = arr[7];
                    r.CustomStr7 = arr[8];

                    if (remoteHost != "")
                        r.ComputerName = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.ComputerName = arrLocation[2];
                    }
                    r.LogName = LogName;

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
