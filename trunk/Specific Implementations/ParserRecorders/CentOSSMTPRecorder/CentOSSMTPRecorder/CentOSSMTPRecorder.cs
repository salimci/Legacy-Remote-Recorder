/*
 * CentOS SMTP Recorder
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
    public class CentOSSMTPRecorder : Parser
    {
        public CentOSSMTPRecorder()
            : base()
        {
            LogName = "CentOSSMTPRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            GetFiles();
        }

        public CentOSSMTPRecorder(String fileName)
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
                String[] arr = SpaceSplit(line, false);

                try
                {
                    Rec r = new Rec();

                    DateTime dt = DateTime.Now;
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                    r.SourceName = arr[0];

                    if (arr[1] == "tcpserver:")
                    {
                        r.EventCategory = arr[1];
                        r.EventType = arr[2];
                        switch (arr[2])
                        {
                            case "end":
                                {
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arr[3]);
                                    }
                                    catch
                                    {
                                        r.CustomStr1 = arr[3];
                                    }
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arr[5]);
                                    }
                                    catch
                                    {
                                        r.CustomStr2 = arr[5];
                                    }
                                } break;
                            case "status:":
                                {
                                    String [] arrIn = arr[3].Split('/');
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arrIn[0]);
                                    }
                                    catch
                                    {
                                        r.CustomStr1 = arrIn[0];
                                    }
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrIn[1]);
                                    }
                                    catch
                                    {
                                        r.CustomStr2 = arrIn[1];
                                    }
                                } break;
                            case "pid":
                                {
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arr[3]);
                                    }
                                    catch
                                    {
                                        r.CustomStr1 = arr[3];
                                    }
                                    r.CustomStr2 = arr[5];
                                } break;
                            case "ok":
                                {
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arr[3]);
                                    }
                                    catch
                                    {
                                        r.CustomInt1 = 0;
                                    }
                                    String[] arrIn = arr[4].Split(':');

                                    r.CustomStr1 = arrIn[0];
                                    r.CustomStr2 = arrIn[1];
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrIn[2]);
                                    }
                                    catch
                                    {
                                        r.CustomInt2 = 0;
                                    }

                                    String[] arrIn2 = arr[5].Split(':');
                                    r.CustomStr3 = arrIn2[1];
                                    try
                                    {
                                        r.CustomInt3 = Convert.ToInt32(arrIn2[3]);
                                    }
                                    catch
                                    {
                                        r.CustomInt3 = 0;
                                    }
                                } break;
                        };
                    }
                    else if (arr[1] == "CHKUSER")
                    {
                        r.EventCategory = arr[1];
                        r.EventType = arr[2];
                        String[] arrIn = arr[5].Split(':');
                        r.CustomStr1 = arrIn[0].TrimStart('<');
                        r.CustomStr2 = arrIn[1];
                        r.CustomStr3 = arrIn[2].TrimEnd('>');
                        String[] arrIn2 = arr[7].Split(':');
                        r.CustomStr4 = arrIn2[0].TrimStart('<');
                        r.CustomStr5 = arrIn2[1];
                        r.CustomStr6 = arrIn2[2].TrimEnd('>');
                        r.CustomStr7 = arr[9].TrimStart('<').TrimEnd('>');
                        for (Int32 i = 11; i < arr.Length; i++)
                        {
                            r.Description += arr[i] + " ";
                        }
                        r.Description = r.Description.Trim();
                    }
                    else if (arr[1] == "rblsmtpd:")
                    {
                        r.EventCategory = arr[1];
                        r.EventType = arr[6];
                        r.CustomStr1 = arr[2];
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[4].TrimEnd(':'));
                        }
                        catch
                        {
                            r.CustomInt1 = 0;
                        }
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[5]);
                        }
                        catch
                        {
                            r.CustomInt2 = 0;
                        }
                        r.Description = arr[9];
                    }
                    else
                    {
                        for (Int32 i = 1; i < arr.Length; i++)
                        {
                            r.Description += arr[i] + " ";
                        }
                        r.Description = r.Description.Trim();
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

        protected override void ParseFileNameLocal()
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
                    if (arr[arr.Length - 1].Contains("smtp"))
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

