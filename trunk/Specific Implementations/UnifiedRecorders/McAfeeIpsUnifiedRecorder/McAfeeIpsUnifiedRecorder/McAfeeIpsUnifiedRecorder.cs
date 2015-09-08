using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.McAfeeIpsUnifiedRecorder
{
    public class McAfeeIpsUnifiedRecorder: SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"^(((?<SOURCE_IP_0>[^:]+):[^:]+:[^.]+\.(?<SYSLOG_SEVERITY_0>[^\s]+)\s+(?<DATE_TIME_0>[^\s]+\s+[^\s]+\s+[^\s]+)\s+([^\)]+\)\s+)?[^:]+:\s+(?<COMPUTER_NAME_0>[^\s]+)\s+[^\s]+\s+(?<EVENT_0>[^\:]+)\:\s+(?<EVENT_MESSAGE_0>[^\(]+)\((?<SEVERITY_0>[^\)]+)[^\s]+\s+(?<CLIENT_IP_0>[^\:]+)\:(?<CLIENT_PORT_0>[^\s]+)\s+[^\s]+\s+(?<DESTINATION_IP_0>[^\:]+):(?<DESTINATION_PORT_0>[^\s]+).+)|([^\.]+\.(?<SYSLOG_SEVERITY_1>[^\s]+)\s+(?<SOURCE_IP_1>[^\s]+)\s+(?<DATE_TIME_1>[^\s]+\s+[^\s]+\s+[^\s]+)\s+([^\)]+\)\s+)?(?<COMPUTER_NAME_1>[^\s]+)\s+[^\s]+\s+(?<EVENT_1>[^\:]+)\:\s+(?<EVENT_MESSAGE_1>[^\(]+)\((?<SEVERITY_1>[^\)]+)[^\s]+\s+(?<CLIENT_IP_1>[^\:]+)\:(?<CLIENT_PORT_1>[^\s]+)\s+[^\s]+\s+(?<DESTINATION_IP_1>[^\:]+):(?<DESTINATION_PORT_1>[^\s]+).+)|(.*?:\s*(?<DATE>[0-9\-]*\s*[0-9\:]*\s*[a-zA-Z]*)\s*\|\s*((?<THREAD_CODE>[^:]*):\s*(?<THREAT>[^\|]*))\s*\|\s*(?<SEVERITY_2>[^\|]*)\s*\|\s*((?<TARGET_IP>[^:]*):(?<TARGET_PORT>[^\|]*))\s*\|\s*((?<SRC_IP>[^:]*):(?<SRC_PORT>[^\|]*))\s*\|\s*(?<RULE>.[^\|]*)\s*\|\s*(?<COUSE>.[^\|]*)\s*\|\s*(?<DIRECTION>[^\|]*)\s*\|\s*(?<RESULT>[^\|]*)\s*\|\s*(?<LAYER>[^\|]*)\s*\|\s*(?<PROTOCOL>[^\|]*)\s*\|\s*(?<FLAG>[^\|]*)))$", RegexOptions.Compiled);

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "MMM dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

         protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original   = new []{new []{"DATE"}, new []{"DATE_TIME_0"}, new []{"DATE_TIME_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"RULE"}, new []{"SEVERITY_0"}, new []{"SEVERITY_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"COUSE"}, new []{"EVENT_0"}, new []{"EVENT_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr2")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_IP"}, new []{"EVENT_MESSAGE_0"}, new []{"EVENT_MESSAGE_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TARGET_IP"}, new []{"CLIENT_IP_0"}, new []{"CLIENT_IP_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DIRECTION"}, new []{"DESTINATION_IP_0"}, new []{"DESTINATION_IP_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RESULT"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LAYER"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY_2"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"THREAT"}, new []{"SYSLOG_SEVERITY_0"}, new []{"SYSLOG_SEVERITY_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"COMPUTER_NAME_0"},new[] {"COMPUTER_NAME_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"THREAT_CODE"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventCategory"),
                        MethodInfo = Concatinate
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"SOURCE_IP_0"},new []{"SOURCE_IP_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("SourceName")
                    }, 
                    new DataMapping
                    {
                        Original = new[] {new[] {"FLAG"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TARGET_PORT"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_PORT"}, new []{"CLIENT_PORT_0"}, new []{"CLIENT_PORT_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new []{"DESTINATION_PORT_0"},new []{"DESTINATION_PORT_1"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt5"),
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

        private object Concatinate(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            var temp = string.Empty;

            foreach (var fieldvalue in fieldvalues)
            {
                if (!string.IsNullOrEmpty(fieldvalue))
                {
                    temp += fieldvalue;
                }
            }
            return temp;
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
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
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
