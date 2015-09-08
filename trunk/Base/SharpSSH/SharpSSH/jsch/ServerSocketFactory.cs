using System;
using SharpSSH.SharpSsh.java.net;

namespace SharpSSH.SharpSsh.jsch
{
	/// <summary>
	/// Summary description for ServerSocketFactory.
	/// </summary>
	public interface ServerSocketFactory
	{
		ServerSocket createServerSocket(int port, int backlog, InetAddress bindAddr);
	}
}
