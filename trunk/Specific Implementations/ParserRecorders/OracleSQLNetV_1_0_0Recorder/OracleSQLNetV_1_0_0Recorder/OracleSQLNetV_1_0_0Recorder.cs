//RemoteRecorder Tanımındaki CustomVar1 kolonuna dosya ismi yazılacak başka hiçbir sorun yok şuanda.
//04122013

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CustomTools;
using Log;
using System.Diagnostics;
using Microsoft.Win32;

namespace OracleSQLNetV_1_0_0Recorder
{
    public class OracleSQLNetV_1_0_0Recorder : CustomBase
    {

        private static Dictionary<string, int> Months;

        static OracleSQLNetV_1_0_0Recorder()
        {
            Months = new Dictionary<string, int>
                                    {
                                        {"JAN", 01},
                                    {"FEB", 02}, 
                                    {"APR", 04}, 
                                    {"JUN", 06}, {"JUL", 07}, {"AUG", 08}, 
                                    {"SEP", 09}, {"OCT", 10}, {"NOV", 11}, 
                                    {"DEC", 12}, {"OCA", 01}, {"ŞUB", 02},
                                    {"SUB", 02}, {"MAR", 03}, {"NIS", 04}, 
                                    {"NİS", 04}, {"MAY", 05}, {"HAZ", 06},
                                    {"TEM", 07}, {"AĞU", 08}, {"AGU", 08},
                                    {"EYL", 09}, {"EKI", 10}, {"EKİ", 10}, 
                                    {"KAS", 11}, {"ARA", 12}
                                    };
        }

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000;
        private long lastRecordnum;
        private uint logging_interval = 60000;
        private string err_log, location, lastFile = "", user = "", password = "";
        private bool reg_flag;
        protected bool usingRegistry = true, fromend;
        protected Int32 Id;
        protected String virtualhost, Dal;
        private CLogger L;
        private string LastRecordDate = "";
        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        readonly object syncRoot = new object();
        public string tempCustomVar1 = "";
        protected Encoding enc;

        public string lastKeywords = "";
        public string remoteHost;
        public string customFileName;
        private Rec record;
        
        private BinaryReader binaryReader;

        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;
        private int maxRecordSend;
        private uint logSize = 1000000;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public OracleSQLNetV_1_0_0Recorder()
        {
            InitializeComponent();
            enc = enc = Encoding.GetEncoding("windows-1254");
            record = new Rec()
                         {
                             LogName = "OracleSQLNetV_1_0_0Recorder"
                         };
        }

        private void InitializeComponent()
        {
        }

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
                        if (!ReadRegistry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        if (!InitializeLogger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on OracleSQLNetV_1_0_0Recorder functions may not be running");
                            return;
                        }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!GetLogDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        if (!InitializeLogger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on OracleSQLNetV_1_0_0Recorder Recorder  functions may not be running");
                            return;
                        }
                        L.Log(LogType.FILE, LogLevel.INFORM, "Start creating OracleSQLNetV_1_0_0Recorder DAL");
                        reg_flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Security Manager OracleSQLNetV_1_0_0Recorder Recorder Init", exception.ToString(), EventLogEntryType.Error);
            }
            L.Log(LogType.FILE, LogLevel.DEBUG, "  OracleSQLNetV_1_0_0Recorder Init Method end.");
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
            timer_interval = SleepTime; //Timer interval.
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            lastRecordnum = Convert_To_Int64(LastPosition);//Last position
            Dal = dal;
            lastFile = LastFile;
            lastKeywords = LastKeywords;
            user = User;
            password = Password;
            remoteHost = RemoteHost;
            customFileName = CustomVar1;
        }

        private long Convert_To_Int64(string value)
        {
            long result;
            return Int64.TryParse(value, out result) ? result : 0;
        }

        public bool GetLogDir()
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
                        err_log = registryKey.GetValue("Home Directory") + @"log\OracleSQLNetV_1_0_0Recorder" + Id + ".log";
                    rk.Close();
                }
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager OracleSQLNetV_1_0_0Recorder  Read Registry", er.ToString(), EventLogEntryType.Error);
                L.Log(LogType.FILE, LogLevel.ERROR, "Security Manager OracleSQLNetV_1_0_0Recorder Read Registry Error. " + er.Message);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        } // Get_logDir

        public bool ReadRegistry()
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " Read_Registry -->> Timer is Started");
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
                logSize = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleSQLNetV_1_0_0Recorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleSQLNetV_1_0_0Recorder").GetValue("Logging Interval"));
                var openSubKey1 = rk.OpenSubKey("Recorder");
                if (openSubKey1 != null)
                {
                    var registryKey1 = openSubKey1.OpenSubKey("OracleSQLNetV_1_0_0Recorder");
                    if (registryKey1 != null)
                        trc_level = Convert.ToInt32(registryKey1.GetValue("Trace Level"));
                }
                var subKey1 = rk.OpenSubKey("Agent");
                if (subKey1 != null)
                    err_log = subKey1.GetValue("Home Directory").ToString() + @"log\OracleSQLNetV_1_0_0Recorder.log";
                timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("OracleSQLNetV_1_0_0Recorder").GetValue("Interval"));
                var subKey = rk.OpenSubKey("Recorder");
                if (subKey != null)
                {
                    var key = subKey.OpenSubKey("OracleSQLNetV_1_0_0Recorder");
                    if (key != null)
                        lastRecordnum = Convert.ToInt64(key.GetValue("LastRecordNum"));
                }

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
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is Started");
                if (location.EndsWith("\\"))
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG,
                            " OracleSQLNetV_1_0_0Recorder In timer1_Tick() --> Directory | " + location);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " OracleSQLNetV_1_0_0Recorder In timer1_Tick() --> lastFile: " + lastFile);
                    ParseFileNameLocal();
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " timer1_Tick -->> Error : " + exception);
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.DEBUG, " timer1_Tick -->> Timer is finished.");
            }
        } // timer1_Tick

        protected void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is STARTED ");
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Position: " + lastRecordnum);

                    if (location.EndsWith("/") || location.EndsWith("\\"))
                    {
                        if (location.StartsWith("\\"))
                            ValidateMe();
                        L.Log(LogType.FILE, LogLevel.DEBUG,
                              " ParseFileNameLocal() -->> Searching files in directory : " + location);
                        var fileName = Path.Combine(location, customFileName);
                        if (!File.Exists(fileName))
                            return;

                        if (lastFile != fileName)
                        {
                            lastFile = fileName;
                            lastRecordnum = 0;
                            LastRecordDate = DateTime.Now.ToString(dateFormat);
                            if (!SetNewFile())
                            {
                                return;
                            }
                            SendLine(lastFile);
                        }
                        else
                        {
                            SendLine(lastFile);
                            L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> lastFile: " + lastFile);
                        }
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR,
                          " ParseFileNameLocal() -->> An error occurred : " + ex);
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }// ParseFileNameLocal

        protected void ValidateMe()
        {

            if (user != "")
            {
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    String userName = user;
                    String domain = "";
                    if (user.Contains("\\"))
                    {
                        String[] arr = user.Split('\\');
                        userName = arr[1];
                        domain = arr[0];
                    }

                    bool ret = LogonUser(userName, domain, password, (Int32)Parser.Parser.LogonType.LOGON32_LOGON_NEW_CREDENTIALS,
                        (Int32)Parser.Parser.LogonProvider.LOGON32_PROVIDER_DEFAULT, ref token);

                    if (ret)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, " Parser In ValidateMe() -->> Impersonation is successful");
                            wi = new WindowsIdentity(tokenDuplicate);
                            wic = wi.Impersonate();
                            if (wic != null)
                            {
                                CloseHandle(token);
                                CloseHandle(tokenDuplicate);
                                return;
                            }
                        }
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        L.Log(LogType.FILE, LogLevel.ERROR, "  Parser In ValidateMe() -->> LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        } // ValidateMe

        public bool SetNewFile()
        {
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " OracleSQLNetV_1_0_0Recorder In SetLastFile() -->> RemoteRecorder Table is updating new parameter." + lastFile + " - " + lastRecordnum + " - " + LastRecordDate);
                CustomServiceBase customServiceBase = GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetReg(Id, lastRecordnum.ToString(CultureInfo.InvariantCulture), null, lastFile, "",
                                     LastRecordDate);
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR,
                      "OracleSQLNetV_1_0_0Recorder SetLastFile update error:" + exception.Message);
                return false;
            }
        } // SetNewFile

        public bool SendLine(string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, "SendLine is Started. FileName" + fileName);
            try
            {
                var stringBuilder = new StringBuilder();
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    binaryReader = new BinaryReader(fileStream, enc);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Info: " + fileName + " - " + fileStream.Length + " - " + lastRecordnum);
                    L.Log(LogType.FILE, LogLevel.DEBUG, " SendLine() -->> Path _ if : " + fileName + " - " + fileStream.Length + " - " + lastRecordnum);
                    binaryReader.BaseStream.Seek(lastRecordnum >= 0 ? lastRecordnum : 0, SeekOrigin.Begin);
                    int r;
                    while ((r = binaryReader.Read()) >= 0)
                    {
                        var ch = (char)r;
                        if (ch == '\n' || ch == '\r')
                        {
                            if (stringBuilder.Length > 0)
                            {
                                CoderParse(stringBuilder.ToString(), fileName);
                            }
                            stringBuilder.Remove(0, stringBuilder.Length);
                        }
                        else
                        {
                            stringBuilder.Append(ch);
                        }
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " SendLine() -->> An error occurred : " + exception);
                return false;
            }
        } // SendLine

        public bool CoderParse(string line, string fileName)
        {
            L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
            L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Line is : " + line);

            if (string.IsNullOrEmpty(line))
            {
                return false;
            }

            line = line.Trim();

            if (line.StartsWith("Fatal"))
            {
                record.EventCategory = line;
            }
            else if (line.StartsWith("Time"))
            {
                try
                {
                    line = line.Replace("Time:", "").Trim();
                    var ar = line.Split(' ');
                    var date = ar[0];
                    var time = ar[1];
                    var day = date.Split('-')[0];
                    var dateAr = date.Split('-');
                    L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Parsed:  " + "Month: " + dateAr[1]);
                    int month;
                    if (!Months.TryGetValue(dateAr[1], out month))
                    {
                        month = 0;
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Parsed:  " + "Month = 0 : " + dateAr[1]);
                    }

                    var year = date.Split('-')[2];
                    var myDateTimeString = year + "-" + month + "-" + day + " " + time;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Parsed:  " + "myDateTimeString: " + myDateTimeString);
                    var myDateString = myDateTimeString;
                    record.Datetime = myDateString;
                    L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime : '" + record.Datetime + "'");
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> Datetime Convert Error. " + exception.Message);
                }
            }
            else if (line.StartsWith("Client address:"))
            {
                try
                {
                    var charSeperetor = new[] { '(', ')' };
                    var arr = line.Split(charSeperetor, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in arr)
                    {
                        if (s.StartsWith("PROTOCOL="))
                        {
                            record.CustomStr1 = s.Split('=')[1];
                        }
                        else if (s.StartsWith("HOST="))
                        {
                            record.CustomStr3 = s.Split('=')[1];
                        }
                        else if (s.StartsWith("PORT="))
                        {
                            record.CustomInt1 = Convert.ToInt32(s.Split('=')[1]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> Client address part error. " + exception.Message);
                }
            }

            else if (line.StartsWith("*"))
            {
                if (!string.IsNullOrEmpty(record.Datetime))
                {
                    try
                    {
                        record.Description = record.Datetime + " | " +
                                             record.EventCategory + " | " +
                                             record.CustomStr1 + " | " +
                                             record.CustomStr3 + " | " +
                                             record.CustomInt1.ToString(CultureInfo.InvariantCulture);

                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Record Sending.");

                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime: " + record.Datetime);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> EventCategory: " + record.EventCategory);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr1: " + record.CustomStr1);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr3: " + record.CustomStr3);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr4: " + record.CustomStr4);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt1: " + record.CustomInt1);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Description: " + record.Description);

                        RecordSendOperation(record, line);
                        L.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Record Sent.");

                    }
                    finally
                    {
                        record.CustomInt1 = 0;
                        record.CustomStr1 = "";
                        record.CustomStr3 = "";
                        record.Datetime = "";
                        record.Description = "";
                        record.EventCategory = "";
                    }
                }
            }
            return true;
        }

        public bool RecordSendOperation(Rec r, string line)
        {
            var customServiceBase = GetInstanceService("Security Manager Remote Recorder");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, " OracleSQLNetV_1_0_0Recorder In RecordSendOperation() -->> Record sending." + lastRecordnum + " - " + lastFile);
                customServiceBase.SetData(Dal, virtualhost, r);
                customServiceBase.SetReg(Id, binaryReader.BaseStream.Position.ToString(CultureInfo.InvariantCulture), line, lastFile, "", LastRecordDate);
                L.Log(LogType.FILE, LogLevel.DEBUG, " OracleSQLNetV_1_0_0Recorder In RecordSendOperation() -->> Record sended.");
                lastRecordnum = binaryReader.BaseStream.Position;
                return true;
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, " OracleSQLNetV_1_0_0Recorder In CoderParse() -->> Record sending Error." + exception.Message);
                return false;
            }
        } // RecordSendOperation

        public bool SetRegistry(long status)
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
                                rk = registryKey1.CreateSubKey("OracleSQLNetV_1_0_0Recorder");
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
                EventLog.WriteEntry("Security Manager OracleSQLNetV_1_0_0Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        } // SetRegistry

        public bool InitializeLogger()
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
                L.SetLogFileSize(logSize);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager RedHatSecure Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        } // InitializeLogger
    }
}
