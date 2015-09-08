/*
 * Apache Access Recorder
 * Copyright (C) 2008-2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

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
    public class IBekciRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public IBekciRecorder()
            : base()
        {
            LogName = "IBekciRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public IBekciRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Ýs Null or Empty");
                return true;
            }

            if (line.Contains("zaman") || dictHash == null)
            {

                string sKeyWord;
                if (line.Contains("zaman"))
                    sKeyWord = line;
                else
                    sKeyWord = "zaman:protokol:kaynak ip:kaynak kapý:adres dönüþümü ip:adres dönüþümü kapý:hedef ip:hedef kapý:tcp durumlarý:paket yönü:giden bayt:gelen bayt:giden paket:gelen paket:süre";

                if (dictHash != null)
                    dictHash.Clear();
                dictHash = new Dictionary<String, Int32>();

                String[] fields = sKeyWord.Split(':');
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

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | SetLastKeywords");
                return true;
            }



            if (!dontSend)
            {

                String[] arr = line.Split(':');

                try
                {

                    Rec r = new Rec();

                    r.Datetime = arr[dictHash["zaman"]];

                    r.CustomStr1 = getProtokolName(ObjectToInt32(arr[dictHash["protokol"]], 0));
                    r.CustomInt1 = ObjectToInt32(arr[dictHash["protokol"]], 0);
                    r.CustomStr2 = arr[dictHash["kaynak ip"]];
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["kaynak kapý"]], 0);
                    r.CustomStr3 = arr[dictHash["adres dönüþümü ip"]];
                    r.CustomInt3 = ObjectToInt32(arr[dictHash["adres dönüþümü kapý"]], 0);
                    r.CustomStr4 = arr[dictHash["hedef ip"]];
                    r.CustomInt4 = ObjectToInt32(arr[dictHash["hedef kapý"]], 0);

                    r.CustomStr5 = getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlarý"]].Split('/')[0], 0)) + " / " +
                                   getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlarý"]].Split('/')[1], 0));

                    r.CustomStr7 = arr[dictHash["tcp durumlarý"]];

                    r.CustomStr6 = getPaketYonu(arr[dictHash["paket yönü"]]);
                    r.CustomInt6 = ObjectToInt64(arr[dictHash["giden bayt"]], 0);
                    r.CustomInt7 = ObjectToInt64(arr[dictHash["gelen bayt"]], 0);
                    r.CustomInt8 = ObjectToInt64(arr[dictHash["giden paket"]], 0);
                    r.CustomInt9 = ObjectToInt64(arr[dictHash["gelen paket"]], 0);
                    r.CustomInt10 = ObjectToInt64(arr[dictHash["süre"]], 0);


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

                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {

                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);

                                    }
                                    else
                                    {
                                        FileName = Dir + dFileNameList[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            "ParseFileNameLocal() | Yeni Dosya  : " + FileName);

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
                FileName = Dir;


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
        /// LastFile'ýn Numarasýna Göre Bir Sonraki Dosyayý Set Eder
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
                            sFunction + " | LastFile Silinmis , Dosya Bulunamadý  Yeni File : " + FileName);

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

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);

            }
            catch
            {
                return iReturn;
            }

        }
        private long ObjectToInt64(string sObject, long iReturn)
        {
            try
            {
                return Convert.ToInt64(sObject);
            }
            catch
            {
                return iReturn;
            }

        }
        private string getProtokolName(int iProtokolNo)
        {
            switch (iProtokolNo)
            {

                case 1: return "ICMP"; break;
                case 2: return "IPIP"; break;
                case 6: return "TCP"; break;
                case 8: return "EGP"; break;
                case 9: return "IGRP"; break;
                case 12: return "PVP"; break;
                case 17: return "UDP"; break;
                case 22: return "IDP"; break;
                case 47: return "GRE"; break;
                case 50: return "ESP"; break;
                case 51: return "AH"; break;
                case 55: return "mobile"; break;
                case 89: return "OSPF"; break;
                case 97: return "etherip"; break;
                case 112: return "vRRP"; break;
                case 255: return "RA"; break;
                default:
                    return "Tanýmsýz!";
                    break;
            }

        }
        private string getTcpDurum(int iTcpDurum)
        {
            switch (iTcpDurum)
            {
                case 0: return "Kapandý"; break;
                case 1: return "Dinliyor"; break;
                case 2: return "Etkin, eþ-zaman imi gönderdi"; break;
                case 3: return "Eþ-zaman imi gönderdi ve aldý"; break;
                case 4: return "Kurulu"; break;
                case 5: return "Bitiþ imi aldý, kapanmayý bekliyor"; break;
                case 6: return "Kapandý, bitiþ imi gönderdi"; break;
                case 7: return "Kapandý, bitiþ imi gönderdi, onay bekliyor"; break;
                case 8: return "Kapandý, bitiþ imi onayýný bekliyor"; break;
                case 9: return "Kapandý, bitiþ imi onaylandý"; break;
                case 10: return "Kapanmadan önce bekliyor"; break;
                default:
                    return "Tanýmsýz!";
                    break;
            }
        }
        private string getPaketYonu(string sPaketYonu)
        {
            switch (sPaketYonu)
            {
                case "i": return "içeri"; break;
                case "d": return "dýþarý"; break;
                default: return "Tanýmsýz!"; break;
            }

        }


    }
}
