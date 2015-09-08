using System;
using System.Text;
using System.Text.RegularExpressions;
using Log;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Helpers.IO.Reader;
using Natek.Helpers.Log;

namespace Natek.Recorders.Remote.StreamBased.Terminal
{
    public abstract class TerminalRecorderContext : FileRecorderContext
    {
        protected StreamExpect streamExpect;
        protected StringBuilder buffer = new StringBuilder();
        protected Random random = new Random((int)DateTime.Now.Ticks);

        public int ReadTimeout { get; set; }
        public int Port { get; set; }
        public Regex Pattern { get; set; }
        public string Keyword { get; set; }
        public Terminal Terminal { get; set; }
        public int NewlineChars { get; set; }
        public bool WaitBegin { get; set; }


        public StreamExpect StreamStreamExpect
        {
            get { return streamExpect; }
        }
        public string[] Fields { get; set; }

        public TerminalRecorderContext()
            : this(null)
        {
        }

        public TerminalRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
            InputRecord = new TextRecord();
            ReadTimeout = 60000;
        }


        public abstract string CommandReadRecords { get; }

        public abstract string CommandListFiles { get; }

        public abstract string CommandFileSystemInfo { get; }

        public abstract string CommandParentOf { get; }

        public override bool SetOffset(long offset, ref Exception error)
        {
            return true;
        }

        public virtual byte[] GetCommand(string command)
        {
            var cmd = "(echo ; echo 'BEGIN" + Keyword + "' ; (( " + command + "|awk -v c=0 '{print \"0;\"++c\";\"$0;}') 2>&1|awk -v c=0 '{if (match($0,\"^0;\") <= 0) printf \"1;\"++c\";\"; print $0;}'|sort -t ';' -n -k 1 -k 2) ; echo 'END" + Keyword + "' )";
            Recorder.Log(LogLevel.DEBUG, "GetCmd=>[" + cmd + "]");
            return Encoding.UTF8.GetBytes(cmd);
        }

        public virtual string ReadLine(ref Exception error)
        {
            buffer.Remove(0, buffer.Length);

            int nl;
            Recorder.Log(LogLevel.DEBUG, "ReadLine Expect Next");
            var r = StreamStreamExpect.Next(buffer, out nl, out error);
            NewlineChars = nl;

            switch (r)
            {
                case StreamExpectResult.Eof:
                    Recorder.Log(LogLevel.INFORM, "Next=>EOF");
                    return null;
                case StreamExpectResult.Error:
                    Recorder.Log(LogLevel.ERROR, "Error:" + error);
                    if (error == null)
                        error = new Exception("Getting next line failed with unknown reason");
                    return null;
                case StreamExpectResult.Expect:
                    Recorder.Log(LogLevel.DEBUG, "Next: Expect Found");
                    error = new ExpectReachedException("Expect reached");
                    return buffer.ToString();
            }
            return buffer.ToString(0, buffer.Length - nl);
        }

        public override long ReadRecord(ref Exception error)
        {
            try
            {
                error = null;
                var text = ReadLine(ref error);
                if (text != null && error == null)
                {
                    var textRecord = InputRecord as TextRecord;
                    if (textRecord != null) textRecord.RecordText = text;
                    return 1;
                }
                if (error == null)
                    error = new Exception("Read failed with unknown reason");
                else if (error is ExpectReachedException)
                    error = null;
            }
            catch (Exception e)
            {
                error = e;
            }
            return 0;
        }

        public virtual bool SendCommand(string command, ref Exception error)
        {
            try
            {
                Keyword = DateTime.Now.Ticks + "." + Guid.NewGuid() + "." + random.Next();
                var cmd = GetCommand(command);
                Terminal.Write(cmd, 0, cmd.Length);

                Terminal.WriteByte(10);
                Terminal.Flush();
                Keyword = "BEGIN" + Keyword;
                WaitBegin = true;
                return true;
            }
            catch (Exception e)
            {
                Recorder.Log(LogLevel.ERROR, "Send Error:" + e);
                error = e;
            }
            return false;
        }

        protected abstract Terminal CreateTerminal();

        protected virtual void StreamExpectOnReadTimeout(object sender, EventArgs e)
        {
            Recorder.Log(LogLevel.WARN, "StreamExpect Read Timeout");
            var reader = sender as StreamExpect;
            if (reader != null)
                try
                {
                    CloseTerminal();
                }
                catch
                {
                }
        }

        public override NextInstruction FixOffsets(NextInstruction nextInstruction, long offset, long[] headerOff, ref Exception error)
        {
            return NextInstruction.Do;
        }

        public virtual bool TerminalReady()
        {
            return Terminal != null && Terminal.IsConnected();
        }

        public virtual void CloseTerminal()
        {
            Recorder.Log(LogLevel.DEBUG, "Close Terminal");
            if (streamExpect != null)
            {
                streamExpect.OnReadTimeout -= StreamExpectOnReadTimeout;
                streamExpect = null;
            }

            DisposeHelper.Close(Terminal, streamExpect);
            Terminal = null;
        }

        protected override void DisposeViaDirectCall()
        {
            base.DisposeViaDirectCall();
            CloseTerminal();
        }

        public delegate void OnRecordReadDelegate(object[] args);

        public virtual void ExecuteRemoteCommand(string command, OnRecordReadDelegate onRecordRead, object[] args = null)
        {
            Exception error = null;
            try
            {
                Recorder.Log(LogLevel.DEBUG, "Execute command [" + command + "]");
                var ins = CreateStream(ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    throw error ?? new Exception("Unknown error from create reader. CreateStream Returned:" + ins);
                if (!SendCommand(command, ref error))
                    throw error ?? new Exception("Unknown error from send command");
                var canRead = true;
                while (canRead && ReadRecord(ref error) > 0)
                {
                    switch (Recorder.InputTextType(this, ref error))
                    {
                        case RecordInputType.Comment:
                            break;
                        case RecordInputType.EndOfStream:
                            canRead = false;
                            break;
                        case RecordInputType.Record:
                            onRecordRead(args);
                            break;
                        default:
                            throw new Exception("Unknown Input:[" + Record + "]");
                    }
                }
                if (canRead)
                    throw new Exception("Unexpected Operation termination:" + error);
            }
            catch (Exception e)
            {
                Recorder.Log(LogLevel.ERROR, "Execute command [" + command + "], Failed:" + e);
                throw;
            }
        }

        public override NextInstruction CreateStream(ref Exception error)
        {
            try
            {
                if (TerminalReady())
                    return NextInstruction.Do;
                CloseTerminal();

                Recorder.Log(LogLevel.DEBUG, "Var:" + Recorder.CustomVar1);
                Terminal = CreateTerminal();
                if (Terminal.Connect(Port <= 0 ? 22 : Port, ref error))
                {
                    Stream = Terminal.GetInputStream(ref error);
                    Recorder.Log(LogLevel.INFORM, "Create Terminal ReadTimeout: " + ReadTimeout);
                    streamExpect = new StreamExpect
                    {
                        Data = Terminal,
                        Expect = Pattern,
                        ReadTimeout = ReadTimeout,
                        Stream = Terminal.GetInputStream(ref error)
                    };
                    streamExpect.OnReadTimeout += StreamExpectOnReadTimeout;
                    return NextInstruction.Do;
                }
            }
            catch (Exception e)
            {
                Recorder.Log(LogLevel.ERROR, "CReate Stream Error:" + e);
                error = e;
            }
            return NextInstruction.Abort;
        }

        public override NextInstruction CloseStream(ref Exception error)
        {
            if (!TerminalReady())
            {
                CloseTerminal();
            }
            return NextInstruction.Do;
        }

        public abstract string GetCurrentFileRecordsFrom();
        public abstract string GetCurrentFileRecordsTo();

        public override bool CreateReader(ref Exception error)
        {
            try
            {
                if (SendCommand(CommandReadRecords.Replace("@NODE", CurrentFile.FullName)
                                                  .Replace("@FROM", GetCurrentFileRecordsFrom())
                                                  .Replace("@TO", GetCurrentFileRecordsTo()), ref error))
                    return true;
                error = new Exception("Unknown error from send command");
            }
            catch (Exception e)
            {
                error = e;
            }
            Recorder.Log(LogLevel.ERROR, "Create Reader Error:" + error);
            return false;
        }
    }
}
