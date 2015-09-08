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

namespace NginxErrorV_1_0_0Recorder
{
    public struct Fields
    {
        public long currentPosition;
        public long rowCount;
    }

    public class NginxErrorV_1_0_0Recorder : CustomBase
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


        public NginxErrorV_1_0_0Recorder()
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NginxErrorV_1_0_0Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NginxErrorV_1_0_0Recorder Recorder  functions may not be running");
                                return;
                            }
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Start creating NginxErrorV_1_0_0Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager NginxErrorV_1_0_0Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  NginxErrorV_1_0_0Recorder Init Method end.");
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NginxErrorV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NginxErrorV_1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager NginxErrorV_1_0_0Recorder Read Registry Error. " + er.Message);
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("Trace Level"));
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NginxErrorV_1_0_0Recorder.log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("Interval"));
                max_record_send = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("MaxRecordSend"));
                last_recordnum = Convert.ToInt64(rk.OpenSubKey("Recorder").OpenSubKey("NginxErrorV_1_0_0Recorder").GetValue("LastRecordNum"));

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
                int port;
                String remoteHost = null;
                String line = "";

                if (remote_host.Contains(":"))
                {
                    port = Convert.ToInt32(remote_host.Split(':')[1]);
                    remoteHost = remote_host.Split(':')[0];
                }
                else
                {
                    remoteHost = remote_host;
                    port = 22;
                }

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (location.EndsWith("/"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " NginxErrorV_1_0_0Recorder In timer1_Tick() --> Directory | " + location);
                    if (!se.Connected)
                    {
                        se.Connect(port);
                    }
                    se.SetTimeout(Int32.MaxValue);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() --> lastFile: " + lastFile);

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
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> SSH command : " + command);

                    StringReader sr = new StringReader(stdOut);
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                          "NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Result: " + stdOut);

                    int state = 1;
                    int lineCounter = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        switch (state)
                        {
                            case 1:
                                L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Start While.");
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
                                    L.Log(LogType.FILE, LogLevel.WARN, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 1 Error There is no file.");
                                    return;
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 1 Error Unknown Line. " + line);
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
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 2 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 2 Error Unknown Line. " + line);
                                }
                                return;
                            case 3:
                                if (line.Equals("key;ENDS"))
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");

                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Record sending. " + lastFile + " / " + _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime);
                                        customServiceBase.SetReg(Id, "0", "", _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Record sended." + lastFile);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
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
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 5 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 5 Error Unknown Line. " + line);
                                }
                                return;
                            case 6:
                                if (line.Equals("OUTPUT;BEGIN"))
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Record sending.");
                                        customServiceBase.SetReg(Id, last_recordnum.ToString(), line, _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Record sended.");
                                        state = 7;
                                        break;
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
                                }
                                return;
                            case 7:
                                if (line.StartsWith("+"))
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> lines: " + line);
                                    if (CoderParse(line.Substring(1).Trim()))
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Date inserted. ");
                                        lineCounter++;
                                        break;
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 7 Error Unknown Line. " + line);
                                }
                                return;
                        }
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> Debug." + state + " / " + lineCounter + " / " + max_record_send);

                    if (state > 1)
                    {
                        if (lineCounter < max_record_send)
                        {
                            CheckEOF(_fileId, _fileName, _filePattern, _fileCreateTime);
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In timer1_Tick() -->> State 0 Error Unexpected end of stream.");
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
                L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> command: " + command);

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
                            if (line.Equals("key;NOFILE"))
                            {
                                L.Log(LogType.FILE, LogLevel.INFORM, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> State 1 There is no file.");
                                return;
                            }
                            L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> State 1 Error Unknown Line");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> State 2 Error Missing Fields. " + line);
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> State 2 Error Unknown Line. " + line);
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
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> Record sending.");
                                        customServiceBase.SetReg(Id, "0", "", newFileId + ";" + newFileName + ";" + _filePattern + ";" + newFileCreationTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = newFileId + ";" + newFileName + ";" + _filePattern + ";" +
                                                   newFileCreationTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> Record sended.");
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> Record sending Error." + exception.Message);
                                    }
                                }
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR,
                                      " NginxErrorV_1_0_0Recorder In CheckEOF() -->> State 3 Unexpected line: " + line);
                            }
                            return;
                    }
                }

            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CheckEOF() -->> Error: " + exception.Message);
            }
        } // CheckEOF

        public bool CoderParse(string line)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CoderParse() -->> Started.");

            try
            {
                var r = new Rec();
                try
                {
                    var arr = line.Split(' ');
                    var date = arr[0];
                    var time = arr[1];
                    var formatedDateTime = date + " " + time;
                    var dt = Convert.ToDateTime(formatedDateTime);
                    r.Datetime = dt.ToString(dateFormat);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + r.Datetime);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "DateTimeError: " + exception.Message);
                }

                r.LogName = "NginxErrorV_1_0_0Recorder";
                r.CustomStr10 = Environment.MachineName;

                string[] lineArr = line.Split(',');
                foreach (var s in lineArr)
                {
                    try
                    {
                        if (s.Trim().StartsWith("request:"))
                        {
                            r.EventCategory = s.Split(':')[1].Split('/')[0].Trim().Replace('"', ' ');
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory Error: " + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory ErrorLine: " + s);
                    }

                    try
                    {
                        if (s.Trim().StartsWith("host:"))
                        {
                            r.CustomStr10 = s.Split(':')[1].Replace('"', ' ').Trim();
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 Error: " + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 ErrorLine: " + s);
                    }

                    try
                    {

                        if (s.Trim().StartsWith("referrer:"))
                        {
                            r.CustomStr4 = s.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace('"', ' ').Trim();
                        }

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 Error: " + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 ErrorLine: " + s);
                    }

                    try
                    {
                        if (s.Trim().StartsWith("client:"))
                        {
                            r.CustomStr3 = s.Split(':')[1].Replace('"', ' ').Trim();
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 Error: " + exception.Message);
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 ErrorLine: " + s);
                    }
                }

                try
                {
                    var splitChars2 = new char[] { '(', ')' };
                    var tmpEventTypeArr = lineArr[0].Split(splitChars2, StringSplitOptions.RemoveEmptyEntries);
                    string tmpEventType = null;
                    if (tmpEventTypeArr.Length > 2)
                    {
                        foreach (var v in tmpEventTypeArr)
                        {
                            if (v.ToLower().Contains("directory"))
                            {
                                tmpEventType = v;
                            }
                        }
                        r.EventType = tmpEventType.Contains(":") ? tmpEventType.Split(':')[1] : tmpEventType;
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "EventType Error: " + exception.Message);
                    L.Log(LogType.FILE, LogLevel.ERROR, "EventType ErrorLine: " + line);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + r.EventCategory);
                L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + r.EventType);

                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + r.CustomStr3);
                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + r.CustomStr4);
                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + r.CustomStr10);

                if (!string.IsNullOrEmpty(remote_host))
                {
                    r.SourceName = remote_host;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + r.SourceName);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + r.CustomStr8);

                r.Description = line.Length > 899 ? line.Substring(0, 899) : line;

                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table updating.");
                    customServiceBase.SetData(Dal, virtualhost, r);
                    ++last_recordnum;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table updated.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CoderParse() -->> RemoteRecorder table update Error." + exception.Message);
                }

                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CoderParse() -->> Record sending.");
                    customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, lastFile, "", LastRecordDate);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " NginxErrorV_1_0_0Recorder In CoderParse() -->> Record sended.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " NginxErrorV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                }
                return true;
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                L.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                return true;
            }
        } // CoderParse

        public bool Set_Registry(long status)
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NginxErrorV_1_0_0Recorder");
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
                //L.SetLogFileSize(log_size);
                L.SetLogFileSize(100000000);

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
