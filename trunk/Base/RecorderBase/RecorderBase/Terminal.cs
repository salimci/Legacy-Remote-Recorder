using System;
using System.IO;

namespace Natek.Recorders.Remote.StreamBased.Terminal
{
    public interface Terminal : IDisposable
    {
        bool Connect(ref Exception error);
        bool Connect(int toPort, ref Exception error);
        bool IsConnected();
        bool CanRead();
        bool CanWrite();
        int Write(byte[] buffer, int offset, int length);
        int ReadByte();
        int Read(byte[] buffer, int offset, int length);
        Stream GetInputStream(ref Exception error);
        Stream GetOutputStream(ref Exception error);
        void WriteByte(byte p);
        void Flush();
    }
}
