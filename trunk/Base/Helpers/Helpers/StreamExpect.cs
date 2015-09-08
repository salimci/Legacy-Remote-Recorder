using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Natek.Helpers.Patterns;

namespace Natek.Helpers.IO.Reader
{
    public class StreamExpect : DisposablePattern
    {
        public event EventHandler OnReadTimeout;
        protected readonly ObjectValue<int> streamState;
        protected int readTimeout;
        protected DateTime timeoutOn;
        protected Match lastMatch;
        protected readonly Timer timer;

        public StreamExpect()
        {
            streamState = new ObjectValue<int>(0);
            timer = new Timer(HandleReadTimeout, null, Timeout.Infinite, Timeout.Infinite);
            timeoutOn = DateTime.Now;
            ReadTimeout = 1000;
        }

        public Stream Stream { get; set; }
        public object Data { get; set; }
        public Regex Expect { get; set; }

        public Match LastMatch
        {
            get { return lastMatch; }
        }

        public ObjectValue<int> StreamState
        {
            get { return streamState; }
        }

        protected virtual void HandleReadTimeout(object state)
        {
            lock (streamState)
            {
                if (streamState == 1 && readTimeout > 0)
                {
                    var next = timeoutOn.Subtract(DateTime.Now).TotalMilliseconds;
                    if (next > 0)
                        timer.Change((long)next, Timeout.Infinite);
                    else if (OnReadTimeout != null)
                        OnReadTimeout(this, EventArgs.Empty);
                }
            }
        }

        public int ReadTimeout
        {
            get { return readTimeout; }
            set
            {
                readTimeout = value;
                if (value > 0)
                {
                    var newDue = DateTime.Now.AddMilliseconds(readTimeout);
                    if (newDue < timeoutOn)
                    {
                        var next = newDue.Subtract(DateTime.Now).TotalMilliseconds;
                        if (next > 0)
                        {
                            timeoutOn = newDue;
                            lock (streamState)
                            {
                                if (streamState != 0)
                                    timer.Change((long)next, Timeout.Infinite);
                            }
                        }
                    }
                }
                else
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        StreamExpectResult StreamDisposed(ref Exception error)
        {
            error = disposed
            ? new ObjectDisposedException("input stream")
            : new Exception("State negative but not disposed. Invalid state");
            return StreamExpectResult.Error;
        }

        public StreamExpectResult Next(StringBuilder buffer, out int nl, out Exception error)
        {
            nl = 0;
            error = null;
            try
            {
                lock (streamState)
                {
                    while (streamState.Value > 0)
                    {
                        if (!Monitor.Wait(streamState))
                        {
                            error = new Exception("Wait failed");
                            return StreamExpectResult.Error;
                        }
                    }

                    if (streamState < 0)
                        return StreamDisposed(ref error);

                    if (readTimeout > 0)
                    {
                        timeoutOn = DateTime.Now.AddMilliseconds(readTimeout);
                        timer.Change(readTimeout, Timeout.Infinite);
                    }
                    streamState.Value = 1;
                    Monitor.PulseAll(streamState);
                }
                var ch = 0;
                nl = 0;
                Match m = null;
                while ((ch = Stream.ReadByte()) >= 0)
                {
                    buffer.Append((char)ch);
                    lock (streamState)
                    {
                        if (streamState == 1 && readTimeout > 0)
                            timeoutOn = DateTime.Now.AddMilliseconds(readTimeout);
                    }

                    if (Expect != null && (m = Expect.Match(buffer.ToString())).Success)
                        break;
                    if (ch == 10)
                    {
                        ++nl;
                        break;
                    }
                    if (ch == 13)
                        ++nl;
                    else
                        nl=0;
                }
                lock (streamState)
                {
                    if (streamState > 0)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        streamState.Value = 0;
                        lastMatch = m;
                        Monitor.PulseAll(streamState);
                        return m != null && m.Success
                                   ? StreamExpectResult.Expect
                                   : (nl > 0 ? StreamExpectResult.Line : StreamExpectResult.Eof);
                    }
                    if (streamState == 0)
                    {
                        error = new Exception("Stream State Found 0. Should not have been set outsize");
                        return StreamExpectResult.Error;
                    }
                    return StreamDisposed(ref error);
                }
            }
            catch (Exception e)
            {
                error = e;
                lock (streamState)
                {
                    if (streamState > 0)
                    {
                        streamState.Value = 0;
                        Monitor.PulseAll(streamState);
                    }
                }
            }
            return StreamExpectResult.Error;
        }

        protected override void DisposeViaFinalize()
        {
            base.DisposeViaFinalize();
            lock (streamState)
            {
                streamState.Value = -1;
                Monitor.PulseAll(streamState);
            }
        }
    }
}
