//Name: Serv-U FTP Recorder
//Writer: Ali Yıldırım
//Date: 18.01.2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class SqlServerRecorder : Parser
    {

        public SqlServerRecorder()
            : base()
        {
            LogName = "SqlServerRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public override void Start()
        {
            base.Start();
        }

        public SqlServerRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);
            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Line is Null Or Empty. ");
                return true;
            }
            CustomBase.Rec rec = new CustomBase.Rec();

            rec.Description = line;
            rec.LogName = LogName;
            if (!string.IsNullOrEmpty(remoteHost))
                rec.ComputerName = remoteHost;

            if (!dontSend)
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //2011-03-03 15:46:36.01 spid7s      SQL Trace ID 1 was started by login "sa".
                //2011-03-03 15:46:36.15 spid7s      Starting up database 'mssqlsystemresource'.
                //2011-03-03 15:46:36.19 spid7s      The resource database build version is 10.00.1600. This is an

                try
                {
                    if (parts.Length > 3)
                    {
                        try
                        {
                            string tarih = parts[0];
                            string saat = parts[1].Split('.')[0];
                            rec.Datetime = Convert.ToDateTime(tarih + " "+ saat).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Date formatting error : " + ex.ToString());
                            rec.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        rec.EventCategory = parts[2];
                        rec.CustomStr10 = "";
                        for (int i = 3; i < parts.Length; i++)
                        {
                            rec.CustomStr10 += parts[i] + " ";
                        }
                        rec.CustomStr10 = rec.CustomStr10.Trim();
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | " + ex.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                    return true;
                }

                SetRecordData(rec);
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Start.");

            try
            { 
                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Searching for file in directory: " + Dir);

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    ArrayList arrFileNames = new ArrayList();


                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        string sFile = Path.GetFileName(file).ToString();

                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | File : " + sFile);

                        if (sFile.Equals("ERRORLOG") == true)
                        {
                            arrFileNames.Add(sFile);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | File in Match : " + sFile);
                        }

                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Sorting file in directory: " + Dir);

                    String[] dFileNameList = SortFiles(arrFileNames);

                    if (dFileNameList.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(lastFile))
                        {
                            if (File.Exists(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | lastFile is not null  : " + lastFile);

                                FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                BinaryReader br = new BinaryReader(fs, enc);

                                br.BaseStream.Seek(Position, SeekOrigin.Begin);

                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

                                if (br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal | Position is at the end of the file so changing the File");

                                    for (int i = 0; i < dFileNameList.Length; i++)
                                    {
                                        if (Dir + dFileNameList[i].ToString() == lastFile)
                                        {
                                            if (i + 1 == dFileNameList.Length)
                                            {
                                                FileName = lastFile;
                                                lastFile = FileName;
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Yeni Dosya Yok. Aynı Dosyaya Devam : " + FileName);
                                                break;
                                            }
                                            else
                                            {
                                                FileName = Dir + dFileNameList[(i + 1)].ToString();
                                                lastFile = FileName;
                                                Position = 0;
                                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Eski dosya bitti. Yeni Dosya  : " + FileName);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Dosya Sonu Okunmadı Okuma Devam Ediyor");
                                    FileName = lastFile;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | FileName = LastFile " + FileName);
                                }
                            }
                            else
                            {
                                //Directory deki en son oluşturulmuş dosya okunacak.
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Daha önceden okunan dosya Directory'de bulunamadı.");

                                FileName = Dir + dFileNameList[dFileNameList.Length - 1];
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() |  Directorydeki en yeni dosyaya atandı.  : " + FileName);

                            }
                        }
                        else
                        {
                            //Directorydeki ilk oluşan dosya okunack.
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Daha önceden hiç dosya okunmamış.");

                            FileName = Dir + dFileNameList[0];
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Directorydeki en eski dosyaya atandı.  : " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | There is any file in the directory.");
                    }

                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Dosya doğrudan verildi...  : " + FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() | En dış hata: " + ex.ToString());
            }
        }

        protected override void ParseFileNameRemote()
        {
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
            //            String command = "ls -lt " + Dir + " | grep access_"; //ls access_*.log
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

            //            se.Connect();
            //            se.RunCommand(command, ref stdOut, ref stdErr);
            //            se.Close();

            //            StringReader sr = new StringReader(stdOut);
            //            ArrayList arrFileNameList = new ArrayList();

            //            while ((line = sr.ReadLine()) != null)
            //            {
            //                String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //                if (arr[arr.Length - 1].StartsWith("access_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
            //                {
            //                    arrFileNameList.Add(arr[arr.Length - 1]);
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
            //                }
            //            }

            //            String[] dFileNameList = SortFiles(arrFileNameList);
            //            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atýlan dosya isimleri sýralandý. ");

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
            //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
            //                    Position = 0;
            //                    lastFile = FileName;
            //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
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
            //    finally
            //    {
            //        //stdOut = "";
            //        //stdErr = "";
            //        //if (se.Connected)
            //        //    se.Close();
            //    }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            try
            {
                for (int i = 0; i < arrFileNames.Count; i++)
                {

                    //errorlog
                    dFileNumberList[i] = Convert.ToUInt64(i);
                    dFileNameList[i] = arrFileNames[i].ToString();
                    
                    
                    
                    //string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    //if (parts.Length == 3)
                    //{
                    //    DateTime date = Convert.ToDateTime(parts[1]);
                    //    string ay = date.Month.ToString();
                    //    if (ay.Length == 1)
                    //    {
                    //        ay = "0" + ay;
                    //    }

                    //    string gun = date.Day.ToString();
                    //    if (gun.Length == 1)
                    //    {
                    //        gun = "0" + gun;
                    //    }

                    //    dFileNumberList[i] = Convert.ToUInt64(date.Year + "" + ay + "" + gun);
                    //    dFileNameList[i] = arrFileNames[i].ToString();
                    //}
                    //else
                    //{
                    //    dFileNameList[i] = null;
                    //}
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralam işlemi. Mesaj: " + ex.ToString());
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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() | File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        }

    }
}
