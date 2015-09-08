using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.TMGUnifiedRecorder
{
    public class TmgUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<SERVERNAME>[^\\t]+)\\t+(?<DATE>[^\\t]+)\\t+(?<TIME>[^\\t]+)\\t+(?<PROTOCOL>[^\\t]+)\\t+(?<SOURCE_IP>[^:]+):(?<SOURCE_PORT>[^\\t]+)\\t+(?<DESTINATION_IP>[^:]+):(?<DESTINATION_PORT>[^\\t]+)\\t+(?<ORIGINAL_CLIENT_IP>[^\\t]+)\\t+(?<SOURCE_NETWORK>[^\\t]+)\\t+(?<DESTINATION_NETWORK>[^\\t]+)\\t+(?<ACTION>[^\\t]+)\\t(?<RESULT_CODE>[^\\t]+)\\t+(?<RULE>[^\\t]+)\\t+(?<APPLICATION_PROTOCOL>[^\\t]+)\\t+(?<BIDIRECTIONAL>[^\\t]+)\\t+(?<BYTES_SENT>[^\\t]+)\\t+(?<BYTES_RECEIVED>[^\\t]+)\\t+(?<CONNECTION_TIME>[^\\t]+)\\t+(?<DESTINATION_NAME>[^\\t]+)\\t+(?<CLIENT_USERNAME>[^\\t]+)\\t+(?<CLIENT_AGENT>[^\\t]+)\\t+(?<SESSION_ID>[^\\t]+)\\t+(?<CONNECTION_ID>[^\\t]+)\\t+(?<INTERFACE>[^\\t]+)\\t+(?<IPHEADER>[^\\t]+)\\t+(?<PAYLOAD>[^\\t]+)\\t+(?<GMT_TIME>[^\\t]+)\\t+(?<IPS_SCANRESULT>[^\\t]+)\\t+(?<IPS_SIGNATURE>[^\\t]+)\\t+(?<NAT_ADDRESS>[^(\\t|\\n)]+)$", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {
                        Original = new[] {new []{"DATE"}, new []{"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_NETWORK"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DESTINATION_NETWORK"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DESTINATION_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ORIGINAL_CLIENT_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"APPLICATION_PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RULE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
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
                        Original = new[] {new[] {"BIDIRECTIONAL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"BYTES_SENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"BYTES_RECEIVED"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CONNECTION_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"ORIGINAL_CLIENT_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var dtStr = fieldvalues[0] + " " + fieldvalues[1];
            return DateTime.TryParseExact(dtStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                try
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    int fieldBufferKey;
                    if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
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

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new TMGUnifiedRecorderContext(this);
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
        }
    }
}
