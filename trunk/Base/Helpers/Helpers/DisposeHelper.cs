using System;
using System.Runtime.InteropServices;

namespace Natek.Helpers
{
    public static class DisposeHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public static void Close(params IntPtr[] pointers)
        {
            if (pointers == null) return;

            foreach (var pointer in pointers)
            {
                if (pointer != IntPtr.Zero)
                {
                    try
                    {
                        CloseHandle(pointer);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void Close(params IDisposable[] disposables)
        {
            if (disposables != null)
            {
                foreach (var disposable in disposables)
                {
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}
