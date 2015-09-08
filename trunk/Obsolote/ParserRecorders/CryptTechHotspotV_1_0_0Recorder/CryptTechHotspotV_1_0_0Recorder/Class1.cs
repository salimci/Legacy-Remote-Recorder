////Düzenleme: Onur Sarıkaya 27.12.2012

//using System;
//using System.Globalization;
//using System.IO;
//using System.Threading;
//using Log;
//using CustomTools;
//using NatekInfra.Connection;
//using SharpSSH.SharpSsh;
//using System.Collections;

//namespace Parser
//{
//    public struct Fields
//    {
//        public Int64 tempPosition;
//        public string fileName;
//        public long lineNumber;
//    }

//    public class CryptTechHotspotV_1_0_0Recorder : Parser
//    {
//        private Fields RecordFields;
//        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

//        public CryptTechHotspotV_1_0_0Recorder()
//            : base()
//        {
//            LogName = "CryptTechHotspotV_1_0_0Recorder";
//            RecordFields = new Fields();
//            usingKeywords = false;
//            lineLimit = 50;
//        }

//        public CryptTechHotspotV_1_0_0Recorder(String fileName)
//            : base(fileName)
//        {

//        }

//        public override void Init()
//        {
//            GetFiles();
//        }


//        public override bool ParseSpecific(String line, bool dontSend)
//        {
//            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
//            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

//            if (Position != 0)
//            {
//                RecordFields.lineNumber++;
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | lineNumber : " + RecordFields.lineNumber);
//            }
//            else if (Position == 0)
//            {
//                RecordFields.lineNumber = 0;
//            }

//            if (line == "")
//                return true;

//            String[] lineArr = SpaceSplit(line, false);

//            try
//            {
//                Rec r = new Rec();
//                r.LogName = LogName;

//                if (line.Length > 899)
//                {
//                    r.Description = line.Substring(0, 899);
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
//                }
//                else
//                {
//                    r.Description = line;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
//                }


//                DateTime dt;
//                string myDateTimeString = lineArr[1] + lineArr[0] + "," + DateTime.Now.Year + "," + lineArr[2];
//                dt = Convert.ToDateTime(myDateTimeString);
//                r.Datetime = dt.ToString(dateFormat);

//                if (lineArr[4] == "logger:" && ValidIp(lineArr[5]))
//                {

//                    if (lineArr.Length > 10)
//                    {
//                        r.EventType = lineArr[10].Replace('"', ' ').Trim();
//                    }

//                    if (lineArr.Length > 11)
//                    {
//                        r.CustomStr1 = lineArr[11].Replace('"', ' ').Trim();
//                    }
//                    if (lineArr.Length > 15)
//                    {
//                        r.CustomStr2 = lineArr[15].Replace('"', ' ').Trim();
//                    }

//                    if (lineArr.Length > 12)
//                    {
//                        r.CustomStr3 = lineArr[12].Replace('"', ' ').Trim();
//                    }

//                    if (lineArr.Length > 5)
//                    {
//                        r.ComputerName = lineArr[5].Replace('"', ' ').Trim();
//                    }

//                    String[] lineArr2 = line.Split('"');

//                    if (lineArr2.Length > 5)
//                    {
//                        r.CustomStr4 = lineArr2[5].Replace('"', ' ').Trim();
//                    }


//                }
//                else
//                {
//                    string[] lineUnusual = line.Split('"');
//                    string[] UnusualStr3 = lineUnusual[0].Split(' ');
//                    r.CustomStr1 = lineUnusual[2];
//                    r.CustomStr3 = UnusualStr3[UnusualStr3.Length - 1];
//                    r.CustomStr4 = lineUnusual[4];
//                }

//                r.CustomStr9 = FileName;

//                if (!string.IsNullOrEmpty(r.EventType))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType" + r.EventType);
//                }

//                if (!string.IsNullOrEmpty(r.ComputerName))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName" + r.ComputerName);
//                }

//                if (!string.IsNullOrEmpty(r.CustomStr1))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1" + r.CustomStr1);
//                }

//                if (!string.IsNullOrEmpty(r.CustomStr2))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2" + r.CustomStr2);
//                }

//                if (!string.IsNullOrEmpty(r.CustomStr3))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3" + r.CustomStr3);
//                }

//                if (!string.IsNullOrEmpty(r.CustomStr4))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4" + r.CustomStr4);
//                }

//                if (!string.IsNullOrEmpty(r.CustomStr9))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9" + r.CustomStr9);
//                }

//                long tempPosition = GetLinuxFileSizeControl(RecordFields.fileName);
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "tempPosition: " + RecordFields.tempPosition);

//                if (Position > tempPosition)
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position büyük  dosya dan büyük pozisyon sıfırlanacak.");
//                    Position = 0;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position = 0 ");
//                }

//                Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
//                SetRecordData(r);
//                Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");

//            }
//            catch (Exception e)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
//                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
//                return true;
//            }

//            return true;
//        } // ParseSpecific

//        /// <summary>
//        /// String olarak gönderilen Ip bilgisinin Geçerli bir Ipv4 olup olmadığını bool olarak döndürür.
//        /// </summary>
//        /// <param name="ip">
//        /// String IpV4</param>
//        /// <returns></returns>
//        private bool ValidIp(string ip)
//        {
//            string[] arrIP = ip.Split('.');
//            if (arrIP.Length != 4)
//            {
//                return false;
//            }
//            byte a = 0;
//            foreach (string item in arrIP)
//            {
//                if (!byte.TryParse(item, out a))
//                {
//                    return false;
//                }
//            }
//            return true;
//        } // ValidIp

//        /// <summary>
//        /// Get string value before b.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="a"></param>
//        /// <returns></returns>
//        public static string Before(string value, string a)
//        {
//            int posA = value.IndexOf(a);
//            if (posA == -1)
//            {
//                return "";
//            }
//            return value.Substring(0, posA);
//        } // Before

//        /// <summary>
//        /// Get string value after [last] a.
//        /// </summary>
//        public static string After(string value, string a)
//        {
//            int posA = value.LastIndexOf(a);
//            if (posA == -1)
//            {
//                return "";
//            }
//            int adjustedPosA = posA + a.Length;
//            if (adjustedPosA >= value.Length)
//            {
//                return "";
//            }
//            return value.Substring(adjustedPosA);
//        } // After

//        /// <summary>
//        /// string between function
//        /// </summary>
//        /// <param name="value"></param>
//        /// gelen tüm string
//        /// <param name="a"></param>
//        /// başlangıç string
//        /// <param name="b"></param>
//        /// bitiş string
//        /// <returns></returns>
//        public static string Between(string value, string a, string b)
//        {
//            int posA = value.IndexOf(a);
//            int posB = value.LastIndexOf(b);

//            if (posA == -1)
//            {
//                return "";
//            }
//            if (posB == -1)
//            {
//                return "";
//            }
//            int adjustedPosA = posA + a.Length;
//            if (adjustedPosA >= posB)
//            {
//                return "";
//            }
//            return value.Substring(adjustedPosA, posB - adjustedPosA);
//        } // Between

//        //protected override void ParseFileNameRemote()
//        //{
//        //    string line = "";
//        //    String stdOut = "";
//        //    String stdErr = "";

//        //    try
//        //    {
//        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
//        //        se = new SshExec(remoteHost, user);
//        //        se.Password = password;
//        //        if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
//        //        {
//        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
//        //            String command = "ls -lt " + Dir;
//        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

//        //            try
//        //            {
//        //                se.Connect();
//        //                se.RunCommand(command, ref stdOut, ref stdErr);
//        //                se.Close();
//        //            }
//        //            catch (Exception exception)
//        //            {
//        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Exception : " + exception);
//        //            }

//        //            StringReader sr = new StringReader(stdOut);
//        //            ArrayList arrFileNameList = new ArrayList();
//        //            while ((line = sr.ReadLine()) != null)
//        //            {
//        //                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//        //                if (arr[arr.Length - 1].StartsWith("radius_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
//        //                {
//        //                    arrFileNameList.Add(arr[arr.Length - 1]);
//        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
//        //                }
//        //            }

//        //            String[] dFileNameList = SortFiles(arrFileNameList);
//        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

//        //            if (!String.IsNullOrEmpty(lastFile))
//        //            {
//        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
//        //                bool bLastFileExist = false;

//        //                for (int i = 0; i < dFileNameList.Length; i++)
//        //                {
//        //                    if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
//        //                    {
//        //                        bLastFileExist = true;
//        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
//        //                        break;
//        //                    }
//        //                }

//        //                if (bLastFileExist)
//        //                {
//        //                    if (Is_File_Finished(lastFile))
//        //                    {
//        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

//        //                        for (int i = 0; i < dFileNameList.Length; i++)
//        //                        {
//        //                            if (Dir + dFileNameList[i].ToString() == lastFile)
//        //                            {
//        //                                if (dFileNameList.Length > i + 1)
//        //                                {
//        //                                    FileName = Dir + dFileNameList[i + 1].ToString();
//        //                                    Position = 0;
//        //                                    lastFile = FileName;
//        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
//        //                                    break;
//        //                                }
//        //                                else
//        //                                {
//        //                                    FileName = lastFile;
//        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
//        //                                }
//        //                            }
//        //                        }
//        //                    }
//        //                    else
//        //                    {
//        //                        FileName = lastFile;
//        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
//        //                    }
//        //                }
//        //                else
//        //                {
//        //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
//        //                    Position = 0;
//        //                    lastFile = FileName;
//        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
//        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
//        //                }
//        //            }
//        //            else
//        //            {
//        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
//        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

//        //                if (dFileNameList.Length > 0)
//        //                {
//        //                    FileName = Dir + dFileNameList[0].ToString();
//        //                    lastFile = FileName;
//        //                    Position = 0;
//        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
//        //                }
//        //                else
//        //                {
//        //                    Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
//        //                }
//        //            }
//        //        }
//        //        else
//        //        {
//        //            FileName = Dir;
//        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
//        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
//        //    }
//        //} // ParseFileNameRemote

//        protected override void ParseFileNameRemote()
//        {
//            string line = "";
//            String stdOut = "";
//            String stdErr = "";

//            IConnector sshConn = ConnectionManager.getConnector("SSH");
//            sshConn.SetConfigData(Log);
//            sshConn.Init();

//            try
//            {
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");

//                try
//                {
//                    //se = new SshExec(remoteHost, user);
//                    //se.Password = password;

//                }
//                catch (Exception exception12)
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> connect time out." + exception12.Message.ToString());
//                }
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dir." + Dir);


//                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory | " + Dir);
//                    String command = "ls -lt " + Dir;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

//                    try
//                    {
//                        Thread.Sleep(2000);
//                        //se.Connect();
//                        //se.RunCommand(command, ref stdOut, ref stdErr);
//                        //se.Close();

//                        if (sshConn.initConnection(remoteHost, user, password, "", 0, false))
//                        {
//                            sshConn.runCommand(command);
//                            stdOut = sshConn.read(100000);
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
//                            sshConn.dropConnection();
//                        }
//                        else
//                        {
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
//                        }//
//                    }
//                    catch (Exception exception)
//                    {
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Exception : " + exception);
//                    }

//                    StringReader sr = new StringReader(stdOut);
//                    ArrayList arrFileNameList = new ArrayList();
//                    while ((line = sr.ReadLine()) != null)
//                    {
//                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                        if (arr[arr.Length - 1].StartsWith("radius_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
//                        {
//                            arrFileNameList.Add(arr[arr.Length - 1]);
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
//                        }
//                    }

//                    String[] dFileNameList = SortFiles(arrFileNameList);
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

//                    if (!String.IsNullOrEmpty(lastFile))
//                    {
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is not null: " + lastFile);
//                        bool bLastFileExist = false;

//                        for (int i = 0; i < dFileNameList.Length; i++)
//                        {
//                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
//                            {
//                                bLastFileExist = true;
//                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile is found: " + lastFile);
//                                break;
//                            }
//                        }

//                        if (bLastFileExist)
//                        {
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> File Sizeing. ");

//                            if (Is_File_Finished(lastFile))
//                            {
//                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);

//                                for (int i = 0; i < dFileNameList.Length; i++)
//                                {
//                                    if (Dir + dFileNameList[i].ToString() == lastFile)
//                                    {
//                                        if (dFileNameList.Length > i + 1)
//                                        {
//                                            FileName = Dir + dFileNameList[i + 1].ToString();
//                                            Position = 0;
//                                            lastFile = FileName;
//                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
//                                            break;
//                                        }
//                                        else
//                                        {
//                                            FileName = lastFile;
//                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                FileName = lastFile;
//                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
//                            }
//                        }
//                        else
//                        {
//                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
//                            Position = 0;
//                            lastFile = FileName;
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
//                        }
//                    }
//                    else
//                    {
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File Is Null");
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> ilk defa log okunacak.");

//                        if (dFileNameList.Length > 0)
//                        {
//                            FileName = Dir + dFileNameList[0].ToString();
//                            lastFile = FileName;
//                            Position = 0;
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandý: " + lastFile);
//                        }
//                        else
//                        {
//                            Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
//                        }
//                    }
//                }
//                else
//                {
//                    FileName = Dir;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
//                    RecordFields.fileName = FileName;
//                    //GetLinuxFileSizeControl(FileName);
//                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> File Size: " + GetLinuxFileSizeControl(FileName)); 
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karþýlaþýldý.");
//                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
//            }
//        } // ParseFileNameRemote

//        private bool Is_File_Finished(string file)
//        {
//            int lineCount = 0;
//            string stdOut = "";
//            string stdErr = "";
//            String commandRead;
//            StringReader stReader;
//            String line = "";

//            IConnector sshConn = ConnectionManager.getConnector("SSH");
//            sshConn.SetConfigData(Log);
//            sshConn.Init();

//            try
//            {
//                if (readMethod == "nread")
//                {
//                    commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

//                    //se.Connect();
//                    //se.RunCommand(commandRead, ref stdOut, ref stdErr);
//                    //se.Close();

//                    if (sshConn.initConnection(remoteHost, user, password, "", 0, false))
//                    {
//                        sshConn.runCommand(commandRead);
//                        stdOut = sshConn.read(100000);
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
//                        sshConn.dropConnection();
//                    }
//                    else
//                    {
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
//                    }

//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

//                    stReader = new StringReader(stdOut);
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsýna bakýlýyor.");
//                    //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor. 
//                    while ((line = stReader.ReadLine()) != null)
//                    {
//                        if (line.StartsWith("~?`Position"))
//                        {
//                            continue;
//                        }
//                        lineCount++;
//                    }
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayýsý bulundu. En az: " + lineCount);
//                }
//                else
//                {
//                    commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

//                    //se.Connect();
//                    //se.RunCommand(commandRead, ref stdOut, ref stdErr);
//                    //se.Close();

//                    if (sshConn.initConnection(remoteHost, user, password, "", 0, false))
//                    {
//                        sshConn.runCommand(commandRead);
//                        stdOut = sshConn.read(100000);
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
//                        sshConn.dropConnection();
//                    }
//                    else
//                    {
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
//                    }

//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

//                    stReader = new StringReader(stdOut);

//                    while ((line = stReader.ReadLine()) != null)
//                    {
//                        lineCount++;
//                    }
//                }

//                if (lineCount > 0)
//                    return false;
//                else
//                    return true;
//            }
//            catch (Exception ex)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
//                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
//                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştirmeden devam edeceğiz.");
//                return false;
//            }
//        } // Is_File_Finished

//        private string[] SortFiles(ArrayList arrFileNames)
//        {
//            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
//            String[] dFileNameList = new String[arrFileNames.Count];

//            try
//            {
//                for (int i = 0; i < arrFileNames.Count; i++)
//                {
//                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
//                    if (parts.Length == 3)
//                    {
//                        dFileNumberList[i] = Convert.ToUInt64(parts[1]);
//                        dFileNameList[i] = arrFileNames[i].ToString();
//                    }
//                    else
//                    {
//                        dFileNameList[i] = null;
//                    }
//                }

//                Array.Sort(dFileNumberList, dFileNameList);

//                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");
//                for (int i = 0; i < dFileNameList.Length; i++)
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
//            }

//            return dFileNameList;
//        } // SortFiles

//        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
//        //{
//        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in DNS_FileRecorderRecorder -->> Enter the Function.");
//        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in DNS_FileRecorderRecorder -->> ParseFileName() method should be trigerred. ");
//        //    ParseFileName();
//        //}

//        public override void GetFiles()
//        {
//            try
//            {
//                Dir = GetLocation();
//                GetRegistry();
//                Today = DateTime.Now;
//                ParseFileName();
//            }
//            catch (Exception e)
//            {
//                if (reg == null)
//                {
//                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
//                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
//                }
//                else
//                {
//                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
//                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
//                }
//            }
//        } // GetFiles

//        private long GetLinuxFileSizeControl(string fileName)
//        {
//            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetLinuxFileSizeControl() -->> Is started. ");

//            IConnector sshConn = ConnectionManager.getConnector("SSH");
//            sshConn.SetConfigData(Log);
//            sshConn.Init();

//            try
//            {
//                string stdOut = "";
//                string stdErr = "";
//                String wcArg = "";
//                String wcCmd = "";

//                wcCmd = "wc";
//                wcArg = "-l";
//                String commandRead;
//                String lineCommand = wcCmd + " " + wcArg + " " + FileName;
//                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + lineCommand);


//                if (sshConn.initConnection(remoteHost, user, password, "", 0, false))
//                {
//                    sshConn.runCommand(lineCommand);
//                    stdOut = sshConn.read(100000);
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Connection open : ");
//                    sshConn.dropConnection();
//                }
//                else
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH connection couldn't open.");
//                }

//                //if (!se.Connected)
//                //{
//                //    se.Connect();
//                //}
//                //se.RunCommand(lineCommand, ref stdOut, ref stdErr);
//                //se.Close();

//                String[] arr = SpaceSplit(stdOut, false);
//                RecordFields.tempPosition = Convert.ToInt64(arr[0]);
//                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + RecordFields.tempPosition.ToString());
//            }
//            catch (Exception exception)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, " GetLinuxFileSizeControl() -->>  : " + exception);
//            }
//            return RecordFields.tempPosition;
//        } // GetLinuxFileSizeControl
//    }
//}



using System;
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
        public Int64 tempPosition;
        public string fileName;
    }

    public class CryptTechHotspotV_1_0_0Recorder : Parser
    {
        private string dateFormat = "yyyy/MM/dd HH:mm:ss";
        private Fields RecordFields;




        
        


        public CryptTechHotspotV_1_0_0Recorder()
            : base()
        {
            LogName = "CryptTechHotspotV_1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = false;
            lineLimit = 50;
            Position = lastTempPosition;
        }

        public CryptTechHotspotV_1_0_0Recorder(String fileName)
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

            if (line == "")
                return true;

            String[] lineArr = SpaceSplit(line, false);

            try
            {
                Rec r = new Rec();
                r.LogName = LogName;

                if (line.Length > 899)
                {
                    r.Description = line.Substring(0, 899);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                }
                else
                {
                    r.Description = line;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + r.Description);
                }

                try
                {
                    DateTime dt;
                    string date1 = lineArr[5].Split(' ')[0];
                    string[] dateArr = date1.Split('/');
                    string day = dateArr[0];
                    string month = dateArr[1];
                    string year = dateArr[2].Split(':')[0];
                    string time = dateArr[2].Substring(5, dateArr[2].Length - 5);
                    string myDateTimeString = month + ", " + day + "," + year + "  ," + time;
                    dt = Convert.ToDateTime(myDateTimeString);
                    string date = dt.ToString(dateFormat);
                    r.Datetime = date.ToString(CultureInfo.InvariantCulture);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + r.Datetime);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Date Error: " + r.Datetime);
                }

                try
                {
                    if (lineArr.Length > 7)
                    {
                        r.CustomStr2 = lineArr[7];
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 Error: " + lineArr[7]);
                }

                try
                {
                    string[] subLineArr = lineArr[6].Split('|');

                    if (subLineArr.Length > 5)
                    {
                        r.EventType = subLineArr[5];
                    }

                    if (subLineArr.Length > 3)
                    {
                        r.UserName = subLineArr[3];
                    }

                    if (subLineArr.Length > 4)
                    {
                        r.CustomStr3 = subLineArr[4];
                    }

                    if (subLineArr.Length > 1)
                    {
                        r.CustomStr8 = subLineArr[1];
                    }

                    if (subLineArr.Length > 6)
                    {
                        r.CustomStr9 = subLineArr[6];
                        r.CustomStr10 = subLineArr[subLineArr.Length - 1];

                    }

                    if (subLineArr.Length > 10)
                    {
                        r.CustomStr10 = subLineArr[10];
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Parsing Error: " + r.Datetime);
                }

                //r.CustomStr9 = FileName;

                if (!string.IsNullOrEmpty(r.EventType))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType" + r.EventType);
                }

                if (!string.IsNullOrEmpty(r.ComputerName))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName" + r.ComputerName);
                }

                if (!string.IsNullOrEmpty(r.CustomStr1))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1" + r.CustomStr1);
                }

                if (!string.IsNullOrEmpty(r.CustomStr2))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2" + r.CustomStr2);
                }

                if (!string.IsNullOrEmpty(r.CustomStr3))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3" + r.CustomStr3);
                }

                if (!string.IsNullOrEmpty(r.CustomStr4))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4" + r.CustomStr4);
                }

                if (!string.IsNullOrEmpty(r.CustomStr9))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9" + r.CustomStr9);
                }

                long fileLength = GetLinuxFileSizeControl(RecordFields.fileName);
                Log.Log(LogType.FILE, LogLevel.INFORM, "fileLength: " + fileLength);
                Log.Log(LogType.FILE, LogLevel.INFORM  , "position: " + Position);
                
                

                r.CustomInt10 = Position;
                if (Position > fileLength)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is greater than the length file position will be reset.");
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position = 0 ");
                }

                Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
                SetRecordData(r);
                
                Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                return true;
            }

            return true;
        } // ParseSpecific

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
        } // Convert_To_Int32

        private long Convert_To_Int64(string strValue)
        {
            long intValue = 0;
            try
            {
                intValue = Convert.ToInt64(strValue);
                return intValue;
            }
            catch (Exception ex)
            {
                return 0;
            }
        } // Convert_To_Int32


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
                        arrFileNameList.Add(arr[arr.Length - 1]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                    }

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
                    RecordFields.fileName = FileName;
                    //Position = lastTempPosition;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
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
            ArrayList fileNameList = new ArrayList();

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    //string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        //dFileNumberList[i] = Convert.ToUInt64(parts[1]);
                        //dFileNameList[i] = arrFileNames[i].ToString();
                        fileNameList.Add(arrFileNames[i].ToString());
                    }
                    else
                    {
                        //dFileNameList[i] = null;
                    }
                }
                fileNameList.Sort();
                //Array.Sort(dFileNumberList, dFileNameList);

                for (int i = 0; i < fileNameList.Count; i++)
                {
                    dFileNameList[i] = fileNameList[i].ToString();
                }

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

        ///<summary>
        /// String olarak gönderilen Ip bilgisinin Geçerli bir Ipv4 olup olmadığını bool olarak döndürür.
        /// </summary>
        /// <param name="ip">
        /// String IpV4</param>
        /// <returns></returns>
        private bool ValidIp(string ip)
        {
            string[] arrIP = ip.Split('.');
            if (arrIP.Length != 4)
            {
                return false;
            }
            byte a = 0;
            foreach (string item in arrIP)
            {
                if (!byte.TryParse(item, out a))
                {
                    return false;
                }
            }
            return true;
        } // ValidIp

        private long GetLinuxFileSizeControl(string fileName)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetLinuxFileSizeControl() -->> Is started. ");
            long position = 0;
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
                //RecordFields.tempPosition = Convert.ToInt64(arr[0]);
                position = Convert.ToInt64(arr[0]);
                //Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + RecordFields.tempPosition.ToString());
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + position);
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetLinuxFileSizeControl() -->>  : " + exception);
            }
            return position;
        } // GetLinuxFileSizeControl
    }
}


