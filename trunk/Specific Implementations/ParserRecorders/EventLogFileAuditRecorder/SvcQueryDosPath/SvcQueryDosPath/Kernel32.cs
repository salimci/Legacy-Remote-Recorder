using System;
using System.Runtime.InteropServices;

namespace Natek.Helpers
{
    public static class Kernel32
    {
        public static readonly int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("kernel32.dll")]
        static extern uint QueryDosDevice(string lpDeviceName, IntPtr lpTargetPath, uint ucchMax);

        public static string QueryDosDevice(string drive)
        {
            uint maxSize = 1024;
            var index = drive.IndexOf('\\');
            var volume = index >= 0 ? drive.Substring(0, index) : drive;
            do
            {
                var mem = Marshal.AllocHGlobal((int)maxSize);
                if (mem != IntPtr.Zero)
                {
                    try
                    {
                        var returnSize = QueryDosDevice(volume, mem, maxSize);
                        if (returnSize != 0)
                        {
                            var allDevices = Marshal.PtrToStringAnsi(mem, (int)returnSize);
                            var indexOfNull = allDevices.IndexOf('\0');
                            return indexOfNull >= 0 ? (allDevices.Substring(0, indexOfNull) + (index >= 0 ? drive.Substring(index) : string.Empty)) : null;
                        }
                        if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        {
                            maxSize += 1024;
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(mem);
                    }
                }
                else
                {
                    throw new OutOfMemoryException();
                }
            } while (true);
        }
    }
}
