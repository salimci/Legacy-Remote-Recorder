using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.LinuxMessageUnifiedRecorder
{
    public class LinuxMessageUnifiedRecorder:TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(?<DATE>[a-zA-Z]*\\s[0-9]*\\s[0-9\\:]+)\\s*(?<HOST_NAME>.[^\\s]+)\\s*\\s*(?<EVENT>.[^\\:]+):\\s*\\[\\s*(?<TICK>.[^\\]]+)\\]\\s*(?<ROOT>.[^\\:]+)\\s*:\\s*(?<DESCRIPTION>.[^\\n]+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy-MM-dd HH:mm:ss";

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo()
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"}, new []{"HOST_NAME"}, new []{"EVENT"}, new []{"TICK"}, new []{"ROOT"},new []{"DESCRIPTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            if (DateTime.TryParseExact(fieldvalues[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

            }
            return NextInstruction.Return;
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null)
            { return base.OnBeforeProcessRecordInput(context); }
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
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

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxMessageUnifiedRecorderContext();
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
            var rec = context.InputRecord;
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
