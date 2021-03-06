

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using System.Collections;
using System.Globalization;

namespace Parser
{
    public class FtpRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public FtpRecorder()
            : base()
        {
            LogName = "FtpRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public FtpRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line.Trim()) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is null or Empty ");
                return true;
            }

            line = line.Trim();
            //#Fields: time c-ip cs-method cs-uri-stem sc-status sc-win32-status 
            //00:00:20 10.1.22.85 [10238]USER avonftp 331 0

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
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line starts with # char");
                return true;

            }
            if (!dontSend)
            {
                String[] arr = line.Split(' ');


                try
                {

                    Rec r = new Rec();

                    if (arr.Length == 6)
                    {

                        if (arr[dictHash["cs-method"]].Contains("[") && arr[dictHash["cs-method"]].Contains("["))
                        {
                            r.CustomStr9 = arr[dictHash["cs-method"]].Substring(arr[dictHash["cs-method"]].IndexOf('[')
                                + 1, arr[dictHash["cs-method"]].IndexOf(']') - arr[dictHash["cs-method"]].IndexOf('[') - 1);

                            r.CustomStr1 = arr[dictHash["cs-method"]].Substring(arr[dictHash["cs-method"]].IndexOf(']')
                                + 1, arr[dictHash["cs-method"]].Length - arr[dictHash["cs-method"]].IndexOf(']') - 1);


                        }
                        else
                            r.CustomStr1 = arr[dictHash["cs-method"]];



                        r.CustomStr3 = arr[dictHash["c-ip"]];

                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[dictHash["sc-status"]]);
                        }
                        catch
                        {
                            r.CustomInt1 = 0;
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["sc-win32-status"]]);
                        }
                        catch
                        {
                            r.CustomInt2 = 0;
                        }


                        if (r.CustomInt1 == 331)
                            r.UserName = arr[dictHash["cs-uri-stem"]];
                        else
                            r.CustomStr7 = arr[dictHash["cs-uri-stem"]];

                        string sFileDate = Path.GetFileName(lastFile).Replace("ex", "").Trim().Split('.')[0];

                        string sda = sFileDate.Substring(4, 2)
                                        + "/" + sFileDate.Substring(2, 2)
                                        + "/" + sFileDate.Substring(0, 2) + " " + arr[dictHash["time"]];

                        string dFileDate = sFileDate.Substring(2, 2)
                                                                + "/" + sFileDate.Substring(4, 2)
                                                                + "/" + sFileDate.Substring(0, 2) + " " + arr[dictHash["time"]];

                        r.Datetime = dFileDate;

                    }
                    else
                    {

                        //9922]DELE 10967.xml 250 0
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Log Olmas� Gereken Formatta De�il");

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[arr.Length - 1]);
                        }
                        catch
                        {
                            r.CustomInt2 = 0;
                        }
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(arr[arr.Length - 2]);
                        }
                        catch
                        {
                            r.CustomInt1 = 0;
                        }

                        if (r.CustomInt1 == 331)
                            r.UserName = arr[arr.Length - 3];
                        else
                            r.CustomStr7 = arr[arr.Length - 3];


                    }




                    r.LogName = LogName;
                    r.Description = line;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Finish Record Data");

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                    return false;
                }

            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParsingEnds");

            return true;
        }

        protected override void ParseFileNameLocal()
        {

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();
                foreach (String file in Directory.GetFiles(Dir))
                {
                    string sFile = Path.GetFileName(file).ToString();
                    if (sFile.StartsWith("ex") == true)
                        arrFileNames.Add(sFile);

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

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal | Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {

                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
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

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal | Dosya Sonu Okunmad� Okuma Devam Ediyor");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameLocal() | FileName = LastFile " + FileName);

                        }


                        br.Close();
                        fs.Close();


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
                               "ParseFileNameLocal() | LastFile �s Null �lk FileName Set  : " + FileName);
                    }

                }



            }
            else
            {
                FileName = Dir;
                lastFile = FileName;
            }


        }

        private string[] SortFileNameByFileNumber(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Replace("ex", "").Trim().Split('.')[0]);
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        }

        /// <summary>
        /// LastFile'�n Numaras�na G�re Bir Sonraki Dosyay� Set Eder
        /// </summary>
        /// <param name="dFileNameList"></param>
        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {

            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Replace("ex", "").Trim().Split('.')[0]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Replace("ex", "").Trim().Split('.')[0]);

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

    }
}
