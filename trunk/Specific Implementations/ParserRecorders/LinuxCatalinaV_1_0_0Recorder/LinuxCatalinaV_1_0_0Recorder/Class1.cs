//LinuxCatalinaV_1_0_0Recorder

//LinuxDansGuardianV_1_0_0Recorder
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
        public string errorLine;
        public string eventCategory;
        public string customStr1;
        public string customStr2;
        public string dateTime;
        public bool exceptionFlag;
        public bool errorFlag;
        public bool infoFlag;
        public bool normalFlag;
    }

    public class LinuxCatalinaV_1_0_0Recorder : Parser
    {
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Fields RecordFields;

        public LinuxCatalinaV_1_0_0Recorder()
            : base()
        {
            LogName = "LinuxCatalinaV_1_0_0Recorder";
            RecordFields = new Fields();
            usingKeywords = false;
            lineLimit = 50;
        }

        public LinuxCatalinaV_1_0_0Recorder(String fileName)
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
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is " + line.Trim());

            //WriteMessage("line: " + line);
            Rec rec = new Rec();
            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }

            string[] lineArr = SpaceSplit(line, false);
            if (!dontSend)
            {
                #region Description
                try
                {
                    if (line.Length < 4000)
                    {
                        rec.Description = line;
                    }

                    else
                    {
                        rec.Description = line.Substring(0, 3999);
                        if (line.Length - 4000 > 1999)
                        {
                            rec.CustomStr10 = line.Substring(4000, line.Length - 4000);
                        }
                    }
                    rec.Description = line;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Description Error" + exception.Message);
                    //WriteMessage("Description Error" + exception.Message);
                }
                #endregion

                #region datetime
                try
                {
                    DateTime dt;
                    string myDateTimeString = lineArr[0] + " " + lineArr[1].Split(',')[0];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> myDateTimeString: " + myDateTimeString);
                    dt = Convert.ToDateTime(myDateTimeString);
                    rec.Datetime = dt.ToString(dateFormat);
                    RecordFields.dateTime = rec.Datetime;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "DateTime: " + rec.Datetime);
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "DateTime Error" + exception.Message);
                    //if (string.IsNullOrEmpty(RecordFields.dateTime))
                    //{
                    //    string date = "1970-01-01 00:00:00";
                    //    RecordFields.dateTime = (Convert.ToDateTime(date).ToString(dateFormat));
                    //}
                    //rec.Datetime = RecordFields.dateTime;
                    //WriteMessage("Description Error" + exception.Message);
                    rec.Datetime = DateTime.Now.ToString(dateFormat);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "DateTime - Error: " + rec.Datetime);
                }
                #endregion

                #region ComputerName
                try
                {
                    if (!String.IsNullOrEmpty(remoteHost))
                    {
                        rec.ComputerName = remoteHost;
                    }
                    else
                    {
                        rec.ComputerName = Environment.MachineName;
                    }
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName Error" + exception.Message);
                    //WriteMessage("Description Error" + exception.Message);
                }
                #endregion

                #region Info
                if (lineArr[2] == "INFO")
                {
                    //RecordFields.infoFlag = true;
                    rec.EventCategory = lineArr[2];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> EventCategory: " + rec.EventCategory);

                    rec.CustomStr1 = lineArr[3];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr1: " + rec.CustomStr1);

                    rec.CustomStr2 = lineArr[4];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr2: " + rec.CustomStr2);

                    string pipeLine = Between(line, "[#", "#]");

                    string[] pipeLineArr = pipeLine.Split('|');

                    rec.EventType = pipeLineArr[0];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> EventType: " + rec.EventType);

                    rec.CustomStr3 = pipeLineArr[4];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr3: " + rec.CustomStr3);

                    rec.CustomStr4 = pipeLineArr[2];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr4: " + rec.CustomStr4);

                    rec.CustomStr5 = pipeLineArr[5];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr5: " + rec.CustomStr5);

                    rec.CustomStr6 = pipeLineArr[6];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr6: " + rec.CustomStr6);

                    rec.CustomStr7 = pipeLineArr[7];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr7: " + rec.CustomStr7);

                    rec.CustomStr8 = pipeLineArr[3];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr8: " + rec.CustomStr8);

                    rec.CustomStr9 = pipeLineArr[1];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() -->> CustomStr9: " + rec.CustomStr9);
                }
                #endregion

                #region Error

                else if (lineArr[2] == "ERROR")
                {
                    //RecordFields.exceptionFlag = true;
                    //RecordFields.errorLine = line;

                    string[] errorLine = SpaceSplit(line, false);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> Line: " + line);

                    //RecordFields.eventCategory = errorLine[2];
                    //Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> EventCategory: " + rec.EventCategory);

                    //RecordFields.customStr1 = errorLine[3];
                    //Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> CustomStr1: " + rec.CustomStr1);

                    //RecordFields.customStr2 = errorLine[4];
                    //Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> CustomStr2: " + rec.CustomStr2);

                    rec.EventCategory = errorLine[2];
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> EventCategory: " + rec.EventCategory);

                    rec.CustomStr1 = errorLine[3];
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> CustomStr1: " + rec.CustomStr1);

                    rec.CustomStr2 = errorLine[4];
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> CustomStr2: " + rec.CustomStr2);
                }


                //if (RecordFields.exceptionFlag)
                //{
                //    if (line.Length > 4)
                //    {
                //        if (line.Substring(0, 4) == "2012" || line.Substring(0, 4) == "2013")
                //        {
                //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific() -->> Onur Sarikaya1 " );

                //            rec.EventCategory = RecordFields.eventCategory;
                //            rec.CustomStr1 = RecordFields.customStr1;
                //            rec.CustomStr2 = RecordFields.customStr2;
                //            rec.Datetime = RecordFields.dateTime;

                //            try
                //            {
                //                if (RecordFields.errorLine.Length < 4000)
                //                {
                //                    rec.Description = RecordFields.errorLine;
                //                }

                //                else
                //                {
                //                    rec.Description = RecordFields.errorLine.Substring(0, 3999);
                //                    if (RecordFields.errorLine.Length - 4000 > 1999)
                //                    {
                //                        rec.CustomStr10 = RecordFields.errorLine.Substring(4000, line.Length - 4000);
                //                    }
                //                }
                //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Description: " + rec.Description);
                //            }
                //            catch (Exception exception)
                //            {
                //                Log.Log(LogType.FILE, LogLevel.ERROR, "Description Error" + exception.Message);
                //                //WriteMessage("Description Error" + exception.Message);
                //            }
                //            RecordFields.errorFlag = true;
                //        }
                //        else
                //        {
                //            RecordFields.errorLine += line;
                //        }
                //    }
                //}

                #endregion

                //else
                //{
                //    RecordFields.normalFlag = true;
                //    rec.Datetime = DateTime.Now.ToString(dateFormat);
                //}

                rec.LogName = LogName;

                //Log.Log(LogType.FILE, LogLevel.INFORM, "infoFlag: " + RecordFields.infoFlag.ToString());
                //Log.Log(LogType.FILE, LogLevel.INFORM, "errorFlag: " + RecordFields.errorFlag.ToString());
                //Log.Log(LogType.FILE, LogLevel.INFORM, "normalFlag: " + RecordFields.normalFlag.ToString());
                //Log.Log(LogType.FILE, LogLevel.INFORM, "exceptionFlag: " + RecordFields.exceptionFlag.ToString());

                //if (RecordFields.infoFlag || RecordFields.errorFlag || RecordFields.normalFlag)

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

                lastFile = FileName;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "lastFile: " + lastFile);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Record will be sent.");
                SetRecordData(rec);
                try
                {
                    CustomServiceBase serviceBase = new CustomServiceBase();
                    serviceBase.SetReg(Id, Position.ToString(CultureInfo.InvariantCulture), lastLine, lastFile, " ", DateTime.Now.ToString(dateFormat));
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "SetReg Error." + exception.Message.ToString());
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
                //if (ClearFields())
                //{
                //    Log.Log(LogType.FILE, LogLevel.DEBUG, "Fields Cleared.");
                //}

            }
            return true;
        } // ParseSpecific

        public bool ClearFields()
        {
            RecordFields.eventCategory = "";
            RecordFields.customStr1 = "";
            RecordFields.customStr2 = "";
            RecordFields.dateTime = "";
            RecordFields.errorLine = "";
            RecordFields.errorFlag = false;
            RecordFields.exceptionFlag = false;
            RecordFields.infoFlag = false;
            return true;

        } // ClearFields

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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Recorder tek dosya okuyacak şekilde geliştirilmiştir. Lütfen tek dosya yolu belirtiniz.");

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
                        if (arr[arr.Length - 1].StartsWith("catalina"))
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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısına bakýlýyor.");
                    //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor. 
                    while ((line = stReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("~?`Position"))
                        {
                            continue;
                        }
                        lineCount++;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısı bulundu. En az: " + lineCount);
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

