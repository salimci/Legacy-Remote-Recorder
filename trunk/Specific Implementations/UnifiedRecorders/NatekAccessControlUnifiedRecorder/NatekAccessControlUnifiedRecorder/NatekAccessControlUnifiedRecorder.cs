using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;


namespace Natek.Recorders.Remote.Unified.NatekAccessControlUnifiedRecorder
{
    public class NatekAccessControlUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<TIME>[^|]+)\\|\\|(?<EVENT_TYPE>()|([^|]+))\\|\\|(?<EVENT>()|([^|]+))\\|\\|(?<EVENT_ROOT>()|([^|]+))\\|\\|(?<EXE_NAME>()|[^\\\\]+)(()|\\\\)(?<EXE_PATH>()|([^|]+))\\|\\|(?<DOMAIN>[^\\\\]+)(()|\\\\)(?<USER_NAME>()|([^|]+))\\|\\|(?<DEVICE>()|([^|]+))\\|\\|((?<PARTITION>()|([^\\\\]+))(()|\\\\)?(?<FILE_NAME>()|[^\\|]+))\\|\\|(?<HASH>([^|]+))$", RegexOptions.Compiled);

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
                        Original = new[] {new []{"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DOMAIN"}, new[] {"USER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PARTITION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"FILE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EXE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EXE_PATH"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PARTITION"}, new[] {"FILE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_ROOT"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    }
                }
            };
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
