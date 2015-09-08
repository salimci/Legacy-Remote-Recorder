//Writer: Onur Sarıkaya 

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public struct Fields
    {
        public Int64 tempPosition;
        public string fileName;
    }

    public class DNS_FileRecorder : Parser
    {
        private Fields RecordFields;

        public DNS_FileRecorder()
            : base()
        {
            LogName = "DNS_FileRecorder Recorder";
            RecordFields = new Fields();
            usingKeywords = false;
            lineLimit = 50;
        }

        public DNS_FileRecorder(String fileName)
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

            if (!dontSend)
            {
                Rec r = new Rec();

                if (line.Length > 899)
                {
                    r.Description = line.Substring(0, 899);
                }
                else
                {
                    r.Description = line;
                }

                string[] spaceItems = line.Split(' ');

                #region Date
                int year = DateTime.Now.Year;
                string myDateString = spaceItems[1] + " " + spaceItems[0] + " " + year + " " + spaceItems[2];
                DateTime dt = Convert.ToDateTime(myDateString);
                r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                r.LogName = LogName;
                #endregion

                for (int i = 0; i < spaceItems.Length; i++)
                {
                    //Tip1
                    if (spaceItems[i].ToUpper().ToString() == "DNS")
                    {
                        try
                        {
                            r.SourceName = spaceItems[3];
                            r.CustomStr1 = spaceItems[11];

                            if (spaceItems.Length > 14)
                            {
                                if (spaceItems[14].Contains("#"))
                                {
                                    try
                                    {
                                        r.CustomStr4 = spaceItems[14].Split('#')[0].Trim();
                                        r.CustomInt4 = Convert.ToInt32(spaceItems[14].Split('#')[1].Replace(':', ' ').Trim());
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 1 Line format is invalid." + line);
                                    }
                                }
                            }

                            r.CustomStr3 = spaceItems[9].Split('#')[0];
                            r.CustomInt3 = Convert.ToInt32(spaceItems[9].Split('#')[1]);

                            r.EventCategory = spaceItems[5] + " " + spaceItems[6] + " " + spaceItems[7];
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 1: " + exception.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 1 line : " + line);
                        }
                    }
                }

                for (int i = 0; i < spaceItems.Length; i++)
                {
                    //Tip2
                    if (spaceItems[i].ToUpper().ToString() == "(FORMERR)")
                    {
                        try
                        {
                            r.SourceName = spaceItems[3];
                            r.EventCategory = Between(line, "]:", "(");
                            r.EventType = Between(line, "(", ")");
                            r.CustomStr1 = Between(line, "\'", "\'");
                            r.CustomStr4 = (After(line, "\':")).Split('#')[0].Trim();
                            r.CustomInt4 = Convert.ToInt32((After(line, "\':")).Split('#')[1]);
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 2: " + exception.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 2 line : " + line);
                        }
                    }
                }

                for (int i = 0; i < spaceItems.Length; i++)
                {
                    //Tip3
                    if (spaceItems[i].ToLower().ToString() == "lame")
                    {
                        try
                        {
                            r.SourceName = spaceItems[3];
                            r.EventCategory = spaceItems[5] + " " + spaceItems[6] + " " + spaceItems[7];
                            r.CustomStr1 = Between(line, "\'", "\' ");

                            r.CustomStr4 = After(line, "):").Split('#')[0].Trim();
                            r.CustomInt4 = Convert.ToInt32(After(line, "):").Split('#')[1]);
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 3: " + exception.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 3 line : " + line);
                        }
                    }
                }

                for (int i = 0; i < spaceItems.Length; i++)
                {
                    //Tip4
                    if (spaceItems[i].ToLower().ToString() == "success")
                    {
                        try
                        {
                            r.SourceName = spaceItems[3];
                            r.EventCategory = spaceItems[5] + " " + spaceItems[6];
                            r.CustomStr1 = spaceItems[7];
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 4: " + exception.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Tip 4 line : " + line);
                        }
                    }
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Position: " + Position);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "tempPosition: " + RecordFields.tempPosition);

                long tempPosition = GetLinuxFileSizeControl(RecordFields.fileName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "tempPosition: " + RecordFields.tempPosition);

                if (Position > tempPosition)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position büyük  dosya dan büyük pozisyon sıfırlanacak." );
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position = 0 ");
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Data sending.");
                SetRecordData(r);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Send Data");
            }
            return true;
        }

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

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
        //            String command = "ls -lt " + Dir;
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

        //            try
        //            {
        //                se.Connect();
        //                se.RunCommand(command, ref stdOut, ref stdErr);
        //                se.Close();
        //            }
        //            catch (Exception exception)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Exception : " + exception);
        //            }

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
        //} // ParseFileNameRemote

        protected override void ParseFileNameRemote()
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");

                try
                {
                    se = new SshExec(remoteHost, user);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> antin kuntin.......");
                    se.Password = password;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> user: " + user);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> password: " + password);
                }
                catch (Exception exception12)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> connect time out." + exception12.Message.ToString());
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dir." + Dir);
                //IConnector connector = ConnectionManager.getConnector("SSH");
                //connector.SetConfigData(Log);
                //connector.Port = 22;
                //connector.Init();

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
                    String command = "ls -lt " + Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    try
                    {
                        Thread.Sleep(2000);
                        se.Connect();
                        se.RunCommand(command, ref stdOut, ref stdErr);
                        se.Close();

                        //if (connector.initConnection(remoteHost, user, password, "", 0))
                        //{
                        //    connector.runCommand(command);
                        //    stdOut = connector.read();
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
                        //    connector.dropConnection();
                        //}
                        //else
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
                        //}//
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
                        if (arr[arr.Length - 1].StartsWith("radius_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> File Sizeing. ");

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
                    RecordFields.fileName = FileName;
                    //GetLinuxFileSizeControl(FileName);
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> File Size: " + GetLinuxFileSizeControl(FileName)); 
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


            //IConnector connector = ConnectionManager.getConnector("SSH");
            //connector.SetConfigData(Log);
            //connector.Port = 22;
            //connector.Init();

            try
            {
                if (readMethod == "nread")
                {
                    commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();

                    //if (connector.initConnection(remoteHost, user, password, "", 0))
                    //{
                    //    connector.runCommand(commandRead);
                    //    stdOut = connector.read();
                    //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
                    //    connector.dropConnection();
                    //}
                    //else
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
                    //}

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                    stReader = new StringReader(stdOut);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsýna bakýlýyor.");
                    //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor. 
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

                    //if (connector.initConnection(remoteHost, user, password, "", 0))
                    //{
                    //    connector.runCommand(commandRead);
                    //    stdOut = connector.read();
                    //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
                    //    connector.dropConnection();
                    //}
                    //else
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
                    //}

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

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in DNS_FileRecorderRecorder -->> Enter the Function.");
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in DNS_FileRecorderRecorder -->> ParseFileName() method should be trigerred. ");
        //    ParseFileName();
        //}

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

        private long GetLinuxFileSizeControl(string fileName)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetLinuxFileSizeControl() -->> Is started. ");

            try
            {
                string stdOut = "";
                string stdErr = "";
                String wcArg = "";
                String wcCmd = "";

                wcCmd = "wc";
                wcArg = "-l";
                String commandRead;
                String lineCommand = wcCmd + " " + wcArg + " " + FileName;
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + lineCommand);
                if (!se.Connected)
                {
                    se.Connect();
                }
                se.RunCommand(lineCommand, ref stdOut, ref stdErr);
                se.Close();
                String[] arr = SpaceSplit(stdOut, false);
                RecordFields.tempPosition = Convert.ToInt64(arr[0]);
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + RecordFields.tempPosition.ToString());
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetLinuxFileSizeControl() -->>  : " + exception);
            }
            return RecordFields.tempPosition;
        } // GetLinuxFileSizeControl
    }
}
