using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using System.Globalization;


namespace Natek.Recorders.Remote.Unified.ApacheSyslogUnifiedRecorder
{
    public class ApacheSyslogUnifiedRecorder : SyslogRecorderBase
    {
        protected  static readonly Regex RegSplitForAll = new Regex("^(?<SOURCE_NAME>[^\\s]+)\\s*:\\s*(?<SEVERITY>[^\\s]+)\\s+(?<DATE_TIME_0>[\\w]+\\s+[0-9]+\\s+[0-9:]+)\\s+(?<COMPUTER_NAME>[^\\s]+)\\s+[\\w]+:\\s*(?<CUSTOM_STR_1>[^\\s]+)\\s+(?<EVENT_CATEGORY>[^\\s]+)\\s+(?<USERS_ID>[^\\s]+)\\s+\\[(?<DATE_TIME>[^\\]]+)\\]\\s+\"(?<EVENT_TYPE>[^\\s]+)\\s+(?<CUSTOM_STR_3>[^\\s]+)\\s+(?<CUSTOM_STR_4>[^\"]+)\"\\s+(?<CUSTOM_INT_1>[^\\s]+)\\s+(?<CUSTOM_INT_2>[^\\s]+)\\s+\"(?<CUSTOM_STR_2>[^\\\"]+)\"\\s+\"(?<CUSTOM_STR_9>[^\"]+)\"$", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"COMPUTER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),    
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_INT_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOM_INT_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            DateTime dt;
            var recorder = data as ApacheSyslogUnifiedRecorder;

            if (DateTime.TryParseExact(fieldValues[0], "dd/MMM/yyyy:HH:mm:ss zzzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (match.Success)
            {
                var groupCollection = match.Groups;

                foreach (var key in RegSplitForAll.GetGroupNames())
                {
                    int tmp;
                    try
                    {
                        if (!int.TryParse(key, out tmp))
                        {
                            int fieldBufferKey;
                            if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                                context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.Out.WriteLine(exception.Message);
                    }
                }

                return NextInstruction.Return;
            }
            return NextInstruction.Skip;
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
    }
}
