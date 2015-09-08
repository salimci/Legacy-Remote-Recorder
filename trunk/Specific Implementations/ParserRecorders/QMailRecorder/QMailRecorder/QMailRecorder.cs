using System;
using Log;
using System.Collections;
using System.IO;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class QMailRecorder : Parser
    {
        public QMailRecorder()
            : base()
        {
            LogName = "QMail";
            //usingKeywords = false;
            //lineLimit = 50;
        }

        public QMailRecorder(String fileName)
            : base(fileName)
        {
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                string[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Rec r = new Rec();

                try
                {
                    //r.CustomStr1 = arr[1];
                    //r.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    r.CustomStr9 = arr[0];
                    try
                    {
                        r.Datetime = Convert_To_Date(arr[0]);
                    }
                    catch (Exception ex)
                    {
                        r.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    r.Description = line;
                    r.LogName = LogName;
                    r.CustomStr10 = Dir;

                    try
                    {
                        #region authlib OKEY

                        if (arr.Length < 6)
                        {
                            if (arr[1].StartsWith("INFO"))
                            {
                                if (arr[2].StartsWith("modules"))
                                {
                                    r.EventCategory = arr[2].Split('=')[0];
                                    r.CustomStr1 = arr[2].Split('=')[1].Trim(',').Trim('"');
                                    r.CustomInt1 = Convert_To_Int32(arr[3].Split('=')[1]);
                                }
                                else
                                    if (arr[2].StartsWith("Installing"))
                                    {
                                        r.EventCategory = arr[2];
                                        r.CustomStr1 = arr[3];
                                    }
                                    else
                                        if (arr[2].StartsWith("Installation"))
                                        {
                                            r.EventCategory = arr[2] + " " + arr[3].Trim(':');
                                            r.CustomStr1 = arr[4];
                                        }
                            }
                        }
                        if (arr[1].StartsWith("user"))
                        {
                            r.EventCategory = arr[1] + " " + arr[2];
                            r.UserName = "";
                            for (int i = 0; i < arr.Length; i++)
                            {
                                r.UserName += arr[i] + " ";
                            }
                            r.UserName = r.UserName.Trim();
                        }

                        #endregion

                        #region clamd OKEY

                        if (arr[1].Contains("/"))
                        {
                            r.CustomStr1 = arr[1].TrimEnd(':');
                            r.CustomStr2 = arr[2];
                        }
                        else if (arr[1].StartsWith("SelfCheck"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr1 = "";
                            for (int i = 0; i < arr.Length; i++)
                            {
                                r.CustomStr1 += arr[i].TrimEnd('.') + " ";
                            }
                            r.CustomStr1 = r.CustomStr1.Trim();
                        }

                        #endregion

                        #region imap4 OKEY

                        if (arr.Length >= 6)
                        {
                            if (arr[1].StartsWith("INFO"))
                            {
                                r.EventCategory = arr[1].Trim(':');
                                r.CustomStr1 = arr[2].Trim(',');
                                r.UserName = arr[3].Split('=')[1].Trim(',');
                                r.CustomStr3 = arr[4].Split('=')[1].Trim(',').TrimStart('[').TrimEnd(']');
                                if (arr[5].Contains("protocol"))
                                {
                                    r.CustomStr4 = arr[5].Split('=')[1].Trim(',');
                                }
                                else
                                {

                                    r.CustomInt1 = Convert_To_Int32(arr[5].Split('=')[1].Trim(','));
                                    r.CustomInt2 = Convert_To_Int32(arr[6].Split('=')[1].Trim(','));
                                    r.CustomInt3 = Convert_To_Int32(arr[7].Split('=')[1].Trim(','));
                                    r.CustomInt4 = Convert_To_Int32(arr[8].Split('=')[1].Trim(','));
                                    r.CustomInt5 = Convert_To_Int32(arr[9].Split('=')[1].Trim(','));
                                }
                            }
                        }

                        if (arr[1].StartsWith("DEBUG"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr1 = arr[2].Trim(',');

                            if (arr[3].StartsWith("ip"))
                            {
                                r.CustomStr3 = arr[3].Split('=')[1].TrimStart('[').TrimEnd(']');
                            }
                            if (arr[3].StartsWith("time"))
                            {
                                r.CustomInt3 = Convert_To_Int32(arr[3].Split('=')[1]);
                            }
                            else
                            {
                                for (int i = 3; i < arr.Length; i++)
                                    r.CustomStr1 += arr[i] + " ";

                                r.CustomStr1 = r.CustomStr1.Trim();
                            }
                        }

                        if (arr[1].StartsWith("tcpserver"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr1 = arr[2].Trim(':');
                            if (arr[2] == "pid" || arr[2] == "ok" || arr[2] == "end")
                            {
                                r.CustomInt1 = Convert_To_Int32(arr[3]);
                                if (arr[4] == "from")
                                {
                                    r.CustomStr3 = arr[5];
                                }
                                else if (arr.Length == 6)
                                {
                                    r.CustomStr1 = arr[4];

                                    string[] parts = arr[5].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2)
                                    {
                                        r.CustomStr3 = parts[0];
                                        r.CustomInt1 = Convert_To_Int32(parts[1]);
                                    }
                                }
                                else if (arr[4] == "status")
                                {
                                    r.CustomInt1 = Convert_To_Int32(arr[5]);
                                }
                            }
                            else if (arr[2].StartsWith("status"))
                            {
                                r.CustomStr2 = arr[3];

                                if (arr.Length > 4)
                                {
                                    r.CustomStr3 = arr[5];
                                }
                            }
                        }
                        #endregion

                        //#region imap4-ssl -->  OKEY (imep4)

                        //#endregion

                        //#region pop3  --> OKEY (imep4)

                        //#endregion

                        #region pop3-ssl --> OKEY (imep4)

                        if (arr[1].StartsWith("couriertls"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr1 = "";
                            for (int i = 0; i < arr.Length; i++)
                            {
                                r.CustomStr1 += arr[3] + " ";
                            }
                        }

                        #endregion

                        #region send OKEY

                        if (arr[1].StartsWith("delivery"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomInt2 = Convert_To_Int32(arr[2].Trim(':'));//delivery
                            r.CustomStr1 = arr[4];
                        }
                        else if (arr[1].StartsWith("status"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr2 = arr[3];
                            if (arr.Length > 4)
                            {
                                r.CustomStr2 = arr[5];
                            }
                        }
                        else if (arr[1].StartsWith("starting"))
                        {
                            r.EventCategory = arr[1] + " " + arr[2];
                            r.CustomInt2 = Convert_To_Int32(arr[3].Trim(':'));
                            r.CustomInt2 = Convert_To_Int32(arr[5]);
                            r.CustomStr1 = arr[8];
                        }
                        else if (arr[1].StartsWith("end") || arr[1].StartsWith("new") || arr[1].StartsWith("info"))
                        {
                            r.CustomInt1 = Convert_To_Int32(arr[3].Trim(':'));//msg

                            if (arr.Length >= 12)
                            {
                                r.CustomInt2 = Convert_To_Int32(arr[5].Trim(':'));
                                r.CustomStr1 = arr[7].TrimStart('<').TrimEnd('>');
                                r.CustomInt3 = Convert_To_Int32(arr[9].Trim(':'));
                                r.CustomInt4 = Convert_To_Int32(arr[11].Trim(':'));
                            }
                        }
                        #endregion

                        #region smtp OKEY

                        if (arr[1].StartsWith("rblsmtpd"))
                        {
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr3 = arr[2];
                            r.CustomInt1 = Convert_To_Int32(arr[4]);
                            r.CustomInt2 = Convert_To_Int32(arr[5]);
                            r.CustomStr1 = arr[6];

                            for (int i = 0; i < arr.Length; i++)
                            {
                                r.CustomStr1 += arr[3] + " ";
                            }
                        }
                        else if (arr[1].StartsWith("CHKUSER"))
                        {

                            //@400000004ccc70a30b5515bc CHKUSER accepted sender: from <newinfotr@adv.strawberrynet.com::> 
                            //remote <smtp156.strawberrynet.com:unknown:202.181.189.53> rcpt <> : sender accepted
                            r.EventCategory = arr[1].Trim(':');
                            r.CustomStr1 = arr[2] + arr[3];

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i] == "from")
                                {
                                    r.CustomStr3 = arr[i + 1].TrimStart('<').TrimEnd('>').TrimEnd(':').TrimEnd(':');
                                }
                                else if (arr[i] == "remote")
                                {
                                    string[] parts = arr[i + 1].TrimStart('<').TrimEnd('>').Split(':');

                                    r.CustomStr1 = parts[0];
                                    r.CustomStr2 = parts[2];
                                }
                                else if (arr[i] == "rcpt")
                                {
                                    r.CustomStr4 = arr[i + 1].TrimStart('<').TrimEnd('>');

                                    r.CustomStr5 = "";
                                    for (int j = i + 2; j < arr.Length; j++)
                                    {
                                        r.CustomStr5 += arr[j] + " ";
                                    }
                                    r.CustomStr5 = r.CustomStr5.TrimStart(':').Trim();
                                }
                            }
                        }
                        else if (arr[1].StartsWith("policy_check"))
                        {

                            r.EventCategory = arr[1].Trim(':');
                            if (arr[1].Contains("remote"))
                            {
                                r.CustomStr4 = arr[3].TrimStart('<').TrimEnd('>');
                                r.CustomStr3 = arr[6].TrimStart('<').TrimEnd('>');

                                r.CustomStr2 = "";
                                for (int i = 7; i < arr.Length; i++)
                                {
                                    r.CustomStr2 += arr[i] + " ";
                                }
                                r.CustomStr2 = r.CustomStr2.Trim();
                            }
                            else
                            {
                                r.CustomStr2 = "";
                                for (int i = 7; i < arr.Length; i++)
                                {
                                    r.CustomStr2 += arr[i] + " ";
                                }
                                r.CustomStr2 = r.CustomStr2.Trim();
                            }
                        }

                        #endregion

                        #region spamd

                        #endregion

                        #region submission

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " İçeride hata ile karşılaşıldı:  " + ex.Message);
                        Log.Log(LogType.FILE, LogLevel.ERROR, " Hatalı Line:  " + line);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Finish Record Data");
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

                    if (filename.StartsWith("access") == true && filename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 2)//Name changed
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
                    String command = "ls -lt " + Dir + " | grep @";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr[arr.Length - 1].StartsWith("@"))//Name changed
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

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    dFileNumberList[i] = Convert.ToUInt64(9999999 - i);
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

        public string Convert_To_Date(string hexValue)
        {
            string secValue = (ulong.Parse(hexValue.Substring(9, 8), System.Globalization.NumberStyles.HexNumber)).ToString();
            string miliValue = (ulong.Parse(hexValue.Substring(17, 8), System.Globalization.NumberStyles.HexNumber)).ToString();

            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(Convert.ToDouble(secValue));
            //dt = dt.AddMilliseconds(Convert.ToDouble(miliValue)); 
            dt = dt.AddMilliseconds(Convert.ToDouble(miliValue.Substring(0, 7)));

            string Date = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return Date;
        }

        private int Convert_To_Int32(string value)
        {
            int intv = 0;
            try
            {
                intv = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                return 0;
            }
            return intv;
        }

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    dayChangeTimer.Close();
        //}

    }
}