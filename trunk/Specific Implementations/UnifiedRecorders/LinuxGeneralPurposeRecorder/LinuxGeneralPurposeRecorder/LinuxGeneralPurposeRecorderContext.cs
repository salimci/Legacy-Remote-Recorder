using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;


namespace Natek.Recorders.Remote.Unified.LinuxGeneralPurposeRecorder
{
    class LinuxGeneralPurposeRecorderContext : LinuxTerminalRecorderContext
    {
        public Regex RegSplitForValue { get; set; }
        public Dictionary<string, int> FieldOrder { get; set; }
        public string DateFormat { get; set; }

        public long PhantomOffsetInSteam;

        public Match LastMatch;

        public StringBuilder DescriptionBuffer;

        public Dictionary<string, string> FieldValueBuffer;

        public LinuxGeneralPurposeRecorderContext() : this(null) { }

        public LinuxGeneralPurposeRecorderContext(RecorderBase recorder) : base(recorder)
        {
            DescriptionBuffer = new StringBuilder();
        }

        public LinuxGeneralPurposeRecorderContext(RecorderBase recorder, Regex regSplitForValue, Dictionary<string, int> fieldOrder, string dateFormat)
            : base(recorder)
        {
            DescriptionBuffer = new StringBuilder();
            RegSplitForValue = regSplitForValue;
            FieldOrder = fieldOrder;
            DateFormat = dateFormat;
            FieldValueBuffer = new Dictionary<string, string>();
        }

        protected override Terminal CreateTerminal()
        {
            return new SshShellTerminal(Recorder.RemoteHost, Recorder.User, Recorder.Password);
        }

        public override string GetCurrentFileRecordsFrom()
        {
            return (OffsetInStream + 1).ToString(CultureInfo.InvariantCulture);
        }

        public override string GetCurrentFileRecordsTo()
        {
            return (OffsetInStream + Recorder.MaxRecordSend + 1).ToString(CultureInfo.InvariantCulture);
        }
    }
}
