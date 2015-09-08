using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.LinuxDhcpUnifiedRecorder
{
    public class LinuxDhcpUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(?<EVENTCATEGORY>[^:]+):\\s+(?<EVENT_TYPE>[^\\s]+)(((?<=DHCPREQUEST)(\\s+for\\s+(?<ASSIGNED_IP_0>[^\\s]+)\\s+from\\s+(?<MAC_ADDRESS_0>[^\\s]+)\\s+via\\s+(?<NIC_INTERFACE_0>[^\\s]+)))|((?<=DHCPACK)(\\s+on\\s+(?<ASSIGNED_IP_1>[^\\s]+)\\s+to\\s+(?<MAC_ADDRESS_1>[^\\s]+)\\s+((\\((?<COMPUTERNAME>[^\\)]+)\\)\\s+)|())via\\s+(?<NIC_INTERFACE_1>[^\\s]+)))|((?<=DHCPDISCOVER)(\\s+from\\s+(?<MAC_ADDRESS_2>[^\\s]+)\\s+via\\s+(?<GATEWAY_0>(([^:]+)|([^\\s]+)))((:\\s+[^\\s]+\\s+(?<NETWORK_ADDRESS>[^:]+):(?<CUSTOMSTR7_0>[^\\n]+))|())))|(?<=DHCPDECLINE)(\\s+of\\s+(?<ASSIGNED_IP_2>[^\\s]+)\\s+from\\s+(?<MAC_ADDRESS>[^\\s]+)\\s+via\\s+(?<GATEWAY_1>[^:]+):\\s+(?<CUSTOMSTR7_1>[^\\n]+)))", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENTCATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"COMPUTERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ASSIGNED_IP_0"}, new []{"ASSIGNED_IP_1"}, new []{"ASSIGNED_IP_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MAC_ADDRESS_0"}, new []{"MAC_ADDRESS_1"}, new []{"MAC_ADDRESS_2"}, new []{"MAC_ADDRESS_3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NIC_INTERFACE_0"}, new []{"NIC_INTERFACE_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GATEWAY_0"}, new []{"GATEWAY_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"NETWORK_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR7_0"}, new []{"CUSTOMSTR7_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
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

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxDhcpUnifiedRecorderContext(this);
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
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
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
