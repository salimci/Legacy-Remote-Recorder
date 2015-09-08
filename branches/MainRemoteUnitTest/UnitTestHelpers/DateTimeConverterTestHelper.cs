using System;

namespace Natek.Recorders.Remote.Test.UnitTestHelper
{
    public class DateTimeConverterTestHelper
    {

        public delegate object DateTimeConverter(RecWrapper rec, string field, string[] fieldvalues, object data);


        public static object TestDatetimeConverter<T>(string methodName, string[] datetime)
        {
            var method = MethodTestHelper.GetMethodByName<T>(methodName);

            var methodDelegate = (DateTimeConverter)Delegate.CreateDelegate(typeof (T), method);
            return TestDatetimeConverter<T>(methodDelegate, datetime);
        }

        public static object TestDatetimeConverter<T>(DateTimeConverter dateTimeConverter, string[] datetime)
        {
            var rec = new RecWrapper();
            var field = string.Empty;
            return dateTimeConverter.Invoke(rec, field, datetime, null);
        }
    }
}
