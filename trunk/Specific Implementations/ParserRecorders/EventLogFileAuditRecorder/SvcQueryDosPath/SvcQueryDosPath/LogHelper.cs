using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Natek.Helpers.Log
{
    public static class LogHelper
    {
        private static Dictionary<string, object> syncFile;
        static LogHelper()
        {
            syncFile = new Dictionary<string, object>();
        }

        static object GetSync(string file)
        {
            lock (syncFile)
            {
                object sync;
                if (!syncFile.TryGetValue(file, out sync))
                {
                    sync = new object();
                    syncFile[file] = sync;
                }
                return sync;
            }
        }
        public static void Log(EventLogEntryType logEntryType, DateTime logTime, string header, string format,
                               params object[] parameters)
        {
            try
            {
                EventLog.WriteEntry(Process.GetCurrentProcess().ProcessName,
                                    string.Format("{0}: {1} : {2}",
                                                  logTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                                  header, string.Format(format, parameters)), logEntryType);
            }
            catch
            {
            }
        }

        public static void Log(string logFile, int maxLogSizeKb, DateTime logTime, string header, EventLogEntryType? eventLogEntryType, string format, params object[] parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(logFile))
                {
                    if (eventLogEntryType.HasValue)
                        Log(eventLogEntryType.Value, logTime, header, format, parameters);
                }
                else
                {
                    lock (GetSync(logFile))
                    {
                        var log = string.Format("{0}: {1} : {2}",
                                         logTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), header,
                            string.Format(format, parameters));
                        var info = new FileInfo(logFile);

                        using (var fs = new StreamWriter(logFile, info.Length + log.Length <= maxLogSizeKb * 1024, Encoding.UTF8))
                        {
                            fs.WriteLine(log);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
