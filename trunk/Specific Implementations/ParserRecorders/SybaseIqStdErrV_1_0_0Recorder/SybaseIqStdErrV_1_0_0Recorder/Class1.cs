////Writer: Onur Sarıkaya 
////Date: 27.03.2012

//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Win32;
//using System.IO;
//using System.Timers;
//using CustomTools;
//using Log;
//using SharpSSH.SharpSsh;
//using System.Collections;

//namespace Parser
//{
//    public struct Fields
//    {
//        public Int64 tempPosition;
//        public string fileName;
//        public int maxRecordSendCounter;
//        public string dateTime;
//    }

//    public class SybaseIqStdErrV_1_0_0Recorder : Parser
//    {
//        private Fields RecordFields;
//        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

//        public SybaseIqStdErrV_1_0_0Recorder()
//            : base()
//        {
//            LogName = "SybaseIqStdErrV_1_0_0Recorder";
//            RecordFields = new Fields();
//        }

//        public override void Init()
//        {
//            GetFiles();
//        }

//        public SybaseIqStdErrV_1_0_0Recorder(String fileName)
//            : base(fileName)
//        {
//        }



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
//            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
//            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

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

//        public override bool ParseSpecific(string line, bool dontSend)
//        {
//            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
//            //bimiq.0016.stderr
//            long fileLength = 0;
//            try
//            {
//                Rec r = new Rec();
//                r.LogName = LogName;

//                string[] lineArr1 = SpaceSplit(line, false);

//                if (line.Contains("Starting server bimiq on"))//startswith'de olabilir.
//                {
//                    //(03/19 14:38:50)
//                    try
//                    {
//                        string date = Between(line, "(", ")");
//                        string[] dateArr = date.Split(' ');
//                        string month = dateArr[0].Split('/')[0];
//                        string day = dateArr[0].Split('/')[1];
//                        string time = dateArr[1];
//                        string year = DateTime.Now.Year.ToString();
//                        DateTime dt;
//                        string myDateTimeString = year + ", " + month + "," + day + "  ," + time;
//                        dt = Convert.ToDateTime(myDateTimeString);
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "DateTime: " + dt.ToString(dateFormat));
//                        RecordFields.dateTime = dt.ToString(dateFormat);
//                    }
//                    catch (Exception exception)
//                    {
//                        Log.Log(LogType.FILE, LogLevel.ERROR, "DateTime Error: " + exception.Message);
//                    }
//                }

//                if (line.ToLower().Contains("error"))
//                {
//                    r.Description = line;
//                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "lineLimit: " + lineLimit);
//                    //Log.Log(LogType.FILE, LogLevel.INFORM, "fileLength: " + fileLength);
//                    //Log.Log(LogType.FILE, LogLevel.INFORM, "position: " + Position);
//                    //Log.Log(LogType.FILE, LogLevel.INFORM, "lineLimit: " + lineLimit);
//                    //if (Position > fileLength)
//                    //{
//                    //    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is greater than the length file position will be reset.");
//                    //    Position = 0;
//                    //    Log.Log(LogType.FILE, LogLevel.INFORM, "Position = 0 ");
//                    //}
//                    r.Datetime = RecordFields.dateTime;
//                    r.CustomStr8 = FileName;
//                    r.CustomInt1 = (int) Position;
//                    Log.Log(LogType.FILE, LogLevel.INFORM, "DateTime:." + r.Datetime);
//                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
//                    SetRecordData(r);
//                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
//                }
//            }
//            catch (Exception e)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
//                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
//                Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
//                return false;
//            }
//            return true;
//        }

//        /*
//                protected override void ParseFileNameLocal()
//                {
//                    if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
//                    {
//                        String day = "access";
//                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
//                        foreach (String file in Directory.GetFiles(Dir))
//                        {
//                            if (file.Contains(day))
//                            {
//                                FileName = file;
//                                break;
//                            }
//                        }
//                    }
//                    else
//                        FileName = Dir;
//                } // ParseFileNameLocal
//         * */

//        protected override void ParseFileNameRemote()
//        {
//            string line = "";
//            String stdOut = "";
//            String stdErr = "";

//            try
//            {
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Enter the Function.");
//                se = new SshExec(remoteHost, user);
//                se.Password = password;
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory : ");
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory : " + Dir);

//                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
//                {//# ls -lt /cache1/nateklog/  | grep ^-
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Home Directory  " + Dir);
//                    String command = "ls -lt " + Dir + " | grep  bimiq";
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> SSH command : " + command);

//                    se.Connect();
//                    se.RunCommand(command, ref stdOut, ref stdErr);
//                    se.Close();

//                    StringReader sr = new StringReader(stdOut);
//                    ArrayList arrFileNameList = new ArrayList();

//                    bool foundAnyFile = false;
//                    int fileCnt = 0;
//                    while ((line = sr.ReadLine()) != null)
//                    {
//                        fileCnt++;
//                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                        string ss = arr[arr.Length - 1].ToString();
//                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Onur : " + ss);
//                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Onur_1 : " + arr[arr.Length - 1]);
//                        string[] extensionArr = ss.Split('.');
//                        //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Onur_2 : " + extensionArr[extensionArr.Length - 1]);
//                        if (extensionArr[extensionArr.Length - 1] == "stderr")
//                        {
//                            arrFileNameList.Add(arr[arr.Length - 1]);
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun Dosya ismi okundu: " + arr[arr.Length - 1]);
//                            foundAnyFile = true;
//                        }
//                        else
//                        {
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> **Uygun OLMAYAN Dosya ismi okundu: " + arr[arr.Length - 1]);
//                        }
//                    }
//                    if (!foundAnyFile)
//                    {
//                        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> There is " + fileCnt + " files counted but there is no proper file in directory; starting like 'access_'");
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
//                            if (Is_File_Finished(lastFile))
//                            {
//                                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + lastFile);
//                                RecordFields.dateTime = "";

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
//                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
//                            Position = 0;
//                            lastFile = FileName;
//                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadý.  Yeni File : " + FileName);
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
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karşılaşıldı.");
//                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
//                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.StackTrace);
//            }
//            finally
//            {
//            }
//        } // ParseFileNameRemote

//        public ArrayList SortFileNamesNew(ArrayList ar)
//        {
//            ArrayList arrReturned = new ArrayList();

//            for (int i = 0; i < ar.Count; i++)
//            {
//                arrReturned.Add(ar[i]);
//            }
//            arrReturned.Sort();
//            arrReturned.Reverse();
//            return arrReturned;

//        } // SortFileNamesNew

//        private bool Is_File_Finished(string file)
//        {
//            Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Started." );

//            int lineCount = 0;
//            string stdOut = "";
//            string stdErr = "";
//            String commandRead;
//            StringReader stReader;
//            String line = "";

//            try
//            {
//                if (readMethod == "nread")
//                {
//                    commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);
//                    se.Connect();
//                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
//                    se.Close();
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);
//                    stReader = new StringReader(stdOut);
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısına bakılıyor.");
//                    //lastFile'dan line ve pozisyon okundu ve şimdi test ediliyor.                     
//                    while ((line = stReader.ReadLine()) != null)
//                    {
//                        if (line.StartsWith("~?`Position"))
//                        {
//                            continue;
//                        }
//                        lineCount++;
//                    }
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satır sayısı bulundu. En az: " + lineCount);
//                }
//                else
//                {
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> ReadMethod : " + readMethod);
//                    commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);
//                    se.Connect();
//                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
//                    se.Close();
//                    Log.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);
//                    stReader = new StringReader(stdOut);
//                    //Replace 
//                    //while ((line = stReader.ReadLine()) != null)
//                    //{
//                    //    lineCount++;
//                    //}
//                    //With
//                    while (line == stReader.ReadLine())
//                    {
//                        lineCount++;
//                    }
//                    //This
//                }

//                if (lineCount > 1)
//                {
//                    return false;
//                }
//                else
//                {
//                    Log.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
//                    return true;
//                }
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

//            for (int i = 0; i < arrFileNames.Count; i++)
//            {
//                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() - arrFileNames  -->> " + arrFileNames[i]);
//            }

//            try
//            {
//                for (int i = 0; i < arrFileNames.Count; i++)
//                {
//                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
//                    //string[] parts = arrFileNames[i].ToString().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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

//            for (int i = 0; i < dFileNameList.Length; i++)
//            {
//                Log.Log(LogType.FILE, LogLevel.DEBUG,
//                        "SortFiles() -->> Sıralanmış dosya isimleri : " + dFileNameList[i].ToString());
//            }
//            return dFileNameList;
//        } // SortFiles

//        public override void GetFiles()
//        {
//            try
//            {
//                Dir = GetLocation();
//                Log.Log(LogType.EVENTLOG, LogLevel.DEBUG, "GetFiles() -->> Directory : " + Dir);
//                GetRegistry();
//                Today = DateTime.Now;
//                ParseFileName();
//            }
//            catch (Exception e)
//            {
//                if (reg == null)
//                {
//                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
//                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "GetFiles() -->> Masaj: " + e.StackTrace);
//                }
//                else
//                {
//                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFiles() -->> Error while getting files, Exception: " + e.Message);
//                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
//                }
//            }
//        } // GetFiles

//    }
//}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;
using SharpSSH.SharpSsh;

namespace SybaseIqStdErrV_1_0_0Recorder
{

    public struct Fields
    {
        public Int64 tempPosition;
        public string fileName;
        public int maxRecordSendCounter;
        public string dateTime;
    }

    public class SybaseIqStdErrV_1_0_0Recorder : CustomBase
    {

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, user, password, remote_host = "", _lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        protected SshExec se;
        private String[] dFileNameList;
        protected String FileName;
        protected string readMethod = "";
        protected List<string> dFileNameList1;
        protected bool FileContinuosly = true;
        private Fields RecordFields;

        public SybaseIqStdErrV_1_0_0Recorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            RecordFields = new Fields();
        }

        public override void Init()
        {
            try
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
                timer1.Interval = timer_interval;
                timer1.AutoReset = false;
                timer1.Enabled = true;


                if (usingRegistry)
                {
                    if (!reg_flag)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "Error on Intialize Logger on SybaseIqStdErrV_1_0_0Recorder functions may not be running");
                            return;
                        }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!Get_logDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "Error on Intialize Logger on SybaseIqStdErrV_1_0_0Recorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating SybaseIqStdErrV_1_0_0Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager SybaseIqStdErrV_1_0_0Recorder Recorder Init",
                                    exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  SybaseIqStdErrV_1_0_0Recorder Init Method end.");
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() +
                          @"log\SybaseIqStdErrV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SybaseIqStdErrV_1_0_0Recorder  Read Registry", er.ToString(),
                                    EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "Security Manager SybaseIqStdErrV_1_0_0Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Initialize_Logger()
        {
            try
            {
                L = new CLogger();
                switch (trc_level)
                {
                    case 0:
                        {
                            L.SetLogLevel(LogLevel.NONE);
                        }
                        break;
                    case 1:
                        {
                            L.SetLogLevel(LogLevel.INFORM);
                        }
                        break;
                    case 2:
                        {
                            L.SetLogLevel(LogLevel.WARN);
                        }
                        break;
                    case 3:
                        {
                            L.SetLogLevel(LogLevel.ERROR);
                        }
                        break;
                    case 4:
                        {
                            L.SetLogLevel(LogLevel.DEBUG);
                        }
                        break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size =
                    Convert.ToUInt32(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval =
                    Convert.ToUInt32(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue(
                            "Logging Interval"));
                trc_level =
                    Convert.ToInt32(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() +
                          @"log\SybaseIqStdErrV_1_0_0Recorder.log";
                this.timer1.Interval =
                    Convert.ToInt32(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue("Interval"));
                max_record_send =
                    Convert.ToInt32(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue(
                            "MaxRecordSend"));
                last_recordnum =
                    Convert.ToInt64(
                        rk.OpenSubKey("Recorder").OpenSubKey("SybaseIqStdErrV_1_0_0Recorder").GetValue(
                            "LastRecordNum"));

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " Read_Registry -->> Error : " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }


        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
         String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
         String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
         String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            fromend = FromEndOnLoss;
            max_record_send = MaxLineToWait;//Data amount to get per tick
            timer_interval = SleepTime; //Timer interval.
            user = User; //User name
            password = Password; //password
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;
            _lastFile = LastFile;

        }

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            else
                return 0;
        }

        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ' && (!useTabs || c != '\t'))
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }// SpaceSplit


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
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

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

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a, int type)
        {
            //type = 0 first
            //type = 1 last
            int posA = 0;

            if (type == 1)
            {
                posA = value.IndexOf(a);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a);
            }

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

        public virtual String[] SpaceSplit(String line, bool useTabs, Char ignoreCharStart, Char ignoreCharEnd)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            bool ignore = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c == ignoreCharStart)
                    ignore = true;
                else if (c == ignoreCharEnd)
                    ignore = false;
                if (c != ' ' && (!useTabs || c != '\t'))
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    if (ignore)
                        sb.Append(c);
                    else
                        space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
            DandikParseFileNameRemote();
        }

        public void DandikParseFileNameRemote()
        {
            string line = "";
            String stdOut = "";
            String stdErr = "";

            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Enter the Function.");
                se = new SshExec(remote_host, user);
                se.Password = password;
                if (location.EndsWith("/") || location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Home Directory | " + location);
                    String command = "ls -lt " + location;
                    L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> SSH command : " + command);

                    try
                    {
                        se.Connect();
                        se.RunCommand(command, ref stdOut, ref stdErr);
                        se.Close();
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Exception : " + exception);
                    }

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();
                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //if (arr[arr.Length - 1].StartsWith("audit")  && arr[arr.Length - 1].Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)//Name changed
                        if (arr[arr.Length - 1].StartsWith("bimiq"))//Name changed
                        {
                            arrFileNameList.Add(arr[arr.Length - 1]);
                            L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Dosya ismi okundu: " + arr[arr.Length - 1]);
                        }
                    }
                    dFileNameList = SortFiles(arrFileNameList);
                    L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı.");

                    if (!String.IsNullOrEmpty(_lastFile))
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> LastFile is not null: " + _lastFile);
                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((location + dFileNameList[i]) == _lastFile)
                            {
                                bLastFileExist = true;
                                L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> LastFile is found: " + _lastFile);
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            if (Is_File_Finished(_lastFile))
                            {
                                L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Last File is finished. Previous File: " + _lastFile);

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (location + dFileNameList[i].ToString(CultureInfo.InvariantCulture) == _lastFile)
                                    {
                                        L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> dFileNameList.length: " + dFileNameList.Length.ToString());
                                        L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> i: " + i.ToString());


                                        if (dFileNameList.Length > i + 1)
                                        {
                                            FileName = location + dFileNameList[i + 1].ToString(CultureInfo.InvariantCulture);
                                            last_recordnum = 0;
                                            _lastFile = FileName;
                                            L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> New File is assigned. New File: " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            //DandikParseFileNameRemote();
                                            int index = Array.IndexOf(dFileNameList, _lastFile);
                                            if (index + 1 != dFileNameList.Length)
                                            {
                                                DandikParseFileNameRemote();
                                            }
                                            else
                                            {
                                                FileName = _lastFile;
                                                L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> There is no new file to assign. Wait this file for log: " + FileName);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                FileName = _lastFile;
                                L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> There is still line in lastfile.  Continue to read this file: " + FileName);
                            }
                        }
                        else
                        {
                            FileName = location + dFileNameList[dFileNameList.Length - 1].ToString(CultureInfo.InvariantCulture);
                            last_recordnum = 0;
                            _lastFile = FileName;
                            L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> LastFile Silinmis , Dosya Bulunamadı.  Yeni File : " + FileName);
                            L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Start to read  main file from beginning: " + FileName);
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> Last File Is Null");
                        L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> ilk defa log okunacak.");

                        if (dFileNameList.Length > 0)
                        {
                            FileName = location + dFileNameList[0].ToString(CultureInfo.InvariantCulture);
                            _lastFile = FileName;
                            last_recordnum = 0;
                            L.Log(LogType.FILE, LogLevel.INFORM, "ParseFileNameRemote() -->> FileName ve LastFile en eski dosya olarak ayarlandı: " + _lastFile);
                        }
                        else
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> In The Log Location There Is No Log File to read.");
                        }
                    }
                }
                else
                {
                    FileName = location;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }

                LastRecordDate = DateTime.Now.ToString(dateFormat);
                line = "";

                try
                {
                    CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                    L.Log(LogType.FILE, LogLevel.INFORM,
                          " SybaseIqStdErrV_1_0_0Recorder In CoderParse() -->> Record sending." + "Id: " + Id +
                          "last_recordnum: " + last_recordnum + "line: " + line + "LastRecordDate: " + LastRecordDate);

                    customServiceBase.SetReg(Id, last_recordnum.ToString(), line, _lastFile, "", LastRecordDate);
                    L.Log(LogType.FILE, LogLevel.INFORM, " SybaseIqStdErrV_1_0_0Recorder In CoderParse() -->> Record sended.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " SybaseIqStdErrV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                }
                GetLinuxFileLines(_lastFile);
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Dosya isimleri getirilirken problemle karşılaşıldı.");
                L.Log(LogType.FILE, LogLevel.ERROR, "ParseFileNameRemote() -->> Hata Mesajı: " + ex.ToString());
            }
        }//DandikParseFileNameRemote

        private void GetLinuxFileLines(string lastFile)
        {

            int lineCount = 0;
            string stdOut = "";
            string stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";
            try
            {
                if (last_recordnum == 0)
                {
                    last_recordnum = 1;
                }
                commandRead = "sed" + " -n " + last_recordnum + "," + (max_record_send) + "p " + lastFile;
                L.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileLines() -->> commandRead For nread Is : " + commandRead);

                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();

                //L.Log(LogType.FILE, LogLevel.DEBUG, " GetLinuxFileLines() -->> commandRead'den dönen strOut : " + stdOut);

                stReader = new StringReader(stdOut);

                while ((line = stReader.ReadLine()) != null)
                {
                    lineCount++;
                    CoderParse(line, lineCount, lastFile);
                }
                L.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileLines() -->> End of Wihle. " + lastFile + " is finished.");
                timer1.Enabled = true;
                //Is_File_Finished(lastFile);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetLinuxFileLines() -->> Error: " + exception.Message);
            }
        } // GetLinuxFileLines

        public void CoderParse(String line, long lineCount, string lastFile)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
            //bimiq.0016.stderr
            long fileLength = 0;
            try
            {
                Rec r = new Rec();
                r.LogName = "SybaseIqStdErrV_1_0_0Recorder";

                string[] lineArr1 = SpaceSplit(line, false);

                if (line.Contains("Starting server bimiq on"))//startswith'de olabilir.
                {
                    //(03/19 14:38:50)
                    try
                    {
                        string date = Between(line, "(", ")");
                        string[] dateArr = date.Split(' ');
                        string month = dateArr[0].Split('/')[0];
                        string day = dateArr[0].Split('/')[1];
                        string time = dateArr[1];
                        string year = DateTime.Now.Year.ToString();
                        DateTime dt;
                        string myDateTimeString = year + ", " + month + "," + day + "  ," + time;
                        dt = Convert.ToDateTime(myDateTimeString);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "DateTime: " + dt.ToString(dateFormat));
                        RecordFields.dateTime = dt.ToString(dateFormat);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "DateTime Error: " + exception.Message);
                    }
                }

                if (line.ToLower().Contains("error"))
                {
                    r.Description = line;
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, "lineLimit: " + lineLimit);
                    //Log.Log(LogType.FILE, LogLevel.INFORM, "fileLength: " + fileLength);
                    //Log.Log(LogType.FILE, LogLevel.INFORM, "position: " + Position);
                    //Log.Log(LogType.FILE, LogLevel.INFORM, "lineLimit: " + lineLimit);
                    //if (Position > fileLength)
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, "Position is greater than the length file position will be reset.");
                    //    Position = 0;
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, "Position = 0 ");
                    //}
                    r.Datetime = RecordFields.dateTime;
                    r.CustomStr8 = FileName;
                    r.CustomInt1 = (int)last_recordnum;
                    L.Log(LogType.FILE, LogLevel.INFORM, "DateTime:." + r.Datetime);
                    L.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");

                    CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                    try
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table updating.");
                        customServiceBase.SetData(Dal, virtualhost, r);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table updated.");
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table update Error." + exception.Message);
                    }
                    last_recordnum = lineCount;
                    LastRecordDate = r.Datetime;
                    try
                    {
                        L.Log(LogType.FILE, LogLevel.INFORM,
                              " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> Record sending." + last_recordnum);
                        customServiceBase.SetReg(Id, last_recordnum.ToString(), line, lastFile, "", LastRecordDate);
                        L.Log(LogType.FILE, LogLevel.INFORM, " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> Record sended.");
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " LinuxApacheAccessV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                    }
                    
                    L.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
            }
        } // CoderParse



        private bool Is_File_Finished(string file)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SybaseIqStdErrV_1_0_0Recorder In Is_File_Finished() Started.");

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
                    commandRead = "nread" + " -n " + last_recordnum + "," + 3 + "p " + file;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

                    stReader = new StringReader(stdOut);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satyr sayysyna bakylyyor.");
                    //lastFile'dan line ve pozisyon okundu ve ?imdi test ediliyor. 
                    while ((line = stReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("~?`Position"))
                        {
                            continue;
                        }
                        lineCount++;
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> Okunacak satyr sayysy bulundu. En az: " + lineCount);
                }
                else
                {
                    commandRead = "sed" + " -n " + last_recordnum + "," + (last_recordnum + 2) + "p " + file;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead For nread Is : " + commandRead);

                    se.Connect();
                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
                    se.Close();

                    L.Log(LogType.FILE, LogLevel.DEBUG, " Is_File_Finished() -->> commandRead'den dönen strOut : " + stdOut);

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
                L.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + _lastFile + " dosyasının sonu aranırken problem ile karşılaşıldı.");
                L.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> Hata Mesajı: " + ex.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, "Is_File_Finished() -->> " + _lastFile + " dosyasını değiştirmeden devam edeceğiz.");
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
                    string[] parts = arrFileNames[i].ToString().Split(new char[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3 && parts[parts.Length - 1] == "stderr")
                    {
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

                L.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> Sıralanmış Dosya isimleri yazdırılıyor.");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "SortFiles() -->> " + dFileNameList[i]);
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "SortFiles() -->> Sıralama işlemi. Mesaj: " + ex.ToString());
            }
            return dFileNameList;
        } // SortFiles
    }
}
