using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.PhpFtpUnifiedRecorder
{
    public class PhpFtpUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"(^(?<DATE_TIME>\S+\s+\S+)\s+(?<EVENTCATEGORY>\S+)(?:\s+(?<EVENT_TYPE>\S+)(\s+\-\>)?)?\s+\[(?<CUSTOM_STR_1>[^]]+)\]\W+((((?<CUSTOM_STR_3_0>[^:.]+)[:.]\s+)\[((?<CUSTOM_STR_4>.*?)\])?.*?\[(?<CUSTOM_STR_5_0>.*$))|((?<CUSTOM_STR_3_1>[^\/]+)(?<CUSTOM_STR_5_1>.*$))))", RegexOptions.Compiled);
        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new PhpFtpUnifiedRecorderContext(this);
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

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
                        MethodInfo = Convert2Date
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"EVENTCATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_3_0"},new[] {"CUSTOM_STR_3_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_5_0"},new[] {"CUSTOM_STR_5_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    }
                }
            };
        }
        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.CustomStr10 = context.Recorder.RemoteHost;
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
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
        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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
        protected override Dictionary<string, int> MimicMappingInfo(IEnumerable<DataMapping> dataMapping)
        {
            var info = new Dictionary<string, int>();
            var index = 0;
            foreach (var mapping in dataMapping)
            {
                if (mapping.Original.Length > 1)
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index;
                    }
                    index++;
                }
                else
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index++;
                    }
                }
            }
            return info;
        }
    }
}
