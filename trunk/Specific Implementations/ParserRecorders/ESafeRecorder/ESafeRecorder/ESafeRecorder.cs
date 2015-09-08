/*
 * E-Safe Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
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
    public class ESafeRecorder : Parser
    {
        public ESafeRecorder()
            : base()
        {
            LogName = "ESafeRecorder";
            lineLimit = 75;
        }

        public override void Init()
        {
            GetFiles();
        }

        private bool CheckString(String val)
        {
            return (val == "result:" || val == "Protocol:" || val == "Subject:"
                || val == "Source:" || val == "Destination:" || val == "Sender:"
                || val == "Recipients:" || val == "Details:");
        }

        private void AddRecord(ref Rec r, String keyword, String val)
        {
            switch (keyword)
            {
                case "result:":
                    r.EventCategory = val;
                    break;
                case "Protocol:":
                    r.EventType = val;
                    break;
                case "Subject:":
                    r.CustomStr1 = val;
                    break;
                case "Source:":
                    r.ComputerName = val;
                    break;
                case "Destination:":
                    r.CustomStr2 = val;
                    break;
                case "Sender:":
                    r.UserName = val;
                    break;
                case "Recipients:":
                    r.CustomStr3 = val;
                    break;
                case "Details:":
                    r.Description = val;
                    break;
            }
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);

                if (arr.Length < 4)
                    return true;

                try
                {
                    if (arr[4] != "eSafeCR:")
                        return true;

                    Rec r = new Rec();

                    if (!line.Contains("Scan result:"))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (Int32 i = 4; i < arr.Length; i++)
                        {
                            sb.Append(arr[i]).Append(" ");
                        }

                        r.Description = sb.ToString().TrimEnd(' ');
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        String keyword = "";
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (CheckString(arr[i]))
                            {
                                if (keyword != "")
                                {
                                    AddRecord(ref r, keyword, sb.ToString().TrimEnd(' '));
                                }
                                keyword = arr[i];
                                sb.Remove(0, sb.Length - 1);
                            }
                            else
                                sb.Append(arr[i]).Append(" ");
                        }
                    }

                    DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

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

        public override void SetData(Rec obj)
        {
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                DateTime dtLast = new DateTime(1970, 12, 12);
                String fileLast = "";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    DateTime dt = File.GetLastAccessTime(file);
                    if (dt > dtLast)
                    {
                        fileLast = file;
                        dtLast = dt;
                    }
                }
                FileName = fileLast;
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
    }
}
