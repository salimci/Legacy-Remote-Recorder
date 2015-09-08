using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.LabrisNetworkSyslogUnifiedRecorder
{
    public class LabrisNetworkSyslogUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"((?<HOST_NAME>[^:]+):(?<HOST_PORT>\S+)\s+\:\s+\S+\s+(?<DATE_TIME>[^:]+\S+)[^]]+\]\s+\S+\s+(?<CUSTOM_STR_1>\S+)\s+(?<EVENT_CATEGORY>\S+)\s+)|((([^\s=]+)\=(.*?)(\s|$)))", RegexOptions.Compiled);

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as LabrisNetworkSyslogUnifiedRecorder;
            if (DateTime.TryParseExact(values[0], "MMM dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(values[0], "MM-dd-yyyy\tHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

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
                        Original = new[] {new []{"HOST_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new []{"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"MAC"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new []{"PROTO"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                   
                      new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_STR_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"IN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"SRC"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"DST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new []{"LEN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"SPT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"DPT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"WINDOW"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
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

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
        }
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            var matchState = 0;
            while (match.Success)
            {
                var groupCollection = match.Groups;
                if (matchState == 0)
                {
                    foreach (var key in RegSplitForAll.GetGroupNames())
                    {
                            int tmp;
                            matchState = 1;
                            if (int.TryParse(key, out tmp)) continue;
                            int fieldBufferKey;
                            if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                            context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

                        
                    }
                    match = match.NextMatch();
                }
                else
                {
                    if (context.SourceHeaderInfo.ContainsKey(match.Groups[4].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups[4].Value]] = match.Groups[5].Value;
                    match = match.NextMatch();
                }

            }

            return NextInstruction.Return;
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
