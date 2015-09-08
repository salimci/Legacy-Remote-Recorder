/*
 * DansGuardian Recorder
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class DansGuardianRecorder : Parser
    {
        public DansGuardianRecorder()
            : base()
        {
            LogName = "DansGuardianRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public DansGuardianRecorder(String fileName)
            : base(fileName)
        {
        }

        private String[] CustomSplit(String line, bool useTabs)
        {
            List<String> lst = new List<String>();
            String[] arr = SpaceSplit(line, useTabs);
            if (arr.Length < 4)
            {
                return lst.ToArray();
            }
            lst.Add(arr[0]);
            lst.Add(arr[1]);
            lst.Add(arr[2]);
            lst.Add(arr[3]);
            Int32 i = 0;
            StringBuilder sb = new StringBuilder();
            for (i = 4; i < arr.Length; i++)
            {
                if (arr[i] == "GET"
                    || arr[i] == "POST"
                    || arr[i] == "HEAD"
                    || arr[i] == "CONNECT")
                {
                    sb.Remove(sb.Length - 1, 1);
                    lst.Add(sb.ToString());
                    break;
                }
                sb.Append(arr[i]).Append(" ");
            }
            StringBuilder sbCenter = new StringBuilder();
            Int32 centerEnd = arr.Length - 4;
            Int32 control = i + 3;
            bool addedString = false;
            for (Int32 j = i; j < centerEnd; j++)
            {
                if (j < control)
                    lst.Add(arr[j]);
                else
                {
                    if (!addedString)
                        addedString = true;
                    sbCenter.Append(arr[j]).Append(" ");
                }
            }
            if (addedString)
                sbCenter.Remove(sbCenter.Length - 1, 1);
            lst.Add(sbCenter.ToString());
            for (Int32 j = centerEnd; j < arr.Length; j++)
                lst.Add(arr[j]);
            return lst.ToArray();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = CustomSplit(line, true);

                if (arr.Length < 12)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r = new Rec();

                    arr[0] = arr[0].Replace('.', '/');

                    r.Datetime = arr[0] + " " + arr[1];
                    r.CustomStr1 = arr[3];
                    String[] descArr = arr[4].Split('*');
                    r.Description = descArr[0];
                    if (descArr.Length > 1)
                        r.CustomStr6 = descArr[1];
                    if (descArr.Length > 2)
                        r.CustomStr5 = descArr[2];
                    r.SourceName = arr[5];
                    r.CustomInt2 = Convert.ToInt32(arr[6]);
                    r.CustomInt1 = Convert.ToInt32(arr[7]);
                    r.CustomStr1 = arr[8];
                    r.EventId = Convert.ToInt64(arr[9]);
                    r.EventCategory = arr[10];
                    r.CustomStr4 = arr[11];
                    r.CustomStr2 = arr[12];

                    r.LogName = LogName;

                    //SetRecordData(r);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return false;
                }
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                String day = "access.log";
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
    }
}
