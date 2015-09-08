using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Microsoft.Share
{
    public class WindowsShareLogUnifiedRecorder : FileLineRecorder
    {
        public static readonly Regex RegSplitForSeparator = new Regex("\\|\\|", RegexOptions.Compiled);

        public static object ExtractUsername(RecWrapper rec, string field, string[] values, object data)
        {
            if (values != null && values.Length >= 1)
            {
                var index = values[0].IndexOf("\\", StringComparison.Ordinal);
                if (index >= 0)
                    rec.CustomStr1 = values[0].Substring(index);
                return values[0];
            }
            return string.Empty;
        }

        #region Mapping
        public static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new []{"time"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"eventType"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"eventCategory"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"sourceName"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"file-dir"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"userSid"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),
                        MethodInfo=ExtractUsername
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"physical"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"file-dir-2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"guid"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    }
                }
            };
        }
        #endregion

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null)
                return RecordInputType.Comment;

            var txt = context.InputRecord.ToString();
            if (txt.Length == 0)
                return RecordInputType.Comment;
            return RecordInputType.Record;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
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

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new WindowsShareLogUnifiedRecorderContext();
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegSplitter; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForSeparator;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForSeparator;
        }

        protected static Regex FileTimestamp = new Regex("^([0-9]+-[0-9]+-[0-9]+)", RegexOptions.Compiled);

        protected override bool GetLastProcessedFile(FileRecorderContext context, bool next)
        {
            if (base.GetLastProcessedFile(context, next))
            {
                var ctx = context as WindowsShareLogUnifiedRecorderContext;
                if (ctx != null)
                {
                    if (!string.IsNullOrEmpty(ctx.LastFile))
                    {
                        var m = FileTimestamp.Match(ctx.LastFile);
                        if (m.Success)
                        {
                            ctx.DateFromFile = m.Groups[1].Value;
                            return true;
                        }
                    }
                    ctx.DateFromFile = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return true;
                }
            }
            return false;
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            var ctx = context as WindowsShareLogUnifiedRecorderContext;
            if (ctx == null) return NextInstruction.Do;
            DateTime dt;
            if (DateTime.TryParseExact(ctx.DateFromFile + " " + ctx.Record.Datetime, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                ctx.Record.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var txt = context.InputRecord == null ? string.Empty : context.InputRecord.ToString();
            context.Record.Description = txt.Length > 900 ? txt.Substring(0, 900) : txt;
            return NextInstruction.Do;
        }
    }
}
