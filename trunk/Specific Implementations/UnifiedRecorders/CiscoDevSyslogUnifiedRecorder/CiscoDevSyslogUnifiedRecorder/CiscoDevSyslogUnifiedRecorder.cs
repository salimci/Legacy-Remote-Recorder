using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.CiscoDevSyslogUnifiedRecorder
{
    public class CiscoDevSyslogUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll =
            new Regex(
                @"([0-9]*-[0-9]*-[0-9]*\s[0-9]*:[0-9]*:[0-9]*)\s*\w*.([\w]*)\s*([0-9]*.[0-9]*.[0-9]*.[0-9]*)\s*([0-9]*):.*%([\w]*)-([0-9]*)-[\w]*:\s*(.*),\s*changed\s*state\s*to\s*([\w]*\s*[\w]*)",
                RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo == null)
            {
                Exception error = null;
                var ins = GetHeaderInfo(context, ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    return ins;
            }
            return base.OnBeforeProcessRecordInput(context);
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"date-time"}},
                        MappedField = typeof (RecWrapper).GetProperty("DateTime")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Ip"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"eventtype"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"customStr1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"customStr2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"customInt1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"customInt2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as CiscoDevSyslogUnifiedRecorder;

            if (DateTime.TryParseExact(values[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone)
                    .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {

            if (match.Success)
            {
                context.FieldBuffer[0] = match.Groups[1].Value;
                context.FieldBuffer[2] = match.Groups[2].Value;
                context.FieldBuffer[1] = match.Groups[3].Value;
                context.FieldBuffer[5] = match.Groups[4].Value;
                context.FieldBuffer[3] = match.Groups[5].Value;
                context.FieldBuffer[7] = match.Groups[6].Value;
                context.FieldBuffer[8] = match.Groups[7].Value;
                context.FieldBuffer[4] = match.Groups[8].Value;
                return NextInstruction.Return;
            }
            context.FieldBuffer[8] = source;
            context.FieldBuffer[1] = context.Recorder.RemoteHost;
            return NextInstruction.Return;
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            var ins = base.GetHeaderInfo(context, ref error);

            if ((ins & NextInstruction.Do) == NextInstruction.Do)
            {
                context.FieldMappingIndexLookup = CreateFieldMappingIndexLookup(context.HeaderInfo, context, GetFieldMappingsFields());
            }

            return ins;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
        }

    }
}