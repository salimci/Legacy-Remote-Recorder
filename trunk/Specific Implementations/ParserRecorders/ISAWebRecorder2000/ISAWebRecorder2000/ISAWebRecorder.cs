
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
    public class ISAWeb2000Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public ISAWeb2000Recorder()
            : base()
        {
            LogName = "ISAWeb2000Recorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public ISAWeb2000Recorder(String fileName)
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
                    r.CustomStr2 = arr[dictHash["r-host"]];
                    r.CustomStr4 = arr[dictHash["r-ip"]];
                    r.CustomStr1 = arr[dictHash["cs-referred"]];
                    r.CustomStr5 = arr[dictHash["s-operation"]];
                    r.CustomStr6 = arr[dictHash["sc-bytes"]];
                    r.CustomStr7 = arr[dictHash["cs-bytes"]];
                    r.CustomStr8 = arr[dictHash["s-object-source"]];
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
                        r.EventType = arr[dictHash["action"]];
                    }
                    catch
                    {
                        r.EventType = "";
                    }
                    r.LogName = LogName;
                    r.ComputerName = arr[dictHash["s-computername"]];

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
                DateTime dt = DateTime.Now;
                String day = dt.Year.ToString() + ((dt.Month < 10) ? ("0" + dt.Month.ToString()) : dt.Month.ToString()) + ((dt.Day < 10) ? ("0" + dt.Day.ToString()) : dt.Day.ToString());
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains(day) && file.Contains("WEB"))
                    {
                        FileName = file;
                        break;
                    }
                }
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
