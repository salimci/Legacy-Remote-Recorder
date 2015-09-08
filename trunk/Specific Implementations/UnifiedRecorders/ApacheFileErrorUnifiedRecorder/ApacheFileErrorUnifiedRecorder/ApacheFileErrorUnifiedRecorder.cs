using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

/*
*Serkan Bey'in bilgisi dahilinde geliştirildi.
 */

namespace Natek.Recorders.Remote.Unified.ApacheFileErrorUnifiedRecorder
{
    public class ApacheFileErrorUnifiedRecorder : FileLineRecorder
    {
        
        protected static readonly Regex RegSplitForValue = new Regex("(^\\[(?<DATE_TIME>[^\\]]+)\\]\\s+\\[(?<SEVERITY_0>[^\\]]+)\\]\\s+\\[(?<EVENTCATEGORY>[^\\s]+)\\s+(?<SOURCE_IP>[^\\]]+)\\]\\s+(((?<EVENT_3>[^:]+):\\s+(?<MESSAGE_2>.*?)(?<EVENT_2>\\/.+\\/)(?<ERROR_ADDRESS>[^\\s]+)\\son\\sline(?<LINE_NUMBER_1>[^,]+),\\s+referer:\\s+(?<REFERER_0>[^\\/]+\\/\\/[^\\/]+)(?<REFERER_NAME_0>[^\\s]+))|((?<EVENT_1>[^:]+):\\s+(?<ERROR_ADDRESS_2>[^,]+),\\s+referer:\\s+(?<REFERER_1>[^\\/]+\\/\\/[^\\/]+)(?<REFERER_NAME_1>[^\\s]+))|(?<EVENT_0>[^:]+):\\s+((((?<MESSAGE_1>[^:]+:[^\\']+)\\'(?<USER>.+)\\'+.*?\\sin\\s)|(?<MESSAGE_0>[^\\/]+))|())(?<ERROR_ADDRESS_0>.+)\\/(?<CUSTOMSTR4>[^\\s]+)(((\\s+on\\sline\\s+(?<LINE_NUMBER>[^\\s]+)))|())|((?<CUSTOMSTR9>[^\\s]+)\\s+\\'(?<ERROR_ADDRESS_1>.+)(?<ERROR_MESSAGE>\\..+)))|((?<DATE_TIME_1>\\S+\\s+\\S+)\\s+\\[(?<SEVERITY_1>[^]]+)\\]\\s+(?<LINE_NUMBER_2>[^#]+)\\#[^*]+\\*(?<CUSTOM_INT_2>\\S+)(?:((?<CUSTOM_STR_9_0>[^:]+)\\:.*?(&total[^&]+\\&[^&]+)\\&.*?id0=(?<ID_0>[^&]+)\\&.*?id1=(?<ID_1>[^&]+)\\&[^,]+\\,)|(?<CUSTOM_STR_9_1>[^,]+\\,.*?))\\s*client:\\s+(?<CLIENT>[^,]+)\\,[^:]+\\:\\s*(?<SERVER>([^,]+)|(\\S+$))(\\,[^\"]+\\\"((?<METHOD>\\S+)\\s+(?<REQUEST>\\S+)\\s+(?<REQUEST_TYPE>[^\"]+))\\\"(\\,\\s*upstream:\\s*\\\"(?<UPSTREAM>[^\"]+)\\\")?(\\,\\s*host:\\s*\\\"(?<HOST>[^\"]+)\\\")?(\\,\\s*referrer:\\s*\\\"(?<REFERRER_2>[^\"]+)\\\")?)?))", RegexOptions.Compiled);
        
        protected string DateFormat = "ddd MMM dd HH:mm:ss yyyy";
       
        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as ApacheFileErrorUnifiedRecorder;
            if (DateTime.TryParseExact(values[0], "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(values[0], "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
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
                        Original = new[] {new[] {"DATE_TIME_0"},new[] {"DATE_TIME_1"}},
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
                        Original = new[] {new[] {"SEVERITY_0"},new []{"SEVERITY_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENTCATEGORY"},new []{"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"), 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"},new []{"CLIENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                       new DataMapping
                    {
                        Original = new[] {new []{"SERVER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR4"}, new []{"ERROR_MESSAGE"},new []{"REQUEST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REQUEST_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR_ADDRESS_0"},new []{"HOST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"REFERER_0"}, new []{"REFERER_1"},new []{"REFERRER_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"REFERER_NAME_0"}, new []{"REFERER_NAME_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_0"}, new []{"EVENT_1"}, new []{"CUSTOMSTR9"},new []{"CUSTOM_STR_9_0"},new []{"CUSTOM_STR_9_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE_0"}, new[] {"MESSAGE_1"}, new[] {"MESSAGE_2"},new []{"UPSTREAM"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LINE_NUMBER"}, new []{"LINE_NUMBER_1"}, new []{"LINE_NUMBER_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new []{"CUSTOM_INT_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                      new DataMapping
                    {
                        Original = new[] {new []{"ID_0"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                      new DataMapping
                    {
                        Original = new[] {new []{"ID_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
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

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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

        protected override Dictionary<string, int> MimicMappingInfo(IEnumerable<DataMapping> dataMapping)
        {
            var info = new Dictionary<string, int>();
            var index = 0;
            foreach (var mapping in dataMapping)
            {
                if (mapping.Original.Length > 1)
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index;
                    }
                    index++;
                }
                else
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index++;
                    }
                }
            }
            return info;
        }
    }
}
