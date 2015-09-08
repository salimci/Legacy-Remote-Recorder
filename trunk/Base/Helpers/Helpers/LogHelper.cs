using System;
using System.Diagnostics;
using Log;

namespace Natek.Helpers.Log {
	public static class LogHelper {
		public static void Log(LogType logType, LogLevel logLevel, string header, string message) {
			try {
				switch (logType) {
					case LogType.EVENTLOG:
						EventLog.WriteEntry(header, message);
						break;
					case LogType.CONSOLE:
						Console.Error.WriteLine(header + " : (" + logLevel + ") " + message);
						break;
				}
			} catch {
			}
		}

		public static void Log(CLogger logger, LogType logType, LogLevel logLevel, string header, string message) {
			try {
				if (logger != null) {
					logger.Log(logType, logLevel, header + " : " + message);
					return;
				}
			} catch {
			}
			Log(logType, logLevel, header, message);
		}

		public static void Log(CLogger logger, LogType logType, LogLevel logLevel, string header, string message, LogType defaultLogType) {
			try {
				if (logger != null) {
					logger.Log(logType, logLevel, header + " : " + message);
					return;
				}
			} catch {
			}
			Log(logger, logType, logLevel, header, message);
		}
	}
}
