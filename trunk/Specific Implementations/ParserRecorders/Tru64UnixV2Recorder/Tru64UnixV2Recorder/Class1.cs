/*Developed by Onur Sarıkaya
 * Date 27.006.2012 
 * 
 * Bu recorder Tru64UnixRecorder ile tamamen aynıdır. Daha öncesinde log, belli bir formattaki subfolder'ın altında olacak şekilde ayarlanmış 
 * makine değiştirilmiş dolayısıyla path ve folder sistemi de değişmiş. Bu sebepten ötürü mevcut dll'de değişiklik yapılmadan sadece 
 * belirtilen folder'daki log ları okuyacak şekilde yeniden düzenlenip Tru64UnixV2Recorder adıyla yeni dll oluşturulmuştur. 
 * Ayrıca date kolonunda yıl 2011 olacak şekilde kodun içine yazılmış olup logun okunduğu yılı alacak şekilde dinamik olarak değiştirilmiştir.
 * 
 * Bu işlem EXIM bank için Şahan Savaşan tarafından istenmiş olup Fatih Sakar tarafından test edilip onaylanmıştır.
 * 
 * 
 */



using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public struct Fields
    {
        public string ConnectionId,
                      EVENTCATEGORY,
                      EVENTTYPE,
                      DATETIME,
                      CUSTOMSTR1,
                      CUSTOMSTR3,
                      CUSTOMSTR4,
                      CUSTOMSTR5,
                      CUSTOMSTR6,
                      CUSTOMSTR7,
                      DESCRIPTION,
                      COMPUTER_Name;

        public string FullLine;

        public int CUSTOMINT1;
        public bool IsInsertData;
        public bool LineEnd;
    }

    public class Tru64UnixV2Recorder : Parser
    {

        private Fields RecordFields;

        public Tru64UnixV2Recorder()
            : base()
        {
            LogName = "Tru64UnixV2Recorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public Tru64UnixV2Recorder(String fileName)
            : base(fileName)
        {

        }

        public override void Init()
        {
            RecordFields = new Fields();
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, " parts[1] : " + parts[1] + " parts[0] : " + parts[0] + " parts[2] : " + parts[2]);
                            int dt = DateTime.Now.Year;
                            string date = parts[1] + "/" + parts[0] + "/" + dt.ToString() + " " + parts[2];
                            Log.Log(LogType.FILE, LogLevel.INFORM, " date " + date);
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
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() --> File name is null or there is no such file name. readingFileName : " + readingFileName);
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
        } // ParseSpecific

        public void ClearRecordFields()
        {
            RecordFields.EVENTTYPE = "";
            RecordFields.EVENTCATEGORY = "";
            RecordFields.COMPUTER_Name = "";
            RecordFields.CUSTOMSTR3 = "";
            RecordFields.CUSTOMSTR4 = "";
            RecordFields.CUSTOMSTR5 = "";
            RecordFields.CUSTOMSTR6 = "";
            RecordFields.CUSTOMSTR7 = "";
            RecordFields.LineEnd = false;
        }

        //protected override void ParseFileNameRemote()
        //{
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
        //            String command = "ls -lt " + Dir + " | grep radius_";
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

        //            se.Connect();
        //            se.RunCommand(command, ref stdOut, ref stdErr);
        //            se.Close();

        //            StringReader sr = new StringReader(stdOut);
        //            ArrayList arrFileNameList = new ArrayList();
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //                if (arr[arr.Length - 1].StartsWith("radius_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
        //                {
        //                    arrFileNameList.Add(arr[arr.Length - 1]);
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
        //                }
        //            }

        //            String[] dFileNameList = SortFiles(arrFileNameList);
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

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
        //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
        //                    Position = 0;
        //                    lastFile = FileName;
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
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
        //}

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnRemote();
                    fileNameList = SortFileNames(fileNameList);
                    //SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> An eror occurred : " + ex.ToString());
            }
        } // ParseFileNameRemote

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is STARTED ");

                String line = "";
                String stdOut = "";
                String stdErr = "";

                String command = "ls -lt " + Dir;//FileNames contains what.*** fileNameFilter
                Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> SSH command : " + command);

                se.Connect();
                se.RunCommand(command, ref stdOut, ref stdErr);
                se.Close();

                StringReader stringReader = new StringReader(stdOut);
                List<String> fileNameList = new List<String>();
                Boolean foundAnyFile = false;

                while ((line = stringReader.ReadLine()) != null)
                {
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameList.Add(arr[arr.Length - 1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> File name is read: " + arr[arr.Length - 1]);
                    foundAnyFile = true;
                }

                if (!foundAnyFile)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFileNamesOnRemote() -->> There is no proper file in directory");
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnRemote() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnRemote

        private List<String> SortFileNames(List<String> fileNameList)
        {
            foreach (string t in fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() " + t);
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            return fileNameList;
        } // SortFileNames

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
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        dFileNumberList[i] = Convert.ToUInt64(parts[1]);
                        dFileNameList[i] = arrFileNames[i].ToString();
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            }

            return dFileNameList;
        }

        public override void GetFiles()
        {
            Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Starting GetFiles.");
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

