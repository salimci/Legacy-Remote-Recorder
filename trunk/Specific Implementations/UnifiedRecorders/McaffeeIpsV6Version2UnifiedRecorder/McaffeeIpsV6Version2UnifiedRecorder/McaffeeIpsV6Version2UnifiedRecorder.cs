using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.McaffeeIpsV6Version2UnifiedRecorder
{
    public class McaffeeIpsV6Version2UnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"(?<DATE>[^\s]+)\s(?<TIME>[^\s]+)\s+.+?\s(?<COMPNAME>[^\s]+).+?:.+?:.+?:.+?\s[^\(]+\(.+?=\s*(?<SEVERITY>[^\)]+)\).*?\s(?<SOURCE_IP>[^\:]+).*?\>\s*(?<TARGET_IP>[^\:]+).*?\=\s*(?<EVENT_TYPE>[^\)]+)\)\s.*?\s(?<PROTOCOL>.+?)([^\s]+\s+){2}[0-9]+.*?((\:)|(\s.*?\s.*?\s))(?<CAUSE>.*?)[0-9].*?3A-3B.*?[^a-z]+((.*?tcp\s)|(.*?udp\s)|())+(?<STATUS>.*?\s)", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original   = new []{new []{"DATE"},new []{"TIME"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"COMPNAME"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr7")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TARGET_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CAUSE"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"STATUS"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr10")
                    }
                  
                }
            };
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

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null)
            { return base.OnBeforeProcessRecordInput(context); }
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                try
                {
                    if (!int.TryParse(key, out tmp))
                    {
                        int fieldBufferKey;
                        if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                            context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                    }
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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

