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
    public class BindRecorder : Parser
    {
        public BindRecorder()
            : base()
        {
            LogName = "BindRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public BindRecorder(String fileName)
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

                if (arr.Length < 5)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                {
                    Rec r = new Rec();

                    DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                    r.SourceName = arr[3];
                    r.EventCategory = arr[4];
                    r.Description = "";

                    bool desc = true;

                    if (r.EventCategory.StartsWith("sshd(pam_unix)"))
                    {
                        bool end = false;
                        desc = false;
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (!end)
                            {
                                if ((i + 2) < arr.Length)
                                {
                                    if (arr[i + 2] == "by")
                                    {
                                        r.UserName = arr[i + 1];
                                        i++;
                                        end = true;
                                    }
                                    else
                                        r.EventType += arr[i] + " ";
                                }
                                else
                                {
                                    if (r.EventType != "")
                                    {
                                        r.Description += r.EventType;
                                        r.EventType = "";
                                    }
                                    r.Description += arr[i] + " ";
                                }
                            }
                            else
                            {
                                r.Description += arr[i] + " ";
                            }
                        }
                        r.EventCategory = "sshd(pam_unix)";
                    }
                    else if(r.EventCategory.StartsWith("named"))
                    {
                        if(arr[5] == "lame")
                        {
                            desc = false;
                            Int32 i = 5;
                            for (i = 5; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("'"))
                                {
                                    r.EventType = r.EventType.Trim();
                                    break;
                                }

                                r.EventType += arr[i] + " ";
                            }

                            arr[i] = arr[i].Trim('\'');
                            r.CustomStr1 = arr[i];
                            i += 2;

                            String[] arrIn = arr[i].Split('\'');
                            r.CustomStr2 = arrIn[1];
                            i++;

                            r.CustomStr3 = arr[i];
                        }
                        else if(arr[5] == "client")
                        {
                            if (arr.Length > 10)
                            {
                                desc = false;
                                r.EventType = arr[5];

                                String[] arrIn = arr[6].Split('#');
                                r.CustomStr1 = arrIn[0];
                                try
                                {
                                    r.CustomInt1 = Convert.ToInt32(arrIn[1]);
                                }
                                catch
                                {
                                    r.CustomInt1 = 0;
                                }
                                r.CustomStr2 = arr[7].TrimEnd(':');
                                r.CustomStr3 = arr[8];
                                r.CustomStr4 = arr[9];
                                r.CustomStr5 = arr[10];
                            }
                        }
                    }

                    if (desc)
                    {
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            r.Description += arr[i] + " ";
                        }
                    }

                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.EventId = Convert.ToInt64(arrEv[1]);
                        }
                        catch
                        {
                            r.EventId = 0;
                        }
                    }

                    r.Description = r.Description.Trim();

                    r.LogName = LogName;
                    r.ComputerName = Environment.MachineName;

                    SetRecordData(r);
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
                    if (arr[arr.Length - 1].Contains("messages") && !arr[arr.Length - 1].Contains("log"))
                    {
                        FileName = Dir + arr[arr.Length - 1];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "File Name Is: " + FileName);
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
