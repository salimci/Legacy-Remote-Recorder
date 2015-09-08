using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using CustomTools;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh.Apache;
using Natek.Recorders.Remote.Unified.ApacheErrorUnifiedRecorder;

using Natek.Recorders.Remote.Unified.ApacheSyslogUnifiedRecorder;
using Natek.Recorders.Remote.Unified.Dhcp;

using Natek.Recorders.Remote.Unified.F5LoadBalancerUnifiedRecorder;
using Natek.Recorders.Remote.Unified.KerioMailUnifiedRecorder;
using Natek.Recorders.Remote.Unified.LabrisAccessUnifiedRecorder;
using Natek.Recorders.Remote.Unified.LabrisNetworkSyslogUnifiedRecorder;
using Natek.Recorders.Remote.Unified.LinuxGeneralPurposeUnifiedRecorder;
using Natek.Recorders.Remote.Unified.LinuxJobsUnifiedRecorder;
using Natek.Recorders.Remote.Unified.MysqlUnifiedRecorder;
using Natek.Recorders.Remote.Unified.NetsafeFirewallUnifiedRecorder;
using Natek.Recorders.Remote.Unified.NginxErrorUnifiedRecorder;

using Natek.Recorders.Remote.Unified.PfSenseUnifiedRecorder;
using Natek.Recorders.Remote.Unified.PhpFpmUnifiedRecorder;
using Natek.Recorders.Remote.Unified.RadiusUnifiedRecorder;
using RemoteRecorderTest.Enum;
using LogMgr;
using Natek.Recorders.Remote.Unified.JuniperPendikUnifiedRecorder;
using Natek.Recorders.Remote.Unified.Microsoft.Exchange;
using Natek.Recorders.Remote.Unified.SambaUnifiedRecorder;
using Natek.Recorders.Remote;
using Natek.Recorders.Remote.Unified.PaloAltoUnified;

namespace RemoteRecorderTest
{
    public class Program
    {
        public static readonly int MaxCell = 23;

        private static void Main(string[] args)
        {
            //OldRecorderTest();
            RunRemoteRecorderTest(args);

            Console.Write("Press any key to terminate...");
            Console.ReadKey();
        }


        /*
        private static void OldRecorderTest()
        {
            var pArgs = InitArgs();
            RunOldRecorderTest(typeof(FortiGateSyslogV_1_0_2Recorder.FortiGateSyslogV_1_0_2Recorder), pArgs, 1);
        }
        */
        private static void RunOldRecorderTest(Type clsRecorder, RecorderArgs args, int id)
        {
            try
            {

                var cInfo = clsRecorder.GetConstructor(Type.EmptyTypes);
                if (cInfo == null) return;

                var recorder = (CustomBase)cInfo.Invoke(null);

                recorder.SetConfigData(id, args.Location, args.LastLine, args.LastPosition, args.LastFile, args.LastKeywords,
                    args.FromEndOnLoss, args.MaxLineToWait, args.User, args.Password, args.RemoteHost, args.SleepTime, args.TraceLevel,
                    args.CustomVar1, args.CustomVar2, args.VirtualHost, args.Dal, args.TimeZone);



                recorder.Init();
                recorder.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void RunRemoteRecorderTest(string[] args)
        {
            //  SqlQueries.InsertDefaultRemoteRecorderArgs();
            var pArgs = InitArgs();

            //pArgs.OutputLocation = @"T:\tmp\tippinglogs";
            TestConfig.OutputMode = TestOutputMode.ToFile;
            pArgs.OutputLocation = (TestConfig.OutputMode == TestOutputMode.ToFile)
                ? pArgs.OutputLocation + @"\report.txt"
                : null;
            RunRecorderTest(typeof(PaloAltoUnifiedSyslogRecorder), pArgs, 1, OnSyslogStarted);
            //RunRecorderTest(typeof(PaloAltoTrafficUnifiedRecorder), pArgs, 1);
        }
        /*
        private static RecorderArgs PArgsForDbRecorder()
        {
            var pArgs = new RecorderArgs
            {
                Location = @"RemoteRecorderTest",
                LastLine = "",
                LastPosition = "",
                LastFile = "",
                LastKeywords = "QUERY_1=\"" + @"SELECT id as RecordNum,username as Customstr1,
                                methodname as customstr2,
                                ip as customstr3,
                                servicetime as customint6,
                                to_char(date,'yyyy/MM/dd HH24:mi:ss') as datetime" +
                                @" FROM tkgm.logs
                                WHERE id >@RECORDNUM_1 AND date >= to_timestamp('@RECORDDATE_1','yyyy/MM/dd HH24:mi:ss')
                                ORDER BY id,date" + "\"",
                FromEndOnLoss = false,
                MaxLineToWait = 1000000,
                User = "postgres",
                Password = "",
                RemoteHost = "localhost",
                SleepTime = 1000,
                TraceLevel = 4,
                CustomVar1 = "",
                CustomVar2 = 0,
                VirtualHost = "test",
                Dal = "natekdb",
                TimeZone = 0,
                LastUpdated = DateTime.Now
            };
            pArgs.OutputLocation = @"C:\Users\burcu.coskun\Documents\testtmp\exchange\";
            return pArgs;
        }

        */
        static void OnSyslogStarted(CustomBase recorder)
        {
            var syslog = recorder as SyslogRecorderBase;
            Console.Write("begin>");
            Console.ReadKey();

            if (syslog != null)
            {
                using (var s = new StreamReader(Path.Combine(syslog.Location, syslog.LastFile), syslog.Encoding))
                {
                    string line;
                    while ((line = s.ReadLine()) != null)
                    {
                        syslog.ProcessSyslogEvent(new LogMgrEventArgs("test-env", line, eventLogEntTypeP: EventLogEntryType.SuccessAudit));
                    }
                }
            }
        }

        private static RecorderArgs InitArgs()
        {
            var pArgs = new RecorderArgs();

            switch (TestConfig.InputMode)
            {
                case TestInputMode.FromFile:
                    DataCenter.SetValuesFromFile(pArgs);
                    break;
                case TestInputMode.FromConsole:
                    DataCenter.SetValuesFromConsole(pArgs);
                    break;
                case TestInputMode.FromDb:
                    DataCenter.SetValuesFromDb(pArgs);
                    break;
            }

            return pArgs;
        }

        private delegate void RecorderStartedDelegate(CustomBase recorder);

        private static void RunRecorderTest(Type clsRecorder, RecorderArgs args, int id, RecorderStartedDelegate onRecorderStarted = null)
        {
            try
            {
                InitOutputHeader(args.OutputLocation);

                ConstructorInfo cInfo = clsRecorder.GetConstructor(Type.EmptyTypes);
                if (cInfo != null)
                {
                    var recorder = (CustomBase)cInfo.Invoke(null);

                    recorder.GetInstanceListService()["Security Manager Remote Recorder"] =
                        new MockSecurityManagerRemoteRecorder
                        {
                            OutputEnabled = !string.IsNullOrEmpty(args.OutputLocation),
                            OutputFile = args.OutputLocation
                        };

                    recorder.GetInstanceListService()["Security Manager Sender"] =
                        new MockSecurityManagerRemoteRecorder
                        {
                            OutputEnabled = !string.IsNullOrEmpty(args.OutputLocation),
                            OutputFile = args.OutputLocation
                        };


                    recorder.SetConfigData(id, args.Location, args.LastLine, args.LastPosition, args.LastFile, args.LastKeywords,
                        args.FromEndOnLoss, args.MaxLineToWait, args.User, args.Password, args.RemoteHost, args.SleepTime, args.TraceLevel,
                        args.CustomVar1, args.CustomVar2, args.VirtualHost, args.Dal, args.TimeZone);

                    if (args.OutputLocation != null)
                    {
                        var fInfo = new FileInfo(args.OutputLocation);
                        if (fInfo.Directory != null)
                        {
                            var pInfo = clsRecorder.GetProperty("LogLocation");
                            if (pInfo != null)
                                pInfo.SetValue(recorder, fInfo.Directory.FullName, null);
                        }
                    }

                    recorder.Init();
                    recorder.Start();
                    if (onRecorderStarted != null)
                        onRecorderStarted(recorder);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void InitOutputHeader(string outFile)
        {
            if (string.IsNullOrEmpty(outFile))
                return;
            var inf = new FileInfo(outFile);
            if (inf.Directory != null && !inf.Directory.Exists)
                return;
            using (var fs = new StreamWriter(outFile, false))
            {
                var begin = 0;
                var end = MaxCell;
                bool next;
                var format = "{0,-" + MaxCell + "}|";
                do
                {
                    next = false;
                    foreach (var f in typeof(CustomBase.Rec).GetFields())
                    {
                        string s;
                        if (f.Name.Length > end)
                        {
                            next = true;
                            s = f.Name.Substring(begin, MaxCell);
                        }
                        else
                            s = f.Name.Substring(begin, f.Name.Length - begin);
                        fs.Write(format, s);
                    }
                    fs.WriteLine();
                    begin = end;
                    end += MaxCell;
                } while (next);
                PrintLine(fs, "-", MaxCell, typeof(CustomBase.Rec).GetFields().Length);
            }
            using (var fs = new StreamWriter(outFile + ".setreg", false))
            {
                fs.WriteLine("Identity\tLastPosition\tLastLine\tLastFile\tLastKeywords");
            }
        }

        public static void PrintLine(StreamWriter fs, string ch, int maxCh, int fCount)
        {
            while (--fCount >= 0)
            {
                for (var i = 0; i < maxCh; i++)
                    fs.Write(ch);
                fs.Write("|");
            }
            fs.WriteLine();
        }
    }
}
