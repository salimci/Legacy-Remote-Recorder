using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Natek.Helpers
{
    public class DateCompareHelper
    {
        /// <summary>
        /// Create day list according to culture name and abbrevited
        /// </summary>
        /// <param name="nslName">Culture name according to NSL i.e. "en-GB" for GB English or "tr-TR" for Turkish (if value is empty it use currentCulture)</param>
        /// <param name="indexMapping">last keyword index map</param>
        /// <returns>Day name index</returns>
        public static Dictionary<string, int> CreateDayIndex(string nslName = null, string indexMapping = null)
        {
            var dayNames = new Dictionary<string, int>();
            var regSplit = new Regex(@"\s*([\w]*)\s*:\s*([0-9]*)\s*", RegexOptions.Compiled);
            if (!string.IsNullOrEmpty(indexMapping))
            {
                var match = regSplit.Match(indexMapping);

                while (match.Success)
                {
                    dayNames.Add(match.Groups[1].Value.ToUpper(), int.Parse(match.Groups[2].Value));
                    match = match.NextMatch();
                }
            }

            var dateTimeFormatInfo = string.IsNullOrEmpty(nslName)
                    ? CultureInfo.CurrentCulture.DateTimeFormat
                    : CultureInfo.GetCultureInfo(nslName).DateTimeFormat;
                var days = new List<string>();
                days.AddRange(Array.ConvertAll(dateTimeFormatInfo.AbbreviatedDayNames, d => d.ToUpperInvariant()));
                days.AddRange(Array.ConvertAll(dateTimeFormatInfo.DayNames, d => d.ToUpperInvariant()));

                foreach (var day in days)
                {
                    if(!dayNames.ContainsKey(day))
                        dayNames.Add(day, days.IndexOf(day));
                }
            

            return dayNames;
        }

        /// <summary>
        /// Compare two day of week according their name
        /// </summary>
        /// <param name="left">left part of the compare</param>
        /// <param name="right">right part of the compare</param>
        /// <param name="dayNames">Ordered day name dictionary for comparetion </param>
        /// <param name="ordinalIgnoreCase">File ordinal ignore case</param>
        /// <returns>Diff value</returns>
        public static int CompareDateTimeDay(string left, string right, Dictionary<string, int> dayNames, bool ordinalIgnoreCase)
        {
            int leftIndex;
            int rightIndex;
            return (dayNames.TryGetValue(left.ToUpper(), out leftIndex) && dayNames.TryGetValue(right.ToUpper(), out rightIndex)) ? leftIndex - rightIndex
                : string.Compare(left, right, ordinalIgnoreCase);
        }
    }
}
