using System;
using Natek.Helpers.IO.Readers;

namespace Natek.Recorders.Remote
{
    public class FileLineRecorderContext : FileRecorderContext
    {
        public FileLineRecorderContext()
            : this(null)
        {
        }

        public FileLineRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
        }

        public BufferedLineReader Reader { get; set; }
        public int NewLineCharCount { get; set; }

        public override long ReadRecord(ref Exception error)
        {
            var off = OffsetInStream;
            try
            {
                var nl = 0;
                var text = Reader.ReadLine(InputEncoding, ref nl);
                NewLineCharCount = nl;
                OffsetInStream = Reader.Position;
                var textRecord = InputRecord as TextRecord;
                if (textRecord != null)
                    textRecord.RecordText = text;
            }
            catch (Exception e)
            {
                error = new Exception("ReadRecord failed", e);
            }
            RecordSizeInBytes = OffsetInStream - off;
            return RecordSizeInBytes;
        }

        public override bool CreateReader(ref Exception error)
        {
            try
            {
                Reader = new BufferedLineReader(Stream)
                {
                    Position = OffsetInStream
                };
                return true;
            }
            catch (Exception e)
            {
                error = new Exception("CreatingReader failed", e);
            }
            return false;
        }

        public override bool SetOffset(long offset, ref Exception error)
        {
            if (base.SetOffset(offset, ref error))
            {
                if (Reader != null)
                    Reader.Reset(offset);
                return true;
            }
            return false;
        }
    }
}
