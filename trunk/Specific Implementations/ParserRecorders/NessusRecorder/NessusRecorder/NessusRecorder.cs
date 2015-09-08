using System;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace NessusRecorder
{

    public class NessusRecorder : CustomBase
    {
        #region "Property Initializations"

        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0, zone = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position = "Tki21Temmuz.nessus=0", last_line = "", remote_host = "", location = @"C:\Documents and Settings\AHMETOZER\Desktop\TKI\Nessus Scan Results\", user = "", password = "", last_recdate = "";
        private bool reg_flag = false, start_state = true;
        protected bool usingRegistry = false;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        //Validation
        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

        #endregion

        #region "API Function Declarations"

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        #endregion

        #region "Logon-Related Enum Declarations"

        public enum LogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations;
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server.
            /// </summary>
            LOGON32_LOGON_INTERACTIVE = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_NETWORK = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_BATCH = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled.
            /// </summary>
            LOGON32_LOGON_SERVICE = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer.
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked.
            /// </summary>
            LOGON32_LOGON_UNLOCK = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections.
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        public enum LogonProvider : int
        {
            /// <summary>
            /// Use the standard logon provider for the system.
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name
            /// is not in UPN format. In this case, the default provider is NTLM.
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            LOGON32_PROVIDER_DEFAULT = 0,
        }

        #endregion

        public NessusRecorder()
        {
            InitializeComponent();
        }

        public override void Init()
        {
            timer1 = new System.Timers.Timer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Interval = timer_interval;
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nessus Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nessus Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
            }
        }

        private void InitializeComponent()
        {
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Trace Level"));
                remote_host = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NessusRecorder" + remote_host + ".log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("MaxLineToWait"));
                fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("FromEndOnLoss"));
                last_position = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("LastPosition").ToString();
                last_line = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("LastLine").ToString();
                location = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Location").ToString();
                user = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("NessusRecorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Nessus Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NessusRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Nessus Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;

            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");

            FileStream fs = null;
            StreamReader tr = null;

            try
            {
                if (!reg_flag & usingRegistry)
                {
                    if (!Read_Registry())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nessus Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Registry read in timer tick");
                }
                else if (!reg_flag & !usingRegistry)
                {
                    if (!Get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log directory");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Nessus Recorder functions may not be running");
                            return;
                        }
                }

                if (start_state & fromend == 1)
                {
                    if (!Set_LastPosition())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on setting the last position see log for more details");
                    }
                    start_state = false;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start Connecting host:");

                if (remote_host == "")
                    remote_host = "localhost";

                try
                {
                    ValidateMe();
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, String.Format("Connection to host {0} failed.", remote_host));

                    return;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);

                DirectoryInfo nessusDirectory = null;
                FileInfo[] nessusFiles = null;

                try
                {
                    nessusDirectory = new DirectoryInfo(location);
                    nessusFiles = nessusDirectory.GetFiles("*.nessus");
                }
                catch (Exception)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Files in the given location could not be listed. Finishing process..");
                    return;
                }

                if (nessusFiles.Length == 0)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "No files to read. Finishing process..");
                    return;
                }

                string[] lastReadFilesArr;
                string fileToRead = "";
                string[] lastPosArr;

                XmlDocument xd = new XmlDocument();

                XmlNodeList reports = null;
                int IndexLastReportParsed = 0;

                bool pickAnotherFile = false;

                if (last_line == "")
                {
                    pickAnotherFile = true;
                    lastReadFilesArr = null;
                    lastPosArr = null;
                }
                else
                {
                    lastReadFilesArr = last_line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); //a:b/../m/x1.nessus,a2:b2/../m2/x2.nessus
                    lastPosArr = last_position.Split('='); // a2:b2/../m2/x2.nessus=IndexLastReportParsed
                    fileToRead = lastPosArr[0];

                    try
                    {
                        fs = new FileStream(fileToRead, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                        xd.Load(fs);

                        reports = xd.SelectNodes("/NessusClientData/Report");
                    }
                    catch (Exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "File (already parsed) could not be loaded: " + fileToRead);
                        return;
                    }
                    if (reports.Count - 1 <= int.Parse(lastPosArr[1]))
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "File completely parsed. Another file is to be parsed");
                        pickAnotherFile = true;
                    }
                }


                if (pickAnotherFile)
                {
                    bool previousFileFound = false;

                    foreach (FileInfo nessusFile in nessusFiles)
                    {
                        if (lastReadFilesArr == null)
                        {
                            fileToRead = nessusFile.FullName;

                            try
                            {
                                fs = new FileStream(fileToRead, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                                xd.Load(fs);

                                reports = xd.SelectNodes("/NessusClientData/Report");
                            }
                            catch (Exception)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "File could not be loaded: " + fileToRead);
                                last_position = fileToRead + "=0";
                                last_line += "," + fileToRead;
                                Set_Registry(last_position, last_line);
                                return;
                            }

                            IndexLastReportParsed = 0;

                            break;
                        }
                        else
                        {
                            previousFileFound = false;

                            foreach (string fileName in lastReadFilesArr)
                            {
                                if (fileName == nessusFile.FullName)
                                {
                                    //Skip this file. Do not parse it anymore
                                    previousFileFound = true;
                                }
                            }
                            if (previousFileFound)
                            {
                                continue;
                            }
                            else
                            {
                                previousFileFound = false;

                                fileToRead = nessusFile.FullName;

                                try
                                {
                                    fs = new FileStream(fileToRead, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    tr = new StreamReader((Stream)fs, System.Text.Encoding.ASCII);
                                    xd.Load(fs);

                                    reports = xd.SelectNodes("/NessusClientData/Report");
                                }
                                catch (Exception)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "File could not be loaded: " + fileToRead);
                                    last_position = fileToRead + "=0";
                                    last_line += "," + fileToRead;
                                    Set_Registry(last_position, last_line);
                                    return;
                                }

                                IndexLastReportParsed = 0;

                                break;
                            }
                        }
                    }
                    if (previousFileFound)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "No files to read. Finishing process..");
                        return;
                    }
                }
                else
                {
                    IndexLastReportParsed = int.Parse(lastPosArr[1]);
                }

                if (IndexLastReportParsed < 0)
                {
                    IndexLastReportParsed = 0;
                }

                int index = IndexLastReportParsed;
                try
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Number of reports: " + reports.Count);
                    for (; index < reports.Count; index++)
                    {
                        string[] reportDateTimeArr = reports[index].SelectSingleNode("ReportName").InnerXml.Split('-')[0].Trim().Split('/');
                        string hour = Convert.ToString(int.Parse(reportDateTimeArr[2].Split(' ')[1].Split(':')[0]));
                        string min = reportDateTimeArr[2].Split(' ')[1].Split(':')[1];
                        string sec = reportDateTimeArr[2].Split(' ')[1].Split(':')[2];

                        string reportDateTime = reportDateTimeArr[1] + "/" + reportDateTimeArr[2].Split(' ')[0] + "/" + "20" + reportDateTimeArr[0] + " " + hour + ":" + min + ":" + sec + ".000"; // +reportDateTimeArr[2].Split(' ')[2];

                        XmlNodeList reportHostItems = reports[index].SelectNodes("ReportHost");

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Number of reported hosts: " + reportHostItems.Count);

                        for (int j = 0; j < reportHostItems.Count; j++)
                        {
                            string reportHostIP = reportHostItems[j].SelectSingleNode("HostName").InnerXml;
                            string reportHostName = reportHostItems[j].SelectSingleNode("netbios_name").InnerXml;
                            string reportHostOS = reportHostItems[j].SelectSingleNode("os_name").InnerXml;
                            string reportHostMAC = reportHostItems[j].SelectSingleNode("mac_addr").InnerXml;

                            XmlNodeList reportItems = reportHostItems[j].SelectNodes("ReportItem");
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Number of report items: " + reportItems.Count);

                            for (int k = 0; k < reportItems.Count; k++)
                            {
                                try
                                {
                                    CustomBase.Rec rec = new CustomBase.Rec();

                                    rec.LogName = "Nessus Recorder";

                                    //rec.Datetime = reportDateTime;

                                    //rec.Datetime = Convert.ToDateTime(rec.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                                    try
                                    {
                                        rec.Datetime = Convert.ToDateTime(reportDateTime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                                    }
                                    catch (Exception)
                                    {
                                        L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing date: " + reportDateTime);
                                        continue;
                                    }

                                    last_recdate = rec.Datetime;

                                    rec.ComputerName = remote_host; //Scanner host name

                                    rec.CustomStr4 = reportHostName; //Scanned host name
                                    rec.CustomStr3 = reportHostIP; //Scanned host IP
                                    rec.SourceName = reportItems[k].SelectSingleNode("port").InnerXml; //Scanned port name                         
                                    rec.CustomStr1 = reportHostOS; //Scanned host operating system
                                    rec.CustomStr2 = reportHostMAC; //Scanned host MAC address
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "HostName: " + rec.CustomStr3);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "HostIP: " + rec.CustomStr5);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Port: " + rec.SourceName);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Host OS: " + rec.CustomStr1);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Host MAC: " + rec.CustomStr2);

                                    rec.EventId = long.Parse(reportItems[k].SelectSingleNode("pluginID").InnerXml); //Plugin ID
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Plugin ID: " + rec.EventId);

                                    rec.EventCategory = reportItems[k].SelectSingleNode("pluginName").InnerXml; //Scan plugin name
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Plugin Name: " + rec.EventCategory);

                                    rec.CustomInt1 = int.Parse(reportItems[k].SelectSingleNode("severity").InnerXml); //Severity of vulnerability

                                    rec.Description = reportItems[k].SelectSingleNode("data").InnerXml.Trim(); //Scan result

                                    string[] DescArr = rec.Description.Normalize(System.Text.NormalizationForm.FormKD).Split(new string[] { "\n\n", "\r\n" /*, "\n", "\r", "\\n", "\\r" */, "\\n\\n", "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "DescArr length is: " + DescArr.Length);

                                    for (int p = 0; p < DescArr.Length; p++)
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "DescArr[" + p + "] is: " + DescArr[p]);

                                        if (DescArr[p].Contains("Synopsis :"))
                                        {
                                            try
                                            {
                                                rec.EventType = DescArr[p + 1].Trim(); //Synopsis
                                            }
                                            catch (Exception ex)
                                            {
                                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing synopsis: " + ex.Message);
                                            }
                                            finally
                                            {
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "Synopsis: " + rec.EventType);
                                            }
                                        }
                                        else if (DescArr[p].Contains("Solution :"))
                                        {
                                            try
                                            {
                                                rec.CustomStr9 = DescArr[p + 1].Trim(); //Solution
                                            }
                                            catch (Exception ex)
                                            {
                                                string[] solArr = DescArr[p].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (solArr.Length > 1)
                                                {
                                                    for (int x = 1; x < solArr.Length; x++)
                                                    {
                                                        rec.CustomStr9 += " : " + solArr[x]; //Solution
                                                    }
                                                }
                                                if (rec.CustomStr9.Contains("CVE :") || rec.CustomStr9.Contains("Risk factor :"))
                                                {
                                                    rec.CustomStr9 = "";
                                                }
                                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing solution: " + ex.Message);
                                            }
                                            finally
                                            {
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "Solution: " + rec.CustomStr9);
                                            }
                                        }
                                        else if (DescArr[p].Contains("Risk factor :"))
                                        {
                                            try
                                            {
                                                rec.CustomStr8 = DescArr[p + 1].Trim(); //Risk factor
                                            }
                                            catch (Exception ex)
                                            {
                                                string[] riskArr = DescArr[p].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (riskArr.Length > 1)
                                                {
                                                    for (int x = 1; x < riskArr.Length; x++)
                                                    {
                                                        rec.CustomStr8 += " : " + riskArr[x]; //Risk factor
                                                    }
                                                }

                                                if (rec.CustomStr8.Contains("Low"))
                                                {
                                                    rec.CustomStr8 = "Low";
                                                }
                                                else if (rec.CustomStr8.Contains("Medium"))
                                                {
                                                    rec.CustomStr8 = "Medium";
                                                }
                                                else if (rec.CustomStr8.Contains("High"))
                                                {
                                                    rec.CustomStr8 = "High";
                                                }
                                                else if (rec.CustomStr8.Contains("None"))
                                                {
                                                    rec.CustomStr8 = "None";
                                                }
                                                else if (rec.CustomStr8.Contains("CVE :"))
                                                {
                                                    rec.CustomStr8 = "None";
                                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing risk factor: " + ex.Message);
                                                }
                                                else
                                                {
                                                    rec.CustomStr8 = "";
                                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing risk factor: " + ex.Message);
                                                }
                                            }
                                            finally
                                            {
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "Risk factor: " + rec.CustomStr8);
                                            }
                                        }
                                        else if (DescArr[p].Contains("CVE :"))
                                        {
                                            try
                                            {
                                                string[] cveArr = DescArr[p].Split(new string[] { "\n", "\r", "\\n", "\\r" }, StringSplitOptions.RemoveEmptyEntries);

                                                for (int q = 0; q < cveArr.Length; q++)
                                                {
                                                    L.Log(LogType.FILE, LogLevel.DEBUG, "cveArr[q] is: " + cveArr[q]);
                                                    if (cveArr[q].Contains("CVE :"))
                                                    {
                                                        rec.CustomStr6 = cveArr[q].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Trim(); //CVE ID
                                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CVE ID: " + rec.CustomStr6);
                                                    }
                                                    else if (cveArr[q].Contains("BID :"))
                                                    {
                                                        rec.CustomStr7 = cveArr[q].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Trim(); //BUG ID
                                                        L.Log(LogType.FILE, LogLevel.DEBUG, "BUG ID: " + rec.CustomStr7);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing CVE and BID: " + ex.Message);
                                            }
                                            finally
                                            {
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "CVE ID: " + rec.CustomStr6);
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "BUG ID: " + rec.CustomStr7);
                                            }
                                        }
                                        //else if (DescArr[p].Contains("CVE :"))
                                        //{
                                        //    rec.CustomStr6 = DescArr[p].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Trim(); //CVE ID
                                        //    L.Log(LogType.FILE, LogLevel.DEBUG, "CVE ID: " + rec.CustomStr6);
                                        //}
                                        //else if (DescArr[p].Contains("BID :"))
                                        //{
                                        //    rec.CustomStr7 = DescArr[p].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1].Trim(); //BUG ID
                                        //    L.Log(LogType.FILE, LogLevel.DEBUG, "BUG ID: " + rec.CustomStr7);
                                        //}
                                    }

                                    if (rec.Description.Length > 900)
                                    {
                                        rec.CustomStr10 = rec.Description.Substring(900);
                                        if (rec.CustomStr10.Length > 900)
                                        {
                                            rec.CustomStr10 = rec.CustomStr10.Substring(0, 900);
                                        }
                                        rec.Description = rec.Description.Substring(0, 900);
                                    }

                                    Send_Record(rec);
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing log: reports[" + index + "], reportHostItems[" + j + "], reportItems[" + k + "]");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing file: " + fileToRead + Environment.NewLine + ex.Message);
                    last_position = fileToRead + "=" + index.ToString();
                    if (pickAnotherFile)
                    {
                        last_line += "," + fileToRead;
                    }
                    Set_Registry(last_position, last_line);
                    return;
                }

                last_position = fileToRead + "=" + Convert.ToString(reports.Count - 1);

                if (pickAnotherFile)
                {
                    last_line += "," + fileToRead;
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Last position is " + last_position);

                Set_Registry(last_position, last_line);
            }

            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    tr.Close();
                }
                timer1.Enabled = true;
            }
        }

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
                    bool ret = LogonUser(userName, domain, password, (Int32)LogonType.LOGON32_LOGON_NEW_CREDENTIALS,
                        (Int32)LogonProvider.LOGON32_PROVIDER_DEFAULT, ref token);
                    if (ret)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate))
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Impersonation is successful");

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
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }

        public bool Set_Registry(string status, string lastlineStat)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistry)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NessusRecorder");
                    rk.SetValue("LastPosition", status);
                    rk.Close();
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NessusRecorder");
                    rk.SetValue("LastLine", lastlineStat);
                    rk.Close();
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, status, lastlineStat, "", "", last_recdate);
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager Nessus Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Set_LastPosition()
        {
            try
            {
                ValidateMe();

                if (remote_host == "")
                    remote_host = "localhost";

                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
        }

        public bool Send_Record(CustomBase.Rec rec)
        {
            last_recdate = rec.Datetime;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error sending data. Reason: " + er.ToString());
                return false;
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
                EventLog.WriteEntry("Security Manager Nessus Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, int CustomVar2, String Virtualhost, String dal, int Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            last_position = LastPosition;
            last_line = LastLine;
            if (FromEndOnLoss)
                fromend = 1;
            else
                fromend = 0;
            max_line_towait = MaxLineToWait;
            timer_interval = SleepTime;
            user = User;
            password = Password;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;
        }

        public override void SetReg(Int32 Identity, String LastPosition, String LastLine, String LastFile, String LastKeywords, String LastRecDate)
        {
            base.SetReg(Identity, LastPosition, LastLine, "", "", LastRecDate);
        }

    }

}
