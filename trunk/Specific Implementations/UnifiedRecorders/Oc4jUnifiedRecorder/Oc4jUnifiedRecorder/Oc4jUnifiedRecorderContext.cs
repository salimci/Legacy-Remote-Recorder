using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;

namespace Natek.Recorders.Remote.Unified.Oc4jUnifiedRecorder
{
    class Oc4jUnifiedRecorderContext: LinuxTerminalRecorderContext
    {

         public Oc4jUnifiedRecorderContext()
            : this(null)
        {
        }

         public Oc4jUnifiedRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
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
