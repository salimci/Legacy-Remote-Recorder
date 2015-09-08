using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Log;

namespace Natek.Recorders.Remote.Unified.LinuxGeneralPurposeRecorder
{
    public class LinuxGeneralPurposeRecorder : TerminalRecorder
    {
        protected Regex RegSplitForValue =new Regex(
                @"^([0-9]+:[0-9]+:[0-9]+:[0-9]+):([0-9]+\/[0-9]+\/[0-9]+[\s][0-9]+:[0-9]+:[0-9]+\.[0-9]+)[\s]([A-Za-z]+)[\s]+(.*)",
                RegexOptions.Compiled);

        protected string DateFormat = "yyyy/MM/dd H:mm:ss";

        protected Dictionary<string, int> FieldOrder = new Dictionary<string, int>
        {
            {"Code",1},
            {"Datetime",2},
            {"Category",3},
            {"Description",4}
        };

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount,
            ref Exception error)
        {
            base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error);

            switch (keyword)
            {
                case "SR":
                    if (!string.IsNullOrEmpty(value))
                        RegSplitForValue = new Regex(value, RegexOptions.Compiled);
                    touchCount++;
                    break;
                case "DF":
                    if (!string.IsNullOrEmpty(value))
                        DateFormat = value;
                    touchCount++;
                    break;
                case "PO":
                    if (!string.IsNullOrEmpty(value))
                    {

                        var sequanceRegex = new Regex(@"([^,]+)", RegexOptions.Compiled);
                        var sequanceMatch = sequanceRegex.Match(value);
                        var index = 1;
                        while (sequanceMatch.Success)
                        {
                            if (!FieldOrder.ContainsKey(sequanceMatch.Groups[1].Value))
                            {
                                Log(LogLevel.ERROR, string.Format("Entered PO value |{0}| is illegal", sequanceMatch.Groups[1].Value));
                                return false;
                            }

                            FieldOrder[sequanceMatch.Groups[1].Value] = index;
                            index++;
                            sequanceMatch = sequanceMatch.NextMatch();
                        }
                    }
                    touchCount++;
                    break;
            }
            return true;
        }

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
            if (DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LinuxGeneralPurposeRecorderContext(this, RegSplitForValue, FieldOrder, DateFormat);
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
                        Original = new []{new []{"Code"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"Datetime"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"Category"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"Description"}},
                        MappedField = typeof(RecWrapper).GetProperty("Description")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"Source"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    }
                }
            };
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

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            var localContext = (LinuxGeneralPurposeRecorderContext)context;

            try
            {
                if (string.IsNullOrEmpty(localContext.DescriptionBuffer.ToString()) && match.Success)
                {
                    SetLastMatchField(context, source, match);               
                    localContext.DescriptionBuffer.Append(match.Groups[localContext.FieldOrder["Description"]].Value);
                    return NextInstruction.Abort;
                }

                if (!string.IsNullOrEmpty(localContext.DescriptionBuffer.ToString()) && match.Success)
                {

                    context.FieldBuffer.Clear();
                    context.FieldBuffer[0] = localContext.LastMatch.Groups[localContext.FieldOrder["Code"]].Value;
                    context.FieldBuffer[1] = localContext.LastMatch.Groups[localContext.FieldOrder["Datetime"]].Value;
                    context.FieldBuffer[2] = localContext.LastMatch.Groups[localContext.FieldOrder["Category"]].Value;
                    context.FieldBuffer[3] = localContext.DescriptionBuffer.ToString();
                    context.FieldBuffer[4] = context.Recorder.RemoteHost;


                    context.OffsetInStream += localContext.PhantomOffsetInSteam;
                    localContext.PhantomOffsetInSteam = 0;
                    localContext.DescriptionBuffer = new StringBuilder();
                    SetLastMatchField(context, source, match);
                    localContext.DescriptionBuffer.Append(match.Groups[localContext.FieldOrder["Description"]].Value);

                    return NextInstruction.Return;

                }

                if (localContext.LastMatch != null)
                    localContext.DescriptionBuffer.Append(" " + source);

                localContext.PhantomOffsetInSteam++;

                return NextInstruction.Skip;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing linux general purpose record: " + e);
                return NextInstruction.Abort;
            }
        }

        protected override void SetLastMatchField(RecorderContext context, string source, Match match)
        {
            var localContext = (LinuxGeneralPurposeRecorderContext)context;
            localContext.LastMatch = match;
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
