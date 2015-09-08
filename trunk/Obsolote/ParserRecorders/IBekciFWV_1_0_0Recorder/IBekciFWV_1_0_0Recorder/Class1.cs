//Rahman ve Rahim Olan Allah'in Adiyla
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
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;

namespace Parser
{
    public class IBekciFWV_1_0_0Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public IBekciFWV_1_0_0Recorder()
            : base()
        {
            LogName = "IBekciFWV_1_0_0Recorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public IBekciFWV_1_0_0Recorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null or Empty");
                return true;
            }

            dictHash = new Dictionary<String, Int32>();
            string sKeyWord = "zaman:protokol:kaynak ip:kaynak kapi:adres dönüþümü ip:adres dönüþümü kapi:hedef ip:hedef kapi:tcp durumlari:paket yönü:giden bayt:gelen bayt:giden paket:gelen paket:süre";

            String[] fields = sKeyWord.Split(':');
            Int32 count = 0;

            foreach (String field in fields)
            {
                dictHash.Add(field, count);
                count++;
            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | Keywords in dictHash");

            if (!dontSend)
            {

                String[] arr = line.Split(':');

                try
                {

                    Rec r = new Rec();

                    r.CustomInt1 = ObjectToInt32(arr[dictHash["kaynak kapi"]], 0);
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["hedef kapi"]], 0);
                    r.CustomStr3 = arr[dictHash["kaynak ip"]];
                    r.CustomStr4 = arr[dictHash["hedef ip"]];
                    r.CustomStr5 = arr[dictHash["adres dönüþümü ip"]];
                    r.LogName = LogName;
                    DateTime dtFile = new DateTime(1970, 1, 1, 0, 0, 0);

                    r.Datetime = dtFile.AddSeconds(ObjectToDouble(arr[dictHash["zaman"]], 0)).ToString();
                    r.CustomStr1 = getProtokolName(ObjectToInt32(arr[dictHash["protokol"]], 0));
                    r.CustomInt3 = ObjectToInt32(arr[dictHash["protokol"]], 0);
                    r.CustomInt4 = ObjectToInt32(arr[dictHash["adres dönüþümü kapi"]], 0);
                    r.CustomStr6 = getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlari"]].Split('/')[0], 0)) + " / " +
                                   getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlari"]].Split('/')[1], 0));

                    r.CustomStr7 = arr[dictHash["tcp durumlari"]];
                    r.CustomStr8 = getPaketYonu(arr[dictHash["paket yönü"]]);
                    r.CustomInt5 = ObjectToInt32(arr[dictHash["giden bayt"]], 0);
                    r.CustomInt6 = ObjectToInt64(arr[dictHash["gelen bayt"]], 0);
                    r.CustomInt7 = ObjectToInt64(arr[dictHash["giden paket"]], 0);
                    r.CustomInt8 = ObjectToInt64(arr[dictHash["gelen paket"]], 0);
                    r.CustomInt9 = ObjectToInt64(arr[dictHash["süre"]], 0);


                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Finish Record Data");


                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                    return true;
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
                    if (sFile.StartsWith("aad") == true)
                        arrFileNames.Add(sFile);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + Dir);
                String[] dFileNameList = SortFileNameByFileNumber(arrFileNames);


                if (string.IsNullOrEmpty(lastFile) == false)
                {

                    if (File.Exists(lastFile) == true)
                    {



                        if (Is_File_Finished(lastFile))
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal | Position is at the end of the file so changing the File");

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {

                                if (Dir + dFileNameList[i] == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseFileNameLocal() | Yeni Dosya Yok Ayni Dosyaya Devam : " + FileName);
                                        break;

                                    }
                                    else
                                    {
                                        FileName = Dir + dFileNameList[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Position = 0;

                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseFileNameLocal() | Yeni Dosya  : " + FileName);
                                        break;
                                    }

                                }

                            }

                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Dosya Sonu Okunmadi Okuma Devam Ediyor");
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
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile is Null ilk FileName Set  : " + FileName);
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
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Replace("aad", "").Trim().Split('.')[0]);
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
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        }

        /// <summary>
        /// LastFile'in Numarasina Göre Bir Sonraki Dosyayi Set Eder
        /// </summary>
        /// <param name="dFileNameList"></param>
        private void SetBirSonrakiFile(String[] dFileNameList, string sFunction)
        {

            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Replace("aad", "").Trim().Split('.')[0]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Replace("aad", "").Trim().Trim().Split('.')[0]);

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                            sFunction + " | LastFile Silinmis , Dosya Bulunamadi  Yeni File : " + FileName);
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
            base.Start();
        }

        private bool Is_File_Finished(string file)
        {
            int lineCount = 0;
            String stdOut = "";
            String stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";
            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " IsFileFinished() -->> reading local file! ");

                    fileStream.Seek(Position, SeekOrigin.Begin);
                    FileInfo fileInfo = new FileInfo(file);
                    Int64 fileLength = fileInfo.Length;
                    Byte[] byteArray = new Byte[3];
                    fileStream.Read(byteArray, 0, 3);
                   
                    using (StreamReader sr = new StreamReader(file))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                        }

                        if ((line == sr.ReadLine()) != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
                return false;
            }
        } // Is_File_Finished


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
        private double ObjectToDouble(string sObject, double iReturn)
        {
            try
            {
                return Convert.ToDouble(sObject);
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
                    return "Tanimsiz!";
                    break;
            }

        }
        private string getTcpDurum(int iTcpDurum)
        {
            switch (iTcpDurum)
            {
                case 0: return "Kapandi"; break;
                case 1: return "Dinliyor"; break;
                case 2: return "Etkin, eþ-zaman imi gönderdi"; break;
                case 3: return "Eþ-zaman imi gönderdi ve aldi"; break;
                case 4: return "Kurulu"; break;
                case 5: return "Bitiþ imi aldi, kapanmayi bekliyor"; break;
                case 6: return "Kapandi, bitiþ imi gönderdi"; break;
                case 7: return "Kapandi, bitiþ imi gönderdi, onay bekliyor"; break;
                case 8: return "Kapandi, bitiþ imi onayini bekliyor"; break;
                case 9: return "Kapandi, bitiþ imi onaylandi"; break;
                case 10: return "Kapanmadan önce bekliyor"; break;
                default:
                    return "Tanimsiz!";
                    break;
            }
        }
        private string getPaketYonu(string sPaketYonu)
        {
            switch (sPaketYonu)
            {
                case "i": return "içeri"; break;
                case "d": return "diþari"; break;
                default: return "Tanimsiz!"; break;
            }

        }


    

    }
}
