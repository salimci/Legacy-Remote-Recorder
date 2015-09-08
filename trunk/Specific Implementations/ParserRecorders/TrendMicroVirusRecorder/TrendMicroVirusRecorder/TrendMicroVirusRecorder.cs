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
    public class TrendMicroVirusRecorder : Parser
    {
        data args = null;
        //private bool firstIn = true;

        public TrendMicroVirusRecorder()
            : base()
        {
            LogName = "TrendMicroVirusRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
            args = new data();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            string value = "";
            bool SendRecord = false;

            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr.Length == 2)
                    value = arr[1].Trim();
                else
                    value = " ";

                try
                {
                    if (arr[0] == "Date")
                    {
                        try
                        {
                            string date = "";
                            date = arr[1] + ":" + arr[2] + ":" + arr[3];
                            args.Date = Convert.ToDateTime(date, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, " Date converting exception. Line : " + line);
                            Log.Log(LogType.FILE, LogLevel.ERROR, " Exception Msg : " + ex.ToString());
                            args.Date = ""; //DateTime.Now.ToString();
                        }
                    }
                    else if (arr[0] == "Method")
                    {
                        args.Method = value;
                    }
                    else if (arr[0] == "From")
                    {
                        args.From = value;
                    }
                    else if (arr[0] == "To")
                    {
                        args.To = value;
                    }
                    else if (arr[0] == "File")
                    {
                        args.File = value;
                    }
                    else if (arr[0] == "Action")
                    {
                        args.Action = value;
                    }
                    else if (arr[0] == "Virus")
                    {
                        args.Virus = value;
                    }
                    else if (arr[0] == "User_id")
                    {
                        args.User_id = value;
                        SendRecord = true;
                    }

                    if (SendRecord)
                    {
                        Rec r = new Rec();
                        r.LogName = LogName;

                        r.Datetime = args.Date;
                        r.SourceName = args.User_id;
                        r.CustomStr1 = args.From;
                        r.CustomStr2 = args.File;
                        r.CustomStr3 = args.Action;
                        r.CustomStr4 = args.Virus;
                        r.CustomStr6 = args.Method;
                        
                        if (!string.IsNullOrEmpty(args.Date))
                            SetRecordData(r);

                        args = null;
                        args = new data();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Log format finished. Line: " + line);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Log format did not finish.! Line: " + line);
                    }
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
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                ArrayList arrFileNameList = new ArrayList();

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    String filename = Path.GetFileName(file);

                    if (filename.StartsWith("virus") == true)
                    {
                        arrFileNameList.Add(filename);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Dosya ismi okundu: " + filename);
                    }
                }

                String[] dFileNameList = SortFiles(arrFileNameList);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                if (!String.IsNullOrEmpty(lastFile))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile is not null: " + lastFile);
                    bool bLastFileExist = false;

                    for (int i = 0; i < dFileNameList.Length; i++)
                    {
                        if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                        {
                            bLastFileExist = true;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile is found: " + lastFile);
                            break;
                        }
                    }

                    if (bLastFileExist)
                    {
                        if (Is_File_Finished(lastFile))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Last File is finished. Previous File: " + lastFile);

                            for (int i = 0; i < dFileNameList.Length; i++)
                            {
                                if (Dir + dFileNameList[i].ToString() == lastFile)
                                {

                                    if (dFileNameList.Length > i + 1)
                                    {
                                        FileName = Dir + dFileNameList[i + 1].ToString();
                                        Position = 0;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> New File is assigned. New File: " + FileName);
                                        break;
                                    }
                                    else
                                    {
                                        FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Continue to read current file.
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                        }
                    }
                    else
                    {
                        //Last file bulunamadı silinmiş olabilir. En yeni dosya atanacak.
                        FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                        Position = 0;
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile Silinmis yada böyle bir dosya yok, Dosya Bulunamadý.  Yeni File : " + FileName);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Start to read newest file from beginning: " + FileName);
                    }
                }
                else
                {
                    //İlk defa log atanacak.
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Last File Is Null");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> ilk defa log okunacak.");

                    if (dFileNameList.Length > 0)
                    {
                        FileName = Dir + dFileNameList[0].ToString();
                        lastFile = FileName;
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameLocal() -->> In The Log Location There Is No Log File to read.");
                    }
                }
            }
            else
            {
                FileName = Dir;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocalM() -->> Directory file olarak gösterildi.: " + FileName);
            }
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
                    String command = "ls -lt " + Dir + " | grep virus";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);
                    if(se.Connected == false)
                        se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("virus") == true && arr[arr.Length - 1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 5)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
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
                            bool fileFin = false;
                            //if (firstIn == false)
                            //{
                            //    firstIn = true;
                            //    fileFin = false;
                            //}
                            //else
                            //{
                                fileFin = Is_File_Finished(lastFile);
                            //}
                            if (fileFin)
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

        //TODO:sed okumada okumaya başlanacak position 1 geri alınır(Position - 1). Sonra lineCount > 2 ise okumaya devam edilir. 
        //Hiçbirşey dönmez ise hata var demektir ve dosya değiştirilmez. 22 Nisan - ali
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

                        if(se.Connected == false)
                            se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);

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

                        if(se.Connected == false)
                            se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                   

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                        stReader = new StringReader(stdOut);

                        while ((line = stReader.ReadLine()) != null)
                        {
                            lineCount++;
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Kalan satır sayısı : " + lineCount);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunan son satır : " + line);
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
            
            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                    //virus.log.2010.12.30
                    string[] arr = arrFileNames[i].ToString().Split('.');
                    if (arr.Length == 5)
                    {
                        string numberStr = arr[2] + arr[3] + arr[4];
                        Int64 numberInt64 = Convert_To_Int64(numberStr);
                        
                        dFileNumberList[i] = Convert.ToUInt64(numberInt64);
                        dFileNameList[i] = arrFileNames[i].ToString();
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

        private Int64 Convert_To_Int64(string strValue)
        {
            Int64 intValue = 0;
            try
            {
                intValue = Convert.ToInt64(strValue);
                return intValue;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Entered. ");
            if (dayChangeTimer.Interval != 600000)
            {
                dayChangeTimer.Interval = 600000;
                Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Interval changed. " + dayChangeTimer.Interval);
            }
            else
            {
                dayChangeTimer.Stop();
                if (remoteHost == "")
                {
                    String fileLast = FileName;
                    Stop();
                    //ParseFileName();
                    this.ParseFileNameRemote();
                    if (FileName != fileLast)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Filename changed. ");

                        Position = 0;
                        lastLine = "";
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "File changed, new file is, " + FileName);
                    }
                    base.Start();
                }
            }
            dayChangeTimer.Start();
        }
    }

    class data
    {
        public string Date;
        public string Method;
        public string From;
        public string To;
        public string File;
        public string Action;
        public string Virus;
        public string User_id;
    }
}