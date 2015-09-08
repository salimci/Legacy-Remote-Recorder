using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.VeriBranchUnifiedRecorder
{
    public class VeriBranchUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"^(?<DATE_TIME>[^,]+),([^\|]+\|){2}(?<EVENT>[^\|]+)\|CID:(?<CID>[^\|]*)\|UID:(?<UID>[^\|]+)\|TxnID:(?<TXNID>[^\|]+)\|UniqKey:(?<UNIQKEY>[^\|]+)\|((Ex:ExNo=(?<EXNO>[^\s]+)\s+)|(ExCode:(?<EXCODE>[^\|]+)\|))((ExMsg=)|(Ex:))(?<EXMSG>[^\|]*)\|Url:(?<URL>[^\s]+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private StringBuilder _lastBuffer = new StringBuilder();
        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EXMSG"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TXNID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"UNIQKEY"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EXCODE"}, new []{"EXNO"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"UID"}},
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

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new VeriBranchUnifiedRecorderContext(this, RegSplitForValue, DateFormat);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                if (string.IsNullOrEmpty(_lastBuffer.ToString()))
                {
                    _lastBuffer.Append(source.TrimEnd());
                    return NextInstruction.Abort;
                }

                var getDatetime = source.Split(',');
                if (getDatetime == null || getDatetime.Count() <= 0) return NextInstruction.Skip;
                DateTime date;
                var isdatetime = DateTime.TryParse(string.Format(getDatetime[0], DateFormat), out date);

                if (!string.IsNullOrEmpty(_lastBuffer.ToString()) && !isdatetime)
                {
                    _lastBuffer.Append("\n" + source.TrimEnd());
                    return NextInstruction.Abort;
                }

                if (string.IsNullOrEmpty(_lastBuffer.ToString()) || !isdatetime)
                {
                    return NextInstruction.Skip;
                }
                    
                var regmatch = RegSplitForValue.Match(_lastBuffer.ToString());

                if (!regmatch.Success) return NextInstruction.Skip;
                var groupCollection = regmatch.Groups;

                foreach (var key in RegSplitForValue.GetGroupNames())
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                    if (groupCollection[key].Value.Length > 0)
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                context.FieldBuffer[context.SourceHeaderInfo["Description"]] = _lastBuffer.ToString();
                _lastBuffer = new StringBuilder();
                _lastBuffer.Append(source.TrimEnd());

                return NextInstruction.Return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing veribranch record: " + e);
                return NextInstruction.Abort;
            }
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

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;

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
