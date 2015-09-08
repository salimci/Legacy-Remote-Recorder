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
    public class Exchange2010Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;
     
        public Exchange2010Recorder()
            : base()
        {
            LogName = "Exchange2010Recorder";
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific -->> line is empty");
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
                else if (arr.Length >= dictHash.Count)
                {
                    try
                    {
                        Rec r = new Rec();
                        Int32 dateIndex = dictHash["date-time"];
                        arr[dateIndex] = arr[dateIndex].TrimEnd('Z');
                        String[] arrDate = arr[dateIndex].Split('T');

                        arrDate[0] = arrDate[0].Replace('-', '/');

                        r.Datetime = Convert.ToDateTime(arrDate[0] + " " + arrDate[1]).ToString("yyyy/MM/dd HH:mm:ss");

                        r.EventCategory = arr[dictHash["event-id"]];//////////
                        try
                        {
                            r.CustomInt6 = Convert.ToInt64(arr[dictHash["total-bytes"]]);/////
                        }
                        catch
                        {
                            r.CustomInt6 = -1;
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["recipient-count"]]);/////
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }

                        try
                        {
                            r.EventId = Convert.ToInt64(arr[dictHash["internal-message-id"]]);////////////
                        }
                        catch
                        {
                            r.EventId = -1;
                        }
                        //Recipient Status
                        String[] arrRecpStatus = arr[dictHash["recipient-status"]].Split(' ');//
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
                            r.CustomStr3 = arr[dictHash["sender-address"]];//
                            r.CustomStr10 = arr[dictHash["return-path"]];//
                        }
                        catch { }
                        r.CustomStr2 = (arr[dictHash["message-subject"]]).ToString(CultureInfo.InvariantCulture);//////////
                        try
                        {
                            if (arr[dictHash["recipient-address"]].Length > 890)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "recipient-address is too big for 1 column, CustomStr8 and CustomStr5 will be used for this recipient-address");
                                StringBuilder sb = new StringBuilder();
                                String temp = arr[dictHash["recipient-address"]];////////
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
                        r.CustomStr4 = arr[dictHash["client-ip"]];//////////////
                        r.CustomStr5 = arr[dictHash["client-hostname"]];//////////
                        r.CustomStr6 = arr[dictHash["message-id"]];////////
                        r.CustomStr7 = arr[dictHash["related-recipient-address"]];//////
                        r.CustomStr9 = arr[dictHash["server-ip"]];///////////////
                        r.SourceName = arr[dictHash["source"]];//////////////
                        r.EventType = arr[dictHash["connector-id"]];///////////////
                        r.Description = arr[dictHash["source-context"]] + " - " + arr[dictHash["reference"]] + " - " + arr[dictHash["message-info"]];/////////
                        r.LogName = "MsExchange2010Recorder";
                        r.ComputerName = arr[dictHash["server-hostname"]];//////////
                        
                        SetRecordData(r);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> is successfully Finished ");
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -- >> is STARTED ");

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -- >> Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();
                foreach (String file in Directory.GetFiles(Dir))
                {
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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -- >> file name is : " + sFile.ToString());
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -- >> Sorting file in directory: " + Dir);

                String[] dFileNameList = SortFileNameByFileNumber(arrFileNames);

                if (string.IsNullOrEmpty(lastFile) == false)
                {
                    if (File.Exists(lastFile) == true)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -- >> lastFile is not null  : " + lastFile);

                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, enc);

                        br.BaseStream.Seek(Position, SeekOrigin.Begin);

                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -- >> Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -- >> Length is: " + br.BaseStream.Length.ToString());
                        #region yeni eklendi
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;
                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                            c = br.ReadChar();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal() -->> Char Is" + c.ToString());

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }
                        #endregion
                        if (br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                        {   
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal -- >> Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {
                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseFileNameLocal() -- >> There is not any new file. Will continue with same file : " + FileName);
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + dFileNameList[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseFileNameLocal() -- >> New file name is : " + FileName);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal -- >> Dosya Sonu Okunmadý Okuma Devam Ediyor");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameLocal() -- >> FileName = LastFile " + FileName);
                        }
                    }
                    else
                    {
                        SetNextFile(dFileNameList, "ParseFileNameLocal()");
                    }
                }
                else
                {
                    if (dFileNameList.Length > 0)
                    {
                        FileName = Dir + dFileNameList[dFileNameList.Length - 1];
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
                            Int64 lFileIndex = Convert.ToInt64(arr[0]);//Dosyadaki satýr sayýsý
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() | FileIndex  = " + lFileIndex.ToString());

                            if (lFileIndex > Position)
                            {

                                //Dosya Sonuna Kadar okunmamýs Ayný Dosyaya Devam Edecek;
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
                                                "ParseFileNameRemote() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);
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
                            SetNextFile(dFileNameList, "ParseFileNameRemote()");

                    }
                    else
                    {

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameRemote() |LastName is null Ýlk FileName Set  : " + FileName);

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
                    if (string.IsNullOrEmpty(tempCustomVar1))
                        lenghtOfFileName = "MSGTRK".Length;
                    else
                        lenghtOfFileName = tempCustomVar1.Length;

                    numberPart = arrFileNames[i].ToString().Remove(0, lenghtOfFileName).Split(new char[] { '-', '.' }, StringSplitOptions.RemoveEmptyEntries);

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

        private void SetNextFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                UInt64 lFileNumber;
                UInt64 lLastFileNumber;

                for (int i = 0; i < dFileNameList.Length; i++)
                {       
                        
                    if (dFileNameList[i].ToString().Contains("MSGTRK") && dFileNameList[i].ToString().Contains("-"))
                    {   
                        string nameDate = dFileNameList[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim();
                        string tempekNum = dFileNameList[i].ToString().Split('.')[0].Split('-')[1].Trim();
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

                        lFileNumber = Convert.ToUInt64(nameDate + ekNum);


                        string _lastfilename = "";
                        _lastfilename = Path.GetFileName(lastFile);

                        string lLastFilenameDate = _lastfilename.ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim();
                        string lLastFiletempekNum = _lastfilename.ToString().Split('.')[0].Split('-')[1].Trim();
                        string lLastFileekNum = "";

                        if (lLastFiletempekNum.Length == 3)
                        {
                            lLastFileekNum = lLastFiletempekNum;
                        }
                        else if (lLastFiletempekNum.Length == 2)
                        {
                            lLastFileekNum = "0" + lLastFiletempekNum;
                        }
                        else if (lLastFiletempekNum.Length == 1)
                        {
                            lLastFileekNum = "00" + lLastFiletempekNum;
                        }
                        
                        lLastFileNumber = Convert.ToUInt64(lLastFilenameDate + lLastFileekNum);
                    
                    }
                    else
                    {
                        lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[0].Trim());
                        lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[0].Trim());
                    }

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                            sFunction + " | LastFile Silinmis , Dosya Bulunamadý  Yeni File : " + FileName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                     "SetNextFile() | " + ex.Message);

            }
        }

    }
}
