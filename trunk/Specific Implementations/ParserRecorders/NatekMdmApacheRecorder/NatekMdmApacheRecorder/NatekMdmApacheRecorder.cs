using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using CustomTools;
using Microsoft.Win32;
using System;
using Log;
using SharpSSH.SharpSsh;

namespace Natek.Recorders.Remote
{
    public class NatekMdmApacheRecorder : CustomBase
    {
        private readonly object _sync;
        private int _status;
        private int _identity;
        private string _location;
        private int _lastPosition;
        private string _lastFile;
        private int _maxLineToWait;
        private string _user;
        private string _password;
        private string _remoteHost;
        private int _traceLevel;
        private string _customVar1;
        private int _customVar2;
        private string _virtualhost;
        private string _dal;
        private int _zone;
        private CLogger _logger;
        private System.Timers.Timer _timer;
        private Regex _filePattern;

        public NatekMdmApacheRecorder()
        {
            _sync = new object();
            _status = 0;

            _timer = new System.Timers.Timer()
            {
                AutoReset = false,
                Interval = 60000
            };
            _timer.Elapsed += new ElapsedEventHandler(ProcessTimerElapsed);
        }

        void ProcessTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_sync)
            {
                while ((_status & 2) == 1)
                {
                    Monitor.Wait(_sync);
                }
                _status |= 1;
            }
            try
            {
                if (!Initialize())
                {
                    return;
                }

                ProcessLogs();
            }
            finally
            {
                lock (_sync)
                {
                    _status ^= 1;
                    try
                    {
                        Monitor.PulseAll(_sync);
                    }
                    finally
                    {
                        _timer.Enabled = true;
                    }
                }
            }
        }

        protected void ProcessLogs()
        {
            SshExec ssh = null;
            try
            {
                ssh = ConnectRemoteHost();
                if (ssh == null)
                    return;

                var ts = DateTime.Now.Ticks;
                var stdOut = string.Empty;
                var stdErr = string.Empty;
                var beginKeyword = "BEGIN " + ts;
                var endKeyword = "END " + ts;

                ssh.RunCommand("echo " + beginKeyword + " && ls -1tci " + _location + " && echo " + endKeyword, ref stdOut, ref stdErr);
                using (var reader = new StringReader(stdOut))
                {
                    var line = "";
                    string file;
                    string inode;

                    string inputFile = null;
                    string inputInode = null;

                    ExtractFileInode(out file, out inode);
                    if ((line = reader.ReadLine()) != null)
                    {
                        if (!line.Equals(beginKeyword))
                        {
                            _logger.Log(LogType.FILE, LogLevel.ERROR,
                                        "Error ProcessLogs: Found(" + line + ") Expected(" + beginKeyword + ")");
                            return;
                        }
                        while ((line = reader.ReadLine()) != null && line != endKeyword)
                        {
                            var info = line.Split(new char[] { ' ' }, 2);
                            if (info.Length != 2)
                            {
                                _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessLogs: Unexpected file info(" + line + ") Expected(<inode> <filename>)");
                                return;
                            }

                            if (info[0] == inode || info[1] == file)
                            {
                                inputInode = info[0];
                                inputFile = info[1];
                                break;
                            }
                            if (_filePattern.IsMatch(info[1]))
                            {
                                inputInode = info[0];
                                inputFile = info[1];
                            }

                        }
                        if (inputInode != null)
                        {
                            if (inputInode != inode || inputFile != file)
                            {
                                GetInstanceService("Security Manager Remote Recorder").SetReg(_identity, "0", string.Empty, inputFile + "," + inputInode, string.Empty);
                                _lastFile = inputFile + "," + inputInode;
                                _lastPosition = 0;
                            }
                            ProcessRemoteFile(ssh, inputFile);
                        }
                        else
                        {
                            _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessLogs: No remote file could be found");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessLogs: " + e.Message);
            }
            finally
            {
                if (ssh != null)
                {
                    try
                    {
                        ssh.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void ProcessRemoteFile(SshExec ssh, string filename)
        {
            var proc = GetProcessor();
            if (!filename.Contains("/"))
                filename = _location + "/" + filename;
            string stdout = null;
            string stderr = null;

            var key = DateTime.Now.Ticks + "@" + new Random((int)DateTime.Now.Ticks).Next();
            var command = _maxLineToWait > 0
                              ? proc + " \"" + filename + "\"|sed -n " + (_lastPosition + 1) + "," +
                                (_lastPosition + _maxLineToWait) + "p"
                              : proc + " \"" + filename + "\"";
            ssh.RunCommand("echo BEGIN " + key + " && (" + command + ") && echo END " + key, ref stdout, ref stderr);

            using (var reader = new StringReader(stdout))
            {
                var line = "";
                if ((line = reader.ReadLine()) != null && line == "BEGIN " + key)
                {
                    key = "END " + key;
                    var record = new List<string>();
                    while ((line = reader.ReadLine()) != null && line != key)
                    {
                        if (!ProcessRecord(line, record))
                            break;
                    }
                }
            }
        }

        private bool ProcessRecord(string line, List<string> record)
        {
            try
            {
                GetRecord(line, record);
                var rec = new Rec()
                {
                    LogName = "NatekMdmApacheRecorder",
                    ComputerName = GetField(record, 0),
                    Datetime = GetDate(GetField(record, 3)),
                    CustomStr1 = GetField(record, 4),
                    CustomStr2 = GetField(record, 5),
                    CustomStr3 = GetField(record, 6),
                    Description = line
                };
                try
                {
                    var sender = GetInstanceService("Security Manager Remote Recorder");
                    sender.SetData(_dal, _virtualhost, rec);
                    sender.SetReg(_identity, (_lastPosition + 1).ToString(), line, _lastFile, string.Empty, rec.Datetime);
                }
                catch (Exception ie)
                {
                    _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessRecord Inner: [" + line + "] => " + ie.Message);
                    return false;
                }
                ++_lastPosition;
            }
            catch (Exception e)
            {
                _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessRecord: [" + line + "] => " + e.Message);
            }
            return true;
        }

        private string GetDate(string strDate)
        {
            DateTime d;
            if (!DateTime.TryParseExact(strDate, "d/MMM/yyyy:H:m:s zzz",
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                d = DateTime.Now;
            }
            return d.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private string GetField(List<string> record, int index)
        {
            return record.Count > index ? record[index] : string.Empty;
        }

        private static Regex REG_FIELD = new Regex("\"[^\"]*\"|[\\[][^\\[]*[\\]]|[^ \t]+");
        private List<string> GetRecord(string line, List<string> record)
        {
            record.Clear();
            var m = REG_FIELD.Match(line);
            while (m.Success)
            {
                record.Add(m.Groups[0].Value.StartsWith("\"") || m.Groups[0].Value.StartsWith("[")
                    ? m.Groups[0].Value.Substring(1, m.Groups[0].Value.Length - 2)
                    : m.Groups[0].Value);
                m = m.NextMatch();
            }
            return record;
        }

        private string GetProcessor()
        {
            var index = _lastFile.LastIndexOf(".");
            if (index >= 0)
            {
                var ext = _lastFile.Substring(index + 1).ToLower();
                switch (ext)
                {
                    case "gz":
                        return "gunzip -c";
                    case "bz2":
                        return "bzcat";
                }
            }
            return "cat";
        }

        private void ExtractFileInode(out string file, out string inode)
        {
            if (string.IsNullOrEmpty(_lastFile))
            {
                file = string.Empty;
                inode = "0";
            }
            else
            {
                var index = _lastFile.IndexOf(',');
                if (index >= 0)
                {
                    file = _lastFile.Substring(0, index);
                    inode = _lastFile.Substring(index + 1);
                }
                else
                {
                    file = _lastFile;
                    inode = "0";
                }
            }
        }

        private SshExec ConnectRemoteHost()
        {
            if (string.IsNullOrEmpty(_remoteHost))
            {
                _logger.Log(LogType.FILE, LogLevel.ERROR, "Error ProcessLogs: No remote host defined");
                return null;
            }
            var address = _remoteHost.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            int port;
            if (address.Length != 2 || int.TryParse(address[1], out port))
                port = 0;

            var ssh = new SshExec(address[0], _user);
            if (!string.IsNullOrEmpty(_password))
                ssh.Password = _password;
            if (port > 0)
                ssh.Connect(port);
            else
                ssh.Connect();
            return ssh;
        }

        public override void SetConfigData(int identity, string location, string lastLine, string lastPosition,
            string lastFile, string lastKeywords, bool fromEndOnLoss, int maxLineToWait,
            string user, string password, string remoteHost, int sleepTime, int traceLevel,
            string customVar1, int customVar2, string virtualhost, string dal, int zone)
        {
            _identity = identity;
            if (!int.TryParse(lastPosition, out _lastPosition))
                _lastPosition = 0;
            _location = location;
            _lastFile = lastFile;
            _maxLineToWait = maxLineToWait;
            _user = user;
            _password = password;
            _remoteHost = remoteHost;

            _timer.Interval = sleepTime < 1000 ? 1000 : sleepTime;
            _traceLevel = traceLevel;
            _customVar1 = customVar1;
            _customVar2 = customVar2;
            _virtualhost = virtualhost;
            _dal = dal;
            _zone = zone;
        }

        public string ReadLogFilename()
        {
            try
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    return Path.Combine(Path.Combine(reg.GetValue("Home Directory").ToString(), "log"), "NatekMdmApacheRecorder" + _identity + ".log");
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Security Manager NatekMdmApacheRecorder Read Registry", e.ToString(), EventLogEntryType.Error);
            }
            return null;
        }

        public override void Start()
        {
            ProcessTimerElapsed(null, null);
        }

        static LogLevel ToLogLevel(int level, LogLevel defaultLevel)
        {
            try
            {
                return (LogLevel)level;
            }
            catch
            {
            }
            return defaultLevel;
        }

        protected bool Initialize()
        {
            lock (_sync)
            {
                if ((_status & 2) == 2)
                    return true;
            }
            try
            {
                var logfile = ReadLogFilename();
                if (logfile != null)
                {
                    _logger = new CLogger();
                    _logger.SetLogFile(logfile);
                    _logger.SetLogLevel(ToLogLevel(_traceLevel, LogLevel.ERROR));

                    ProcessArgs();

                    lock (_sync)
                    {
                        _status |= 2;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Security Manager NatekMdmApacheRecorder Read Registry", e.ToString(), EventLogEntryType.Error);
            }
            return false;
        }

        private void ProcessArgs()
        {
            var args = SplitArgs(_customVar1);
            string v;

            if (args.TryGetValue("r", out v) && v.Length > 0)
            {
                if (!v.StartsWith("^"))
                    v = "^" + v;
                if (!v.EndsWith("$"))
                    v += "$";
            }
            else
                v = "^.*$";
            _filePattern = new Regex(v, RegexOptions.Compiled);
        }

        private Dictionary<string, string> SplitArgs(string dictionaryArgs)
        {
            var args = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(dictionaryArgs))
            {
                var vals = dictionaryArgs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var val in vals)
                {
                    var index = val.IndexOf('=');
                    if (index >= 0)
                    {
                        args[val.Substring(0, index).Trim().ToLower()] = val.Substring(index + 1).Trim();
                    }
                }
            }
            return args;
        }
    }
}
