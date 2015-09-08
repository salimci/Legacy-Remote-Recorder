using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DAL;

namespace Natek.Helpers.Security.Logon
{
    public static class AccountValidator
    {
        public static ImpersonationContext ValidateAccount(string user, string password, ref Exception error)
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                    return null;

                var domain = string.Empty;
                SplitUserDomain(ref user, ref domain);
                return ValidateAccount(domain, user, password, ref error);
            }
            catch (Exception e)
            {
                error = e;
            }
            return null;
        }

        public static ImpersonationContext ValidateAccount(string domain, string user, string password, ref Exception error)
        {
            ImpersonationContext context = null;
            try
            {
                if (!NativeAccountValidator.RevertToSelf())
                    throw new Exception("Revert to self failed:" + Marshal.GetLastWin32Error());

                var ptr = IntPtr.Zero;
                if (!NativeAccountValidator.LogonUser(user, domain, password,
                                                      (int)LogonType.Logon32LogonNewCredentials,
                                                      (int)LogonProvider.Logon32ProviderDefault, ref ptr))
                    throw new Exception("Logon user failed with error code:" + Marshal.GetLastWin32Error());
                context = new ImpersonationContext() { Token = ptr };
                ptr = IntPtr.Zero;
                if (!NativeAccountValidator.DuplicateToken(context.Token, 2, ref ptr))
                    throw new Exception("Logon user failed while duplicating token with error code:" +
                                        Marshal.GetLastWin32Error());
                context.DuplicateToken = ptr;
                context.Context = WindowsIdentity.Impersonate(context.DuplicateToken);
                return context;
            }
            catch (Exception e)
            {
                error = e;
                DisposeHelper.Close(context);
            }
            return null;
        }

        public static void SplitUserDomain(ref string user, ref string domain)
        {
            var index = user.IndexOf('\\');
            if (index >= 0)
            {
                if (index + 1 == user.Length)
                    throw new Exception("Domain found but no user:[" + user);
                domain = user.Substring(0, index++);
                user = user.Substring(index);
            }
        }

        public static void DecryptPassword(string key, ref string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                try
                {
                    password = Encrypter.Decyrpt(key, password);
                }
                catch
                {
                    throw new Exception("password is not in expected encoding");
                }
            }
        }
    }
}
