     using System;
using System.Collections.Generic;
     using System.Globalization;
     using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.PhpFpmUnifiedRecorder
{
    public class PhpFpmUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(^\\[(?<DATE_TIME>[^]]+)\\]\\s+(?<EVENT>[^:]+)\\:\\s+\\[(?<POOL>[^]]+)\\]\\s+(?<CHILD_PROCESS>[^:]+\\:\\s+)?(\\\"\\s*)?(?<MESSAGE>.*?)(\\\")?$)", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo =Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"POOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CHILD_PROCESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }
        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new PhpFpmUnifiedRecorderContext(this);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            return NextInstruction.Return;
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
        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        protected override RecorderBase.CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override RecorderBase.CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }


    }
}