/*
 * Isa2004VPNRecorder
 * Copyright (C) 2012 Onur Sarıkaya 
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
    public class Isa2004VPNRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public Isa2004VPNRecorder()
            : base()
        {
            LogName = "Isa2004VPNRecorder";
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

        public Isa2004VPNRecorder(String fileName)
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

            try
            {
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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "key : " + kvp.Key);
                        }
                        SetLastKeywords(add);
                        keywordsFound = true;
                    }
                    return true;
                }

                else if (!line.StartsWith("#"))
                {
                    String[] arr = line.Split('\t');
                    r.Datetime = arr[dictHash["date"]] + " " + arr[dictHash["time"]];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                    r.LogName = LogName;

                    try
                    {
                        r.EventCategory = arr[dictHash["IP protocol"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "EventCategory : " + ex.Message);
                    }

                    try
                    {
                        r.EventType = arr[dictHash["action"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "EventType : " + ex.Message);
                    }

                    try
                    {

                        r.UserName = arr[dictHash["session ID"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName : " + r.UserName);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "UserName : " + ex.Message);
                    }

                    try
                    {
                        r.LogName = LogName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "LogName : " + ex.Message);
                    }

                    try
                    {
                        r.ComputerName = arr[dictHash["computer"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                    }

                    try
                    {
                        r.CustomStr1 = arr[dictHash["username"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                    }

                    try
                    {

                        r.CustomStr3 = arr[dictHash["source"]].Split(':')[0];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 : " + ex.Message);
                    }

                    try
                    {

                        r.CustomStr4 = arr[dictHash["destination"]].Split(':')[0];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 : " + ex.Message);
                    }

                    try
                    {

                        r.CustomStr5 = arr[dictHash["original client IP"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomStr6 = arr[dictHash["status"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 : " + ex.Message);

                    }

                    try
                    {
                        r.CustomStr7 = arr[dictHash["rule"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 : " + ex.Message);

                    }

                    try
                    {
                        r.CustomStr8 = arr[dictHash["application protocol"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8 : " + r.CustomStr8);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 : " + ex.Message);
                    }

                    try
                    {

                        r.CustomStr9 = arr[dictHash["agent"]];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomInt3 = Convert.ToInt32(arr[dictHash["source"]].Split(':')[1]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3 : " + r.CustomInt3);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[dictHash["destination"]].Split(':')[1]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4 : " + r.CustomInt4);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomInt6 = Convert.ToInt32(arr[dictHash["bytes sent intermediate"]]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 : " + r.CustomInt6);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomInt7 = Convert.ToInt32(arr[dictHash["bytes received"]]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7 : " + r.CustomInt7);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt7 : " + ex.Message);
                    }

                    try
                    {
                        r.CustomInt8 = Convert.ToInt32(arr[dictHash["bytes sent"]]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8 : " + r.CustomInt8);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt8 : " + ex.Message);
                    }

                    try
                    {
                        r.Description = line;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + line);
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Description : " + ex.Message);
                    }

                    try
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Start sending data.");
                        SetRecordData(r);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Finished sending data.");
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "SetRecordData : " + ex.Message);
                    }


                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | " + e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | " + e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "StartsWith(#) | Line : " + line);
                return false;
            }

           
            return true;
        }

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
