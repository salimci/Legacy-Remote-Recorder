using System;
using System.Net.Sockets;
using Log;
using LogMgr;
using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public class SyslogRecorderContext : RecorderContext
    {

        protected Syslog syslogInstance;
        public ProtocolType ProtocolType { get; set; }
        public int Port { get; set; }
        public string SyslogAddress { get; set; }

        public SyslogRecorderContext(RecorderBase recorder, ProtocolType protocolType, string syslogAddress, int port = 514)
            : base(recorder)
        {
            ProtocolType = protocolType;
            Port = port;
            SyslogAddress = syslogAddress;
        }

        public Syslog SyslogInstance
        {
            get { return syslogInstance; }
        }

        public override bool SetOffset(long offset, ref Exception error)
        {
            return true;
        }

        public override long ReadRecord(ref Exception error)
        {
            return 0;
        }

        public override bool CreateReader(ref Exception error)
        {
            try
            {
                Recorder.Log(LogLevel.DEBUG, "Creating instance for:" + string.Format("Server({0}), Port({1}) Protocol({2})", SyslogAddress, Port, ProtocolType));
                syslogInstance = new Syslog(SyslogAddress, Port, ProtocolType);
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public override NextInstruction FixOffsets(NextInstruction nextInstruction, long offset, long[] headerOff, ref Exception error)
        {
            return NextInstruction.Do;
        }
    }
}
