
using System.IO;

namespace Natek.Helpers.IO.Readers
{
    public class BufferedStreamReader
    {
        public const long DefaultBufferSize = 32768;
        protected byte[] buffer;
        protected Stream stream;
        protected int offset;
        protected int size;

        public BufferedStreamReader(string filename, long bufferSize = DefaultBufferSize)
            : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), bufferSize)
        {
        }

        public BufferedStreamReader(Stream stream, long bufferSize = DefaultBufferSize)
        {
            this.stream = stream;
            buffer = new byte[bufferSize <= 0 ? DefaultBufferSize : bufferSize];
            Reset();
        }

        protected virtual void ResetBuffer()
        {
            offset = 0;
        }

        public virtual void Reset()
        {
            Reset(stream == null ? 0 : stream.Position);
        }

        public virtual void Reset(long virtualPosition)
        {
            offset = 0;
            size = 0;
            Position = virtualPosition;
        }

        public virtual int Read()
        {
            if (offset == size)
            {
                ResetBuffer();
                size = stream.Read(buffer, 0, buffer.Length);
                if (size <= 0)
                    return -1;
            }
            Position++;
            return buffer[offset++];
        }

        public long Position { get; set; }
    }
}
