//Name: Oracle DB Recorder
//Writer: Ali Yıldırım
//Date: 18/02/2011

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
    public class Tru64unixRecorder : Parser
    {

        public Tru64unixRecorder()
            : base()
        {
            LogName = "Tru64unixRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            if (line == "")
                return true;

            if (!dontSend)
            {
                try
                {
                    string readingFileName = "";
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        string[] lastFileParts = lastFile.Split(new char[] { '/', '\\', '.' }, StringSplitOptions.RemoveEmptyEntries);
                        readingFileName = lastFileParts[lastFileParts.Length - 2];
                    }

                    String[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    Rec rec = new Rec();
                    rec.Description = line;
                    rec.LogName = LogName;
                    rec.EventCategory = readingFileName;
                    rec.Datetime = DateTime.Now.ToString();

                    try
                    {
                        //Feb  8 08:32:07 toprak syslog: Oracle Cluster Ready Services starting up automatically.
                        //Deamon
                        //Apr 19 21:06:09 bulut /usr/sbin/collect[1287816]: Forcing data buffer flush


                        if (parts.Length >= 5)
                        {
                            string date = parts[1] + "/" + parts[0] + "/2011 " + parts[2];
                            rec.Datetime = Convert.ToDateTime(date, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                            rec.ComputerName = parts[3];

                            //auth,daemon,kern,lpr,mail,syslog,user

                            switch (readingFileName)
                            {
                                case "auth":
                                    {
                                        rec.CustomStr2 = parts[4].Split('[')[0];
                                        rec.CustomStr3 = parts[4].Split('[')[1].TrimEnd(':').TrimEnd(']');

                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr1 = allLeftStr.Trim();
                                    }
                                    break;

                                case "daemon":
                                    {
                                        rec.CustomStr4 = parts[4].Split('[')[0];
                                        rec.CustomStr5 = parts[4].Split('[')[1].TrimEnd(':').TrimEnd(']');

                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr2 = allLeftStr.Trim();
                                    }
                                    break;

                                case "kern":
                                    {
                                        rec.CustomStr4 = parts[4].TrimEnd(':');
                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr2 = allLeftStr.Trim();
                                    }
                                    break;

                                case "lpr":

                                    break;

                                case "mail":
                                    {
                                        rec.CustomStr1 = parts[4].Split('[')[0];
                                        rec.CustomStr2 = parts[4].Split('[')[1].TrimEnd(':').TrimEnd(']');

                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr3 = allLeftStr.Trim();
                                    }
                                    break;

                                case "syslog":
                                    {
                                        rec.CustomStr1 = parts[4].TrimEnd(':');

                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr2 = allLeftStr.Trim();
                                    }
                                    break;

                                case "user":
                                    {
                                        rec.CustomStr1 = parts[4].TrimEnd(':');

                                        string allLeftStr = "";
                                        for (int i = 5; i < parts.Length; i++)
                                        {
                                            allLeftStr += parts[i] + " ";
                                        }
                                        rec.CustomStr2 = allLeftStr.Trim();
                                    }
                                    break;

                                default:
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() --> File name is null or there is no such file name. readingFileName : " + readingFileName);
                                    }
                                    break;
                            }

                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() --> Line format is not like we want! Line: " + line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " Inner Hata : " + ex.Message);
                        Log.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() --> Line : " + line);
                        return true;
                    }

                    SetRecordData(rec);
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " Outher Hata : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() --> Line : " + line);

                }
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {

        }

        protected override void ParseFileNameRemote()
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";
            string DirWithSub = "";
            bool bLastFolderExist = false;
            bool bLastFileExist = false;
            String[] dFolderNameList;
            string[] filenames;
            string[] sortedFileNames;

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    ///var/cluster/members/member1/adm/syslog.dated/08-Feb-08:31/
                    //Find sub folder.
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Home Directory | " + Dir);

                    String command = "ls -lt " + Dir; //String command = "ls -lt " + Dir + " | grep access";
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> SSH command : " + command);
                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();
                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFolderNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //08-Feb-08:31
                        if (arr[arr.Length - 1].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Length == 3)//Name changed
                        {
                            arrFolderNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Klasör ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }

                    dFolderNameList = SortFolders(arrFolderNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> arrFolderNameList'e atılan klasör isimleri sıralandı.");

                    if (dFolderNameList.Length > 0)
                    {
                        if (!String.IsNullOrEmpty(lastFile))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFile is not null: " + lastFile);

                            string[] lastFileparts = lastFile.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            string lastfolder = lastFileparts[lastFileparts.Length - 2];

                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFolder name : " + lastfolder);

                            for (int i = 0; i < dFolderNameList.Length; i++)
                            {
                                if ((dFolderNameList[i].ToString()) == lastfolder)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFolder is found: " + lastFile);
                                    bLastFolderExist = true;
                                    DirWithSub = Dir + dFolderNameList[i].ToString() + "/";
                                    break;
                                }
                            }

                            if (bLastFolderExist)
                            {
                                filenames = Get_Files_From_Subdir(DirWithSub);

                                if (filenames.Length > 0)
                                {
                                    sortedFileNames = SortFiles(filenames);

                                    for (int i = 0; i < sortedFileNames.Length; i++)
                                    {
                                        if (lastFile == DirWithSub + sortedFileNames[i])
                                        {
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFile is found: " + lastFile);
                                            bLastFileExist = true;
                                            break;
                                        }
                                    }

                                    if (bLastFileExist)
                                    {
                                        if (Is_File_Finished(lastFile))
                                        {
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile + " Position : " + Position);

                                            for (int i = 0; i < sortedFileNames.Length; i++)
                                            {
                                                if (DirWithSub + sortedFileNames[i].ToString() == lastFile)
                                                {
                                                    if (sortedFileNames.Length > i + 1)
                                                    {
                                                        FileName = DirWithSub + sortedFileNames[i + 1].ToString();
                                                        Position = 0;
                                                        lastFile = FileName;
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                                    }
                                                    else
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no new file to assign. If there is new folder we will change folder.");

                                                        //lastFileparts = lastFile.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                                        //string lastfolder = lastFileparts[lastFileparts.Length - 2];

                                                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> LastFolder name : " + lastfolder);

                                                        for (int j = 0; j < dFolderNameList.Length; j++)
                                                        {
                                                            if ((dFolderNameList[j].ToString()) == lastfolder)
                                                            {
                                                                if (dFolderNameList.Length > j + 2)
                                                                {
                                                                    DirWithSub = Dir + dFolderNameList[j + 1].ToString() + "/";
                                                                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> New Folder is assigned: " + DirWithSub);

                                                                    filenames = Get_Files_From_Subdir(DirWithSub);

                                                                    if (filenames.Length > 0)
                                                                    {
                                                                        sortedFileNames = SortFiles(filenames);

                                                                        FileName = DirWithSub + sortedFileNames[0].ToString();
                                                                        Position = 0;
                                                                        lastFile = FileName;
                                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                                                    }
                                                                    else
                                                                    {
                                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no filr in new folder.");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no new folder to assign. We will wait on this folder.");
                                                                }

                                                                break;
                                                            }
                                                        }


                                                        FileName = lastFile;
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> File did not finished continue to read. FileName : " + FileName);
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Last File could not found in last directory. LastFile : " + lastFile);
                                    }
                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no file in subdirectory. DirWithSub : " + DirWithSub);
                                }
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Last Folder could not found in main directory. Lastfolder : " + lastfolder);

                                DirWithSub = Dir + dFolderNameList[0].ToString() + "/";
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> New folder is assined: " + DirWithSub);
                                filenames = Get_Files_From_Subdir(DirWithSub);
                                if (filenames.Length > 0)
                                {
                                    sortedFileNames = SortFiles(filenames);
                                    FileName = DirWithSub + sortedFileNames[0].ToString();
                                    Position = 0;
                                    lastFile = FileName;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> New file is assigned. FileName : " + FileName);

                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no file in New folder!! ");
                                }
                            }
                        }
                        else
                        {
                            //İlk defa log okunacak. En eski klasör atanacak.
                            DirWithSub = Dir + dFolderNameList[0] + "/";
                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Lastfile empty. Oldest directory will be assign. DirWithSub : " + DirWithSub);

                            filenames = Get_Files_From_Subdir(DirWithSub);

                            if (filenames.Length > 0)
                            {
                                sortedFileNames = SortFiles(filenames);
                                FileName = DirWithSub + sortedFileNames[0];
                                lastFile = FileName;
                                Position = 0;

                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> First file of oldest direcort is assiged : Filename: " + lastFile + " | Position: " + Position);
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> There is no file in subdirectory. DirWithSub : " + DirWithSub);
                            }
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> There is no subfolder that we want!!!");
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> Lütfen doğru formatta bir path giriniz.");
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

        private string[] Get_Files_From_Subdir(string DirWithSub)
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";
            string[] filenamelist = null;
            ArrayList arrFileNameList = new ArrayList();

            try
            {
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (DirWithSub.EndsWith("/") || DirWithSub.EndsWith("\\"))
                {

                    ///var/cluster/members/member1/adm/syslog.dated/08-Feb-08:31/
                    //Find sub folder.

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Get_Files_From_Subdir() -->> Home Directory | " + DirWithSub);
                    String command = "ls -lt " + DirWithSub; //String command = "ls -lt " + Dir + " | grep access";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Get_Files_From_Subdir() -->> SSH command : " + command);
                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " Get_Files_From_Subdir() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);

                        //auth,daemon,kern,lpr,mail,syslog,user
                        if (arr[arr.Length - 1].Contains("."))
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " Get_Files_From_Subdir() -->> Error on getting file of SubDir: " + DirWithSub);
                Log.Log(LogType.FILE, LogLevel.ERROR, " Get_Files_From_Subdir() -->> " + ex.ToString());
            }

            filenamelist = new string[arrFileNameList.Count];

            for (int i = 0; i < filenamelist.Length; i++)
            {
                filenamelist[i] = arrFileNameList[i].ToString();
            }

            return filenamelist;
        }

        private string[] SortFolders(ArrayList arrFolderNames)
        {
            UInt64[] dFolderNumberList = new UInt64[arrFolderNames.Count];
            String[] dFolderNameList = new String[arrFolderNames.Count];

            try
            {
                for (int i = 0; i < arrFolderNames.Count; i++)
                {
                    //08-Feb-08:31
                    string[] parts = arrFolderNames[i].ToString().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime date = Convert.ToDateTime(parts[0] + "/" + parts[1] + "/" + DateTime.Now.Year.ToString() + " " + parts[2] + ":00", CultureInfo.InvariantCulture);

                    string dayStr = "";
                    string ayStr = "";
                    string saatStr = "";

                    if (date.Month < 10)
                        ayStr = "0" + date.Month;
                    else
                        ayStr = date.Month.ToString();

                    if (date.Day < 10)
                        dayStr = "0" + date.Day;
                    else
                        dayStr = date.Day.ToString();

                    if (date.Hour < 10)
                        saatStr = "0" + date.Hour;
                    else
                        saatStr = date.Hour.ToString();

                    dFolderNumberList[i] = Convert.ToUInt64(date.Year + ayStr + dayStr + saatStr); //ayStr + dayStr
                    dFolderNameList[i] = arrFolderNames[i].ToString();
                }

                Array.Sort(dFolderNumberList, dFolderNameList);

                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFolders() -->> Sýralanmýþ Subfolder isimleri yazýlýyor.");

                for (int i = 0; i < dFolderNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFolders() -->> " + dFolderNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SortFolders() -->> Error on sorting folders. Mesaj: " + ex.ToString());
            }
            return dFolderNameList;
        }

        private string[] SortFiles(string[] arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Length];
            String[] dFileNameList = new String[arrFileNames.Length];

            //auth,daemon,kern,lpr,mail,syslog,user

            try
            {
                for (int i = 0; i < arrFileNames.Length; i++)
                {
                    string s = "";
                    for (int j = 0; j < arrFileNames[i].ToString().Length; j++)
                    {
                        s += Convert.ToString(Convert.ToInt32(arrFileNames[i].ToString()[j]));
                    }

                    s = s.Substring(0, 8);

                    dFileNumberList[i] = Convert.ToUInt64(s);
                    dFileNameList[i] = arrFileNames[i].ToString();
                }

                Array.Sort(dFileNumberList, dFileNameList);

                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");

                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
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

            if (dayChangeTimer.Interval < 120000)
            {
                dayChangeTimer.Interval = 120000;
            }
            else
            {
                String fileLast = FileName;
                Stop();

                if (remoteHost == "")
                {
                    this.ParseFileNameLocal();
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | base.dayChangeTimer_Elapsed fonksiyonu ParseFileNameLocal fonksiyonunu çağırdı. " + FileName);
                }
                else
                {
                    this.ParseFileNameRemote();
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | base.dayChangeTimer_Elapsed fonksiyonu ParseFileNameRemote fonksiyonunu çağırdı. " + FileName);
                }

                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File changed, new file is, " + FileName);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File did not changed, same file to read.");
                }
                base.Start();

            }

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