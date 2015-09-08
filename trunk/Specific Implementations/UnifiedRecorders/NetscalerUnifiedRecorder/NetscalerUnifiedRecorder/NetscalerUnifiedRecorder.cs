using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Microsoft.Exchange
{
    public class NetscalerUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("([^ \"]*\"([^\"]*)\"[^ ]*|([^ ]*))[ ]*", RegexOptions.Compiled);

        #region Converter
        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as NetscalerUnifiedRecorder;
            var dtStr = values[0] + " " + values[1];
            if (DateTime.TryParseExact(dtStr, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(dtStr, "y-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(dtStr, "yyyy/M/d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(dtStr, "y/M/d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected static object HttpDecode(RecWrapper rec, string field, string[] values, object data)
        {
            if (values != null && values.Length >= 1)
                return HttpHelper.UrlDecode(values[0]);
            return string.Empty;
        }

        #endregion

        #region Mapping
        public static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new []{"date"},new [] {"time"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"src-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"http-method"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"url-resource"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                        MethodInfo = HttpDecode
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"unk"}},
                        MappedField = null //will be skipped during apply
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dest-port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dest-address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"agent"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                        MethodInfo = HttpDecode
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"url-host"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5"),
                        MethodInfo = HttpDecode
                    }
                }
            };
        }
        #endregion

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            return context.InputRecord == null
                       ? RecordInputType.Unknown
                       : (context.InputRecord.ToString().Length == 0 ? RecordInputType.Comment : RecordInputType.Record);
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            context.Record.ComputerName = Environment.MachineName;
            return NextInstruction.Do;
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
