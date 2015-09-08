using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Log;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.TrendMicroUnifiedRecorder
{
    public class TrendMicroUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"([\w\S]*)\s*:\t(.*)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy/MM/dd H:mm:ss";
        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"Date"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Method"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"User"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Server"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Content-Type"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ClientIP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ServerIP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Domain"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Path"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Category"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32                       
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CategoryType"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32  
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Content-Length"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32  
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Operation"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    }

                }
            };
        }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);
            switch (keyword)
            {
                case "DF":
                    if (!string.IsNullOrEmpty(value))
                        DateFormat = value;
                    touchCount++;
                    break;
            }
            return true;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new TrendMicroUnifiedRecorderContext(this);
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;

            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            var ctxFile = context as TrendMicroUnifiedRecorderContext;
            if (ctxFile == null) return NextInstruction.Abort;

            match = RegSplitForValue.Match(source);
            try
            {
                if (match.Success)
                {
                    if (!ctxFile.Buffer.ContainsKey(match.Groups[1].Value))
                        ctxFile.Buffer.Add(match.Groups[1].Value, match.Groups[2].Value);
                    return NextInstruction.Skip;
                }

                foreach (var key in ctxFile.Buffer.Keys)
                {
                    ctxFile.FieldBuffer[ctxFile.SourceHeaderInfo[key]] = ctxFile.Buffer[key];
                }

                ctxFile.Buffer.Clear();
                return NextInstruction.Return;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing Trend Micro unified record: " + e);
                return NextInstruction.Abort;
            }
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            if (MappingInfos == null) return NextInstruction.Do;
            foreach (var mappingInfo in MappingInfos)
            {
                context.SourceHeaderInfo = MimicMappingInfo(mappingInfo.Mappings);
                context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                break;
            }
            return NextInstruction.Do;
        }
    }
}
