using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.UbuntuAuthUnifiedRecorder
{
    public class UbuntuAuthUnifiedRecorder: TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(?<DATE>[a-zA-Z]*\\s[0-9]*\\s[0-9\\:]+)\\s+(?<HOST_NAME>[^\\s]+)\\s+(?<EVENT>[^\\[]+)\\s*\\[\\s*(?<ID>[^\\]]+)\\]\\s*:\\s*(?<DESCRIPTION>[^\\n]+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy-MM-dd HH:mm:ss";

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (context.SourceHeaderInfo.ContainsKey(key))
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

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"}, new []{"HOST_NAME"}, new []{"EVENT"},new []{"DESCRIPTION"} },
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new UbuntuAuthUnifiedRecorderContext(this);
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
