/*
 * Exchange 2007 Recorder
 * Copyright (C) 2008 Erdo�an Kalemci <olligan@gmail.com>
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
using System.Globalization;
using System.Collections;

namespace Parser
{
    public class Exchange2007Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public Exchange2007Recorder()
            : base()
        {
            LogName = "Exchange2007Recorder";
        }
        public override void Init()
        {
            GetFiles();
            //Because of remote usage im increasing daychangetimers period
            dayChangeTimer.Interval = 5000;
            //enc = Encoding.GetEncoding("iso-8859-1");
            enc = Encoding.UTF8;
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
                else
                {
                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length - 1 > Position)
                    {
                        Stop();
                        Start();
                    }                
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Day Change Timer File is: " + FileName);
            }
            dayChangeTimer.Enabled = true;
        }        
        
        public override bool ParseSpecific(string line, bool dontSend)
        {            
            if (line == "")
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is empty");
                return true;
            }

            if (line.StartsWith("#"))
            {
                if (line.StartsWith("#Fields:"))
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    String[] fields = line.Split(',');
                    Int32 count = 0;
                    foreach (String field in fields)
                    {
                        if (field.StartsWith("#Fields:"))
                        {
                            String[] arr = field.Split(' ');
                            if (arr.Length == 2)
                            {
                                dictHash.Add(arr[1], count);
                                count++;
                            }
                            continue;
                        }
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
                String[] arr = SpaceSplit(line, false, '"', true, ',');

                if (arr.Length < 2)
                    return true;
                else if (arr.Length > 19)
                {
                    try
                    {
                        Rec r = new Rec();
                        Int32 dateIndex = dictHash["date-time"];
                        arr[dateIndex] = arr[dateIndex].TrimEnd('Z');
                        String[] arrDate = arr[dateIndex].Split('T');

                        arrDate[0] = arrDate[0].Replace('-', '/');

                        r.Datetime = arrDate[0] + " " + arrDate[1];

                        r.EventCategory = arr[dictHash["event-id"]];
                        try
                        {
                            r.CustomInt6 = Convert.ToInt64(arr[dictHash["total-bytes"]]);
                        }
                        catch
                        {
                            r.CustomInt6 = -1;
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["recipient-count"]]);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }

                        try
                        {
                            r.EventId = Convert.ToInt64(arr[dictHash["internal-message-id"]]);
                        }
                        catch
                        {
                            r.EventId = -1;
                        }
                        //Recipient Status
                        String[] arrRecpStatus = arr[dictHash["recipient-status"]].Split(' ');
                        if (arrRecpStatus.Length > 2)
                        {
                            try
                            {
                                r.CustomInt1 = Convert.ToInt32(arrRecpStatus[0]);
                            }
                            catch
                            {
                                r.CustomInt1 = -1;
                            }
                        }
                        try//if log format is journal.report than sender-address may not be exist..
                        {
                            r.CustomStr3 = arr[dictHash["sender-address"]];
                            r.CustomStr10 = arr[dictHash["return-path"]];
                        }
                        catch { }
                        r.CustomStr2 = (arr[dictHash["message-subject"]]).ToString(CultureInfo.InvariantCulture);
                        try
                        {
                            if (arr[dictHash["recipient-address"]].Length > 890)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "recipient-address is too big for 1 column, CustomStr8 and CustomStr5 will be used for this recipient-address");
                                StringBuilder sb = new StringBuilder();
                                String temp = arr[dictHash["recipient-address"]];
                                Char[] tempArr = temp.ToCharArray();
                                Int32 i = 0;
                                Int32 val = 0;
                                for (i = 0; i < tempArr.Length; i++)
                                {
                                    sb.Append(tempArr[i]);
                                    if (i > 890 && val == 0)
                                    {
                                        r.CustomStr1 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                    else if ((i > 8880 && val == 1) || (val == 1 && i == tempArr.Length - 1))
                                    {
                                        r.CustomStr8 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                    else if ((i > 16870 && val == 2) || (val == 2 && i == tempArr.Length - 1))
                                    {
                                        r.CustomStr5 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                }
                            }
                            else
                                r.CustomStr1 = arr[dictHash["recipient-address"]];

                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Error at parsing recipient-address");
                        }
                        r.CustomStr4 = arr[dictHash["client-ip"]];
                        //r.CustomStr5 = arr[dictHash["client-hostname"]];
                        r.CustomStr6 = arr[dictHash["message-id"]];
                        r.CustomStr7 = arr[dictHash["related-recipient-address"]];
                        r.CustomStr9 = arr[dictHash["server-ip"]];
                        r.SourceName = arr[dictHash["source"]];
                        r.EventType = arr[dictHash["connector-id"]];
                        r.Description = arr[dictHash["source-context"]] + " - " + arr[dictHash["reference"]];
                        r.LogName = "MsExchange2007Recorder";
                        r.ComputerName = arr[dictHash["server-hostname"]];
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
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Log is in wrong format");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Line : " + line);
                }
            }
            return true;
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
        
        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Start");

            try
            {
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | File names to be read : " + tempCustomVar1);

                    ArrayList arrFileNames = new ArrayList();
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | File : " + file);

                        string sFile = Path.GetFileName(file).ToString();
                        
                        if (string.IsNullOrEmpty(tempCustomVar1) || tempCustomVar1 == "MSGTRK")
                        {
                            if (sFile.StartsWith("MSGTRK") && !sFile.StartsWith("MSGTRKM"))
                                arrFileNames.Add(sFile);
                        }
                        else if (tempCustomVar1 == "MSGTRKM")
                        {
                            if (sFile.StartsWith("MSGTRKM"))
                                arrFileNames.Add(sFile);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + Dir);

                    String[] dFileNameList = SortFileNameByFileNumber(arrFileNames);

                    if (string.IsNullOrEmpty(lastFile) == false)
                    {
                        if (File.Exists(lastFile) == true)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile is not null  : " + lastFile);

                            FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            BinaryReader br = new BinaryReader(fs, enc);

                            br.BaseStream.Seek(Position, SeekOrigin.Begin);

                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());
                            #region yeni eklendi
                            FileInfo fi = new FileInfo(lastFile);
                            Int64 fileLength = fi.Length;
                            Char c = ' ';
                            while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                                c = br.ReadChar();
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Char Is" + c.ToString());

                                if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                                }
                            }
                            #endregion
                            if (br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Position is at the end of the file so changing the File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                "ParseFileNameLocal() | Yeni Dosya Yok Ayn� Dosyaya Devam : " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
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
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal | Dosya Sonu Okunmad� Okuma Devam Ediyor");
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | FileName = LastFile " + FileName);
                            }
                        }
                        else
                        {
                            SetBirSonrakiFile(dFileNameList, "ParseFileNameLocal()");
                        }
                    }
                    else
                    {
                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | LastFile �s Null �lk FileName Set  : " + FileName);
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                }

                #region OldCode
                //String oldFileName = lastFile;
                //if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                //{
                //    // FileName Format : MSGTRK20090728-1.LOG
                //    DateTime dt = DateTime.Now;
                //    String fileStart = "MSGTRK";
                //    Int32 lastCount = 0;
                //    UInt64 maxDateTime = 0;
                //    FileName = lastFile;                
                //    if (FileName.Length < 1)
                //    {
                //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is null, so searching for last file.." + Dir);
                //        UInt64 fileNameDate = 0;
                //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is null, so searching for last file..");                    
                //        foreach (String file in Directory.GetFiles(Dir))
                //        {
                //            String fileShort = Path.GetFileName(file);
                //            if (!(fileShort.StartsWith("MSGTRKM")))
                //            {                            
                //                String[] arr = fileShort.Split('-');
                //                if (arr.Length == 1)
                //                    arr = fileShort.Split('.');
                //                if (arr.Length == 2)
                //                {
                //                    try
                //                    {
                //                        fileNameDate = Convert.ToUInt64(arr[0].Remove(0, 6));
                //                    }
                //                    catch
                //                    {
                //                        Log.Log(LogType.FILE, LogLevel.WARN, "Different files found in folder, filename : "+ file);                    
                //                    }
                //                }
                //                if (fileNameDate >= maxDateTime)
                //                {
                //                    if (arr.Length == 2)
                //                    {
                //                        if (fileNameDate == maxDateTime)
                //                        {
                //                            String[] arrIn = arr[1].Split('.');
                //                            if (arrIn.Length == 2)
                //                            {
                //                                Int32 count = Convert.ToInt32(arrIn[0]);
                //                                if (count > lastCount)
                //                                {
                //                                    lastCount = count;
                //                                    FileName = file;
                //                                }
                //                            }
                //                            else
                //                                Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //                        }
                //                        else
                //                        {
                //                            FileName = file;
                //                            lastCount = 0;
                //                        }
                //                    }
                //                    else
                //                        Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //                    maxDateTime = fileNameDate;
                //                } 
                //            }
                //        }
                //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename is null so getting last File: " + FileName);
                //        lastFile = FileName;
                //        Position = 0;
                //        return;
                //    }
                //    //End �f the file?-----------------------------------------------------------------------------
                //    FileInfo fi = new FileInfo(FileName);
                //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is: " + Position.ToString());
                //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Length is: " + fi.Length.ToString());
                //    if (Position == fi.Length - 1 || Position >= fi.Length)
                //    {
                //        Log.Log(LogType.FILE, LogLevel.INFORM, "Position is at the end of the file so changing the File");

                //        String[] arrold = FileName.Split('-');                    
                //        String[] arrInold = arrold[1].Split('.');
                //        Int32 oldCount = Convert.ToInt32(arrInold[0]);
                //        String[] arrMsgtrk = arrold[0].Split('\\');
                //        UInt64 oldDate = Convert.ToUInt64(arrMsgtrk[arrMsgtrk.Length - 1].Remove(0, 6));                    
                //        lastCount = 1000;
                //        foreach (String file in Directory.GetFiles(Dir))
                //        {
                //            String fileShort = Path.GetFileName(file);
                //            if (!(fileShort.StartsWith("MSGTRKM")) && fileShort.StartsWith(fileStart) && fileShort.Contains(oldDate.ToString()))
                //            {

                //                String[] arr = fileShort.Split('-');
                //                            if (arr.Length == 2)
                //                            {
                //                                String[] arrIn = arr[1].Split('.');
                //                                if (arrIn.Length == 2)
                //                                {
                //                                    Int32 count = Convert.ToInt32(arrIn[0]);
                //                                    if (count > oldCount)
                //                                    {
                //                                        if(count < lastCount)
                //                                        {
                //                                            lastCount = count;
                //                                            FileName = file;
                //                                            Position = 0;
                //                                            Log.Log(LogType.FILE, LogLevel.INFORM, "There is a new file in same date, parsing this file: " + file);
                //                                        }
                //                                    }
                //                                }
                //                                else
                //                                    Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //                            }
                //                            else
                //                                Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //            }
                //        }
                //        if (FileName == lastFile)
                //        {
                //            String[] arrNew = null;                                     
                //            String[] arrNew2 = null;
                //            UInt64 oldDate2 = 0;
                //            UInt64 minDate = 99999999;
                //            try
                //            {
                //                foreach (String file in Directory.GetFiles(Dir))
                //                {
                //                    String fileShort = Path.GetFileName(file);
                //                    if (!fileShort.StartsWith("MSGTRKM") && fileShort.StartsWith("MSGTRK"))
                //                    {                                    
                //                        arrNew = fileShort.Split('-');
                //                        arrNew2 = arrNew[0].Split('\\');
                //                        oldDate2 = Convert.ToUInt64(arrNew2[arrNew2.Length - 1].Remove(0, 6));
                //                        if (oldDate2 > oldDate)
                //                            if (oldDate2 < minDate)
                //                                minDate = oldDate2;
                //                    }
                //                }
                //            }
                //            catch (Exception e)
                //            {
                //                Log.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                //            }                                                
                //            lastCount = 1000;
                //            foreach (String file in Directory.GetFiles(Dir))
                //            {
                //                String fileShort = Path.GetFileName(file);
                //                if (fileShort.StartsWith(fileStart) && fileShort.Contains(minDate.ToString()) && !fileShort.StartsWith("MSGTRKM"))
                //                {
                //                    String[] arr = fileShort.Split('-');
                //                    if (arr.Length == 2)
                //                    {
                //                        String[] arrIn = arr[1].Split('.');
                //                        if (arrIn.Length == 2)
                //                        {
                //                            Int32 count = Convert.ToInt32(arrIn[0]);
                //                            if (count < lastCount)
                //                            {
                //                                lastCount = count;
                //                                FileName = file;
                //                                Position = 0;
                //                                Log.Log(LogType.FILE, LogLevel.INFORM, "No new file in same date, changing date,parsing this file: " + file);
                //                            }
                //                        }
                //                        else
                //                            Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //                    }
                //                    else
                //                        Log.Log(LogType.FILE, LogLevel.WARN, "File found but not with correct format \"MSGTRKYYYYMMDD-X.log\", this file (" + Path.GetFileName(file) + ") will not be parsed.");
                //                }
                //            }
                //        }
                //    }                
                //    else
                //    {
                //        Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is not at the end of the file so no changing the File");
                //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal Filename is: " + FileName);
                //        return;
                //    }                  
                //}
                //else
                //    FileName = Dir;
                //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal Filename is: " + FileName);
                //lastFile = FileName;
                //if (FileName != oldFileName)
                //    Position = 0;
                //return; 
                #endregion

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | " + ex.StackTrace);
            }
        }
        
        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() | Started ");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep ^-";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() | SSH command -1 : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    StringReader sr = new StringReader(stdOut);

                    ArrayList arrFileNameList = new ArrayList();
                    while ((line = sr.ReadLine()) != null)
                    {

                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].StartsWith("MSGTRKM") == true)
                            arrFileNameList.Add(arr[arr.Length - 1]);

                    }

                    String[] dFileNameList = SortFileNameByFileNumber(arrFileNameList);

                    //lastFile'a deger atanm�ssa 
                    if (string.IsNullOrEmpty(lastFile) == false)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() | LastFile  = " + lastFile);

                        bool bLastFileExist = false;
                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {

                            String wcArg = "";
                            String wcCmd = "";
                            if (readMethod == "sed")
                            {
                                wcCmd = "wc";
                                wcArg = "-l";
                            }
                            else if (readMethod == "nread")
                            {
                                wcCmd = "wc";
                                wcArg = "-c";
                            }

                            command = wcCmd + " " + wcArg + " " + lastFile;

                            stdOut = "";
                            stdErr = "";

                            se.RunCommand(command, ref stdOut, ref stdErr);

                            String[] arr = SpaceSplit(stdOut, false);
                            Int64 lFileIndex = Convert.ToInt64(arr[0]);//Dosyadaki sat�r say�s�
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() | FileIndex  = " + lFileIndex.ToString());

                            if (lFileIndex > Position)
                            {

                                //Dosya Sonuna Kadar okunmam�s Ayn� Dosyaya Devam Edecek;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "ParseFileNameRemote() | FileIndex ( " + lFileIndex.ToString() + " ) > Position (" + Position.ToString() + " )");

                                FileName = lastFile;

                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "ParseFileNameRemote() | FileName = LastFile " + lastFile);


                            }
                            else
                            {

                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                   "ParseFileNameRemote() | ParseFileNameRemote() | Dosya Sonuna Kadar Okundu Position (" + this.Position.ToString() + " )  > FileIndex ( " + lFileIndex.ToString() + " )");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                "ParseFileNameRemote() | Yeni Dosya Yok Ayn� Dosyaya Devam : " + FileName);
                                            break;

                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                "ParseFileNameRemote() | Yeni Dosya  : " + FileName);
                                            break;

                                        }
                                    }
                                }
                            }
                        }
                        else
                            SetBirSonrakiFile(dFileNameList, "ParseFileNameRemote()");

                    }
                    else
                    {

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameRemote() |LastName is null �lk FileName Set  : " + FileName);

                        }

                    }
                    stdOut = "";
                    stdErr = "";
                    se.Close();

                }
                else
                    FileName = Dir;

            }
            catch (Exception exp)
            {

                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() |" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() |" + exp.StackTrace);
                return;
            }


            #region OldCode
            //String stdOut = "";
            //String stdErr = "";
            //se = new SshExec(remoteHost, user);
            //se.Password = password;
            //if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            //{
            //    se.Connect();
            //    se.RunCommand("ls -lt " + Dir + " | grep ^-", ref stdOut, ref stdErr);
            //    StringReader sr = new StringReader(stdOut);
            //    String line = sr.ReadLine();
            //    String[] arr = line.Split(' ');

            //    FileName = Dir + arr[12];
            //    stdOut = "";
            //    stdErr = "";
            //    se.Close();
            //}
            //else
            //    FileName = Dir; 
            #endregion
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

        private string[] SortFileNameByFileNumber_old(ArrayList arrFileNames)
        {
            try
            {
                UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
                String[] dFileNameList = new String[arrFileNames.Count];

                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    if (arrFileNames[i].ToString().Contains("MSGTRK") && arrFileNames[i].ToString().Contains("-"))
                    {
                        string nameDate = arrFileNames[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim();
                        string tempekNum = arrFileNames[i].ToString().Split('.')[0].Split('-')[1].Trim();
                        string ekNum = "";

                        if (tempekNum.Length == 3)
                        {
                            ekNum = tempekNum;
                        }
                        else if (tempekNum.Length == 2)
                        {
                            ekNum = "0" + tempekNum;
                        }
                        else if (tempekNum.Length == 1)
                        {
                            ekNum = "00" + tempekNum;
                        }

                        dFileNumberList[i] = Convert.ToUInt64(nameDate + ekNum);
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                    else
                    {
                        dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Trim());
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                }

                Array.Sort(dFileNumberList, dFileNameList);
                return dFileNameList;
            }
            catch (Exception exp)
            {

                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFileNameByFileNumber() |" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFileNameByFileNumber() |" + exp.StackTrace);
                throw exp;
            }
        }

        private string[] SortFileNameByFileNumber(ArrayList arrFileNames)
        {            
            try
            {
                UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
                String[] dFileNameList = new String[arrFileNames.Count];
                int lenghtOfFileName = 0;

                string[] numberPart = null;

                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    if(string.IsNullOrEmpty(tempCustomVar1))
                        lenghtOfFileName = "MSGTRK".Length;
                    else
                        lenghtOfFileName = tempCustomVar1.Length;

                     numberPart = arrFileNames[i].ToString().Remove(0, lenghtOfFileName).Split(new char[]{'-','.'},StringSplitOptions.RemoveEmptyEntries);

                     dFileNumberList[i] = Convert.ToUInt64(numberPart[0] + numberPart[1]);
                     dFileNameList[i] = arrFileNames[i].ToString();
           

                    //if (arrFileNames[i].ToString().Contains("MSGTRK") && arrFileNames[i].ToString().Contains("-") && !arrFileNames[i].ToString().Contains("A"))
                    //{
                    //    dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
                    //    dFileNameList[i] = arrFileNames[i].ToString();
                    //}
                    //else 
                    //{
                    //    if (!arrFileNames[i].ToString().Contains("A"))
                    //    {
                    //        dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Trim());
                    //        dFileNameList[i] = arrFileNames[i].ToString(); 
                    //    }
                    //}
                }

                Array.Sort(dFileNumberList, dFileNameList);

                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFileNameByFileNumber() | File : " + dFileNameList[i]);
                }

                    return dFileNameList;
            }
            catch (Exception exp)
            {

                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFileNameByFileNumber() |" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFileNameByFileNumber() |" + exp.StackTrace);
                throw exp;

            }
        }
        
        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                UInt64 lFileNumber = 0;

                UInt64 lLastFileNumber = 0;
                
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    if (dFileNameList[i].ToString().Contains("MSGTRK") && dFileNameList[i].ToString().Contains("-") && !dFileNameList[i].ToString().Contains("A"))
                    {
                        lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
                        lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
                    }
                    else 
                    {
                        if (!dFileNameList[i].Contains("A"))
                        {
                            lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[0].Trim());
                            lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[0].Trim()); 
                        }
                    }

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                            sFunction + " | LastFile Silinmis , Dosya Bulunamad�  Yeni File : " + FileName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                     "SetBirSonrakiFile() | " + ex.Message);

            }
        }
    }
}
