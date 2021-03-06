//Düzenleme: Ali Yıldırım, 24.11.2010
// Updated by Onur Sarıkaya 22,02,2012

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.Collections;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class ApacheErrorRecorder : Parser
    {

        public ApacheErrorRecorder()
            : base()
        {
            LogName = "ApacheErrorRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false, '[', ']');

                try
                {
                    Rec r = new Rec();

                    arr[0] = arr[0].TrimStart('[').TrimEnd(']');

                    r.CustomStr10 = Dir;

                    String[] dateArr = arr[0].Split(' ');

                    StringBuilder date = new StringBuilder();
                    if (dateArr[2] == "")
                        date.Append(dateArr[3]).Append(" ").Append(dateArr[1]).Append(" ").Append(dateArr[5]).Append(" ").Append(dateArr[4]);
                    else
                        date.Append(dateArr[2]).Append(" ").Append(dateArr[1]).Append(" ").Append(dateArr[4]).Append(" ").Append(dateArr[3]);

                    DateTime dt = DateTime.Parse(date.ToString());
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                    r.EventType = arr[1].TrimStart('[').TrimEnd(']');
                    arr[2] = arr[2].TrimStart('[').TrimEnd(']');
                    String[] nameArr = arr[2].Split(' ');
                    if (nameArr.Length != 2)
                        r.CustomStr5 = arr[2];
                    else
                        r.CustomStr5 = nameArr[1];
                    r.EventCategory = arr[3].TrimStart('[').TrimEnd(']');
                    StringBuilder desc = new StringBuilder();
                    for (Int32 i = 4; i < arr.Length; i++)
                        desc.Append(arr[i]).Append(" ");

                    desc.Remove(desc.Length - 1, 1);
                    r.Description = desc.ToString();

                    if (remoteHost != "")
                        r.CustomStr4 = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.CustomStr4 = arrLocation[2];
                    }

                    r.LogName = LogName;

                    SetRecordData(r);
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
                //    if (sFile.StartsWith("error_") == true)
                //        arrFileNames.Add(sFile);
                //}

                foreach (String file in Directory.GetFiles(Dir))
                {
                    string sFile = Path.GetFileName(file).ToString();
                    if (sFile.StartsWith("error.") == true)
                        arrFileNames.Add(sFile);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Sorting file in directory: " + Dir);

                String[] dFileNameList = SortFiles(arrFileNames);
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
                    String command = "ls -lt " + Dir + " | grep error_";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();
                    bool foundAnyFile = false;
                    int fileCnt = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        fileCnt++;
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("error_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun Dosya ismi okundu: " + arr[arr.Length - 1]);
                            foundAnyFile = true;
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun OLMAYAN Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }
                    if (!foundAnyFile)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is " + fileCnt + " files counted but there is no proper file in directory; starting like 'error_'");
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
                    //string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        //dFileNumberList[i] = Convert.ToUInt64(parts[1]);
                        //dFileNameList[i] = arrFileNames[i].ToString();
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

                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralam işlemi. Mesaj: " + ex.ToString());
            }

            return dFileNameList;
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Masaj: " + e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }
    }
}
