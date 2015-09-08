using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.SambaUnifiedRecorder
{
    public class SambaUnifiedRecorder:FileLineRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("\\[(?<DATE_TIME>[^,]+)\\,\\s+\\S+\\s*(?<EVENT>[^\\.]+)\\.\\D+(?<NUM_OPEN>\\d+)\\((?<OPERATION>[^\\)]+)\\)\\D+(?<CLIENT_IP>\\S+)\\s*\\S+\\s*(?<RESULT>[^\\(]+)(\\(\\D+(?<USER_ID>\\d+)\\D+(?<GID>\\d+)\\D+(?<PID>\\d+)\\))?", RegexOptions.Compiled);
        private  readonly Regex _state1 = new Regex("^\\[", RegexOptions.Compiled);
        private  readonly Regex _state2 = new Regex("^\\s*_", RegexOptions.Compiled);
        private readonly StringBuilder _lastBuffer = new StringBuilder();
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
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"OPERATION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NUM_OPEN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },new DataMapping
                    {
                        Original = new[] {new[] {"USER_ID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"PID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"RESULT"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
       
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
            return NextInstruction.Return;
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
      
        protected override NextInstruction OnProcessInputTextRecord(RecorderContext context, string[] fields, ref Exception error)
        {
            var lineRecord = context.InputRecord.ToString();
            var matchState1 = _state1.Match(lineRecord);
            if (matchState1.Success)
            {
                _lastBuffer.Append(lineRecord);
                return NextInstruction.Skip;
            }
            var matchState2 = _state2.Match(lineRecord);
            if (matchState2.Success)
            {
                _lastBuffer.Append(lineRecord);
                context.InputRecord.SetValue(_lastBuffer.ToString());
                _lastBuffer.Remove(0, _lastBuffer.Length);
                 return NextInstruction.Do;
            }
                _lastBuffer.Remove(0, _lastBuffer.Length);
                return NextInstruction.Skip;          
        }
        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }
     
    }
}
