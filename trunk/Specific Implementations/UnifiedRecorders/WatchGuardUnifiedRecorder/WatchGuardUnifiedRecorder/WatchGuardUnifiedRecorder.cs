using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.WatchGuardRecorder
{
    public class WatchGuardUnifiedRecorder : SyslogRecorderBase
    {
        protected readonly Regex RegSplitForAll = new Regex("[0-9\\-]*\\s*[0-9\\:]*\\t.[^\\s]*\\s*[0-9\\.]*\\s*(?<DATE>[a-zA-Z]+\\s*[0-9]+\\s*[0-9:]+)\\s*(?<HOST>[a-zA-Z]*)\\s*(?<EVENT>.[^\\[]*)\\[(?<EVENT_ID>.[^\\]]*)\\]:\\s*(?<ACT>.[^\\s]*)\\s*(?<HOSTNAME>.[^\\s]*)\\s*(?<INTERFACE>.[^\\s]*)\\s*(?<PROTO>.[^\\s]*)\\s*(?<SRC_IP>[0-9\\.]*)\\s*(?<DST_IP>[0-9\\.]*)\\s*(?<SRC_PORT>[0-9]*)\\s*(?<DST_PORT>[0-9]*)\\s*msg=\"(?<MSG>.[^\"]*)\"\\s*proxy_act=\"(?<PROXY_ACT>.[^\"]*)\"\\s*op=\"(?<OP>.[^\"]*)\"\\s*dstname=\"(?<DSTNAME>.[^\"]*)\"\\s*arg=\"(?<ARG>.[^\"]*)\"\\s*sent_bytes=\"(?<SENT_BYTES>.[^\"]*)\"\\s*rcvd_bytes=\\\"(?<RCVD_BYTES>.[^\"]*)\"\\s*elapsed_time=\"(?<ELAPSED_TIME>.[^\"]*)\"\\s*reputation=\"(?<REPUTATION>.[^\"]*)\"\\s*reason=\"(?<REASON>.[^\"]*)\"\\s*action=\"(?<ACTION>.[^\"]*)\"\\s*", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"MSG"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROXY_ACT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DST_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"INTERFACE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DSTNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ARG"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ELAPSED_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"OP"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DST_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SENT_BYTES"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RCVD_BYTES"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
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

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
           
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                    
                int fieldBufferKey;
                if(context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                    context.FieldBuffer[fieldBufferKey] = groupCollection[key].Value;
            }

            context.FieldBuffer.Add(source);
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
    }
}
