using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class EndianRecorder : Parser
    {
        public EndianRecorder()
            : base()
        {
            LogName = "EndianRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public EndianRecorder(String fileName)
            : base(fileName)
        {
            LogName = "EndianRecorder";
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                String day = "firewall";
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                        "Searching for file: " + day.StartsWith("firewall") + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Contains(day))
                    {
                        FileName = file;
                        break;
                    }
                }
            }
            else
                FileName = Dir;
        } // ParseFileNameLocal

        public override String GetLocation()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  EndianRecorder In GetLocation() -->> Enter The Function");

            string templocationregistry = "";
            string templocationspecialfolder = "";
            string templocation = "";

            if (usingRegistry)
            {
                try
                {
                    if (reg == null)
                    {
                        reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\" + LogName, true);
                    }
                    templocationregistry = reg.GetValue("Location").ToString();
                    //templocationspecialfolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                    templocation = templocationregistry;
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Catch In GetLocation() : " + ex.Message + " templocation  is : " + templocation);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Location is : " + templocation);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Exit The Function");
                return templocation;
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Location is : " + Dir);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Exit The Function");
                return Dir;
            }
        } // GetLocation


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
                    String command = "ls -lt " + Dir + " | grep access_";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

                    se.Connect();
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    bool foundAnyFile = false;
                    int fileCnt = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        fileCnt++;
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (arr[arr.Length - 1].StartsWith("access_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        string ss = arr[arr.Length - 1].ToString();
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Onur : " + ss);
                        if (arr[arr.Length - 1].StartsWith("firewall") && arr[arr.Length - 1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length <= 2)//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun Dosya ismi okundu: " + arr[arr.Length - 1]);
                            foundAnyFile = true;
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun OLMAYAN Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }
                    if (!foundAnyFile)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is " + fileCnt + " files counted but there is no proper file in directory; starting like 'firewall'");
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
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
            }
            finally
            {
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
            //lastTempPosition = (long)reg.GetValue("LastPosition");

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
                    //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor.                     
                    while ((line = stReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("~?`Position"))
                        {
                            continue;
                        }
                        lineCount++;
                    }
                    while ((line = stReader.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısı bulundu. En az: " + lineCount);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> ReadMethod : " + readMethod);
                    commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);
                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);
                    stReader = new StringReader(stdOut);
                    //Replace 
                    //while ((line = stReader.ReadLine()) != null)
                    //{
                    //    lineCount++;
                    //}
                    //With
                    while (line == stReader.ReadLine())
                    {
                        lineCount++;
                    }
                    //This
                }
                if (lineCount > 0)
                {
                    return false;
                }
                else
                {
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
        } // Is_File_Finished

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    //string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] parts = arrFileNames[i].ToString().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sıralanmış dosya isimleri yazılıyor.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            }

            for (int i = 0; i < dFileNameList.Length; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                        "SortFiles() -->> Sıralanmış dosya isimleri : " + dFileNameList[i].ToString());
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Masaj: " + e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        } // GetFiles
        // fdsf

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific -->> Line : " + line);
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific -->> position getReg  : " + reg.GetValue("LastPosition"));

            if (line == "")
                return true;

            String[] arr = SpaceSplit(line, true);

            //if (arr.Length < 10)
            //{
            //    Log.Log(LogType.FILE, LogLevel.WARN, " ParseSpecific -->> Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
            //    Log.Log(LogType.FILE, LogLevel.WARN, " ParseSpecific -->> Please fix your Squid Logger before messing with developer! Parsing will continue...");
            //    return true;
            //}

            Rec r = new Rec();
            try
            {
                //r.Datetime = arr[0] + " " + arr[1] + " " + arr[2];
                //r.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime df = DateTime.Now;
                DateTime dt;
                string myDateTimeString = arr[0] + arr[1] + "," + df.Year + "," + arr[2];
                dt = Convert.ToDateTime(myDateTimeString);
                string lastDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                r.Datetime = lastDate;

                r.SourceName = arr[3];
                r.EventCategory = arr[5];
                r.CustomStr1 = arr[6];
                r.CustomStr2 = arr[7];

                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific -->> Datetime" + r.Datetime);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Contains("PROTO"))
                    {
                        r.EventType = arr[i].Split('=')[1];
                    }

                    if (arr[i].Contains("MAC"))
                    {
                        r.ComputerName = arr[i].Split('=')[1];
                    }
                    if (arr[i].Contains("SRC"))
                    {
                        r.CustomStr3 = arr[i].Split('=')[1];
                    }
                    if (arr[i].Contains("DST"))
                    {
                        r.CustomStr4 = arr[i].Split('=')[1];
                    }
                    if (arr[i].Contains("SPT"))
                    {
                        r.CustomInt3 = Convert.ToInt32(arr[i].Split('=')[1]);
                    }
                    if (arr[i].Contains("DPT"))
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[i].Split('=')[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> Line is not proper format. Line could not got.");
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> " + ex.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> Line : " + line);
                r.Description = line;
                r.LogName = LogName;
                Log.Log(LogType.FILE, LogLevel.ERROR,
                           " ParseSpecific -->> Line description'a yazıldı. " + r.Description);
            }

            r.Description = line;
            r.LogName = LogName;

            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                          " ParseSpecific -->> SetRecordData öncesi");

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                    " ParseSpecific -->> SetRecordData öncesi 3 :" + reg.GetValue("ControlStr3").ToString());


                Log.Log(LogType.FILE, LogLevel.DEBUG,
                          " ParseSpecific -->> SetRecordData öncesi 4 : " + reg.GetValue("ControlStr4").ToString());


                if (reg.GetValue("ControlStr3").ToString() == r.CustomStr3 || reg.GetValue("ControlStr4").ToString() == r.CustomStr4)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN,
                            " ParseSpecific -->> Log satırı atlandı. " + line);

                }
                else
                {
                    SetRecordData(r);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseSpecific -->> SetRecordData Sonrası");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR,
                           " ParseSpecific -->> SetRecordData catch" + ex.Message);
            }

            return true;
        } // ParseSpecific

    }
}
