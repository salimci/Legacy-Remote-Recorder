using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;


namespace Natek.Recorders.Remote.Unified.IisFtpUnifiedRecorder
{
    public class IisFtpUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<DATE>[^\\s]+)\\s+(?<TIME>[^\\s]+)\\s+(?<C_IP>[^\\s]+)\\s+(?<CS_USERNAME>[^\\s]+)\\s+(?<SERVICE_NAME>[^\\s]+)\\s+(?<SERVER_NAME>[^\\s]+)\\s+(?<S_IP>[^\\s]+)\\s+(?<S_PORT>[^\\s]+)\\s+\\[(?<EVENT_ID>[^\\]]+)\\]\\s*(?<EVENT>[^\\s]+)\\s+(?<URI>[^\\s]+)\\s+(?<SC_STATUS>[^\\s]+)\\s+(?<SC_WIN32_STATUS>[^\\s]+)\\s+(?<SC_BYTES>[^\\s]+)\\s+(?<CS_BYTES>[^\\s]+)\\s+(?<TIME_TAKEN>[^\\s]+)\\s+(?<CS_VERSION>[^\\s]+)\\s+(?<CS_HOST>[^\\s]+)\\s+(?<USER_AGENT>[^\\s]+)\\s+(?<COOKIE>[^\\s]+)\\s+(?<REFERRER>[^\\s]+)\\s+(?<PROTOCOL_SUBSTATUS>[^\\n]+)$", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {
                        Original = new[] {new []{"DATE"},new []{"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"C_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URI"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_BYTES"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var dtStr = fieldvalues[0] + " " + fieldvalues[1];
            if (DateTime.TryParseExact(dtStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
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

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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
