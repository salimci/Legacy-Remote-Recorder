using Ex = System.Exception;

namespace SharpSSH.SharpSsh.java
{
	/// <summary>
	/// Summary description for Exception.
	/// </summary>
	public class Exception : Ex
	{
		public Exception() : base()
		{
		}
		public Exception(string msg) : base(msg)
		{
		}

		public virtual string toString()
		{
			return ToString();
		}
	}
}
