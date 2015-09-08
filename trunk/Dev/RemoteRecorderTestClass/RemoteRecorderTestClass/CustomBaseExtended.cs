using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using CustomTools;
using Log;
using Microsoft.Win32;

namespace RemoteRecorderTestClass
{
    class CustomBaseExtended : CustomBase
    {

        public override void SetConfigData(int Identity, string Location,
            string LastLine, string LastPosition, string LastFile,
            string LastKeywords, bool FromEndOnLoss, int MaxLineToWait,
            string User, string Password, string RemoteHost, int SleepTime,
            int TraceLevel, string CustomVar1, int CustomVar2,
            string Virtualhost, string Dal, int Zone)
        {
            Location = @"C:\tmp\a.txt";
            SleepTime = 100;
            User = "Administrator";
            Password = "Password12345";
            LastFile = "";
            LastLine = "";
            LastPosition = "0";
            Location = @"C:\tmp\";
        }

        public string dateFormat = "yyyy-MM-dd HH:mm:ss";
        public void Test(string dllPath)
        {

            Assembly testAssembly = Assembly.LoadFile(dllPath);
            Type type = testAssembly.GetType("Parser.IISV_7_1_0Recorder");
            object instance = Activator.CreateInstance(type);

            string iisLogFileLocation = @"C:\tmp\";

            PropertyInfo propertyInfo = type.GetProperty("Text");
            PropertyInfo propertyInfoDir = type.GetProperty("text");

            //propertyInfoDir.SetValue(instance, iisLogFileLocation, null);

            var Init =
                (string)
                type.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null,
                                  instance, null);

            Console.WriteLine("Dir: ");
            Console.ReadLine();

        }

        public void Test1(string dllPath, CLogger logger)
        {
            Assembly testAssembly = Assembly.LoadFile(dllPath);
            Type type = testAssembly.GetType("Parser.IISV_7_1_0Recorder");
            var instance = (CustomBase)Activator.CreateInstance(type);

            TestRecorder testRecorder = new TestRecorder();
            
            //instance.GetInstanceListService()["Security Manager Remote Recorder"] = new CustomServiceBase();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");

            string iisLogFileLocation = @"C:\tmp\iisLogs\";


            //Assembly içindeki erişilen tüm varieble'ların değerlerini gösterir.
            /*
            FieldInfo[] myFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine(type);

            for (int i = 0; i < myFields.Length; i++)
            {
                Console.WriteLine("The value of {0} is: {1}",
                    myFields[i].Name, myFields[i].GetValue(instance));
            }
             * */

            /*FieldInfo fieldInfoDir = type.GetField("Dir", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoDir.SetValue(instance, iisLogFileLocation);
            Console.WriteLine("Dir. {0}", fieldInfoDir.GetValue(instance));*/

            /* FieldInfo fieldInfoUsingRegistry = type.GetField("usingRegistry", BindingFlags.NonPublic | BindingFlags.Instance);
             fieldInfoUsingRegistry.SetValue(instance, false);
             Console.WriteLine("usingRegistry.{0}", fieldInfoUsingRegistry.GetValue(instance));*/

            /*FieldInfo fieldInfoLogFileLocation = type.GetField("logFileLocation", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoLogFileLocation.SetValue(instance, @"C:\tmp\iis.log");
            Console.WriteLine("fieldInfoLogFileLocation.{0}", fieldInfoLogFileLocation.GetValue(instance));*/

            /*FieldInfo fieldInfoLogFileLocation = type.GetField("Log", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoLogFileLocation.SetValue(instance, logger);
            Console.WriteLine("fieldInfoLogFileLocation.{0}", fieldInfoLogFileLocation.GetValue(instance));*/

            //readMethod
            //threadSleepTime
            //dontSend
            /*
            FieldInfo fieldInfodontSend = type.GetField("dontSend", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfodontSend.SetValue(instance, true);
            Console.WriteLine("threadSleepTime:'{0}'", fieldInfodontSend.GetValue(instance));
            */

            //FieldInfo fieldInfoTEST_ACTIVE = type.GetField("TEST_ACTIVE", BindingFlags.NonPublic | BindingFlags.Instance);
            //fieldInfoTEST_ACTIVE.SetValue(instance, testRecorder.GetInstanceService("Security Manager Remote Recorder"));

            FieldInfo fieldInfologFileSize = type.GetField("logFileSize", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfologFileSize.SetValue(instance, (uint)10000000);
            Console.WriteLine("LastKeywords:'{0}'", fieldInfologFileSize.GetValue(instance));


            FieldInfo fieldInfoLastKeywords = type.GetField("lastKeywords", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoLastKeywords.SetValue(instance, "");
            Console.WriteLine("LastKeywords:'{0}'", fieldInfoLastKeywords.GetValue(instance));

            DateTime dt = DateTime.Now;
            string dateTime = dt.ToString(dateFormat);
            FieldInfo fieldInfoLastRecDate = type.GetField("LastRecDate", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoLastRecDate.SetValue(instance, dateTime);
            Console.WriteLine("LastRecDate:'{0}'", fieldInfoLastRecDate.GetValue(instance));

            FieldInfo fieldInfothreadSleepTime = type.GetField("threadSleepTime", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfothreadSleepTime.SetValue(instance, 10);
            Console.WriteLine("threadSleepTime:'{0}'", fieldInfothreadSleepTime.GetValue(instance));

            FieldInfo fieldInfoRemoteHost = type.GetField("remoteHost", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoRemoteHost.SetValue(instance, "");
            Console.WriteLine("RemoteHost:'{0}'", fieldInfoRemoteHost.GetValue(instance));

            FieldInfo fieldInfoPosition = type.GetField("Position", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoPosition.SetValue(instance, 0);
            Console.WriteLine("Position:'{0}'", fieldInfoPosition.GetValue(instance));

            FieldInfo fieldInfoUser = type.GetField("user", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoUser.SetValue(instance, "Administrator");
            Console.WriteLine("User:'{0}'", fieldInfoUser.GetValue(instance));

            FieldInfo fieldInfoSleepTime = type.GetField("_SleepTime", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoSleepTime.SetValue(instance, 10);
            Console.WriteLine("_SleepTime:'{0}'", fieldInfoSleepTime.GetValue(instance));

            FieldInfo fieldInfoPassword = type.GetField("password", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfoPassword.SetValue(instance, "");
            Console.WriteLine("Password:'{0}'", fieldInfoPassword.GetValue(instance));

            Console.WriteLine("Dir. '{0}'", FieldSetNewValue("Dir", type, instance, iisLogFileLocation));
            Console.WriteLine("UsingRegistry.'{0}'", FieldSetNewValue("usingRegistry", type, instance, false));

            RegistryKey regIn = Registry.LocalMachine.OpenSubKey("Software\\NATEK\\Security Manager\\Remote Recorder");
            string home = (String)regIn.GetValue("Home Directory");

            Console.WriteLine("home: {0}", home);
            Console.ReadLine();

            var Init =
                (string)
                    type.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null,
                                  instance, null);

            var Start =
                (string)
                    type.InvokeMember("Start", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null,
                                  instance, null);

            //Start


        }

        public object FieldSetNewValue(string fieldName, Type type, object instance, object newValue)
        {
            object o;
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(instance, newValue);
                return fieldInfo.GetValue(instance);
            }
            return null;
        } // FieldSetNewValue
    }
}
