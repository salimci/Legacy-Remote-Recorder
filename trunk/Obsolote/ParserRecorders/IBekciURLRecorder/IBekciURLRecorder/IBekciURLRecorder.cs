//Rahman ve Rahim Olan Allah'ýn Adýyla
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using IBekciURLRecorder;
using Log;
using CustomTools;
using System.Collections;
using System.Globalization;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using IBekciURLRecorder;

namespace IBekciURLRecorder
{
    public class IBekciURLRecorder : Parser.Parser
    {
        Dictionary<String, Int32> dictHash;

        public IBekciURLRecorder()
            : base()
        {
            LogName = "IBekciURLRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public IBekciURLRecorder(String fileName)
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

            string sKeyWord = "zaman kaynakip hedefip tür kuralno izin kategori url";

            dictHash = new Dictionary<String, Int32>();

            String[] fields = sKeyWord.Split(' ');
            Int32 count = 0;

            foreach (String field in fields)
            {
                dictHash.Add(field, count);
                count++;
            }


            if (!dontSend)
            {

                String[] arr = SpaceSplit(line, false); ;

                try
                {

                    Rec r = new Rec();

                    r.CustomStr3 = arr[dictHash["kaynakip"]];
                    r.CustomStr4 = arr[dictHash["hedefip"]];
                    r.Description = arr[dictHash["url"]];
                    r.CustomStr6 = r.Description.Substring(0, r.Description.IndexOf('/'));

                    r.EventCategory = arr[dictHash["kategori"]];
                    r.LogName = LogName;
                    DateTime dtFile = new DateTime(1970, 1, 1, 0, 0, 0);
                    r.Datetime = dtFile.AddSeconds(ObjectToDouble(arr[dictHash["zaman"]], 0)).ToString();
                    r.CustomStr2 = arr[dictHash["tür"]];
                    r.CustomInt1 = ObjectToInt32(arr[dictHash["kuralno"]], 0);
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["izin"]], 0);
                    r.CustomStr5 = getIzin(ObjectToInt32(arr[dictHash["izin"]], -1));

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
                    if (sFile.StartsWith("web") == true)
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
                FileName = Dir;


        }

        private string[] SortFileNameByFileNumber(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Replace("web", "").Trim().Split('.')[0]);
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
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Replace("web", "").Trim().Split('.')[0]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Replace("web", "").Trim().Split('.')[0]);

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

        private string getIzin(int iIzinKod)
        {
            switch (iIzinKod)
            {
                case 1: return "Geçti";
                case 0: return "Takýldý";
                default: return "-";
            }

        }

        //build ederken  "public override bool ReadLocal()" hali ile hata verdi
        //public override bool ReadLocal()
        protected bool ReadLocal()
        {
#if HARD_LOG_MODE
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Read Local");
#endif
            List<String> lst = new List<String>();
            String line = "";

            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ReadLocal Filename is " + FileName);

                if (readMethod == "sed")
                {
                    fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                else if (readMethod == "gz")
                {
                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);
#if USEZIPLIBRARYFORGZ
                    GZipInputStream zipInStream = new GZipInputStream(fileStreamIn);
#else
                    GZipStream zipInStream = new GZipStream(fileStreamIn, CompressionMode.Decompress);
#endif
                    String folder = Path.GetDirectoryName(FileName);
                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
                    Int32 size;
                    Byte[] buffer = new Byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    zipInStream.Close();
                    fileStreamOut.Close();
                    fileStreamIn.Close();
                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                else if (readMethod == "zip")
                {
                    FileStream fileStreamIn = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    ZipInputStream zipInStream = new ZipInputStream(fileStreamIn);
                    ZipEntry entry = zipInStream.GetNextEntry();
                    String folder = Path.GetDirectoryName(FileName);
                    FileStream fileStreamOut = new FileStream(folder + "\\tempZip", FileMode.Create, FileAccess.Write);
                    Int32 size;
                    Byte[] buffer = new Byte[1024];
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    zipInStream.Close();
                    fileStreamOut.Close();
                    fileStreamIn.Close();
                    fs = new FileStream(folder + "\\tempZip", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                br = new BinaryReader(fs, enc);

                bool dontSend = false;
                if ((Position == -1) || (started && startFromEndOnLoss))
                {
                    startFromEndOnLoss = false;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Parser to last line");
                    ReverseLastLine(br);
                    dontSend = true;
                }

                if (Position > br.BaseStream.Length)
                    return false;

                if (!dontSend)
                    br.BaseStream.Seek(Position, SeekOrigin.Begin);
                else
                    br.BaseStream.Seek(0, SeekOrigin.Begin);


                //dali
                Log.Log(LogType.FILE, LogLevel.DEBUG, "FirstPosition:" + Position);
                if (Position != 0)
                    if (!dontSend)
                        if (started)
                            while (br.BaseStream.Position != 0)
                            {
                                br.BaseStream.Position = br.BaseStream.Position - 2;
                                if (Environment.NewLine.Contains(br.ReadChar().ToString()))
                                {
                                    Position = br.BaseStream.Position;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "newPosition:" + Position);
                                    break;
                                }
                            }


                Int32 readLineCount = 0;
                FileInfo fi = new FileInfo(FileName);
                Int64 fileLength = fi.Length;

                Log.Log(LogType.FILE, LogLevel.DEBUG, "br.BaseStream.Position:" + br.BaseStream.Position);
                StringBuilder lineSb = new StringBuilder();
                while (br.BaseStream.Position != fileLength)
                {
                    Char c = ' ';
                    while (!Environment.NewLine.Contains(c.ToString()))
                    {
                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "While starting");
                        //Char c = ' ';
                        c = br.ReadChar();
                        if (Environment.NewLine.Contains(c.ToString()))
                        {

                            line = lineSb.ToString();
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "new line found line:" + line);

                            if (!dontSend)
                            {
                                if (started)
                                {
                                    started = false;
                                    if (checkLineMismatch && Position != 0 && line != lastLine)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line:" + line + "::");
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LastLine:" + lastLine + "::");
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line mismatch reading file from start");
                                        return false;
                                    }
                                    else
                                        if (checkLineMismatch && Position != 0 && line == lastLine)
                                        {
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Kaldigi Satiri Buldu");
                                            dontSend = true;
                                        }

                                }

                                currentPosition = br.BaseStream.Position;
                                Position = currentPosition;


                                lastLine = line;

                                if (maxReadLineCount != -1)
                                {
                                    readLineCount++;
                                    if (readLineCount > maxReadLineCount)
                                    {
                                        if (threadSleepTime <= 0)
                                            Thread.Sleep(60000);
                                        else
                                            Thread.Sleep(threadSleepTime);
                                        readLineCount = 0;
                                    }
                                }
                            }

                            bool noerr = ParseSpecific(line, dontSend);
                            while (!noerr)
                            {
                                Thread.Sleep(threadSleepTime);
                                noerr = ParseSpecific(line, dontSend);
                            }
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "setting registry key");
                            if (!dontSend)
                                SetRegistry();
                            else
                            {
                                if (usingKeywords && keywordsFound)
                                    break;
                            }
                            lineSb.Remove(0, lineSb.Length);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "looking for new line");
                        }
                        else
                            lineSb.Append(c);
                    }
                }
            }
            catch (Exception e)
            {
                parsing = false;
                if (br != null)
                {
                    br.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                if (usingCheckTimer)
                {
                    if (!checkTimer.Enabled)
                        checkTimer.Start();

                    return true;
                }
                else
                    return false;
            }
            try
            {
                if (br != null)
                    br.Close();
                if (fs != null)
                    fs.Close();
                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
            }
            catch
            {
                if (readMethod == "gz" || readMethod == "zip")
                {
                    if (File.Exists(Path.GetDirectoryName(FileName) + "\\tempZip"))
                        File.Delete(Path.GetDirectoryName(FileName) + "\\tempZip");
                }
            }
            return true;
        }


    }
}
