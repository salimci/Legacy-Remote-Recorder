using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Log;
using Microsoft.Win32;
using Natek.Helpers;

namespace EventLogFileAuditRecorder
{
    public class RemoteKernelPath
    {
        protected string user;
        protected string ip;
        protected string password;

        protected ObjectValue<int> status;
        protected DateTime lastRefresh;
        protected Dictionary<string, string[]> translations;

        public long RefreshPeriod { get; set; }
        public CLogger Logger { get; set; }

        public RemoteKernelPath(string ip, string user, string password)
        {
            this.user = user;
            this.ip = ip;
            this.password = password;
            status = new ObjectValue<int>(0);
            lastRefresh = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));
            translations = new Dictionary<string, string[]>();
            RefreshPeriod = 3600000;
        }

        public string this[string remotePath]
        {
            get
            {
                bool needRefresh;
                int lastStatus;
                remotePath = remotePath.ToLowerInvariant();
                do
                {
                    lock (status)
                    {
                        lastStatus = status;
                        needRefresh = status == 0 ||
                                      RefreshPeriod > 0 &&
                                      DateTime.Now.Subtract(lastRefresh).TotalMilliseconds > RefreshPeriod;
                        if (needRefresh)
                        {
                            status.Value = 1;
                            break;
                        }
                        if (status == 1)
                            Monitor.Wait(status);
                        else
                            break;
                    }
                } while (true);
                if (needRefresh)
                    Refresh(lastStatus);
                lock (translations)
                {
                    string[] path;
                    return translations.TryGetValue(remotePath, out path) ? path[0] : null;
                }
            }
        }

        private void Refresh(int backupStatus)
        {
            try
            {
                if (Logger != null)
                    Logger.Log(LogType.FILE, LogLevel.DEBUG, "Refreshing with backup status:" + backupStatus);
                var port = GetRemotePort();
                if (port <= 0)
                    return;
                if (Logger != null)
                    Logger.Log(LogType.FILE, LogLevel.DEBUG, "Refreshing remote port:" + port);
                using (var tcp = new TcpClient(ip, port))
                {
                    using (var reader = new StreamReader(tcp.GetStream(), Encoding.UTF8))
                    {
                        using (var writer = new StreamWriter(tcp.GetStream(), Encoding.UTF8))
                        {
                            if (Logger != null)
                                Logger.Log(LogType.FILE, LogLevel.DEBUG, "Sending All Command");
                            writer.WriteLine("All");
                            writer.Flush();
                            var trans = new Dictionary<string, string[]>();
                            var line = reader.ReadLine();
                            if (line != "BEGIN")
                                return;
                            var sp = new[] { ';' };
                            while ((line = reader.ReadLine()) != null && line != "END")
                            {
                                if (line.Length == 0)
                                    continue;
                                var parts = line.Split(sp, 3);
                                if (parts.Length != 3 || parts[0] != "S")
                                    break;
                                if (Logger != null)
                                    Logger.Log(LogType.FILE, LogLevel.DEBUG, "Read Parts[1]:" + parts[1] + ", Parts[2]:" + parts[2]);
                                trans[parts[2].ToLowerInvariant()] = new[] { parts[1], parts[2] };
                            }
                            if (line == "END")
                            {
                                lock (status)
                                {
                                    lastRefresh = DateTime.Now;
                                    translations = trans;
                                    backupStatus = 2;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Log(LogType.FILE, LogLevel.ERROR, "Refresh failed:" + e);
            }
            finally
            {
                lock (status)
                {
                    status.Value = backupStatus;
                }
            }
        }

        private int GetRemotePort()
        {
            try
            {
                var ms = new ManagementScope
                    {
                        Path = { Server = ip, NamespacePath = "\\root\\cimv2" },
                        Options = { Username = user, Password = password }
                    };
                ms.Connect();

                var registry = new ManagementClass(ms, new ManagementPath("StdRegProv"), null);
                var inParams = registry.GetMethodParameters("GetDWORDValue");
                inParams["hDefKey"] = 0x80000002;// HKEY_LOCAL_MACHINE;
                inParams["sSubKeyName"] = @"SOFTWARE\QueryDosPath";
                inParams["sValueName"] = "CurrentPort";

                var outParams = registry.InvokeMethod("GetDWORDValue", inParams, null);

                return outParams != null && outParams.Properties["uValue"].Value != null
                           ? (int)(UInt32)outParams.Properties["uValue"].Value
                           : 0;
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Log(LogType.FILE, LogLevel.DEBUG, "Getting remote port from:" + ip + " failed: " + e);
            }
            return -1;
        }
    }
}
