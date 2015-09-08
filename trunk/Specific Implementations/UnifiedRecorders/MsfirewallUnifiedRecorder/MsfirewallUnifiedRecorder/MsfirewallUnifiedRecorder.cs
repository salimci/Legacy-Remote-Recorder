using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.MsfirewallUnifiedRecorder
{
    public class MsfirewallUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<DATE>[^\\s]+)\\s+(?<TIME>[^\\s]+)\\s+(?<ACTION>[^\\s]+)\\s+(?<PROTOCOL>[^\\s]+)\\s+(?<SRC_IP>[^\\s]+)\\s+(?<DST_IP>[^\\s]+)\\s+(?<SRC_PORT>[^\\s]+)\\s+(?<DST_PORT>[^\\s]+)\\s+(?<SIZE>[^\\s]+)\\s+(?<TCP_FLAGS>[^\\s]+)\\s+(?<TCP>[^\\s]+)\\s+(?<SYN>[^\\s]+)\\s+(?<TCPWIN>[^\\s]+)\\s+(?<ICMPTYPE>[^\\s]+)\\s+(?<ICMPCODE>[^\\s]+)\\s+(?<INFO>[^\\s]+)\\s+(?<PATH>[^\\s]+)$", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PATH"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"INFO"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
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
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TCP_FLAGS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TCP"},new[] {"SYN"},new[] {"ICMPTYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"TCPWIN"},new[] {"ICMPCODE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DST_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SIZE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var dtStr = fieldvalues[0] + " " + fieldvalues[1];
            if (DateTime.TryParseExact(dtStr.Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
