using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Text;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;
using SharpSSH.SharpSsh;

namespace RedhatSecureV_1_0_1Recorder
{
    public struct Fields
    {
        public long currentPosition;
        public long rowCount;
    }

    public class RedhatSecureV_1_0_1Recorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100, zone = 0, sleeptime = 0;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, user, password, remote_host = "", lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        private Fields RecordFields;
        protected SshExec se;


        public RedhatSecureV_1_0_1Recorder()
        {
            InitializeComponent();
            RecordFields = new Fields();
        }

        private void InitializeComponent()
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on RedhatSecureV_1_0_1Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on RedhatSecureV_1_0_1Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating RedhatSecureV_1_0_1Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager RedhatSecureV_1_0_1Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  RedhatSecureV_1_0_1Recorder Init Method end.");
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
            lastFile = LastFile;
        }

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            else
                return 0;
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\RedhatSecureV_1_0_1Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedhatSecureV_1_0_1Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager RedhatSecureV_1_0_1Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\RedhatSecureV_1_0_1Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("RedhatSecureV_1_0_1Recorder").GetValue("LastRecordNum"));

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

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
                String stdOut = "";
                String stdErr = "";

                string _fileId = "";
                string _filePattern = "";
                string _fileName = "";
                string _fileCreateTime = "";

                String line = "";

                se = new SshExec(remote_host, user);
                se.Password = password;
                if (location.EndsWith("/"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " RedhatSecureV_1_0_0Recorder In timer1_Tick() --> Directory | " + location);
                    if (!se.Connected)
                    {
                        se.Connect();
                    }
                    se.SetTimeout(Int32.MaxValue);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_0Recorder In timer1_Tick() --> lastFile: " + lastFile);

                    string[] linuxFileParameters = lastFile.Trim().Split(';');

                    if (linuxFileParameters.Length > 0)
                    {
                        _fileId = linuxFileParameters[0];
                        _fileName = linuxFileParameters[1];
                        _filePattern = linuxFileParameters[2];
                        _fileCreateTime = linuxFileParameters[3];
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "LastFile is unrecognized:" + lastFile);
                        return;
                    }

                    if (_fileCreateTime == "-1")
                    {
                        long dt = (long)DateTimeToUnixTimestamp(DateTime.Now);
                        _fileCreateTime = dt.ToString();
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileId: " + _fileId);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileName: " + _fileName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_filePattern: " + _filePattern);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileCreateTime: " + _fileCreateTime);

                    if (last_recordnum == 0)
                    {
                        last_recordnum = 1;
                    }

                    String command = location + "printLog.sh" + " key " + location + " " + _fileId.Trim() + " " +
                                     _fileName.Trim() + " '"
                                     + _filePattern.Trim() + "' "
                                     + _fileCreateTime + " "
                                     + last_recordnum + " " +
                                     (last_recordnum + max_record_send);


                    se.RunCommand(command, ref stdOut, ref stdErr);
                    L.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> SSH command : " + command);

                    StringReader sr = new StringReader(stdOut);
                    L.Log(LogType.FILE, LogLevel.INFORM,
                          "RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Result: " + stdOut);

                    int state = 1;
                    int lineCounter = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        switch (state)
                        {
                            case 1:
                                L.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Start While.");
                                if (line.Equals("key;BEGIN;NEW"))
                                {
                                    state = 2;
                                }
                                else if (line.Equals("key;BEGIN;OK"))
                                {
                                    state = 5;
                                }
                                else if (line.Equals("key;NOFILE"))
                                {
                                    L.Log(LogType.FILE, LogLevel.WARN, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 1 Error There is no file.");
                                    return;
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 1 Error Unknown Line. " + line);
                                    return;
                                }
                                break;
                            case 2:
                                if (line.StartsWith("FILE;"))
                                {
                                    string[] lineArr = line.Split(new char[] { ';' }, 4);
                                    if (lineArr.Length == 4)
                                    {
                                        _fileId = lineArr[1];
                                        _fileCreateTime = lineArr[2];
                                        _fileName = lineArr[3];
                                        state = 3;
                                        break;
                                    }
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 2 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 2 Error Unknown Line. " + line);
                                }
                                return;
                            case 3:
                                if (line.Equals("key;ENDS"))
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");

                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Record sending. " + lastFile + " / " + _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime);
                                        customServiceBase.SetReg(Id, "0", "", _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Record sended." + lastFile);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
                                }
                                return;
                            case 5:
                                if (line.StartsWith("FILE;"))
                                {
                                    string[] lineArr = line.Split(new char[] { ';' }, 4);
                                    if (lineArr.Length == 4)
                                    {
                                        _fileId = lineArr[1];
                                        _fileCreateTime = lineArr[2];
                                        _fileName = lineArr[3];
                                        state = 6;
                                        break;
                                    }
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 5 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 5 Error Unknown Line. " + line);
                                }
                                return;
                            case 6:
                                if (line.Equals("OUTPUT;BEGIN"))
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Record sending.");
                                        customServiceBase.SetReg(Id, last_recordnum.ToString(), line, _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Record sended.");
                                        state = 7;
                                        break;
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
                                }
                                return;
                            case 7:
                                if (line.StartsWith("+"))
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> lines: " + line);
                                    if (CoderParse(line.Substring(1).Trim()))
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Date inserted. ");
                                        lineCounter++;
                                        break;
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 7 Error Unknown Line. " + line);
                                }
                                return;
                        }
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> Inform." + state + " / "+lineCounter + " / "+ max_record_send);

                    if (state > 1)
                    {
                        if (lineCounter < max_record_send)
                        {
                            CheckEOF(_fileId, _fileName, _filePattern, _fileCreateTime);
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In timer1_Tick() -->> State 0 Error Unexpected end of stream.");
                    }
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick

        public void CheckEOF(string _fileId, string _fileName, string _filePattern, string _fileCreateTime)
        {
            string stdOut = "";
            string stdErr = "";
            string line = "";
            string newFileId = "";
            string newFileName = "";
            string newFileCreationTime = "";
            try
            {
                String command = location + "printLog.sh" + " key " + location + " 0 - '"
                                 + _filePattern.Trim() + "' "
                                 + _fileCreateTime + " 1 2";
                L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> command: " + command);

                se.RunCommand(command, ref stdOut, ref stdErr);
                StringReader sr = new StringReader(stdOut);
                int state = 1;
                while ((line = sr.ReadLine()) != null)
                {
                    switch (state)
                    {
                        case 1:
                            if (line.Equals("key;BEGIN;NEW"))
                            {
                                state = 2;
                                break;
                            }
                            if(line.Equals("key;NOFILE"))
                            {
                                L.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> State 1 There is no file.");
                                return;
                            }
                            L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> State 1 Error Unknown Line");
                            return;
                        case 2:
                            if (line.StartsWith("FILE;"))
                            {
                                string[] lineArr = line.Split(new char[] { ';' }, 4);
                                if (lineArr.Length == 4)
                                {
                                    newFileId = lineArr[1];
                                    newFileCreationTime = lineArr[2];
                                    newFileName = lineArr[3];
                                    state = 3;
                                    break;
                                }
                                L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> State 2 Error Missing Fields. " + line);
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> State 2 Error Unknown Line. " + line);
                            }
                            return;
                        case 3:
                            if (line.Equals("key;ENDS"))
                            {
                                if (newFileId != _fileId)
                                {
                                    try
                                    {
                                        //0;-;^[0-9][0-9]*[ \t]*secure([0-9]*)?$;-1
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> Record sending.");
                                        customServiceBase.SetReg(Id, "0", "", newFileId + ";" + newFileName + ";" + _filePattern + ";" + newFileCreationTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = newFileId + ";" + newFileName + ";" + _filePattern + ";" +
                                                   newFileCreationTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> Record sended.");
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> Record sending Error." + exception.Message);
                                    }
                                }
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR,
                                      " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> State 3 Unexpected line: " + line);
                            }
                            return;
                    }
                }

            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CheckEOF() -->> Error: " + exception.Message);
            }
        } // CheckEOF

        public bool CoderParse(string line)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> Started.");
            bool isInformMessage = false;
            try
            {
                String[] arr = SpaceSplit(line, true);
                if (arr.Length < 5)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    L.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }
                if (arr.Length < 5)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    L.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }

                Rec r = new Rec();
                DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                r.SourceName = arr[3];
                r.CustomStr6 = arr[4];
                r.Description = "";
                r.EventType = "Success";
                r.EventCategory = null;
                if (r.CustomStr6.StartsWith("sshd[") || r.CustomStr6.StartsWith("login["))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                    if (arr[5].StartsWith("Accepted")
                        || arr[5].StartsWith("Failed")
                        || arr[5].StartsWith("pam_unix") || arr[5].StartsWith("FAILED"))
                    {
                        Int32 i = 5;
                        for (i = 5; i < arr.Length; i++)
                        {
                            if ((i + 2) < arr.Length)
                            {
                                if (arr[i + 2] == "from"
                                    || arr[i + 2] == "by")
                                {
                                    r.UserName = arr[i + 1];
                                    i += 2;
                                    break;
                                }
                            }
                        }
                        if (i < arr.Length)
                        {
                            r.CustomStr1 = arr[i];
                            i++;
                            r.CustomStr3 = arr[i];
                            i++;
                        }
                        if ((arr[5].StartsWith("pam_unix")))
                        {
                            r.EventCategory = arr[6] + " " + arr[7];
                        }
                        else
                        {
                            r.EventCategory = arr[5] + " " + arr[6];
                        }
                    }
                    else if (arr[5].StartsWith("reverse")
                        || arr[5].StartsWith("Received"))
                    {
                        Int32 i = 5;
                        for (i = 5; i < arr.Length; i++)
                        {
                            if (arr[i].EndsWith(":"))
                                break;

                            r.EventCategory += arr[i] + " ";
                        }
                        if (i < arr.Length)
                        {
                            r.EventCategory = r.EventCategory.Trim();

                            r.CustomStr1 = arr[i].TrimEnd(':');
                            i++;

                            for (; i < arr.Length; i++)
                            {
                                r.CustomStr3 += arr[i] + " ";
                            }
                            r.CustomStr3 = r.CustomStr3.Trim();
                        }
                    }
                    else if (arr[5].StartsWith("Did"))
                    {
                        r.CustomStr1 = arr[arr.Length - 1];
                    }
                    else if (arr[5].StartsWith("Connection") || arr[5].Contains("ession"))
                    {
                        if (arr.Length > 6)
                            r.EventCategory = arr[5] + " " + arr[6];
                        for (int i = 5; i < arr.Length - 1; i++)
                        {
                            if (arr[i] == "by")
                            {
                                r.CustomStr3 = arr[i + 1].Trim();
                                break;
                            }
                            else if (arr[i] == "user")
                            {
                                r.UserName = arr[i + 1].Trim();
                            }
                        }
                    }
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("passwd(pam_unix)"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }

                    for (Int32 i = 5; i < arr.Length; i++)
                    {
                        if (arr[i] == "for")
                        {
                            if (i + 1 <= arr.Length)
                            {
                                r.UserName = arr[i + 1];
                                break;
                            }
                        }
                    }
                    if (arr.Length > 7)
                        r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("su(pam_unix)") || r.CustomStr6.StartsWith("sudo(pam_unix)") || r.CustomStr6.StartsWith("login(pam_unix)"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }

                    bool end = false;
                    for (Int32 i = 5; i < arr.Length; i++)
                    {
                        if (!end)
                        {
                            if ((i + 2) < arr.Length)
                            {
                                if (arr[i + 2] == "by")
                                {
                                    r.UserName = arr[i + 1];
                                    i++;
                                    end = true;
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    if (r.UserName == "" || r.UserName == null)
                    {
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (arr[i] == "user")
                            {
                                if (i + 1 <= arr.Length)
                                {
                                    r.UserName = arr[i + 1];
                                    break;
                                }
                            }
                            else if (arr[i].Contains("ruser="))
                            {
                                r.UserName = arr[i].Split('=')[1];
                                break;
                            }
                        }
                    }
                    if (arr.Length > 7)
                        r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("adduser") || r.CustomStr6.StartsWith("useradd"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                    if (r.UserName == "" || r.UserName == null)
                    {
                        for (Int32 i = 7; i < arr.Length; i++)
                        {
                            if (arr[i].Contains("name="))
                            {
                                if (arr[6] == "user:")
                                    r.UserName = arr[i].Split('=')[1].Trim(',');
                                if (arr[6] == "group:")
                                    r.CustomStr9 = arr[i].Split('=')[1].Trim(',');
                            }
                        }
                    }

                    if (arr.Length > 7)
                        r.EventCategory = arr[5] + " " + arr[6].Trim(':');
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("webmin[") || r.CustomStr6.StartsWith("portmap["))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                }
                else if (r.CustomStr6.StartsWith("userdel"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                    if (arr.Length > 9)
                    {
                        r.EventCategory = arr[5] + " " + arr[7] + " " + arr[9];
                        r.UserName = arr[6].Trim('\'').Remove(0, 1);
                        r.CustomStr9 = arr[8].Trim('\'').Remove(0, 1);
                    }
                    else if (arr.Length > 7)
                    {
                        r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                        if (arr[6].Contains("user"))
                            r.UserName = arr[7].Trim('\'').Remove(0, 1);
                        else
                            r.CustomStr9 = arr[7].Trim('\'').Remove(0, 1);
                    }
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("<nateklog>"))
                {
                    String[] natekArr = arr[5].Split(':');
                    r.CustomStr5 = natekArr[0];
                    String[] natekArr2 = natekArr[1].Split('|');
                    if (natekArr2.Length > 1)
                    {
                        r.UserName = natekArr2[1];
                        r.CustomStr9 = natekArr2[0];
                    }
                    else
                        r.UserName = natekArr[1];
                    r.EventCategory = natekArr[2];
                    for (int h = 6; h < arr.Length; h++)
                    {
                        r.EventCategory += " " + arr[h];
                        if (arr[h].Contains("."))
                            break;
                    }
                }
                else if (r.CustomStr6.StartsWith("usermod"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                    if (arr.Length > 10)
                    {
                        r.EventCategory = arr[5] + " " + arr[7] + " " + arr[8] + " " + arr[9];
                        if (r.EventCategory == "add to shadow group")
                        {
                            r.UserName = arr[6].Trim('\'').Remove(0, 1);
                            r.CustomStr9 = arr[10].Trim('\'').Remove(0, 1);
                        }
                    }
                    else if (arr.Length > 9)
                    {
                        r.EventCategory = arr[5] + " " + arr[7] + " " + arr[8];
                        if (r.EventCategory == "add to group")
                        {
                            r.UserName = arr[6].Trim('\'').Remove(0, 1);
                            r.CustomStr9 = arr[9].Trim('\'').Remove(0, 1);
                        }
                    }
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("sshd(pam_unix)"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }

                    bool end = false;
                    for (Int32 i = 5; i < arr.Length; i++)
                    {
                        if (!end)
                        {
                            if ((i + 2) < arr.Length)
                            {
                                if (arr[i + 2] == "by")
                                {
                                    r.UserName = arr[i + 1];
                                    i++;
                                    end = true;
                                }
                                else
                                    r.EventCategory += arr[i] + " ";
                            }
                        }
                    }

                    if (arr[5] == "authentication")
                    {
                        r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                        for (int i = 5; i < arr.Length; i++)
                        {
                            if (arr[i].StartsWith("user="))
                            {
                                r.UserName = arr[i].Split('=')[1];
                                break;
                            }
                        }
                    }
                    r.CustomStr6 = arrEv[0].ToString();
                }
                else if (r.CustomStr6.StartsWith("dovecot-auth"))
                {
                    r.UserName = "";
                    Int32 i = 6;
                    for (i = 6; i < arr.Length; i++)
                    {
                        if (arr[i].EndsWith(";"))
                        {
                            r.EventCategory += arr[i].TrimEnd(';');
                            break;
                        }

                        r.EventCategory += arr[i] + " ";
                    }
                    for (; i < arr.Length; i++)
                    {
                        if (arr[i].StartsWith("user"))
                            break;
                    }
                    for (; i < arr.Length; i++)
                        r.UserName += arr[i] + " ";

                    r.UserName = r.UserName.Trim();

                    r.CustomStr6 = "dovecot-auth: pam_unix(dovecot:auth):";
                }
                else if (r.CustomStr6.StartsWith("remote(pam_unix)"))
                {
                    String[] arrEv = arr[4].Split('[');
                    if (arrEv.Length > 1)
                    {
                        arrEv[1] = arrEv[1].TrimEnd(':', ']');
                        try
                        {
                            r.CustomStr7 = (arrEv[1]);
                        }
                        catch
                        {
                            r.CustomStr7 = "";
                        }
                    }
                    r.EventCategory = arr[5] + arr[6].Trim(';');

                    for (Int32 i = 7; i < arr.Length; i++)
                    {
                        if (arr[i].Split('=')[0] == "user")
                        {
                            try
                            {
                                r.UserName = arr[i].Split('=')[1];
                            }
                            catch { }
                        }
                    }
                    r.CustomStr6 = "remote(pam_unix)";
                }
                else
                {
                    isInformMessage = true;
                }

                if (remote_host != "")
                    r.ComputerName = remote_host;
                else
                {
                    String[] arrLocation = location.Split('\\');
                    if (arrLocation.Length > 1)
                        r.ComputerName = arrLocation[2];
                }

                r.LogName = "RedhatSecureV_1_0_1Recorder";
                r.CustomStr10 = Environment.MachineName;
                r.CustomStr1 = r.UserName;

                //Nov 12 15:54:36 itim last message repeated 2 times                    
                if (r.EventCategory != null)
                {
                    if (r.EventCategory.Contains("Fail") || r.EventCategory.Contains("fail"))
                    {
                        r.EventType = "Failure";
                    }
                }
                else
                {
                    r.EventType = "";
                }
                if (isInformMessage)
                {
                    for (int j = 4; j < arr.Length; j++)
                    {
                        r.Description += arr[j] + " ";
                    }
                }
                else
                {
                    for (int j = 5; j < arr.Length; j++)
                    {
                        r.Description += arr[j] + " ";
                    }
                }

                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> RemoteRecorder table updating.");
                    customServiceBase.SetData(Dal, virtualhost, r);
                    ++last_recordnum;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> RemoteRecorder table updated.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> RemoteRecorder table update Error." + exception.Message);
                }

                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> Record sending.");
                    customServiceBase.SetReg(Id, last_recordnum.ToString(), line, lastFile, "", LastRecordDate);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> Record sended.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureV_1_0_1Recorder In CoderParse() Error.");
                return false;
            }
        } // CoderParse

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

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("RedhatSecureV_1_0_1Recorder");
                rk.SetValue("LastRecordNum", status);
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SQLServer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
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
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
