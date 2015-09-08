using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Exchange2010SP2V1_0_2Recorder
{
    public class FileNameComperator : IComparer<string>
    {
        private static Regex RegFilename = new Regex("^(.*[0-9]{8})-([0-9]+).LOG$", RegexOptions.IgnoreCase);
        public int Compare(string x, string y)
        {
            Match mx, my;

            int c;
            if ((mx = RegFilename.Match(x)) != null && mx.Success)
            {
                if ((my = RegFilename.Match(y)) != null && my.Success)
                {
                    if ((c = mx.Groups[1].Value.CompareTo(my.Groups[1].Value)) == 0)
                    {
                        int xi, yi;

                        if (int.TryParse(mx.Groups[2].Value, out xi))
                        {
                            if (int.TryParse(my.Groups[2].Value, out yi))
                            {
                                return xi - yi;
                            }
                            return -1;
                        }
                        if (int.TryParse(my.Groups[2].Value, out yi))
                        {
                            return 1;
                        }
                        return mx.Groups[2].Value.CompareTo(my.Groups[2]);
                    }
                    return c;
                }
                return -1;
            }
            if ((my = RegFilename.Match(y)) != null && my.Success)
            {
                return 1;
            }
            return x.CompareTo(y);
        }
    }
}
