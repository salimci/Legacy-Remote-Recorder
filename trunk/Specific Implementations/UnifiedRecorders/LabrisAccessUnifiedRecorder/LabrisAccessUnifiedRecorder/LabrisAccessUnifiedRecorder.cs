using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.LabrisAccessUnifiedRecorder
{
    public class LabrisAccessUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"(?<HOSTNAME>[^:]+):(?<PORT>\S+)\s+:\s+(?<PRIORITY>\S+)[^\]]+]:\s+(?<DATE_TIME>\S+)\s+(?<USER_COMPUTER>\S+)\s+(?<REGUESTING_IP>\S+)\s+(?<unkown_area>\S+)\s+(?<unkown_area_2>\S+)\s+(?<URL>.*?\/(?=\/).[^\/]+\/)(?<URI>\S+)\s+\*(?<ACTION>\S+)\*(?<REASON>\S+)\s+(?<METHOD>\S+)\t(?<SIZE>\S+)\t(?<WEIGHT>\S+)\t(?<CATEGORY>.*?)\t(?<FILTER_GROUP_NUMBER>.*?)\s+(?<HTTP_RETURN_CODE>.*?)\s+(?<MIME_TYPE>.*?)\t(?<CLIENT_NAME>.*?)\t(?<FILTER_GROUP_NAME>.*?)\t(?<CUSTOMINT8>.*?)\t(?<UNKOWN_NUMBER>.*?)\t(?<USER_AGENT>\S+)", RegexOptions.Compiled);

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

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }


        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "yyyy.M.dd-HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"} },
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo=Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_COMPUTER"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PRIORITY"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },

                     new DataMapping
                     {
                         Original = new[] {new[] {"HOSTNAME"}},
                         MappedField = typeof (RecWrapper).GetProperty("UserName")
                     }, 
                       new DataMapping
                     {
                         Original = new[] {new[] {"REGUESTING_IP"}},
                         MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                     }, 
                      new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                         new DataMapping
                    {
                        Original = new[] {new[] {"URI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    
                          new DataMapping
                    {
                        Original = new[] {new[] {"FILTER_GROUP_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"REASON"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"SIZE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"WEIGHT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"FILTER_GROUP_NUMBER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"HTTP_RETURN_CODE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"UNKOWN_NUMBER"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_AGENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }
                   
                }
            };
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            context.Record.Description = context.InputRecord.ToString();
            return base.OnBeforeSetData(context);
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
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
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
