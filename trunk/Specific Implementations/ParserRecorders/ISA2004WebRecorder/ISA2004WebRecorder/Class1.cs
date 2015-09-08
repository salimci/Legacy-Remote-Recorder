

/*
 * ISA Web Recorder
 * Copyright (C) 2008 Erdoğan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

/*
 ISA Web Recorder 
 * 07.03.2012 Onur Sarıkaya 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using System.Collections;

namespace Parser
{
    public class ISA2004WebRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public ISA2004WebRecorder()
            : base()
        {
            LogName = "ISA2004WebRecorder";
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Enabled = false;

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
                }
            }
            dayChangeTimer.Enabled = true;
        }

        public override void Init()
        {
            GetFiles();
        }

        public ISA2004WebRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | dontSend : " + dontSend);

            Rec r = new Rec();
            if (line == "")
                return true;


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

            if (!dontSend)
            {
                if (!line.StartsWith("#"))
                {
                    string[] values = line.Split('\t');
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | line : " + line);
                    r.ComputerName = values[0];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | ComputerName : " + r.ComputerName);
                    r.UserName = values[1];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | UserName : " + r.UserName);
                    r.CustomStr1 = values[2];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr1 : " + r.CustomStr1);
                    r.Datetime = values[3].Replace("-", "/") + " " + values[4];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Datetime : " + r.Datetime);
                    r.CustomStr2 = values[5];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr2 : " + r.CustomStr2);
                    r.CustomStr10 = values[6];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr10 : " + r.CustomStr10);
                    r.CustomInt3 = Convert.ToInt32(values[7]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomInt3 : " + r.CustomInt3);
                    r.CustomStr3 = values[8];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr3 : " + r.CustomStr3);
                    r.EventCategory = values[9];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | EventCategory : " + r.EventCategory);
                    r.CustomStr6 = values[10];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr6 : " + r.CustomStr6);

                    try
                    {
                        r.EventId = Convert.ToInt32(values[12]);
                    }
                    catch (Exception)
                    {
                        r.EventId = Convert.ToInt32(values[14]); ;
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | EventId : " + r.EventId);
                    r.EventType = values[13];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | EventType : " + r.EventType);
                    r.CustomStr5 = values[15];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomStr5 : " + r.CustomStr5);
                    r.LogName = LogName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | LogName : " + r.LogName);

                    try
                    {
                        if (values[8] == "-")
                        {
                            r.CustomInt1 = 0;
                        }
                        else
                        {
                            r.CustomInt1 = Convert.ToInt32(values[8]);
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomInt1 : " + r.CustomInt1);
                    }
                    catch (Exception)
                    {
                        r.CustomInt1 = Convert.ToInt32(values[8]);
                    }
                    try
                    {
                        if (values[9] == "-")
                        {
                            r.CustomInt2 = 0;
                        }
                        else
                        {
                            r.CustomInt2 = Convert.ToInt32(values[9]);
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | CustomInt2 : " + r.CustomInt2);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    SetRecordData(r);
                }
            }

            //try
            //{
            //    if (line.StartsWith("#"))
            //    {
            //        if (line.StartsWith("#Fields:"))
            //        {
            //            if (dictHash != null)
            //                dictHash.Clear();
            //            dictHash = new Dictionary<String, Int32>();
            //            String[] fields = line.Split('\t');
            //            String[] first = fields[0].Split(' ');
            //            fields[0] = first[1];
            //            Int32 count = 0;
            //            foreach (String field in fields)
            //            {
            //                dictHash.Add(field, count);
            //                count++;
            //            }
            //            String add = "";

            //            foreach (KeyValuePair<String, Int32> kvp in dictHash)
            //            {
            //                add += kvp.Key + ",";
            //            }
            //            SetLastKeywords(add);
            //            keywordsFound = true;
            //        }
            //        else if (line.StartsWith("#Date:"))
            //        {

            //            String[] arr = line.Split('\t');
            //            string dateString = arr[0];
            //            string date = dateString.Substring(6, dateString.Length - 6);
            //            r.Datetime = date.Replace("-", "/");
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
            //        }
            //        return true;
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | " + e.Message);
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | " + e.StackTrace);
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | Line : " + line);
            //    return false;
            //}

            //if (!dontSend)
            //{
            //    //10.1.13.9	anonymous	Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)	2010-04-22	06:34:55	GMISA02	-	adonline.e-kolay.net	10.1.1.15	80	1	374	170	http	GET	http://adonline.e-kolay.net/adnetobject11.asp?target=milliyetgalerihaber&PID=2&c=h&g=h&ks=2	-	12202	Yasak Site	Req ID: 2bd3d572 	Internal	External	0x0	Denied

            //    String[] arr = line.Split('\t');
            //    try
            //    {
            //        dictHash = new Dictionary<String, Int32>();
            //        Log.Log(LogType.FILE, LogLevel.ERROR, "Line is : " + line);
            //        //if (dictHash.Count > 0)
            //        //{
            //        //    Int32 dateIndex = dictHash["date"];
            //        //    arr[dateIndex] = arr[dateIndex].Replace('-', '/');
            //        //    r.Datetime = arr[0] + " " + arr[dictHash["time"]];
            //        //    arr[0] = arr[0].Replace('-', '/');
            //        //    r.Datetime = arr[0];
            //        //}
            //        r.UserName = arr[dictHash["cs-username"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName : " + r.UserName);
            //        r.Description = arr[dictHash["cs-uri"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);

            //        if (r.Description.Length > 900)
            //        {
            //            if (r.Description.Length > 1800)
            //            {
            //                r.CustomStr8 = r.Description.Substring(900, 900);
            //            }
            //            else
            //            {
            //                r.CustomStr8 = r.Description.Substring(900, r.Description.Length - 900 - 2);
            //            }
            //            r.Description = r.Description.Substring(0, 900);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr8");
            //        }
            //        try
            //        {
            //            r.SourceName = arr[dictHash["FilterInfo"]];
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "SourceName : " + r.SourceName);
            //        }
            //        catch
            //        {
            //            r.SourceName = "";
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "SourceName : " + r.SourceName);
            //        }
            //        r.CustomStr3 = arr[dictHash["c-ip"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
            //        r.CustomStr2 = arr[dictHash["r-host"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
            //        r.CustomStr4 = arr[dictHash["r-ip"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
            //        r.CustomStr1 = arr[dictHash["cs-protocol"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
            //        try
            //        {
            //            r.CustomStr5 = arr[dictHash["rule"]];
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
            //        }
            //        catch
            //        {
            //            r.CustomStr5 = "";
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
            //        }
            //        r.CustomStr6 = arr[dictHash["cs-Network"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);
            //        r.CustomStr7 = arr[dictHash["sc-Network"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
            //        try
            //        {
            //            r.CustomInt1 = Convert.ToInt32(arr[dictHash["r-port"]]);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + r.CustomInt1);
            //        }
            //        catch
            //        {
            //            r.CustomInt1 = 0;
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1 : " + r.CustomInt1);
            //        }
            //        try
            //        {
            //            r.CustomInt2 = Convert.ToInt32(arr[dictHash["time-taken"]]);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + r.CustomInt2);
            //        }
            //        catch
            //        {
            //            r.CustomInt2 = 0;
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + r.CustomInt2);
            //        }
            //        try
            //        {
            //            r.EventType = arr[dictHash["action"]];
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
            //        }
            //        catch
            //        {
            //            r.EventType = "";
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);

            //        }
            //        r.LogName = LogName;
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
            //        r.ComputerName = arr[dictHash["s-computername"]];
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
            //        SetRecordData(r);
            //    }
            //    catch (Exception e)
            //    {
            //        Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            //        Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
            //        Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
            //        return true;
            //    }
            //}
            return true;
        }

        //protected override void ParseFileNameLocal()
        //{
        //    FileStream fs = null;
        //    BinaryReader br = null;
        //    Int64 currentPosition = Position;
        //    FileName = lastFile;
        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Last file is : " + FileName);
        //    string oldFileName = FileName;
        //    UInt64 oldFileDate = 0;
        //    UInt64 count = 0;
        //    Int32 count2 = 0;
        //    Boolean dayEnds = true;

        //    if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
        //    {
        //        try
        //        {
        //            if (oldFileName == null || oldFileName == "")
        //            {
        //                foreach (String file in Directory.GetFiles(Dir))
        //                {

        //                    String fname = Path.GetFileName(file);
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, file);
        //                    if (fname.Contains("ISALOG_") && fname.EndsWith(".w3c"))
        //                    {
        //                        String[] arr = fname.Split('_');
        //                        String arrIn = arr[1];
        //                        Int32 sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
        //                        UInt64 newCount = Convert.ToUInt64(arrIn);
        //                        if ((newCount == count && sameDayCount >= count2))
        //                        {
        //                            count = newCount;
        //                            count2 = sameDayCount;
        //                            FileName = file;

        //                        }
        //                        if (newCount > count)
        //                        {
        //                            count = newCount;
        //                            FileName = file;
        //                            count2 = sameDayCount;
        //                        }
        //                    }
        //                }

        //                Log.Log(LogType.FILE, LogLevel.INFORM, "Filename is null so getting last File: " + FileName);
        //                lastFile = FileName;
        //                return;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Can not find a file to parse..");
        //        }

        //        //Is file exist?
        //        //Is file exist
        //        UInt64 fileDate = 0;
        //        int fileValidFlag = 1;
        //        foreach (String file in Directory.GetFiles(Dir))
        //        {

        //            fileValidFlag = 0;
        //            if (file.EndsWith(".w3c"))
        //                if (file == lastFile)
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, "Debug: " + FileName + " is a valid file , not changing file");
        //                    fileValidFlag = 1;
        //                    break;
        //                }

        //        }
        //        if (fileValidFlag == 0)
        //        {
        //            Int32 sameDayCount = 0;
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "Debug: " + FileName + " is not a valid file, so changing file");
        //            foreach (String file in Directory.GetFiles(Dir))
        //            {
        //                String fname = Path.GetFileName(file);
        //                if (fname.StartsWith("ISALOG_") && fname.EndsWith(".w3c"))
        //                {
        //                    String[] arr = fname.Split('_');
        //                    String arrIn = arr[1];
        //                    UInt64 newCount = Convert.ToUInt64(arrIn);
        //                    sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
        //                    if (fileDate <= newCount)
        //                    {
        //                        fileDate = newCount;
        //                        FileName = file;
        //                    }
        //                }
        //            }
        //            foreach (String file in Directory.GetFiles(Dir))
        //            {
        //                String fname = Path.GetFileName(file);
        //                if (fname.StartsWith("ISALOG_") && fname.Contains(FileName.Split('_')[1]) && fname.EndsWith(".w3c"))
        //                {
        //                    String[] arr = fname.Split('_');
        //                    String arrIn = arr[3];
        //                    String arrSameDay = arrIn.Split('.')[0];
        //                    Int32 minValue = 0;
        //                    if (minValue < Convert.ToInt32(arrSameDay))
        //                    {
        //                        minValue = Convert.ToInt32(arrSameDay);
        //                        FileName = file;
        //                    }
        //                }
        //            }
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "File is changed, new file is : " + FileName);
        //            fileValidFlag = 1;
        //            lastFile = FileName;
        //            Position = 0;
        //            lastLine = "";
        //            return;
        //        }

        //        // End of the file?------------------------------------------------------------
        //        FileInfo fi = new FileInfo(FileName);
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is: " + Position.ToString());
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Length is: " + fi.Length.ToString());
        //        oldFileDate = Convert.ToUInt64(oldFileName.Split('_')[1]);
        //        Int32 oldFileCount = Convert.ToInt32(oldFileName.Split('_')[3].Split('.')[0]);

        //        if (Position == fi.Length - 1 || Position == fi.Length)
        //        {
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "Position is at the end of the file so changing the File");
        //            count = 9000000000;
        //            count2 = 1000; //minCount                    
        //            foreach (String file in Directory.GetFiles(Dir))
        //            {
        //                String fname = Path.GetFileName(file);
        //                if (fname.Contains("ISALOG_") && fname.EndsWith(".w3c"))
        //                {
        //                    String[] arr = fname.Split('_');
        //                    String arrIn = arr[1];
        //                    Int32 sameDayCount = Convert.ToInt32(arr[3].Split('.')[0]);
        //                    UInt64 newCount = Convert.ToUInt64(arrIn);

        //                    if (newCount == oldFileDate && sameDayCount > oldFileCount && sameDayCount < count2)
        //                    {
        //                        count2 = sameDayCount;
        //                        FileName = file;
        //                        dayEnds = false;
        //                    }
        //                    if (newCount > oldFileDate && dayEnds && newCount < count)
        //                    {
        //                        count = newCount;
        //                        FileName = file;
        //                    }
        //                }
        //            }
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal New file is set to: " + FileName);
        //        }
        //        else
        //        {
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "Position is not at the end of the file so no changing the File");
        //        }

        //    }
        //    else
        //        FileName = Dir;
        //    lastFile = FileName;
        //}

        protected override void ParseFileNameLocal()
        {

            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Searching in directory: " + Dir);

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {

                ArrayList arrFileNames = new ArrayList();

                foreach (String sFileName in Directory.GetFiles(Dir))
                {
                    string sFile = Path.GetFileName(sFileName).ToString();
                    if (sFile.StartsWith("ISALOG_") && sFile.EndsWith(".w3c"))
                    {
                        arrFileNames.Add(sFile);
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal1() | " + sFile);
                    }
                }

                Sorter[] dSortTemp = new Sorter[arrFileNames.Count];
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    //ISALOG_20090805_WEB_000.w3c
                    string sFileName = arrFileNames[i].ToString();
                    String[] arr = sFileName.Split('.');
                    String[] arr1 = arr[0].Split('_');
                    dSortTemp[i] = new Sorter(Convert.ToUInt64(arr1[1]), Convert.ToUInt64(arr1[3]), sFileName);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal2() | " + sFileName);

                }

                Array.Sort(dSortTemp);

                string[] dFileNames = new string[arrFileNames.Count];

                for (int i = 0; i < dSortTemp.Length; i++)
                {
                    dFileNames[i] = dSortTemp[i].name;
                }


                if (String.IsNullOrEmpty(lastFile))
                {
                    if (dFileNames.Length > 0)
                    {
                        FileName = Dir + dFileNames[dFileNames.Length - 1].ToString();
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Last File Is Null So Last File Setted The First File " + lastFile);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Last File Is Null And There Is No File To Set");
                    }
                }
                else
                {
                    if (File.Exists(lastFile))
                    {
                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, enc);
                        br.BaseStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;

                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                            c = br.ReadChar();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Char Is" + c.ToString());

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }


                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position Is : " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Length Is : " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length || br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            for (int i = 0; i < dFileNames.Length; i++)
                            {
                                if (Dir + dFileNames[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNames.Length)
                                    {
                                        FileName = lastFile;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | There Is No New Fıle and Waiting For New Record");
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + dFileNames[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Yeni FileName: " + FileName);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Continue reading the last file : " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Last File Not Found : " + lastFile);
                        SetNextFile(dFileNames, "ParseFileNameLocal");

                    }
                }


            }
            else
                FileName = Dir;


            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() Exit | Filename is: " + FileName);

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
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            }
            base.Start();
        }



        [Serializable]
        public class Sorter : IComparable
        {
            public Sorter(UInt64 id, UInt64 count, string name)
            {
                this.name = name;
                this.id = id;
                this.count = count;
            }
            public int CompareTo(object obj)
            {
                try
                {
                    Sorter p = (Sorter)obj;

                    int sonuc = id.CompareTo(p.id);
                    if (sonuc == 0)
                    {
                        sonuc = count.CompareTo(p.count);
                    }
                    return sonuc;
                }
                catch (Exception exp)
                {
                    return 0;
                }
            }
            public string name;
            public UInt64 id;
            public UInt64 count;
        }

        /// <summary>
        /// LastFile'ın Numarasına Göre Bir Sonraki Dosyayı Set Eder
        /// </summary>
        /// <param name="dFileNameList"></param>
        private void SetNextFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                       sFunction + " | SetNextFile() ");

                UInt64 lastFileDate = 0;
                UInt64 lastFileNumber = 0;

                String[] arr = Path.GetFileName(lastFile).ToString().Split('.');
                String[] arr1 = arr[0].Split('_');

                lastFileDate = Convert.ToUInt64(arr1[1]);
                lastFileNumber = Convert.ToUInt64(arr1[3]);

                for (int i = 0; i < dFileNameList.Length; i++)
                {

                    String[] ar = dFileNameList[i].Split('.');
                    String[] ar1 = ar[0].Split('_');

                    UInt64 FileDate = Convert.ToUInt64(ar1[1]);
                    UInt64 FileNumber = Convert.ToUInt64(ar1[3]);


                    if (FileDate == lastFileDate)
                    {
                        if (FileNumber > lastFileNumber)
                        {
                            FileName = Dir + dFileNameList[i].ToString();
                            lastFile = FileName;
                            break;
                        }
                    }
                    else
                        if (FileDate > lastFileDate)
                        {
                            FileName = Dir + dFileNameList[i].ToString();
                            lastFile = FileName;
                            break;
                        }

                }

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                          sFunction + " | LastFile Silinmis Dosya Bulunamadı.Bir sonraki dosya. LastFile : " + FileName);

            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SetNextFile() | " + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "SetNextFile() | " + exp.StackTrace);

            }

        }




    }
}
