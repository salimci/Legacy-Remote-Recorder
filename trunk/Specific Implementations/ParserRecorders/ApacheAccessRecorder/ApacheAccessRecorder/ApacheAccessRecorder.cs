//Düzenleme: Ali Yıldırım, 24.11.2010
//updated by Onur SARIKAYA

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;
using System.Net;

namespace Parser
{
    public class ApacheAccessRecorder : Parser
    {
        public ApacheAccessRecorder()
            : base()
        {
            LogName = "ApacheAccessRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public ApacheAccessRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }

            if (!dontSend)
            {
                string[] strArray = line.Split('-');
                ArrayList list = new ArrayList();
                try
                {
                    int num;
                    CustomBase.Rec rec = new CustomBase.Rec();

                    for (num = 0; num < strArray.Length; num++)
                    {
                        if (!string.IsNullOrEmpty(strArray[num].Trim()))
                        {
                            list.Add(strArray[num]);
                        }
                    }

                    rec.CustomStr3 = list[0].ToString().Trim();
                    IPAddress ip_adres = null;
                    bool check = IPAddress.TryParse(rec.CustomStr3, out ip_adres);

                    if (!check)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is not begin with a valid ip address");
                        return true;
                    }

                    if (string.IsNullOrEmpty(remoteHost) == false)
                        rec.ComputerName = remoteHost;

                    rec.LogName = LogName;
                    rec.CustomStr10 = Dir;

                    if (list.Count > 1)
                    {
                        string[] strArray2 = list[1].ToString().Trim().Split('[');
                        if (!string.IsNullOrEmpty(strArray2[0]))
                        {
                            rec.UserName = strArray2[0];
                        }
                        strArray2 = strArray2[1].Split(']');
                        string[] strArray3 = strArray2[0].Trim('[').Split(':');
                        rec.Datetime = Convert.ToDateTime(strArray3[0] + " " + strArray3[1] + ":" + strArray3[2] + ":" + strArray3[3]).ToString("yyyy/MM/dd HH:mm:ss");
                        strArray2 = strArray2[1].Trim().Split(' ');
                        rec.EventType = strArray2[0].Trim('"');
                        bool flag = false;
                        string str = "";
                        for (num = 1; num < strArray2.Length; num++)
                        {
                            if (!strArray2[num].StartsWith("HTT"))
                            {
                                if (!flag)
                                {
                                    flag = true;
                                    rec.Description = rec.Description + " " + strArray2[num];
                                }
                                else
                                {
                                    str = str + " " + strArray2[num];
                                }
                            }
                            else
                            {
                                str = str + " " + strArray2[num];
                            }
                        }
                        string[] strArray4 = str.Replace('-', ' ').Trim().Split(' ');
                        rec.CustomStr1 = strArray4[0];
                        if (strArray4.Length > 1)
                        {
                            rec.CustomInt1 = Convert.ToInt32(strArray4[1]);
                        }
                        if (strArray4.Length > 2)
                        {
                            rec.CustomInt2 = Convert.ToInt32(strArray4[2]);
                        }
                    }

                    SetRecordData(rec);

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Start");

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();
                //foreach (String file in Directory.GetFiles(Dir))
                //{
                //    string sFile = Path.GetFileName(file).ToString();
                //    if (sFile.StartsWith("access_") == true)
                //        arrFileNames.Add(sFile);
                //}
                foreach (String file in Directory.GetFiles(Dir))
                {
                    string sFile = Path.GetFileName(file);
                    if (sFile.StartsWith("access."))
                        arrFileNames.Add(sFile);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + Dir);
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                        "ParseFileNameLocal() | Sorting file in directory: " + arrFileNames.Count.ToString());
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + arrFileNames[i].ToString());
                }

                String[] dFileNameList = SortFiles(arrFileNames);

                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + dFileNameList[i].ToString());
                }


                if (dFileNameList.Length > 0)
                {
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
                                //Dosya bitmiş değiştirilecek.

                                Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Position is at the end of the file so changing the File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            //Daha yeni bir doya yok. Burada bekliyoruz.

                                            FileName = lastFile;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                "ParseFileNameLocal() | Yeni Dosya Yok Aynı Dosyaya Devam : " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            //Daha yeni dosyaya ekliyoruz.

                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            lastFile = FileName;
                                            Position = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                "ParseFileNameLocal() | Yeni Dosya atandı. Lastfile : " + FileName);

                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Dosyayı okumaya devam.

                                Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameLocal | Dosya Sonu Okunmadı Okuma Devam Ediyor");
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "ParseFileNameLocal() | FileName = LastFile " + FileName);
                            }
                        }
                        else
                        {
                            //son okunan dosya bulunamadı en yeni oluşan dosya okunmaya başlanacak.
                            if (dFileNameList.Length > 0)
                            {
                                FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                       "ParseFileNameLocal() | Lastfile directory'de bulunamadı. LastFile en yeni dosya olarak atandı : " + FileName);
                            }
                        }
                    }
                    else
                    {
                        //ilk defa log okunacak.

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0];
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                   "ParseFileNameLocal() | İlk defa log okunacak . Lastfile en eski dosya olarak atandı : " + FileName);
                        }
                    }
                }
                else
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() | There is any file in directory.");
            }
            else
                FileName = Dir;
        }

        protected override void ParseFileNameRemote()
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
                    String command = "ls -lt " + Dir + " | grep access_"; //ls access_*.log
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("access_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                        //if (arr[arr.Length - 1].StartsWith("access.") && arr[arr.Length - 1].Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        //{
                        //    arrFileNameList.Add(arr[arr.Length - 1]);
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        //}
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->>Dosya sayısı : " + arrFileNameList.Count.ToString());
                    for (int i = 0; i < arrFileNameList.Count; i++)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                "ParseFileNameRemote() -->>Dosya sayısı : " + arrFileNameList[i].ToString());
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atýlan dosya isimleri sýralandý. ");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            if (Is_File_Finished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = Dir + dFileNameList[i + 1].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
            }
            finally
            {
                //stdOut = "";
                //stdErr = "";
                //if (se.Connected)
                //    se.Close();
            }
        }

        private bool Is_File_Finished(string file)
        {
            int lineCount = 0;
            string stdOut = "";
            string stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            try
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

                if (lineCount > 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştimeden devam edeceğiz.");
                return false;
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    //Ulaştırma: access_log.1277337600
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Onur : " + arrFileNames[i].ToString());

                    //string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    // Replace with
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Onur : " + parts[0].ToString());
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Onur : " + parts[1].ToString());
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Onur : " + parts[2].ToString());
                    if (parts.Length == 3)
                    {
                        if (parts[1] == "log")
                        {
                            dFileNumberList[i] = Convert.ToUInt64(parts[2]);
                            dFileNameList[i] = arrFileNames[i].ToString();
                        }
                        else
                        {
                            dFileNumberList[i] = Convert.ToUInt64(parts[1]);
                            dFileNameList[i] = arrFileNames[i].ToString();
                        }
                    }
                    else
                    {
                        dFileNameList[i] = null;
                    }
                }

                Array.Sort(dFileNumberList, dFileNameList);

                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sıralanmış dosya isimleri yazılıyor.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            }

            return dFileNameList;
        }

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in ApacheAccessRecorder -->> Enter the Function.");
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in ApacheAccessRecorder -->> ParseFileName() method should be trigerred. ");
        //    ParseFileName();
        //}

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    dayChangeTimer.Enabled = false;

        //    if (Today.Day != DateTime.Now.Day || FileName == null)
        //    {
        //        String oldFile = FileName;
        //        DateTime oldTime = Today;
        //        Int64 oldPosition = Position;
        //        String oldLastLine = lastLine;
        //        Today = DateTime.Now;
        //        Stop();
        //        ParseFileName();
        //        if (oldFile == FileName)
        //        {
        //            if (FileName == null)
        //            {
        //                Today = oldTime;
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "  ApacheAccessRecorder In ParseFileNameRemote() -->> Cannot Find File To Parse, Please Check Your Path!");
        //            }
        //            else
        //            {
        //                Today = DateTime.Now;
        //                Position = oldPosition;
        //                lastLine = oldLastLine;
        //                Start();
        //            }
        //        }
        //        else
        //        {
        //            Start();
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "  ApacheAccessRecorder In ParseFileNameRemote() -->> Day Changed, New File Is, " + FileName);
        //        }
        //    }
        //    else/* if (remoteHost == "")*/
        //    {
        //        String fileLast = FileName;
        //        ParseFileName();
        //        if (FileName != fileLast)
        //        {
        //            Stop();
        //            Position = 0;
        //            lastLine = "";
        //            FileName = fileLast;
        //            Start();
        //            Log.Log(LogType.FILE, LogLevel.INFORM, "  ApacheAccessRecorder In ParseFileNameRemote() -->> File changed, new file is, " + FileName);
        //        }
        //    }

        //    dayChangeTimer.Enabled = true;
        //}

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
