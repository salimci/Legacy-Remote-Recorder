using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.BarracudaWafUnifiedRecorder
{
    public class BarracudaWafUnifiedRecorder : SyslogRecorderBase
    {
        protected static Regex RegSplitForAll = new Regex("^(?<SOURCENAME>[^:]+):(?<SOURCE_PORT>\\S+)\\s:\\s\\S+\\s\\S+\\s+\\S+\\s\\S+\\s(?<UNIT_NAME>\\S+)\\s(?<TIME_STAMP>[^.]+)((\\S+\\s+\\S+\\s+\\S+\\s+(?<LOG_TYPE>(.(?![0-9]+.))*)\\s+(?<CLIENT_IP>\\S+)\\s(?<CLIENT_PORT>\\S+)\\s+(?<APPLICATION_IP>\\S+)\\s(?<APPLICATION_PORT>\\S+)\\s+\\S+\\s+(?<RULE_TYPE>(.(?!\\s\"?\\[))*.)\\s+\\[[^]]+\\]\\s+(?<METHOD>\\S+)\\s+(?<URL>[^?]+)[^=]+=(?<USER>[^&]+)[^=]+=(?<DEVICE_ID>[^&]+)[^=]+=(?<DEVICE_TYPE>[^&]+)\\S+\\s+(?<PROTOCOL>\\S+)\\s\"-\"\\s\"(?<USER_AGENT>[^\"]+)\".*)|((.(?![0-9]+\\.))*.(?<APPLICATION_IP_1>\\S+)\\s+(?<APPLICATION_PORT_1>\\S+)\\s+(?<CLIENT_IP_1>\\S+)\\s+(?<CLIENT_PORT_1>\\S+)\\s\\S+\\s\\S+\\s(?<METHOD_1>\\S+)\\s(?<PROTOCOL_1>\\S+)\\s(?<HOST>\\S+\\s\\S+)(.(?![0-9]+\\.))*.(?<SERVER_IP>\\S+)(.(?!\"-\"))*\\s\"-\"\\s(?<CUSTOMSTR2>(.(?!\\/))*)\\s\\S+\\s+([^=]+=(?<USER_1>[^&]+)[^=]+=(?<DEVICE_ID_1>[^&]+)&[^=]+=(?<DEVICE_TYPE_1>[^&]+)\\S+)?\\s?(\"-\"\\s)+\"(?<USER_AGENT_1>[^\"]+)\".*))", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"TIME_STAMP"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCENAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"METHOD_1"}, new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                 
                       new DataMapping
                    {
                        Original = new[] {new[] {"USER"}, new[] {"USER_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),
                   
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"UNIT_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"APPLICATION_IP_1"}, new []{"APPLICATION_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new [] {"CUSTOMSTR2"}, new[] {"LOG_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST"}, new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_IP"}, new[] {"CLIENT_IP_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE_ID"}, new[] {"DEVICE_ID_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE_TYPE"}, new[] {"DEVICE_TYPE_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"USER_AGENT_1"}, new[] {"PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"PROTOCOL_1"}, new[] {"USER_AGENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVER_IP"}, new[] {"RULE_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"APPLICATION_PORT"}, new[] {"APPLICATION_PORT_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_PORT"}, new[] {"CLIENT_PORT_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    }
                   
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            var groupCollection = match.Groups;
            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
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
