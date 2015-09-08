using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using SharpSSH.SharpSsh;

namespace ApacheAccessTebligatOutRecorder
{
    public class ApacheAccessTebligatOutRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 100;
        private long last_recordnum;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, location, user, password, remote_host = "", lastFile = "";
        private bool reg_flag = false;
        protected bool usingRegistry = true, fromend = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private const string LastRecordDate = "";
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        private Dictionary<int, string> types = new Dictionary<int, string>() { };
        protected SshExec se;

        public override void Init()
        {
            try
            {
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += timer1_Tick;
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ApacheAccessTebligatOutRecorder functions may not be running");
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
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on ApacheAccessTebligatOutRecorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating ApacheAccessTebligatOutRecorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager ApacheAccessTebligatOutRecorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  ApacheAccessTebligatOutRecorder Init Method end.");
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
            lastFile = LastFile;
        }

        private long Convert_To_Int64(string value)
        {
            long result;
            if (Int64.TryParse(value, out result))
                return result;
            return 0;
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Remote Recorder");
                    if (registryKey != null)
                        err_log = registryKey.GetValue("Home Directory") + @"log\ApacheAccessTebligatOutRecorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager ApacheAccessTebligatOutRecorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager ApacheAccessTebligatOutRecorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null) rk.Close();
            }
        }

        public bool Read_Registry()
        {
            L.Log(LogType.FILE, LogLevel.INFORM, " Read_Registry -->> Timer is Started");
            RegistryKey rk = null;
            try
            {
                var openSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                if (openSubKey != null)
                {
                    var registryKey = openSubKey.OpenSubKey("Natek");
                    if (registryKey != null)
                        rk = registryKey.OpenSubKey("Security Manager");
                }
                if (rk != null)
                {
                    var registryKey = rk.OpenSubKey("Recorder");
                    if (registryKey != null)
                    {
                        var subKey = registryKey.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (subKey != null)
                            log_size = Convert.ToUInt32(subKey.GetValue("Log Size"));
                    }
                    var key = rk.OpenSubKey("Recorder");
                    if (key != null)
                    {
                        var openSubKey1 = key.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (openSubKey1 != null)
                            logging_interval = Convert.ToUInt32(openSubKey1.GetValue("Logging Interval"));
                    }
                    var registryKey1 = rk.OpenSubKey("Recorder");
                    if (registryKey1 != null)
                    {
                        var subKey1 = registryKey1.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (subKey1 != null)
                            trc_level = Convert.ToInt32(subKey1.GetValue("Trace Level"));
                    }
                    var key1 = rk.OpenSubKey("Agent");
                    if (key1 != null)
                        err_log = key1.GetValue("Home Directory") + @"log\ApacheAccessTebligatOutRecorder.log";
                    var openSubKey2 = rk.OpenSubKey("Recorder");
                    if (openSubKey2 != null)
                    {
                        var registryKey2 = openSubKey2.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (registryKey2 != null)
                            timer1.Interval = Convert.ToInt32(registryKey2.GetValue("Interval"));
                    }
                    var subKey2 = rk.OpenSubKey("Recorder");
                    if (subKey2 != null)
                    {
                        var key2 = subKey2.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (key2 != null)
                            max_record_send = Convert.ToInt32(key2.GetValue("MaxRecordSend"));
                    }
                    var openSubKey3 = rk.OpenSubKey("Recorder");
                    if (openSubKey3 != null)
                    {
                        var registryKey3 = openSubKey3.OpenSubKey("ApacheAccessTebligatOutRecorder");
                        if (registryKey3 != null)
                            last_recordnum = Convert.ToInt64(registryKey3.GetValue("LastRecordNum"));
                    }

                    rk.Close();
                }
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
                    rk.Close();
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

        public static string unicodeToTrEncoding(string strIn)
        {
            var enTr = Encoding.GetEncoding("windows-1254");
            var unicode = Encoding.Unicode;
            var unicodeBytes = unicode.GetBytes(strIn);
            var trBytes = Encoding.Convert(unicode, enTr, unicodeBytes);
            var trChars = new char[enTr.GetCharCount(trBytes, 0, trBytes.Length)];
            enTr.GetChars(trBytes, 0, trBytes.Length, trChars, 0);
            var trString = new string(trChars);
            return trString;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                L.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
                var stdOut = "";
                var stdErr = "";

                string _fileId = "";
                string _filePattern = "";
                string _fileName = "";
                string _fileCreateTime = "";

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
                        var dt = (long)DateTimeToUnixTimestamp(DateTime.Now);
                        _fileCreateTime = dt.ToString(CultureInfo.InvariantCulture);
                    }

                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileId: " + _fileId);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileName: " + _fileName);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_filePattern: " + _filePattern);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "_fileCreateTime: " + _fileCreateTime);

                    if (last_recordnum == 0)
                    {
                        last_recordnum = 1;
                    }

                    var command = location + "printLog.sh" + " key " + location + " " + _fileId.Trim() + " " +
                                     _fileName.Trim() + " '"
                                     + _filePattern.Trim() + "' "
                                     + _fileCreateTime + " "
                                     + last_recordnum + " " +
                                     (last_recordnum + max_record_send);
                    
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    L.Log(LogType.FILE, LogLevel.INFORM, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> SSH command : " + command);

                    var sr = new StringReader(stdOut);
                    L.Log(LogType.FILE, LogLevel.INFORM,
                          "ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Result: " + stdOut);

                    int state = 1;
                    int lineCounter = 0;

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        switch (state)
                        {
                            case 1:
                                L.Log(LogType.FILE, LogLevel.INFORM, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Start While.");
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
                                    L.Log(LogType.FILE, LogLevel.WARN, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 1 Error There is no file.");
                                    return;
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 1 Error Unknown Line. " + line);
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
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 2 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 2 Error Unknown Line. " + line);
                                }
                                return;
                            case 3:
                                if (line.Equals("key;ENDS"))
                                {
                                    try
                                    {
                                        var customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Record sending. " + lastFile + " / " + _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime);
                                        customServiceBase.SetReg(Id, "0", "", _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Record sended." + lastFile);
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
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
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 5 Error Missing Fields. " + line);
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 5 Error Unknown Line. " + line);
                                }
                                return;
                            case 6:
                                if (line.Equals("OUTPUT;BEGIN"))
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Record sending.");
                                        customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, _fileId + ";" + _fileName + ";" + _filePattern + ";" + _fileCreateTime, "", LastRecordDate);
                                        lastFile = _fileId + ";" + _fileName + ";" + _filePattern + ";" +
                                                   _fileCreateTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Record sended.");
                                        state = 7;
                                        break;
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Record sending Error." + exception.Message);
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 3 Error Unknown Line. " + line);
                                }
                                return;
                            case 7:
                                if (line.StartsWith("+"))
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> lines: " + line);
                                    if (CoderParse(line.Substring(1).Trim()))
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Date inserted. ");
                                        lineCounter++;
                                        break;
                                    }
                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 7 Error Unknown Line. " + line);
                                }
                                return;
                        }
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> Inform." + state + " / " + lineCounter + " / " + max_record_send);

                    if (state > 1)
                    {
                        if (lineCounter < max_record_send)
                        {
                            CheckEOF(_fileId, _fileName, _filePattern, _fileCreateTime);
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In timer1_Tick() -->> State 0 Error Unexpected end of stream.");
                    }
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception);
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
                var command = location + "printLog.sh" + " key " + location + " 0 - '"
                                 + _filePattern.Trim() + "' "
                                 + _fileCreateTime + " 1 2";
                L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> command: " + command);

                se.RunCommand(command, ref stdOut, ref stdErr);
                var sr = new StringReader(stdOut);
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
                                L.Log(LogType.FILE, LogLevel.INFORM, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> State 1 There is no file.");
                                return;
                            }
                            L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> State 1 Error Unknown Line");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> State 2 Error Missing Fields. " + line);
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> State 2 Error Unknown Line. " + line);
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
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> Record sending.");
                                        customServiceBase.SetReg(Id, "0", "", newFileId + ";" + newFileName + ";" + _filePattern + ";" + newFileCreationTime, "", LastRecordDate);
                                        last_recordnum = 0;
                                        lastFile = newFileId + ";" + newFileName + ";" + _filePattern + ";" +
                                                   newFileCreationTime;
                                        L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> Record sended.");
                                    }
                                    catch (Exception exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> Record sending Error." + exception.Message);
                                    }
                                }
                            }
                            else
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR,
                                      " ApacheAccessTebligatOutRecorder In CheckEOF() -->> State 3 Unexpected line: " + line);
                            }
                            return;
                    }
                }

            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CheckEOF() -->> Error: " + exception.Message);
            }
        } // CheckEOF

        public bool CoderParse(string line)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Started. Line: " + line);
            try
            {
                var r = new Rec();
                var lineArr = SpaceSplit(line, true);
                r.LogName = "ApacheAccessTebligatOutRecorder";

                try
                {
                    var datePart = lineArr[0];
                    var timePart = lineArr[1];

                    var dateString = datePart + " " + timePart;
                    var dt = Convert.ToDateTime(dateString);
                    var myDateString = dt.ToString(dateFormat);
                    r.Datetime = myDateString;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Datetime: " + r.Datetime);
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Datetime conversation error. " + exception.Message);
                }

                r.Description = line.Length > 899 ? line.Substring(0, 899) : line;
                r.Description = ConvertUTF8(r.Description);


                try
                {
                    if (lineArr.Length > 2)
                    {
                        r.EventType = lineArr[2];
                    }

                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        if (lineArr[i] == "IP:")
                            if (lineArr.Length > (i + 1))
                            {
                                r.ComputerName = lineArr[i + 1];
                            }
                        if (lineArr.Length > (i + 1))
                            if (lineArr[i] == "SICIL:")
                            {
                                r.CustomStr1 = lineArr[i + 1];
                            }

                        if (lineArr.Length > (i + 1))
                            if (lineArr[i] == "SIFRE:")
                            {
                                r.CustomStr2 = lineArr[i + 1];
                            }
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Parse Error. " + exception.Message);

                }

                var customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> RemoteRecorder table updating.");
                    customServiceBase.SetData(Dal, virtualhost, r);
                    ++last_recordnum;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> RemoteRecorder table updated.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CoderParse() -->> RemoteRecorder table update Error." + exception.Message);
                }

                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Record sending.");
                    customServiceBase.SetReg(Id, last_recordnum.ToString(CultureInfo.InvariantCulture), line, lastFile, "", LastRecordDate);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Record sended.");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CoderParse() -->> Record sending Error." + exception.Message);
                }
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " ApacheAccessTebligatOutRecorder In CoderParse() Error." + exception.Message);
                return false;
            }
        } // CoderParse

        public string ConvertUTF8(string myString)
        {
            try
            {
                var utf8 = Encoding.UTF8;
                byte[] utfBytes = utf8.GetBytes(myString);
                myString = utf8.GetString(utfBytes, 0, utfBytes.Length);
            }
            catch (Exception)
            {
                return myString;
            }
            return myString;
        } // ConvertUTF8

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
            var lst = new List<String>();
            var sb = new StringBuilder();
            bool space = false;
            foreach (var c in line.ToCharArray())
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
                var registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE");
                if (registryKey != null)
                {
                    var subKey = registryKey.CreateSubKey("Natek");
                    if (subKey != null)
                    {
                        var key = subKey.CreateSubKey("Security Manager");
                        if (key != null)
                        {
                            var registryKey1 = key.CreateSubKey("Recorder");
                            if (registryKey1 != null)
                                rk = registryKey1.CreateSubKey("ApacheAccessTebligatOutRecorder");
                        }
                    }
                }
                if (rk != null)
                {
                    rk.SetValue("LastRecordNum", status);
                    rk.Close();
                }
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
                    rk.Close();
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
