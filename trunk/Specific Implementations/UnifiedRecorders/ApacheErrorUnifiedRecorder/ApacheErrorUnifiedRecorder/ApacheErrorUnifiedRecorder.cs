using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.ApacheErrorUnifiedRecorder
{
    public class ApacheErrorUnifiedRecorder : TerminalRecorder
    {

        protected static readonly Regex RegSplitForValue = new Regex("^\\[(?<DATE_TIME>[^\\]]+)\\]\\s+\\[(?<SEVERITY>[^\\]]+)\\]\\s+\\[(?<EVENTCATEGORY>[^\\s]+)\\s+(?<SOURCE_IP>[^\\]]+)\\]\\s+(((?<EVENT_3>[^:]+):\\s+(?<MESSAGE_2>.*?)(?<EVENT_2>\\/.+\\/)(?<ERROR_ADDRESS>[^\\s]+)\\son\\sline(?<LINE_NUMBER_1>[^,]+),\\s+referer:\\s+(?<REFERER_0>[^\\/]+\\/\\/[^\\/]+)(?<REFERER_NAME_0>[^\\s]+))|((?<EVENT_1>[^:]+):\\s+(?<ERROR_ADDRESS_2>[^,]+),\\s+referer:\\s+(?<REFERER_1>[^\\/]+\\/\\/[^\\/]+)(?<REFERER_NAME_1>[^\\s]+))|(?<EVENT_0>[^:]+):\\s+((((?<MESSAGE_1>[^:]+:[^\\']+)\\'(?<USER>.+)\\'+.*?\\sin\\s)|(?<MESSAGE_0>[^\\/]+))|())(?<ERROR_ADDRESS_0>.+)\\/(?<CUSTOMSTR4>[^\\s]+)(((\\s+on\\sline\\s+(?<LINE_NUMBER>[^\\s]+)))|())|((?<CUSTOMSTR9>[^\\s]+)\\s+\\'(?<ERROR_ADDRESS_1>.+)(?<ERROR_MESSAGE>\\..+)))", RegexOptions.Compiled);
        protected string DateFormat = "ddd MMM dd HH:mm:ss yyyy";

        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);
            switch (keyword)
            {
                case "DF":
                    if (!string.IsNullOrEmpty(value))
                        DateFormat = value;
                    touchCount++;
                    break;
            }
            return true;
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
                        Original = new[] {new[] {"USER"}},
                        MappedField = typeof (RecWrapper).GetProperty("UsersId"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENTCATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR4"}, new []{"ERROR_MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR_ADDRESS_0"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REFERER_0"}, new []{"REFERER_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"REFERER_NAME_0"}, new []{"REFERER_NAME_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_0"}, new []{"EVENT_1"}, new []{"CUSTOMSTR9"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE_0"}, new[] {"MESSAGE_1"}, new[] {"MESSAGE_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LINE_NUMBER"}, new []{"LINE_NUMBER_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new ApacheErrorUnifiedContext(this);
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
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
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
