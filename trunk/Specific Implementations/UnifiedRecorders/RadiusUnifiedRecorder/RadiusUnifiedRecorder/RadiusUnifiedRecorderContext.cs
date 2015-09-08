using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;

namespace Natek.Recorders.Remote.Unified.RadiusUnifiedRecorder
{

        public class RadiusUnifiedRecorderContext : LinuxTerminalRecorderContext
        {
            public int State { get; set; }
            private readonly StringBuilder _lastBuffer = new StringBuilder();
            public StringBuilder LastBuffer {
                get
                {
                    return _lastBuffer;
                   
                }
            }
            public RadiusUnifiedRecorderContext()
                : this(null)
            {
            }

            public RadiusUnifiedRecorderContext(RecorderBase recorder)
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
