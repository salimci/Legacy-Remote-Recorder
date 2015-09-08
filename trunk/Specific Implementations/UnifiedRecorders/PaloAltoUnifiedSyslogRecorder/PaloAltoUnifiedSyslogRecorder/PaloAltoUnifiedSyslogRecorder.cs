using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PaloAltoUnified
{
    public class PaloAltoUnifiedSyslogRecorder : SyslogRecorderBase
    {
        public PaloAltoUnifiedRecorderBase baseRecorder;

        public PaloAltoUnifiedSyslogRecorder()
        {
            baseRecorder = new PaloAltoUnifiedRecorderBase();
            baseRecorder.RegSplitForValue = new Regex("^(?<SYSLOG_SENDER_IP>[^:]*):(?<SYSLOG_SENDER_PORT>\\S*)\\s*:\\s*(?<EVENTTYPE>\\S+)\\s*(?<FUTURE_USE_1>((\"[^\"]*\")?[^,]*?),(?<RECEIVE_TIME>(\"[^\"]*\")?[^,]*?),(?<SERIAL_NUMBER>(\"[^\"]*\")?[^,]*?),(?<LOG_TYPE>(?<TRAFFIC>traff.c)|(?<THREAT>threat)|(?<CONFIG>config)|(?<SYSTEM>system)|(?<HIP>hip-match)|(?<UNK>(\"[^\"]*\")?[^,]*?)),(?<SUB_TYPE>(\"[^\"]*\")?[^,]*?),(?<FUTURE_USE_2>(\"[^\"]*\")?[^,]*?),((?(TRAFFIC)(?<_TRAFFIC_GENERATE_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_RULE_NAME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_USER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_APPLICATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_ZONE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_ZONE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_INGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_EGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_LOG_FORWARDING_PROFILE>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SESSION_ID>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_NAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FLAGS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PROTOCOL>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ACTION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES_SENT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_BYTES_RECEIVED>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_START_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ELAPSED_TIME>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_CATEGORY>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_ACTION_FLAGS>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_SOURCE_LOCATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_DESTINATION_LOCATION>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS_SENT>(\"[^\"]*\")?[^,]*),(?<_TRAFFIC_PACKETS_RECEIVED>(\"[^\"]*\")?[^,]*)(?<TRAFFIC_UNK>,.*)?$|((?(THREAT)(?<_THREAT_GENERATE_TIME>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_SOURCE_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_DESTINATION_IP>(\"[^\"]*\")?[^,]*),(?<_THREAT_RULE_NAME>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_USER>(\"[^\"]*\")?[^,]*),(?<_THREAT_APPLICATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_ZONE>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_ZONE>(\"[^\"]*\")?[^,]*),(?<_THREAT_INGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_THREAT_EGRESS_INTERFACE>(\"[^\"]*\")?[^,]*),(?<_THREAT_LOG_FORWARDING_PROFILE>(\"[^\"]*\")?[^,]*),(?<_THREAT_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_THREAT_SESSION_ID>(\"[^\"]*\")?[^,]*),(?<_THREAT_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_SOURCE_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_NAT_DESTINATION_PORT>(\"[^\"]*\")?[^,]*),(?<_THREAT_FLAGS>(\"[^\"]*\")?[^,]*),(?<_THREAT_PROTOCOL>(\"[^\"]*\")?[^,]*),(?<_THREAT_ACTION>(\"[^\"]*\")?[^,]*),(?<_THREAT_MISCELLANEOUS>(\"[^\"]*\")?[^,]*),(?<_THREAT_THREAT_ID>(\"[^\"]*\")?[^,]*),(?<_THREAT_CATEGORY>(\"[^\"]*\")?[^,]*),(?<_THREAT_SEVERITY>(\"[^\"]*\")?[^,]*),(?<_THREAT_DIRECTION>(\"[^\"]*\")?[^,]*),(?<_THREAT_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_THREAT_ACTION_FLAGS>(\"[^\"]*\")?[^,]*),(?<_THREAT_SOURCE_LOCATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_DESTINATION_LOCATION>(\"[^\"]*\")?[^,]*),(?<_THREAT_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_THREAT_CONTENT_TYPE>(\"[^\"]*\")?[^,]*)(?<THREAT_UNK>,.*)?$|((?(HIP)(?<_HIP_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_HIP_SOURCE_USER>(\"[^\"]*\")?[^,]*),(?<_HIP_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_HIP_MACHINE_NAME>(\"[^\"]*\")?[^,]*),(?<_HIP_SOURCE_ADDRESS>(\"[^\"]*\")?[^,]*),(?<_HIP_HIP>(\"[^\"]*\")?[^,]*),(?<_HIP_REPEAT_COUNT>(\"[^\"]*\")?[^,]*),(?<_HIP_HIP_TYPE>(\"[^\"]*\")?[^,]*),(?<_HIP_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_HIP_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_HIP_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_HIP_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<HIP_UNK>,.*)?$|((?(CONFIG)(?<_CONFIG_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_CONFIG_HOST>(\"[^\"]*\")?[^,]*),(?<_CONFIG_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_CONFIG_COMMAND>(\"[^\"]*\")?[^,]*),(?<_CONFIG_ADMIN>(\"[^\"]*\")?[^,]*),(?<_CONFIG_CLIENT>(\"[^\"]*\")?[^,]*),(?<_CONFIG_RESULT>(\"[^\"]*\")?[^,]*),(?<_CONFIG_CONFIGURATION_PATH>(\"[^\"]*\")?[^,]*),(?<_CONFIG_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_CONFIG_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<CONFIG_UNK>,.*)?$|((?(SYSTEM)(?<_SYSTEM_FUTURE_USE_1>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_VIRTUAL_SYSTEM>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_EVENT_ID>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_OBJECT>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_FUTURE_USE_2>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_FUTURE_USE_3>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_MODULE>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_SEVERITY>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_DESCRIPTION>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_SEQUENCE_NUMBER>(\"[^\"]*\")?[^,]*),(?<_SYSTEM_ACTION_FLAGS>(\"[^\"]*\")?[^,]*)(?<SYSTEM_UNK>,.*)?$)))))))))))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);
            switch (keyword)
            {
                case "Pattern":
                    baseRecorder.RegSplitForValue = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    break;
            }
            return true;
        }


        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            var ins=baseRecorder.OnFieldMatchPublic(context, source, ref match);
            if (ins == NextInstruction.Return)
                context.FieldBuffer[context.SourceHeaderInfo["__full_text"]] = source;
            return ins;
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            return baseRecorder.GetHeaderInfo(context, ref error);
        }

        public override Regex CreateHeaderSeparator()
        {
            return baseRecorder.CreateHeaderSeparator();
        }

        public override Regex CreateFieldSeparator()
        {
            return baseRecorder.CreateFieldSeparator();
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return baseRecorder.CreateMappingInfosPublic();
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            return baseRecorder.InputTextType(context, ref error);
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return baseRecorder.GetHeaderTextPublic(context);
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return baseRecorder.CanAddMatchFieldPublic; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return baseRecorder.CanAddMatchHeaderPublic; }
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null) return base.OnBeforeProcessRecordInput(context);
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
        }
    }
}