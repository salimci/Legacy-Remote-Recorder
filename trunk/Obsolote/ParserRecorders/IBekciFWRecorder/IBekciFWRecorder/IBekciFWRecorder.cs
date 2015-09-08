//Rahman ve Rahim Olan Allah'ýn Adýyla
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
    public class IBekciFWRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public IBekciFWRecorder()
            : base()
        {
            LogName = "IBekciFWRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public IBekciFWRecorder(String fileName)
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

            dictHash = new Dictionary<String, Int32>();
            string sKeyWord = "zaman:protokol:kaynak ip:kaynak kapý:adres dönüþümü ip:adres dönüþümü kapý:hedef ip:hedef kapý:tcp durumlarý:paket yönü:giden bayt:gelen bayt:giden paket:gelen paket:süre";

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

                    r.CustomInt1 = ObjectToInt32(arr[dictHash["kaynak kapý"]], 0);
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["hedef kapý"]], 0);
                    r.CustomStr3 = arr[dictHash["kaynak ip"]];
                    r.CustomStr4 = arr[dictHash["hedef ip"]];
                    r.CustomStr5 = arr[dictHash["adres dönüþümü ip"]];
                    r.LogName = LogName;
                    DateTime dtFile = new DateTime(1970, 1, 1, 0, 0, 0);

                    r.Datetime = dtFile.AddSeconds(ObjectToDouble(arr[dictHash["zaman"]], 0)).ToString();
                    r.CustomStr1 = getProtokolName(ObjectToInt32(arr[dictHash["protokol"]], 0));
                    r.CustomInt3 = ObjectToInt32(arr[dictHash["protokol"]], 0);
                    r.CustomInt4 = ObjectToInt32(arr[dictHash["adres dönüþümü kapý"]], 0);
                    r.CustomStr6 = getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlarý"]].Split('/')[0], 0)) + " / " +
                                   getTcpDurum(ObjectToInt32(arr[dictHash["tcp durumlarý"]].Split('/')[1], 0));

                    r.CustomStr7 = arr[dictHash["tcp durumlarý"]];
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

                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {
                                    if (i + 1 == dFileNameList.Length)
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                            " ParseFileNameLocal() | Yeni Dosya Yok Ayný Dosyaya Devam : " + FileName);
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Dosya Sonu Okunmadý Okuma Devam Ediyor");
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
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile Ýs Null Ýlk FileName Set  : " + FileName);
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
        /// LastFile'ýn Numarasýna Göre Bir Sonraki Dosyayý Set Eder
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

        public override void Start()
        {
            base.Start();
        }

        private bool Is_File_Finished(string file)
        {
            bool isFileFinished = false;

            //Windows
            FileStream fs = null;
            BinaryReader br = null;
            FileInfo finfo = null;
            Int64 fileLength = 0;
            Int64 positionOfParser = 0;
            //Linux
            Int32 lineCount = 0;
            String stdOut = "";
            String stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            try
            {
                if (remoteHost != "")
                {
                    if (readMethod == "nread")
                    {
                        commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                        stReader = new StringReader(stdOut);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsýna bakýlýyor.");
                        //lastFile'dan line ve pozisyon okundu ve þimdi test ediliyor. 
                        while ((line = stReader.ReadLine()) != null)
                        {
                            if (line.StartsWith("~?`Position"))
                            {
                                continue;
                            }
                            lineCount++;
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsý bulundu. En az: " + lineCount);
                    }
                    else
                    {
                        commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                        stReader = new StringReader(stdOut);

                        while ((line = stReader.ReadLine()) != null)
                        {
                            lineCount++;
                        }
                    }

                    if (lineCount > 1)
                        isFileFinished = false; //return false;
                    else
                        isFileFinished = true; //return true;
                }
                else
                {
                    fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    br = new BinaryReader(fs, enc);
                    br.BaseStream.Seek(Position, SeekOrigin.Begin);
                    finfo = new FileInfo(lastFile);
                    fileLength = finfo.Length;

                    Char c = ' ';
                    while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                        c = br.ReadChar();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                        if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                        }
                    }

                    positionOfParser = br.BaseStream.Position;
                    finfo = null;
                    br.Close();
                    fs.Close();

                    finfo = new FileInfo(lastFile);
                    fileLength = finfo.Length;

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Position is: " + positionOfParser);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Length is: " + fileLength);

                    if (positionOfParser > fileLength - 2 || positionOfParser > fileLength - 1 || positionOfParser == fileLength)
                        isFileFinished = true; //return true;
                    else
                        isFileFinished = false; //return false;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştirmeden devam edeceğiz.");
                isFileFinished = false; //return false;
            }
            finally
            {
                if (se.Connected)
                    se.Close();
                if (fs != null)
                    fs.Close();
                if (br != null)
                    fs.Close();
            }

            return isFileFinished;
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


        //        public override bool ReadLocal()
        //        {
        //#if HARD_LOG_MODE
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Read Local");
        //#endif
        //            List<String> lst = new List<String>();
        //            String line = "";

        //            FileStream fs = null;
        //            BinaryReader br = null;
        //            Int64 currentPosition = Position;
        //            try
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ReadLocal Filename is " + FileName);

        //                if (readMethod == "sed")
        //                {
        //                    fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //                }
        //                else if (readMethod == "gz")
        //                {
        //                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        //#if USEZIPLIBRARYFORGZ
        //                    GZipInputStream zipInStream = new GZipInputStream(fileStreamIn);
        //#else
        //                    GZipStream zipInStream = new GZipStream(fileStreamIn, CompressionMode.Decompress);
        //#endif
        //                    String folder = Path.GetDirectoryName(FileName);
        //                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
        //                    Int32 size;
        //                    Byte[] buffer = new Byte[1024];
        //                    do
        //                    {
        //                        size = zipInStream.Read(buffer, 0, buffer.Length);
        //                        fileStreamOut.Write(buffer, 0, size);
        //                    } while (size > 0);
        //                    zipInStream.Close();
        //                    fileStreamOut.Close();
        //                    fileStreamIn.Close();
        //                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //                }
        //                else if (readMethod == "zip")
        //                {
        //                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        //                    ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
        //                    ZipEntry entry = zipInStream.GetNextEntry();
        //                    String folder = Path.GetDirectoryName(FileName);
        //                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
        //                    Int32 size;
        //                    Byte[] buffer = new Byte[1024];
        //                    do
        //                    {
        //                        size = zipInStream.Read(buffer, 0, buffer.Length);
        //                        fileStreamOut.Write(buffer, 0, size);
        //                    } while (size > 0);
        //                    zipInStream.Close();
        //                    fileStreamOut.Close();
        //                    fileStreamIn.Close();
        //                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //                }
        //                br = new BinaryReader(fs, enc);

        //                bool dontSend = false;
        //                if ((Position == -1) || (started && startFromEndOnLoss))
        //                {
        //                    startFromEndOnLoss = false;
        //                    Position = 0;
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Parser to last line");
        //                    ReverseLastLine(br);
        //                    dontSend = true;
        //                }

        //                if (Position > br.BaseStream.Length)
        //                    return false;

        //                if (!dontSend)
        //                    br.BaseStream.Seek(Position, SeekOrigin.Begin);
        //                else
        //                    br.BaseStream.Seek(0, SeekOrigin.Begin);


        //                //dali
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "FirstPosition:" + Position);
        //                if (Position != 0)
        //                    if (!dontSend)
        //                        if (started)
        //                            while (br.BaseStream.Position != 0)
        //                            {
        //                                br.BaseStream.Position = br.BaseStream.Position - 2;
        //                                if (Environment.NewLine.Contains(br.ReadChar().ToString()))
        //                                {
        //                                    Position = br.BaseStream.Position;
        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "newPosition:" + Position);
        //                                    break;
        //                                }
        //                            }


        //                Int32 readLineCount = 0;
        //                FileInfo fi = new FileInfo(FileName);
        //                Int64 fileLength = fi.Length;

        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "br.BaseStream.Position:" + br.BaseStream.Position);
        //                StringBuilder lineSb = new StringBuilder();
        //                while (br.BaseStream.Position != fileLength)
        //                {
        //                    Char c = ' ';
        //                    while (!Environment.NewLine.Contains(c.ToString()))
        //                    {
        //                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "While starting");
        //                        //Char c = ' ';
        //                        c = br.ReadChar();
        //                        if (Environment.NewLine.Contains(c.ToString()))
        //                        {

        //                            line = lineSb.ToString();
        //                            Log.Log(LogType.FILE, LogLevel.DEBUG, "new line found line:" + line);

        //                            if (!dontSend)
        //                            {
        //                                if (started)
        //                                {
        //                                    started = false;
        //                                    if (checkLineMismatch && Position != 0 && line != lastLine)
        //                                    {
        //                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line:" + line + "::");
        //                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LastLine:" + lastLine + "::");
        //                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line mismatch reading file from start");
        //                                        return false;
        //                                    }
        //                                    else
        //                                        if (checkLineMismatch && Position != 0 && line == lastLine)
        //                                        {
        //                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Kaldigi Satiri Buldu");
        //                                            dontSend = true;
        //                                        }

        //                                }

        //                                currentPosition = br.BaseStream.Position;
        //                                Position = currentPosition;


        //                                lastLine = line;

        //                                if (maxReadLineCount != -1)
        //                                {
        //                                    readLineCount++;
        //                                    if (readLineCount > maxReadLineCount)
        //                                    {
        //                                        if (threadSleepTime <= 0)
        //                                            Thread.Sleep(60000);
        //                                        else
        //                                            Thread.Sleep(threadSleepTime);
        //                                        readLineCount = 0;
        //                                    }
        //                                }
        //                            }

        //                            bool noerr = ParseSpecific(line, dontSend);
        //                            while (!noerr)
        //                            {
        //                                Thread.Sleep(threadSleepTime);
        //                                noerr = ParseSpecific(line, dontSend);
        //                            }
        //                            Log.Log(LogType.FILE, LogLevel.DEBUG, "setting registry key");
        //                            if (!dontSend)
        //                                SetRegistry();
        //                            else
        //                            {
        //                                if (usingKeywords && keywordsFound)
        //                                    break;
        //                            }
        //                            lineSb.Remove(0, lineSb.Length);
        //                            Log.Log(LogType.FILE, LogLevel.DEBUG, "looking for new line");
        //                        }
        //                        else
        //                            lineSb.Append(c);
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                parsing = false;
        //                if (br != null)
        //                {
        //                    br.Close();
        //                }
        //                if (fs != null)
        //                {
        //                    fs.Close();
        //                }
        //                if (readMethod == "gz" || readMethod == "zip")
        //                {
        //                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
        //                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
        //                }
        //                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
        //                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
        //                if (usingCheckTimer)
        //                {
        //                    if (!checkTimer.Enabled)
        //                        checkTimer.Start();

        //                    return true;
        //                }
        //                else
        //                    return false;
        //            }
        //            try
        //            {
        //                if (br != null)
        //                    br.Close();
        //                if (fs != null)
        //                    fs.Close();
        //                if (readMethod == "gz" || readMethod == "zip")
        //                {
        //                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
        //                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
        //                }
        //            }
        //            catch
        //            {
        //                if (readMethod == "gz" || readMethod == "zip")
        //                {
        //                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
        //                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
        //                }
        //            }
        //            return true;
        //        }


    }
}
