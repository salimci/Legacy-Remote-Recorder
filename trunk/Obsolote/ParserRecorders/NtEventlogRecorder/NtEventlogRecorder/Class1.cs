/*
 * Onur Sarýkaya 
 * 27.04.2012 
 * NtEventLogRecorder güncellendi 
 * EventId = 560 için özel bir condition yazýldý.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CustomTools;
using System.ServiceProcess;
using System.Threading;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.Common;
using System.Data;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace NtEventlogRecorder
{
    public class NtEventlogRecorder : CustomBase
    {
        static NtEventlogRecorder()
        {
           //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
           //AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
        }


        //static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        //{
        //    StreamWriter sw = null;
        //    try
        //    {
        //        sw=new StreamWriter(@"c:\tmp\recorder.log",true);
        //        sw.WriteLine("load {0} {1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture),args.LoadedAssembly.FullName);
        //    }catch
        //    {
        //    }
        //    finally
        //    {
        //        if(sw != null)
        //        {
        //            try
        //            {
        //                sw.Close();
        //            }catch
        //            {
        //            }
        //        }
        //    }
        //}
        private System.Timers.Timer timer1;
        private int trc_level = 3, timer_interval = 3000, max_line_towait = 100, fromend = 0, zone = 0;
        private uint logging_interval = 60000, log_size = 1000000;
        private string err_log, last_position = "0", first_position = "0", remote_host = "", location, user, password, last_recdate = "";
        private bool reg_flag = false, start_state = true;
        protected bool usingRegistry = true;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private CLogger L;
        private ConnectionOptions co;
        private ManagementScope scope;

        //Validation
        private WindowsImpersonationContext wic;
        private WindowsIdentity wi;

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

        public NtEventlogRecorder()
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog Recorder functions may not be running");
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog Recorder functions may not be running");
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
                log_size = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Log Size"));
                logging_interval = Convert.ToUInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Logging Interval"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Trace Level"));
                remote_host = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Remote Host").ToString();
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NtEventlogRecorder" + remote_host + ".log";
                this.timer1.Interval = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("SleepTime"));
                max_line_towait = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("MaxLineToWait"));
                fromend = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("FromEndOnLoss"));
                last_position = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("LastPosition").ToString();
                location = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Location").ToString();
                user = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("User").ToString();
                password = rk.OpenSubKey("Recorder").OpenSubKey("NtEventlogRecorder").GetValue("Password").ToString();
                if (password != "")
                    password = Encrypter.Decyrpt("natek12pass", password);

                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NtEventlog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NtEventlogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NtEventlog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public virtual void Stop()
        {
            timer1.Stop();

            L.Log(LogType.FILE, LogLevel.DEBUG, "Recorder has been Stopped");
        }

        private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            L.Log(LogType.FILE, LogLevel.INFORM, "Service Started");
            try
            {
                // Fill the record fileds with necessary parameters 
                //readQuery = "SELECT UPPER(HOST_NAME) AS HOST_NAME FROM NODE WHERE LAST_UPDATED < (getdate() - CONVERT(datetime,'" + respond_hour + ":" + respond_time + ":0',108)) ORDER BY LAST_UPDATED DESC";
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
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog Recorder functions may not be running");
                            return;
                        }
                    reg_flag = true;
                }
                else
                {
                    if (!Get_logDir())
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log directory");
                        return;
                    }
                    else
                        if (!Initialize_Logger())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NtEventlog Recorder functions may not be running");
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
                {
                    co = new ConnectionOptions();
                    co.Timeout = new TimeSpan(0, 10, 0);
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.PacketPrivacy;
                    scope = new ManagementScope(@"\\localhost\root\cimv2", co);
                }
                else
                {
                    co = new ConnectionOptions();
                    co.Username = user;
                    co.Password = password;
                    co.Timeout = new TimeSpan(0, 10, 0);
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.PacketPrivacy;
                    scope = new ManagementScope(@"\\" + remote_host + @"\root\cimv2", co);
                }
                scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                scope.Connect();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");

                // default blocksize = 1, larger value may increase network throughput
                EnumerationOptions opt = new EnumerationOptions();
                opt.BlockSize = 1000;

                first_position = last_position;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is :" + last_position);
                L.Log(LogType.FILE, LogLevel.DEBUG, "First Position is :" + first_position);

                SelectQuery query = new SelectQuery("select CategoryString,ComputerName, EventIdentifier,Type,Message,RecordNumber,SourceName,TimeGenerated,User " +
                    "from Win32_NtLogEvent " +
                    "where Logfile ='" + location + "' and RecordNumber >=" + first_position + " and RecordNumber <" + Convert.ToString(Convert.ToInt32(first_position) + max_line_towait));

                L.Log(LogType.FILE, LogLevel.DEBUG, "query is :" + query.QueryString);
                L.Log(LogType.FILE, LogLevel.DEBUG, "last position :" + last_position);

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start collection event logs of :" + location);
                bool resetposition = true;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        if (Convert.ToInt32(mo["RecordNumber"]) == Convert.ToInt32(first_position))
                            resetposition = false;
                        else
                        {
                            resetposition = false;
                            if (!Send_Record(mo))
                            {
                                L.Log(LogType.FILE, LogLevel.DEBUG, "Error on sending record with record number" + mo["RecordNumber"].ToString());
                            }

                            if (Convert.ToInt32(last_position) < Convert.ToInt32(mo["RecordNumber"]))
                            {
                                last_position = mo["RecordNumber"].ToString();
                                string dat = mo["TimeGenerated"].ToString().Split('.')[0];
                                L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated1:" + dat);
                                last_recdate = dat.Substring(0, 4) + "/" + dat.Substring(4, 2) + "/" + dat.Substring(6, 2) + " " + dat.Substring(8, 2) + ":" + dat.Substring(10, 2) + ":" + dat.Substring(12, 2); //+ "." + dat.Substring(14, 2);                                 
                                last_recdate = Convert.ToDateTime(last_recdate).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                        }
                    }
                if (resetposition)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "No data come Start Reseting the position of the log files");
                    Set_LastPosition();
                }

                Set_Registry(last_position, last_recdate);
            }

            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                L.Log(LogType.FILE, LogLevel.INFORM, "Service Stopped");
            }
        }
        public bool Set_Registry(string status, string lastrecdate)
        {
            RegistryKey rk = null;
            try
            {
                if (usingRegistry)
                {
                    rk = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Natek").CreateSubKey("Security Manager").CreateSubKey("Recorder").CreateSubKey("NtEventlogRecorder");
                    rk.SetValue("LastPosition", status);
                    rk.Close();
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetReg(Id, status, "", "", "", lastrecdate);
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                EventLog.WriteEntry("Security Manager NtEventlog Recorder Set Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
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
                            L.Log(LogType.FILE, LogLevel.INFORM, "Impersonation is successful");
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
                        L.Log(LogType.FILE, LogLevel.ERROR, "LogonUser failed with error code : " + error);
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);

            }
        }
        public bool Set_LastPosition()
        {
            try
            {
                ValidateMe();
                EventLog ev;
                if (remote_host == "")
                    ev = new EventLog(location);
                else
                    ev = new EventLog(location, remote_host);

                if (fromend == 1)
                {
                    L.Log(LogType.FILE, LogLevel.WARN, "Fromend Position is set to: " + last_position.ToString());
                    last_position = ev.Entries[ev.Entries.Count - 1].Index.ToString();
                    last_recdate = ev.Entries[ev.Entries.Count - 1].TimeGenerated.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm.ss");
                }
                else
                {
                    /*
                    if(start_state) 
                    {
                        if (Convert.ToInt32(last_position) < ev.Entries[1].Index)
                            last_position = ev.Entries[1].Index.ToString();
                    }
                    else*/
                    L.Log(LogType.FILE, LogLevel.WARN, "Position is set to: " + last_position.ToString());
                    last_position = ev.Entries[1].Index.ToString();
                }
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }

            /*
            int first_rec=0, second_rec=0, record_count=0;
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start Connecting host:");
                co = new ConnectionOptions();
                co.Username = user;
                co.Password = password;
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.PacketPrivacy;
                //co.Authority = "ntlmdomain:" +remote_host;

                if (remote_host == "")
                    remote_host = "localhost";

                scope = new ManagementScope(@"\\" + remote_host + @"\root\cimv2", co);
                scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                scope.Connect();
                L.Log(LogType.FILE, LogLevel.DEBUG, "Connection successfull:");

                // default blocksize = 1, larger value may increase network throughput
                EnumerationOptions opt = new EnumerationOptions();
                opt.BlockSize = 1000;

                L.Log(LogType.FILE, LogLevel.DEBUG, "Start executing query for getting the last position");

                SelectQuery query = new SelectQuery("select RecordNumber from Win32_NtLogEvent where Logfile ='" + location + "'");

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish executing query for getting the last position");

                int i = 1;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, opt))
                    foreach (ManagementObject mc in searcher.Get())
                    {
                        if (i == 2)
                        {
                            second_rec = Convert.ToInt32(mc["RecordNumber"]);
                            break;
                        }
                        if (i == 1)
                        {
                            record_count = searcher.Get().Count;
                            first_rec = Convert.ToInt32(mc["RecordNumber"]);
                            i++;
                        }
                    }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);
                L.Log(LogType.FILE, LogLevel.DEBUG, "First record is: "+first_rec.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Second record is: "+second_rec.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Record Count: "+record_count.ToString());

                if (first_rec > second_rec)
                {
                    if (fromend == 1)
                        last_position = Convert.ToString(first_rec);
                    else
                    {
                        if (Convert.ToInt32(last_position) < first_rec - record_count)
                            last_position = Convert.ToString(first_rec - record_count);
                    }
                }
                else
                {
                    if (fromend == 1)
                        last_position = Convert.ToString(first_rec + record_count);
                    else
                    {
                        if (Convert.ToInt32(last_position) < first_rec)
                            last_position = Convert.ToString(first_rec);
                    }
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Last Position is: " + last_position);
                return true;
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
            */
        }
        public override void Clear()
        {
            if (timer1 != null)
            {
                timer1.Enabled = false;
                timer1.Dispose();
            }
        }
        public bool Send_Record(ManagementObject mo)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                //rec.LogName = "NTEventlogRecorder";
                rec.LogName = "NT-" + location;
                rec.CustomStr8 = remote_host;
                if (mo["ComputerName"] != null)
                {
                    rec.ComputerName = mo["ComputerName"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Comptername:" + rec.ComputerName);
                }
                if (mo["CategoryString"] != null)
                {
                    rec.EventCategory = mo["CategoryString"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory:" + rec.EventCategory.ToString());
                }
                if (mo["EventIdentifier"] != null)
                {
                    rec.EventId = Convert.ToInt64(mo["EventIdentifier"]);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventIdentifier:" + rec.EventId.ToString());
                }
                if (mo["Type"] != null)
                {
                    //Audit Failure => FailureAudit
                    //Audit Success => SuccessAudit
                    string evt = mo["Type"].ToString();

                    if (evt == "Audit Failure")
                        evt = "FailureAudit";
                    if (evt == "Denetim Baþarýsýz")
                        evt = "FailureAudit";
                    if (evt == "Audit Success")
                        evt = "SuccessAudit";
                    if (evt == "Denetim Baþarýsýz")
                        evt = "SuccessAudit";

                    rec.EventType = evt;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType:" + rec.EventType);
                }
                if (mo["RecordNumber"] != null)
                {
                    rec.Recordnum = Convert.ToInt32(mo["RecordNumber"]);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "RecordNumber:" + rec.Recordnum.ToString());
                }
                if (mo["SourceName"] != null)
                {
                    rec.SourceName = mo["SourceName"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName:" + rec.SourceName);
                }
                if (mo["User"] != null)
                {
                    rec.UserName = mo["User"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "User:" + rec.SourceName);
                }
                if ((mo["Message"] != null))
                {
                    rec.Description = mo["Message"].ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Message:" + rec.Description);

                    string[] DescArr = rec.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    bool subjectMode = false;
                    bool objectMode = false;
                    bool targetMode = false;
                    bool accessMode = false;
                    bool processMode = false;
                    bool applMode = false;
                    bool networkMode = false;
                    bool authenMode = false;
                    bool dummyAccessControl = false;

                    if (rec.EventId != 560)
                    {
                        for (int i = 0; i < DescArr.Length; i++)
                        {
                            if (!DescArr[i].Contains(":"))
                            {
                                if (accessMode)
                                {
                                    rec.CustomStr7 += " " + DescArr[i].Trim();
                                }
                            }
                            else
                            {
                                string[] lineArr = DescArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                if (lineArr[1].Trim() == "")
                                {
                                    #region Mode
                                    if (lineArr[0].Trim() == "Application Information")
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = true;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Network Information")
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = true;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Subject" || lineArr[0].Trim() == "New Logon" || lineArr[0].Trim() == "Account Whose Credentials Were Used" || lineArr[0].Trim() == "Credentials Which Were Replayed" || lineArr[0].Trim() == "Account That Was Locked Out" || lineArr[0].Trim() == "New Computer Account" || lineArr[0].Trim() == "Computer Account That Was Changed" || lineArr[0].Trim() == "Source Account")
                                    {
                                        subjectMode = true;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Target" || lineArr[0].Trim() == "Target Account" || lineArr[0].Trim() == "Target Computer" || lineArr[0].Trim() == "Target Server")
                                    {
                                        subjectMode = true;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Object")
                                    {
                                        subjectMode = false;
                                        objectMode = true;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Process Information" || lineArr[0].Trim() == "Process")
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = true;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Access Request Information")
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = true;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    else if (lineArr[0].Trim() == "Detailed Authentication Information")
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = true;
                                    }
                                    else
                                    {
                                        subjectMode = false;
                                        objectMode = false;
                                        targetMode = false;
                                        accessMode = false;
                                        processMode = false;
                                        applMode = false;
                                        networkMode = false;
                                        authenMode = false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    if (subjectMode)
                                    {
                                        #region SubjectMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "User Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Account Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Client Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            //case "Security ID":
                                            //    if (rec.CustomStr2 == null)
                                            //    {
                                            //        rec.CustomStr2 = appendArrayElements(lineArr);
                                            //    }
                                            //    break;
                                            case "Logon ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = 0;
                                                }
                                                break;
                                            case "Client Context ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = 0;
                                                }
                                                break;
                                            case "Account Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Client Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (targetMode)
                                    {
                                        #region TargetMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "User Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            //case "Target Server Name":
                                            //    rec.CustomStr2 = appendArrayElements(lineArr);
                                            //    break;
                                            case "Account Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            case "Old Account Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "New Account Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            case "Account Domain":
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Domain":
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (objectMode)
                                    {
                                        #region ObjectMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Object Name":
                                                rec.CustomStr8 = appendArrayElements(lineArr);
                                                break;
                                            case "Object Type":
                                                rec.CustomStr9 = appendArrayElements(lineArr);
                                                break;
                                            case "Operation Type":
                                                rec.CustomStr9 = appendArrayElements(lineArr);
                                                break;
                                            case "Handle ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = 0;
                                                }
                                                break;
                                            case "Primary User Name":
                                                if (rec.CustomStr1 == null)
                                                {
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                }
                                                break;
                                            case "Client User Name":
                                                if (rec.CustomStr2 == null)
                                                {
                                                    rec.CustomStr2 = appendArrayElements(lineArr);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (accessMode)
                                    {
                                        #region AccessMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Accesses":
                                                if (rec.CustomStr7 == null)
                                                {
                                                    rec.CustomStr7 = appendArrayElements(lineArr);
                                                    dummyAccessControl = true;
                                                }
                                                break;
                                            case "Access Mask":
                                                if (dummyAccessControl)
                                                {
                                                    rec.CustomStr7 += " " + appendArrayElements(lineArr);
                                                }
                                                break;
                                            case "Operation Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (processMode)
                                    {
                                        #region ProcessMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Duration":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = 0;
                                                }
                                                break;
                                            case "Process ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "PID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Image File Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Logon Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (applMode)
                                    {
                                        #region ApplMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Logon Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Duration":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = 0;
                                                }
                                                break;
                                            case "Process ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "Application Instance ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Application Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Image File Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (networkMode)
                                    {
                                        #region NetWorkMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Client Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Network Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Network Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Port":
                                                try
                                                {
                                                    rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                }
                                                catch (Exception)
                                                {
                                                    rec.CustomInt4 = 0;
                                                }
                                                break;
                                            case "Port":
                                                try
                                                {
                                                    rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                }
                                                catch (Exception)
                                                {
                                                    rec.CustomInt4 = 0;
                                                }
                                                break;
                                            case "Workstation Name":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else if (authenMode)
                                    {
                                        #region AuthenMode
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Authentication Package":
                                                string authenPack = appendArrayElements(lineArr);
                                                if (authenPack.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack.Contains("Kerberos"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;
                                            case "Pre-Authentication Type":
                                                string authenPack3 = appendArrayElements(lineArr);
                                                if (authenPack3.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack3.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack3.Contains("Kerberos"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;
                                            case "Logon Process":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "Message: Onur Other ");

                                        #region Other
                                        switch (lineArr[0].Trim())
                                        {
                                            case "Logon Type":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt3 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt3 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt3 = 0;
                                                }
                                                break;
                                            case "Error Code":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = 0;
                                                }
                                                break;
                                            case "Status Code":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = 0;
                                                }
                                                break;
                                            case "Failure Code":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = 0;
                                                }
                                                break;
                                            case "Caller Workstation":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            case "Workstation Name":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Workstation":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            case "User Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Account Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Client Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Logon account":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Caller User Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            case "Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Account Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Client Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Name":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Group Domain":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Caller Domain":
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                break;
                                            case "Target Domain":
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                break;
                                            case "Target User Name":
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Network Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Client Address":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Source Port":
                                                try
                                                {
                                                    rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                }
                                                catch (Exception)
                                                {
                                                    rec.CustomInt4 = 0;
                                                }
                                                break;
                                            case "Authentication Package":
                                                string authenPack = appendArrayElements(lineArr);
                                                if (authenPack.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack.Contains("Kerberos") || authenPack.Contains("KDS"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;
                                            case "Pre-Authentication Type":
                                                string authenPack2 = appendArrayElements(lineArr);
                                                if (authenPack2.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack2.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack2.Contains("Kerberos"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;
                                            case "Caller Process ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "PID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "Logon Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Logon Process":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Process Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Image File Name":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Duration":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = 0;
                                                }
                                                break;
                                            case "Object Name":
                                                rec.CustomStr8 = appendArrayElements(lineArr);
                                                break;
                                            case "Object Type":
                                                rec.CustomStr9 = appendArrayElements(lineArr);
                                                break;
                                            case "Operation Type":
                                                rec.CustomStr9 = appendArrayElements(lineArr);
                                                break;
                                            case "Handle ID":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = 0;
                                                }
                                                break;
                                            case "Primary User Name":
                                                if (rec.CustomStr1 == null)
                                                {
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                }
                                                break;
                                            case "Client User Name":
                                                if (rec.CustomStr2 == null)
                                                {
                                                    rec.CustomStr2 = appendArrayElements(lineArr);
                                                }
                                                break;//D.Ali Türkce Gelen Loglar Ýçin
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "Message: Onur Türkçe Other ");
                                            case "Kullanýcý Adý":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Ýþ Ýstasyonu Adý":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            case "Oturum Açma iþlemi":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Oturum Açma Türü":
                                                if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                    rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                else
                                                    rec.CustomInt5 = -1;
                                                break;
                                            case "Etki Alaný":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Kaynak Að Adresi":
                                                rec.CustomStr3 = appendArrayElements(lineArr);
                                                break;
                                            case "Oturum Hesabý":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Kaynak Ýþ Ýstasyonu":
                                                rec.CustomStr4 = appendArrayElements(lineArr);
                                                break;
                                            case "Share Name":
                                                rec.CustomStr8 = appendArrayElements(lineArr);
                                                break;
                                            case "Hesap Adý":
                                                if (string.IsNullOrEmpty(rec.CustomStr1))
                                                    rec.CustomStr1 = appendArrayElements(lineArr);
                                                else
                                                    rec.CustomStr2 = appendArrayElements(lineArr);
                                                break;

                                            //////////7

                                            case "Güvenlik Kimliði":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Hesap Etki Alaný":
                                                rec.CustomStr5 = appendArrayElements(lineArr);
                                                break;
                                            case "Oturum Açma Kimliði":
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                                break;
                                            case "Oturum Türü":
                                                if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                    rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                                else
                                                    rec.CustomInt5 = -1;
                                                break;

                                            case "Ýþlem Kimliði":
                                                if (!lineArr[1].Contains("-"))
                                                {
                                                    if (lineArr[1].Contains("0x"))
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                    }
                                                    else
                                                    {
                                                        rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                    }
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = 0;
                                                }
                                                break;
                                            case "Ýþlem Adý":
                                                rec.CustomStr6 = appendArrayElements(lineArr);
                                                break;
                                            case "Kaynak Baðlantý Noktasý":
                                                try
                                                {
                                                    rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                                }
                                                catch (Exception)
                                                {
                                                    rec.CustomInt4 = 0;
                                                }
                                                break;
                                            case "Kimlik Doðrulama Paketi":
                                                string authenPack4 = appendArrayElements(lineArr);
                                                if (authenPack4.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack4.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack4.Contains("Kerberos"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;
                                            case "Paket Adý (yalnýzca NTLM)":
                                                string authenPack3 = appendArrayElements(lineArr);
                                                if (authenPack3.Contains("Negotiate"))
                                                {
                                                    rec.CustomInt5 = 0;
                                                }
                                                else if (authenPack3.Contains("NTLM"))
                                                {
                                                    rec.CustomInt5 = 1;
                                                }
                                                else if (authenPack3.Contains("Kerberos") || authenPack3.Contains("KDS"))
                                                {
                                                    rec.CustomInt5 = 2;
                                                }
                                                else
                                                {
                                                    rec.CustomInt5 = 3;
                                                }
                                                break;

                                            default:
                                                break;
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Event Id = 560");

                        for (int i = 0; i < DescArr.Length; i++)
                        {
                            if (DescArr[i].Contains("Nesne Adý") || DescArr[i].Contains("Object Name"))
                            {
                                rec.CustomStr1 = DescArr[i].Split(':')[1] + ":" + DescArr[i].Split(':')[2];
                            }
                            if (DescArr[i].StartsWith("Eriþim") && !DescArr[i].StartsWith("Eriþim Maskesi") || DescArr[i].Contains("Access Mask"))
                            {
                                rec.CustomStr2 = DescArr[i].Split(':')[1].Trim();// +DescArr[i].Split(':')[2];
                            }

                            if (DescArr[i].Contains("Primary Domain"))
                            {
                                rec.CustomStr3 = DescArr[i].Split(':')[1].Trim();
                            }

                            if (DescArr[i].Contains("Object Type"))
                            {
                                rec.CustomStr3 = DescArr[i].Split(':')[1].Trim();
                            }

                            if (DescArr[i].Contains("Accesses"))
                            {
                                rec.CustomStr4 = DescArr[i].Split(':')[1].Trim();
                            }

                            if (!string.IsNullOrEmpty(rec.UserName) && rec.UserName.Contains('\\'.ToString(CultureInfo.InvariantCulture)))
                            {
                                rec.CustomStr5 = rec.UserName.Split('\\')[1];
                            }
                        }
                    }

                    if (rec.Description.Length > 900)
                    {
                        if (rec.Description.Length > 1800)
                        {
                            rec.CustomStr10 = rec.Description.Substring(900, 900);
                        }
                        else
                        {
                            rec.CustomStr10 = rec.Description.Substring(900, rec.Description.Length - 900);
                        }
                        rec.Description = rec.Description.Substring(0, 900);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");
                    }
                    for (int j = 0; j < DescArr.Length; j++)
                    {

                        L.Log(LogType.FILE, LogLevel.DEBUG, "Message: Onur Array" + DescArr[j]);
                        //if (DescArr[j].Contains("Nesne Sunucusu"))
                        //{

                        //    //rec.CustomStr1 = DescArr[i].Split(':')[1].Trim();
                        //    L.Log(LogType.FILE, LogLevel.DEBUG, "Message: Onur " + DescArr[j].Split(':')[1].Trim());

                        //}
                    }

                    string[] newArray = rec.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (newArray[i].Contains("Logon Type") || newArray[i].Contains("Oturum Türü"))
                        {
                            rec.CustomInt3 = Convert.ToInt32(newArray[i].Split(':')[1].Trim());
                        }

                        #region Onur Sarýkaya Ekledi.
                        //19.04.2012
                        /*
                        for (int j = 0; j < newArray.Length; j++)
                        {
                            L.Log(LogType.FILE, LogLevel.DEBUG, "Onur. Array Elements:" + newArray[i]);
                        }
                        */
                        try
                        {
                            if (newArray[i].Contains("Security ID"))
                            {
                                rec.CustomStr1 = GetName(newArray[i].Split(':')[1].Trim());
                            }

                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + rec.CustomStr1);

                        }

                        catch (Exception ex)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Security id to user name translate error. ");
                        }

                        if (string.IsNullOrEmpty(rec.CustomStr1) || rec.CustomStr1.Contains("S-"))
                        {
                            if (newArray[i].Contains("Account Name") || newArray[i].Contains("Hesap Adý"))
                            {
                                rec.CustomStr1 = newArray[i].Split(':')[1].Trim();
                            }
                        }

                        if (newArray[i].Contains("Account Domain") || newArray[i].Contains("Hesap Etki Alaný"))
                        {
                            rec.CustomStr5 = newArray[i].Split(':')[1].Trim();
                        }

                        if (newArray[i].Contains("Source Network Address") || newArray[i].Contains("Kaynak Að Adresi"))
                        {
                            rec.CustomStr3 = newArray[i].Split(':')[1].Trim();
                        }
                        #endregion
                    }
                }
                if ((mo["TimeGenerated"] != null))
                {
                    string dat = mo["TimeGenerated"].ToString().Split('.')[0];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated1:" + dat);
                    rec.Datetime = dat.Substring(0, 4) + "/" + dat.Substring(4, 2) + "/" + dat.Substring(6, 2) + " " + dat.Substring(8, 2) + ":" + dat.Substring(10, 2) + ":" + dat.Substring(12, 2); //+ "." + dat.Substring(14, 2);
                    rec.Datetime = Convert.ToDateTime(rec.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    last_recdate = rec.Datetime;
                    L.Log(LogType.FILE, LogLevel.DEBUG, "TimeGenerated:" + rec.Datetime);
                }
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
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                return false;
            }
            finally
            {
            }
        }

        /// <summary>
        /// The method converts SID string (user, group) into object name.
        /// Bu fonksiyon 06.08.2012 tarihinde Hazine Müsteþarlýðýnda bulunan sistemdeki 
        /// SID(Security Id)'leri anlamlý olan name'e translate etmek amacý ile Onur Sarýkaya tarafýndan eklenmiþtir. 
        /// </summary>
        /// <param name="name">SID string.</param>
        /// <returns>Object name in form domain\object_name.</returns>
        public String GetName(string sid)
        {
            string account = "";
            try
            {
                account =
                    new System.Security.Principal.SecurityIdentifier(sid).Translate(
                        typeof(System.Security.Principal.NTAccount)).ToString();
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error on GetName: " + exception.Message);
                account = sid;
            }
            return account;

        } //GetName

        private string appendArrayElements(string[] arr)
        {
            string totalString = "";
            for (int i = 1; i < arr.Length; i++)
            {
                totalString += ":" + arr[i].Trim();
            }
            return totalString.TrimStart(":".ToCharArray()).TrimEnd(":".ToCharArray());
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
                EventLog.WriteEntry("Security Manager NtEventlog Recorder", er.ToString(), EventLogEntryType.Error);
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
    }
}
