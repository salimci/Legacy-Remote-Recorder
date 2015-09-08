/*
 * E-Safe Mail Sec Recorder
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
    public class EsafeMailSecRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public EsafeMailSecRecorder()
            : base()
        {
            LogName = "EsafeMailSecRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            String[] arr = line.Split('|');
            if (arr.Length < 3)
                return true;

            if (dictHash.Count > 0)
            {
                if (dictHash.Count > arr.Length)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count " + dictHash.Count + ", found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your DHCP Logger before messing with developer! Parsing will continue...");
                    return true;
                }
            }

            try
            {
                if (arr[0].StartsWith("Date "))
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    Int32 count = 0;
                    foreach (String field in arr)
                    {
                        dictHash.Add(field.Trim(), count);
                        count++;
                    }
                    SetLastKeywords(line);
                    keywordsFound = true;
                }
                else
                {
                    if (!dontSend)
                    {
                        Rec r = new Rec();

                        Int32 dateIndex = dictHash["Date (yyyy-mm-dd  HH:mm:ss)"];

                        String[] temp = arr[dateIndex].Split('-');
                        String[] tempTime = temp[2].Split(' ');
                        r.Datetime = temp[0] + "/" + temp[1] + "/" + tempTime[0] + " " + tempTime[2];

                        r.EventCategory = arr[dictHash["Method"]];
                        r.EventType = arr[dictHash["ProtocolType"]];
                        r.CustomStr1 = arr[dictHash["Event"]];
                        r.CustomStr2 = arr[dictHash["File Name\\Mail Subject"]];
                        r.CustomStr4 = arr[dictHash["Source IP"]];
                        r.CustomStr3 = arr[dictHash["Destination IP"]];
                        r.CustomStr5 = arr[dictHash["Mail Sender"]];
                        r.CustomStr6 = arr[dictHash["Mail Recipients"]];
                        r.CustomStr7 = arr[dictHash["Extended result"]];
                        r.CustomStr8 = arr[dictHash["SessionID"]];
                        r.CustomStr10 = arr[dictHash["File Type"]];
                        r.CustomStr2 = arr[dictHash["MessageID"]];
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[dictHash["#File Size"]]);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }

                        r.Description = arr[dictHash["Details"]];

                        r.LogName = LogName;

                        r.CustomStr10 = Dir;

                        SetRecordData(r);
                    }
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
            try
            {
                String keywords = GetLastKeywords();
                String[] arr = keywords.Split('|');

                if (dictHash == null)
                    dictHash = new Dictionary<String, Int32>();
                if (arr.Length > 2)
                    dictHash.Clear();
                Int32 count = 0;
                foreach (String keyword in arr)
                {
                    if (keyword == "")
                        continue;
                    dictHash.Add(keyword.Trim(), count);
                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            }
            base.Start();
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                if (FileName != null)
                    System.Threading.Thread.Sleep(60000);

                String day = DateTime.Now.ToString("yyyy_MM_dd");

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
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
                String cmd = "ls -lt " + Dir;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in folder " + Dir + " with command " + cmd);
                se.RunCommand(cmd, ref stdOut, ref stdErr);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Command Returned: " + stdOut);
                StringReader sr = new StringReader(stdOut);
                String line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Checking line: " + line);
                    if (line.StartsWith("-"))
                        break;
                }

                String[] arr = line.Split(' ');

                FileName = Dir + arr[arr.Length - 1];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is " + FileName);
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
