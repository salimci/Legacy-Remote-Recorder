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
    public class HotspotRecorder : Parser
    {

        public HotspotRecorder()
            : base()
        {
            LogName = "HotspotRecorder";
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

                //Jun  6 08:51:15 clog logger: 10.10.1.36 - - [06/Jun/2011:08:51:15 +0300] "GET http://img5.mynet.com/ha6/yazi/pkk-mhp-miting.jpg HTTP/1.1" - - "http://www.mynet.com/" "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"
                //Jun  6 08:51:15 clog logger: 10.10.1.36 - - [06/Jun/2011:08:51:15 +0300] "GET http://img5.mynet.com/ha6/yazi/kocaoglu.jpg HTTP/1.1" - - "http://www.mynet.com/" "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"
                //Jun  6 08:51:15 clog logger: 10.10.1.36 - - [06/Jun/2011:08:51:15 +0300] "GET http://img5.mynet.com/ha6/yazi/sevismee.jpg HTTP/1.1" - - "http://www.mynet.com/" "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"

                //Jun  9 08:06:40 clog logger: 10.10.1.12 - - [09/Jun/2011:08:06:40 +0300] "GET http://www.facebook.com/extern/login_status.php?api_key=106932686001126&app_id=106932686001126&channel_url=http%3A%2F%2Fstatic.ak.fbcdn.net%2Fconnect%2Fxd_proxy.php%3Fversion%3D3%23cb%3Df3b6e2acc33461c%26origin%3Dhttp%253A%252F%252Fapps.oyun.mynet.com%252Ff10856232571bbc%26relation%3Dparent.parent%26transport%3Dpostmessage&display=hidden&extern=0&locale=tr_TR&next=http%3A%2F%2Fstatic.ak.fbcdn.net%2Fconnect%2Fxd_proxy.php%3Fversion%3D3%23cb%3Df20c9a12291d718%26origin%3Dhttp%253A%252F%252Fapps.oyun.mynet.com%252Ff10856232571bbc%26relation%3Dparent%26transport%3Dpostmessage%26frame%3Df18f296882bc716%26result%3D%2522xxRESULTTOKENxx%2522&no_session=http%3A%2F%2Fstatic.ak.fbcdn.net%2Fconnect%2Fxd_proxy.php%3Fversion%3D3%23cb%3Df1c0702d1cb7a86%26origin%3Dhttp%253A%252F%252Fapps.oyun.mynet.com%252Ff10856232571bbc%26relation%3Dparent%26transport%3Dpostmessage%26frame%3Df18f296882bc716&no_user=http%3A%2F%2Fstatic.ak.fbcdn.net%2Fconnect%2Fxd_proxy.php%3Fversion%3D3%23cb%
                //Jun  9 11:16:42 clog coova-chilli[1212]: chilli.c: 4251: Successful UAM login from username=ikarakas IP=10.10.1.25
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

                try
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Kategori :" + arr[4]);

                    if (arr[4].Contains("logger"))
                    {
                        if (arr.Length > 10)
                        {
                            r.LogName = LogName;
                            r.CustomStr3 = arr[5];

                            string[] dpart = arr[8].TrimStart('[').Split(':');
                            r.Datetime = Convert.ToDateTime(dpart[0] + " " + dpart[1] + ":" + dpart[2] + ":" + dpart[3], CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

                            r.EventType = arr[10].TrimStart('"');

                            if (arr.Length > 15)
                            {
                                if (arr[11].Length > 890)
                                    r.CustomStr2 = arr[11].Substring(0, 890);
                                else
                                    r.CustomStr2 = arr[11];

                                r.CustomStr6 = arr[12].TrimEnd('"');
                                r.CustomStr4 = arr[13];
                                r.CustomStr5 = arr[14];
                                r.CustomStr1 = arr[15].Trim('"');

                                string[] arr2 = line.Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                                r.CustomStr7 = arr2[arr2.Length - 1];
                            }
                            else if (arr.Length == 11)
                            {
                                if (arr[11].Length > 890)
                                    r.CustomStr2 = arr[11].Substring(0, 890);
                                else
                                    r.CustomStr2 = arr[11];
                            }
                        }
                    }

                    if (arr[4].Contains("coova") && !line.Contains("Permission"))
                    { 

                    //Jun  9 11:16:42 clog coova-chilli[1212]: chilli.c: 4251: Successful UAM login from username=ikarakas IP=10.10.1.25
                    //Jun  6 08:20:32 clog coova-chilli[1237]: chilli.c: 3721: New DHCP request from MAC=00-21-C5-11-20-FB
                    //Jun  6 08:20:32 clog coova-chilli[1237]: chilli.c: 3676: Client MAC=00-21-C5-11-20-FB assigned IP 10.10.1.34
                    //Jun  6 08:31:17 clog coova-chilli[1237]: chilli.c: 3887: DHCP Released MAC=00-22-6B-DB-14-07 IP=10.10.1.33


                        string[] dpart = arr[8].TrimStart('[').Split(':');
                        r.Datetime = Convert.ToDateTime(arr[1] + "/" + arr[0] + "/2011 " + arr[2], CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Coova Line : " + line);
                       
                        r.EventCategory = "coova-chilli";

                        if (arr[7].Equals("New"))
                            r.EventType = "DHCP";
                        else
                            r.EventType = arr[7];

                        r.CustomStr2 = "";
                        for (int i = 7; i < arr.Length; i++)
                        {
                            if (arr[i].Contains("username") || arr[i].Contains("MAC"))
                                break;

                            r.CustomStr2 += arr[i] + " ";
                        }
                        r.CustomStr2 = r.CustomStr2.Trim();

                        for (int i = 7; i < arr.Length; i++)
                        {
                            if (arr[i].Contains("username="))
                            {
                                r.UserName = arr[i].Split('=')[1];
                            }
                            else if (arr[i].Contains("IP="))
                            {
                                r.CustomStr3 = arr[i].Split('=')[1];
                            }
                            else if (arr[i].Contains("MAC="))
                            {
                                r.CustomStr4 = arr[i].Split('=')[1];
                            }
                            else if (arr[i].Equals("IP"))
                            {
                                r.CustomStr3 = arr[i + 1];
                            }
                        }
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
                    String command = "ls -lt " + Dir + " | grep natek";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string filename = arr[arr.Length - 1];

                        if (filename == "syslog")
                        {
                            arrFileNameList.Add(filename);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + filename);
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
                    lastFile = FileName;
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
            //<customvar1>_YYYYMMDD.log 
            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    int number;
                    if (parts.Length == 3)
                    {
                        number = Convert_To_Int32(parts[1]);
                        dFileNumberList[i] = Convert.ToUInt64(number);
                        dFileNameList[i] = arrFileNames[i].ToString();
                    }
                    else
                    {
                        dFileNameList[i] = null;
                    }
                }

                Array.Sort(dFileNumberList, dFileNameList);

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

            if (remoteHost == "")
            {
                ParseFileNameLocal();
            }
            else
            {
                ParseFileNameRemote();
            }

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