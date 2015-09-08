using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.CiscopixSyslogUnifiedRecorder
{
    public class CiscopixSyslogUnifiedRecorder : SyslogRecorderBase
    {

        protected static readonly Regex RegSplitForAll = new Regex("^(?<GATEWAY>[^:]+):(?<GATEWAY_PORT>[^\\s]+)\\s+:\\s+(?<SEVERITY>[^\\s]+)\\s+(?<COMPUTERNAME>[^:]+):\\s+(?<EVENT>.*?)from\\s+(?<SUBNET_FROM>[^:]+):(?<SOURCE_IP>[^\\s]+)\\s+to\\s+(?<SUBNET_TO>[^:]+):(?<ASSIGNED_IP>[^\\s]+)\\s+((duration\\s+(?<DURATION>[^\\s]+)\\s+)|())$", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"COMPUTERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GATEWAY"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ASSIGNED_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SUBNET_FROM"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SUBNET_TO"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                   new DataMapping
                    {
                        Original = new[] {new[] {"GATEWAY_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"DURATION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
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
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
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
