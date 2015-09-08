using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Win32;
using CustomTools;
using Log;
using Natek.IO.Readers;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Helpers.Mapping;
using Natek.Security.Logon;

namespace Natek.Recorders.Remote
{
    public class IISUnifiedRecorder : CustomBase
    {
        protected static readonly DataMappingInfo[] MappingInfos;

        static IISUnifiedRecorder()
        {
            MappingInfos = new[] { CreateMappingEn() };
        }

        protected static object Convert2Date(RecWrapper record, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as IISUnifiedRecorder;
            if (DateTime.TryParseExact(values[0] + " " + values[1], "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
            || DateTime.TryParseExact(values[0] + " " + values[1], "y-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected static object Convert2Int64(RecWrapper record, string field, string[] values, object data)
        {
            long l;
            return Int64.TryParse(values[0], out l) ? l : 0L;
        }

        protected static object Convert2Int32(RecWrapper record, string field, string[] values, object data)
        {
            int i;
            return Int32.TryParse(values[0], out i) ? i : 0;
        }

        protected static object VersionConcat(RecWrapper record, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
                return UrlDecode(values[1]);
            if (string.IsNullOrEmpty(values[1]))
                return UrlDecode(values[0]);
            return UrlDecode(values[0] + " " + values[1]);
        }

        protected static object DescriptionSplitter(RecWrapper record, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
            {
                record.CustomStr1 = string.Empty;
                return string.Empty;
            }

            var uriStr = UrlDecode(values[0]);
            var parts = uriStr.Split(new[] { ';' }, 2);
            if (parts.Length == 2)
                record.CustomStr1 = StringHelper.MakeSureLength(parts[1], 900);

            return StringHelper.MakeSureLength(parts[0], 900);
        }

        protected static string UrlDecode(string url)
        {
            try
            {
                return HttpUtility.UrlDecode(url);
            }
            catch
            {
                return url;
            }
        }

        protected static object CustomStr1Splitter(RecWrapper record, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
                return values[0];

            var uriStr = UrlDecode(values[0]);
            if (uriStr.Length > 1800)
                record.CustomStr10 = uriStr.Substring(900, 900);
            else if (uriStr.Length > 900)
                record.CustomStr10 = uriStr.Substring(900, uriStr.Length - 900);
            return StringHelper.MakeSureLength(uriStr, 900);
        }

        protected static object CustomStr5Splitter(RecWrapper record, string field, string[] values, object data)
        {
            return StringHelper.MakeSureLength(values[0], 900);
        }

        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
                {
                    Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new [] {new [] {"date"}, new [] {"time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-sitename"}},
                                    MappedField = typeof (RecWrapper).GetProperty("SourceName")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-method"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventType")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-uri-stem"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description"),
                                    MethodInfo = DescriptionSplitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-uri-query"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                                    MethodInfo = CustomStr1Splitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-username"}},
                                    MappedField = typeof (RecWrapper).GetProperty("UserName")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"c-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-substatus"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-win32-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-port"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-version"},new [] {"cs(User-Agent)"} },
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                                    MethodInfo = VersionConcat,
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs(Referer)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr5"),
                                    MethodInfo = CustomStr5Splitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-bytes"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs(Cookie)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-host"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-bytes"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                                    MethodInfo = Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"time-taken"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                                    MethodInfo = Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"s-computername"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                        }
                };
        }

        protected System.Timers.Timer ProcessDataTimer;
        protected int TrcLevel = 3;
        protected int TimerInterval = 3000;
        protected int MaxRecordSend = 1000000;
        protected double Zone;
        protected long LastPosition;
        protected bool IsLastFileInLocation;
        protected uint LoggingInterval = 60000;
        protected uint LogSize = 1000000;
        protected string ErrLog;
        protected string Location;
        protected string LastFile;
        protected bool RegFlag;
        protected bool UsingRegistry = true;
        protected string VirtualHost;
        protected string Dal;
        protected int Id;
        protected CLogger L;
        protected Encoding Encoding;
        protected string FilePattern;
        protected string IisType;
        protected string RemoteHost;
        protected string Domain;
        protected string User;
        protected string Password;
        protected WindowsImpersonationContext wiContext;

        public override void Init()
        {
            try
            {
                InitializeInstance();
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IISUnified Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                base.Init();
            }
        }

        public override void SetConfigData(Int32 identity, String location, String lastLine, String lastPosition,
                                           String lastFile, String lastKeywords, bool fromEndOnLoss, Int32 maxLineToWait,
                                           String user,
                                           String password, String remoteHost, Int32 sleepTime, Int32 traceLevel,
                                           String customVar1, int customVar2, String virtualhost, String dal, Int32 zone)
        {
            UsingRegistry = false;
            Id = identity;
            Location = location;
            if (maxLineToWait > 0)
                MaxRecordSend = maxLineToWait;
            if (sleepTime > 0)
                TimerInterval = sleepTime;
            TrcLevel = traceLevel;
            LastFile = lastFile;
            long.TryParse(lastPosition, out LastPosition);
            Zone = zone;
            VirtualHost = virtualhost;
            Dal = dal;
            User = user;
            AccountValidator.SplitUserDomain(ref User, ref Domain);
            Password = password;
            RemoteHost = remoteHost;
            ParseParams(customVar1);
        }

        private void ParseParams(string argStr)
        {
            if (string.IsNullOrEmpty(argStr))
                return;

            var args = argStr.Split(new[] { ',', ';' });
            foreach (var arg in args)
            {
                var i = arg.IndexOf('=');
                if (i > 0)
                {
                    var key = arg.Substring(0, i++).Trim();
                    var value = i < arg.Length ? arg.Substring(i).Trim() : null;
                    switch (key)
                    {
                        case "E":
                            Encoding = GetEncoding(value);
                            break;
                        case "FP":
                            FilePattern = value;
                            break;
                        case "T":
                            IisType = value;
                            break;
                    }
                }
            }
        }

        private Encoding GetEncoding(string encoding)
        {
            if (!string.IsNullOrEmpty(encoding))
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch
                {
                }
            }
            return null;
        }

        protected bool InitializeLogger()
        {
            try
            {
                L = new CLogger();
                switch (TrcLevel)
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
                L.SetLogFile(ErrLog);
                L.SetTimerInterval(LogType.FILE, LoggingInterval);
                L.SetLogFileSize(LogSize);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IISUnified Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private bool ReadLocalRegistry()
        {
            try
            {
                using (var regManager = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager"))
                {
                    if (regManager != null)
                    {
                        RegistryKey regIisUnififed;
                        using (regIisUnififed = regManager.OpenSubKey(@"Recorder\IISUnifiedRecorder"))
                        {
                            if (regIisUnififed != null)
                            {
                                LogSize = Convert.ToUInt32(regIisUnififed.GetValue("Log Size"));
                                LoggingInterval = Convert.ToUInt32(regIisUnififed.GetValue("Logging Interval"));
                                TrcLevel = Convert.ToInt32(regIisUnififed.GetValue("Trace Level"));
                                ProcessDataTimer.Interval = Convert.ToInt32(regIisUnififed.GetValue("Interval"));
                                MaxRecordSend = Convert.ToInt32(regIisUnififed.GetValue("MaxRecordSend"));
                                LastPosition = Convert.ToInt64(regIisUnififed.GetValue("LastRecordNum"));
                            }
                        }
                        using (var regAgent = regManager.OpenSubKey("Agent"))
                        {
                            if (regAgent != null)
                                ErrLog = regAgent.GetValue("Home Directory") + @"log\" + GetType().Name + ".log";
                        }
                    }
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager DHCP Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        protected bool GetLogDir()
        {
            try
            {
                using (
                    var regRecorder =
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    if (regRecorder != null)
                        ErrLog = regRecorder.GetValue("Home Directory") + @"log\" + GetType().Name + Id + ".log";

                    var fInfo = new FileInfo(ErrLog);

                    if (fInfo.Directory != null && !fInfo.Directory.Exists)
                        fInfo.Directory.Create();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager DHCP Recorder Read Registry", er.ToString(),
                                    EventLogEntryType.Error);
                return false;
            }
        }

        private bool InitializeInstance()
        {
            if (!RegFlag)
            {
                if (UsingRegistry)
                {
                    if (!ReadLocalRegistry())
                    {
                        L.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error on Reading the Registry ");
                        return false;
                    }
                }
                else
                {
                    if (!GetLogDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return false;
                    }
                }

                if (!InitializeLogger())
                {
                    L.Log(LogType.EVENTLOG, LogLevel.ERROR,
                          "Error on Intialize Logger on IISUnified Recorder functions may not be running");
                    return false;
                }

                RegFlag = true;
            }
            return true;
        }

        public override void Start()
        {
            try
            {
                base.Start();
            }
            finally
            {
                ProcessDataTimer = new System.Timers.Timer();
                ProcessDataTimer.Elapsed += ProcessDataTimerTick;
                ProcessDataTimer.Interval = TimerInterval;
                ProcessDataTimer.AutoReset = false;
                ProcessDataTimer.Enabled = true;
            }
        }

        private void ProcessDataTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Begin Processing lastFile(" + LastFile + ") LastRecord(" + LastPosition + ")");
                if (!InitializeInstance())
                    return;

                if (!GetLastFile(false))
                    return;

                PrepareEncoding(ref Encoding);

                var recordSent = 0;
                Dictionary<string, int> header = null;
                var svc = GetInstanceService(UsingRegistry ? "Security Manager Sender" : "Security Manager Remote Recorder");
                while (recordSent < MaxRecordSend)
                {
                    var fInfo = new FileInfo(Path.Combine(Location, LastFile));
                    if (fInfo.Exists)
                    {
                        using (var inp = new FileStream(fInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            long position = 0;

                            var headerInfo = GetHeaderInfo(inp, ref header, ref position);
                            if (headerInfo == null)
                            {
                                L.Log(LogType.FILE, LogLevel.WARN, fInfo.FullName + " has no header");
                            }
                            else
                            {
                                WarnForMissingHeaders(fInfo, headerInfo, header);
                                if (position > LastPosition)
                                {
                                    SetReg(Id, position.ToString(CultureInfo.InvariantCulture), string.Empty, fInfo.Name, string.Empty);
                                    LastPosition = position;
                                }
                                inp.Seek(LastPosition, SeekOrigin.Begin);
                                string line;
                                var reader = new BufferedLineReader(inp);
                                var nl = 0;
                                var rec = new RecWrapper();
                                while (recordSent < MaxRecordSend && (line = reader.ReadLine(Encoding, ref nl)) != null)
                                {
                                    if (ProcessLine(headerInfo, line, rec))
                                    {
                                        rec.LogName = "IISUnifiedRecorder";
                                        if (string.IsNullOrEmpty(rec.ComputerName))
                                            rec.ComputerName = UsingRegistry ? rec.CustomStr4 : RemoteHost;
                                        if (string.IsNullOrEmpty(rec.CustomStr10))
                                            rec.CustomStr10 = IisType;
                                        svc.SetData(Dal, VirtualHost, rec.rec);
                                        recordSent++;
                                    }
                                    svc.SetReg(Id, reader.Position.ToString(CultureInfo.InvariantCulture), string.Empty, fInfo.Name, string.Empty, rec.Datetime);
                                    LastPosition = reader.Position;
                                }
                                if (recordSent == MaxRecordSend)
                                    return;
                            }
                        }
                    }
                    if (IsLastFileInLocation)
                        return;
                    if (!GetLastFile(true))
                        return;
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error in timer handler:" + ex);
            }
            finally
            {
                ProcessDataTimer.Enabled = true;
            }
        }

        private void PrepareEncoding(ref Encoding encoding)
        {
            if (encoding == null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(1254);
                }
                catch (Exception e)
                {
                    encoding = Encoding.UTF8;
                    L.Log(LogType.FILE, LogLevel.ERROR, "Getting default Encoding Windows-1254 failed:" + e.Message + ". Switch to UTF8");
                }
            }
        }

        bool ProcessLine(DataMappingInfo headerInfo, string line, RecWrapper rec)
        {
            try
            {
                string[] fields;
                Exception error = null;
                if (MappedDataHelper.ProcessLine(headerInfo, line, rec, this, out fields, ref error, new[] { ' ' }))
                {
                    if (string.IsNullOrEmpty(rec.Datetime))
                        rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    return true;
                }
                throw error ?? new Exception("Unknown error in data helper process line");
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error while processing line:[" + line + "]:" + e);
            }
            return false;
        }



        private void WarnForMissingHeaders(FileInfo sourceFile, DataMappingInfo headerInfo, Dictionary<string, int> header)
        {
            foreach (var mapping in headerInfo.Mappings)
            {
                foreach (var originals in mapping.Original)
                {
                    var i = 0;
                    for (; i < originals.Length; i++)
                        if (header.ContainsKey(originals[i]))
                            break;
                    if (i == originals.Length)
                    {
                        L.Log(LogType.FILE, LogLevel.WARN, "File does not have record mapping field. Please send this line back to programmer if could be added:["
                            + sourceFile.FullName + "->" + PrepareField(originals) + "]");
                    }
                }
            }
        }

        private string PrepareField(string[] originals)
        {
            if (originals == null || originals.Length == 0)
                return null;

            var f = originals[0];
            for (var i = 1; i < originals.Length; i++)
                f += "," + originals[i];
            return f;
        }

        private DataMappingInfo GetHeaderInfo(FileStream inp, ref Dictionary<string, int> header, ref long endOfHeader)
        {
            var offset = -1L;
            try
            {
                offset = inp.Position;
                var reader = new BufferedLineReader(inp);
                string line;
                var nl = 0;

                while ((line = reader.ReadLine(Encoding, ref nl)) != null)
                {
                    if (line.StartsWith("#Fields: "))
                    {
                        if (header == null)
                            header = new Dictionary<string, int>();
                        else
                            header.Clear();
                        var lineParts = line.Substring(8).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var cnt = 0;
                        while (cnt < lineParts.Length)
                        {
                            header[lineParts[cnt]] = cnt;
                            cnt++;
                        }
                        endOfHeader = reader.Position;
                        return RecordFields2Info(header);
                    }
                }
                return null;
            }
            finally
            {
                if (offset >= 0)
                    inp.Seek(offset, SeekOrigin.Begin);
            }
        }


        DataMappingInfo GetBestMatch(Dictionary<string, int> header)
        {
            DataMappingInfo result = null;
            var maxMatch = 0.0D;
            foreach (var info in MappingInfos)
            {
                var match = 0.0D;
                var fieldCount = 0;
                foreach (var field in info.Mappings)
                {
                    foreach (var fieldNames in field.Original)
                    {
                        fieldCount++;
                        if (fieldNames.Any(header.ContainsKey))
                        {
                            ++match;
                        }
                    }
                }
                if (match / fieldCount > maxMatch)
                {
                    maxMatch = match / fieldCount;
                    result = info;
                }
            }
            return result;
        }

        private DataMappingInfo RecordFields2Info(Dictionary<string, int> header)
        {
            var result = GetBestMatch(header);
            if (result == null)
                return null;
            return BestMatch2Info(result, header);
        }

        private DataMappingInfo BestMatch2Info(DataMappingInfo result, Dictionary<string, int> header)
        {
            var fin = new DataMappingInfo
                {
                    Name = result.Name,
                    Mappings = new DataMapping[result.Mappings.Length]
                };
            var i = 0;
            foreach (var field in result.Mappings)
            {
                fin.Mappings[i] = new DataMapping
                    {
                        MappedField = result.Mappings[i].MappedField,
                        MethodInfo = result.Mappings[i].MethodInfo,
                        Original = result.Mappings[i].Original,
                        SourceIndex = new int[result.Mappings[i].Original.Length],
                        SourceValues = new string[result.Mappings[i].Original.Length]
                    };
                var origIndex = 0;
                foreach (var fieldNames in field.Original)
                {
                    foreach (var fieldName in fieldNames)
                    {
                        fin.Mappings[i].SourceIndex[origIndex] = -1;
                        if (header.ContainsKey(fieldName))
                        {
                            fin.Mappings[i].SourceIndex[origIndex++] = header[fieldName];
                            break;
                        }
                    }
                }
                ++i;
            }
            return fin;
        }

        static int CompareIisFiles(FileInfo l, FileInfo r)
        {
            return String.Compare(l.Name, r.Name, StringComparison.Ordinal);
        }

        private bool GetLastFile(bool next)
        {
            try
            {
                Exception error = null;
                if ((!string.IsNullOrEmpty(RemoteHost) || !string.IsNullOrEmpty(Location) && Location.StartsWith(@"\\"))
                    && !ValidateAccount(ref error))
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Validate account failed for remote host [" + RemoteHost + "] for user [" + User + "]: " + (error == null ? " Unknown error, no error reported back" : error.Message));
                    return false;
                }
                var dir = new DirectoryInfo(Location);
                if (!dir.Exists)
                    return false;

                var regIis = GetIisPattern();
                if (regIis == null)
                    return false;

                var files = (from f in dir.GetFileSystemInfos()
                             where regIis.IsMatch(f.Name) && (f.Attributes & FileAttributes.Directory) != FileAttributes.Directory
                             select new FileInfo(f.FullName)).ToList();

                if (files.Count == 0)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "No iis log file found under [" + dir.FullName + "]");
                    return false;
                }

                files.Sort(CompareIisFiles);

                var i = 0;
                var cmp = 1;
                var lastFileInfo = new FileInfo(Path.Combine(dir.FullName, LastFile));
                if (string.IsNullOrEmpty(LastFile))
                    cmp = -1;
                else
                {
                    while (i < files.Count && (cmp = CompareIisFiles(lastFileInfo, files[i])) > 0)
                        ++i;
                    if (i == files.Count)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              "All iis files seems old, but current does not exist:[" + lastFileInfo.FullName +
                              "]");
                        return false;
                    }
                }

                if (cmp == 0)
                {
                    if (next)
                    {
                        if (++i >= files.Count)
                            return false;
                    }
                    else
                    {
                        IsLastFileInLocation = i == files.Count - 1;
                        return true;
                    }
                }
                IsLastFileInLocation = i == files.Count - 1;
                SetReg(Id, "0", string.Empty, files[i].Name + "," + files[i].LastWriteTimeUtc.Ticks, string.Empty);
                LastFile = files[i].Name;
                LastPosition = 0;
                return true;
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Error while getting last file:" + e);
            }
            return false;
        }

        protected bool ValidateAccount(ref Exception error)
        {
            if (wiContext == null)
                wiContext = AccountValidator.ValidateAccount(Domain, User, Password, ref error);
            return wiContext != null;
        }

        private Regex GetIisPattern()
        {
            try
            {
                return new Regex(string.IsNullOrEmpty(FilePattern) ? "^.*$" : FilePattern,
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error while compiling file pattern [" + FilePattern + "]: " + e.Message);
            }
            return null;
        }
    }
}
