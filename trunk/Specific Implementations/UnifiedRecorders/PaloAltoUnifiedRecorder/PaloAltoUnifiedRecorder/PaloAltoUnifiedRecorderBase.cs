using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PaloAltoUnified
{
    public class PaloAltoUnifiedRecorderBase : FileLineRecorder
    {
        public Regex RegSplitForValue = new Regex("^(?<FUTURE_USE_1>((\"[^\"]*\")?[^,]*?),(?<RECEIVE_TIME>(\"[^\"]*\")?[^,]*?),(?<SERIAL_NUMBER>(\"[^\"]*\")?[^,]*?),(?<LOG_TYPE>(?<TRAFFIC>traff.c)|(?<THREAT>threat)|(?<CONFIG>config)|(?<SYSTEM>system)|(?<HIP>hip-match)|(?<UNK>(\"[^\"]*\")?[^,]*?)),(?<SUB_TYPE>(\"[^\"]*\")?[^,]*?),(?<FUTURE_USE_2>(\"[^\"]*\")?[^,]*?),((?(TRAFFIC)(?<_TRAFFIC_GENERATE_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_RULE_NAME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_USER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_APPLICATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_ZONE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_ZONE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_INGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_EGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_LOG_FORWARDING_PROFILE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SESSION_ID>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FLAGS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PROTOCOL>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ACTION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES_SENT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES_RECEIVED>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_START_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ELAPSED_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_CATEGORY>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ACTION_FLAGS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_LOCATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_LOCATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS_SENT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS_RECEIVED>(\"[^\"]*\")?[^,]*)(?<TRAFFIC_UNK>,.*)?$|((?(THREAT)(?<_THREAT_GENERATE_TIME>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_RULE_NAME>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_USER>(\"[^\"]*\")?[^,]*),(?<_THREAT_APPLICATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_ZONE>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_ZONE>(\"[^\"]*\")?[^,]*),(?<_THREAT_INGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_THREAT_EGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_THREAT_LOG_FORWARDING_PROFILE>(\"[^\"]*\")?[^,]*),(?<_THREAT_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_THREAT_SESSION_ID>(\"[^\"]*\")?[^,]*),(?<_THREAT_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_FLAGS>(\"[^\"]*\")?[^,]*),(?<_THREAT_PROTOCOL>(\"[^\"]*\")?[^,]*),(?<_THREAT_ACTION>(\"[^\"]*\")?[^,]*),(?<_THREAT_MISCELLANEOUS>(\"[^\"]*\")?[^,]*),(?<_THREAT_THREAT_ID>(\"[^\"]*\")?[^,]*),(?<_THREAT_CATEGORY>(\"[^\"]*\")?[^,]*),(?<_THREAT_SEVERITY>(\"[^\"]*\")?[^,]*),(?<_THREAT_DIRECTION>(\"[^\"]*\")?[^,]*),(?<_THREAT_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_THREAT_ACTION_FLAGS>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_LOCATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_LOCATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_THREAT_CONTENT_TYPE>(\"[^\"]*\")?[^,]*)(?<THREAT_UNK>,.*)?$|((?(HIP)(?<_HIP_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_HIP_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_HIP_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_HIP_MACHINE_NAME>(\"[^\"]*\")?[^,]*),(?<_HIP_SOURCE_ADDRESS>(\"[^\"]*\")?[^,]*),(?<_HIP_HIP>(\"[^\"]*\")?[^,]*),(?<_HIP_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_HIP_HIP_TYPE>(\"[^\"]*\")?[^,]*),(?<_HIP_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_HIP_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_HIP_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_HIP_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<HIP_UNK>,.*)?$|((?(CONFIG)(?<_CONFIG_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_CONFIG_HOST>(\"[^\"]*\")?[^,]*),(?<_CONFIG_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_CONFIG_COMMAND>(\"[^\"]*\")?[^,]*),(?<_CONFIG_ADMIN>(\"[^\"]*\")?[^,]*),(?<_CONFIG_CLIENT>(\"[^\"]*\")?[^,]*),(?<_CONFIG_RESULT>(\"[^\"]*\")?[^,]*),(?<_CONFIG_CONFIGURATION_PATH>(\"[^\"]*\")?[^,]*),(?<_CONFIG_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_CONFIG_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<CONFIG_UNK>,.*)?$|((?(SYSTEM)(?<_SYSTEM_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_EVENT_ID>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_OBJECT>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_MODULE>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_SEVERITY>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_DESCRIPTION>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<SYSTEM_UNK>,.*)?$)))))))))))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #region Mapping
        public static DataMappingInfo CreateMappingEn()
        {
            /*
                 * 0: [Domain]
1: [Receive Time]
2: [Serial #]
3: [Type]
4: [Threat/Content Type]
5: [Config Version]
6: [Generate Time]
7: [Source address]
8: [Destination address]
9: [NAT Source IP]
10: [NAT Destination IP]
11: [Rule]
12: [Source User]
13: [Destination User]
14: [Application]
15: [Virtual System]
16: [Source Zone]
17: [Destination Zone]
18: [Inbound Interface]
19: [Outbound Interface]
20: [Log Action]
21: [Time Logged]
22: [Session ID]
23: [Repeat Count]
24: [Source Port]
25: [Destination Port]
26: [NAT Source Port]
27: [NAT Destination Port]
28: [Flags]
29: [IP Protocol]
30: [Action]
31: [Bytes]
32: [Bytes Sent]
33: [Bytes Received]
34: [Packets]
35: [Start Time]
36: [Elapsed Time (sec)]
37: [Category]
38: [Padding]
39: [seqno]
40: [actionflags]
41: [Source Country]
42: [Destination Country]
43: [cpadding]
44: [pkts_sent]
45: [pkts_received]
                 */
            return new DataMappingInfo
            {
                Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new [] {new [] {"DOMAIN"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"REPEAT_COUNT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"SOURCE_PORT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"DESTINATION_PORT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"NAT_SOURCE_PORT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"NAT_DESTINATION_PORT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                                    MethodInfo=RecorderBase.Convert2Int64
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"SESSION_ID"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                                    MethodInfo=RecorderBase.Convert2Int64
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"BYTES_SENT"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                                    MethodInfo=RecorderBase.Convert2Int64
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"BYTES_RECEIVED"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
                                    MethodInfo=RecorderBase.Convert2Int64
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"ELAPSED_TIME"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt10"),
                                    MethodInfo=RecorderBase.Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"LOG_TYPE"}},
                                    MappedField = typeof (RecWrapper).GetProperty("SourceName")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"MISCELLANEOUS"}},
                                    MethodInfo=ParseUri,
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"SOURCE_IP"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"DESTINATION_IP"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"NAT_SOURCE_IP"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"NAT_DESTINATION_IP"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"PROTOCOL"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"RULE_NAME"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"APPLICATION"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"RECEIVE_TIME"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"SOURCE_USER"}},
                                    MappedField = typeof (RecWrapper).GetProperty("UserName")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"ACTION"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventType")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"CATEGORY"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"__full_text"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                        }
            };
        }
        private static object ParseUri(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            Uri uri;
            try
            {
                var h = HttpHelper.UrlDecode(fieldValues[0].StartsWith("\"") ? fieldValues[0].Substring(1, fieldValues[0].Length - 2) : fieldValues[0]);
                if (!Uri.TryCreate(h, UriKind.Absolute, out uri))
                {
                    if (!Uri.TryCreate("http://" + h, UriKind.Absolute, out uri))
                        return fieldValues[0];
                }
                rec.CustomStr2 = uri.PathAndQuery;
                return uri.Host;
            }
            catch
            {
                return fieldValues[0];
            }
        }
        #endregion

        public virtual NextInstruction OnFieldMatchPublic(RecorderContext context, string source, ref Match match)
        {
            return OnFieldMatch(context, source, ref match);
        }


        public Regex RegReplaceField = new Regex("^_+[^_]+_+", RegexOptions.Compiled);

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp) || !groupCollection[key].Success) continue;
                var field = RegReplaceField.Replace(key, m => "");
                if (context.SourceHeaderInfo.ContainsKey(field))
                    context.FieldBuffer[context.SourceHeaderInfo[field]] = groupCollection[key].Value;
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

        public DataMappingInfo[] CreateMappingInfosPublic()
        {
            return CreateMappingInfos();
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

        public virtual string GetHeaderTextPublic(RecorderContext context)
        {
            return GetHeaderText(context);
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        public CanAddMatchDelegate CanAddMatchFieldPublic
        {
            get { return CanAddMatchField; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        public virtual CanAddMatchDelegate CanAddMatchHeaderPublic
        {
            get { return CanAddMatchHeader; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }
    }
}
