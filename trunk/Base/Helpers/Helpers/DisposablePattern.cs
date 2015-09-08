using System;
using System.Collections.Generic;
using System.Text;

namespace Natek.Helpers.Patterns
{
    public class DisposablePattern : IDisposable
    {
        protected bool disposed;

        protected virtual void DisposeViaDirectCall()
        {
        }

        protected virtual void DisposeViaFinalize()
        {
        }

        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                DisposeViaDirectCall();
            }
            DisposeViaFinalize();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
