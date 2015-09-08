using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.SymantecBrightmailUnifiedRecorder
{
    public class SymantecBrightmailUnifiedRecorder : SyslogRecorderBase
    {

        protected readonly Regex RegSplitForAll = new Regex(@"(([0-9]*\.[0-9]*\.[0-9]*\.[0-9]*):([0-9]*)\s*:\s*(\w*\S\w*)\s([a-zA-Z]{3}\s*[0-9]*\s*[0-9]*:[0-9]*:[0-9]*)\s*([\w*\s]*):\s*([0-9]*)\s*\|\s*([\w*\d*-]*)\s*\|\s*([\w]*)\s*\|\s*([\w*\.*\w*]*):([\d]*))|(([0-9]*\.[0-9]*\.[0-9]*\.[0-9]*):([0-9]*)\s*:\s*(\w*\S\w*)\s([a-zA-Z]{3}\s*[0-9]*\s*[0-9]*:[0-9]*:[0-9]*)\s*([\w*\s]*).*\s(ML-RECEIVED_RECIPIENT\w*)\s*:\s*Message ID\s*:\s*(\w*\S*),\s*Audit ID\s*:\s*(\w*\S*),\s*recipient\s*:\s*(\w*\S*))|(([0-9]*\.[0-9]*\.[0-9]*\.[0-9]*):([0-9]*)\s*:\s*(\w*\S\w*)\s([a-zA-Z]{3}\s*[0-9]*\s*[0-9]*:[0-9]*:[0-9]*)\s*([\w*\s]*).*\s(ML-RECEIVED)\s*:\s*Message ID\s*:\s*(\w*\S*),\s*Audit ID\s*(\w*\S*),\s*Received on\s*:\s*(\w*\S*)\s*,\s*from host\s*:\s*([\w*\.]*):([\d]*)\s*,\s*sender\s*:\s*(\w*\S*@\w*\S*)\s*,\s*Size\s*:\s*(\d*),.*)|(([0-9]*\.[0-9]*\.[0-9]*\.[0-9]*):([0-9]*)\s*:\s*(\w*\S\w*)\s([a-zA-Z]{3}\s*[0-9]*\s*[0-9]*:[0-9]*:[0-9]*)\s*([\w*\s]*):\s*([0-9]*)\s*\|\s*([\w*\d*-]*)\s*\|\s*([\w]*)\s*\|\s*([\w*\S*\w*].*))", RegexOptions.Compiled);
        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as SymantecBrightmailUnifiedRecorder;

            if (DateTime.TryParseExact(values[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone)
                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"date-time"}},//0
                        MappedField = typeof (RecWrapper).GetProperty("DateTime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Event"}},//1
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EventType"}},//2
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LocalIp"}},//3
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RemoteIp"}},//4
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SecDate"}},//5
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MailId"}},//6
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Number"}},//7
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Sender"}},//8
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Info"}},//9
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ExtInfo"}},//10
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RemotePort"}},//11
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LocalPort"}},//12
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AuditId"}},//13
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Size"}},//14
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},//15
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HostName"}},//16
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    }
                    
                }
            };
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
            if (match.Groups[1].Success)
            {
                context.FieldBuffer[5] = match.Groups[5].Value; //sec date
                context.FieldBuffer[7] = match.Groups[7].Value; //number
                context.FieldBuffer[1] = match.Groups[9].Value; //event
                context.FieldBuffer[2] = match.Groups[6].Value; //eventtype
                context.FieldBuffer[6] = match.Groups[8].Value; //mailId
                context.FieldBuffer[3] = match.Groups[2].Value; //localIp
                context.FieldBuffer[12] = match.Groups[3].Value; //localPort
                context.FieldBuffer[4] = match.Groups[10].Value; //info
                context.FieldBuffer[11] = match.Groups[11].Value; //Exctinfo
                context.FieldBuffer[16] = match.Groups[4].Value; //hostname
            }
            if (match.Groups[36].Success)
            {
                context.FieldBuffer[5] = match.Groups[40].Value; //sec date
                context.FieldBuffer[7] = match.Groups[42].Value; //number
                context.FieldBuffer[1] = match.Groups[44].Value; //event
                context.FieldBuffer[2] = match.Groups[41].Value; //eventtype
                context.FieldBuffer[6] = match.Groups[43].Value; //mailId
                context.FieldBuffer[3] = match.Groups[37].Value; //localIp
                context.FieldBuffer[12] = match.Groups[38].Value; //localPort
                context.FieldBuffer[9] = match.Groups[45].Value; //info
                context.FieldBuffer[16] = match.Groups[39].Value; //hostname
            }
            if (match.Groups[12].Success)
            {
                context.FieldBuffer[5] = match.Groups[16].Value; //sec date
                context.FieldBuffer[1] = match.Groups[18].Value; //event
                context.FieldBuffer[2] = match.Groups[17].Value; //eventtype
                context.FieldBuffer[13] = match.Groups[19].Value; //mailId
                context.FieldBuffer[6] = match.Groups[20].Value; //auditId
                context.FieldBuffer[3] = match.Groups[13].Value; //localIp
                context.FieldBuffer[12] = match.Groups[14].Value; //localPort
                context.FieldBuffer[9] = match.Groups[21].Value; //info
                context.FieldBuffer[16] = match.Groups[15].Value; //hostname
            }
            if (match.Groups[22].Success)
            {
                context.FieldBuffer[5] = match.Groups[26].Value; //sec date
                context.FieldBuffer[1] = match.Groups[28].Value; //event
                context.FieldBuffer[2] = match.Groups[27].Value; //eventtype
                context.FieldBuffer[13] = match.Groups[29].Value; //mailId
                context.FieldBuffer[6] = match.Groups[30].Value; //auditId
                context.FieldBuffer[3] = match.Groups[23].Value; //localIp
                context.FieldBuffer[12] = match.Groups[24].Value; //localPort
                context.FieldBuffer[9] = match.Groups[34].Value; //info
                context.FieldBuffer[10] = match.Groups[31].Value; //extrinfo
                context.FieldBuffer[4] = match.Groups[32].Value; //remoteIp
                context.FieldBuffer[11] = match.Groups[33].Value; //remotePort
                context.FieldBuffer[14] = match.Groups[35].Value; //remotePort
                context.FieldBuffer[16] = match.Groups[25].Value; //hostname
            }
            context.FieldBuffer[15] = source;
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
