using System;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Helpers.IO.Reader;
using SharpSSH.SharpSsh;

namespace Natek.Recorders.Remote.Linux.Ssh
{
    public class LinuxHistoryContext : RecorderContext
    {
        public int ReadTimeout { get; set; }
        public int Port { get; set; }
        public Regex Pattern { get; set; }
        public string Keyword { get; set; }
        protected SshShell Ssh { get; set; }
        protected StreamExpect sshReader;
        protected StringBuilder buffer = new StringBuilder();
        protected Random random = new Random((int)DateTime.Now.Ticks);
        public int NewlineChars { get; set; }
        public bool WaitBegin { get; set; }

        public StreamExpect StreamReader
        {
            get { return sshReader; }
        }

        public string[] Fields { get; set; }


        public LinuxHistoryContext()
            : this(null)
        {
        }

        public LinuxHistoryContext(RecorderBase recorder)
            : base(recorder)
        {
            InputRecord = new TextRecord();
        }

        public override bool SetOffset(long offset, ref Exception error)
        {
            return true;
        }

        protected virtual byte[] GetCommand()
        {
            return Encoding.ASCII.GetBytes("echo && echo 'BEGIN" + Keyword + "' && unset HISTFILE && export HISTTIMEFORMAT='%s %F %T ' && (history|grep -v '" + Keyword + "') && echo && echo 'END" + Keyword + "'");
        }

        protected virtual string ReadLine(ref Exception error)
        {
            buffer.Remove(0, buffer.Length);

            int nl;
            var r = StreamReader.Next(buffer, out nl, out error);
            NewlineChars = nl;
            switch (r)
            {
                case StreamExpectResult.Eof:
                case StreamExpectResult.Error:
                    return null;
                case StreamExpectResult.Expect:
                    error = new Exception("Expect reached");
                    return buffer.ToString();
            }
            return buffer.ToString(0, buffer.Length - nl);
        }

        public override long ReadRecord(ref Exception error)
        {
            try
            {
                var textRecord = InputRecord as TextRecord;
                if (textRecord != null) textRecord.RecordText = ReadLine(ref error);
                return error == null ? 1 : 0;
            }
            catch (Exception e)
            {
                error = e;
            }
            return 0;
        }

        protected virtual bool SendCommand(ref Exception error)
        {
            try
            {
                var command = GetCommand();
                Ssh.IO.Write(command, 0, command.Length);
                Ssh.IO.WriteByte(10);
                Ssh.IO.Flush();
                Keyword = "BEGIN" + Keyword;
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public override bool CreateReader(ref Exception error)
        {
            try
            {
                Ssh = new SshShell(Recorder.RemoteHost, Recorder.User, Recorder.Password);
                Ssh.Connect(Port <= 0 ? 22 : Port, int.MaxValue);
                Keyword = DateTime.Now.Ticks + "." + Guid.NewGuid() + "." + random.Next();
                if (!SendCommand(ref error))
                    return false;
                WaitBegin = true;
                sshReader = new StreamExpect
                    {
                        Data = Ssh,
                        Expect = Pattern,
                        ReadTimeout = ReadTimeout,
                        Stream = Ssh.IO
                    };
                sshReader.OnReadTimeout += sshReader_OnReadTimeout;
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        private void sshReader_OnReadTimeout(object sender, EventArgs e)
        {
            var reader = sender as StreamExpect;
            if (reader != null)
                try
                {
                    var sshShell = reader.Data as SshShell;
                    if (sshShell != null) sshShell.Close();
                }
                catch
                {
                }
        }

        public override NextInstruction FixOffsets(NextInstruction nextInstruction, long offset, long[] headerOff, ref Exception error)
        {
            return NextInstruction.Do;
        }

        protected override void DisposeViaDirectCall()
        {
            base.DisposeViaDirectCall();
            if (Ssh != null)
            {
                try
                {
                    Ssh.Close();
                }
                catch
                {
                }
            }
        }
    }
}
