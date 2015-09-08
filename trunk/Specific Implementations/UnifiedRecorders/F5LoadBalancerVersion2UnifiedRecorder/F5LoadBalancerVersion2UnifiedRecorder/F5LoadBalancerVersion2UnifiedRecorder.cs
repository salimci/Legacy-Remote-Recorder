using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.F5LoadBalancerVersion2UnifiedRecorder
{
    public class F5LoadBalancerVersion2UnifiedRecorder:SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"^(?<COMPUTERNAME>[^\[]+).*?\s(?<DATE>[^\s]+)\s(?<TIME>[^\s]+)\s(?<DESTINATION_IP>[^\s]+)\s(?<EVENTTYPE>[^\s]+)\s(?<CUSTOMSTR6>[^\s]+)\s(?<CLIENT_IP>[^\s]+)\s(?<POSITION>[^\s]+)\s(?<USERAGENT>[^\s]+)\s-\s+(?<URL>.*?(((http).*?\s)|()))(?<DOMAIN>[^\s]+)\s(?<PORT>[^\s]+)\s.*?\s.*?\s.*?\s.*?\s(?<USED>[^\s]+)$", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
      
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"},new []{"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"), 
                       },
                    new DataMapping
                    {
                        Original = new[] {new[] {"COMPUTERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"), 
                       },
                    
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENTTYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DESTINATION_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR6"}}, 
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DOMAIN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"POSITION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_AGENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                   
                    new DataMapping
                    {
                        Original = new[] {new[] {"USED"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
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

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null) return base.OnBeforeProcessRecordInput(context);
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
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
            if (MappingInfos == null) return NextInstruction.Do;
            foreach (var mappingInfo in MappingInfos)
            {
                context.SourceHeaderInfo = MimicMappingInfo(mappingInfo.Mappings);
                context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                break;
            }
            return NextInstruction.Do;
        }
    }
}
