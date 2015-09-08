using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.JuniperPendikUnifiedRecorder
{
    public class JuniperPendikUnifiedRecorder:SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("(^(?<DATE_TIME>\\S+\\s+\\S+)\\s+\\S+\\s+\\S+\\s+\\S+\\s+\\S+\\s+(?<INFO>\\S+\\s+\\S+)\\s-\\s(?<EVENT_TYPE>\\S+)\\s+)|(([^=\\s]+)\\=\\\"([^\"]+)\\\")", RegexOptions.Compiled);

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
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo =Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"policy-name"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"username"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"source-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"destination-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"nat-source-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"nat-destination-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"source-port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo=Convert2Int32
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"destination-port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo=Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"nat-source-port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo=Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"nat-destination-port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo=Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }
                }
            };
        }
        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
                if (int.TryParse(key, out tmp)) continue;
                if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                if (groupCollection[key].Value.Length > 0)
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            match = match.NextMatch();
            while (match.Success)
            {
                if (context.SourceHeaderInfo.ContainsKey(match.Groups[3].Value))
                    context.FieldBuffer[context.SourceHeaderInfo[match.Groups[3].Value]] = match.Groups[4].Value;
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
        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
        }
    }
}
