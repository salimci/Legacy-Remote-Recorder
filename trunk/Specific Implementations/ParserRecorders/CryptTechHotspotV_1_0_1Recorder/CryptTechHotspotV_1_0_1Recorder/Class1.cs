using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;
using SharpSSH.SharpSsh;

namespace CryptTechHotspotV_1_0_1Recorder
{
    public class CryptTechHotspotV_1_0_1Recorder : CustomBase
    {
        private System.Timers.Timer _timer1;
        private int _trcLevel = 3, _timerInterval = 3000, _maxRecordSend = 100;
        private long _lastRecordnum;
        private const uint LoggingInterval = 60000;
        private const uint LogSize = 1000000;
        private string _errLog, _location, _user, _password, _remoteHost = "", _lastFile = "";
        protected bool usingRegistry = true, fromend;
        protected Int32 id;
        protected String virtualhost, dal;
        private CLogger _l;
        private string _lastRecordDate = "";

        public CryptTechHotspotV_1_0_1Recorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        public override void Init()
        {
            try
            {
                if (!GetLogDir())
                {
                    _l.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                    return;
                }
                if (!Initialize_Logger())
                {
                    _l.Log(LogType.FILE, LogLevel.ERROR,
                          "Error on Intialize Logger on CryptTechHotspotV_1_0_1Recorder Recorder  functions may not be running");
                    return;
                }
                _l.Log(LogType.FILE, LogLevel.INFORM, "Start creating CryptTechHotspotV_1_0_1Recorder DAL");
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager CryptTechHotspotV_1_0_1Recorder Recorder Init",
                                    exception.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                _timer1 = new System.Timers.Timer();
                _timer1.Elapsed += timer1_Tick;
                _timer1.Interval = _timerInterval;
                _timer1.AutoReset = false;
                _timer1.Enabled = true;
            }
            _l.Log(LogType.FILE, LogLevel.DEBUG, "  CryptTechHotspotV_1_0_1Recorder Init Method end.");
        }

        public override void SetConfigData(Int32 identity, String location, String lastLine, String lastPosition,
         String lastFile, String lastKeywords, bool fromEndOnLoss, Int32 maxLineToWait, String user,
         String password, String remoteHost, Int32 sleepTime, Int32 traceLevel,
         String customVar1, int customVar2, String dbVirtualhost, String dbDal, Int32 zone)
        {
            usingRegistry = false;
            id = identity;
            _location = location;
            fromend = fromEndOnLoss;
            _maxRecordSend = maxLineToWait;
            _timerInterval = sleepTime;
            _user = user;
            _password = password;
            _remoteHost = remoteHost;
            _trcLevel = traceLevel;
            virtualhost = dbVirtualhost;
            _lastRecordnum = Convert_To_Int64(lastPosition);
            dal = dbDal;
            _lastFile = lastFile;
        }

        private long Convert_To_Int64(string value)
        {
            long result = 0;
            if (Int64.TryParse(value, out result))
                return result;
            return 0;
        }

        public bool GetLogDir()
        {
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    if (rk != null)
                    {
                        _errLog = Path.Combine(Path.Combine((string)rk.GetValue("Home Directory"),
                                                            "log"), "CryptTechHotspotV_1_0_1Recorder" + id + ".log");
                    }
                    return true;
                }
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CryptTechHotspotV_1_0_1Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                _l.Log(LogType.FILE, LogLevel.ERROR, "Security Manager CryptTechHotspotV_1_0_1Recorder Read Registry Error. " + er.Message);
                return false;
            }
        }

        public override void Clear()
        {
            if (_timer1 != null)
                _timer1.Enabled = false;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer1.Enabled = false;
                _l.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is Started");
                var stdOut = "";
                var stdErr = "";

                if (string.IsNullOrEmpty(_remoteHost))
                {
                    _l.Log(LogType.FILE, LogLevel.ERROR,
                           " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() --> Remote host is empty");
                    return;
                }

                string host;
                int port;
                int index = _remoteHost.IndexOf(':');
                if (index >= 0 && ++index < _remoteHost.Length)
                {
                    if (int.TryParse(_remoteHost.Substring(index), out port))
                        host = _remoteHost.Substring(0, index - 1);
                    else
                    {
                        _l.Log(LogType.FILE, LogLevel.ERROR,
                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() --> Invalid Port in remote host:" +
                               _remoteHost);
                        return;
                    }
                }
                else
                {
                    host = index >= 0 ? _remoteHost.Substring(0, index - 1) : _remoteHost;
                    port = 22;
                }

                SshExec se = null;
                try
                {
                    se = new SshExec(host, _user) { Password = _password };
                    se.ConnectTimeout(int.MaxValue);
                    if (_location.EndsWith("/"))
                    {
                        _l.Log(LogType.FILE, LogLevel.DEBUG,
                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() --> Directory | " + _location);
                        se.Connect(port);

                        _l.Log(LogType.FILE, LogLevel.DEBUG,
                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() --> lastFile: " + _lastFile);

                        var linuxFileParameters = _lastFile.Trim().Split(';');

                        var fileId = "";
                        var fileCreateTime = "";
                        var fileName = "";
                        var filePattern = "";
                        if (linuxFileParameters.Length > 0)
                        {
                            fileId = linuxFileParameters[0];
                            fileName = linuxFileParameters[1];
                            filePattern = linuxFileParameters[2];
                            fileCreateTime = linuxFileParameters[3];
                        }
                        else
                        {
                            _l.Log(LogType.FILE, LogLevel.ERROR, "LastFile is unrecognized:" + _lastFile);
                            return;
                        }

                        if (fileCreateTime == "-1")
                        {
                            var dt = (long)DateTimeToUnixTimestamp(DateTime.Now);
                            fileCreateTime = dt.ToString(CultureInfo.InvariantCulture);
                        }

                        _l.Log(LogType.FILE, LogLevel.DEBUG, "_fileId: " + fileId);
                        _l.Log(LogType.FILE, LogLevel.DEBUG, "_fileName: " + fileName);
                        _l.Log(LogType.FILE, LogLevel.DEBUG, "_filePattern: " + filePattern);
                        _l.Log(LogType.FILE, LogLevel.DEBUG, "_fileCreateTime: " + fileCreateTime);

                        if (_lastRecordnum == 0)
                        {
                            _lastRecordnum = 1;
                        }

                        var command = _location + "printLog.sh" + " key " + _location + " " + fileId.Trim() + " " +
                                      fileName.Trim() + " '"
                                      + filePattern.Trim() + "' "
                                      + fileCreateTime + " "
                                      + _lastRecordnum + " " +
                                      (_lastRecordnum + _maxRecordSend);


                        se.RunCommand(command, ref stdOut, ref stdErr);
                        _l.Log(LogType.FILE, LogLevel.INFORM,
                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> SSH command : " + command);

                        var sr = new StringReader(stdOut);
                        _l.Log(LogType.FILE, LogLevel.INFORM,
                               "CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Result: " + stdOut);

                        var state = 1;
                        var lineCounter = 0;

                        var line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            switch (state)
                            {
                                case 1:
                                    _l.Log(LogType.FILE, LogLevel.INFORM,
                                           " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Start While.");
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
                                        _l.Log(LogType.FILE, LogLevel.WARN,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 1 Error There is no file.");
                                        return;
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 1 Error Unknown Line. " +
                                               line);
                                        return;
                                    }
                                    break;
                                case 2:
                                    if (line.StartsWith("FILE;"))
                                    {
                                        var lineArr = line.Split(new[] { ';' }, 4);
                                        if (lineArr.Length == 4)
                                        {
                                            fileId = lineArr[1];
                                            fileCreateTime = lineArr[2];
                                            fileName = lineArr[3];
                                            state = 3;
                                            break;
                                        }
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 2 Error Missing Fields. " +
                                               line);
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 2 Error Unknown Line. " +
                                               line);
                                    }
                                    return;
                                case 3:
                                    if (line.Equals("key;ENDS"))
                                    {
                                        try
                                        {
                                            CustomServiceBase customServiceBase =
                                                GetInstanceService("Security Manager Remote Recorder");
                                            _l.Log(LogType.FILE, LogLevel.DEBUG,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Record sending. " +
                                                   _lastFile + " / " + fileId + ";" + fileName + ";" + filePattern + ";" +
                                                   fileCreateTime);
                                            customServiceBase.SetReg(id, "0", "",
                                                                     fileId + ";" + fileName + ";" + filePattern + ";" +
                                                                     fileCreateTime, "", _lastRecordDate);
                                            _lastRecordnum = 0;
                                            _lastFile = fileId + ";" + fileName + ";" + filePattern + ";" +
                                                        fileCreateTime;
                                            _l.Log(LogType.FILE, LogLevel.DEBUG,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Record sended." +
                                                   _lastFile);
                                        }
                                        catch (Exception exception)
                                        {
                                            _l.Log(LogType.FILE, LogLevel.ERROR,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Record sending Error." +
                                                   exception.Message);
                                        }
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " +
                                               line);
                                    }
                                    return;
                                case 5:
                                    if (line.StartsWith("FILE;"))
                                    {
                                        var lineArr = line.Split(new[] { ';' }, 4);
                                        if (lineArr.Length == 4)
                                        {
                                            fileId = lineArr[1];
                                            fileCreateTime = lineArr[2];
                                            fileName = lineArr[3];
                                            state = 6;
                                            break;
                                        }
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 5 Error Missing Fields. " +
                                               line);
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 5 Error Unknown Line. " +
                                               line);
                                    }
                                    return;
                                case 6:
                                    if (line.Equals("OUTPUT;BEGIN"))
                                    {
                                        try
                                        {
                                            CustomServiceBase customServiceBase =
                                                GetInstanceService("Security Manager Remote Recorder");
                                            _l.Log(LogType.FILE, LogLevel.DEBUG,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Record sending.");
                                            customServiceBase.SetReg(id,
                                                                     _lastRecordnum.ToString(
                                                                         CultureInfo.InvariantCulture), line,
                                                                     fileId + ";" + fileName + ";" + filePattern + ";" +
                                                                     fileCreateTime, "", _lastRecordDate);
                                            _lastFile = fileId + ";" + fileName + ";" + filePattern + ";" +
                                                        fileCreateTime;
                                            _l.Log(LogType.FILE, LogLevel.DEBUG,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Record sended.");
                                            state = 7;
                                            break;
                                        }
                                        catch (Exception exception)
                                        {
                                            _l.Log(LogType.FILE, LogLevel.ERROR,
                                                   " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Record sending Error." +
                                                   exception.Message);
                                        }
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 3 Error Unknown Line. " +
                                               line);
                                    }
                                    return;
                                case 7:
                                    if (line.StartsWith("+"))
                                    {
                                        _l.Log(LogType.FILE, LogLevel.DEBUG,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> lines: " + line);
                                        if (CoderParse(line.Substring(1).Trim()))
                                        {
                                            _l.Log(LogType.FILE, LogLevel.DEBUG,
                                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Date inserted. ");
                                            lineCounter++;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR,
                                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 7 Error Unknown Line. " +
                                               line);
                                    }
                                    return;
                            }
                        }
                        _l.Log(LogType.FILE, LogLevel.DEBUG,
                               " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> Inform." + state + " / " +
                               lineCounter + " / " + _maxRecordSend);

                        if (state > 1)
                        {
                            if (lineCounter < _maxRecordSend)
                            {
                                CheckEof(se, fileId, fileName, filePattern, fileCreateTime);
                            }
                        }
                        else
                        {
                            _l.Log(LogType.FILE, LogLevel.ERROR,
                                   " CryptTechHotspotV_1_0_1Recorder In timer1_Tick() -->> State 0 Error Unexpected end of stream.");
                        }
                    }
                }
                finally
                {
                    if (se != null)
                        se.Close();
                }
            }
            catch (Exception exception)
            {
                _l.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception);
            }
            finally
            {
                _timer1.Enabled = true;
                _l.Log(LogType.FILE, LogLevel.INFORM, " timer1_Tick -->> Timer is finished.");
            }
        }

        public void CheckEof(SshExec se, string fileId, string fileName, string filePattern, string fileCreateTime)
        {
            string stdOut = "";
            string stdErr = "";
            string newFileId = "";
            string newFileName = "";
            string newFileCreationTime = "";
            try
            {
                String command = _location + "printLog.sh" + " key " + _location + " 0 - '"
                                 + filePattern.Trim() + "' "
                                 + fileCreateTime + " 1 2";
                _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> command: " + command);

                se.RunCommand(command, ref stdOut, ref stdErr);
                var sr = new StringReader(stdOut);
                var state = 1;
                var line = "";
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
                                _l.Log(LogType.FILE, LogLevel.INFORM, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> State 1 There is no file.");
                                return;
                            }
                            _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> State 1 Error Unknown Line");
                            return;
                        case 2:
                            if (line.StartsWith("FILE;"))
                            {
                                var lineArr = line.Split(new[] { ';' }, 4);
                                if (lineArr.Length == 4)
                                {
                                    newFileId = lineArr[1];
                                    newFileCreationTime = lineArr[2];
                                    newFileName = lineArr[3];
                                    state = 3;
                                    break;
                                }
                                _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> State 2 Error Missing Fields. " + line);
                            }
                            else
                            {
                                _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> State 2 Error Unknown Line. " + line);
                            }
                            return;
                        case 3:
                            if (line.Equals("key;ENDS"))
                            {
                                if (newFileId != fileId)
                                {
                                    try
                                    {
                                        CustomServiceBase customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                                        _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> Record sending.");
                                        customServiceBase.SetReg(id, "0", "", newFileId + ";" + newFileName + ";" + filePattern + ";" + newFileCreationTime, "", _lastRecordDate);
                                        _lastRecordnum = 0;
                                        _lastFile = newFileId + ";" + newFileName + ";" + filePattern + ";" +
                                                   newFileCreationTime;
                                        _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> Record sended.");
                                    }
                                    catch (Exception exception)
                                    {
                                        _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> Record sending Error." + exception.Message);
                                    }
                                }
                            }
                            else
                            {
                                _l.Log(LogType.FILE, LogLevel.ERROR,
                                      " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> State 3 Unexpected line: " + line);
                            }
                            return;
                    }
                }

            }
            catch (Exception exception)
            {
                _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CheckEOF() -->> Error: " + exception.Message);
            }
        } // CheckEOF

        static readonly Regex RegRecordOld = new Regex("^([A-Za-z]+[ ]+[0-9]+[ ]+[0-9]+:[0-9]+:[0-9]+)[ ]+clog[ ]+logger:[ ]+([0-9]+/[A-Za-z]+/[0-9]+:[0-9]+:[0-9]+:[0-9]+[ ]+[0-9+-][0-9]*)[|]([^|]*)[|]([^|]*)[|]([^|]*)[|]([^|]*)[|]([^|]*)[|]([^|]*)[|]([^|]*)", RegexOptions.Compiled);
        static readonly Regex RegRecordNew = new Regex("^([A-Za-z]+[ ]+[0-9]+[ ]+[0-9]+:[0-9]+:[0-9]+)[ ]+clog[ ]+logger:[ ]+([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+)[ ]+\\[([0-9]+/[A-Za-z]+/[0-9]+:[0-9]+:[0-9]+:[0-9]+[ ]+[^\\]]+)\\][ ]+\"([^\"]*)\"[ ]+([^ ]+)[ ]+([^ ]+)[ ]+\"([^\"]*)\"[ ]+\"([^\"]*)\"$", RegexOptions.Compiled);
        static readonly Regex RegHttpMethodNew = new Regex("^([^ ]+)[ ]+([^ ]+)[ ]+([^ ]+)", RegexOptions.Compiled);
        static readonly Regex RegHttpMethodOld = new Regex("^(.*)[ ]+([^ ]*)$", RegexOptions.Compiled);

        public bool CoderParse(string line)
        {
            _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Started.");
            try
            {
                var m = RegRecordNew.Match(line);
                var rec = new Rec
                    {
                        LogName = "CryptTechHotspotV_1_0_1Recorder",
                    };
                if (m.Success)
                    PrepareNewFormat(ref rec, line, m);
                else
                {
                    m = RegRecordOld.Match(line);
                    if (m.Success)
                        PrepareOldFormat(ref rec, line, m);
                    else
                    {
                        _l.Log(LogType.FILE, LogLevel.DEBUG,
                               " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> SKIP:[" + line + "]");
                        return true;
                    }
                }

                var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                try
                {
                    _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Sending");
                    customServiceBase.SetData(dal, virtualhost, rec);
                    _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Sent");
                }
                catch (Exception exception)
                {
                    _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Send Error." + exception.Message);
                    return false;
                }

                try
                {
                    _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Setting Reg");
                    _lastRecordDate = rec.Datetime;
                    customServiceBase.SetReg(id, _lastRecordnum.ToString(CultureInfo.InvariantCulture), line, _lastFile, "", _lastRecordDate);
                    ++_lastRecordnum;
                    _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Set Reg");
                }
                catch (Exception exception)
                {
                    _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> Set Reg Error." + exception.Message);
                }
                return true;
            }
            catch (Exception exception)
            {
                _l.Log(LogType.FILE, LogLevel.ERROR, " CryptTechHotspotV_1_0_1Recorder In CoderParse() Error." + exception);
                return false;
            }
        }

        private void PrepareOldFormat(ref Rec rec, string line, Match m)
        {
            rec.Description = line.Length > 900 ? line.Substring(0, 900) : line;

            DateTime dt;
            if (!DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yyyy:H:m:s zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yyyy:H:m:s zz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yyyy:H:m:s z", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yyyy:H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yy:H:m:s zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yy:H:m:s zz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yy:H:m:s z", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[2].Value, "d/MMM/yy:H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> DateConversionFailed:" + m.Groups[2].Value);
                dt = DateTime.Now;
            }
            rec.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var mMethod = RegHttpMethodOld.Match(m.Groups[9].Value);
            if (mMethod.Success)
            {
                rec.CustomStr2 = UrlDecode(mMethod.Groups[2].Value);
                rec.CustomStr10 = UrlDecode(mMethod.Groups[1].Value);
            }
            rec.CustomStr9 = UrlDecode(m.Groups[8].Value);
            rec.EventType = m.Groups[7].Value;
            rec.UserName = m.Groups[5].Value;
            rec.CustomStr3 = m.Groups[6].Value;
            rec.CustomStr8 = m.Groups[3].Value;
        }

        private void PrepareNewFormat(ref Rec rec, string line, Match m)
        {
            rec.Description = line.Length > 900 ? line.Substring(0, 900) : line;

            rec.CustomStr4 = UrlDecode(m.Groups[13].Value);
            rec.CustomStr3 = m.Groups[5].Value;
            rec.CustomStr8 = m.Groups[4].Value;

            var mMethod = RegHttpMethodNew.Match(m.Groups[9].Value);
            if (mMethod.Success)
            {
                rec.EventType = mMethod.Groups[1].Value;
                var address = UrlDecode(mMethod.Groups[2].Value);
                var resource = string.Empty;
                SplitUri(address, ref address, ref resource);
                rec.CustomStr9 = address;
                rec.CustomStr10 = resource;
                rec.CustomStr2 = mMethod.Groups[3].Value;
            }

            DateTime dt;
            if (!DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yyyy:H:m:s zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yyyy:H:m:s zz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yyyy:H:m:s z", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yyyy:H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yy:H:m:s zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yy:H:m:s zz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yy:H:m:s z", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
             && !DateTime.TryParseExact(m.Groups[8].Value, "d/MMM/yy:H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                _l.Log(LogType.FILE, LogLevel.DEBUG, " CryptTechHotspotV_1_0_1Recorder In CoderParse() -->> DateConversionFailed:" + m.Groups[8].Value);
                dt = DateTime.Now;
            }
            rec.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private void SplitUri(string url, ref string address, ref string resource)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                address = uri.Host;
                resource = uri.PathAndQuery;
            }
        }

        private string UrlDecode(string url)
        {
            try
            {
                return HttpUtility.UrlDecode(url);
            }
            catch
            {
            }
            return url;
        }

        public bool Set_Registry(long status)
        {
            try
            {
                using (var rk = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Natek\Security Manager\Recorder\CryptTechHotspotV_1_0_1Recorder"))
                {
                    if (rk != null)
                    {
                        rk.SetValue("LastRecordNum", status);
                        rk.Close();
                    }
                    return true;
                }
            }
            catch (Exception er)
            {
                _l.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager SQLServer Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public bool Initialize_Logger()
        {
            try
            {
                _l = new CLogger();
                switch (_trcLevel)
                {
                    case 0:
                        {
                            _l.SetLogLevel(LogLevel.NONE);
                        } break;
                    case 1:
                        {
                            _l.SetLogLevel(LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            _l.SetLogLevel(LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            _l.SetLogLevel(LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            _l.SetLogLevel(LogLevel.DEBUG);
                        } break;
                }

                _l.SetLogFile(_errLog);
                _l.SetTimerInterval(LogType.FILE, LoggingInterval);
                _l.SetLogFileSize(LogSize);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CryptTechHotspotV_1_0_1Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
