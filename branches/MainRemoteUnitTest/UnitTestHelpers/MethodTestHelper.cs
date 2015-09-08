using System;
using System.Reflection;

namespace Natek.Recorders.Remote.Test.UnitTestHelper
{
    public class MethodTestHelper
    {

        public static void RunInstanceMethod<T>(string methodName, object objectInstance, object[] methodParams)
        {
            const BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            RunMethod(typeof(T), methodName, objectInstance, methodParams, eFlags);
        }

        public static TR RunInstanceMethod<T, TR>(string methodName, object objectInstance, object[] methodParams)
        {
            const BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            return RunMethod<TR>(typeof(T), methodName, objectInstance, methodParams, eFlags);
        }

        private static void RunMethod(IReflect type, string methodName, object objectInstance, object[] methodParams, BindingFlags eFlags)
        {
            var methodInfo = type.GetMethod(methodName, eFlags);

            if (methodInfo == null)
            {
                throw new ArgumentException("There is no method '" + methodName + "' for type '" + type + "'.");
            }

            try
            {
                methodInfo.Invoke(objectInstance, methodParams);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        private static TR RunMethod<TR>(IReflect type, string methodName, object objectInstance, object[] methodParams, BindingFlags eFlags)
        {
            var methodInfo = type.GetMethod(methodName, eFlags);

            if (methodInfo == null)
            {
                throw new ArgumentException("There is no method '" + methodName + "' for type '" + type + "'.");
            }

            TR returnObject;

            try
            {
                returnObject = (TR)methodInfo.Invoke(objectInstance, methodParams);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }

            return returnObject;
        }

        public static MethodInfo GetMethodByName<T>(string methodName)
        {
            const BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var type = typeof (T);

            return type.GetMethod(methodName, eFlags);
        }
    }
}
