using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.CyberroamUnifiedRecorder
{
    public class CyberroamUnifiedRecorder : SyslogRecorderBase
    {

        protected static readonly Regex RegSplitForAll = new Regex(@"([^=]+)=([^\s]+)\s*", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"date"},new[] {"time"},new[] {"timezone"} },
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"protocol"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"user_gp"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"src_ip"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dst_ip"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"category"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"contenttype"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"domain"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"status"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8"),    
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"url"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"fw_rule_id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"iap"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"src_port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dst_port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"httpresponsecode"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"sent_bytes"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"recv_bytes"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"device_name"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"log_type"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"log_component"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"user_name"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),    
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"device_id"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),    
                    }

                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            DateTime dt;
            var recorder = data as CyberroamUnifiedRecorder;
            var dtStr = fieldValues[0] + " " + fieldValues[1];
            if (DateTime.TryParseExact(dtStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            while (match.Success)
            {
                if (context.SourceHeaderInfo.ContainsKey(match.Groups[1].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups[1].Value]] = match.Groups[2].Value;
                match = match.NextMatch();
            }
            return NextInstruction.Return;
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo == null)
            {
                Exception error = null;
                var ins = GetHeaderInfo(context, ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    return ins;
            }
            return base.OnBeforeProcessRecordInput(context);
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
