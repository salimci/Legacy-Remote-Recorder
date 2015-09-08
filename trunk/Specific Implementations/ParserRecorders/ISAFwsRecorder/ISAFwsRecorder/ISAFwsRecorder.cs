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
    public class ISAFwsRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public ISAFwsRecorder()
            : base()
        {
            LogName = "ISAFwsRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                ParseFileName();
                if (FileName != fileLast)
                {
                    Stop();
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                    Start();
                }
                else
                {
                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length - 1 > Position)
                    {
                        Stop();                        
                        Start();
                    }
                    else
                    {                      
                    }
                }
            }
            dayChangeTimer.Start();
        }

        public ISAFwsRecorder(String fileName)
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

                    r.ComputerName = arr[dictHash["computer"]];
                    r.UserName = arr[dictHash["username"]];
                    r.Description = arr[dictHash["rule"]];
                    r.EventType = arr[dictHash["action"]];
                    r.CustomStr1 = arr[dictHash["IP protocol"]];

                    String parse = arr[dictHash["source"]];
                    String[] arrIn = parse.Split(':');
                    r.CustomStr3 = arrIn[0];
                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arrIn[1]);
                    }
                    catch
                    {
                        r.CustomInt1 = 0;
                    }

                    parse = arr[dictHash["destination"]];
                    arrIn = parse.Split(':');
                    r.CustomStr4 = arrIn[0];
                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arrIn[1]);
                    }
                    catch
                    {
                        r.CustomInt2 = 0;
                    }

                    r.CustomStr5 = arr[dictHash["source network"]];
                    r.CustomStr6 = arr[dictHash["destination network"]];
                    r.CustomStr7 = arr[dictHash["status"]];
                    r.CustomStr8 = arr[dictHash["application protocol"]];
                    r.CustomStr2 = arr[dictHash["agent"]];
                    try
                    {
                        r.CustomInt9 = Convert.ToInt32(arr[dictHash["bytes sent"]]);
                    }
                    catch
                    {
                        r.CustomInt9 = 0;
                    }
                    try
                    {
                        r.CustomInt10 = Convert.ToInt32(arr[dictHash["bytes sent intermediate"]]);
                    }
                    catch
                    {
                        r.CustomInt10 = 0;
                    }
                    try
                    {
                        r.CustomInt3 = Convert.ToInt32(arr[dictHash["bytes received"]]);
                    }
                    catch
                    {
                        r.CustomInt3 = 0;
                    }
                    try
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[dictHash["bytes received intermediate"]]);
                    }
                    catch
                    {
                        r.CustomInt4 = 0;
                    }
                    try
                    {
                        r.CustomInt5 = Convert.ToInt32(arr[dictHash["connection time"]]);
                    }
                    catch
                    {
                        r.CustomInt5 = 0;
                    }
                    try
                    {
                        r.CustomInt6 = Convert.ToInt32(arr[dictHash["connection time intermediate"]]);
                    }
                    catch
                    {
                        r.CustomInt6 = 0;
                    }
                    try
                    {
                        r.CustomInt7 = Convert.ToInt32(arr[dictHash["session ID"]]);
                    }
                    catch
                    {
                        r.CustomInt7 = 0;
                    }
                    try
                    {
                        r.CustomInt8 = Convert.ToInt32(arr[dictHash["connection ID"]]);
                    }
                    catch
                    {
                        r.CustomInt8 = 0;
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
            Int64 currentPosition = Position;
            FileName = lastFile;
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Last file is : " + FileName);
            string oldFileName = FileName;
            UInt64 oldFileDate = 0;
            UInt64 count = 0;
            Int32 count2 = 0;
            Boolean dayEnds = true;

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                try
                {
                    if (oldFileName == null || oldFileName == "")
                    {
                        foreach (String file in Directory.GetFiles(Dir))
                        {

                            String fname = Path.GetFileName(file);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, file);
                            if (fname.Contains("ISALOG_"))
                            {
                                String[] arr = fname.Split('_');
                                String arrIn = arr[1];
                                Int32 sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
                                UInt64 newCount = Convert.ToUInt64(arrIn);
                                if ((newCount == count && sameDayCount >= count2))
                                {
                                    count = newCount;
                                    count2 = sameDayCount;
                                    FileName = file;                                    
                                }
                                if (newCount > count)
                                {
                                    count = newCount;
                                    FileName = file;
                                    count2 = sameDayCount;
                                }
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Filename is null so getting last File: " + FileName);
                        lastFile = FileName;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Can not find a file to parse..");
                }

                //Is file exist?-------------------------------------------------------------------------------               
                UInt64 fileDate = 0;
                int fileValidFlag = 1;
                foreach (String file in Directory.GetFiles(Dir))
                {
                    fileValidFlag = 0;
                    if (file == lastFile)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Debug: " + FileName + " is a valid file , not changing file");
                        fileValidFlag = 1;
                        break;
                    }
                }
                if (fileValidFlag == 0)
                {
                    Int32 sameDayCount = 0;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Debug: " + FileName + " is not a valid file, so changing file");
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        String fname = Path.GetFileName(file);
                        if (fname.StartsWith("ISALOG_"))
                        {
                            String[] arr = fname.Split('_');
                            String arrIn = arr[1];
                            UInt64 newCount = Convert.ToUInt64(arrIn);
                            sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
                            if (fileDate <= newCount)
                            {
                                fileDate = newCount;
                                FileName = file;
                            }
                        }
                    }
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        String fname = Path.GetFileName(file);
                        if (fname.StartsWith("ISALOG_") && fname.Contains(FileName.Split('_')[1]))
                        {
                            String[] arr = fname.Split('_');
                            String arrIn = arr[3];
                            String arrSameDay = arrIn.Split('.')[0];
                            Int32 minValue = 0;
                            if (minValue < Convert.ToInt32(arrSameDay))
                            {
                                minValue = Convert.ToInt32(arrSameDay);
                                FileName = file;
                            }
                        }
                    }
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File is changed, new file is : " + FileName);
                    fileValidFlag = 1;
                    lastFile = FileName;
                    Position = 0;
                    lastLine = "";
                    return;
                }

                //End of the file?---------------------------------------------------------------------------
                FileInfo fi = new FileInfo(FileName);                
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is: " + Position);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Length is: " + fi.Length);
                oldFileDate = Convert.ToUInt64(oldFileName.Split('_')[1]);
                Int32 oldFileCount = Convert.ToInt32(oldFileName.Split('_')[3].Split('.')[0]);                
                if (Position == fi.Length - 1 || Position == fi.Length)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is at the end of the file so changing the File");
                    count = 9000000000;
                    count2 = 1000; //minCount                    
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        String fname = Path.GetFileName(file);                        
                        if (fname.Contains("ISALOG_"))
                        {
                            String[] arr = fname.Split('_');
                            String arrIn = arr[1];
                            Int32 sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
                            UInt64 newCount = Convert.ToUInt64(arrIn);                            

                            if (newCount == oldFileDate && sameDayCount > oldFileCount && sameDayCount < count2)
                            {
                                count2 = sameDayCount;
                                FileName = file;
                                dayEnds = false;                                
                            }
                            if (newCount > oldFileDate && dayEnds && newCount < count)
                            {
                                count = newCount;
                                FileName = file;                                
                            }
                        }
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal New file is set to: " + FileName);                    
                }
                else
                {                    
                    Log.Log(LogType.FILE, LogLevel.INFORM ,"Position is not at the end of the file so no changing the File");
                }

            }
            else
                FileName = Dir;
            lastFile = FileName;            
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Start()");
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Start()");
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
