using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Natek.Helpers;
using Natek.Helpers.Log;
using Timer = System.Timers.Timer;

namespace SvcQueryDosPath
{
    public partial class SvcQueryDosPath : ServiceBase
    {
        public static readonly int DefaultTimerPeriod = 5000;
        public static readonly int DefaultLogSize = 20 * 1024;

        protected object syncRoot;
        protected Timer timer;
        protected int period;
        protected int port;
        protected int logSizeKb;
        protected string logFile;
        protected ObjectValue<int> serverStatus;
        protected TcpListener activeServer;
        protected ObjectValue<string> instanceName;

        public SvcQueryDosPath()
        {
            InitializeComponent();

            instanceName = new ObjectValue<string>();
            logFile = "svcQueryDosPath.log";
            syncRoot = new object();
            period = DefaultTimerPeriod;
            logSizeKb = DefaultLogSize;
            serverStatus = new ObjectValue<int>();
            timer = new Timer
            {
                AutoReset = false,
                Interval = period
            };
            timer.Elapsed += OnTimerElapsed;
        }

        public virtual string InstanceName
        {
            get
            {
                lock (instanceName)
                {
                    if (instanceName.Value == null)
                        instanceName.Value = ReadServiceName();
                    return instanceName.Value;
                }
            }
        }

        protected void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (syncRoot)
                {
                    if (!ReadRegistry())
                        return;
                    StartServer();
                }
            }
            finally
            {
                RestartTimer();
            }
        }

        protected string ReadServiceName()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service where ProcessId = " + Process.GetCurrentProcess().Id))
            {
                var collection = searcher.Get();
                return collection.Count > 0
                           ? (string)collection.Cast<ManagementBaseObject>().First()["Name"]
                           : ServiceName;
            }
        }

        protected bool StartServer()
        {
            try
            {
                lock (syncRoot)
                {
                    if (serverStatus > 0)
                        return true;
                    TcpListener server = null;
                    try
                    {
                        int i;
                        for (i = 0; i < 5; i++)
                        {
                            try
                            {
                                server = new TcpListener(IPAddress.Any, port);
                                server.Start();
                                break;
                            }
                            catch
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        if (i == 5)
                        {
                            server = new TcpListener(IPAddress.Any, 0);
                            server.Start();
                        }

                        if (SetRegistry(Registry.LocalMachine, @"SOFTWARE\" + InstanceName, "CurrentPort",
                                                          (server.LocalEndpoint as IPEndPoint).Port, RegistryValueKind.DWord))
                        {
                            lock (serverStatus)
                            {
                                activeServer = server;
                                new Thread(args =>
                                    {
                                        if (!SetServerStatus(1))
                                            return;
                                        Server(server);
                                    }).Start();
                                Monitor.Wait(serverStatus);
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        lock (serverStatus)
                        {
                            if (serverStatus == 0)
                            {
                                activeServer = null;
                                StopServer(server);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Start Server", EventLogEntryType.Error, "Error while trying to start server: {0}", e);
            }
            return false;
        }

        protected delegate void QueryDosHandler(TcpClient client);

        protected void ClientQueryDosHandler(TcpClient client)
        {
            try
            {
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Client handler", null, "Client {0} accepted",
                              client.Client.RemoteEndPoint.ToString());
                using (var reader = new StreamReader(client.GetStream(), Encoding.UTF8))
                {
                    using (var writer = new StreamWriter(client.GetStream(), Encoding.UTF8))
                    {
                        var line = string.Empty;
                        var lenBuffer = new byte[4];
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Equals("q", StringComparison.InvariantCultureIgnoreCase))
                            {
                                CloseClient(client);
                                return;
                            }
                            string output;
                            if (line.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                                output = AllVolumeInfo();
                            else if (line.StartsWith("P2V:", StringComparison.InvariantCultureIgnoreCase))
                                output = Path2Volume(line.Substring(4));
                            else if (line.StartsWith("V2P:", StringComparison.InvariantCultureIgnoreCase))
                                output = Volume2Path(line.Substring(4));
                            else
                                output = "F:0:Unknown command:" + line;
                            writer.WriteLine("BEGIN");
                            writer.WriteLine(output);
                            writer.WriteLine("END");
                            writer.Flush();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private string AllVolumeInfo()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var drive in DriveInfo.GetDrives())
                {
                    var i = drive.Name.Length;
                    while (--i >= 0 && drive.Name[i] == Path.DirectorySeparatorChar)
                    {
                    }
                    var driveName = drive.Name.Substring(0, ++i);
                    var driveVolume = Kernel32.QueryDosDevice(driveName);
                    if (driveVolume != null)
                    {
                        sb.Append("S;").Append(driveName).Append(';').AppendLine(driveVolume);
                    }
                }
                return sb.Length > 0 ? sb.ToString() : "F:0:No volume info can be constructed";
            }
            catch (Exception e)
            {
                return string.Format("F:-1:Exception while translating all volume info failed", e.Message);
            }
        }

        private string Volume2Path(string volume)
        {
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    var i = drive.Name.Length;
                    while (--i >= 0 && drive.Name[i] == Path.DirectorySeparatorChar)
                    {
                    }
                    var driveName = drive.Name.Substring(0, ++i);
                    var driveVolume = Kernel32.QueryDosDevice(driveName);
                    if (driveVolume != null && volume.StartsWith(driveVolume, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (driveVolume.Length == volume.Length)
                            return "S:0:" + driveName;
                        if (volume[driveVolume.Length] == Path.DirectorySeparatorChar)
                            return "S:0:" + driveName + volume.Substring(driveVolume.Length);
                    }
                }
                return string.Format("F:0:No volume info for {0}", volume);
            }
            catch (Exception e)
            {
                return string.Format("F:-1:Exception while translating volume to path failed for {0} due to {1}", volume, e.Message);
            }
        }

        private string Path2Volume(string path)
        {
            try
            {
                var volume = Kernel32.QueryDosDevice(path);
                if (volume == null)
                    return "F:1:Translate path to volume failed for " + path;
                return "S:0:" + volume;
            }
            catch (Exception e)
            {
                return string.Format("F:-1:Exception while translating path to volume failed for {0} due to {1}", path, e.Message);
            }
        }

        private void CloseClient(TcpClient client)
        {
            if (client != null)
            {
                try
                {
                    client.Close();
                }
                catch
                {
                }
            }
        }

        private void Server(TcpListener server)
        {
            try
            {
                TcpClient tcpClient = null;
                try
                {
                    while (ActiveServer(server))
                    {
                        tcpClient = server.AcceptTcpClient();
                        new QueryDosHandler(ClientQueryDosHandler).BeginInvoke(tcpClient, null, null);
                    }
                }
                finally
                {
                    CloseClient(tcpClient);
                }
            }
            catch (Exception e)
            {
                StopServer(server);
                lock (serverStatus)
                {
                    if (activeServer != server)
                        return;
                    activeServer = null;
                }
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Server", EventLogEntryType.Error,
                                  "Server terminated due to:" + e);
            }
        }

        private bool ActiveServer(TcpListener server)
        {
            lock (serverStatus)
            {
                if (server != activeServer && serverStatus != 0)
                {
                    StopServer(server);
                    return false;
                }
                return true;
            }
        }

        private void StopServer(TcpListener server)
        {
            try
            {
                server.Stop();
            }
            catch
            {
            }
        }

        private bool SetServerStatus(int status)
        {
            try
            {
                lock (serverStatus)
                {
                    try
                    {
                        serverStatus.Value = status;
                        return true;
                    }
                    finally
                    {
                        Monitor.Pulse(serverStatus);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Set Server Status", EventLogEntryType.Error, "Setting server status to {0} failed: {1}", status, e);
                return false;
            }
        }

        private bool SetRegistry(RegistryKey root, string path, string name, object value, RegistryValueKind kind)
        {
            try
            {
                using (var reg = root.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (reg != null)
                    {
                        reg.SetValue(name, value, kind);
                        return true;
                    }
                }
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Set Registry", EventLogEntryType.Error, "Setting value {0} to {1} failed: Registry {3} does not exist", name, value, root + "\\" + path);
            }
            catch (Exception e)
            {
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Set Registry", EventLogEntryType.Error, "Setting value {0} to {1} failed: {2}", name, value, e);
            }
            return false;
        }

        private bool ReadRegistry()
        {
            try
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\" + InstanceName, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (reg != null)
                    {
                        ReadRegistry(reg);
                    }
                    else
                    {
                        using (var regSvc =
                            Registry.LocalMachine.CreateSubKey(
                                @"SOFTWARE\" + InstanceName,
                                RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            ReadRegistry(regSvc);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Log(logFile, logSizeKb, DateTime.Now, "Read Registry", EventLogEntryType.Error, "Error occured while trying to read registry values: {0}", e);
                return false;
            }
        }

        private void ReadRegistry(RegistryKey regSvc)
        {
            period = ReadRegistry(regSvc, "TimerPeriod", RegistryValueKind.DWord, DefaultTimerPeriod);
            port = ReadRegistry(regSvc, "CurrentPort", RegistryValueKind.DWord, 0);
            logSizeKb = ReadRegistry(regSvc, "LogSizeInKB", RegistryValueKind.DWord, DefaultLogSize);
        }

        private T ReadRegistry<T>(RegistryKey regSvc, string name, RegistryValueKind kind, T defaultValue)
        {
            var v = regSvc.GetValue(name);
            if (v == null)
            {
                regSvc.SetValue(name, defaultValue, kind);
                return defaultValue;
            }
            return (T)v;
        }

        protected void RestartTimer()
        {
            lock (syncRoot)
            {
                try
                {
                    timer.Interval = period <= 0 ? DefaultTimerPeriod : period;
                }
                finally
                {
                    timer.Enabled = true;
                }
            }
        }

        public void Run(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogHelper.Log(EventLogEntryType.Information, DateTime.Now, InstanceName, "{0} starting", InstanceName);
                timer.Start();
            }
            catch (Exception e)
            {
                LogHelper.Log(EventLogEntryType.Error, DateTime.Now, "OnStart", "An error occured while trying to start timer: {0}", e);
            }
        }

        protected override void OnStop()
        {
            LogHelper.Log(EventLogEntryType.Information, DateTime.Now, InstanceName, "{0} stopping", InstanceName);
            lock (syncRoot)
            {
                StopServer();
                DisposeHelper.Close(timer);
            }
        }

        private void StopServer()
        {
            lock (serverStatus)
            {
                try
                {
                    if (activeServer != null)
                    {
                        activeServer.Stop();
                    }
                }
                finally
                {
                    activeServer = null;
                    SetServerStatus(0);
                }
            }
        }
    }
}
