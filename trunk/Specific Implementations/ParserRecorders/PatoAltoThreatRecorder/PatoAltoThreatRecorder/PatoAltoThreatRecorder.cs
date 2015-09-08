using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomTools;
using Log;
using Parser;
using System.Collections;
using System.IO;
using System.Timers;

namespace Parser
{
    public class PaloAltoThreatRecorder : Parser
    {
        public PaloAltoThreatRecorder()
            : base()
        {
            LogName = "PaloAltoThreatRecorder";
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

        public PaloAltoThreatRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            //Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseSpecific() | Parsing Specific line. Line : " + line);
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

                //10 //Domain*,Receive Time*,Serial #*,Type*,Threat/Content Type*,Config Version*,Generate Time*,Source address*,Destination address*,NAT Source IP*,
                //20 //NAT Destination IP*,Rule*,Source User*,Destination User*,Application*,Virtual System*,Source Zone*,Destination Zone*,Inbound Interface*,Outbound Interface*,
                //30 //Log Action*,Time Logged*,Session ID*,Repeat Count*,Source Port*,Destination Port*,NAT Source Port*,NAT Destination Port*,Flags*,IP Protocol*,
                //36 //Action,URL,Threat/Content Name,Category,Severity,Direction

                //     Domain,Receive Time,Serial #,Type,Threat/Content Type,Config Version,Generate Time,Source address,Destination address,NAT Source IP,NAT Destination IP,Rule,Source User,Destination User,Application,Virtual System,Source Zone,Destination Zone,Inbound Interface,Outbound Interface,Log Action,Time Logged,Session ID,Repeat Count,Source Port,Destination Port,NAT Source Port,NAT Destination Port,Flags,IP Protocol,Action,Bytes,Bytes Sent,Bytes Received,Packets,Start Time,Elapsed Time (sec),Category,Padding
                //1,2011/01/25 05:45:17,0004C100832,THREAT,vulnerability,2,2011/01/25 05:45:12,193.189.142.32,168.216.29.89,192.168.0.12,168.216.29.89,Dis_Web_Server_erisim,,,web-browsing,vsys1,DMZ,Internet,ethernet1/1,ethernet1/4,,2011/01/25 05:45:17,56500,1,80,4149,80,4149,0x40,tcp,alert,,HTTP Non RFC-Compliant Response Found(32880),any,informational,server-to-client

                string[] parts = line.Split(',');


                try
                {

                    rec.CustomInt1 = Convert_to_Int32(parts[0]);

                    try
                    {
                        rec.Datetime = Convert.ToDateTime(parts[6]).ToString("yyyy-MM-dd HH:mm:ss");//Date time conversion requeired.
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | There is a problem converting to date.  date : " + parts[4]);
                        return true;
                    }

                    rec.CustomStr3 = parts[7];
                    rec.CustomStr4 = parts[8];
                    rec.CustomStr5 = parts[9];
                    rec.CustomStr6 = parts[10];
                    rec.CustomStr9 = parts[11];
                    rec.UserName = parts[12];
                    rec.CustomStr10 = parts[14];
                    rec.CustomStr1 = parts[18];
                    rec.CustomStr2 = parts[19];
                    rec.CustomInt7 = Convert_to_Int32(parts[22]);
                    rec.CustomInt2 = Convert_to_Int32(parts[23]);
                    rec.CustomInt3 = Convert_to_Int32(parts[24]);
                    rec.CustomInt4 = Convert_to_Int32(parts[25]);
                    rec.CustomInt5 = Convert_to_Int32(parts[26]);
                    rec.CustomInt6 = Convert_to_Int32(parts[27]);
                    rec.CustomStr7 = parts[29];
                    rec.EventType = parts[30];
                    rec.CustomStr8 = parts[32];
                    rec.EventCategory = parts[33];

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
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | Searching for file in directory: " + Dir);
                    ArrayList arrFileNames = new ArrayList();
                    //PA-2020_threat_2011_01_26_last_calendar_day.csv
                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        string sFile = Path.GetFileName(file).ToString();
                        if (sFile.Contains("threat") == true)
                        {
                            arrFileNames.Add(sFile);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() | File in directory: " + sFile);
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
                                FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                BinaryReader br = new BinaryReader(fs, enc);
                                br.BaseStream.Seek(Position, SeekOrigin.Begin);

                                FileInfo fi = new FileInfo(lastFile);
                                Int64 fileLength = fi.Length;
                                StreamReader sr = new StreamReader(fs);
                                string line = "";
                                int linecntTotal = 0;
                                while (string.IsNullOrEmpty(line = sr.ReadLine()))
                                {
                                    linecntTotal++;
                                }
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> linecntTotal " + linecntTotal);


                                sr.BaseStream.Seek(Position, SeekOrigin.Begin);
                                int linecntLeft = 0;
                                while (string.IsNullOrEmpty(line = sr.ReadLine()))
                                {
                                    linecntLeft++;
                                }
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> linecntLeft " + linecntLeft);

                                Char c = ' ';
                                while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                                    c = br.ReadChar();
                                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);


                                    if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                                    {
                                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "  ParseFileNameLocal In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                                    }
                                }

                                //Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Position is: " + br.BaseStream.Position.ToString());
                                //Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() | Length is: " + br.BaseStream.Length.ToString());

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
                    FileName = Dir;
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
                    //TPE_FW1_url_2011_04_09_last_calendar_day.csv
                    //PA-2020_threat_2011_01_26_last_calendar_day.csv
                    //TPE_FW1_url_2011_04_09_last_calendar_day_0.csv
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    int sonNum = parts.Length - 1;

                    string extraNum = parts[sonNum - 1];
                    string yil = parts[sonNum - 7];
                    string ay = parts[sonNum - 6];
                    string gun = parts[sonNum - 5];

                    if (Convert.ToInt32(extraNum) < 10)
                    {
                        extraNum = "0" + extraNum;
                    }

                    dFileNumberList[i] = Convert.ToUInt64(yil + ay + gun + extraNum);
                    dFileNameList[i] = arrFileNames[i].ToString();
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
            Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Entered. ");
            if (dayChangeTimer.Interval != 600000)
            {
                dayChangeTimer.Interval = 600000;
                Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Interval changed. " + dayChangeTimer.Interval);
            }
            else
            {
                dayChangeTimer.Stop();

                String fileLast = FileName;
                Stop();
                if (remoteHost == "")
                {
                    this.ParseFileNameLocal();
                }
                else
                {
                    this.ParseFileNameRemote();
                }
                if (FileName != fileLast)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> Filename changed. ");

                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "File changed, new file is, " + FileName);
                }

                base.Start();

            }

            dayChangeTimer.Start();
        }

        private int Convert_to_Int32(string value)
        {
            int sayi = 0;
            try
            {
                sayi = Convert.ToInt32(value);
                return sayi;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
