using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using CustomTools;
using Log;
using Natek.IO.Readers;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Helpers.Mapping;
using Natek.Security.Logon;

namespace Natek.Recorders.Remote
{
    public class DHCPRecorder : CustomBase
    {
        protected static readonly DataMappingInfo[] MappingInfos;

        static DHCPRecorder()
        {
            MappingInfos = new[] { CreateMappingEn(), CreateMappingEnV6(), CreateMappingTr() };
        }

        protected static object Convert2Date(RecWrapper record, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as DHCPRecorder;
            if (DateTime.TryParseExact(values[0] + " " + values[1], "M/d/y H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(values[0] + " " + values[1], "M/d/yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected static object Convert2Int64(RecWrapper record, string field, string[] values, object data)
        {
            var l = 0L;
            return Int64.TryParse(values[0], out l) ? l : 0L;
        }

        protected static object Convert2Int32(RecWrapper record, string field, string[] values, object data)
        {
            var i = 0;
            return Int32.TryParse(values[0], out i) ? i : 0;
        }

        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo()
                {
                    Mappings = new DataMapping[]
                        {
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Date"}, new string[] {"Time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"ID"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Description"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Host Name"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"IP Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"MAC Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                }
                        }
                };
        }

        protected static DataMappingInfo CreateMappingEnV6()
        {
            return new DataMappingInfo()
                {
                    Name = "v6",
                    Mappings = new DataMapping[]
                        {
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Date"}, new string[] {"Time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"ID"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Description"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Host Name"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"IPv6 Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Duid Bytes(Hex)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Duid Length"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Error Code"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                                }
                        }
                };
        }

        protected static DataMappingInfo CreateMappingTr()
        {
            return new DataMappingInfo()
                {
                    Mappings = new DataMapping[]
                        {
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Tarih"}, new string[] {"Saat"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Kimlik"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"Açiklama", "Açýklama"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping()
                                {
                                    Original =
                                        new string[][] {new string[] {"Ana Bilgisayar Adi", "Ana Bilgisayar Adý"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"IP Adresi"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping()
                                {
                                    Original = new string[][] {new string[] {"MAC Adresi"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                }
                        }
                };
        }

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_record_send = 1000000;
        double zone = 0;
        private long lastPosition, lastFileModifiedOn;
        private bool isLastFileInLocation;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, Location, lastFile;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private string _virtualHost, _dal;
        protected Int32 Id = 0;
        private CLogger L;
        private Encoding encoding = null;
        protected string filePattern;
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
                EventLog.WriteEntry("Security Manager Websense Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                base.Init();
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
                                           String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait,
                                           String user,
                                           String password, String remoteHost, Int32 SleepTime, Int32 TraceLevel,
                                           String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            this.Location = Location;
            if (MaxLineToWait > 0)
                max_record_send = MaxLineToWait;
            if (SleepTime > 0)
                timer_interval = SleepTime;
            trc_level = TraceLevel;
            lastFile = LastFile;
            lastFileModifiedOn = 0;
            if (!string.IsNullOrEmpty(LastFile))
            {
                var parts = LastFile.Split(',');
                lastFile = parts[0];
                if (parts.Length == 2)
                    long.TryParse(parts[1], out lastFileModifiedOn);
            }
            long.TryParse(LastPosition, out lastPosition);
            zone = Zone;
            _virtualHost = Virtualhost;
            _dal = dal;
            User = user;
            AccountValidator.SplitUserDomain(ref User, ref Domain);
            Password = password;
            RemoteHost = remoteHost;
            ParseParams(CustomVar1);
        }

        private void ParseParams(string argStr)
        {
            if (string.IsNullOrEmpty(argStr))
                return;

            var args = argStr.Split(new char[] { ',', ';' });
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
                            encoding = GetEncoding(value);
                            break;
                        case "FP":
                            filePattern = value;
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
                EventLog.WriteEntry("Security Manager Websense Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private bool ReadLocalRegistry()
        {
            try
            {
                using (var regManager = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager"))
                {
                    using (var regWebsense = regManager.OpenSubKey(@"Recorder\DHCPRecorder"))
                    {
                        log_size = Convert.ToUInt32(regWebsense.GetValue("Log Size"));
                        logging_interval = Convert.ToUInt32(regWebsense.GetValue("Logging Interval"));
                        trc_level = Convert.ToInt32(regWebsense.GetValue("Trace Level"));
                        timer1.Interval = Convert.ToInt32(regWebsense.GetValue("Interval"));
                        max_record_send = Convert.ToInt32(regWebsense.GetValue("MaxRecordSend"));
                        lastPosition = Convert.ToInt64(regWebsense.GetValue("LastRecordNum"));
                    }
                    using (var regAgent = regManager.OpenSubKey("Agent"))
                    {
                        err_log = Path.Combine(Path.Combine(regAgent.GetValue("Home Directory").ToString(), "log"), GetType().Name + ".log");
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
                    err_log = Path.Combine(Path.Combine(regRecorder.GetValue("Home Directory").ToString(), "log"),
                                           GetType().Name + Id + ".log");
                    var fInfo = new FileInfo(err_log);

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
            if (!reg_flag)
            {
                if (usingRegistry)
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
                          "Error on Intialize Logger on Websense Recorder functions may not be running");
                    return false;
                }

                reg_flag = true;
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
                timer1 = new System.Timers.Timer();
                timer1.Elapsed += timer1_Tick;
                timer1.Interval = timer_interval;
                timer1.AutoReset = false;
                timer1.Enabled = true;
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

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.INFORM, "Begin Processing LastFile(" + lastFile + ") LastRecord(" + lastPosition + ")");
                if (!InitializeInstance())
                    return;

                if (!GetLastFile(false))
                    return;

                PrepareEncoding(ref encoding);

                var recordSent = 0;
                Dictionary<string, int> header = null;
                var svc = GetInstanceService(usingRegistry ? "Security Manager Sender" : "Security Manager Remote Recorder");
                while (recordSent < max_record_send)
                {
                    var fInfo = new FileInfo(Path.Combine(Location, lastFile));
                    if (fInfo.Exists)
                    {
                        using (
                            var inp = new FileStream(fInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                            )
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
                                if (position > lastPosition)
                                {
                                    SetReg(Id, position.ToString(CultureInfo.InvariantCulture), string.Empty, fInfo.Name,
                                           string.Empty);
                                    lastPosition = position;
                                }
                                inp.Seek(lastPosition, SeekOrigin.Begin);
                                var line = string.Empty;
                                var reader = new BufferedLineReader(inp);
                                var nl = 0;
                                var rec = new RecWrapper();
                                while (recordSent < max_record_send &&
                                       (line = reader.ReadLine(encoding, ref nl)) != null)
                                {
                                    if (ProcessLine(headerInfo, line, rec))
                                    {
                                        rec.LogName = "DHCPRecorder";
                                        rec.CustomStr9 = headerInfo.Name;
                                        svc.SetData(_dal, _virtualHost, rec.rec);
                                        recordSent++;
                                    }
                                    svc.SetReg(Id, reader.Position.ToString(CultureInfo.InvariantCulture), string.Empty,
                                               fInfo.Name, string.Empty, rec.Datetime);
                                    lastPosition = reader.Position;
                                }
                                if (recordSent == max_record_send)
                                    return;
                            }
                        }
                    }
                    if (isLastFileInLocation)
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
                timer1.Enabled = true;
            }
        }

        private bool ProcessLine(DataMappingInfo headerInfo, string line, RecWrapper rec)
        {
            try
            {
                string[] fields;
                Exception error = null;
                if (MappedDataHelper.ProcessLine(headerInfo, line, rec, this, out fields, ref error, new[] { ',' }))
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

        private void ClearRecord(RecWrapper rec)
        {
            foreach (var pInfo in typeof(RecWrapper).GetProperties())
            {
                pInfo.SetValue(rec, pInfo.PropertyType.IsValueType ? Activator.CreateInstance(pInfo.PropertyType) : null, null);
            }
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
                var line = string.Empty;
                var lineParts = new string[2][];
                var headerPos = new long[] { 0, 0 };
                var curr = 0;
                var cnt = 0;
                var nl = 0;

                while ((line = reader.ReadLine(encoding, ref nl)) != null)
                {
                    lineParts[curr] = line.Split(',');
                    if (lineParts[curr].Length > 2) //Line must have at least 2 commas
                    {
                        headerPos[curr] = reader.Position;
                        curr ^= 1;
                        if (++cnt == 2)
                        {
                            if (lineParts[curr].Length <= lineParts[curr ^ 1].Length)
                            {
                                cnt = 0;
                                if (header == null)
                                    header = new Dictionary<string, int>();
                                else
                                    header.Clear();
                                while (cnt < lineParts[curr].Length)
                                {
                                    header[lineParts[curr][cnt].Trim()] = cnt;
                                    cnt++;
                                }
                                endOfHeader = headerPos[curr];
                                return RecordFields2Info(header);
                            }
                            cnt = 1;
                        }
                    }
                    else
                        cnt = 0;
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
                        foreach (var fieldName in fieldNames)
                        {
                            if (header.ContainsKey(fieldName))
                            {
                                ++match;
                                break;
                            }
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
            var fin = new DataMappingInfo()
            {
                Name = result.Name,
                Mappings = new DataMapping[result.Mappings.Length]
            };
            var i = 0;
            foreach (var field in result.Mappings)
            {
                fin.Mappings[i] = new DataMapping()
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

        static int CompareDhcpFiles(FileInfo l, FileInfo r)
        {
            if (l.LastWriteTimeUtc < r.LastWriteTimeUtc)
                return -1;
            if (l.LastWriteTimeUtc > r.LastWriteTimeUtc)
                return 1;
            return l.Name.CompareTo(r.Name);
        }

        private Regex GetDhcpPattern()
        {
            try
            {
                return new Regex(string.IsNullOrEmpty(filePattern) ? "^.*$" : filePattern,
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error while compiling file pattern [" + filePattern + "]: " + e.Message);
            }
            return null;
        }

        protected bool ValidateAccount(ref Exception error)
        {
            if (wiContext == null)
                wiContext = AccountValidator.ValidateAccount(Domain, User, Password, ref error);
            return wiContext != null;
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

                var regDhcp = GetDhcpPattern();
                if (regDhcp == null)
                    return false;

                var files = new List<FileInfo>();

                foreach (var f in dir.GetFileSystemInfos())
                {
                    if (regDhcp.IsMatch(f.Name) && (f.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                        files.Add(new FileInfo(f.FullName));
                }

                if (files.Count == 0)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "No dhcp file found under [" + dir.FullName + "]");
                    return false;
                }

                files.Sort(CompareDhcpFiles);

                var i = 0;
                var cmp = 1;
                var lastFileInfo = new FileInfo(Path.Combine(dir.FullName, lastFile));
                if (string.IsNullOrEmpty(lastFile))
                    cmp = -1;
                else
                {
                    if (lastFileInfo.Exists)
                    {
                        while (i < files.Count && (cmp = CompareDhcpFiles(lastFileInfo, files[i])) > 0)
                            ++i;
                    }
                    else if (lastFileModifiedOn > 0)
                    {
                        while (i < files.Count && lastFileModifiedOn > files[i].LastWriteTimeUtc.Ticks)
                            ++i;
                    }

                    if (i == files.Count)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR,
                              "All dhcp files seems old, but current does not exist:[" + lastFileInfo.FullName +
                              "]");
                        return false;
                    }
                }

                var position = 0L;
                if (cmp == 0)
                {
                    if (next)
                    {
                        if (++i >= files.Count)
                            return false;
                    }
                    else
                    {
                        if (lastFileModifiedOn > 0)
                        {
                            isLastFileInLocation = i == files.Count - 1;
                            return true;
                        }
                        position = lastPosition;
                    }
                }
                isLastFileInLocation = i == files.Count - 1;
                SetReg(Id, position.ToString(CultureInfo.InvariantCulture), string.Empty, files[i].Name + "," + files[i].LastWriteTimeUtc.Ticks, string.Empty);
                lastFile = files[i].Name;
                lastFileModifiedOn = files[i].LastWriteTimeUtc.Ticks;
                lastPosition = position;
                return true;
            }
            catch (Exception e)
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Error while getting last file:" + e);
            }
            return false;
        }
    }
}
