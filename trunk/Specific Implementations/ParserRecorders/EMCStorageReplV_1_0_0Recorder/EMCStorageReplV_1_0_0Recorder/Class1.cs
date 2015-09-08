﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CustomTools;
using Log;
using Microsoft.Win32;

namespace EMCStorageReplV_1_0_0Recorder
{
    public class EMCStorageReplV_1_0_0Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, remote_host = "", lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;
        private bool IsFileFinished;
        Dictionary<String, Int32> dictHash;
        private String LogName = "EMCStorageReplV_1_0_0Recorder";

        public EMCStorageReplV_1_0_0Recorder()
        {

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
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on EMCStorageReplV_1_0_0Recorder functions may not be running");
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
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on EMCStorageReplV_1_0_0Recorder functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating EMCStorageReplV_1_0_0Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager EMCStorageReplV_1_0_0Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  EMCStorageReplV_1_0_0Recorder Init Method end.");
        } // Init


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
                        } break;
                    case 1:
                        {
                            L.SetLogLevel(LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            L.SetLogLevel(LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            L.SetLogLevel(LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            L.SetLogLevel(LogLevel.DEBUG);
                        } break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                //L.SetLogFileSize(log_size);
                L.SetLogFileSize(1000000000);


                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
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
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            last_recordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            zone = Zone;
            sleeptime = SleepTime;
            lastFile = LastFile;
        } // SetConfigData

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            else
                return 0;
        } // Convert_To_Int64

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\EMCStorageReplV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager EMCStorageReplV_1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager EMCStorageReplV_1_0_0Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        } // Get_logDir

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\EMCStorageReplV_1_0_0Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("EMCStorageReplV_1_0_0Recorder").GetValue("LastRecordNum"));

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
        } // Read_Registry

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        } // Clear


        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is Started");

                String line = "";
                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " EMCStorageReplV_1_0_0Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In timer1_Tick() --> lastFile: " + lastFile);
                    ParseFileNameLocal();
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick


        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                List<String> fileNameList = new List<String>();
                foreach (String fileName in Directory.GetFiles(location))
                {
                    String fileShortName = Path.GetFullPath(fileName);
                    fileNameList.Add(fileShortName);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnLocal

        protected void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED ");
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Position: " + last_recordnum);

                    if (location.EndsWith("/") || location.EndsWith("\\"))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseFileNameLocal() -->> Searching files in directory : " + location);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList = SortFileNames(fileNameList);
                        if (fileNameList.Count == 0)
                        {
                            return;
                        }

                        for (int i = 0; i < fileNameList.Count; i++)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG,
                                  " ParseFileNameLocal() -->> Sorting Files. " + fileNameList[i]);
                        }

                        L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED " + lastFile);

                        if (string.IsNullOrEmpty(lastFile))
                        {
                            lastFile = fileNameList[0];
                            last_recordnum = 0;
                            LastRecordDate = DateTime.Now.ToString(dateFormat);
                            if (!SetNewFile())
                            {
                                return;
                            }
                            SendLine(lastFile);
                        }
                        else
                        {
                            if (fileNameList.Contains(lastFile))
                            {
                                SendLine(lastFile);
                                L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> lastFile: " + lastFile);
                            }
                            else
                            {
                                try
                                {
                                    bool foundFile = false;
                                    foreach (var v in fileNameList)
                                    {
                                        if (lastFile.CompareTo(v) < 0)
                                        {
                                            lastFile = v;
                                            last_recordnum = 0;
                                            LastRecordDate = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);

                                            if (!SetNewFile())
                                            {
                                                return;
                                            }

                                            foundFile = true;
                                            break;
                                        }
                                    }
                                    if (foundFile)
                                    {
                                        SendLine(lastFile);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> FileName fount exception: " + exception.Message);
                                }
                            }
                        }

                    }
                    else
                    {
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR,
                          " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }// ParseFileNameLocal

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);
                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, last_recordnum.ToString(), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "EMCStorageReplV_1_0_0Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        private List<String> SortFileNames(List<String> fileNameList)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");

            for (int i = 0; i < fileNameList.Count; i++)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> fileNameList: " + fileNameList[i]);

            }

            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                //FileInfo f = new FileInfo(fileNameList[i]);
                //string[] fn = f.Name.Split('.');
                //if (fn.Length == 2 && fn[1] == "txt")
                {
                    _fileNameList.Add(fileNameList[i]);
                }
            }
            _fileNameList.Sort();
            foreach (string t in _fileNameList)
            {
                L.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames

        public bool SendLine(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "SendLine is Started. FileName" + fileName);

            try
            {
                char c;
                int tChar = 0;
                StringBuilder stringBuilder;
                FileStream fileStream;
                BinaryReader reader;


                stringBuilder = new StringBuilder();
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                reader = new BinaryReader(fileStream, Encoding.UTF8);
                string line = "";

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Info: " + fileName + " - " + fileStream.Length + " - " + last_recordnum);
                char ch;

                bool dontSend;

                L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Path _ if : " + fileName + " - " + fileStream.Length + " - " + last_recordnum);

                if (last_recordnum >= 0)
                {
                    reader.BaseStream.Seek(last_recordnum, SeekOrigin.Begin);
                }
                else
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                int r;
                while ((r = reader.Read()) >= 0)
                {
                    ch = (char)r;
                    if (ch == '\n' || ch == '\r')
                    {
                        last_recordnum = reader.BaseStream.Position;
                        if (stringBuilder.Length > 0)
                        {
                            line = stringBuilder.ToString();
                            CoderParse(line, fileName);
                        }
                        stringBuilder.Remove(0, stringBuilder.Length);
                    }
                    else
                    {
                        stringBuilder.Append(ch);
                    }
                }
                return SetLastFile(fileName);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " SendLine() -->> An error occurred : " + exception.ToString());
                return false;
            }
        } // SendLine

        public bool SetLastFile(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile is started with " + fileName);

            CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
            try
            {
                List<String> fileNameList = GetFileNamesOnLocal();
                fileNameList = SortFileNames(fileNameList);
                for (int i = 0; i < fileNameList.Count; i++)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          " SetLastFile() -->> Sorting Files. " + fileNameList[i]);
                }
                int idx = fileNameList.IndexOf(lastFile);


                L.Log(LogType.FILE, LogLevel.DEBUG,
                      " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileName);

                if (idx >= 0)
                {
                    if (fileNameList.Count != idx + 1)
                    {
                        idx++;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new lastfile." + fileNameList[idx]);

                        lastFile = fileNameList[idx];
                        LastRecordDate = DateTime.Now.ToString(dateFormat);
                        last_recordnum = 0;
                        try
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + last_recordnum + " - " + LastRecordDate);

                            customServiceBase.SetReg(Id, last_recordnum.ToString(), null, lastFile, "",
                                                 LastRecordDate);
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR,
                                  "EMCStorageReplV_1_0_0Recorder SetLastFile update error:" + exception.Message);
                            return false;

                        }
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updated.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      " EMCStorageReplV_1_0_0Recorder In SetLastFile() -->> Record sending Error." +
                      exception.Message);
                return false;
            }
        } // SetLastFile

        public bool CoderParse(string line, String fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> Started. " + line);

            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "CoderParse | Line : " + line);
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "CoderParse | Log error : " + exception.ToString());
            }

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            try
            {
                try
                {
                    string[] lineArr = line.Split(',');
                    Rec rec = new Rec();
                    
                    try
                    {
                        string day = lineArr[0].Split('/')[1];
                        string month = lineArr[0].Split('/')[0];
                        string year = lineArr[0].Split('/')[2];

                        string date = year + "/" + month + "/" + day+ " " + lineArr[1].Split(' ')[0];
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> dateString:" + date);

                        DateTime dt = DateTime.Parse(date);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> Datetime." + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> DateTime Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 5)
                        {
                            rec.SourceName = lineArr[5];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> SourceName: " + rec.SourceName);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> SourceName Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 6)
                        {
                            rec.EventCategory = lineArr[6];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> EventCategory: " + rec.EventCategory);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> EventCategory Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 7)
                        {
                            rec.EventType = lineArr[7];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> EventType: " + rec.EventType);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> EventType Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 2)
                        {
                            rec.ComputerName = lineArr[2];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> ComputerName: " + rec.ComputerName);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> ComputerName Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 3)
                        {
                            rec.CustomStr1 = lineArr[3];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr1: " + rec.CustomStr1);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr1 Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 4)
                        {
                            rec.CustomStr2 = lineArr[4];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr2: " + rec.CustomStr2);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr2 Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 8)
                        {
                            rec.CustomStr3 = lineArr[8];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr3: " + rec.CustomStr3);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr3 Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 9)
                        {
                            rec.CustomStr4 = lineArr[9];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr4: " + rec.CustomStr4);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr4" + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 10)
                        {
                            rec.CustomStr5 = lineArr[10];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr5: " + rec.CustomStr5);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr5 Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 11)
                        {
                            rec.CustomStr6 = lineArr[11];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr6: " + rec.CustomStr6);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr6 Error." + exception.Message);
                    }

                    try
                    {
                        if (lineArr.Length > 12)
                        {
                            rec.CustomStr7 = lineArr[3];
                            L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr7: " + rec.CustomStr7);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> CustomStr7 Error." + exception.Message);
                    }


                    if (line.Length > 899)
                    {
                        rec.Description = line.Substring(0,899);
                    }
                    else
                    {
                        rec.Description = line;
                    }


                    rec.CustomStr8 = fileName;


                    CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                    try
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> Record sending."  + " - " + lastFile);
                        customServiceBase.SetData(Dal, virtualhost, rec);
                        customServiceBase.SetReg(Id, last_recordnum.ToString(), line, lastFile, " ", LastRecordDate);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> Record sended.");
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " EMCStorageReplV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "ikinci kısım | " + ex.Message);
                }
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | Line : " + line);
                L.Log(LogType.FILE, LogLevel.ERROR, " EMCStorageReplV_1_0_0Recorder In CoderParse() Error.");
                //WriteMessage("CoderParse | Parsing Error : " + e.ToString());
                //WriteMessage("CoderParse | line: " + line);
                return false;
            }
            return true;
        } // CoderParse
    }
}