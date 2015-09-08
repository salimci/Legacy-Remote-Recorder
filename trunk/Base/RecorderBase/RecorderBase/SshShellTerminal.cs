using System;
using System.IO;
using SharpSSH.SharpSsh;

namespace Natek.Recorders.Remote.StreamBased.Terminal.Ssh
{
    public class SshShellTerminal : Terminal
    {
        protected SshShell sshShell;
        protected string host;
        protected string user;
        protected string password;
        protected int port;
        protected int connectTimeout;

        public SshShellTerminal(string host, string user, string password = null)
        {
            this.host = host;
            this.user = user;
            this.password = password;
            port = 22;
            sshShell = string.IsNullOrEmpty(password) ? new SshShell(host, user) : new SshShell(host, user, password);
        }



        public bool Connect(ref Exception error)
        {
            return Connect(22, ref error);
        }

        public bool Connect(int toPort, ref Exception error)
        {
            return Connect(toPort, int.MaxValue, ref error);
        }

        public bool Connect(int toPort, int within, ref Exception error)
        {
            try
            {
                port = toPort;
                connectTimeout = within;
                sshShell.Connect(toPort, within);
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public bool IsConnected()
        {
            return sshShell.Connected;
        }

        public bool CanRead()
        {
            var s = sshShell.IO;
            return s != null && s.CanRead;
        }

        public bool CanWrite()
        {
            var s = sshShell.IO;
            return s != null && s.CanWrite;
        }

        public int Write(byte[] buffer, int offset, int length)
        {
            sshShell.IO.Write(buffer, offset, length);
            return length;
        }

        public int ReadByte()
        {
            return sshShell.IO.ReadByte();
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            return sshShell.IO.Read(buffer, offset, length);
        }

        public Stream GetInputStream(ref Exception error)
        {
            return sshShell.IO;
        }

        public Stream GetOutputStream(ref Exception error)
        {
            return sshShell.IO;
        }

        public void WriteByte(byte value)
        {
            sshShell.IO.WriteByte(value);
        }

        public void Flush()
        {
            sshShell.IO.Flush();
        }

        public void Dispose()
        {
            if (sshShell != null)
            {
                try
                {
                    sshShell.Close();
                }
                finally
                {
                    try
                    {
                        sshShell.GetStream().Close();
                    }
                    finally
                    {
                        sshShell = null;
                    }
                }
            }
        }
    }
}
