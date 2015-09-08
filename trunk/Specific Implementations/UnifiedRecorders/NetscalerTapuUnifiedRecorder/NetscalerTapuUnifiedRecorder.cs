using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.NetscalerTapuUnifiedRecorder
{
    public class NetscalerTapuUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("(((?<SOURCE_NAME>[^:]+):(?<CUSTOMINT2>\\S+)\\s:\\s(?<EVENT_CATEGORY>\\S+)\\s+(?<DATE_TIME>\\S+)\\s(?<EVENT_TYPE>(.(?!\\s(?=:)))*.)\\s:\\s\\S+\\s+\\S+\\s(?<CUSTOMINT3>\\S+)\\s0\\s:\\s+\"(?<CUSTOMSTR1>[^:]+):-->)|(?:\\s+(?<x>[^:]+):\\s+(?<y>[^\\s\"]+)))", RegexOptions.Compiled);
       

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "dd/MM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
                        Original = new[] {new[] {"SOURCE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new [] {"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ISTEK YAPAN IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"VSERVER IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },
                   
                    new DataMapping
                    {
                        Original = new[] {new[] {"ISTEK YAPILAN DOMAIN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                        
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ISTEK YAPILAN URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7"),
                    },
                   
                     new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                        
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                   
                }
            };
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }

            match = match.NextMatch();

            while (match.Success)
            {
                if (context.SourceHeaderInfo.ContainsKey(match.Groups["x"].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups["x"].Value]] = match.Groups["y"].Value;
                match = match.NextMatch();
            }

            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
        }

       

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null) return base.OnBeforeProcessRecordInput(context);
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
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
