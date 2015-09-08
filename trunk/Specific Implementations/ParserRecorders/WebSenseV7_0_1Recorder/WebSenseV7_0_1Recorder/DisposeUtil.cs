using System;

namespace Natek.Util
{
    public class DisposeUtil
    {
        public static void Dispose(params IDisposable[] dArgs)
        {
            if (dArgs != null)
            {
                foreach (IDisposable d in dArgs)
                {
                    if (d != null)
                    {
                        try
                        {
                            d.Dispose();
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
