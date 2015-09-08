//Düzenleme: Ali Yıldırım, 24.11.2010

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
using System.Globalization;

namespace Parser
{
    public class PostFixRecorder : Parser
    {
        public PostFixRecorder() : base()
        {
            LogName = "PostFixRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public PostFixRecorder(String fileName) : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true);

                if (arr.Length < 5)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 5+, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your postfix logs before messing with developer! Parsing will continue...");
                    return true;
                }

                try
                { 
                    Rec r = new Rec();
                    if (line.StartsWith("deliver"))
                    {
                        r.Description = line;
                        StringBuilder dateString = new StringBuilder();
                        dateString.Append(DateTime.Now.Year).Append(" ").Append(arr[1]).Append(" ").Append(arr[2]).Append(" ").Append(arr[3]);
                        DateTime dt = DateTime.Parse(dateString.ToString());

                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.EventType = arr[0].Split('(')[0];
                        r.UserName = arr[0].Split('(')[1].TrimEnd(':').TrimEnd(')');
                        r.EventCategory = arr[4].TrimEnd(':');
                        r.LogName = LogName;
                        if (remoteHost != "")
                            r.ComputerName = remoteHost;
                        else
                        {
                            String[] arrLocation = Dir.Split('\\');
                            if (arrLocation.Length > 1)
                                r.ComputerName = arrLocation[2];
                        }

                        SetRecordData(r);
                    }
                    else
                    {
                       
                        bool logTypeConnection = false;
                        StringBuilder dateString = new StringBuilder();
                        dateString.Append(DateTime.Now.Year).Append(" ").Append(arr[0]).Append(" ").Append(arr[1]).Append(" ").Append(arr[2]);
                        DateTime dt = DateTime.Parse(dateString.ToString());

                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        //r.Datetime = Convert.ToDateTime(arr[1] + "/" + arr[0] + "/" + DateTime.Now.Year + " " + arr[2], CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss");

                        r.EventType = arr[4];
                        if (arr[5].EndsWith(":"))
                        {
                            r.SourceName = arr[5].TrimEnd(':');
                        }
                        else
                        {
                            r.SourceName = "";
                            logTypeConnection = true;
                        }
                        StringBuilder descString = new StringBuilder();
                        for (Int32 i = 6; i < arr.Length; i++)
                            descString.Append(arr[i]).Append(" ");
                        if (!logTypeConnection)
                        {
                            r.Description = descString.ToString().Trim();
                        }
                        else
                        {
                            r.Description = arr[5] + " " + descString.ToString().Trim();
                        }

                        if (r.SourceName == "pop3-login")
                        {
                            r.EventCategory = arr[6];
                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].Contains("user") == true)
                                    r.UserName = arr[i].Split('=')[1].TrimStart('<').TrimEnd(',', '>');
                                else
                                    if (arr[i].Contains("rip=") == true)
                                        r.ComputerName = arr[i].Split(':')[arr[i].Split(':').Length - 1].Trim(',');
                            }
                        }
                        else if (r.SourceName == "imap-login")
                        {
                            r.EventCategory = arr[6];
                            String[] arrUser = arr[7].Split('=');
                            if (arrUser.Length > 1)
                                r.UserName = arrUser[1].TrimStart('<').TrimEnd(',', '>');
                            String[] arrRip = arr[9].Split(':');
                            if (arrRip.Length > 2)
                                r.ComputerName = arrRip[3].TrimEnd(',');
                        }
                        else if (r.SourceName.StartsWith("POP3("))
                        {
                            String[] arrUser = r.SourceName.Split('(');
                            if (arrUser.Length > 1)
                                r.UserName = arrUser[1].TrimEnd(')');
                            r.EventCategory = arr[7] + " " + arr[8];
                        }
                        else if (r.SourceName.StartsWith("IMAP("))
                        {
                            String[] arrUser = r.SourceName.Split('(');
                            if (arrUser.Length > 1)
                                r.UserName = arrUser[1].TrimEnd(')');
                            r.EventCategory = arr[6] + " " + arr[7];
                            //r.EventCategory = arr[7] + " " + arr[8];//dali
                        }
                        else if (r.EventType.StartsWith("postfix/"))
                        {
                            String[] arrTest = r.EventType.Split('[');
                            if (arrTest.Length > 1)
                            {
                                String test = arrTest[0].Substring(8);
                                switch (test)
                                {
                                    case "cleanup":
                                    case "qmgr":
                                        {
                                            String[] arrIn = arr[6].Split('=');
                                            r.EventCategory = arrIn[0];
                                            if (arrIn.Length > 1)
                                            {
                                                String arrStr = "";
                                                String[] arrOther = null;
                                                StringBuilder sbOther = new StringBuilder();
                                                for (Int32 i = 1; i < arrIn.Length; i++)
                                                {
                                                    sbOther.Append(arrIn[i]).Append('=');
                                                }
                                                arrStr = sbOther.ToString().TrimEnd('=');
                                                arrOther = arrStr.Split(',');
                                                r.UserName = arrOther[0].TrimStart('<').TrimEnd('>');
                                                try
                                                {
                                                    r.CustomInt1 = Convert.ToInt32(arrOther[1].Split('=')[1]);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        } break;
                                    case "smtp":
                                    case "pipe":
                                    case "local":
                                        {
                                            String[] arrIn = arr[6].Split('=');
                                            r.EventCategory = arrIn[0];
                                            if (arrIn.Length > 1)
                                            {
                                                String arrStr = "";
                                                String[] arrOther = null;
                                                StringBuilder sbOther = new StringBuilder();
                                                for (Int32 i = 1; i < arrIn.Length; i++)
                                                {
                                                    sbOther.Append(arrIn[i]).Append('=');
                                                }
                                                arrStr = sbOther.ToString().TrimEnd('=');
                                                arrOther = arrStr.Split(',');
                                                r.UserName = arrOther[0].TrimStart('<').TrimEnd('>');
                                            }
                                            String[] arrCN = arr[7].Split('[');
                                            r.ComputerName = arrCN[0].Substring(6);
                                        } break;
                                    case "smtpd":
                                    case "bounce":
                                    case "anvil":
                                    case "scache":
                                    case "pickup":
                                        {

                                        } break;
                                    default:
                                        {
                                        } break;
                                }
                            }
                        }

                        r.LogName = LogName;
                        if (remoteHost != "")
                            r.ComputerName = remoteHost;
                        else
                        {
                            String[] arrLocation = Dir.Split('\\');
                            if (arrLocation.Length > 1)
                                r.ComputerName = arrLocation[2];
                        }

                        SetRecordData(r);
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
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            /*String day = "access";
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
            foreach (String file in Directory.GetFiles(Dir))
            {
                if (file.Contains(day))
                {
                    FileName = file;
                    break;
                }
            }*/
            FileName = Dir;
        }

        //Parser bu methodu çağırdığında kesinlikle FileName değişkeninde bir dosya adı istiyor.
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
                    String command = "ls -lt " + Dir + " | grep mail_";
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
                        if (arr[arr.Length - 1].StartsWith("mail_") == true && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
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
                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is " + fileCnt + " files counted but there is no proper file in directory; starting like 'mail_'");
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> arrayFileNameList'e atýlan dosya isimleri sýralandý. ");

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
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Finished File: " + lastFile);

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = Dir + dFileNameList[i + 1].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName + " ,New Position: " + Position + " Last File : " + lastFile);
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajý: " + ex.ToString());
            }
            finally
            {
                //stdOut = "";
                //stdErr = "";
                //if (se.Connected)
                //    se.Close();
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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satýr sayısına bakýlýyor.");
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

                if (lineCount > 1)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasını değiştimeden devam edeceğiz.");
                return false;
            }
        }

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
    }
}
