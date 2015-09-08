using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.PaloAltoUnified
{
    public class PaloAltoUnifiedRecorder : FileLineRecorder
    {
        public PaloAltoUnifiedRecorderBase baseRecorder;

        public PaloAltoUnifiedRecorder()
        {
            baseRecorder = new PaloAltoUnifiedRecorderBase();
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
            return baseRecorder.OnFieldMatchPublic(context, source, ref match);
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
    }
}