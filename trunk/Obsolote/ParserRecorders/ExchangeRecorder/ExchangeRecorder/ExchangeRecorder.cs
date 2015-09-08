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
    public class ExchangeRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public ExchangeRecorder()
            : base()
        {
            LogName = "ExchangeRecorder";
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

        public override void Init()
        {
            GetFiles();
            //Because of remote usage im increasing daychangetimers period
            dayChangeTimer.Interval = 5000;
            //enc = Encoding.GetEncoding("iso-8859-1");
            enc = Encoding.UTF8;
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
            if (line == "")
                return true;

            if (line.StartsWith("#"))
            {
                if (line.StartsWith("# Date"))
                {
                    try
                    {
                        if (dictHash != null)
                        {
                            string addtemp = "";
                            foreach (KeyValuePair<String, Int32> kvptemp in dictHash)
                            {
                                addtemp += kvptemp.Key + ",";
                            }
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "OLD VALUE IN DICTIONARY BEFORE CLEAR: " + addtemp);

                            dictHash.Clear();
                        }
                        dictHash = new Dictionary<String, Int32>();
                        String[] fields = line.Split('\t');
                        Int32 count = 0;
                        foreach (String field in fields)
                        {
                            String temp = field.TrimStart('#', ' ');
                            if (!dictHash.ContainsKey(temp))
                            {
                                dictHash.Add(temp, count);
                                count++;
                            }
                        }
                        String add = "";
                        foreach (KeyValuePair<String, Int32> kvp in dictHash)
                        {
                            add += kvp.Key + ",";
                        }
                        

                        SetLastKeywords(add);
                        keywordsFound = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Error in Parce Specific is " + ex);
                    }
                }
                return true;
            }

            if (!dontSend)
            {
                string add2 = "";
                foreach (KeyValuePair<String, Int32> kvpr in dictHash)
                {
                    add2 += kvpr.Key + ",";
                }
               

                String[] arr = line.Split('\t');

                if (arr.Length < 2)
                    return true;

                Rec r = new Rec();
                try
                {
                    
                    r.Description = line;
                    Int32 value;
                    bool control = true;

                    if (dictHash.TryGetValue("Date", out value))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Date");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the key Date");
                        control = false;
                    }

                    if (control)
                    {
                        Int32 dateIndex = dictHash["Date"];
                        arr[dateIndex] = arr[dateIndex].Replace('-', '/');

                        String timeTemp = "";
                        String[] arrTimeTemp = arr[dictHash["Time"]].Split(':');
                        Int32 count = 0;
                        foreach (String sTime in arrTimeTemp)
                        {
                            if (count == arrTimeTemp.Length - 1)
                            {
                                Char[] arrSTime = sTime.ToCharArray();
                                foreach (Char c in arrSTime)
                                {
                                    if (Char.IsNumber(c))
                                        timeTemp += c;
                                    else
                                        break;
                                }
                            }
                            else
                            {
                                timeTemp += sTime + ":";
                            }
                            count++;
                        }
                        r.Datetime = arr[dateIndex] + " " + timeTemp;
                    }
                    else
                    {
                        r.Datetime = "2010.02.01 12:21:32";
                    }

                    try
                    {
                        r.EventId = Convert.ToInt64(arr[dictHash["Event-ID"]]);
                    }
                    catch
                    {
                        r.EventId = -1;
                    }

                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[dictHash["total-bytes"]]);
                    }
                    catch
                    {
                        r.CustomInt1 = -1;
                    }

                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[dictHash["Number-Recipients"]]);
                    }
                    catch
                    {
                        r.CustomInt2 = -1;
                    }

                    try
                    {
                        r.CustomInt8 = Convert.ToInt64(arr[dictHash["Linked-MSGID"]]);
                    }
                    catch
                    {
                        r.CustomInt8 = -1;
                    }

                    try
                    {
                        r.CustomInt9 = Convert.ToInt64(arr[dictHash["Recipient-Report-Status"]]);
                    }
                    catch
                    {
                        r.CustomInt9 = -1;
                    }

                    try
                    {
                        r.CustomInt10 = Convert.ToInt64(arr[dictHash["Priority"]]);
                    }
                    catch
                    {
                        r.CustomInt10 = -1;
                    }

                    Int32 value1;
                    bool control1 = true;

                    if (dictHash.TryGetValue("Recipient-Address", out value1))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Recipient-Address");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value1);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Recipient-Address");
                        control1 = false;
                    }

                    if (control1)
                    {
                        r.CustomStr1 = arr[dictHash["Recipient-Address"]];
                    }
                    else
                    {
                        r.CustomStr1 = "";
                    }

                    Int32 value2;
                    bool control2 = true;

                    if (dictHash.TryGetValue("Message-Subject", out value2))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Message-Subject");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value2);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Message-Subject");
                        control2 = false;
                    }


                    if (control2)
                    {
                        r.CustomStr2 = (arr[dictHash["Message-Subject"]]).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        r.CustomStr2 = "";
                    }

                    Int32 value3;
                    bool control3 = true;

                    if (dictHash.TryGetValue("Sender-Address", out value3))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Sender-Address");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value3);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Sender-Address");
                        control3 = false;
                    }

                    if (control3)
                    {
                        r.CustomStr3 = arr[dictHash["Sender-Address"]];
                    }
                    else
                    {
                        r.CustomStr3 = "";
                    }


                    Int32 value4;
                    bool control4 = true;

                    if (dictHash.TryGetValue("client-ip", out value4))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key client-ip");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value4);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the client-ip");
                        control4 = false;
                    }

                    if (control4)
                    {
                        r.CustomStr4 = arr[dictHash["client-ip"]];
                    }
                    else
                    {
                        r.CustomStr4 = "";
                    }


                    Int32 value5;
                    bool control5 = true;

                    if (dictHash.TryGetValue("Client-hostname", out value5))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Client-hostname");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value5);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Client-hostname");
                        control5 = false;
                    }

                    if (control5)
                    {
                        r.CustomStr5 = arr[dictHash["Client-hostname"]];
                    }
                    else
                    {
                        r.CustomStr5 = "";
                    }

                    Int32 value6;
                    bool control6 = true;

                    if (dictHash.TryGetValue("service-Version", out value6))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key service-Version");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value6);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the service-Version");
                        control6 = false;
                    }

                    if (control6)
                    {
                        r.CustomStr6 = arr[dictHash["service-Version"]];
                    }
                    else
                    {
                        r.CustomStr6 = "";
                    }

                    Int32 value7;
                    bool control7 = true;

                    if (dictHash.TryGetValue("Encryption", out value7))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Encryption");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value7);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Encryption");
                        control7 = false;
                    }

                    if (control7)
                    {
                        r.CustomStr7 = arr[dictHash["Encryption"]];
                    }
                    else
                    {
                        r.CustomStr7 = "";
                    }

                    Int32 value8;
                    bool control8 = true;

                    if (dictHash.TryGetValue("Origination-Time", out value8))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Origination-Time");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value8);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Origination-Time");
                        control8 = false;
                    }

                    if (control8)
                    {
                        r.CustomStr8 = arr[dictHash["Origination-Time"]];
                    }
                    else
                    {
                        r.CustomStr8 = "";
                    }


                    Int32 value9;
                    bool control9 = true;

                    if (dictHash.TryGetValue("server-IP", out value9))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key server-IP");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value9);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the server-IP");
                        control9 = false;
                    }

                    if (control9)
                    {
                        r.CustomStr9 = arr[dictHash["server-IP"]];
                    }
                    else
                    {
                        r.CustomStr9 = "";
                    }


                    Int32 value10;
                    bool control10 = true;

                    if (dictHash.TryGetValue("Partner-Name", out value10))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Partner-Name");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value10);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Partner-Name");
                        control10 = false;
                    }

                    if (control10)
                    {
                        r.CustomStr10 = arr[dictHash["Partner-Name"]];
                    }
                    else
                    {
                        r.CustomStr10 = "";
                    }

                    r.LogName = "MsExchangeRecorder";



                    Int32 value11;
                    bool control11 = true;

                    if (dictHash.TryGetValue("Server-hostname", out value11))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary has the key Server-hostname");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "The value is " + value11);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Dictionary does not has the Server-hostname");
                        control11 = false;
                    }


                    if (control11)
                    {
                        r.ComputerName = arr[dictHash["Server-hostname"]];
                    }
                    else
                    {
                        r.ComputerName = "";
                    }

                    
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return false;
                }
                
                SetRecordData(r);
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

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();
                try
                {
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        string sFile = Path.GetFileName(file).ToString();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "sfile is : " + sFile);
                        //if (sFile.StartsWith("MSGTRK") == true)
                        arrFileNames.Add(sFile);
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR,ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR,ex.StackTrace);
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

                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;
                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ExchangeRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                            c = br.ReadChar();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ExchangeRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  ExchangeRecorder In ParseFileNameLocal() -->> Char Is" + c.ToString());

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ExchangeRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ExchangeRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }

                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length || br.BaseStream.Position == br.BaseStream.Length - 1)
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
                                            "ParseFileNameLocal() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Dosya Sonu Okunmadý Okuma Devam Ediyor");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameLocal() | FileName = LastFile " + FileName);
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

        private string[] SortFileNameByFileNumber(ArrayList arrFileNames)
        {
            try
            {
                UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
                String[] dFileNameList = new String[arrFileNames.Count];

                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    if (arrFileNames[i].ToString().Contains("MSGTRK") && arrFileNames[i].ToString().Contains("-"))
                    {
                        dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                    else if (arrFileNames[i].ToString().Contains("ex"))
                    {
                        dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Replace("ex", "20").Trim());
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                    else
                    {
                        dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[0].Trim());
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                }

                Log.Log(LogType.FILE, LogLevel.ERROR, "FileNameList " + dFileNameList);

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

        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                UInt64 lFileNumber;
                UInt64 lLastFileNumber;

                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    if (dFileNameList[i].ToString().Contains("MSGTRK") && dFileNameList[i].ToString().Contains("-"))
                    {
                        lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
                        lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[0].Split('-')[0].Replace("MSGTRK", " ").Trim().Trim('M').Trim());
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
                     "SetBirSonrakiFile() | " + ex.Message);
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
    }
}

