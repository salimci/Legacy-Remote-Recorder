using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.ArubaWifiControllerUnifiedRecorder
{
    public class ArubaWifiControllerUnifiedRecorder : SyslogRecorderBase
    {

        protected static readonly Regex RegSplitForAll = new Regex("(?<SOURCE_IP>[^:]+):(?<SOURCE_PORT>[^\\s]+)\\s+:\\s+(?<SYSLOG_SEVERITY>[^\\s]+)(?<DATE_TIME>(\\s[^\\s]+){4})\\s+(?<USERNAME>[^\\s]+)((.*?>)|())\\s+(?<PROCESS>[^\\[]+)\\[(?<PROCESS_ID>[^\\]]+)\\]:\\s+(<(?<ERROR_NUMBER>[^>]+)>\\s+<(?<SEVERITY>[^>]+)>\\s+((<[^\\s]+\\s+(?<CLIENT_IP>[^>]+)>\\s+)|())(((\\|(?<ADDRESS>[^\\s]+\\s+[^\\s]+)[^\\|]+\\|.*?)|())\\|(?<UNUSED_FIELD>[^\\|]+)\\|(((\\s+[^:]+:\\s+(?<MESSAGE_0>[^=]+)=\\s+(?<MAC_ADDRESS>[^\\s]+)[^=]+=\\s+(?<SSID>[^\\s]+)\\s+[^\\s]+\\s+(?<BSSID>[^\\s]+))|([^:]+:\\s+(?<MESSAGE_1>[^\\n]+)))|(\\s+[^\\s]+\\s+[^\\s]+\\s+(?<RADIUS_SERVER>.+)\\-[^\\s]+\\s+(?<ERROR_MESSAGE>[^=]+)=(?<CLIENT_ADDRESS>[^\\s]+)\\s+.+\\s(?<METHOD>[^\\s]+))))|((.+\\s(?<FROM_STATION>[^\\s]+)\\s+(?<TO_STATION>[^,]+),\\s+(?<MESSAGE_2>[^\\n]+))|((?<MESSAGE_TEXT>[^:]+))))", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserId"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR_NUMBER"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SYSLOG_SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MAC_ADDRESS"}, new []{"FROM_STATION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"BSSID"}, new []{"CLIENT_ADDRESS"}, new []{"TO_STATION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SSID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE_1"}, new []{"MESSAGE_0"}, new []{"ERROR_MESSAGE"}, new []{"MESSAGE_TEXT"}, new []{"MESSAGE_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROCESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_IP"}, new []{"USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RADIUS_SERVER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                   new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROCESS_ID"}},
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
