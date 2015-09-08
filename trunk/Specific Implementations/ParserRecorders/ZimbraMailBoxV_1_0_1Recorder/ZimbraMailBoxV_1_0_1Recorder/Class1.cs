//ZimbraMailBoxV_1_0_1Recorder
//Writer Onur Sarıkaya

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
        public string recordLine;
        public string msgId;
        //public bool flag;
        public string dateTime;
        public string eventCategory;
        public string eventType;
        public string computerName;

        public string customStr1;
        public string customStr2;
        public string customStr7;

        public string customStr8;
        public string customStr9;
        public string customStr10;

        public Int32 customInt3;
        public Int32 customInt4;


    }

    public class ZimbraMailBoxV_1_0_1Recorder : Parser
    {
        private string dateFormat = "yyyy/MM/dd HH:mm:ss";

        private Fields RecordFields;

        public ZimbraMailBoxV_1_0_1Recorder()
            : base()
        {
            LogName = "ZimbraMailBoxV_1_0_1Recorder";
            RecordFields = new Fields();
            usingKeywords = false;
            lineLimit = 50;
        }

        public ZimbraMailBoxV_1_0_1Recorder(String fileName)
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
                string[] lineArr = SpaceSplit(line, true);

                string eventCat = lineArr[3].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[0];

                #region Date_Time
                try
                {
                    DateTime dt;
                    string s = lineArr[0] + " " + lineArr[1].Split(',')[0];
                    dt = Convert.ToDateTime(s);
                    rec.Datetime = dt.ToString(dateFormat);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime " + rec.Datetime.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Datetime Error" + exception.Message);
                }
                #endregion

                #region EventCategory
                rec.EventCategory = eventCat;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory " + rec.EventCategory);
                #endregion

                #region EventType
                rec.EventType = lineArr[2];
                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType " + rec.EventType);
                #endregion

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

                if (eventCat.Contains("Pop3SSLServer"))
                {
                    try
                    {

                        #region UserName
                        string userName = Between(line, "[name=", "ip=").Replace(';', ' ').Trim();
                        rec.UserName = userName;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName " + rec.UserName.ToString(CultureInfo.InvariantCulture));
                        #endregion

                        #region CustomStr4
                        string str4 = Between(line, "ip=", ";").Replace(';', ' ').Trim();
                        rec.CustomStr4 = str4;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 " + rec.CustomStr4.ToString(CultureInfo.InvariantCulture));
                        #endregion

                        #region CustomStr6
                        if (lineArr.Length > 9)
                        {
                            rec.CustomStr6 = lineArr[9];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 " + rec.CustomStr6.ToString(CultureInfo.InvariantCulture));
                        }
                        #endregion

                        #region CustomStr9

                        if (lineArr.Length > 10)
                        {
                            if (lineArr[10].StartsWith("mechanism"))
                            {
                                rec.CustomStr9 = lineArr[10].Split('=')[1];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9: " + rec.CustomStr9);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("mechanism"))
                                    {
                                        rec.CustomStr9 = lineArr[i].Split('=')[1];
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Parsing Error:" + exception.Message);
                    }

                    //RecordFields.flag = true;
                }

                else if (eventCat.Contains("LmtpServer"))
                {
                    string msgId = "";
                    if (line.Contains("msgid"))
                    {
                        rec.ComputerName = Between(line, "ip=", ";").Replace(';', ' ').Trim();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, ComputerName: " + rec.ComputerName);

                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].StartsWith("sender"))
                            {
                                rec.CustomStr1 = lineArr[i].Split('=')[1];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomStr1: " + rec.CustomStr1);
                            }
                        }

                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].StartsWith("size"))
                            {
                                rec.CustomInt3 = Convert.ToInt32(lineArr[i].Split('=')[1]);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomInt3: " + rec.CustomInt3);
                            }
                        }
                        rec.CustomStr8 = Between(line, "lmtp -", ":");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomStr8: " + rec.CustomStr8);
                    }

                    else if (line.Contains("Message-ID"))
                    {
                        //RecordFields.recordLine += line;
                        for (int i = 0; i < lineArr.Length; i++)
                        {
                            if (lineArr[i].StartsWith("Message-ID"))
                            {
                                rec.CustomStr10 = lineArr[i].Split('=')[1];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomStr10: " + rec.CustomStr10);
                            }
                            //2013-05-13 05:56:34,758 INFO  [LmtpServer-132] [name=bkorkmaz@kafkas.edu.tr;mid=543;ip=192.168.254.2;] index - Deferred Indexing: submitted 20 items in 137ms (145.99/sec). (0 items failed to index). IndexDeferredCount now at 20 NumNotSubmitted= 0
                            if (lineArr[i].StartsWith("[name"))
                            {
                                rec.CustomStr2 = Between(lineArr[i], "name=", ";mid=");
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomStr2: " + rec.CustomStr2);

                                if (!string.IsNullOrEmpty(rec.ComputerName))
                                {
                                    rec.ComputerName = Between(lineArr[i], "ip=", ";]");
                                }
                            }

                            if (lineArr[i].Contains("mid="))
                            {
                                rec.CustomInt4 = Convert.ToInt32(Between(lineArr[i], "mid=", ";ip="));
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LmtpServer, CustomStr2: " + rec.CustomInt4);
                            }
                        }

                        //if (RecordFields.customStr10 == msgId)
                        //{
                        //    //RecordFields.flag = true;
                        //    rec.Datetime = RecordFields.dateTime;
                        //    rec.EventCategory = RecordFields.eventCategory;
                        //    rec.EventType = RecordFields.eventType;
                        //    rec.ComputerName = RecordFields.computerName;
                        //    rec.CustomStr1 = RecordFields.customStr1;
                        //    rec.CustomStr2 = RecordFields.customStr2;
                        //    rec.CustomStr8 = RecordFields.customStr8;
                        //    rec.CustomStr10 = RecordFields.customStr10;
                        //    rec.CustomInt3 = RecordFields.customInt3;
                        //    rec.CustomInt4 = RecordFields.customInt4;
                        //}
                    }
                }
                rec.LogName = LogName;

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Position: " + Position);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "tempPosition: " + RecordFields.tempPosition);

                long tempPosition = GetLinuxFileSizeControl(RecordFields.fileName);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "tempPosition: " + RecordFields.tempPosition);

                if (Position > tempPosition)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position büyük  dosya dan büyük pozisyon sıfırlanacak.");
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position = 0 ");
                }

                //if (RecordFields.flag)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record will be sent.");
                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
                    //RecordFields.flag = false;
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
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ONUR : " + arr[arr.Length - 1]);
                        if (arr[arr.Length - 1].StartsWith("mailbox"))//Name changed
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
                    RecordFields.fileName = FileName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }//
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

