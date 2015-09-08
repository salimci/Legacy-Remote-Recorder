using System;
using System.Collections.Generic;

namespace Natek.Recorders.Remote.Helpers.Basic
{
    public class StringHelper : IEqualityComparer<string>
    {
        public static string MakeSureLength(string s, int maxLen)
        {
            return string.IsNullOrEmpty(s) || maxLen < 0 || s.Length <= maxLen ? s : s.Substring(0, maxLen);
        }

        public StringComparison Comparison { get; set; }

        public StringHelper(StringComparison comparision)
        {
            Comparison = comparision;
        }

        public bool Equals(string x, string y)
        {
            return String.Compare(x, y, Comparison) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }

        public static bool NullEmptyEquals(string l, string r, StringComparison comparison = StringComparison.Ordinal)
        {
            if (l == null)
                return string.IsNullOrEmpty(r);
            if (r == null)
                return l.Length == 0;
            return l.Equals(r);
        }
    }
}
