/*
 * Xfer Recorder
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
    public class XferRecorder : Parser
    {
        public XferRecorder()
            : base()
        {
            LogName = "XferRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public XferRecorder(String fileName)
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
                String[] arr = SpaceSplit(line, true);

                if (arr.Length < 18)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 18+, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your xfer logs before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r = new Rec();

                    StringBuilder dateString = new StringBuilder();
                    dateString.Append(arr[4]).Append(" ").Append(arr[1]).Append(" ").Append(arr[2]).Append(" ").Append(arr[3]);
                    DateTime dt = DateTime.Parse(dateString.ToString());

                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[5]);
                    }
                    catch
                    {
                        r.CustomInt1 = 0;
                    }

                    r.CustomStr3 = arr[6];

                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[7]);
                    }
                    catch
                    {
                        r.CustomInt2 = 0;
                    }

                    r.CustomStr1 = arr[8];
                    r.CustomStr2 = arr[9];
                    switch (r.CustomStr2)
                    {
                        case "a":
                            {
                                r.CustomStr2 = "ascii";
                            } break;
                        case "b":
                            {
                                r.CustomStr2 = "binary";
                            } break;
                        /*default:
                            {
                                r.CustomStr2 = "You are so noob that you dont tell me an option like " + r.CustomStr2 + " exists. I'm so sorry";
                            } break;*/
                    }
                    r.CustomStr7 = arr[10];
                    switch (r.CustomStr7)
                    {
                        case "C":
                            {
                                r.CustomStr7 = "compressed";
                            } break;
                        case "U":
                            {
                                r.CustomStr7 = "uncompressed";
                            } break;
                        case "T":
                            {
                                r.CustomStr7 = "tar";
                            } break;
                        case "_":
                            {
                                r.CustomStr7 = "no action was taken";
                            } break;
                        /*default:
                            {
                                r.CustomStr7 = "You are so noob that you dont tell me an option like " + r.CustomStr2 + " exists. I'm so sorry";
                            } break;*/
                    }
                    r.CustomStr4 = arr[11];
                    switch (r.CustomStr4)
                    {
                        case "o":
                            {
                                r.CustomStr4 = "outgoing";
                            } break;
                        case "i":
                            {
                                r.CustomStr4 = "incoming";
                            } break;
                        case "d":
                            {
                                r.CustomStr4 = "deleted";
                            } break;
                        /*default:
                            {
                                r.CustomStr4 = "You are so noob that you dont tell me an option like " + r.CustomStr2 + " exists. I'm so sorry";
                            } break;*/
                    }
                    r.CustomStr5 = arr[12];
                    switch (r.CustomStr5)
                    {
                        case "a":
                            {
                                r.CustomStr5 = "anonymous";
                            } break;
                        case "r":
                            {
                                r.CustomStr5 = "real";
                            } break;
                        /*default:
                            {
                                r.CustomStr5 = "You are so noob that you dont tell me an option like " + r.CustomStr2 + " exists. I'm so sorry";
                            } break;*/
                    }
                    r.UserName = arr[13];
                    r.EventType = arr[14];
                    try
                    {
                        r.EventId = Convert.ToInt64(arr[15]);
                    }
                    catch
                    {
                        r.EventId = 0;
                    }
                    r.UserName = arr[16];
                    r.CustomStr6 = arr[17];
                    switch (r.CustomStr6)
                    {
                        case "c":
                            {
                                r.CustomStr6 = "complete";
                            } break;
                        case "i":
                            {
                                r.CustomStr6 = "incomplete";
                            } break;
                        /*default:
                            {
                                r.CustomStr6 = "You are so noob that you dont tell me an option like " + r.CustomStr2 + " exists. I'm so sorry";
                            } break;*/
                    }
                    if (remoteHost != "")
                        r.CustomStr8 = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.CustomStr8 = arrLocation[2];
                    }

                    r.LogName = LogName;
                    r.ComputerName = Environment.MachineName;

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
            /*String day = "access";
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
            foreach (String file in Directory.GetFiles(Dir))
            {
                if (file.Contains(day))
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
                    if (arr[arr.Length - 1].Contains("xferlog"))
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
