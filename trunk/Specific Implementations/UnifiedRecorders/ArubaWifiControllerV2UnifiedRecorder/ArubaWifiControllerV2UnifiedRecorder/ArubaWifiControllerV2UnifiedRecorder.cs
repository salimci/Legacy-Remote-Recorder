using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.ArubaWifiControllerV2UnifiedRecorder
{
    public class ArubaWifiControllerV2UnifiedRecorder : SyslogRecorderBase
    {

        protected static readonly Regex RegSplitForAll = new Regex(@"^.+\s+:(?<ERROR_NUMBER>[^:]+):\s+\<(?<SEVERITY>[^\>]+)\>((.+\s+(?<AP_MAC_ADDRESS>[^@]+)@(?<AP_IP_ADDRESS>[^\s]+).+(?<STA_BSS>sta[^\s]+\s+bss[^\s]+).+)|(([^|]+\|){2}(?<MESSAGE>[^:]+):\s+username=(?<USERNAME_0>[^\s]+)\s+MAC=(?<MAC_0>[^\s]+)\s+IP=(?<IP_0>[^\s]+).+(?<VLAN_0>VLAN=[^\s]+)\s+AP=(?<AP>[^\s]+)\s+SSID=(?<SSID>[^\s]+).+method=(?<AUTH_METHOD>[^\s]+).+=(?<AUTH_SERVER>[^\s]+))|(.+type:\s*(?<TYPE>[^\s]+)\s+ip\s*(?<IP_1>[^\s]+)\s+mac\s+(?<MAC_1>[^\s]+).+apname\s+(?<AP_NAME>[^\,]+).+username\s+(?<USERNAME>[^\,]+)\,\s*(?<VLAN_1>vlan\s+[^\s]+))|(.+mac\s+(?<MAC_2>[^\s]+).+name=(?<NAME>[^\,]+).+)|(.+MAC=(?<MAC_3>[^\,]+),IP=(?<IP_2>[^\s]+).+idle-timeout=(?<IDLE_TIMEOUT>[^\s]+)))$", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"AP_MAC_ADDRESS"}, new []{"AP"}, new []{"AP_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AUTH_SERVER"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USERNAME_0"}, new []{"USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"AP_IP_ADDRESS"}, new []{"IP_0"}, new []{"IP_1"}, new []{"NAME"}, new []{"IP_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"MAC_0"}, new []{"MAC_1"},new []{"MAC_2"},new []{"MAC_3"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"SSID"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"STA_BSS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"VLAN_0"}, new []{"VLAN_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AUTH_METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR_NUMBER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"IDLE_TIMEOUT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
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
