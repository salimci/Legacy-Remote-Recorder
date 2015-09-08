using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using System.Globalization;

namespace Natek.Recorders.Remote.Unified.LinuxDnsUnifiedRecorder
{
    public class LinuxDnsUnifiedRecorder : TerminalRecorder
    {

        protected static readonly Regex RegSplitForAll = new Regex(@"^(?<DateTime>.*?\s.*?\s.*?[^\s]+)\s(?<SourceName>.*?[^:]+)\:.*?\s.*?\s(?<CustomStr1>[^#]+)\#(?<CustomStr2>[^:]+)\:\s.*?\s(?<CustomStr3>[^:]+):\s(?<CustomStr4>.*?\))\s(?<CustomStr5>.*?'.*?')\s(?<CustomStr6>[^\s]+)$", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxDnsUnifiedRecorderContext(this);
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {                                    
                        Original = new[] {new []{"DateTime"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                        },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SourceName"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var recorder = data as LinuxDnsUnifiedRecorder;
            if (DateTime.TryParseExact(fieldvalues[0], "MMM dd HH:mm:ss", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone)
                         .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                try
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    int fieldBufferKey;
                    if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            return NextInstruction.Return;
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

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
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
