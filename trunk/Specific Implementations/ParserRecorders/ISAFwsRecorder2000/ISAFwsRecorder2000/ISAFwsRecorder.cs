/*
 * ISA Fsw Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
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

namespace Parser
{
    public class ISAFws2000Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public ISAFws2000Recorder()
            : base()
        {
            LogName = "ISAFws2000Recorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public ISAFws2000Recorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;
            if (line.StartsWith("#"))
            {
                if (line.StartsWith("#Fields:"))
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    String[] fields = line.Split('\t');
                    String[] first = fields[0].Split(' ');
                    fields[0] = first[1];
                    Int32 count = 0;
                    foreach (String field in fields)
                    {
                        dictHash.Add(field, count);
                        count++;
                    }
                    String add = "";
                    foreach (KeyValuePair<String, Int32> kvp in dictHash)
                    {
                        add += kvp.Key + ",";
                    }
                    SetLastKeywords(add);
                    keywordsFound = true;
                }
                return true;
            }

            if (!dontSend)
            {
                String[] arr = line.Split('\t');

                try
                {
                    Rec r = new Rec();
                    Int32 dateIndex = dictHash["date"];
                    arr[dateIndex] = arr[dateIndex].Replace('-', '/');
                    r.Datetime = arr[dateIndex] + " " + arr[dictHash["time"]];
                    r.UserName = arr[dictHash["cs-username"]];
                    try
                    {
                        r.Description = arr[dictHash["cs-uri"]];
                    }
                    catch
                    {
                        r.Description = "";
                    }
                    r.SourceName = arr[dictHash["c-agent"]];
                    r.CustomStr3 = arr[dictHash["c-ip"]];
                    r.CustomStr9 = arr[dictHash["r-host"]];
                    r.CustomStr4 = arr[dictHash["r-ip"]];
                    r.CustomStr1 = arr[dictHash["cs-protocol"]];
                    r.CustomStr6 = arr[dictHash["cs-transport"]];
                    r.CustomStr2 = arr[dictHash["s-operation"]];
                    r.CustomStr7 = arr[dictHash["sc-bytes"]];
                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[dictHash["r-port"]]);
                    }
                    catch
                    {
                        r.CustomInt1 = 0;
                    }
                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[dictHash["time-taken"]]);
                    }
                    catch
                    {
                        r.CustomInt2 = 0;
                    }
                    try
                    {
                        r.EventCategory = arr[dictHash["action"]];
                    }
                    catch
                    {
                        r.EventCategory = "";
                    }
                    r.LogName = LogName;
                    r.ComputerName = arr[dictHash["s-computername"]];

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
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                DateTime dtLast = new DateTime(1970, 12, 12);
                String fileLast = "";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    DateTime dt = File.GetLastAccessTime(file);
                    if (dt > dtLast && file.Contains("FWS"))
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

        public override void Start()
        {
            try
            {
                String keywords = GetLastKeywords();
                String[] arr = keywords.Split(',');
                if (dictHash == null)
                    dictHash = new Dictionary<String, Int32>();
                if (arr.Length > 2)
                    dictHash.Clear();
                Int32 count = 0;
                foreach (String keyword in arr)
                {
                    if (keyword == "")
                        continue;
                    dictHash.Add(keyword, count);
                    count++;
                }
            }
            catch(Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            }
            base.Start();
        }
    }
}
