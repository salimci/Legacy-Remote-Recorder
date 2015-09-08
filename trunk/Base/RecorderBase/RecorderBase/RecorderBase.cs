using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CustomTools;
using Log;
using Microsoft.Win32;
using Natek.Helpers.Config;
using Natek.Helpers.CSharp;
using Natek.Helpers.Execution;
using Natek.Helpers.Limit;
using Natek.Helpers.Log;
using Natek.Helpers.Security.Logon;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote
{
    public delegate NextInstruction ContextProcessor(RecorderContext context, ref Exception error);

    public abstract class RecorderBase : CustomBase
    {
        #region statics
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

        public static void ClearRecord(RecWrapper rec)
        {
            foreach (var pInfo in typeof(RecWrapper).GetProperties())
            {
                pInfo.SetValue(rec, pInfo.PropertyType.IsValueType ? Activator.CreateInstance(pInfo.PropertyType) : null, null);
            }
        }
        #endregion

        #region delegates
        public delegate NextInstruction InitBufferDelegate(RecorderContext context, string source);
        public delegate NextInstruction OnMatchDelegate(RecorderContext context, string source, ref Match match);
        public delegate bool CanAddMatchDelegate(RecorderContext context, string source, Match match, Match lastMatch, out string matchValue);
        public delegate string[] BufferToArrayDelegate(RecorderContext context, string source);
        public delegate void SetLastMatchDelegate(RecorderContext context, string source, Match match);
        #endregion

        #region fields
        protected long MaxTextLength = 900;

        protected int identity;
        protected string location;
        protected string lastLine;
        protected string lastPosition;
        protected string lastFile;
        protected string lastKeywords;
        protected string lastRecordDate;
        protected long inputModifiedOn;
        protected readonly long[] headerOffset = { 0, 0 };
        protected bool fromEndOnLoss;
        protected int maxRecordSend;
        protected string user;
        protected string domain;
        protected string password;
        protected string remoteHost;
        protected int sleepTime;
        protected int traceLevel;
        protected string customVar1;
        protected int customVar2;
        protected string virtualhost;
        protected string dal;
        protected int zone;
        protected RecorderStatus status;
        protected Encoding encoding;
        protected CLogger logger;
        protected uint loggingInterval;
        protected uint logSize;
        protected string logFile;
        protected LogType logType = LogType.FILE;
        protected LogType defaultLogType = LogType.EVENTLOG;
        protected DataMappingInfo[] mappingInfos;
        protected Regex headerSeparators;
        protected Regex fieldSeparators;

        protected Dictionary<string, ConstraintCollection<RecWrapper>> ConstrainedDataLookup;
        #endregion

        #region constructor(s)
        // ReSharper disable PublicConstructorInAbstractClass
        public RecorderBase()
            // ReSharper restore PublicConstructorInAbstractClass
            : this(60000)
        {
        }

        // ReSharper disable PublicConstructorInAbstractClass
        public RecorderBase(uint loggingInterval)
        // ReSharper restore PublicConstructorInAbstractClass
        {
            SyncRoot = new object();

            this.loggingInterval = loggingInterval;
            status = RecorderStatus.None;
            encoding = Encoding.GetEncoding(Encoding.Default.CodePage);

            InitializeconstraintDataLookUp();

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            InitializeComponent();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        protected virtual void InitializeconstraintDataLookUp()
        {
            ConstrainedDataLookup = new Dictionary<string, ConstraintCollection<RecWrapper>>();

            foreach (var property in typeof(RecWrapper).GetProperties(BindingFlags.Instance
                | BindingFlags.GetProperty
                | BindingFlags.SetProperty
                | BindingFlags.Public))
            {
                if (typeof(string).IsAssignableFrom(property.PropertyType))
                {
                    var collection = new ConstraintCollection<RecWrapper>();
                    collection.AddConstraint(new TextSizeConstraint<RecWrapper>
                    {
                        Size = MaxTextLength,
                        Property = new TextProperty { PropertyInfo = property }
                    });
                    ConstrainedDataLookup[property.Name] = collection;
                }
            }
        }
        #endregion

        #region Absracts

        protected virtual void InitializeComponent()
        {

        }

        protected abstract RecorderContext CreateContextInstance(params object[] ctxArgs);
        protected abstract NextInstruction ValidateGlobalParameters();
        public abstract Regex CreateHeaderSeparator();
        public abstract Regex CreateFieldSeparator();
        protected abstract DataMappingInfo[] CreateMappingInfos();
        protected abstract NextInstruction DoLogic(RecorderContext context);
        protected abstract void PrepareKeywords(RecorderContext context, StringBuilder keywordBuffer);
        public abstract RecordInputType InputTextType(RecorderContext context, ref Exception error);
        protected abstract string GetHeaderText(RecorderContext context);
        public abstract string GetInputName(RecorderContext context);
        #endregion

        #region properties
        protected Regex FieldSeparators
        {
            get
            {
                lock (SyncRoot)
                {
                    if (fieldSeparators == null)
                        fieldSeparators = CreateFieldSeparator();
                }
                return fieldSeparators;
            }
        }

        protected Regex HeaderSeparators
        {
            get
            {
                lock (SyncRoot)
                {
                    if (headerSeparators == null)
                        headerSeparators = CreateHeaderSeparator();
                }
                return headerSeparators;
            }
        }

        public object SyncRoot { get; set; }

        public int Identity { get { return identity; } }

        public string Location { get { return location; } }

        public string LastLine { get { return lastLine; } }

        public string LastPosition { get { return lastPosition; } }

        public string LastFile { get { return lastFile; } }

        public string LastKeywords { get { return lastKeywords; } }

        public string LastRecordDate { get { return lastRecordDate; } }

        public long InputModifiedOn { get { return inputModifiedOn; } }

        public long[] HeaderOffset { get { return headerOffset; } }

        public bool FromEndOnLoss { get { return fromEndOnLoss; } }

        public int MaxRecordSend { get { return maxRecordSend; } }

        public string User { get { return user; } }

        public string Domain { get { return domain; } }

        public string Password { get { return password; } }

        public string RemoteHost { get { return remoteHost; } }

        public int SleepTime { get { return sleepTime; } }

        public int TraceLevel { get { return traceLevel; } }

        public string CustomVar1 { get { return customVar1; } }

        public int CustomVar2 { get { return customVar2; } }

        public string Virtualhost { get { return virtualhost; } }

        public string Dal { get { return dal; } }

        public int Zone { get { return zone; } }

        public RecorderStatus Status { get { return status; } }

        public Encoding Encoding { get { return encoding; } }

        public CLogger Logger { get { return logger; } }

        public uint LoggingInterval { get { return loggingInterval; } }

        public uint LogSize { get { return logSize; } }

        public string LogFile { get { return logFile; } }

        public LogType LogType { get { return logType; } }

        public LogType DefaultLogType { get { return defaultLogType; } }
        #endregion

        #region methods

        protected virtual Dictionary<string, int> CreateFieldMappingIndexLookup(DataMappingInfo dataMappingInfo, RecorderContext context, IEnumerable<string> fields)
        {
            var fieldMappingLookup = new Dictionary<string, int>();

            if (dataMappingInfo == null) return fieldMappingLookup;

            foreach (var field in fields)
            {
                var index = FindMapIndex(dataMappingInfo.Mappings, field);
                fieldMappingLookup.Add(field, index);
            }
            return fieldMappingLookup;
        }

        protected virtual int FindMapIndex(DataMapping[] dataMappings, string field)
        {
            for (var i = 0; i < dataMappings.Length; i++)
            {
                if (dataMappings[i].Original.Any(j => j.Any(k => k.Equals(field))))
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual IEnumerable<string> GetFieldMappingsFields()
        {
            return null;
        }

        protected virtual RecorderContext CreateContext(params object[] ctxArgs)
        {
            Log(LogLevel.DEBUG, "CreateContext->Create Instance");
            var context = CreateContextInstance(ctxArgs);
            Log(LogLevel.DEBUG, "CreateContext->Init Context Instance");
            InitContextInstance(context, ctxArgs);
            Log(LogLevel.DEBUG, "CreateContext->Return Context Instance");
            return context;
        }

        protected virtual void InitContextInstance(RecorderContext context, params object[] ctxArgs)
        {
            if (context != null)
            {
                context.LastFile = lastFile;
                context.LastKeywords = lastKeywords;
                context.LastLine = lastLine;
                context.LastRecordDate = lastRecordDate;
                var position = 0L;
                context.OffsetInStream = long.TryParse(lastPosition, out position) ? position : 0;
                context.Record = new RecWrapper();

                if (GetInstanceListService() != null)
                {
                    foreach (var s in GetInstanceListService())
                    {
                        Log(LogLevel.DEBUG, "Service:[" + s.Key + "," + s.Value.ServiceName + "]");
                    }
                }
                else
                    Log(LogLevel.DEBUG, "No Instance List Service");
                const string recSrv = "Security Manager Remote Recorder";
                context.Service = GetInstanceService(recSrv);
                if (context.Service == null)
                    Log(LogLevel.DEBUG, recSrv + " could not be found!!!!");
                context.InputEncoding = encoding;
            }
        }

        protected virtual string GetRecorderName()
        {
            return GetType().Name;
        }

        protected virtual Dictionary<string, int> MimicMappingInfo(IEnumerable<DataMapping> dataMapping)
        {
            var info = new Dictionary<string, int>();
            var index = 0;
            foreach (var mapping in dataMapping)
            {
                foreach (var orig in mapping.Original)
                {
                    info[orig[0]] = index++;
                }
            }
            return info;
        }

        protected virtual NextInstruction InitializeInstance()
        {
            return InitializeLogger();
        }

        public virtual NextInstruction PerformRecorderLogic(ref Exception error)
        {
            Log(LogLevel.DEBUG, "RecorderBase->Initialize");
            if (!Initialize())
            {
                Log(LogLevel.ERROR, "RecorderBase->Initialize Failed");
                return NextInstruction.Abort;
            }

            ImpersonationContext winImpCtx = null;
            try
            {
                Log(LogLevel.DEBUG, "RecorderBase->ValidateAccount");
                if (ValidateAccount(ref winImpCtx, ref error))
                {
                    Log(LogLevel.DEBUG, "RecorderBase->InitializeInstance");
                    var ins = InitializeInstance();
                    if ((ins & NextInstruction.Do) != NextInstruction.Do)
                    {
                        Log(LogLevel.DEBUG, "RecorderBase->InitializeInstance cause exit:" + ins);
                        return ins;
                    }
                    Log(LogLevel.DEBUG, "RecorderBase->Creating Context");
                    var context = CreateContext(this);
                    if (context != null)
                    {
                        Log(LogLevel.DEBUG, "RecorderBase->DoLogic");
                        return DoLogic(context);
                    }
                    Log(LogLevel.ERROR, "RecorderBase->CreateContext failed with no context");
                }
                else
                {
                    Log(LogLevel.DEBUG,
                        "RecorderBase->ValidateAccount FAILED. Error:" +
                        (error == null ? "Unknown reason" : error.ToString()));
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.ERROR, "RecorderBase->Exception:" + ex);
            }
            finally
            {
                DestoryWinImpersonationContext(ref winImpCtx, ref error);
            }
            return NextInstruction.Abort;
        }

        public virtual IEnumerable<DataMappingInfo> MappingInfos
        {
            get
            {
                lock (SyncRoot)
                {
                    if (mappingInfos == null)
                        mappingInfos = CreateMappingInfos();
                }
                return mappingInfos;
            }
        }

        public virtual NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            var offset = context.OffsetInStream;
            var headerOff = new[] { context.HeaderOffset[0], context.HeaderOffset[1] };
            try
            {
                if (!context.SetOffset(context.HeaderOffset[0], ref error))
                    return context.FixOffsets(NextInstruction.Abort, offset, headerOff, ref error);
                var lastStart = context.OffsetInStream;

                var canRead = true;
                while (canRead && context.ReadRecord(ref error) > 0)
                {
                    NextInstruction ins;
                    switch (InputTextType(context, ref error))
                    {
                        case RecordInputType.EndOfStream:
                            canRead = false;
                            break;
                        case RecordInputType.Header:
                            ins = Text2Header(context, GetHeaderText(context));
                            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                                return context.FixOffsets(ins, offset, headerOff, ref error);

                            if ((ins & NextInstruction.Do) == NextInstruction.Do)
                            {
                                context.HeaderOffset[0] = lastStart;
                                context.HeaderOffset[1] = context.OffsetInStream;
                                return context.FixOffsets(ins, offset, headerOff, ref error);
                            }
                            break;
                        case RecordInputType.Error:
                            if (ProcessInputTextType(context, OnGetHeaderInfoError, out ins, ref error))
                                break;
                            return context.FixOffsets(ins, offset, headerOff, ref error);
                        case RecordInputType.Unknown:
                            if (ProcessInputTextType(context, OnGetHeaderInfoUnknown, out ins, ref error))
                                break;
                            return context.FixOffsets(ins, offset, headerOff, ref error);
                        case RecordInputType.Comment:
                            if (ProcessInputTextType(context, OnGetHeaderInfoComment, out ins, ref error))
                                break;
                            return context.FixOffsets(ins, offset, headerOff, ref error);
                    }
                    lastStart = context.OffsetInStream;
                }
                return context.FixOffsets(NextInstruction.Skip, offset, headerOff, ref error);
            }
            catch (Exception e)
            {
                error = e;
                return OnGetHeaderInfoError(context, offset, headerOff, error);
            }
        }

        protected virtual NextInstruction OnGetHeaderInfoError(RecorderContext context, long offset, long[] headerOff, Exception error)
        {
            return context.FixOffsets(NextInstruction.Abort, offset, headerOff, ref error);
        }

        protected virtual bool ProcessInputTextType(RecorderContext context, ContextProcessor processor, out NextInstruction ins, ref Exception error)
        {
            ins = processor(context, ref error);
            if ((ins & NextInstruction.Continue) == NextInstruction.Continue)
                return true;
            if (error != null)
                throw error;
            return false;
        }

        protected virtual NextInstruction ProcessContextRecords(RecorderContext context, ref Exception error)
        {
            try
            {
                Log(LogLevel.DEBUG, "Create Reader");
                if (!context.CreateReader(ref error))
                {
                    Log(LogLevel.DEBUG, "Create Reader Abort:" + error);
                    return NextInstruction.Abort;
                }

                Log(LogLevel.DEBUG, "Fix Offset in Context To:" + context.OffsetInStream);
                var ins = context.FixOffsets(NextInstruction.Do, context.OffsetInStream, context.HeaderOffset, ref error);
                if (ins != NextInstruction.Do)
                {
                    Log(LogLevel.DEBUG, "Fix Offset did not allow continue. Return Value:" + ins);
                    if (error != null)
                        Log(LogLevel.ERROR, "Fix Offset Error:" + error);
                    return ins;
                }
                Log(LogLevel.DEBUG, "Getting Header Info");
                ins = GetHeaderInfo(context, ref error);

                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                {
                    Log(LogLevel.DEBUG, "Get Header Info return break:" + ins + (error != null ? error.ToString() : ""));
                }
                else if (ins == NextInstruction.Do)
                {
                    Log(LogLevel.DEBUG, "Prepare Keywords");
                    context.LastKeywordBuffer.Remove(0, context.LastKeywordBuffer.Length);
                    PrepareKeywords(context, context.LastKeywordBuffer);
                    context.LastKeywords = context.LastKeywordBuffer.ToString();
                    string[] fields = null;
                    Log(LogLevel.DEBUG, "Process Stream");
                    ins = ProcessStream(context, ref fields, ref error);
                    if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    {
                        Log(LogLevel.DEBUG, "Process Stream return break");
                    }
                    else if (context.RecordSent >= maxRecordSend)
                    {
                        Log(LogLevel.DEBUG, "Process Stream: Max Record Send Reached");
                        return NextInstruction.Return;
                    }
                }
                return ins;
            }
            catch (Exception e)
            {
                Log(LogLevel.DEBUG, "Process Context Records Error:" + e);
                error = e;
                return OnProcessContextRecordsError(context, error);
            }
            finally
            {
                context.Dispose();
            }
        }

        protected virtual NextInstruction OnProcessContextRecordsError(RecorderContext context, Exception error)
        {
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction ProcessStream(RecorderContext context, ref string[] fields, ref Exception error)
        {
            try
            {
                if (context.OffsetInStream < context.HeaderOffset[1]
                    && (!context.SetOffset(context.HeaderOffset[1], ref error)
                    || context.OffsetInStream != context.HeaderOffset[1]))
                {
                    error = new Exception("Adjusting offset to last header end failed");
                    return OnContextSetOffsetError(context);
                }

                Log(LogLevel.DEBUG, "Setting Registry");
                var ins = SetReg(context);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                {
                    Log(LogLevel.DEBUG, "Set Registry returned don't continue:" + ins);
                    return ins;
                }

                headerOffset[0] = context.HeaderOffset[0];
                headerOffset[1] = context.HeaderOffset[1];
                inputModifiedOn = context.InputModifiedOn;

                Log(LogLevel.DEBUG, "Begin Process");
                while (context.RecordSent < maxRecordSend && context.ReadRecord(ref error) > 0)
                {
                    ins = ExecuteRecordProcessStages(context, ref fields, ref error);
                    if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                        break;
                }

                if (context.RecordSent >= maxRecordSend)
                {
                    Log(LogLevel.DEBUG, "Process Stream: Max Record Send Reached");
                    return NextInstruction.Return;
                }

                if (error == null)
                {
                    Log(LogLevel.DEBUG, "Process Stream: Break Occured with no error");
                    return NextInstruction.Do;
                }
                Log(LogLevel.DEBUG, "Process Stream: Error:" + error);
            }
            catch (Exception e)
            {
                Log(LogLevel.DEBUG, "Process Stream: Error:" + e);
                error = e;
            }
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction ExecuteRecordProcessStages(RecorderContext context, ref string[] fields, ref Exception error)
        {
            try
            {
                var ins = OnBeforeProcessRecordInput(context);
                if (ins != NextInstruction.Do)
                    return ins;
                Log(LogLevel.DEBUG, "Process Input Record:" + context.RecordSent);
                ins = ProcessRecordInput(context, ref fields, ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                {
                    Log(LogLevel.DEBUG, "Process Input Record:" + context.RecordSent + " required break");
                    return ins;
                }
                Log(LogLevel.DEBUG, "Processed Input Record:" + context.RecordSent);
                return (ins & NextInstruction.Skip) == NextInstruction.Skip ? ins : OnAfterProcessRecordInput(context);
            }
            catch (Exception e)
            {
                error = e;
                return NextInstruction.Abort;
            }
        }

        protected virtual NextInstruction OnContextSetOffsetError(RecorderContext context)
        {
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnAfterProcessRecordInput(RecorderContext context)
        {
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnGetHeaderInfoUnknown(RecorderContext context, ref Exception error)
        {
            throw new Exception("Unknown text while seeking for header info");
        }

        protected virtual NextInstruction OnGetHeaderInfoError(RecorderContext context, ref Exception error)
        {
            throw new Exception("Errenous text while seeking for header info");
        }

        protected virtual NextInstruction OnGetHeaderInfoComment(RecorderContext context, ref Exception error)
        {
            return NextInstruction.Skip;
        }

        protected virtual DataMappingInfo GetBestMatch(IEnumerable<DataMappingInfo> dataMappingInfos, Dictionary<string, int> header)
        {
            DataMappingInfo result = null;
            var maxMatch = 0.0D;
            foreach (var info in dataMappingInfos)
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

        protected virtual DataMappingInfo RecordFields2Info(IEnumerable<DataMappingInfo> dataMappingInfos, Dictionary<string, int> header)
        {
            var result = GetBestMatch(dataMappingInfos, header);
            if (result == null)
                return null;
            return BestMatch2Info(result, header);
        }

        protected virtual DataMappingInfo BestMatch2Info(DataMappingInfo result, Dictionary<string, int> header)
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
                var origIndex = -1;
                foreach (var fieldNames in field.Original)
                {
                    fin.Mappings[i].SourceIndex[++origIndex] = -1;
                    foreach (var fieldName in fieldNames)
                    {
                        if (header.ContainsKey(fieldName))
                        {
                            fin.Mappings[i].SourceIndex[origIndex] = header[fieldName];
                            break;
                        }
                    }
                }
                ++i;
            }
            return fin;
        }

        protected virtual NextInstruction OnSetDataException(RecorderContext context, Exception error)
        {
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction OnSetRegException(RecorderContext context, Exception error)
        {
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction SetData(RecorderContext context)
        {
            try
            {
                var ins = OnBeforeSetData(context);
                if ((ins & NextInstruction.Do) != NextInstruction.Do)
                    return ins;

                context.Service.SetData(dal, virtualhost, context.Record.rec);
                //context.Record.Recordnum++;
                /* try
                 {
                     context.Record.Recordnum = (int)context.Record.CustomInt6;
                 }
                 catch (Exception ex)
                 {
                     L.Log(LogType.FILE, LogLevel.ERROR, "RecordNum Casting Error. ");
                     context.Record.Recordnum = 0;
                 }
                 */
                return OnAfterSetData(context);
            }
            catch (Exception e)
            {
                Log(LogLevel.WARN, "Error while sending record data :" + e.Message);
                return OnSetDataException(context, e);
            }
        }

        protected virtual NextInstruction SetReg(RecorderContext context)
        {
            try
            {
                Log(LogLevel.DEBUG, "OnBefore SetReg");
                var ins = OnBeforeSetReg(context);
                Log(LogLevel.DEBUG, "OnBefore SetReg Status:" + ins);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                {
                    Log(LogLevel.DEBUG, "OnBefore SetReg Required Break:" + ins);
                    return ins;
                }

                context.Service.SetReg(GetIdentity(context), GetContextPosition(context), GetContextLastLine(context),
                    GetContextLastFile(context), GetContextLastKeywords(context), GetContextLastRecordDate(context));

                Log(LogLevel.DEBUG, "Context Set registry Completed..Call After SetReg");
                ins = OnAfterSetReg(context);
                Log(LogLevel.DEBUG, "OnAfter SetReg and Return Status:" + ins);
                return ins;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while setting registry data :" + e);
                return OnSetRegException(context, e);
            }
        }

        protected virtual string GetContextLastRecordDate(RecorderContext context)
        {
            return context.LastRecordDate ?? context.LastRecordDate;
        }

        protected virtual string GetContextLastKeywords(RecorderContext context)
        {
            return context.LastKeywords ?? context.LastKeywords;
        }

        protected virtual string GetContextLastFile(RecorderContext context)
        {
            return context.LastFile ?? string.Empty;
        }

        protected virtual string GetContextLastLine(RecorderContext context)
        {
            return context.LastLine ?? string.Empty;
        }

        protected virtual string GetContextPosition(RecorderContext context)
        {
            return context.OffsetInStream.ToString(CultureInfo.InvariantCulture);
        }

        protected virtual int GetIdentity(RecorderContext context)
        {
            return identity;
        }

        protected virtual NextInstruction OnAfterSetReg(RecorderContext context)
        {
            lastPosition = GetContextPosition(context);
            lastLine = GetContextLastLine(context);
            lastFile = GetContextLastFile(context);
            lastKeywords = GetContextLastKeywords(context);
            lastRecordDate = GetContextLastRecordDate(context);
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnBeforeSetReg(RecorderContext context)
        {
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.LogName = GetRecorderName();
            if (String.IsNullOrEmpty(context.Record.Datetime))
                context.Record.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (context.Record != null)
                context.LastRecordDate = context.Record.Datetime;
            return ApplyConstraints(context.Record);
        }

        protected virtual NextInstruction ApplyConstraints(RecWrapper recWrapper)
        {
            if (ConstrainedDataLookup == null)
                return NextInstruction.Do;
            foreach (var constraintCollection in ConstrainedDataLookup)
            {
                var consApplied = constraintCollection.Value.Apply(recWrapper, null);
                if ((consApplied & NextInstruction.Continue) != NextInstruction.Continue)
                    return consApplied;
            }
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnAfterSetData(RecorderContext context)
        {
            lastFile = GetContextLastFile(context);
            lastKeywords = GetContextLastKeywords(context);
            lastLine = GetContextLastLine(context);
            lastPosition = GetContextPosition(context);
            return NextInstruction.Do;
        }

        protected virtual NextInstruction InitializeLogger()
        {
            try
            {
                if (logger != null)
                    return NextInstruction.Do;
                if (String.IsNullOrEmpty(logFile))
                {
                    logFile = GetLogFileFromRegistry();
                    if (String.IsNullOrEmpty(logFile))
                        return NextInstruction.Abort;
                }

                logger = new CLogger();
                logger.SetLogLevel((LogLevel)traceLevel);
                logger.SetLogFile(logFile);
                logger.SetTimerInterval(logType, loggingInterval <= 0 ? 60000 : loggingInterval);
                logger.SetLogFileSize(logSize <= 0 ? 10 * 1024 * 1024 : logSize);
                return NextInstruction.Do;
            }
            catch (Exception er)
            {
                Log(LogLevel.ERROR, er.ToString());
                return NextInstruction.Abort;
            }
        }

        public virtual void Log(LogLevel logLevel, string message)
        {
            LogHelper.Log(logger, logType, logLevel, GetRecorderName(), message, defaultLogType);
        }

        protected virtual string GetLogFileFromRegistry()
        {
            try
            {
                using (var regRecorder = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder"))
                {
                    if (regRecorder != null)
                    {
                        var logFileName = Path.Combine(Path.Combine(regRecorder.GetValue("Home Directory").ToString(), "log"),
                            GetRecorderName() + identity + ".log");
                        var fInfo = new FileInfo(logFileName);

                        if (fInfo.Directory != null && !fInfo.Directory.Exists)
                            fInfo.Directory.Create();
                        return logFileName;
                    }
                }

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager DHCP Recorder Read Registry", er.ToString(),
                                    EventLogEntryType.Error);

            }
            return null;

        }

        public override void SetConfigData(int cfgIdentity, string cfgLocation,
            string cfgLastLine, string cfgLastPosition, string cfgLastFile,
            string cfgLastKeywords, bool cfgFromEndOnLoss, int cfgMaxLineToWait,
            string cfgUser, string cfgPassword, string cfgRemoteHost, int cfgSleepTime,
            int cfgTraceLevel, string cfgCustomVar1, int cfgCustomVar2, string cfgVirtualhost,
            string cfgDal, int cfgZone)
        {
            identity = cfgIdentity;
            location = cfgLocation;
            lastLine = cfgLastLine;
            lastPosition = cfgLastPosition;
            lastFile = cfgLastFile;
            lastKeywords = cfgLastKeywords;
            fromEndOnLoss = cfgFromEndOnLoss;
            maxRecordSend = cfgMaxLineToWait;
            user = cfgUser;
            AccountValidator.SplitUserDomain(ref user, ref domain);
            password = cfgPassword;
            remoteHost = cfgRemoteHost;
            virtualhost = cfgVirtualhost;
            dal = cfgDal;
            zone = cfgZone;
            sleepTime = cfgSleepTime;
            traceLevel = cfgTraceLevel;
            customVar1 = cfgCustomVar1;
            customVar2 = cfgCustomVar2;
            ParseParams(cfgCustomVar1, OnArgParsed, OnUnhandledKeyword);
            ParseParams(lastKeywords, OnKeywordParsed, OnUnhandledKeyword);
        }

        protected virtual NextInstruction InitFieldBuffer(RecorderContext context, string source)
        {
            context.FieldBuffer.Clear(true);
            context.LastFieldMatch = null;
            return NextInstruction.Do;
        }

        protected virtual NextInstruction InitHeaderBuffer(RecorderContext context, string source)
        {
            context.HeaderBuffer.Clear();
            context.LastHeaderMatch = null;
            return NextInstruction.Do;
        }

        protected abstract CanAddMatchDelegate CanAddMatchField { get; }

        protected abstract CanAddMatchDelegate CanAddMatchHeader { get; }

        protected virtual bool CanAddMatchRegValue(RecorderContext context, string source, Match match, Match lastMatch, out string matchValue)
        {
            if (match.Success)
            {
                //Since regex has been assumed having GROUPS[1] contains all match alternatives
                //index starts from 2. If no match occurs, then GROUP[1] is only matching part
                var i = 2;
                while (i < match.Groups.Count && !match.Groups[i].Success)
                {
                    ++i;
                }
                matchValue = match.Groups[i < match.Groups.Count ? i : 1].Value;
                return true;
            }
            matchValue = null;
            return false;
        }

        protected virtual bool CanAddMatchRegSplitter(RecorderContext context, string source, Match match, Match lastMatch, out string matchValue)
        {
            var lastIndex = lastMatch == null ? 0 : lastMatch.Index + lastMatch.Length;
            var len = (match.Success ? match.Index : source.Length) - lastIndex;
            matchValue = len > 0 ? source.Substring(lastIndex, len) : string.Empty;
            return true;
        }

        protected virtual NextInstruction OnHeaderMatch(RecorderContext context, string source, ref Match match)
        {
            string matchValue;
            if (CanAddMatchHeader(context, source, match, context.LastHeaderMatch, out matchValue))
                context.HeaderBuffer.Add(matchValue);
            return match.Success ? NextInstruction.Do : NextInstruction.Return;
        }

        protected virtual NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            string matchValue;
            if (CanAddMatchField(context, source, match, context.LastFieldMatch, out matchValue))
                context.FieldBuffer.Add(matchValue);
            return match.Success ? NextInstruction.Do : NextInstruction.Return;
        }

        protected virtual string[] FieldBufferToArray(RecorderContext context, string source)
        {
            return context.FieldBuffer.ToArray();
        }

        protected virtual string[] HeaderBufferToArray(RecorderContext context, string source)
        {
            return context.HeaderBuffer.ToArray();
        }

        protected virtual void SetLastMatchField(RecorderContext context, string source, Match match)
        {
            context.LastFieldMatch = match;
        }

        protected virtual void SetLastMatchHeader(RecorderContext context, string source, Match match)
        {
            context.LastHeaderMatch = match;
        }

        protected virtual string[] RegexSplitterCommon(RecorderContext context, string source,
                                                       InitBufferDelegate initBuffer, OnMatchDelegate onMatch,
                                                       BufferToArrayDelegate bufferToArray,
                                                       SetLastMatchDelegate setLastMatch)
        {
            if (context == null || string.IsNullOrEmpty(source))
                return null;

            switch (initBuffer(context, source))
            {
                case NextInstruction.Return:
                    return bufferToArray(context, source);
                case NextInstruction.Abort:
                    return null;
            }

            var m = FieldSeparators.Match(source);
            do
            {
                while (m.Success && m.Index < source.Length)
                {
                    switch (onMatch(context, source, ref m))
                    {
                        case NextInstruction.Return:
                            return bufferToArray(context, source);
                        case NextInstruction.Abort:
                            return null;
                    }
                    setLastMatch(context, source, m);
                    m = m.NextMatch();
                }
                if (m.Success)
                    return bufferToArray(context, source);
                switch (onMatch(context, source, ref m))
                {
                    case NextInstruction.Return:
                        return bufferToArray(context, source);
                    case NextInstruction.Skip:
                    case NextInstruction.Abort:
                        return null;
                }
            } while (true);
        }

        public virtual string[] FieldSplitter(RecorderContext context, string source)
        {
            Log(LogLevel.DEBUG, "Record source ==> " + source);
            return RegexSplitterCommon(context, source, InitFieldBuffer, OnFieldMatch, FieldBufferToArray, SetLastMatchField);
        }

        public virtual string[] HeaderSplitter(RecorderContext context, string source)
        {
            return RegexSplitterCommon(context, source, InitHeaderBuffer, OnHeaderMatch, HeaderBufferToArray, SetLastMatchHeader);
        }

        protected virtual NextInstruction InputText2RecordField(RecorderContext context, ref string[] fields)
        {
            if (context.InputRecord != null)
            {
                fields = FieldSplitter(context, context.InputRecord.ToString());
                if (fields != null)
                    return NextInstruction.Do;
            }
            return NextInstruction.Skip;
        }

        protected virtual NextInstruction Text2Header(RecorderContext ctxFile, string headerText)
        {
            try
            {
                if (ctxFile.SourceHeaderInfo == null)
                    ctxFile.SourceHeaderInfo = new Dictionary<string, int>();
                else
                    ctxFile.SourceHeaderInfo.Clear();
                var lineParts = HeaderSplitter(ctxFile, headerText);
                if (lineParts != null)
                {
                    var index = 0;
                    foreach (var linePart in lineParts)
                    {
                        if (string.IsNullOrEmpty(linePart)) continue;
                        ctxFile.SourceHeaderInfo[linePart.Trim()] = index;
                        index++;
                    }
                }
                ctxFile.HeaderInfo = RecordFields2Info(MappingInfos, ctxFile.SourceHeaderInfo);
                return ctxFile.HeaderInfo == null ? NextInstruction.Skip : NextInstruction.Do;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "An error occured while trying to convert text to header [" + ctxFile.InputRecord + "] :" + e);
                return NextInstruction.Abort;
            }
        }

        protected virtual bool OnUnhandledKeyword(string keyword, bool quotedKeyword, 
            string value, bool quotedValue, bool keywordValueError, ref int touchCount, ref Exception error)
        {
            if (keywordValueError)
            {
                if (touchCount > 0)
                {
                    Log(LogLevel.ERROR,string.Format("Missused keyword or an error accure when parsing keyword [{0}]", keyword));
                }
                else
                {
                    Log(LogLevel.ERROR, string.Format("Unknown keyword [{0}]", keyword));
                }
            }
            else if (touchCount == 0)
            {
                {
                    Log(LogLevel.ERROR, string.Format("Unhandled keyword [{0}]", keyword));
                }
            }
            return true;
        }

        protected virtual bool OnArgParsed(string keyword, bool quotedKeyword,
            string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (keyword == "E")
            {
                encoding = GetEncoding(value);
                touchCount++;
            }
            return true;
        }

        protected virtual bool OnKeywordParsed(string keyword, bool quotedKeyword,
            string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            switch (keyword)
            {
                case "HOff":
                    var parts = value.Split('|');
                    if (!ArrayHelper.AssureLength(parts, 2))
                    {
                        touchCount++;
                        break;
                    }
                    if (long.TryParse(parts[0], out headerOffset[0]))
                    {
                        long.TryParse(parts[1], out headerOffset[1]);
                        touchCount++;
                    }
                    break;
                case "FMdf":
                    long.TryParse(value, out inputModifiedOn);
                    touchCount++;
                    break;
            }
            return true;
        }

        protected virtual Encoding GetEncoding(string strEncoding)
        {
            if (!String.IsNullOrEmpty(strEncoding))
            {
                try
                {
                    return Encoding.GetEncoding(strEncoding);
                }
                catch
                {
                    Log(LogLevel.ERROR, "Undefined strEncoding :" + strEncoding);
                }
            }
            return null;
        }

        protected virtual void ParseParams(string argStr, ConfigHelper.OnKeywordValue onArgParsed, ConfigHelper.OnUnhandledKeywordValue onUnhandledKeyword)
        {
            if (String.IsNullOrEmpty(argStr))
                return;

            Exception error = null;
            ConfigHelper.ParseKeywords(argStr, onArgParsed, null, null, onUnhandledKeyword, ref error);
        }

        protected virtual void PrepareEncoding(ref Encoding encodingInUse)
        {
            if (encodingInUse == null)
            {
                try
                {
                    encodingInUse = Encoding.GetEncoding(1254);
                }
                catch (Exception e)
                {
                    encodingInUse = Encoding.UTF8;
                    Log(LogLevel.ERROR, "Getting default _encoding Windows-1254 failed:" + e.Message + ". Switch to UTF8");
                }
            }
        }

        protected virtual string PrepareField(string[] originals)
        {
            if (originals == null || originals.Length == 0)
                return null;

            var f = originals[0];
            for (var i = 1; i < originals.Length; i++)
                f += "," + originals[i];
            return f;
        }

        protected virtual bool ValidateAccount(ref ImpersonationContext winImpCtx, ref Exception error)
        {
            try
            {
                lock (SyncRoot)
                {
                    Log(LogLevel.DEBUG, "Check need impersonation");
                    if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(user))
                    {
                        Log(LogLevel.DEBUG, "Empty username, no need to impersonate");
                        return true;
                    }

                    winImpCtx = AccountValidator.ValidateAccount(domain, user, password, ref error);
                    if (winImpCtx != null)
                    {
                        Log(LogLevel.DEBUG, "Impersonation successful");
                        return true;
                    }
                    throw error ?? new Exception("Impersonation failed with unknown reason: Location(" + location +
                                        "), Domain(" + domain + "), User(" + user + ")");
                }
            }
            catch (Exception e)
            {
                error = e;
                Log(LogLevel.DEBUG, "Error while checking remote location access:" + e);
            }
            return false;
        }

        protected virtual bool OnBeforeDestroyWinImpersonationContext(ref ImpersonationContext winImpCtx, ref Exception error)
        {
            return true;
        }

        protected virtual void OnAfterDestroyWinImpersonationContext()
        {
        }

        protected virtual void DestoryWinImpersonationContext(ref ImpersonationContext winImpCtx, ref Exception error)
        {
            if (winImpCtx != null)
            {
                if (OnBeforeDestroyWinImpersonationContext(ref winImpCtx, ref error))
                {
                    try
                    {
                        winImpCtx.Dispose();
                    }
                    catch
                    {
                    }
                    winImpCtx = null;
                    OnAfterDestroyWinImpersonationContext();
                }
            }
        }

        protected virtual NextInstruction ProcessInputText(RecorderContext context, ref string[] fields, ref Exception error)
        {
            NextInstruction ins;

            try
            {
                ClearRecord(context.Record);
                switch (InputTextType(context, ref error))
                {
                    case RecordInputType.EndOfStream:
                        return NextInstruction.Return;
                    case RecordInputType.Header:
                        ins = OnProcessInputTextHeader(context, fields, ref error);
                        if ((ins & NextInstruction.Do) != NextInstruction.Do)
                            return ins;
                        ins = Text2Header(context, GetHeaderText(context));
                        if ((ins & NextInstruction.Do) != NextInstruction.Do)
                            return ins;
                        context.HeaderOffset[0] = context.OffsetInStream - context.RecordSizeInBytes;
                        context.HeaderOffset[1] = context.OffsetInStream;
                        context.LastKeywordBuffer.Remove(0, context.LastKeywordBuffer.Length);
                        PrepareKeywords(context, context.LastKeywordBuffer);
                        context.LastKeywords = context.LastKeywordBuffer.ToString();

                        ins = SetReg(context);
                        if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                            return ins;
                        return NextInstruction.Skip;
                    case RecordInputType.Record:
                        ins = OnProcessInputTextRecord(context, fields, ref error);
                        if ((ins & NextInstruction.Do) != NextInstruction.Do)
                            return ins;
                        ins = InputText2RecordField(context, ref fields);
                        if ((ins & NextInstruction.Do) != NextInstruction.Do)
                            return ins;
                        return ApplyContextMapping(context, fields, ref error);
                    case RecordInputType.Comment:
                        return OnProcessInputTextComment(context, fields, ref error);
                    case RecordInputType.Error:
                        return OnProcessInputTextError(context, fields, ref error);
                    case RecordInputType.Unknown:
                        return OnProcessInputTextUnknown(context, fields, ref error);
                }
                return NextInstruction.Do;
            }
            catch (Exception e)
            {
                ins = OnProcessInputTextException(context, fields, e, ref error);
            }
            return ins;
        }

        protected virtual NextInstruction ApplyContextMapping(RecorderContext context, string[] fields, ref Exception error)
        {
            try
            {
                if (context.HeaderInfo == null)
                {
                    Log(LogLevel.DEBUG, "Skipping due to no header info");
                    return NextInstruction.Skip;
                }
                foreach (var info in context.HeaderInfo.Mappings)
                {
                    if (info.MappedField == null)
                        continue;
                    var i = 0;
                    if (info.SourceIndex != null && info.SourceValues != null)
                    {
                        foreach (var index in info.SourceIndex)
                        {
                            if (info.SourceValues.Length > i)
                            {
                                info.SourceValues[i++] = index != -1
                                                             ? (fields != null && fields.Length > index
                                                                    ? fields[index]
                                                                    : String.Empty)
                                                             : null;
                            }
                        }
                    }
                    var value = info.MethodInfo != null
                                    ? info.MethodInfo(context.Record,
                                                      info.Original[0][0],
                                                      info.SourceValues, info.FormatterData)
                                    : info.SourceValues != null && info.SourceValues.Length > 0
                                          ? info.SourceValues[0]
                                          : null;
                    info.MappedField.SetValue(context.Record, value, null);
                }
                return NextInstruction.Do;
            }
            catch (Exception e)
            {
                error = e;
                Log(LogLevel.ERROR, "Error while applying context mapping:" + e);
                return OnApplyContextMappingError(context, fields, e);
            }
        }

        protected virtual NextInstruction OnApplyContextMappingError(RecorderContext context, string[] fields, Exception error)
        {
            return NextInstruction.Skip;
        }

        protected virtual NextInstruction OnProcessInputTextHeader(RecorderContext context, string[] fields, ref Exception error)
        {
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnProcessInputTextRecord(RecorderContext context, string[] fields, ref Exception error)
        {
            return NextInstruction.Do;
        }

        protected virtual NextInstruction OnProcessInputTextComment(RecorderContext context, string[] fields, ref Exception error)
        {
            return NextInstruction.Skip;
        }

        protected virtual NextInstruction OnProcessInputTextError(RecorderContext context, string[] fields, ref Exception error)
        {
            return NextInstruction.Skip;
        }

        protected virtual NextInstruction OnProcessInputTextException(RecorderContext context, string[] fields, Exception e, ref Exception error)
        {
            error = e;
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction OnProcessRecordException(RecorderContext context, string[] fields, Exception e, ref Exception error)
        {
            error = e;
            return NextInstruction.Abort;
        }

        protected virtual NextInstruction OnProcessInputTextUnknown(RecorderContext context, string[] fields, ref Exception error)
        {
            return NextInstruction.Skip;
        }

        protected virtual NextInstruction ProcessRecordInput(RecorderContext context, ref string[] fields, ref Exception error)
        {
            try
            {
                var ins = ProcessInputText(context, ref fields, ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    return ins;
                if ((ins & NextInstruction.Do) == NextInstruction.Do)
                {
                    context.LastRecordDate = context.Record.Datetime;
                    context.Record.LogName = GetRecorderName();

                    ins = SetData(context);
                    if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                        return ins;
                    context.RecordSent++;
                }
                return SetReg(context);
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing record input:" + e);
                return OnProcessRecordException(context, fields, e, ref error);
            }
        }

        protected virtual bool Initialize()
        {
            lock (SyncRoot)
            {
                while ((status & RecorderStatus.Initializing) != RecorderStatus.None)
                    Monitor.Wait(SyncRoot);

                if ((status & RecorderStatus.Initialized) != RecorderStatus.None)
                    return true;

                status |= RecorderStatus.Initializing;
                Monitor.PulseAll(SyncRoot);
            }
            try
            {
                if ((InitializeInstance() & NextInstruction.Do) == NextInstruction.Do
                    && (ValidateGlobalParameters() & NextInstruction.Do) == NextInstruction.Do)
                {
                    PrepareEncoding(ref encoding);

                    lock (SyncRoot)
                    {
                        status = RecorderStatus.Initialized;
                        Monitor.PulseAll(SyncRoot);
                        return true;
                    }
                }

            }
            finally
            {
                lock (SyncRoot)
                {
                    if ((status & RecorderStatus.Initializing) == RecorderStatus.Initializing)
                    {
                        status ^= (status & RecorderStatus.Initializing);
                        Monitor.PulseAll(SyncRoot);
                    }
                }
            }
            return false;
        }
        #endregion
    }

}
