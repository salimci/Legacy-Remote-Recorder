using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.WampServerUnifiedRecorder
{
    public class WampServerUnifiedRecorder:TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(?<SOURCE_IP>[^\\s]+)\\s+(?<IDENTD>[^\\s]+)\\s+(?<USERID>[^\\s]+)\\s+\\[(?<DATE_TIME>[^\\]]+)\\s*\\]\\s*\"(?<REQUEST>[^\\s]+)\\s+(?<REQUESTED_RESOURCE>[^\\s]+)\\s+(?<PROTOCOL>[^\"]+)\"\\s+(?<STATUS>[^\\s]+)\\s+(?<SIZE>[^\\s]+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy-MM-dd HH:mm:ss";

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new WampServerUnifiedContext(this);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

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

        protected DataMappingInfo CreateMappingEn()
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
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SIZE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"REQUESTED_RESOURCE"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"REQUEST"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
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

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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
    }
}
