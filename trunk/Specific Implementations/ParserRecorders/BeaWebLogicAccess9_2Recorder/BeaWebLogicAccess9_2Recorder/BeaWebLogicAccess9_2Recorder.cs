//Name: Bea Web Logic Access Recorder 9.2
//Writer: Ali Yıldırım
//Date: 15.12.2010

using System;
using System.Text;
using System.Timers;
using System.IO;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class BeaWebLogicAccess9_2Recorder : Parser
    {
        public BeaWebLogicAccess9_2Recorder()
            : base()
        {
            LogName = "BeaWebLogicAccess9_2Recorder";
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
                String[] arr = SpaceSplit(line, false, '"');

                try
                {
                    Rec r = new Rec();

                    if (arr.Length >= 7)
                    {
                        arr[3] = arr[3].TrimStart('[');

                        StringBuilder date = new StringBuilder();
                        bool first = false;
                        foreach (Char c in arr[3])
                        {
                            if (!first && c == ':')
                            {
                                first = true;
                                date.Append(' ');
                            }
                            else
                                date.Append(c);
                        }

                        DateTime dt = DateTime.Parse(date.ToString());
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.SourceName = arr[0];

                        arr[5] = arr[5].TrimStart('"').TrimEnd('"');
                        String[] eventArr = arr[5].Split(' ');
                        r.EventCategory = eventArr[0];
                        r.CustomStr3 = eventArr[1].Trim();
                        r.CustomStr4 = eventArr[2].Trim();
                        r.CustomStr1 = arr[6];
                        r.CustomStr2 = arr[7];

                        r.Description = line;
                        r.LogName = LogName;

                        SetRecordData(r);
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Line format is not like we want! Line: " + line);
                        return true;
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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // New Added
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();

                if (tempCustomVar1.Length <= 0)
                {
                    tempCustomVar1 = string.Empty;
                }


                if (FileName != fileLast && FileName != tempCustomVar1)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "File changed, new file is, " + FileName);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "File Name can not equal to, " + tempCustomVar1);
                }
                base.Start();
            }
            dayChangeTimer.Start();

            // End new Added

            //dayChangeTimer.Close();
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
                    string[] fileParts = filename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);


                    if (filename.StartsWith("access") && fileParts.Length == 2 && fileParts[1].Length > 3)//Name changed

                        // && filename != "access.log"
                        if (filename.StartsWith("access") == true && filename != tempCustomVar1 && filename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 2)//Name changed
                        {
                            arrFileNameList.Add(filename);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> File name read : " + filename);
                        }
                }

                String[] dFileNameList = SortFiles(arrFileNameList);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> Array List filenames sorted.");

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
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> LastFile Silinmis yada böyle bir dosya yok, Dosya Bulunamadı.  Yeni File : " + FileName);
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
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() -->> FileName ve LastFile en eski dosya olarak ayarlandı: " + lastFile);
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
                    String command = "ls -lt " + Dir + " | grep access";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string file = arr[arr.Length - 1];
                        string[] fileParts = file.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        if (fileParts.Length == 2 && fileParts[1].Trim().Length > 3)//Name changed
                        {

                            arrFileNameList.Add(file);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + file);

                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Filename readed: " + arr[arr.Length - 1]);

                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Filenames in arrayFileNameList are sorted.");

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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile was deleted , file nto found.  New File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        //İlk defa log atanacak.

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is NULL");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> The first log read operation.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName and LastFile were set with oldiest file : " + lastFile);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is no logfile to read.");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory name is set like a filename (not end with \\ or /): " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> An error occured, during filename operation.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Error Message : " + ex.ToString());
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
                    #region endOfFile

                    //bool fileIsFinished = false;
                    // FileStream fs = null;
                    //  BinaryReader br = null;
                    // Int64 positionOfParser = 0;
                    // Int64 lengthOfFile = 26;
                    // try
                    // {
                    //     fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    // }
                    // catch (Exception exc)
                    // {
                    //     Log.Log(LogType.FILE, LogLevel.WARN, "File deleted or rotated..");
                    //     Log.Log(LogType.FILE, LogLevel.WARN, "Last file not found, looking for next file");
                    //     Log.Log(LogType.FILE, LogLevel.ERROR, exc.Message);
                    //     lastFile = "";
                    //     Position = 0;
                    // }

                    // br = new BinaryReader(fs, enc);
                    // br.BaseStream.Seek(Position, SeekOrigin.Begin);

                    // #region yeni eklendi
                    // FileInfo finfo = new FileInfo(file);
                    // Int64 fileLength = finfo.Length;
                    // Char c = ' ';
                    // while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                    // {
                    //     Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                    //     c = br.ReadChar();
                    //     Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);



                    //     if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                    //     {
                    //         Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                    //         Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                    //     }
                    // }
                    // #endregion

                    // positionOfParser = br.BaseStream.Position;
                    // br.Close();
                    // fs.Close();
                    // FileInfo fi = new FileInfo(file);
                    // lengthOfFile = fi.Length;
                    // Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is: " + positionOfParser);
                    // Log.Log(LogType.FILE, LogLevel.DEBUG, "Length is: " + lengthOfFile);

                    // if (positionOfParser > lengthOfFile - 2 || positionOfParser > lengthOfFile - 1 || positionOfParser == lengthOfFile)
                    //     fileIsFinished = true;
                    // else
                    // {
                    //     Log.Log(LogType.FILE, LogLevel.INFORM, "Not at the end of file, parsing continues");
                    //     fileIsFinished = false;
                    // }
                    #endregion
                    FileInfo fi = new FileInfo(file);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Checking File Length (" + fi.Length + ") and position (" + Position + ") ");
                    if (fi.Length == Position || fi.Length - 1 == Position)
                        //if (fi.Length > Position)
                        if (fi.Length > Position - 2 || fi.Length > Position - 1 || fi.Length == Position)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> File finished, Return true");
                            return true;
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> File not finished, Return false");

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
            return true;
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

                    dFileNumberList[i] = Convert.ToUInt64(parts[1].Remove(0, 3));
                    dFileNameList[i] = arrFileNames[i].ToString();
                }

                Array.Sort(dFileNumberList, dFileNameList);

                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Printing Sorted Filenames.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Error : " + ex.ToString());
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
    }
}
