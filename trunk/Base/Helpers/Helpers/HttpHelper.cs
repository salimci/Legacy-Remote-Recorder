using System;
using System.Web;

namespace Natek.Helpers {
	public static class HttpHelper {
		public static string UrlDecode(string url, bool throwException = false) {
			try {
				return HttpUtility.UrlDecode(url);
			} catch (Exception) {
				if (throwException)
					throw;
			}
			return url;
		}
	}
}
