using System;
using System.IO;
using System.Text;

namespace Natek.Helpers.IO.Readers
{
    public class BufferedLineReader : BufferedStreamReader
    {
        public const int LineBufferIncrement = 4096;

        private byte[] _lineBuffer;
        private int _index;
        private int _baseBegin;

        public BufferedLineReader(string filename)
            : this(filename, DefaultBufferSize)
        {
        }

        public BufferedLineReader(string filename, long bufferSize)
            : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), bufferSize)
        {
        }

        public BufferedLineReader(Stream stream)
            : this(stream, DefaultBufferSize)
        {
        }

        public BufferedLineReader(Stream stream, long bufferSize)
            : base(stream, bufferSize)
        {
            _lineBuffer = new byte[DefaultBufferSize];
        }

        protected override void ResetBuffer()
        {
            CopyBuffer();
            _baseBegin = 0;
            base.ResetBuffer();
        }

        public override void Reset()
        {
            base.Reset();
            _index = 0;
            _baseBegin = 0;
        }

        protected virtual void CopyBuffer()
        {
            var chunkSize = offset - _baseBegin;
            if (chunkSize == 0)
                return;

            if (_index + chunkSize > _lineBuffer.Length)
                Array.Resize(ref _lineBuffer, _lineBuffer.Length + ((_index + chunkSize) / LineBufferIncrement + 1) * LineBufferIncrement);

            Buffer.BlockCopy(buffer, _baseBegin, _lineBuffer, _index, chunkSize);
            _index += chunkSize;
        }

        public String ReadLine(Encoding encoding, ref int newLineChars)
        {
            _baseBegin = offset;
            _index = 0;
            int ch;
            var nl = 0;
            while ((ch = Read()) >= 0)
            {
                if (ch == '\r')
                {
                    ch = Read();
                    if (ch < 0)
                    {
                        nl = 1;
                        break;
                    }
                    if (ch == '\n')
                    {
                        nl = 2;
                        break;
                    }
                }
                else if (ch == '\n')
                {
                    nl = 1;
                    break;
                }
            }
            CopyBuffer();
            if (_index > 0)
            {
                newLineChars = nl;
                encoding = encoding ?? Encoding.Default;
                return encoding.GetString(_lineBuffer, encoding.CodePage == 1200 && _lineBuffer[0] == 0 ? 1 : 0, _index - nl);
            }
            return null;
        }
    }
}
