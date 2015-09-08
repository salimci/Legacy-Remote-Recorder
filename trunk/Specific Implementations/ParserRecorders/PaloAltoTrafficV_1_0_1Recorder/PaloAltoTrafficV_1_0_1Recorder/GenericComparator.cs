using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Natek.Helpers.GenericComparator
{
    class GenericComparator : IComparer<string>
    {
        public delegate int Comparer<T>(T t1, T t2);

        public Regex Pattern { get; set; }
        public StringComparison ComparisonType { get; set; }
        public Comparer<string>[] PatternFieldComperators { get; set; }
        public bool IsNullAndEmptyEqual { get; set; }

        public int Compare(string x, string y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                return IsNullAndEmptyEqual && y.Length == 0 ? 0 : -1;
            }
            if (y == null)
                return IsNullAndEmptyEqual && x.Length == 0 ? 0 : 1;

            if (Pattern != null && PatternFieldComperators != null && PatternFieldComperators.Length > 0)
            {
                var mx = Pattern.Match(x);
                var my = Pattern.Match(y);
                if (mx.Success)
                {
                    if (my.Success)
                    {
                        var i = 0;

                        foreach (var cmp in PatternFieldComperators)
                        {
                            if (++i >= mx.Groups.Count)
                                throw new ArgumentException("There are fewer groups than PatternFieldComparators:" + (mx.Groups.Count - 1) + "," + PatternFieldComperators.Length);
                            if (cmp == null) continue;
                            var diff = cmp(mx.Groups[i].Value, my.Groups[i].Value);
                            if (diff != 0)
                                return diff;
                        }
                        return 0;
                    }
                    return -1;
                }
                if (my.Success)
                    return 1;
            }
            return String.Compare(x, y, ComparisonType);
        }
    }
}
