//WebwasherAccessDeniedV_6_9_3_1Recorder

// ay
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log;
using System.IO;
using System.Collections;
using SharpSSH.SharpSsh;
using System.Globalization;
using System.Timers;

namespace Parser
{
    public class WebwasherAccessDeniedV_6_9_3_1Recorder : Parser
    {
        public WebwasherAccessDeniedV_6_9_3_1Recorder()
            : base()
        {
            LogName = "WebwasherAccessDeniedRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                // eski --->>  192.168.20.62 eucfcu\sgurbuz [28/Dec/2010:10:35:36 +0000] "GET http://www.playboy.com/ HTTP/1.1" "sx"
                //192.168.20.100 - 192.168.20.100 - EUCFCU\gaktas - [29/Dec/2010:10:56:00 +0200] - "GET http://forum.memurlar.net/htc/banner.htc HTTP/1.1" - ""
                //192.168.20.159 - leventaydos.eucfcu.local - EUCFCU\laydos - [30/Dec/2010:10:10:14 +0200] - "GET http://forum.memurlar.net/htc/banner.htc HTTP/1.1" - ""
                //192.168.20.191 - mypc-d106bee486 - EUCFCU\btanrikulu - [30/Dec/2010:13:05:29 +0200] - "GET http://ecl.labs.popcap.com/v118/facebook/bj2/js/kt_common.js HTTP/1.1" - ""


                //Yeni format
                //192.168.20.186 209.85.149.189 "EUCFCU\mgcelik" [22/Feb/2011:10:13:05 +0200] "CONNECT chatenabled.mail.google.com:443 HTTP/1.0" 403 1091 1834 "" "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; InfoPath.3; .NET4.0C; .NET4.0E)" "im" 10 "-" "Genel" 0.024 "-" Neutral -
                //192.168.20.180 69.63.189.16 "EUCFCU\lsutcu" [22/Feb/2011:10:13:27 +0200] "POST http://www.facebook.com/ajax/chat/buddy_list.php?__a=1 HTTP/1.1" 403 1947 1832 "http://www.facebook.com/?sk=messages#!/" "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; GTB0.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; InfoPath.3; .NET4.0C; .NET4.0E)" "ch" 10 "-" "Genel" 0.052 "-" Neutral -

                if (line.StartsWith("#"))
                    return true;

                String[] arr = line.Split(new char[] { ' ' });
                Rec r = new Rec();
                if (line.Length > 891)
                {
                    r.Description = line.Substring(0, 890);
                    r.Description = r.Description.Replace("'", "|");
                }
                else
                {
                    r.Description = line;
                    r.Description = r.Description.Replace("'", "|");
                }

                r.LogName = LogName;
                r.Datetime = DateTime.Now.ToString();

                try
                {
                    if (arr.Length >= 6)
                    {
                        r.SourceName = arr[0];
                        r.ComputerName = arr[1];
                        r.UserName = arr[2].Trim('"');

                        string[] dateParts = arr[3].Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string date = dateParts[0].TrimStart('[') + " " + dateParts[1] + ":" + dateParts[2] + ":" + dateParts[3];
                        r.Datetime = Convert.ToDateTime(date.Trim().Trim(':').Trim(), CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");


                        string[] parts = line.Split(new char[] { '"' });

                        string[] url = parts[3].Split(' ');
                        r.EventType = url[0];
                        if (url[1].Length > 891)
                            r.CustomStr1 = url[1].Substring(0, 890);
                        else
                            r.CustomStr1 = url[1];
                        r.CustomStr3 = url[2];


                        string[] ints = parts[4].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        r.CustomInt1 = Convert_To_Int32(ints[0].Trim());
                        r.CustomInt2 = Convert_To_Int32(ints[1].Trim());
                        r.CustomInt3 = Convert_To_Int32(ints[2].Trim());

                        r.CustomStr2 = parts[7];
                        r.CustomStr10 = parts[5];

                        //if (parts.Length > 3)
                        //{
                        //    r.CustomStr3 = parts[3];
                        //}
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Line format is not like we want! Line: " + line);
                    }

                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                }
                SetRecordData(r);
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            //if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            //{
            //    ArrayList arrFileNameList = new ArrayList();

            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
            //    foreach (String file in Directory.GetFiles(Dir))
            //    {
            //        String filename = Path.GetFileName(file);

            //        if (filename.StartsWith("access") == true && filename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 2)//Name changed
            //        {
            //            arrFileNameList.Add(filename);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Dosya ismi okundu: " + filename);
            //        }
            //    }

            //    String[] dFileNameList = SortFiles(arrFileNameList);
            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

            //    if (!String.IsNullOrEmpty(lastFile))
            //    {
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile is not null: " + lastFile);
            //        bool bLastFileExist = false;

            //        for (int i = 0; i < dFileNameList.Length; i++)
            //        {
            //            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
            //            {
            //                bLastFileExist = true;
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile is found: " + lastFile);
            //                break;
            //            }
            //        }

            //        if (bLastFileExist)
            //        {
            //            if (Is_File_Finished(lastFile))
            //            {
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Last File is finished. Previous File: " + lastFile);

            //                for (int i = 0; i < dFileNameList.Length; i++)
            //                {
            //                    if (Dir + dFileNameList[i].ToString() == lastFile)
            //                    {

            //                        if (dFileNameList.Length > i + 1)
            //                        {
            //                            FileName = Dir + dFileNameList[i + 1].ToString();
            //                            Position = 0;
            //                            lastFile = FileName;
            //                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> New File is assigned. New File: " + FileName);
            //                            break;
            //                        }
            //                        else
            //                        {
            //                            FileName = lastFile;
            //                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> There is no new file to assign. Wait this file for log: " + FileName);
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                //Continue to read current file.
            //                FileName = lastFile;
            //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
            //            }
            //        }
            //        else
            //        {
            //            //Last file bulunamadı silinmiş olabilir. En yeni dosya atanacak.
            //            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
            //            Position = 0;
            //            lastFile = FileName;
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile Silinmis yada böyle bir dosya yok, Dosya Bulunamadý.  Yeni File : " + FileName);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Start to read newest file from beginning: " + FileName);
            //        }
            //    }
            //    else
            //    {
            //        //İlk defa log atanacak.
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Last File Is Null");
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> ilk defa log okunacak.");

            //        if (dFileNameList.Length > 0)
            //        {
            //            FileName = Dir + dFileNameList[0].ToString();
            //            lastFile = FileName;
            //            Position = 0;
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
            //        }
            //        else
            //        {
            //            Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() -->> In The Log Location There Is No Log File to read.");
            //        }
            //    }
            //}
            //else
            //{
            //    FileName = Dir;
            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocalM() -->> Directory file olarak gösterildi.: " + FileName);
            //}
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
                    String command = "ls -lt " + Dir + " | grep access_";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("access_") && arr[arr.Length - 1].Split('.').Length == 3) //Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i + 1].ToString()) == base.lastFile)
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
                                    if (Dir + dFileNameList[i + 1].ToString() == lastFile)
                                    {

                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = Dir + dFileNameList[i + 2].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Recorder durduruldu. ");
                                            base.Stop();
                                            Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Recorder Başlatıldı. ");
                                            base.Start();
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
                                //Continue to read current file.
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {
                            //Last file bulunamadı silinmiş olabilir. En yeni dosya atanacak.
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        //İlk defa log atanacak.

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[1].ToString();
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
                    Position = 0;
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
                        return false;
                    else
                        return true;
                }
                else
                {
                    FileInfo fi = new FileInfo(file);

                    if (fi.Length > Position)
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştirmeden devam edeceğiz.");
                return false;
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            ArrayList ar = new ArrayList();

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> arrFileNames : " + arrFileNames[i]);
                ar.Add(arrFileNames[i]);
            }
            ar.Sort();

            for (int i = 0; i < ar.Count; i++)
            {
                dFileNameList[i] = (string) ar[i];
            }

            //try
            //{
            //    for (int i = 0; i < arrFileNames.Count; i++)
            //    {
            //        string[] parts = arrFileNames[i].ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            //        if (parts.Length == 3 && arrFileNames.Contains("merged") && arrFileNames.Contains("denied"))
            //        {
            //            if (parts[0].StartsWith("access_"))
            //            {
            //                //if (parts[1] == "merged")
            //                {
            //                    for (int j = 0; j < parts.Length; j++)
            //                    {
            //                        Log.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> " + parts[i]);
            //                    }
            //                    dFileNumberList[i] = Convert.ToUInt64(parts[0].Remove(0, 13));
            //                    dFileNameList[i] = arrFileNames[i].ToString();
            //                }
            //            }
            //        }

            //    }

            //    Array.Sort(dFileNumberList, dFileNameList);

            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");

            //    for (int i = 0; i < dFileNameList.Length; i++)
            //    {
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            //}

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

        private int Convert_To_Int32(string strValue)
        {
            int intValue = 0;
            try
            {
                intValue = Convert.ToInt32(strValue);
                return intValue;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | base.Start() is called: " + FileName);

            //ParseFileNameLocal();

            base.ParseFileName();



            //if (remoteHost == "")
            //{
            //    String fileLast = FileName;
            //    Stop();
            //    ParseFileName();
            //    if (FileName != fileLast)
            //    {
            //        Position = 0;
            //        lastLine = "";
            //        lastFile = FileName;
            //        Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File changed, new file is, " + FileName);
            //    }
            //    base.Start();
            //}
            dayChangeTimer.Start();
        }


    }
}