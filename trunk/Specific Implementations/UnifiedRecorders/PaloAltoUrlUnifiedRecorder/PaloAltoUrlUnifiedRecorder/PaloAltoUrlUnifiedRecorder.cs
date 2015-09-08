using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PaloAltoUrlUnifiedRecorder
{
    public class PaloAltoUrlUnifiedRecorder : FileLineRecorder
    {
        protected static Regex RegSplitForValue = new Regex("^(?<DOMAIN>[^,]*),(?<RECEIVE_TIME>[^,]*),(?<SERIAL>[^,]*),(?<TYPE>[^,]*),(?<CONTENT_TYPE>[^,]*),(?<CONFIG_VERSION>[^,]*),(?<GENERATE_TIME>[^,]*),(?<SOURCE_ADDRESS>[^,]*),(?<DESTINATION_ADDRESS>[^,]*),(?<NAT_SOURCE_IP>[^,]*),(?<NAT_DESTINATION_IP>[^,]*),(?<RULE>[^,]*),(?<SOURCE_USER>[^,]*),(?<DESTINATION_USER>[^,]*),(?<APPLICATION>[^,]*),(?<VIRTUAL_SYSTEM>[^,]*),(?<SOURCE_ZONE>[^,]*),(?<DESTINATION_ZONE>[^,]*),(?<INBOUND_INTERFACE>[^,]*),(?<OUTBOUND_INTERFACE>[^,]*),(?<LOG_ACTION>[^,]*),(?<TIME_LOGGED>[^,]*),(?<SESSION_ID>[^,]*),(?<REPEAT_COUNT>[^,]*),(?<SOURCE_PORT>[^,]*),(?<DESTINATION_PORT>[^,]*),(?<NAT_SOURCE_PORT>[^,]*),(?<NAT_DESTINATION_PORT>[^,]*),(?<FLAGS>[^,]*),(?<IP_PROTOCOL>[^,]*),(?<ACTION>[^,]*),(((?<BYTES>[^,]*),(?<BYTES_RCV>[^,]*),(?<BYTES_SEND>[^,]*),(?<PACKETS>[^,]*),(?<START_TIME>[^,]*),(?<ELAPSED>[^,]*),(?<URL_CATEGORY>[^,]*),(?<PADDING>[^\\,]*))|((?<URL>[^,]*),(?<CONTENT_NAME>[^,]*),(?<CATEGORY>[^,]*),(?<SEVERITY>[^,]*),(?<DIRECTION>[^\\,]*)))$", RegexOptions.Compiled);

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);
            switch (keyword)
            {
                case "Pattern":
                    RegSplitForValue = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
                        Original = new[] {new[] {"RECEIVE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"INBOUND_INTERFACE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"OUTBOUND_INTERFACE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"DESTINATION_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"NAT_SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"NAT_DESTINATION_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"IP_PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"CONTENT_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"RULE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"APPLICATION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_USER"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },

                      new DataMapping
                    {
                        Original = new[] {new[] {"CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"DOMAIN"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },


                     new DataMapping
                    {
                        Original = new[] {new[] {"REPEAT_COUNT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"DESTINATION_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"NAT_SOURCE_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"NAT_DESTINATION_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },


                    new DataMapping
                    {
                        Original = new[] {new[] {"SESSION_ID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
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
                if (int.TryParse(key, out tmp) || !groupCollection[key].Success) continue;
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

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;

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
    }
}
