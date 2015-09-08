/*
 * Redhat Secure Recorder
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
using System.Collections;

namespace Parser
{
    public class SolarisSecureRecorder : Parser
    {
        public SolarisSecureRecorder()
            : base()
        {
            LogName = "SolarisSecureRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }
        public SolarisSecureRecorder(String fileName)
            : base(fileName)
        {
        }
        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");

            if (string.IsNullOrEmpty(line) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "line is nul or empty");
                return true;
            }

            Rec rRec = new Rec();

            if (!dontSend)
            {

                String[] arr = SpaceSplit(line, true);
                ArrayList dictHash = new ArrayList();
                Int32 count = 0;
                foreach (String field in arr)
                {

                    if (string.IsNullOrEmpty(field) == false)
                    {
                        dictHash.Add(field);
                    }
                    count++;
                }

                rRec.LogName = LogName;
                rRec.ComputerName = remoteHost;


                rRec.Datetime = Convert.ToDateTime(arr[0] + " " + arr[1] + " " + DateTime.Now.Year.ToString() + " " + arr[2]).ToString("yyyy/MM/dd HH:mm:ss");
                rRec.SourceName = arr[3];

                if (dictHash.Contains("[ID") == true)
                {
                    rRec.CustomInt1 = Convert.ToInt32(dictHash[dictHash.IndexOf("[ID") + 1]);
                    rRec.CustomStr2 = dictHash[dictHash.IndexOf("[ID") + 2].ToString().Replace(']', ' ');
                }
                if (dictHash.Count > 4)
                {
                    string[] ssh = dictHash[4].ToString().Split('[');

                    if (ssh.Length > 1)
                        rRec.CustomStr4 = ssh[0].ToString();
                }

                if (dictHash.Contains("from") == true)
                {
                    rRec.CustomStr3 = dictHash[dictHash.IndexOf("from") + 1].ToString();
                }
                if (dictHash.Contains("for") == true)
                {
                    rRec.UserName = dictHash[dictHash.IndexOf("for") + 1].ToString();
                }

                if (dictHash.Contains("[ID") == true)
                {

                    if (dictHash.Contains("for") == true)
                    {
                        int start = dictHash.IndexOf("[ID") + 3;
                        int End = dictHash.IndexOf("for");

                        for (int i = start; i < End; i++)
                        {
                            string sKelime = dictHash[i].ToString();

                            if (sKelime.StartsWith("\'") == true)
                            {
                                rRec.EventType = sKelime.Replace('\'', ' ').Trim();

                            }
                            else
                                if (sKelime.EndsWith("\'") == true)
                                {
                                    rRec.CustomStr2 = sKelime.Replace('\'', ' ').Trim();
                                }
                                else
                                    if (sKelime.EndsWith(":") == true)
                                    {
                                        rRec.EventType = sKelime;
                                    }
                                    else
                                    {
                                        rRec.EventType += " " + sKelime;
                                    }

                        }

                    }

                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SetRecordData");
                SetRecordData(rRec);


            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                String day = "secure";
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
                    if (arr[arr.Length - 1].Contains("secure"))
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
