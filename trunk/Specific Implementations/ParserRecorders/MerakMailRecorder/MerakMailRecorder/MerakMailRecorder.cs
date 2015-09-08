//Name: Merak Mail Recorder
//Writer: Ali Yıldırım
//Date: 18.01.2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using System.Collections;

namespace Parser
{
    public class MerakMailRecorder : Parser
    {
        Data data;

        public MerakMailRecorder()
            : base()
        {
            LogName = "MerakMailRecorder";
            usingKeywords = false;
            lineLimit = 50;
            data = new Data();
        }

        public override void Init()
        {
            GetFiles();
        }

        public override void Start()
        {
            base.Start();
        }

        public MerakMailRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);
                if (string.IsNullOrEmpty(line))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Line is Null Or Empty. ");
                    return true;
                }

                if (line.Length < 850)
                    data.description = line;
                else
                    data.description = line.Substring(0, 850);

                data.logname = LogName;

                if (!dontSend)
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //10.6.3.155      [0838] 09:33:03 Connected, local IP=10.6.21.17
                    //10.6.21.6       [15C0] 00:02:10 >>> 220 mail.kosgeb.gov.tr ESMTP IceWarp 10.3.1; Mon, 16 May 2011 00:02:10 +0300
                    //10.6.21.6       [15C0] 00:02:10 <<< EHLO sbg.kosgeb.gov.tr
                    //10.6.21.6       [15C0] 00:02:10 >>> 250-mail.kosgeb.gov.tr Hello sbg.kosgeb.gov.tr [10.6.21.6], pleased to meet you.
                    //10.6.21.6       [15C0] 00:02:10 <<< MAIL FROM:<adventyazilimbilgi@mynet.com>
                    //10.6.21.6       [15C0] 00:02:10 >>> 250 2.1.0 <adventyazilimbilgi@mynet.com>... Sender ok
                    //10.6.21.6       [15C0] 00:02:10 <<< RCPT TO:<alper.soykal@kosgeb.gov.tr>
                    //10.6.21.6       [15C0] 00:02:10 >>> 250 2.1.5 <alper.soykal@kosgeb.gov.tr>... Recipient ok
                    //10.6.21.6       [15C0] 00:02:10 <<< DATA
                    //10.6.21.6       [15C0] 00:02:10 >>> 354 Enter mail, end with "." on a line by itself
                    //10.6.21.6       [15C0] 00:02:10 <<< 2809 bytes (overall data transfer speed=325115741 B/s)
                    //10.6.21.6       [15C0] 00:02:10 Start of mail processing
                    //10.6.21.6       [15C0] 00:02:12 *** <adventyazilimbilgi@mynet.com> <alper.soykal@kosgeb.gov.tr> 1 2804 00:00:01 OK YAR93710
                    //10.6.21.6       [15C0] 00:02:12 >>> 250 2.6.0 2804 bytes received in 00:00:01; Message id YAR93710 accepted for delivery
                    //10.6.21.6       [15C0] 00:02:17 <<< QUIT

                    if (parts.Length > 3)
                    {
                        data.computername = parts[0];
                        string tarih = lastFile.Split(new char[] { '\\', '/' })[lastFile.Split(new char[] { '\\', '/' }).Length - 1].Substring(1, 8);
                        string yil = tarih.Substring(0, 4);
                        string ay = tarih.Substring(4, 2);
                        string gun = tarih.Substring(6, 2);

                        string saat = parts[2];
                        data.datetime = Convert.ToDateTime(yil + "/" + ay + "/" + gun + " " + saat).ToString("yyyy-MM-dd HH:mm:ss");

                        if (line.Contains("Connected"))
                        {
                            bool result = false;
                            if (!string.IsNullOrEmpty(data.datetime))
                            {
                                result = Send_Rec_Data(data);
                            }

                            if (result)
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Data is send successfully. ");
                            else
                                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Data could not send. ");


                            data = new Data();
                        }

                        if (line.Contains("bytes (overall"))
                        {
                            data.str6 = "";
                            for (int i = 4; i < parts.Length; i++)
                            {
                                data.str6 += parts[i] + " ";
                            }
                            data.str6 = data.str6.Trim();
                        }

                        if (line.Contains("***"))
                        {
                            //10.6.21.6       [1644] 09:33:31 *** <e3-1251596693647-309a5II8ebbe2@e3.emarsys.net> <egemen.demirag@kosgeb.gov.tr> 1 135081 00:00:00 OK ZLX67131

                            data.str3 = parts[4].TrimStart('<').TrimEnd('>');
                            data.str4 = parts[5].TrimStart('<').TrimEnd('>');
                            data.str8 = parts[parts.Length - 1];
                        }

                        // Message id 
                        if (line.Contains("Message id"))
                        {
                            for (int i = 5; i < parts.Length; i++)
                            {
                                if (parts[i] == "Message")
                                {
                                    data.eventtype = "";
                                    for (int j = i + 3; j < parts.Length; j++)
                                    {
                                        data.eventtype += parts[j] + " ";
                                    }
                                    data.eventtype = data.eventtype.Trim();

                                    break;
                                }
                            }
                        }

                        if (line.Contains("Sender ok"))
                        {
                            data.str1 = "Sender ok";
                        }
                        if (line.Contains("Recipient ok"))
                        {
                            data.str2 = "Recipient ok";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                return true;
            }

            return true;
        }

        private bool Send_Rec_Data(Data data)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                rec.ComputerName = data.computername;
                rec.Datetime = data.datetime;
                rec.Description = data.description;
                rec.EventType = data.eventtype;
                rec.LogName = data.logname;
                rec.CustomStr1 = data.str1;
                rec.CustomStr2 = data.str2;
                rec.CustomStr3 = data.str3;
                rec.CustomStr4 = data.str4;
                rec.CustomStr6 = data.str6;
                rec.CustomStr7 = data.str7;
                rec.CustomStr8 = data.str8;

                SetRecordData(rec);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Start.");

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Searching for file in directory: " + Dir);

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    ArrayList arrFileNames = new ArrayList();


                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        string sFile = Path.GetFileName(file).ToString();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | File : " + sFile);

                        if (sFile.StartsWith("s") == true)
                        {
                            arrFileNames.Add(sFile);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | File in Match : " + sFile);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Sorting file in directory: " + Dir);

                    String[] dFileNameList = SortFiles(arrFileNames);

                    if (dFileNameList.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(lastFile))
                        {
                            if (File.Exists(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | lastFile is not null. LastFile : " + lastFile);

                                FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                BinaryReader br = new BinaryReader(fs, enc);

                                br.BaseStream.Seek(Position, SeekOrigin.Begin);

                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                                #region dosyanın sonunu bulamayabilir.
                                FileInfo finfo = new FileInfo(lastFile);
                                Int64 fileLength = finfo.Length;
                                Char c = ' ';
                                while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                                    c = br.ReadChar();
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                                    if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                                    }
                                }
                                #endregion

                                if (br.BaseStream.Position == br.BaseStream.Length - 2 || br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal | Position is at the end of the file so changing the File");

                                    for (int i = 0; i < dFileNameList.Length; i++)
                                    {
                                        if (Dir + dFileNameList[i].ToString() == lastFile)
                                        {
                                            if (i + 1 == dFileNameList.Length)
                                            {
                                                FileName = lastFile;
                                                lastFile = FileName;
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Yeni Dosya Yok. Aynı Dosyaya Devam : " + FileName);
                                                break;
                                            }
                                            else
                                            {
                                                FileName = Dir + dFileNameList[(i + 1)].ToString();
                                                lastFile = FileName;
                                                Position = 0;
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Eski dosya bitti. Yeni Dosya  : " + FileName);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Dosya Sonu Okunmadı Okuma Devam Ediyor");
                                    FileName = lastFile;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | FileName = LastFile " + FileName);
                                }
                            }
                            else
                            {
                                //Directory deki en son oluşturulmuş dosya okunacak.
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Daha önceden okunan dosya Directory'de bulunamadı. LastFile is not found.");

                                FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() |  Directorydeki en yeni dosyaya atandı.  : " + FileName);
                            }
                        }
                        else
                        {
                            //Directorydeki ilk oluşan dosya okunack.
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Daha önceden hiç dosya okunmamış.");

                            FileName = Dir + dFileNameList[0];
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Directorydeki en eski dosyaya atandı.  : " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | There is any file in the directory.");
                    }

                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Dosya doğrudan verildi...  : " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | En dış hata: " + ex.ToString());
            }
        }

        protected override void ParseFileNameRemote()
        {
            //    string line = "";
            //    String stdOut = "";
            //    String stdErr = "";

            //    try
            //    {
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
            //        se = new SshExec(remoteHost, user);
            //        se.Password = password;
            //        if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            //        {
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
            //            String command = "ls -lt " + Dir + " | grep access_"; //ls access_*.log
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

            //            se.Connect();
            //            se.RunCommand(command, ref stdOut, ref stdErr);
            //            se.Close();

            //            StringReader sr = new StringReader(stdOut);
            //            ArrayList arrFileNameList = new ArrayList();

            //            while ((line = sr.ReadLine()) != null)
            //            {
            //                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //                if (arr[arr.Length - 1].StartsWith("access_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
            //                {
            //                    arrFileNameList.Add(arr[arr.Length - 1]);
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
            //                }
            //            }

            //            String[] dFileNameList = SortFiles(arrFileNameList);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atýlan dosya isimleri sýralandý. ");

            //            if (!String.IsNullOrEmpty(lastFile))
            //            {
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
            //                bool bLastFileExist = false;

            //                for (int i = 0; i < dFileNameList.Length; i++)
            //                {
            //                    if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
            //                    {
            //                        bLastFileExist = true;
            //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
            //                        break;
            //                    }
            //                }

            //                if (bLastFileExist)
            //                {
            //                    if (Is_File_Finished(lastFile))
            //                    {
            //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

            //                        for (int i = 0; i < dFileNameList.Length; i++)
            //                        {
            //                            if (Dir + dFileNameList[i].ToString() == lastFile)
            //                            {
            //                                if (dFileNameList.Length > i + 1)
            //                                {
            //                                    FileName = Dir + dFileNameList[i + 1].ToString();
            //                                    Position = 0;
            //                                    lastFile = FileName;
            //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
            //                                    break;
            //                                }
            //                                else
            //                                {
            //                                    FileName = lastFile;
            //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
            //                                }
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        FileName = lastFile;
            //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
            //                    }
            //                }
            //                else
            //                {
            //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
            //                    Position = 0;
            //                    lastFile = FileName;
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
            //                }
            //            }
            //            else
            //            {
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

            //                if (dFileNameList.Length > 0)
            //                {
            //                    FileName = Dir + dFileNameList[0].ToString();
            //                    lastFile = FileName;
            //                    Position = 0;
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
            //                }
            //                else
            //                {
            //                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            FileName = Dir;
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
            //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
            //    }
            //    finally
            //    {
            //        //stdOut = "";
            //        //stdErr = "";
            //        //if (se.Connected)
            //        //    se.Close();
            //    }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    //s20110516-00.log
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

                    Int64 sayi = Convert.ToInt64(parts[0].TrimStart('s') + parts[1]);
                    dFileNumberList[i] = Convert.ToUInt64(sayi);
                    dFileNameList[i] = arrFileNames[i].ToString();

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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | Entered.");
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                this.ParseFileNameLocal();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | Exit.");
        }
    }

    public class Data
    {
        public string datetime;
        public string computername;
        public string eventtype;
        public string description;
        public string logname;
        public string str1;
        public string str2;
        public string str3;
        public string str4;
        public string str6;
        public string str7;
        public string str8;
    }
}
