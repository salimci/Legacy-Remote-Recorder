/*
 * DNS Debug Recorder Recorder
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

//#define OLDSYNTAX

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Globalization;

namespace Parser
{
    public class DNSDebugRecorder : Parser
    {

        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        public DNSDebugRecorder()
            : base()
        {
            LogName = "DNSDebugRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public DNSDebugRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
            if (line == "" || !Char.IsNumber(line, 0))
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true, '[', ']');

                for (int i = 0; i < arr.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "arr[" + i + "]: " + arr[i]);
                }

                if (arr.Length < 13)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r = new Rec();
                    StringBuilder sb = new StringBuilder();
                    //sb.Append(arr[0]).Append(" ").Append(arr[1]);
                    //DateTime dt = DateTime.ParseExact(sb.ToString(), "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
                    //sb.Remove(0, sb.Length);
                    //sb.Append(dt.Year).Append("/").Append(dt.Month).Append("/").Append(dt.Day).Append(" ").Append(dt.Hour).Append(":").Append(dt.Minute).Append(":").Append(dt.Second);
                    //r.Datetime = sb.ToString();

                    string[] arr1 = SpaceSplit(line, true);

                    string day = arr[0].Split('/')[1];
                    string month = arr[0].Split('/')[0];
                    string year = arr[0].Split('/')[2];

                    string time = arr[1];
                    string myDateTimeString = year + ", " + month + "," + day + "  ," + time;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Date: " + myDateTimeString);

                    DateTime dt123 = new DateTime();
                    dt123 = Convert.ToDateTime(myDateTimeString);
                    //Log.Log(LogType.FILE, LogLevel.INFORM, "1_ " + arr[0] + "2_ " + arr[1]);
                    //Log.Log(LogType.FILE, LogLevel.INFORM, "1_ dt123 : " + dt123.ToString(dateFormat));

                    r.Datetime = dt123.ToString(dateFormat);

                    try
                    {
                        if (arr.Length > 2)
                        {
                            r.CustomInt10 = Convert.ToInt64(arr[2]);
                        }
                    }
                    catch
                    {
                        r.CustomInt10 = 0;
                    }

                    if (arr.Length > 3)
                    {
                        r.CustomStr10 = arr[3];
                    }

                    if (arr.Length > 4)
                    {
                        r.CustomStr9 = arr[4];
                    }
                    if (arr.Length > 5)
                    {
                        r.CustomStr4 = arr[5];
                    }
                    if (arr.Length > 6)
                    {
                        r.EventType = arr[6];
                    }
                    if (arr.Length > 7)
                    {
                        r.CustomStr1 = arr[7];
                    }
                    if (arr.Length > 8)
                    {
                        r.CustomStr5 = arr[8];
                    }
                    Int32 count = 9;
                    //#if OLDSYNTAX
                    //                    if (arr[9] != "R")
                    //                        r.EventCategory = "Query";
                    //                    else
                    //                    {
                    //                        count = 10;
                    //                        r.EventCategory = "Response";
                    //                    }
                    //#else
                    if (arr.Length > 9)
                    {
                        if (arr[9] != "R")
                            r.EventCategory = "";
                        else
                        {
                            count = 10;
                            r.EventCategory = "R";
                        }
                    }
                    //#endif
                    //r.CustomStr6 = arr[count++];
                    //#if OLDSYNTAX
                    //                    switch (r.CustomStr6)
                    //                    {
                    //                        case "Q":
                    //                            r.CustomStr6 = "Standard Query";
                    //                            break;
                    //                        case "N":
                    //                            r.CustomStr6 = "Notify";
                    //                            break;
                    //                        case "U":
                    //                            r.CustomStr6 = "Update";
                    //                            break;
                    //                        case "?":
                    //                            r.CustomStr6 = "Unknown";
                    //                            break;
                    //                        default:
                    //                            r.CustomStr6 = "Unknown-Default";
                    //                            break;
                    //                    };
                    //#endif
                    String[] arrTemp = SpaceSplit(arr[count], true);

                    if (arrTemp.Length == 3)
                    {
                        //#if OLDSYNTAX
                        //                        String temp = "";
                        //                        switch (arrTemp[1])
                        //                        {
                        //                            case "A":
                        //                                temp = "Authoritative Answer";
                        //                                break;
                        //                            case "T":
                        //                                temp = "Truncated Response";
                        //                                break;
                        //                            case "D":
                        //                                temp = "Recursion Desired";
                        //                                break;
                        //                            case "R":
                        //                                temp = "Recursion Available";
                        //                                break;
                        //                            default:
                        //                                temp = "Unknown";
                        //                                break;
                        //                        };
                        //                        r.CustomStr2 = temp;
                        //#else
                        //                        r.CustomStr2 = arrTemp[1];
                        //#endif
                        if (arrTemp.Length > 0)
                        {
                            r.Description = arrTemp[0];
                        }
                        if (arrTemp.Length > 2)
                        {
                            r.CustomStr7 = arrTemp[2];
                        }
                    }
                    else
                    {
                        if (arrTemp.Length > 0)
                        {
                            r.Description = arrTemp[0];
                        }

                        if (arrTemp.Length > 1)
                        {
                            r.CustomStr7 = arrTemp[1];
                        }
                    }
                    count++;

                    try
                    {
                        r.CustomStr8 = arr[count++];

                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomStr8 Error. " + exception.Message);
                    }

                    try
                    {
                        r.CustomStr3 = arr[count++];
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.WARN, "CustomStr3 Error. " + exception.Message);
                    }

                    // 13.08.2013 Onur SARIKAYA ekledi.
                    try
                    {
                        if (arr.Length > 14)
                        {
                            r.CustomStr6 = arr[14];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + r.CustomStr6);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 Error. " + exception.Message);
                    }

                    r.LogName = LogName;
                    r.ComputerName = Environment.MachineName;

                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record sending. Position: " + Position + " line: " + line);
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record succesfully sended.");

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
                String day = "messages";
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
