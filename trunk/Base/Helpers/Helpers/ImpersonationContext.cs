using System;
using System.Security.Principal;
using Natek.Helpers.Patterns;

namespace Natek.Helpers.Security.Logon
{
    public class ImpersonationContext : DisposablePattern
    {
        public IntPtr Token { get; set; }
        public IntPtr DuplicateToken { get; set; }
        public WindowsImpersonationContext Context { get; set; }

        protected override void DisposeViaFinalize()
        {
            try
            {
                base.DisposeViaFinalize();
            }
            finally
            {
                if (Context != null)
                {
                    try
                    {
                        Context.Undo();
                    }
                    catch
                    {
                    }
                    DisposeHelper.Close(Context);
                }
                DisposeHelper.Close(Token, DuplicateToken);
            }
        }
    }
}
