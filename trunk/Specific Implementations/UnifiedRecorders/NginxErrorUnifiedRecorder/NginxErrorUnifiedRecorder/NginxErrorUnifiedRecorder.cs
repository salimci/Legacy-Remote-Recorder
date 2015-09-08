using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.NginxErrorUnifiedRecorder
{
    public class NginxErrorUnifiedRecorder  : TerminalRecorder 
    {

        protected static readonly Regex RegSplitForAll = new Regex("((?<TIME_LOCAL>[^[]+)\\[(?<SEVERITY>[^]]+)\\](?<PID>[^#]+)\\#\\S+\\s+(\\*(?<CID>\\S+)\\s+)?(?<MESSAGE>[^(,]+|.*)(((?=,))|((?=\\()\\(\\)\\s*))?\\s*(\"(?<ADDRESS>[^\"]+)\")?(\\s*((?<ACTION>[^:\\s]+)(((?=\\:)\\:\\s+(?<ERROR_0>.*))|(?=\\s)[^:]+:\\s*(?<ERROR_1>[^)]+)\\))|([^,]+)))?((.*?client:\\s*(?<CLIENT>[^,]*))?(.*?server:\\s*(?<SERVER>[^,]*))?(.*?request:\\s*\"(?<REQUEST>[^\"]+)\")?(.*?host:\\s*\"(?<HOST>[^\"]+)\")?(.*?referrer:\\s*\"(?<REFERER>[^\"]+))?)?)", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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
                        Original = new[] {new[] {"ERROR_0"},new[] {"ERROR_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REQUEST"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REFERER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int64
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            return fieldvalues[0];
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
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new NginxErrorUnifiedRecorderContext(this);
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
    }
}
