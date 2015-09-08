using System;
using System.Text.RegularExpressions;

namespace Natek.Helpers.Config.Relocated {
    //TODO: This class has been moved here till recorder is converted
    // to new format. After conversion the same class in Helpers.dll should be used
	public static class ConfigHelper {
		public static readonly Regex RegKeywordValue = new Regex("([,;])|([ \t]+)|((\"((\\\\\"|[^\"])*)\"|((\\\\.|[^=,;])+)|([^=;,]*))=((\"(\\\\\"|[^\"])*)\"|((\\\\,|\\\\;|[^,;])*)))",
																 RegexOptions.Compiled);
		public static readonly Regex RegEscape = new Regex("\\\\(.)", RegexOptions.Compiled);

		public delegate bool OnKeywordValue(string keyword, bool keywordError, bool quotedKeyword,
			string value, bool valueError, bool quotedValue, ref Exception error);

		public delegate bool OnSeparator(string separator, ref Exception error);

		public delegate bool OnWhitespace(string ws, ref Exception error);

		public static bool ParseKeywords(string keywords,
			OnKeywordValue onKeywordValue,
			OnSeparator onSeparatorValue,
			OnWhitespace onWhitespace, ref Exception error) {

			try {
				if (string.IsNullOrEmpty(keywords))
					return true;
				var m = RegKeywordValue.Match(keywords);
				var lastIndex = 0;
				while (m.Success) {
					if (m.Index > lastIndex) {
						var tmp = Escape(keywords.Substring(lastIndex, m.Index - lastIndex));
						if (onKeywordValue != null
							&& !onKeywordValue(tmp, true, tmp.StartsWith("\""), null, true, false, ref error))
							return false;
					}
					if (m.Groups[3].Success) {
						if (onKeywordValue != null
							&& !onKeywordValue(Escape(m.Groups[5].Success
												   ? m.Groups[5].Value
												   : (m.Groups[7].Success ? m.Groups[7].Value : m.Groups[9].Value)), m.Groups[9].Success,
											   m.Groups[5].Success, Escape(m.Groups[11].Success ? m.Groups[11].Value : m.Groups[13].Value), false,
											   m.Groups[11].Success, ref error))
							return false;
					} else if (m.Groups[1].Success) {
						if (onSeparatorValue != null && !onSeparatorValue(m.Groups[1].Value, ref error))
							return false;
					} else if (onWhitespace != null && !onWhitespace(m.Groups[2].Value, ref error))
						return false;
					lastIndex = m.Index + m.Length;
					m = m.NextMatch();
				}
				if (lastIndex < keywords.Length) {
					var tmp = Escape(keywords.Substring(lastIndex, keywords.Length - lastIndex));
					if (onKeywordValue != null
						&& !onKeywordValue(tmp, true, tmp.StartsWith("\""), null, true, false, ref error))
						return false;
				}
			} catch (Exception e) {
				error = e;
			}
			return false;
		}

		public static string Escape(string str) {
			if (string.IsNullOrEmpty(str))
				return str;

			return RegEscape.Replace(str, (m => {
				switch (m.Groups[1].Value) {
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
