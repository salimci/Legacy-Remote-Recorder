using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;

namespace Natek.Recorders.Remote.Unified.LinuxGeneralPurposeUnifiedRecorder
{
    class LinuxGeneralPurposeUnifiedRecorderContext : LinuxTerminalRecorderContext
    {

        public Regex RegSplitForValue { get; set; }
        public string DateFormat { get; set; }

        protected Dictionary<DataMappingInfo, string> contextKeys;
        protected Dictionary<DataMappingInfo, string> contextVariables;
        public Dictionary<DataMappingInfo, string> ContextKeys { get { return contextKeys; } }

        public Dictionary<DataMappingInfo, string> ContextVariables { get { return contextVariables; } }
        
        public LinuxGeneralPurposeUnifiedRecorderContext() : this(null) { }

        public LinuxGeneralPurposeUnifiedRecorderContext(RecorderBase recorder) : base(recorder)
        {
           
        }

        public LinuxGeneralPurposeUnifiedRecorderContext(RecorderBase recorder, Regex regSplitForValue, string dateFormat)
            : base(recorder)
        {
            contextKeys = new Dictionary<DataMappingInfo, string>();
            contextVariables = new Dictionary<DataMappingInfo, string>();

            RegSplitForValue = regSplitForValue;
            DateFormat = dateFormat;
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
