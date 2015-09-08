//Düzenleme: Ali Yıldırım, 24.11.2010
//Düzenleme: Onur Sarıkaya 20.06.2012

using System;
using System.Globalization;
using System.IO;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class FreeRadiusRecorder : Parser
    {
        public FreeRadiusRecorder() : base()
        {
            LogName = "FreeRadius Recorder";
            usingKeywords = false;
            lineLimit = 50;    
        }

        public FreeRadiusRecorder(String fileName) : base(fileName)
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
                string[] tempfields = line.Split(' ');
                
                int count = 0;

                for (int i = 0; i < tempfields.Length; i++)
                {
                    if (tempfields[i] == "")
                    {
                        count++;
                    }
                }

                string[] fields = new string[tempfields.Length - count];
                int h = 0;
                for (int k = 0; k < tempfields.Length; k++)
                {
                    if (tempfields[k] != "")
                    {
                        fields[h] = tempfields[k];
                        h++;
                    }
                }

                //Log.Log(LogType.FILE, LogLevel.DEBUG, "fields.Length is " + fields.Length.ToString());

                if (fields.Length >= 16)
                {
                    bool logtype = false;
                    bool usertype = false;
                    try
                    {
                        string eventcategory = fields[6].TrimEnd(':');
                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "eventcategory  is " + eventcategory);

                        string eventtype = fields[7] + " " + fields[8].TrimEnd(':');

                        if (eventcategory.ToLower() == "auth" && eventtype == "Login OK")
                        {
                            logtype = true;
                        }

                        string tempusername = fields[9].TrimStart('[').TrimEnd(']');
                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "tempusername is " + tempusername);
                        
                        string username = "";
                        if (!tempusername.StartsWith("anonymous"))
                        {
                            username = fields[9].TrimStart('[').TrimEnd(']');
                            usertype = true;
                        }

                        string lastwords = "";

                        int len = fields.Length;

                        lastwords = fields[len-3]+" "+fields[len-2]+" "+fields[len-1];

                        bool controloflastwords;
                        controloflastwords = true;

                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "lastwords" + lastwords);
                        
                        if (lastwords.TrimEnd(')') != "via TLS tunnel") 
                        {
                            controloflastwords = false;
                        }

                        

                        if (logtype && usertype && controloflastwords)
                        {   
                            CustomBase.Rec rec = new CustomBase.Rec();
                            rec.LogName = LogName;

                            string date = fields[2] + "/" + fields[1] + "/" + fields[4] + " " + fields[3];
                            try
                            {
                                rec.Datetime = Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            catch (Exception ex)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, "An Error Accured While Parsing Date");
                                Log.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                Log.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                            }

                            rec.EventCategory = eventcategory;
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory is :" + rec.EventCategory);
                            rec.EventType = fields[7] + " " + fields[8].TrimEnd(':');
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType is :" + rec.EventType);
                            rec.UserName = username;
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "UserName is :" + rec.UserName);
                            rec.ComputerName = fields[16].Replace('-', ':');
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName is : " + rec.ComputerName);

                            SetRecordData(rec);
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "Send Data");
                        }   
                        else
                        {
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
                else 
                {
                    return true;
                }
            }
            return true;
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

        //protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in FreeRadiusRecorder -->> Enter the Function.");
        //    Log.Log(LogType.FILE, LogLevel.ERROR, "  dayChangeTimer_Elapsed() in FreeRadiusRecorder -->> ParseFileName() method should be trigerred. ");
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
    }
}
