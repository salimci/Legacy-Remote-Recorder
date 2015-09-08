using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.CoslatUrlUnifiedRecorder
{
    public class CoslatUrlUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"(?<DATE>[a-zA-Z]*\s*[0-9]*,\s*[0-9]*\s*[0-9\:\.]*)\s*(?<SRC_IP>[0-9\.]*)\s*(?<SRC_MAC>.[^\s]*)\s*(?<DST_IP>[0-9\.]*)\s*(?<METHOD>.[^\s]*)\s*(?<URL>.[^\s]+)\s+(?<URI>.+)", RegexOptions.Compiled);
        protected string DateFormat = "MMM/dd/yyyy hh:mm:ss.zzzz";


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
                        Original = new[] {new[] {"SRC_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_MAC"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"DST_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"URI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    }
                }
            };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new CoslatUrlUnifiedRecorderContext(this);
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }


        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
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
