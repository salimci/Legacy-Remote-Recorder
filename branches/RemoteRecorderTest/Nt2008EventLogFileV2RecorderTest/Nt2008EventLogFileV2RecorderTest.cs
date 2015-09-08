using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using MicrosoftEventing;
using System.Globalization;

namespace Nt2008EventLogFileV2RecorderTest
{
    public class Nt2008EventLogFileV2RecorderTest
    {


        private static void Main()
        {
            Console.WriteLine(CultureInfo.GetCultureInfo("tr-TR").LCID);
            Console.WriteLine(CultureInfo.GetCultureInfo("en-US").LCID);
            using (var fs = new StreamWriter(@"O:\\tmp\\evtLog.txt"))
            {
                var metaLookup = new Dictionary<string, EventLogHandle>();
                foreach (var evt in new[] { "Application", "Security", "Setup", "System" })
                {
                    var dt = DateTime.Now;
                    try
                    {
                        foreach (var record in EventLogHelper.EnumerateRecords(evt, 1033, metaLookup, "*[System/EventRecordID > 11135]", null, null, null, null))
                        {
                            PrintStat(++total, 10);
                        }
                    }
                    finally
                    {
                        Console.WriteLine(DateTime.Now.Subtract(dt).TotalMilliseconds);
                    }
                }
            }
        }






        private static void WriteRecord(TextWriter writer, Dictionary<string, object> record)
        {
            foreach (var kv in record.Where(k => k.Key.StartsWith("EvtFormatMessage")))
            {
                writer.Write("[{0}]=[{1}] ", kv.Key, kv.Value);
            }
            object _v;
            NativeWrapper.SystemProperties sysProp;
            if (record.TryGetValue("SystemProperties", out _v) && (sysProp = _v as NativeWrapper.SystemProperties) != null)
            {
                foreach (var prop in typeof(NativeWrapper.SystemProperties).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    writer.Write("[{0}]=[{1}] ", prop.Name, prop.GetValue(sysProp, null));

                foreach (var prop in typeof(NativeWrapper.SystemProperties).GetFields(BindingFlags.Instance | BindingFlags.Public))
                    writer.Write("[{0}]=[{1}] ", prop.Name, prop.GetValue(sysProp));

            }
            IList<object> userData;
            if (record.TryGetValue("UserData", out _v) && (userData = _v as IList<object>) != null)
            {
                foreach (var u in userData)
                {
                    writer.Write(" [Item]=[{0}]", u);
                }
            }
            writer.WriteLine();
        }

        static DateTime dt = DateTime.Now;
        static int total = 0;
        private static void PrintStat(int pr, int len)
        {
            if (DateTime.Now.Subtract(dt).TotalMilliseconds < 40)
                return;
            for (var i = 0; i < len; i++)
                Console.Write("\b \b");
            Console.Write("{0," + len + "}", pr);
            dt = DateTime.Now;
        }
    }
}
