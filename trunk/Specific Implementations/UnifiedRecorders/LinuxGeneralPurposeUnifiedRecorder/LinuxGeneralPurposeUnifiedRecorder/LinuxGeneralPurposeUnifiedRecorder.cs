using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.LinuxGeneralPurposeUnifiedRecorder
{
    public class LinuxGeneralPurposeUnifiedRecorder : TerminalRecorder
    {

        protected Regex RegSplitForValue = new Regex(@"(?<CODE>[^:]+:[^:]+:[^:]+:[^:]+):(?<DATE_TIME>[^\\s]+\\s[^\\s]+)\\s+(?<CATEGORY>[^\\s]+)\\s+(?<DESCRIPTION>(.|\\n)+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy/MM/dd H:mm:ss";
        private StringBuilder _lastBuffer = new StringBuilder();

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);
            switch (keyword)
            {
                case "NR":
                    if (!string.IsNullOrEmpty(value))
                        RegSplitForValue = new Regex(value);
                    touchCount++;
                    break;
            }
            return true;
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxGeneralPurposeUnifiedRecorderContext(this, RegSplitForValue, DateFormat);
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

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new []{new []{"CODE"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"DATE_TIMR"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"CATEGORY"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"DESCRIPTION"}},
                        MappedField = typeof(RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                if (string.IsNullOrEmpty(_lastBuffer.ToString()))
                {
                    _lastBuffer.Append(source.TrimEnd());
                    return NextInstruction.Abort;
                }

                var getDatetime = source.Split(']');
                if (getDatetime == null || getDatetime.Count() <= 0) return NextInstruction.Skip;
                DateTime date;
                var isdatetime = DateTime.TryParse(string.Format(getDatetime[0].TrimStart('['), DateFormat), out date);

                if (!string.IsNullOrEmpty(_lastBuffer.ToString()) && !isdatetime)
                {
                    _lastBuffer.Append("\n" + source.TrimEnd());
                    return NextInstruction.Abort;
                }

                if (string.IsNullOrEmpty(_lastBuffer.ToString()) || !isdatetime)
                {
                    return NextInstruction.Skip;
                }

                var regmatch = RegSplitForValue.Match(_lastBuffer.ToString());

                if (!regmatch.Success) return NextInstruction.Skip;
                var groupCollection = regmatch.Groups;

                foreach (var key in RegSplitForValue.GetGroupNames())
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                    if (groupCollection[key].Value.Length > 0)
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }

                _lastBuffer = new StringBuilder();
                _lastBuffer.Append(source.TrimEnd());

                return NextInstruction.Return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing veribranch record: " + e);
                return NextInstruction.Abort;
            }
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
