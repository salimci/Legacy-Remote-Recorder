using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.LinuxJobsUnifiedRecorder
{
    public class LinuxJobsUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"^(?<DATE_TIME>\S+\s+\S+)[^:]+:\s+(?<NAME>\S+)\s+\-\s+(?<CUSTOM_STR_2>[^]]+.*\]?)\](((?=\.).*$)|((?=\s)[^\.]+\..*?is\s+\[(?<CUSTOM_STR_5>.*?(\]|$))))(.*$)?", RegexOptions.Compiled);
        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxJobsUnifiedRecorderContext(this);
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
                        Original = new[] {new[] {"NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                   
                }
            };
        }
        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.CustomStr3 = context.Recorder.RemoteHost;
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
    }
}
