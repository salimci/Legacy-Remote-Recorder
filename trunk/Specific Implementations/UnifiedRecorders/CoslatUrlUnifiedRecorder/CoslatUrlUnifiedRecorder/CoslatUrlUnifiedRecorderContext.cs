using System.Globalization;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;

namespace Natek.Recorders.Remote.Unified.CoslatUrlUnifiedRecorder
{
    class CoslatUrlUnifiedRecorderContext : LinuxTerminalRecorderContext
    {

        public CoslatUrlUnifiedRecorderContext(RecorderBase recorder)
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
