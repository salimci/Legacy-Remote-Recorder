using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.NetscalerSyslogUnifiedRecorder
{
    public class NetscalerSyslogUnifiedRecorder : SyslogRecorderBase
    {

        protected static readonly Regex RegSplitForAll = new Regex("((?<SOURCE_NAME>[^\\s]+)\\s+:\\s*(?<SEVERITY>[^\\s]+)\\s+(?<DATE_TIME>[^\\s]+)\\s+(?<TIMEZONE>[^\\s]+)\\s+(?<CLIENT>[^\\s]+)\\s+(?<USERNAME>[^\\s]+)\\s+:\\s+(?<EVENT_TYPE>\\S+)\\s+(?<EVENT_CATEGORY>\\S+)\\s+(?<CUSTOMINT1>\\S+)\\s+0\\s+:\\s+((?<Source2>\\S+)\\s+\\S+\\s+\\S+\\s+(?<CustomStr9_0>\\S+)[^:]+:\\s(?<URL>\\S+)\\s<(?<CUSTOMSTR7_0>[^>]+)>)?)|(\\s*(?<x>.*?)\\s\"?(?<y>(?<=\")[^\"]+|\\S+(?:\\sGMT)?)\"?(?:(?:(\\s-\\s)|$)))", RegexOptions.Compiled);
        protected static readonly Regex RegSplitForPort = new Regex("([^:]+):(\\S+)", RegexOptions.Compiled);


        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "dd/MM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        private object SourceSplit(RecWrapper rec, string field, string[] values, object data)
        {
            if (values[0] != null)
            {
                var m = RegSplitForPort.Match(values[0]);
                if (m.Success)
                {
                    rec.CustomInt3 = Convert.ToInt32(m.Groups[2].Value);
                    return m.Groups[1].Value;
                }
                return values[0];
            }
            return string.Empty;
        }

        private object DestinationSplit(RecWrapper rec, string field, string[] values, object data)
        {
            if (values[0] != null)
            {
                var m = RegSplitForPort.Match(values[0]);
                if (m.Success)
                {
                    rec.CustomInt4 = Convert.ToInt32(m.Groups[2].Value);
                    return m.Groups[1].Value;
                }
                return values[0];
            }
            return string.Empty;
        }

        private object VServerSplit(RecWrapper rec, string field, string[] values, object data)
        {
            if (values[0] != null)
            {
                var m = RegSplitForPort.Match(values[0]);
                if (m.Success)
                {
                    rec.CustomInt7 = Convert.ToInt32(m.Groups[2].Value);
                    return m.Groups[1].Value;
                }
                return values[0];
            }
            return string.Empty;
        }

        private object SourcePortMethod(RecWrapper rec, string field, string[] values, object data)
        {
            if (rec.CustomInt3 != 0)
            {
                return rec.CustomInt3;
            }
            
            if (values[0] != null)
            {
                int i;
                return Int32.TryParse(values[0], out i) ? i : 0;
            }
            return 0;
        }

        private object VserverServicePortMethod(RecWrapper rec, string field, string[] values, object data)
        {
            if (rec.CustomInt7 != 0)
            {
                return rec.CustomInt7;
            }

            if (values[0] != null)
            {
                int i;
                return Int32.TryParse(values[0], out i) ? i : 0;
            }
            return 0;
        }

        private object NatIPSplit(RecWrapper rec, string field, string[] values, object data)
        {
            if (values[0] != null)
            {
                var m = RegSplitForPort.Match(values[0]);
                if (m.Success)
                {
                    rec.CustomInt8 = Convert.ToInt32(m.Groups[2].Value);
                    return m.Groups[1].Value;
                }
                return values[0];
            }
            return string.Empty;
        }



        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"), 
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new [] {"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Session"}, new[] {"Start Time"}, new[] {"Status"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"End Time"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Source"}, new [] {"ClientIP"}, new [] {"Source2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                        MethodInfo = SourceSplit
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Destination"}, new[] {"Remote_ip"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                        MethodInfo = DestinationSplit
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Vserver"}, new [] {"VserverServiceIP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5"),
                        MethodInfo = VServerSplit
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NatIP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                        MethodInfo = NatIPSplit
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ClientVersion"}, new[] {"User"}, new [] {"CUSTOMSTR7_0"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CipherSuite"}, new[] {"Command"}, new [] {"CustomStr9_0"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"SPCBId"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ClientPort"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = SourcePortMethod
                    },
                 
                    new DataMapping
                    {
                        Original = new[] {new[] {"Total_bytes_send"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Total_bytes_recv"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"VserverServicePort"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = VserverServicePortMethod
                    },
                  
                    new DataMapping
                    {
                        Original = new[] {new[] {"TOTAL_BYTES_RECV"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
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

            match = match.NextMatch();

            while (match.Success)
            {
                if (context.SourceHeaderInfo.ContainsKey(match.Groups["x"].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups["x"].Value]] = match.Groups["y"].Value;
                match = match.NextMatch();
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
