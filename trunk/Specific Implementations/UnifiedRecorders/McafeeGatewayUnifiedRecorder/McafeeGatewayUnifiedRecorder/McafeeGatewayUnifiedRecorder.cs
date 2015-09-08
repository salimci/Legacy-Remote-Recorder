using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using System.IO;

namespace Natek.Recorders.Remote.Unified.McafeeGatewayUnifiedRecorder
{
    public class McafeeGatewayUnifiedRecorder : SyslogRecorderBase
    {
        protected static Regex RegSplitForAll = new Regex(@"(?<CUSTOMSTR7>[^:]+)\:514 \: system.(?<SOURCE_NAME>[^\s]+)\s(?<Unnecessary1>[^\bw]+)(?<COMPUTERNAME>[^\s]+)\smwg: CEF:0\|McAfee\|Web Gateway\|7.4.2.4.0\|(?<Unnecessary2>[^\|]+)\|(?<CUSTOMSTR6>[^\|]+)(?<Unnecessary3>[^\=]+)\=(?<DATE_TIME>.+)cat\=Access\sLog\sdst\=(?<CUSTOMSTR4>[^\s]+)\sdhost\=(?<CUSTOMSTR8>[^\s]+)\ssuser=-\ssrc=(?<CUSTOMSTR3>[^\s]+)\srequestMethod=GET\srequest=(?<CUSTOMSTR9>[^((\?)|(\s))]+)(?<Unnecessary4>.+)requestClientApplication=(?<CUSTOMSTR10>.+)", RegexOptions.Compiled);

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            try
            {
                if (keyword == "Regex")
                {
                    RegSplitForAll = new Regex(value);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            return false;
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

        protected DataMappingInfo CreateMappingEn()
        {

            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                     
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"COMPUTERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo =Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo=Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo=Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo=Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo=Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo=Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT6"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo=Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT7"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo=Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT8"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo=Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT9"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
                        MethodInfo=Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMINT10"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt10"),    
                        MethodInfo=Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR6"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR7"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR8"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR9"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CUSTOMSTR10"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10"),    
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            DateTime dt;
            var recorder = data as McafeeGatewayUnifiedRecorder;
            var dtStr = fieldValues[0];
            if (DateTime.TryParseExact(dtStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) || DateTime.TryParseExact(dtStr, "MMM dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
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

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                try
                {
                    if (!int.TryParse(key, out tmp))
                    {
                        int fieldBufferKey;
                        if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                            context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                    }
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
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
