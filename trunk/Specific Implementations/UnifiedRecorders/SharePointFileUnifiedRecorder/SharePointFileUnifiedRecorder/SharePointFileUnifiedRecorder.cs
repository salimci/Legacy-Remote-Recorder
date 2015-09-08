using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.SharePointFileUnifiedRecorder
{
    public class SharePointFileUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("(?<TIMESTAMP>[^\\t]+\\s*[0-9:\\.]+)\\s*(?<PROCESS>[^\\t]+)\\s*(?<TID>[^\\t]+)\\s*(?<AREA>[^\\t]+)\\s*(?<CATEGORY>[^\\t]+)\\s*(?<EVENT_ID>[^\\t]+)\\s*(?<LEVEL>[^\\t]+)\\s*\\[(?<CORRELATION>[^\\]]+)\\]\\s*(?<MESSAGE>[^\\n]+)", RegexOptions.Compiled);

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
                        Original = new[] {new []{"TIMESTAMP"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_ID"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TID"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROCESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LEVEL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROCESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CORRELATION"}, new []{"MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"AREA"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            if (DateTime.TryParseExact(fieldvalues[0], "dd/MM/yyyy HH:mm:ss.SS", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
                context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
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
