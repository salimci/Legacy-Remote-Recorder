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
    public class TrendMicroAccessRecorder : Parser
    {
        data args = null;

        public TrendMicroAccessRecorder()
            : base()
        {
            LogName = "TrendMicroAccessRecorder";
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

            try
            {
                if (!dontSend)
                {
                    String[] arr = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length >= 2)
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
                                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> Date converting exception. Line : " + line);
                                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->>  Exception Msg : " + ex.ToString());
                               
                                args.Date = "";
                            }
                        }
                        else if (arr[0] == "Method")
                        {
                            args.Method = value;
                        }
                        else if (arr[0] == "Server")
                        {
                            args.Server = value;
                        }
                        else if (arr[0] == "User")
                        {
                            args.User = value;
                        }
                        else if (arr[0] == "ClientIP")
                        {
                            args.ClientIP = value;
                        }
                        else if (arr[0] == "ServerIP")
                        {
                            args.ServerIP = value;
                        }
                        else if (arr[0] == "Domain")
                        {
                            args.Domain = value;
                        }
                        else if (arr[0] == "Content-Type")
                        {
                            args.ContentType = value;
                        }
                        else if (arr[0] == "Content-Length")
                        {
                            args.ContentLength = value;
                        }
                        else if (arr[0] == "Path")
                        {
                            args.Path = value;
                        }
                        else if (arr[0] == "Operation")
                        {
                            args.Operation = value;
                        }
                        else if (arr[0] == "Category")
                        {
                            args.Category = value;
                        }
                        else if (arr[0] == "CategoryType")
                        {
                            args.CategoryType = value;
                            SendRecord = true;
                        }

                        if (SendRecord)
                        {
                            if (!string.IsNullOrEmpty(args.Date))
                            {
                                Rec r = new Rec();

                                r.LogName = LogName;
                                r.Datetime = args.Date;
                                r.SourceName = args.User;
                                r.CustomStr1 = args.Domain;
                                r.CustomStr2 = args.Path;
                                r.CustomStr3 = args.ClientIP;
                                r.CustomStr4 = args.ServerIP;
                                r.CustomStr5 = args.Server;
                                r.CustomStr6 = args.Method;
                                r.CustomStr7 = args.ContentType;
                                r.CustomStr8 = args.Operation;
                                r.CustomInt1 = Convert_To_Int32(args.Category);
                                r.CustomInt2 = Convert_To_Int32(args.CategoryType);
                                r.CustomInt3 = Convert_To_Int32(args.ContentLength);
                                r.Description = args.Date + " | " + args.User + " | " + args.Domain + " | " + args.Path + " | " + args.ClientIP + " | " + args.ServerIP + " | " + args.Operation;

                                SetRecordData(r);

                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Log format finished. Line: " + line);
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> Log format finished. But because of the date conversion. Rec is not sent. Line: " + line);
                            }

                            SendRecord = false;
                            args = null;
                            args = new data();
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> Log format did not finished.! Line: " + line);
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
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() -->> En dışta Hata : " + ex.ToString()); 
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
                    if (filename.StartsWith("access"))//Name changed
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Home Directory | " + Dir);
                    String command = "ls -lt " + Dir + " | grep access";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> SSH command : " + command);
                    
                    //There may be connection problem.
                    if(se.Connected == false)
                        se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    //se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("access") == true && arr[arr.Length - 1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 6)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFile is found: " + lastFile);
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            if (Is_File_Finished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = Dir + dFileNameList[i + 1].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                        }
                                     }
                                }
                            }
                            else
                            {
                                //Continue to read current file.
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {   //22 mart değişti. .. ali
                            //Last file bulunamadı silinmiş olabilir. En yeni dosya atanacak.
                            //FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            //Position = 0;
                            //lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        //İlk defa log atanacak.

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Last File Is Null");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> ilk defa log okunacak.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
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

                        if (se.Connected == false)
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

                        if (se.Connected == false)
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

                    //access.log.2010.12.30.0001
                    string[] arr = arrFileNames[i].ToString().Split('.');
                    if (arr.Length == 6)
                    {
                        string numberStr = arr[2] + arr[3] + arr[4] + arr[5];
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
                if (remoteHost != "")
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

        private bool Is_File_Finished_Yeni(string file)
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
    }

    class data
    {
        public string Date;
        public string Method;
        public string Server;
        public string User;
        public string ClientIP;
        public string ServerIP;
        public string Domain;
        public string ContentType;
        public string ContentLength;
        public string Path;
        public string Operation;
        public string Category;
        public string CategoryType;
    }
}