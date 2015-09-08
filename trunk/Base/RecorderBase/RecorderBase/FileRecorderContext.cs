using System;
using System.IO;
using Natek.Helpers;
using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public abstract class FileRecorderContext : RecorderContext
    {
        public FileRecorderContext()
            : this(null)
        {
        }

        public FileRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
        }

        public bool IsLastFile { get; set; }
        public RecorderFileSystemInfo CurrentFile { get; set; }
        public Stream Stream { get; set; }
        public string DayIndexMap { get; set; }
        public string FileSortOrder { get; set; }


        public virtual NextInstruction CreateStream(ref Exception error)
        {
            try
            {
                DisposeHelper.Close(Stream);
                var inf = CurrentFile;
                if (inf != null)
                {
                    Stream = new FileStream(inf.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return NextInstruction.Do;
                }
                error = new Exception("Null FileInfo. Cannot Create Stream");
            }
            catch (Exception e)
            {
                error = e;
            }
            return NextInstruction.Abort;
        }

        public virtual NextInstruction CloseStream(ref Exception error)
        {
            DisposeHelper.Close(Stream);
            Stream = null;
            return NextInstruction.Do;
        }

        public override bool SetOffset(long offset, ref Exception error)
        {
            try
            {
                if (Stream != null)
                {
                    OffsetInStream = Stream.Seek(offset, SeekOrigin.Begin);
                    return OffsetInStream == offset;
                }
                error = new NullReferenceException("No stream has been created for this context");
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public override NextInstruction FixOffsets(NextInstruction nextInstruction,
            long offset, long[] headerOff, ref Exception error)
        {
            try
            {
                if ((nextInstruction & NextInstruction.Continue) != NextInstruction.Continue)
                {
                    HeaderOffset[0] = headerOff[0];
                    HeaderOffset[1] = headerOff[1];
                }
                else
                    headerOff = HeaderOffset;

                if (!SetOffset(offset, ref error) || OffsetInStream != offset)
                {
                    error = new Exception("Setting offset back failed");
                    return NextInstruction.Abort;
                }
                return nextInstruction;
            }
            catch (Exception e)
            {
                error = e;
            }
            return NextInstruction.Abort;
        }

        protected override void DisposeViaDirectCall()
        {
            base.DisposeViaDirectCall();
            if (Stream != null)
            {
                try
                {
                    Stream.Dispose();
                }
                catch
                {
                }
                Stream = null;
            }
        }
    }
}
