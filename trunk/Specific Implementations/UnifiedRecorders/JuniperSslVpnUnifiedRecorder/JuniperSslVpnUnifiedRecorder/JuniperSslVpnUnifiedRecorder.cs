using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.JuniperSslVpnUnifiedRecorder
{
    public class JuniperSslVpnUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"^(?<HOST_NAME>[^:]+):(?<HOST_PORT>[^\s]+).+\s+[^\s]+\s(?<DATE_TIME>[0-9\-]+\s[0-9:]+).[^\[]+\[(?<HOST_IP>[0-9\.]+)\]\s(?<DEST_IP>[\w]+\([\w]*\)\[[\w]*\])\s*-[\s\*]*((?<NETWORK_CONNECT>.*?)\s+IP\s(?<NETWORK_CONNECT_IP>[0-9\.]+)(()|(?<CONNECTION_URL>[^(\|\n)]+))|((?<NETWORK_CONNECT_>.*?)count\s=\s(?<NETWORK_CONNECT_COUNT>[0-9]+))|(Closed\sconnection.+port\s(?<PORT>[0-9]+)\s*[\w]+\s*(?<DURATION>[0-9]+)[\s\w\,]+with\s(?<BYTES_READ>[0-9]+)[\w\s\(]+in\s(?<READ_CHUNKS>[0-9]+)[\s\w\)]+and\s(?<BYTES_WRITTEN>[0-9]+)[\s\w\(]+in\s(?<WRITTEN_CHUNKS>[0-9]+)\s\w+\))|(Logout\s+[^\s]+\s+(?<LOGOUT_IP>[^\s]+).+)|((?<TRANSPORT>.*?)\s+NCIP\s+(?<TRANSPORT_IP>[0-9\.]+))|(Key\sExchange\s[a-zA-Z]+\s(?<EXCHANGE_NUMBER>[0-9]+)[\sa-zA-Z]+(?<EXCANGE_IP>[0-9\.]+))|(Connected.+port\s+(?<CONNECTED_PORT>[0-9]+))|(Login\s*suc.+for\s*(?<LOGIN_USER>[^\s]+)\s*.+from\s*(?<LOGIN_IP>[0-9\.]+))|((?<ACTION>.*?)\s+for\s+(?<AUTH_USER>[\w\-\/\s]+)from\s(?<AUTH_IP>[0-9\.]+))|(?<DESCRIPTION>[^\n]))$", RegexOptions.Compiled);

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
                        MappedField = typeof (RecWrapper).GetProperty("DateTime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DEST_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"WHOLE_ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DESCRIPTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as JuniperSslVpnUnifiedRecorder;

            if (DateTime.TryParseExact(values[0], "yyyy-MM-dd H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone)
                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
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

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            var keys = new List<string> { "DATE_TIME", "HOST_IP", "HOST_PORT", "DEST_IP" };

            foreach (var key in keys)
            {
                if (context.SourceHeaderInfo.ContainsKey(key))
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }

            if (!string.IsNullOrEmpty(groupCollection["TRANSPORT_IP"].Value))
                context.FieldBuffer[context.SourceHeaderInfo["USER_IP"]] = groupCollection["TRANSPORT_IP"].Value;

            if (!string.IsNullOrEmpty(groupCollection["NETWORK_CONNECT_IP"].Value))
                context.FieldBuffer[context.SourceHeaderInfo["USER_IP"]] = groupCollection["NETWORK_CONNECT_IP"].Value;

            context.FieldBuffer[context.SourceHeaderInfo["SOURCE"]] = context.Recorder.RemoteHost;
            context.FieldBuffer[context.SourceHeaderInfo["WHOLE_ACTION"]] = match.Groups[1].Value;
            context.FieldBuffer[context.SourceHeaderInfo["DESCRIPTION"]] = source;

            
return NextInstruction.Return;
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
