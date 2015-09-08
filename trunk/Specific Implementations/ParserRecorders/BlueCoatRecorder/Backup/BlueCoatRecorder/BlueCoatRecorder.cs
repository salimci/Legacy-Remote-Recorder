/*
 * Blue Coat Recorder
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
using System.Collections;

namespace Parser
{
    public class BlueCoatRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;
        public BlueCoatRecorder()
            : base()
        {
            LogName = "BlueCoatRecorder";
        }
        public override void Init()
        {
            GetFiles();
        }

        public BlueCoatRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
            if (line == "")
                return true;
            if (line.StartsWith("#"))
            {
                if (line.StartsWith("#Fields:"))
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    String[] fields = line.Split(' ');
                    Int32 count = 0;
                    foreach (String field in fields)
                    {
                        if (field == "#Fields:")
                            continue;
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
                String[] arr = SpaceSplit(line, false, '"');

                try
                {
                    Rec r = new Rec();

                    Int32 dateIndex = dictHash["date"];
                    arr[dateIndex] = arr[dateIndex].Replace('-', '/');
                    r.Datetime = arr[dateIndex] + " " + arr[dictHash["time"]];
                    /*DateTime dt = DateTime.Parse("asd");
                    Int32 timeTaken = Convert.ToInt32(arr[dictHash["time-taken"]]);
                    dt.Add(new TimeSpan(0, 0, 0, 0, timeTaken));

                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.ToLongTimeString();*/

                    try
                    {
                        r.CustomStr3 = arr[dictHash["c-ip"]];
                    }
                    catch { }
                    try
                    {
                        r.UserName = arr[dictHash["cs-username"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomStr2 = arr[dictHash["cs-auth-group"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomStr1 = arr[dictHash["x-exception-id"]];
                    }
                    catch { }
                    try
                    {
                        r.EventType = arr[dictHash["sc-filter-result"]];
                    }
                    catch { }
                    try
                    {
                        r.EventCategory = arr[dictHash["cs-categories"]].TrimEnd('"').TrimStart('"');
                    }
                    catch { }
                    try
                    {
                        if (arr[dictHash["cs(Referer)"]].Length < 890)
                            r.Description = arr[dictHash["cs(Referer)"]];
                        else if (arr[dictHash["cs(Referer)"]].Length < 8888)
                        {
                            r.Description = arr[dictHash["cs(Referer)"]].Substring(0, 890);
                            r.CustomStr8 = arr[dictHash["cs(Referer)"]].Substring(890, arr[dictHash["cs(Referer)"]].Length);
                        }
                    }
                    catch { }
                    try
                    {
                        r.EventId = Convert.ToInt64(arr[dictHash["sc-status"]]);
                    }
                    catch { }
                    try
                    {
                        r.CustomStr5 = arr[dictHash["s-action"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomStr6 = arr[dictHash["cs-method"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomStr7 = arr[dictHash["rs(Content-Type)"]];
                    }
                    catch { }
                    try
                    {
                        r.SourceName = arr[dictHash["cs-host"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[dictHash["cs-uri-port"]]);
                    }
                    catch { }
                    try
                    {
                        r.CustomStr10 = arr[dictHash["cs-uri-path"]];
                    }
                    catch { }
                    try
                    {
                        if (arr[dictHash["cs-uri-query"]].Length < 2000)
                            r.CustomStr4 = arr[dictHash["cs-uri-query"]];
                        else
                            r.CustomStr4 = arr[dictHash["cs-uri-query"]].Substring(0, 2000);
                    }
                    catch { }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(arr[dictHash["cs-uri-scheme"]]).Append("-").Append(arr[dictHash["cs(User-Agent)"]].TrimStart('"').TrimEnd('"'));
                        r.CustomStr9 = sb.ToString();
                    }
                    catch { }
                    try
                    {
                        r.ComputerName = arr[dictHash["s-ip"]];
                    }
                    catch { }
                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[dictHash["sc-bytes"]]);
                    }
                    catch { }
                    try
                    {
                        r.CustomInt3 = Convert.ToInt32(arr[dictHash["cs-bytes"]]);
                    }
                    catch { }

                    r.LogName = LogName;

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
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Day Change Timer File is: " + FileName);
            }
            dayChangeTimer.Enabled = true;
        }

        protected override void ParseFileNameLocal()
        {

            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Searching in directory: " + Dir);
            
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {

                char[] charsToTrim = { 'd', 'u', 'p' };
                string[] dFileNames = Directory.GetFiles(Dir);
                Sorter[] dSortTemp = new Sorter[dFileNames.Length];

                for (int i = 0; i < dFileNames.Length; i++)
                {
                    string sFile = Path.GetFileName(dFileNames[i]).ToString();

                    if (sFile.StartsWith("SG_main__") == true)
                    {
                        //String[] arr1 = Path.GetFileName(sFile).Split('_');
                        String[] arr1 = sFile.Split('_');
                        String[] arrIn1 = arr1[3].Split('.');
                        dSortTemp[i] = new Sorter(Convert.ToUInt64(arrIn1[0]), 0, dFileNames[i].ToString());
                    }
                    else
                        if (sFile.StartsWith("dup") == true)
                        {
                            //String[] arr1 = Path.GetFileName(sFile).Split('_');
                            String[] arr1 = sFile.Split('_');
                            String[] arrIn1 = arr1[4].Split('.');
                            dSortTemp[i] = new Sorter(Convert.ToUInt64(arrIn1[0]), Convert.ToUInt64(arr1[0].ToString().Trim(charsToTrim)), dFileNames[i].ToString());
                        }
                        else
                        {
                            dSortTemp[i] = new Sorter(0, 0, "a");
                        }

                }

                Array.Sort(dSortTemp);

                for (int i = 0; i < dSortTemp.Length; i++)
                {
                    dFileNames[i] = dSortTemp[i].name;
                }

                if (string.IsNullOrEmpty(lastFile) == false)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile :" + lastFile);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile varmý Kontrol Ediliyor!");

                    if (File.Exists(lastFile))
                    {

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile is not null  : " + lastFile);
                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, enc);

                        br.BaseStream.Seek(Position, SeekOrigin.Begin);

                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNames.Length; i++)
                            {
                                if (dFileNames[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNames.Length)
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);
                                        break;
                                    }
                                    else
                                    {
                                        FileName = dFileNames[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya  : " + FileName);
                                        break;

                                    }

                                }
                            }
                        }
                        else
                        {

                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Dosya Sonu Okunmadý Okuma Devam Ediyor");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameLocal() | FileName = LastFile " + lastFile);

                        }

                        br.Close();
                        fs.Close();

                    }
                    else
                    {
                        SetBirSonrakiFile(dFileNames, " ParseFileNameLocal() ");
                    }

                }
                else
                {
                    if (dFileNames.Length > 0)
                    {
                        FileName = dFileNames[dFileNames.Length - 1];
                        lastFile = FileName;
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                               "ParseFileNameLocal() | LastFile Ýs Null Ýlk FileName Set  : " + FileName);
                    }
                }
            }
            else
            {
                FileName = Dir;
                lastFile = FileName;
            }

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

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Start File is: " + FileName);

        }

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
        /// LastFile'ýn Numarasýna Göre Bir Sonraki Dosyayý Set Eder
        /// </summary>
        /// <param name="dFileNameList"></param>
        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG,
                  sFunction + " | LastFile Silinmis Dosya Bulunamadý ");

            UInt64 lLastFileNumber = 0;
            UInt64 lFileNumber=0;

            if (lastFile.StartsWith("SG_main__") == true)
            {
                
                String[] arr1 = lastFile.Split('_');
                String[] arrIn1 = arr1[3].Split('.');
                lLastFileNumber = Convert.ToUInt64(arrIn1[0]);
            }
            else
                if (lastFile.StartsWith("dup") == true)
                {
                    String[] arr1 = lastFile.Split('_');
                    String[] arrIn1 = arr1[4].Split('.');
                    lLastFileNumber = Convert.ToUInt64(arrIn1[0]);
                }



            for (int i = 0; i < dFileNameList.Length; i++)
            {

                if (dFileNameList[i].StartsWith("SG_main__") == true)
                {

                    String[] arr1 = dFileNameList[i].Split('_');
                    String[] arrIn1 = arr1[3].Split('.');
                    lLastFileNumber = Convert.ToUInt64( arrIn1[0]);
                }
                else
                    if (dFileNameList[i].StartsWith("dup") == true)
                    {
                        String[] arr1 = dFileNameList[i].Split('_');
                        String[] arrIn1 = arr1[4].Split('.');
                        lLastFileNumber = Convert.ToUInt64(arrIn1[0]);
                    }

                if (lFileNumber > lLastFileNumber)
                {
                    FileName = Dir + dFileNameList[i].ToString();
                    Position = 0;
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                        sFunction + " | LastFile Silinmis Dosya Bulunamadý.Bir sonraki dosya. LastFile : " + FileName);
                    break;

                }
            }
        }


    }
}
