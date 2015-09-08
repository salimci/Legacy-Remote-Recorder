/*
 * OpenFire Info Recorder
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
using System.Xml;

namespace Parser
{
    public class OpenFireInfoRecorder : Parser
    {
        public OpenFireInfoRecorder()
            : base()
        {
            LogName = "OpenFireInfoRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public OpenFireInfoRecorder(String fileName)
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
                line = line.Trim();
                Char check = line[0];
                if (!(Char.IsNumber(check) || check == '<'))
                    return true;

                String[] arr = SpaceSplit(line, false);

                if (arr.Length < 2)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your OpenFire Info Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r;
                    if(storage)
                        r = sRec;
                    else
                        r = new Rec();

                    if (Char.IsNumber(check))
                    {
                        r.EventCategory = "";
                        arr[0] = arr[0].Replace('.', '/');
                        r.Datetime = arr[0] + " " + arr[1];
                        Int32 i = 0;
                        for (i = 2; i < arr.Length; i++)
                        {
                            if (arr[i].StartsWith("<"))
                                break;
                            r.EventCategory += arr[i] + " ";
                        }
                        r.EventCategory = r.EventCategory.Trim();
                    }
                    else
                    {
                        try
                        {
                            StringReader sr = new StringReader(line);
                            XmlTextReader reader = new XmlTextReader(sr);
                            while (reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        {
                                            if (reader.HasAttributes)
                                            {
                                                for (Int32 i = 0; i < reader.AttributeCount; i++)
                                                {
                                                    reader.MoveToAttribute(i);
                                                    switch (reader.Name)
                                                    {
                                                        case "id":
                                                            storage = true;
                                                            r.CustomStr1 = reader.Value;
                                                            break;
                                                        case "to":
                                                            r.ComputerName = reader.Value;
                                                            break;
                                                        case "type":
                                                            r.EventType = reader.Value;
                                                            break;
                                                        case "from":
                                                            r.SourceName = reader.Value;
                                                            break;
                                                        case "xmlns":
                                                            storage = false;
                                                            r.Description = reader.Value;
                                                            break;
                                                        default:
                                                            break;
                                                    };
                                                }
                                            }
                                        } break;
                                    //case XmlNodeType.EndElement:

                                    // Todo

                                    //case XmlNodeType.Text:

                                    // Todo
                                    default:
                                        break;
                                }
                            }
                            sr.Close();
                            reader.Close();
                        }
                        catch
                        {
                        }
                    }

                    r.LogName = LogName;

                    if (storage)
                        sRec = r;
                    else
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
                String day = "info.log";
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
                    if (arr[arr.Length - 1].Contains("info.log"))
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
