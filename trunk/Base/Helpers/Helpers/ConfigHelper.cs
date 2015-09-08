using System;
using System.Text.RegularExpressions;

namespace Natek.Helpers.Config
{
    public static class ConfigHelper
    {
        public static readonly Regex RegKeywordValue = new Regex("([;])|([ \t]+)|((\"((\\\\\\\\|\\\\\"|[^\"])*)\"|((\\\\.|[^=;])+)|([^=;]*))=(\"((\\\\\\\\|\\\\\"|[^\"])*)\"|((\\\\,|\\\\;|[^;])*)))",
                                                                 RegexOptions.Compiled | RegexOptions.Multiline);
        public static readonly Regex RegEscape = new Regex("\\\\(.)", RegexOptions.Compiled);

        public delegate bool OnKeywordValue(string keyword, bool quotedKeyword,
            string value, bool quotedValue, ref int touchCount, ref Exception error);

        public delegate bool OnUnhandledKeywordValue(string keyword, bool quotedKeyword,
            string value, bool quotedValue, bool keywordValueError, ref int touchCount, ref Exception error);

        public delegate bool OnSeparator(string separator, ref Exception error);

        public delegate bool OnWhitespace(string ws, ref Exception error);

        public static bool ParseKeywords(string keywords,
            OnKeywordValue onKeywordValue,
            OnSeparator onSeparatorValue,
            OnWhitespace onWhitespace,
            OnUnhandledKeywordValue onUnhandledKeywordValue,
            ref Exception error)
        {

            try
            {
                if (string.IsNullOrEmpty(keywords))
                    return true;
                var m = RegKeywordValue.Match(keywords);
                var lastIndex = 0;
                while (m.Success)
                {
                    if (m.Index > lastIndex)
                    {
                        var tmp = Unescape(keywords.Substring(lastIndex, m.Index - lastIndex));
                        if (onKeywordValue != null)
                        {
                            var touchCount = 0;
                            if (onKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, ref touchCount, ref error))
                            {
                                onUnhandledKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, true, ref touchCount,
                                    ref error);
                            }
                            else
                            {
                                onUnhandledKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, true, ref touchCount,
                                    ref error);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (m.Groups[3].Success)
                    {
                        if (onKeywordValue != null)
                        {
                            var touchCount = 0;

                             if (onKeywordValue(Unescape(m.Groups[5].Success
                                ? m.Groups[5].Value
                                : (m.Groups[7].Success ? m.Groups[7].Value : m.Groups[9].Value)),
                                m.Groups[5].Success,
                                Unescape(m.Groups[11].Success ? m.Groups[11].Value : m.Groups[13].Value),
                                m.Groups[12].Success, ref touchCount, ref error))
                            {
                                if (touchCount == 0)
                                {
                                    onUnhandledKeywordValue(
                                        Unescape(m.Groups[5].Success
                                            ? m.Groups[5].Value
                                            : (m.Groups[7].Success ? m.Groups[7].Value : m.Groups[9].Value)),
                                        m.Groups[5].Success,
                                        Unescape(m.Groups[11].Success ? m.Groups[11].Value : m.Groups[13].Value),
                                        m.Groups[12].Success, m.Groups[9].Success, ref touchCount, ref error);
                                }
                            }
                            else
                            {
                                onUnhandledKeywordValue(
                                    Unescape(m.Groups[5].Success
                                        ? m.Groups[5].Value
                                        : (m.Groups[7].Success ? m.Groups[7].Value : m.Groups[9].Value)),
                                    m.Groups[5].Success,
                                    Unescape(m.Groups[11].Success ? m.Groups[11].Value : m.Groups[13].Value),
                                    m.Groups[12].Success, true, ref touchCount, ref error);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (m.Groups[1].Success)
                    {
                        if (onSeparatorValue != null && !onSeparatorValue(m.Groups[1].Value, ref error))
                            return false;
                    }
                    else if (onWhitespace != null && !onWhitespace(m.Groups[2].Value, ref error))
                        return false;
                    lastIndex = m.Index + m.Length;
                    m = m.NextMatch();
                }
                if (lastIndex < keywords.Length)
                {
                    var tmp = Unescape(keywords.Substring(lastIndex, keywords.Length - lastIndex));
                    if (onKeywordValue != null)
                    {
                        var touchCount = 0;
                        if (onKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, ref touchCount, ref error))
                        {
                            onUnhandledKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, true, ref touchCount,
                                ref error);
                        }
                        else
                        {
                            onUnhandledKeywordValue(tmp, tmp.StartsWith("\""), string.Empty, false, true, ref touchCount,
                                ref error);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public static readonly Regex RegEscapeString = new Regex("([\\\\\"])", RegexOptions.Compiled);

        public static string Escape(string str)
        {
            return string.IsNullOrEmpty(str) ? str : RegEscape.Replace(str, m => "\\" + m.Groups[0].Value);
        }

        public static string Unescape(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return RegEscape.Replace(str, (m =>
            {
                switch (m.Groups[1].Value)
                {
                    case "t":
                        return "\t";
                    case "b":
                        return "\b";
                    case "n":
                        return "\n";
                    case "r":
                        return "\r";
                    case "\\":
                        return "\\";
                }
                return m.Groups[1].Value;
            }));
        }
    }
}
