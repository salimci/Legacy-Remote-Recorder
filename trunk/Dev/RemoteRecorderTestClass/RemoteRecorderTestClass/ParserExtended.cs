using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CustomTools;

namespace Parser
{
    class ParserExtended : Parser
    {
        public void Test(string dllPath)
        {
            try
            {
                Assembly testAssembly = Assembly.LoadFile(dllPath);
                Type type = testAssembly.GetType("Parser.IISV_7_1_0Recorder");
                object instance = Activator.CreateInstance(type);

                CustomBase customBase = new CustomBase();
                Type myType = typeof(CustomBase);

                string iisLogFileLocation = @"C:\tmp\";

                PropertyInfo propertyInfo = type.GetProperty("Text");
                PropertyInfo propertyInfoDir = type.GetProperty("Dir");
                FieldInfo[] myFields = myType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine("\nDisplaying the values of the fields of {0}.\n",myType);
                for (int i = 0; i < myFields.Length; i++)
                {
                    Console.WriteLine("The value of {0} is: {1}",
                        myFields[i].Name, myFields[i].GetValue(instance));
                }
                propertyInfoDir.SetValue(instance, iisLogFileLocation, null);

                var Init =
                    (string)
                    type.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null,
                                      instance, null);

                Console.WriteLine("Dir: ");
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }
    }
}
