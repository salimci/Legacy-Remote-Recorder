using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.WebsenseDlpUnifiedRecorder
{
    public class WebsenseDlpUnifiedRecorder : SyslogRecorderBase
    {
        protected readonly Regex RegSplitForAll = new Regex(@"^.*\|([\d]*)\|\s*act=\s*([\w]*)\s*duser=\s*([\w]*)\s*fname=\s*(.*)\s*-\s*([\w\S]*\s*[\w]*)\s*msg=\s*([\w\s]*)\s*suser=\s*(.*)\s*cat=\s*([\w\s]*)\s*sourceServiceName=([\w\s]*)", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"date-time"}},
                        MappedField = typeof (RecWrapper).GetProperty("DateTime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"act"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"suser"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"duser"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"sourceServiceName"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"cat"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"msg"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"fname"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dlpSyslog"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"fileSize"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Byte
                    }
                }
            };
        }

        protected object Convert2Byte(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            var rawValue = string.Empty;
            if (fieldvalues.Length >= 1)
                rawValue = fieldvalues[0];

            var fileSizeRegex = new Regex(@"([\w\S]*)\s*([\w]*)", RegexOptions.Compiled);

            var valueMatch = fileSizeRegex.Match(rawValue);

            if (valueMatch.Success)
            {
                var value = valueMatch.Groups[1].Value;
                var unit = valueMatch.Groups[2].Value;
                double result = double.Parse(value);
                switch (unit)
                {
                    case "KB" :
                        return (int) result*1024;
                    case "MB" :
                        return (int) result*1048576;
                    case "GB" :
                        return (int) result*1073741824;
                }
            }
            return 0;
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var recorder = data as WebsenseDlpUnifiedRecorder;

            if (DateTime.TryParseExact(fieldvalues[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
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

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] {CreateMappingEn()};
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
            if (match.Success)
            {
                context.FieldBuffer[1] = match.Groups[2].Value; //act => eventtype
                context.FieldBuffer[2] = match.Groups[7].Value; //suser => usersId
                context.FieldBuffer[3] = match.Groups[3].Value; //duser => customStr1
                context.FieldBuffer[4] = match.Groups[9].Value; //sourceServiceName => customStr2
                context.FieldBuffer[5] = match.Groups[8].Value; //cat => customStr3
                context.FieldBuffer[6] = match.Groups[6].Value; //msg => customStr4
                context.FieldBuffer[7] = match.Groups[4].Value; //fname => customStr5
                context.FieldBuffer[8] = match.Groups[1].Value; //dlpSyslog => customInt1
                context.FieldBuffer[9] = match.Groups[5].Value; //fileSize => customInt5
                return NextInstruction.Return;
            }
            return NextInstruction.Skip;
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
