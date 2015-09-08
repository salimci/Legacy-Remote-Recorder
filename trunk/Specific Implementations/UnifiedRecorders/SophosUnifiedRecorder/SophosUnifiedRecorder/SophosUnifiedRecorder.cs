using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.SophosUnifiedRecorder
{
    public class SophosUnifiedRecorder : SyslogRecorderBase
    {
        protected static Regex RegSplitForAll = new Regex("([^:]+:[^:]+:\\s+\\S+\\s+(?<DATE_TIME>\\S+)\\s+(?<SOURCE_NAME>\\S+)[^:]+\\:\\s+\\S+\\s+[^\"]+\"(?<EVENT_CATEGORY>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_6>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_7>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_5>.*?)\"\\s+[^\"]+\"(?<EVENT_TYPE>.*?)\"\\s+[^\"]+\"(?<CUSTOM_INT_1>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_8>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_9>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_1>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_2>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_3>.*?)\"\\s+[^\"]+\"(?<CUSTOM_STR_4>.*?)\"\\s+[^\"]+\"(?<PROTO>.*?)\"\\s+[^\"]+\"(?<LENGTH>.*?)\"\\s+[^\"]+\"(?<TOS>.*?)\"\\s+[^\"]+\"(?<PREC>.*?)\"\\s+[^\"]+\"(?<TTL>.*?)\"\\s+[^\"]+\"(?<CUSTOM_INT_3>.*?)\"\\s+[^\"]+\"(?<CUSTOM_INT_4>.*?)(\"|$)(\\s+tcpflags\\=\\\"[^\\\"]+?\\\")?)", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_6"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_7"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_8"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_9"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_INT_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_INT_3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_INT_4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "yyyy:MM:dd-HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
