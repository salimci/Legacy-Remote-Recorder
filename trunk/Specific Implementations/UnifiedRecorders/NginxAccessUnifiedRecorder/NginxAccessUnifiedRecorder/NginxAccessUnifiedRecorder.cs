using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.NginxAccessUnifiedRecorder
{
    public class NginxAccessUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("(?<REMOTE_ADDRESS>[^\\s]+)\\s+(?<HYPHEN>[^\\s]+)\\s+(?<REMOTE_USER>[^\\s]+)\\s+\\[(?<TIME_LOCAL>[^\\]]+)\\s*\\]\\s*\"(?<METHOD>[^\\s]+)\\s+(?<URI>[^\\s]+)\\s+(?<PROTO>[^\"]+)\"\\s*(?<STATUS>[0-9]+)\\s*(?<BODY_BYTES_SENT>[0-9]+)\\s*\"(?<HTTP_REFERER>.[^\"]+)\"\\s*\"(?<HTTP_USER_AGENT>.[^\"]+)\"\\s*", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new NginxAccessUnifiedRecorderContext(this);
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {
                        Original = new[] {new []{"TIME_LOCAL"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTO"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REMOTE_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REMOTE_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HTTP_USER_AGENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HTTP_REFERER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"BODY_BYTES_SENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "dd/MMM/yyyy:HH:mm:ss zzzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
