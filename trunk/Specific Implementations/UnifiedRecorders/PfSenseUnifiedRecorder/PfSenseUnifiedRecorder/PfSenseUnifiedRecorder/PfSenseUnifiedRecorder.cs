using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PfSenseUnifiedRecorder
{
    public class PfSenseUnifiedRecorder:SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("(?<LOG_DATE>[0-9\\-]+\\t[0-9\\:]+)\\t(?<SEVERITY>.[^\\s]+)\\s*(?<HOST>[0-9\\.]+)\\s*(?<CREATION_DATE>[a-zA-Z]*\\s*[0-9]*\\s*[0-9\\:]+)\\s*.[^\\:]:\\s*(?<LOG_ID>[0-9]+)\\s*rule\\s*(?<RULE_ID>.[^\\:]+):\\s*(?<EVENT>[a-zA-Z\\-_\\\\\\/\\.]*)\\s*\\s*(?<DIRECTION>[a-zA-Z\\-_\\\\\\/\\.]*)\\s*.[^\\s]+\\s*(?<INTERFACE>.[^\\:]+):\\s*\\(\\s*((tos\\s*(?<TOS>.[^,]+))|()).[^a-zA-Z]+((ttl\\s*(?<TTL>.[^,]+))|()).[^a-zA-Z]+((id\\s*(?<ID>.[^,]+))|()).[^a-zA-Z]+((offset\\s*(?<OFFSET>.[^,]+))|()).[^a-zA-Z]+((flags\\s*(?<FLAGS>.[^,]+))|()).[^a-zA-Z]+((proto\\s*(?<PROTO>.[^,]+))|()).[^a-zA-Z]+((length\\s*(?<LENGTH>.[^\\)]+))|())\\)\\s*(?<SRC_IP>([0-9]*\\.){4})(?<SRC_PORT>[0-9]*)\\s*.\\s*(?<DST_IP>([0-9]*\\.){4})(?<DST_PORT>[0-9]*)(?<DETAIL>.[^\\n]+)", RegexOptions.Compiled);
        protected string DateFormat = "MM-yyyy-dd HH:mm:ss";

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"CREATION_DATE"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TOS"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DST_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TTL"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ID"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"OFFSET"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr7")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"FLAGS"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr8")
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"PROTO"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr9")
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"LENGTH"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr10")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof(RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            if (DateTime.TryParseExact(fieldvalues[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
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

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null)
            { return base.OnBeforeProcessRecordInput(context); }
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;

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
    }
}
