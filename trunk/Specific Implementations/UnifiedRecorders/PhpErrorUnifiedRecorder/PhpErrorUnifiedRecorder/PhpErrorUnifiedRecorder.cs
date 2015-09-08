using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PhpErrorUnifiedRecorder
{
    public class PhpErrorUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"^\[(?<DateTime>[^\]]+)\]\s*(?<EventCategory>[^\:]+)\:\s*((.+\((?<CustomStr10>[^\)]+).*?:)|())(?<CustomStr7>[^:]+)\:\s+[^\s]+\s[^\s]+\s+((?<CustomStr8>.+\:[^\s]+)|().+)(\s|())((?<CustomStr9>.+)|())$", RegexOptions.Compiled);

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

        protected DataMappingInfo CreateMappingEn()
        {

            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DateTime"} },
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                       
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EventCategory"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                     
                    },
                    
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr7"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr8"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8"),
                      //  MethodInfo =Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr9"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr10"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }
                }
            };
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
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
    }
}
