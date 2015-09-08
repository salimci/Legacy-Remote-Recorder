//Name: Squid Recorder
//Writer: Ali Yıldırım
//Date: 28.10.2010

//Updated 
//Writer: Onur Sarıkaya 
//Date: 27.03.2012

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
    public class SquidRecorder : Parser
    {
        public SquidRecorder()
            : base()
        {
            LogName = "SquidRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public SquidRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            //13 mayıs
            //1304729478.841    199 10.241.1304729482.773    174 10.241.3.44 TCP_MISS/200 1141 POST http://safebrowsing.clients.google.com/safebrowsing/downloads? - DIRECT/74.125.232.227 application/vnd.google.safebrowsing-update

            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific -->> Line : " + line);
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true);
                if (arr.Length < 10)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, " ParseSpecific -->> Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, " ParseSpecific -->> Please fix your RedHatSecure Logger before messing with developer! Parsing will continue...");
                    return true;
                }
                Rec r = new Rec();
                try
                {
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
                    String[] dateArr = arr[0].Split('.');
                    dt = dt.AddSeconds(Convert.ToDouble(dateArr[0]));
                    dt = dt.AddMilliseconds(Convert.ToDouble(dateArr[1]));
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[1]);
                        r.CustomStr1 = arr[2];
                        r.EventCategory = arr[3];
                        r.CustomInt6 = Convert.ToInt64(arr[4]);
                        r.SourceName = arr[5];
                        if (arr[6].Length > 900)
                        {
                            string tempfield1 = null;
                            string tempfield2 = null;
                            int lastcharacter = arr[6].Length - 900;
                            tempfield1 = arr[6].Substring(0, 900);
                            r.Description = tempfield1;
                            tempfield2 = arr[6].Substring(900, lastcharacter);
                            if (tempfield2.Length > 900)
                            {
                                string tempfield3 = null;
                                string tempfield4 = null;
                                int numberofcharecter = tempfield2.Length - 900;
                                tempfield3 = tempfield2.Substring(0, 900);
                                tempfield4 = tempfield2.Substring(900, numberofcharecter);
                                r.CustomStr7 = tempfield3;
                                r.CustomStr8 = tempfield4;
                            }
                            else
                            {
                                r.CustomStr7 = tempfield2;
                            }
                        }
                        else
                        {
                            String[] parts = arr[6].Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            String custStr10 = "";
                            for (int i = 2; i < parts.Length; i++)
                            {
                                custStr10 += parts[i];
                            }
                            r.CustomStr9 = parts[0] + "//" + parts[1];
                            r.CustomStr10 = custStr10;
                            r.Description = arr[6];
                        }
                        r.CustomStr2 = arr[7];
                        if (arr[8].ToString().Contains("/"))
                        {
                            string[] dstip = arr[8].Split('/');
                            r.CustomStr3 = dstip[1];
                            r.CustomStr6 = dstip[0];
                        }
                        else
                        {
                            r.CustomStr3 = arr[8];
                        }
                        r.CustomStr4 = arr[9];
                        r.CustomStr5 = remoteHost;
                        r.LogName = LogName;
                        r.ComputerName = Environment.MachineName;
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> Line is not proper format. But line is send to Description column!");
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> " + ex.ToString());
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific -->> Line : " + line);
                        r.Description = line;
                        r.LogName = LogName;
                        Log.Log(LogType.FILE, LogLevel.ERROR,
                                " ParseSpecific -->> Line description'a yazıldı. " + r.Description);
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

                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific -->> SetRecordData öncesi");
                SetRecordData(r);
                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseSpecific -->> SetRecordData sonrası");
            }

            return true;
        } // ParseSpecific

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                String day = "access";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory : ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory : " + Dir);

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {//# ls -lt /cache1/nateklog/  | grep ^-
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory  " + Dir);
                    //String command = "ls -lt " + Dir + " | grep ^ access_";
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
                        if (arr[arr.Length - 1].StartsWith("access") && arr[arr.Length - 1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length <= 2)//Name changed
                        //if (arr[arr.Length - 1].StartsWith("access_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
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
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is " + fileCnt + " files counted but there is no proper file in directory; starting like 'access_'");
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.StackTrace);
            }
            finally
            {
            }
        } // ParseFileNameRemote

        public ArrayList SortFileNamesNew(ArrayList ar)
        {
            ArrayList arrReturned = new ArrayList();

            for (int i = 0; i < ar.Count; i++)
            {
                arrReturned.Add(ar[i]);
            }
            arrReturned.Sort();
            arrReturned.Reverse();
            return arrReturned;

        } // SortFileNamesNew

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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısına bakılıyor.");
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

                if (lineCount > 1)
                {
                    return false;
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
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

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() - arrFileNames  -->> " + arrFileNames[i]);
            }

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    //string[] parts = arrFileNames[i].ToString().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                Log.Log(LogType.EVENTLOG, LogLevel.DEBUG, "GetFiles() -->> Directory : " + Dir);
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

    }
}
