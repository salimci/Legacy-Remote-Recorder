using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NetScalerNetworkBalancerV_1_0_0Recorder
{
    public class FileNameComperator : IComparer<string>
    {
        //NS130719.log.0
        private static Regex RegFilename = new Regex("^(.*)\\.log(\\.([0-9]+))?$", RegexOptions.IgnoreCase);
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
                        int xi = -1, yi = -1;

                        if (mx.Groups.Count >= 4)
                        {
                            if (!int.TryParse(mx.Groups[3].Value, out xi))
                            {
                                xi = -2;
                            }
                        }
                        if (mx.Groups.Count >= 4)
                        {
                            if (!int.TryParse(my.Groups[3].Value, out yi))
                            {
                                yi = -2;
                            }
                        }
                        return xi - yi;
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
