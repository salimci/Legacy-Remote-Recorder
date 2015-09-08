/*
 * DHCP Recorder
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
    public class SymantecWebSecurityRecorder : Parser
    {
        public SymantecWebSecurityRecorder()
            : base()
        {
            LogName = "SymantecWebSecurityRecorder";
            usingKeywords = false;
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

            String[] arr = line.Split(',');
            if (arr.Length < 2)
                return true;

            try
            {
                if (!dontSend)
                {
                    Rec r = new Rec();

                    r.CustomInt1 = 0;
                    r.EventId = 0;
                    r.CustomInt3 = 0;
                    r.UserName = "";
                    r.SourceName = "";
                    r.CustomInt2 = 0;
                    r.Description = "";
                    r.EventCategory = "";
                    r.ComputerName = "";
                    r.EventType = "";

                    StringBuilder dateString = new StringBuilder();
                    Char[] arrDate = arr[0].ToCharArray();
                    dateString.Append(arrDate[0]).Append(arrDate[1]).Append(arrDate[2]).Append(arrDate[3]);
                    dateString.Append("/").Append(arrDate[4]).Append(arrDate[5]);
                    dateString.Append("/").Append(arrDate[6]).Append(arrDate[7]);

                    Char[] arrTime = arr[1].ToCharArray();
                    Int32 check = 0;
                    String second = "";
                    String minute = "";
                    String hour = "";
                    for (Int32 i = arrTime.Length - 1; i != -1; i--)
                    {
                        switch (check)
                        {
                            case 0:
                                {
                                    second = arrTime[i] + second;
                                    if (second.Length == 2)
                                        check++;
                                } break;
                            case 1:
                                {
                                    minute = arrTime[i] + minute;
                                    if (minute.Length == 2)
                                        check++;
                                } break;
                            case 2:
                                {
                                    hour = arrTime[i] + hour;
                                } break;
                        };
                    }

                    if (hour.Length == 1)
                        hour = "0" + hour;
                    else if (hour.Length == 0)
                        hour = "00";
                    if (minute.Length == 1)
                        minute = "0" + minute;
                    else if (minute.Length == 0)
                        minute = "00";
                    if (second.Length == 1)
                        second = "0" + second;
                    else if (second.Length == 0)
                        second = "00";

                    dateString.Append(" ").Append(hour).Append(":").Append(minute).Append(":").Append(second);
                    r.Datetime = dateString.ToString();

                    for (Int32 i = 2; i < arr.Length; i++)
                    {
                        String[] temp = arr[i].Split('=');

                        if(temp.Length != 2)
                            return true;

                        switch (Convert.ToInt32(temp[0]))
                        {
                            case 1:
                                    r.CustomInt1 = Convert.ToInt32(temp[1]);
                                break;
                            case 2:
                                    r.EventId = Convert.ToInt32(temp[1]);
                                break;
                            case 3:
                                    r.CustomInt3 = Convert.ToInt32(temp[1]);
                                break;
                            case 10:
                                    r.UserName = temp[1];
                                break;
                            case 11:
                                    r.SourceName = temp[1];
                                break;
                            case 30:
                                    r.CustomInt2 = Convert.ToInt32(temp[1]);
                                break;
                            case 60:
                                    r.Description = temp[1];
                                break;
                            case 100:
                                    r.EventCategory = temp[1];
                                break;
                            case 1000:
                                    r.ComputerName = temp[1];
                                break;
                            case 1106:
                                    r.EventType = temp[1];
                                break;
                            default:
                                break;
                        }
                    }

                    r.LogName = LogName;

                    SetRecordData(r);
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                return false;
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                DateTime dt = DateTime.Now;
                String day = "symc" + dt.Year.ToString() + ((dt.Month < 10) ? ("0" + dt.Month.ToString()) : dt.Month.ToString()) + ((dt.Day < 10) ? ("0" + dt.Day.ToString()) : dt.Day.ToString());
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
                se.RunCommand("ls -lt " + Dir + " | grep ^-", ref stdOut, ref stdErr);
                StringReader sr = new StringReader(stdOut);
                String line = sr.ReadLine();
                String[] arr = line.Split(' ');

                FileName = Dir + arr[12];
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
