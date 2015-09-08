using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.WamppServerErrorUnifiedRecorder
{
    public class WamppServerErrorUnifiedRecorder : TerminalRecorder
    {

        protected Regex RegSplitForValue = new Regex(@"\[(?<DATE_TIME>[^\]]+)\s*\]\s*\[(?<SEVERITY>[^\]]+)\]\s+(\[client\s*(?<CLIENT_IP>[^\]]+)\]\s*)?(?<MESSAGE>[^\n]+)", RegexOptions.Compiled);

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;

            var recorder = data as WamppServerErrorUnifiedRecorder;

            return DateTime.TryParseExact(values[0], "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,out dt) ? dt.AddSeconds(recorder == null ? 0 : recorder.Zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new []{new []{"DATE_TIME"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"SEVERITY"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")      
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"CLIENT_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"MESSAGE"}},
                        MappedField = typeof(RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            try
            {
                var groupCollection = match.Groups;

                foreach (var key in RegSplitForValue.GetGroupNames())
                {
                    if (key != "0" && key != "1")
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }

                context.FieldBuffer.Add(source);
                return NextInstruction.Return;
            }
            catch (Exception)
            {
                return NextInstruction.Skip;
            }
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new WamppServerErrorUnifiedRecorderContext(this);
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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

