using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Log;
using Parser;

namespace RemoteRecorderTestClass
{
    class Program
    {
        protected static CLogger L;
        public static int TraceLevel = 4;


        static void Main()
        {
            const string dllPath = @"M:\Recorders2011\trunk\Libraries\Base\IISV_7_1_0Recorder.dll";
            //Assembly testAssembly = Assembly.LoadFile(dllPath);
            //Type type = testAssembly.GetType("Parser.IISV_7_1_0Recorder");
            //object instance = Activator.CreateInstance(type);
            //PropertyInfo propertyInfo = type.GetProperty("Text");

            //var textString = (string)type.InvokeMember("WriteData",
            //        BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
            //        null, instance, new object[] { "Command Message." });

            InitializeLogger();
            /*ParserExtended parserExtended = new ParserExtended();
            parserExtended.Test(dllPath);*/
             

            CustomBaseExtended customBaseExtended = new CustomBaseExtended();
            customBaseExtended.Test1(dllPath, L);
            /*
            // Assembly içinde ki bir değişkenin degerini aldım.
            //Console.WriteLine(propertyInfo.GetValue(instance, null).ToString());
            // set value of property: public double Number
            //propertyInfo.SetValue(instance, "Command Message.", null);

            // invoke public instance method: public void Clear()
            //type.InvokeMember("WriteData",
            //    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
            //    null, instance, null);
            */


            Console.WriteLine("Operation Completed.");
            Console.ReadLine();
        }

        public static bool InitializeLogger()
        {
            try
            {
                L = new CLogger();
                L.SetLogLevel((LogLevel)((TraceLevel < 0 || TraceLevel > 4) ? 3 : TraceLevel));
                L.SetLogFile(@"C:\tmp\IISRecorderLog.log");
                L.SetTimerInterval(LogType.FILE, 0);
                L.SetLogFileSize(10000000);
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("RemoteRecorderBase->InitializeLogger() ", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
