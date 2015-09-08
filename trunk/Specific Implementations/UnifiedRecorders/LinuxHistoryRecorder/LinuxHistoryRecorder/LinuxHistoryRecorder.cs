using System;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using System.Globalization;

namespace Natek.Recorders.Remote.Linux.Ssh
{
    public class LinuxHistoryRecorder : PeriodicRecorder
    {
        protected static object Convert2Date(RecWrapper record, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as LinuxHistoryRecorder;
            if (DateTime.TryParseExact(values[0] + ' ' + values[1], "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new[] {new[] {"HistoryId"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                                new DataMapping
                                {
                                    Original = new[] {new[] {"Date"}, new[] {"Time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                                new DataMapping
                                {
                                    Original = new[] {new[] {"Command"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                }
                        }
            };
        }

        public long LastId { get; set; }
        public long LastDateUtc { get; set; }
        public int Port { get; set; }
        public int ReadTimeout { get; set; }
        public Regex Pattern { get; set; }

        protected override NextInstruction DoLogic(RecorderContext context)
        {
            throw new NotImplementedException();
        }

        protected override void PrepareKeywords(RecorderContext context, StringBuilder buffer)
        {
            if (buffer.Length > 0)
                buffer.Append(';');
            buffer.Append("LId=")
                  .Append(LastId)
                  .Append(";LDUtc=")
                  .Append(LastDateUtc);
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if (ins != NextInstruction.Do)
                return ins;
            context.Record.UserName = User;
            return NextInstruction.Do;
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var ctx = context as LinuxHistoryContext;
            if (ctx == null)
            {
                error = new Exception("Context is not LinuxHistoryContext or null");
                return RecordInputType.Error;
            }
            var line = context.InputRecord.ToString();
            if (line == ctx.Keyword)
            {
                if (ctx.WaitBegin)
                {
                    ctx.Keyword = "END" + ctx.Keyword.Substring(5);
                    ctx.WaitBegin = false;
                    return RecordInputType.Comment;
                }
                return RecordInputType.EndOfStream;
            }
            return ctx.WaitBegin ? RecordInputType.Comment : RecordInputType.Record;
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

        public override Regex CreateFieldSeparator()
        {
            throw new NotImplementedException();
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { throw new NotImplementedException(); }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { throw new NotImplementedException(); }
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[]
                {
                    CreateMappingEn()
                };
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        public override string GetInputName(RecorderContext context)
        {
            return "LinuxHistory";
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxHistoryContext(this);
        }

        protected override void InitContextInstance(RecorderContext context, params object[] ctxArgs)
        {
            base.InitContextInstance(context, ctxArgs);
            var ctxLinux = context as LinuxHistoryContext;
            if (ctxLinux != null)
            {
                if (ctxLinux.Port <= 0)
                    ctxLinux.Port = 22;
                if (ctxLinux.ReadTimeout <= 0)
                    ReadTimeout = 10000;
                ctxLinux.Fields = new string[4];
            }
            else
                throw new Exception("Context is not LinuxHistoryContext or null");
        }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            int iVal;
            switch (keyword)
            {
                case "Port":
                    touchCount++;
                    if (int.TryParse(value, out iVal))
                    {
                        Port = iVal;
                    }
                    break;
                case "ReadTimeout":
                    touchCount++;
                    if (int.TryParse(value, out iVal))
                    {
                        ReadTimeout = iVal;
                    }
                    break;
                case "Pattern":
                    touchCount++;
                    try
                    {
                        Pattern = new Regex(value);
                    }
                    catch (Exception e)
                    {
                        error = e;
                        return false;
                    }
                    break;
            }
            return true;
        }

        protected override NextInstruction Text2Header(RecorderContext ctxFile, string headerText)
        {
            return NextInstruction.Do;
        }

        private static readonly Regex RegHistory =
            new Regex("^[ \t]*([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+(.*)[ \t]*$", RegexOptions.Compiled);

        protected override NextInstruction InputText2RecordField(RecorderContext context, ref string[] fields)
        {
            var m = RegHistory.Match(context.InputRecord.ToString());
            if (m.Success)
            {
                var ctx = context as LinuxHistoryContext;
                if (ctx != null)
                {
                    long lastId = 0, lastDateUtc = 0;
                    long.TryParse(m.Groups[1].Value, out lastId);
                    long.TryParse(m.Groups[2].Value, out lastDateUtc);
                    if (lastId > LastId)
                    {
                        LastId = lastId;
                        LastDateUtc = lastDateUtc;
                        ctx.Fields[0] = m.Groups[1].Value;
                        ctx.Fields[1] = m.Groups[3].Value;
                        ctx.Fields[2] = m.Groups[4].Value;
                        ctx.Fields[3] = m.Groups[5].Value;
                        fields = ctx.Fields;
                        return NextInstruction.Do;
                    }
                }
                else
                {
                    throw new Exception("Context is not LinuxHistoryContext or null");
                }
            }
            return NextInstruction.Skip;
        }

        protected override NextInstruction ValidateGlobalParameters()
        {
            return NextInstruction.Do;
        }

        public override Regex CreateHeaderSeparator()
        {
            throw new NotImplementedException();
        }
    }
}
