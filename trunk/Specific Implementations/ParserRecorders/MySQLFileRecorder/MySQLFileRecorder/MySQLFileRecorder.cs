/*
 Writer Onur Sarıkaya
 */
using System;
using System.Globalization;
using System.IO;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class MySQLFileRecorder : Parser
    {
        private string dateFormat = "yyyy/MM/dd HH:mm:ss";
        private string myDateTimeString;

        public MySQLFileRecorder()
            : base()
        {
            LogName = "MySQLFileRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public MySQLFileRecorder(String fileName)
            : base(fileName)
        {

        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is " + line);

            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }
            Rec rec = new Rec();

            if (!dontSend)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] arr1 = SpaceSplit(line, true);
                    #region Description
                    try
                    {
                        if (line.Length > 3999)
                        {
                            rec.Description = line.Substring(0, 3999);
                        }
                        else
                        {
                            rec.Description = line;
                        }
                        rec.Description = line;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + rec.Description);
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Description Error" + exception.Message);
                    }
                    #endregion

                    for (int i = 0; i < arr1.Length; i++)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Arr[" + i + " .] : " + arr1[i]);
                    }

                    #region DateTime                    

                    if (!line.StartsWith("InnoDB"))
                    {
                        rec.CustomStr1 = arr1[0] + " " + arr1[1];

                        string[] s = rec.CustomStr1.Split(' ');
                        string year = s[0].Substring(0, 2);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "year : " + year);
                        string month = s[0].Substring(2, 2);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "month : " + month);
                        string day = s[0].Substring(4, 2);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "day : " + day);
                        string time = s[1];
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "time : " + time);
                        DateTime dt;
                        myDateTimeString = day + "-" + month + "-20" + year + " " + time;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "myDateTimeString : " + myDateTimeString);
                        rec.CustomStr2 = myDateTimeString;
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "myDateTimeString : " + myDateTimeString);
                        rec.CustomStr2 = myDateTimeString;
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "dateFormat : " + dateFormat);
                    rec.Datetime = DateTime.Now.ToString(dateFormat); 
                    #endregion

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record will be sent.");
                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
                }
            }
            return true;
        } // ParseSpecific

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After


        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

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
                    String command = "ls -lt " + Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    try
                    {
                        se.Connect();
                        se.RunCommand(command, ref stdOut, ref stdErr);
                        se.Close();
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Exception : " + exception);
                    }

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();
                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (arr[arr.Length - 1].StartsWith("audit")  && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        if (arr[arr.Length - 1].StartsWith("mysqld"))//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }
                    //audit.log.2012-10-08
                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i]) == base.lastFile)
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
                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
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
        } // ParseFileNameRemote

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
        } // Is_File_Finished

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
        } // SortFiles

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
        } // GetFiles
    }
}


