using System;
using System.Runtime.InteropServices;

namespace Natek.Helpers.Security.Logon {
	public static class NativeAccountValidator {
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool RevertToSelf();

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
			int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public extern static bool DuplicateToken(IntPtr existingTokenHandle,
			int securityImpersonationLevel, ref IntPtr duplicateTokenHandle);
	}
}
