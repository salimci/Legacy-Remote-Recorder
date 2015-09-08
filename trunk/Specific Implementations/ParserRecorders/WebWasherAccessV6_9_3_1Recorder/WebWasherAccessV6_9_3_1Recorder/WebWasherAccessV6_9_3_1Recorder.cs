//


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
    public class WebWasherAccessV6_9_3_1Recorder : Parser
    {

        public WebWasherAccessV6_9_3_1Recorder()
            : base()
        {
            LogName = "WebwasherAccessRecorder";
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
                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // 192.168.20.62 - 192.168.20.62 -      -        - [04/Jan/2011:15:42:13 +0200] - "GET http://ff.1click.weather.com/weather/local/TUXX0002?cc=*%26unit=m HTTP/1.1"           - 407 - 151  - ""                                                                                                                                                                                                                           - "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 (.NET CLR 3.5.30729)"
                // 192.168.20.4   - 192.168.20.4  -      -        - [05/Jan/2011:13:34:42 +0200] - "GET http://i3.microsoft.com/global/downloads/en/RenderingAssets/Silverlight.js HTTP/1.1"  - 407 - 151  - "http://www.microsoft.com/downloads/en/confirmation.aspx?FamilyID=230ecdfb-89ec-4d4a-8a85-89fd98329f7b&DisplayLang=EN&hash=C5vhW1uXplrZJ7yJz%2bwxjJfLYw16pn6a7DqS4JlEH7nhMo32DEbyhSkpGeFNBPsIE%2bTDGnfpsR27V6y9DjFJVg%3d%3d" - "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; WOW64; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)"
                //192.168.20.146  - cfcu-lt-005   - EUCFCU\shanci - [29/Dec/2010:11:42:33 +0200] - "GET http://www.hurriyet.com.tr/images/HurriyetKiyasla/mortgage-ikon.gif.jpg HTTP/1.1"     - 0   - 1886 - "http://www.hurriyet.com.tr/anasayfa/"                                                                                                                                                                                      - "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; InfoPath.3)"

                //Yeni format
                //192.168.20.232 192.168.2.5 "EUCFCU\bmkose" [22/Feb/2011:10:11:30 +0200] "GET http://www.mfib.gov.tr/imgs/logo.jpg HTTP/1.1" 0 1339 1069 "http://www.mfib.gov.tr/" "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; GTB6.5; .NET CLR 1.0.3705; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E; InfoPath.3)" "gv" 0 "-" "Genel" 0.773 "-" Trusted -
                //192.168.20.232 192.168.2.5 "EUCFCU\bmkose" [22/Feb/2011:10:11:30 +0200] "GET http://www.mfib.gov.tr/imgs/logo.jpg HTTP/1.1" 200 814 7500 "http://www.mfib.gov.tr/" "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; GTB6.5; .NET CLR 1.0.3705; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E; InfoPath.3)" "gv" - "image/jpeg" "Genel" 0.006 "-" Trusted TCP_HIT
                //192.168.20.232 - "-" [22/Feb/2011:10:11:30 +0200] "GET http://www.mfib.gov.tr/imgs/logo.jpg HTTP/1.1" 407 562 151 "http://www.mfib.gov.tr/" "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; GTB6.5; .NET CLR 1.0.3705; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E; InfoPath.3)" "-" - "-" "-" 0.000 "-" Neutral -
                //192.168.20.201 [no host IP available] "EUCFCU\feramuzy" [15/Mar/2011:14:32:46 +0200] "CONNECT cfcu.gov.tr:443 HTTP/1.0" 0 930 765 "" "Microsoft Office/14.0 (Windows NT 5.1; Microsoft Outlook 14.0.5128; Pro)" "ed" 0 "-" "Genel" 0.007 "-" Neutral -

                if (line.StartsWith("#"))
                    return true;

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
                        if (arr[1].Contains("["))
                        {
                            r.ComputerName = " ";
                            r.UserName = arr[5].Trim('"');

                            string[] dateParts = arr[6].Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            string date = dateParts[0].TrimStart('[') + " " + dateParts[1] + ":" + dateParts[2] + ":" + dateParts[3];
                            r.Datetime = Convert.ToDateTime(date.TrimStart('[').TrimEnd(']').Trim(), CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

                        }
                        else
                        {
                            r.ComputerName = arr[1];
                            r.UserName = arr[2].Trim('"');
                            //[29/Dec/2010:11:42:33 +0200] 

                            string[] dateParts = arr[3].Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            string date = dateParts[0].TrimStart('[') + " " + dateParts[1] + ":" + dateParts[2] + ":" + dateParts[3];
                            r.Datetime = Convert.ToDateTime(date.TrimStart('[').TrimEnd(']').Trim(), CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                        }
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

            // PART 0 DENİED İÇİN 23 
            // ACCESS İÇİN 16

            try
            {//
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Home Directory | " + Dir);
                    String command = "ls -lt " + Dir + " | grep access";
                    Log.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (arr[arr.Length - 1].Split('.')[0].Length == 16)
                        if (!arr[arr.Length - 1].Split('.')[0].Contains("denied"))
                        {
                            if (arr[arr.Length - 1].StartsWith("access") == true &&
                                arr[arr.Length - 1].Split('.').Length == 3) //Name changed                        
                            {
                                arrFileNameList.Add(arr[arr.Length - 1]);
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                        "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                            }
                        }
                    }
                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> dFileNameList[1] : " + dFileNameList[1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->>base.Dir : " + base.Dir);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->>base.lastFile : " + base.lastFile);
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
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Old Position: " + Position);
                                            FileName = Dir + dFileNameList[i + 2].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New Position: " + Position);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Recorder durduruldu. ");
                                            base.Stop();
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Recorder Başlatıldı. ");
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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> bLastFileExist : false ");
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
                    lastFile = FileName;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);

                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
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
                    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    BinaryReader br = new BinaryReader(fs, enc);

                    br.BaseStream.Seek(Position, SeekOrigin.Begin);

                    Log.Log(LogType.FILE, LogLevel.INFORM, " Is_File_Finished() | Position is: " + br.BaseStream.Position.ToString());
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Is_File_Finished() | Length is: " + br.BaseStream.Length.ToString());

                    #region dosyanın sonunu bulamayabilir.
                    FileInfo finfo = new FileInfo(lastFile);
                    Int64 fileLength = finfo.Length;
                    Char c = ' ';
                    while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  LastLineCheck In Is_File_Finished() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                        c = br.ReadChar();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  LastLineCheck In Is_File_Finished() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                        if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  LastLineCheck In Is_File_Finished() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  LastLineCheck In Is_File_Finished() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                        }
                    }
                    #endregion

                    if (br.BaseStream.Position == br.BaseStream.Length - 2 || br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                    {
                        fs.Close();
                        br.Close();
                        return true;
                    }
                    else
                    {
                        fs.Close();
                        br.Close();
                        return false;
                    }

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
                dFileNameList[i] = (string)ar[i];
            }

            //try
            //{
            //    for (int i = 0; i < arrFileNames.Count; i++)
            //    {
            //        string[] parts = arrFileNames[i].ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            //        if (parts.Length == 3 && arrFileNames.Contains("merged") && !arrFileNames.Contains("denied"))
            //        {
            //            if (parts[0].StartsWith("access"))
            //            {
            //                //if (parts[0].Length == 16)
            //                {
            //                    //if (parts[1] == "merged")
            //                    {
            //                        for (int j = 0; j < parts.Length; j++)
            //                        {
            //                            Log.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> " + parts[i]);
            //                        }
            //                        dFileNumberList[i] = Convert.ToUInt64(parts[0].Remove(0, 6));
            //                        dFileNameList[i] = arrFileNames[i].ToString();
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    Array.Sort(dFileNumberList, dFileNameList);
            //    // Array.Sort(dFileNameList);
            //    Log.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> Sıranmış dosya isimleri yazılıyor");
            //    for (int i = 0; i < dFileNameList.Length; i++)
            //    {
            //        Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
            //    }
            //    //Log.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> Sıralanan Dosya Adedi : " + dFileNameList.Length);
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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | base.Start() is called: " + FileName);

            //ParseFileNameLocal();

            //base.ParseFileName();

            String fileLast = FileName;
            Stop();
            ParseFileName();
            if (FileName != fileLast)
            {
                Position = 0;
                lastLine = "";
                lastFile = FileName;
                Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File changed, new file is, " + FileName);
            }
            base.Start();

            dayChangeTimer.Start();
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

    }
}
