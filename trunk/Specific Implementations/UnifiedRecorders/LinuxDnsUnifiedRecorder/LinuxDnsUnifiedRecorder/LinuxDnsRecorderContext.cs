using System.Globalization;

namespace Natek.Recorders.Remote.StreamBased.Terminal.Ssh.Dns
{
    public class LinuxDnsRecorderContext : LinuxTerminalRecorderContext
    {
        public LinuxDnsRecorderContext()
            : this(null)
        {
        }

        public LinuxDnsRecorderContext(RecorderBase recorder)
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
