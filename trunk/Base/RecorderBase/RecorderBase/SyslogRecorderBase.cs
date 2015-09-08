using System;
using System.Net.Sockets;
using System.Text;
using Log;
using LogMgr;
using Natek.Helpers.Execution;
using Natek.Helpers.IO;
using System.IO;

namespace Natek.Recorders.Remote
{
    public abstract class SyslogRecorderBase : PeriodicRecorder
    {
        protected ProtocolType protocolType = ProtocolType.Udp;
        protected int port = 514;
        protected SyslogRecorderContext commonContext;
        protected string syslogOutputFile;
        protected int syslogOutputFileSize;
        protected int receiveBufferSize;

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new SyslogRecorderContext(this, protocolType, RemoteHost, port);
        }

        protected override NextInstruction ValidateGlobalParameters()
        {
            var ins = base.ValidateGlobalParameters();
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            if (syslogOutputFileSize <= 0)
            {
                syslogOutputFileSize = 1024 * 1024;
                Log(LogLevel.INFORM, "Syslog Output File Size set to default:" + syslogOutputFileSize);
            }
            return NextInstruction.Do;
        }

        protected override NextInstruction DoLogic(RecorderContext context)
        {
            try
            {
                lock (SyncRoot)
                {
                    if (commonContext != null)
                    {
                        Log(LogLevel.ERROR, "Cannot assign a new context while there is already a common instance");
                        return NextInstruction.Abort;
                    }
                    if (context == null)
                    {
                        Log(LogLevel.ERROR, "Null context. Expected an instance");
                        return NextInstruction.Abort;
                    }
                    var ctxSyslog = context as SyslogRecorderContext;
                    if (ctxSyslog == null)
                    {
                        Log(LogLevel.ERROR, "SyslogRecorderContext expected but incompatible context is given");
                        return NextInstruction.Abort;
                    }

                    Log(LogLevel.DEBUG, "Create Reader");
                    Exception error = null;
                    if (!ctxSyslog.CreateReader(ref error))
                    {
                        Log(LogLevel.DEBUG, "Create Syslog Reader Abort:" + error);
                        return NextInstruction.Abort;
                    }
                    if (!string.IsNullOrEmpty(syslogOutputFile))
                    {
                        Log(LogLevel.DEBUG, string.Format("Output Redirect to File({0}) LogSize({1})", syslogOutputFile, syslogOutputFileSize));
                        ctxSyslog.SyslogInstance.EnablePacketLog = true;
                        ctxSyslog.SyslogInstance.Encoding = encoding;
                        ctxSyslog.SyslogInstance.LogFile = syslogOutputFile;
                        ctxSyslog.SyslogInstance.LogSize = syslogOutputFileSize;
                    }
                    else
                    {
                        Log(LogLevel.DEBUG, "No log file to redirect output");
                    }

                    if (receiveBufferSize > 0)

                        ctxSyslog.SyslogInstance.ReceiveBufferSize = receiveBufferSize;

                    ctxSyslog.SyslogInstance.SyslogEvent += ProcessSyslogEvent;
                    commonContext = ctxSyslog;
                    ctxSyslog.SyslogInstance.Start();
                    return NextInstruction.Return;
                }
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Initializing common context failed with error:" + e);
            }
            return NextInstruction.Abort;
        }

        public virtual void ProcessSyslogEvent(LogMgrEventArgs args)
        {
            Exception error = null;
            string[] fields = null;
            try
            {
                commonContext.InputRecord = new TextRecord { RecordText = args.Message };
                Log(LogLevel.DEBUG, "record text ===>" + args.Message);
                ExecuteRecordProcessStages(commonContext, ref fields, ref error);
            }
            catch (Exception e)
            {
                error = e;
            }
            if (error != null)
            {
                Log(LogLevel.ERROR, "Error while processing record:" + error);
            }
        }

        public override NextInstruction PerformRecorderLogic(ref Exception error)
        {
            lock (SyncRoot)
            {
                if (commonContext == null)
                {
                    var ins = base.PerformRecorderLogic(ref error);
                    if (ins != NextInstruction.Return)
                        return ins;
                }
                if (commonContext != null && SleepTime < 60000)
                {
                    sleepTime = 60000;
                }
                return NextInstruction.Return;
            }
        }

        /// <summary>
        /// GUI part track the changes on the position value.
        /// Value of the position chage with the date in the legacy recorder. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override string GetContextPosition(RecorderContext context)
        {
            return context.LastRecordDate;
        }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            switch (keyword)
            {
                case "Proto":
                    touchCount++;
                    if (String.Compare(value, "tcp", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        protocolType = ProtocolType.Tcp;
                    }
                    break;
                case "Port":
                    touchCount++;
                    int vPort;
                    if (int.TryParse(value, out vPort) && vPort > 0)
                    {
                        port = vPort;
                    }
                    else
                    {
                        error = new Exception("Invalid Port Value:[" + value + "]");
                        return false;
                    }
                    break;
                case "Lf":
                    touchCount++;
                    if (!FileSystemHelper.CreateDirectoryOf(value, out error))
                        return false;

                    using (var sw = new StreamWriter(value, true)) { }
                    syslogOutputFile = value;
                    break;
                case "Ls":
                    int.TryParse(value, out syslogOutputFileSize);
                    touchCount++;
                    break;
                case "Rbs":
                    int.TryParse(value, out receiveBufferSize);
                    touchCount++;
                    break;
            }
            return true;
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        public override string GetInputName(RecorderContext context)
        {
            return protocolType + "://" + remoteHost + ":" + port;
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            if (MappingInfos != null)
            {
                foreach (var mappingInfo in MappingInfos)
                {
                    context.SourceHeaderInfo = MimicMappingInfo(mappingInfo.Mappings);
                    context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                    break;
                }
            }
            return NextInstruction.Do;
        }

        protected override void PrepareKeywords(RecorderContext context, StringBuilder keywordBuffer)
        {
        }
    }
}
