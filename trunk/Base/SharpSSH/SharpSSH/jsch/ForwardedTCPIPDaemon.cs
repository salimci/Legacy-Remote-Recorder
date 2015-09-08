using System;
using SharpSSH.SharpSsh.java.lang;

namespace SharpSSH.SharpSsh.jsch
{
	public interface ForwardedTCPIPDaemon : Runnable
	{
		void setChannel(ChannelForwardedTCPIP channel);
		void setArg(Object[] arg);
	}
}
