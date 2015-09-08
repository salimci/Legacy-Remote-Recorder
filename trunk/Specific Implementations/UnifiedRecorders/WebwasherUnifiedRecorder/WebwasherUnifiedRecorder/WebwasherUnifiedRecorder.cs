using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;


namespace Natek.Recorders.Remote.Unified.WebWasherUnifiedRecorder
{
    public class WebwasherUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("(?<SRC_IP>[^\\s]+)\\s+(?<SERVER_IP>[^\\s]+)\\s+\"(?<AUTH_USER>[^\"]+)\"\\s+\\[(?<TIME_STAMP>[^\\]]+)\\]\\s+\"(?<METHOD>[^\\s]+)\\s*(?<REQ_LINE>[^\\s]+)\\s*(?<PROTO>[^\"]+)\"\\s+(?<STATUS_CODE>[^\\s]+)\\s+(?<BYTE_FROM_CLIENT>[^\\s]+)\\s+(?<BYTE_TO_CLIENT>[^\\s]+)\\s+\"(?<REFERER>[^\"]+)\"\\s+\"(?<USER_AGENT>[^\"]+)\"\\s+\"(?<ATTRIBUTE>[^\"]+)\"\\s+(?<BLOCK_RES>[^\\s]+)\\s+\"(?<MEDIA_TYPE>[^\"]+)\"\\s+\"(?<PROFILE>[^\"]+)\"\\s+(?<ELAPSED_TIME>[^\\s]+)\\s+\"(?<VIRUS_NAME>[^\"])\"\\s+(?<REP_LEVEL>[^\\s]+)\\s+(?<CACHE_STATUS>[^\\n]+)", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        private DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {
                        Original = new[] {new []{"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTO"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NO_1","NO_2","PATH"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER","KEY"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ACT"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "dd/MMM/yyyy:HH:mm:ss zzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
        
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                try
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    int fieldBufferKey;
                    if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            return NextInstruction.Return;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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
