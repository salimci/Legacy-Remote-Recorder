using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.ZimbraUnifiedRecorder
{
    public class ZimbraUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"^(?<DATE_TIME>[0-9-]*\s*[0-9:,]*)\s*(?<SEVERITY>[a-zA-Z][^\s]*)\s*\[(?<EVENT_CATEGORY>[^:]+):(?<SERVICE_NAME>.[^\]]*)\]\s*\[name=(?<NAME>[^;]*);\s*ip=(?<IP>[0-9\.][^;]*);\s*ua=(?<UA>.[^;]*);\]\s*security-cmd=(?<SECURITY_CMD>[^;]*);\s*account=(?<ACCOUNT>[^;]*);\s*error=(?<ERROR>[^\;]*);$", RegexOptions.Compiled);

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new ZimbraUnifiedRecorderContext(this);
        }
        
        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        private DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"ACCOUNT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SECURITY_CMD"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVICE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"UA"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVICE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"TRANSFER_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DESCRIPTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
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

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (context.SourceHeaderInfo.ContainsKey(key))
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                context.FieldBuffer[context.SourceHeaderInfo["DESCRIPTION"]] = source;
            }
            return NextInstruction.Return;
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
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }
    }
}
