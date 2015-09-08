using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.SquidSyslogUnifiedRecorder
{
    public class SquidSyslogUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<HOST_IP>[^:]+):(?<HOST_PORT>[^\\s]+)\\s*:\\s*(?<EVENT>[^\\s]+)\\s+(?<DATE>[\\w]+\\s+[0-9]+\\s[0-9:]+)\\s+(?<PROCESS>[^\\[]+)\\[(?<LOG_ID>[^\\]]+)\\]:\\s+(?<ELAPSED_TIME>[^\\s]+)\\s+(?<CODE>[^\\s]+)\\s+(?<IP>[^\\s]+)\\s+((?<CACHE_STATUS>[^\\/]+)\\/)(?<STATUS_CODE>[^\\s]+)\\s+(?<BYTES>[^\\s]+)\\s+(?<METHOD>[^\\s]+)\\s+(?<URL_PROTOCO>[^\\s]+)\\s+(?<URL_ADDRESS>[^\\s]+)\\s+(?<RFC931>[^\\s]+)\\s+(?<PEERSTATUS>[^\\/]+)\\/(?<PEERHOST>[^\\s]+)\\s+(?<TYPE>[^\\s]+)$", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"DATE"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PEERSTATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PEERHOST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URL_PROTOCO"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URL_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"STATUS_CODE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URL_PROTOCO"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CACHE_STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),    
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            DateTime dt;
            var recorder = data as SquidSyslogUnifiedRecorder;
            var dtStr = fieldValues[0];
            if (DateTime.TryParseExact(dtStr, "MMM d HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
