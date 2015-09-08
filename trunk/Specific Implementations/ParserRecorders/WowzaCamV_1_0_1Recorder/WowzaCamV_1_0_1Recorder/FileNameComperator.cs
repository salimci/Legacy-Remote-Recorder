using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WowzaCamV_1_0_1Recorder
{
    public class FileNameComperator : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return
                  DateTime.ParseExact(x.Substring(x.Length - 18, 14), "ddMMyyyyHHmmss", CultureInfo.InvariantCulture).
                      CompareTo(DateTime.ParseExact(y.Substring(y.Length - 18, 14), "ddMMyyyyHHmmss",
                                                    CultureInfo.InvariantCulture));
        }
    }
}
