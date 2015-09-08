using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.KerioMailUnifiedRecorder
{
    public class KerioMailUnifiedRecorder:SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("((\\[(?<Datetime>[^]]+)\\]\\s+([^:]+\\:\\s+)?)|((?:\\s*([^:,]*)\\s*):\\s*((<?(.*?)>?(?=\\,[^:,]*\\s*\\:))|(.*$))))", RegexOptions.Compiled);

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

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }


        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"Datetime"} },
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo=Convert2Date
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Service"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Queue-ID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"From"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                       new DataMapping
                     {
                         Original = new[] {new[] {"To"}},
                         MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                     }, 
                      new DataMapping
                    {
                        Original = new[] {new[] {"Size"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    
                         new DataMapping
                    {
                        Original = new[] {new[] {"Sender-Host"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    
                          new DataMapping
                    {
                        Original = new[] {new[] {"SSL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    }
                }
            };
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
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
            var datetime = false;
            while (match.Success)
            {
                if (!datetime) context.FieldBuffer[context.SourceHeaderInfo["Datetime"]] = match.Groups["Datetime"].Value;
                datetime = true;
                if (context.SourceHeaderInfo.ContainsKey(match.Groups[5].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups[5].Value]] = match.Groups[8].Value;
                match = match.NextMatch();
            }
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
