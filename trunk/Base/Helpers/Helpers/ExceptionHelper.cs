using System;

namespace Natek.Helpers
{
    /// <summary>
    /// Manage the exception handling mechanism
    /// </summary>
    public static class ExceptionHelper
    {
        public delegate void CatchSenario<T>(T e);
        public delegate R ExceptionableMethod<R>(params object[] args);
        public delegate void ExceptionableMethod(params object[] args);

        /// <summary>
        /// Exception handling catch method for handling the try/catch block
        /// </summary>
        /// <typeparam name="TR">Return type</typeparam>
        /// <typeparam name="T">Exception type</typeparam>
        /// <param name="actualMethod">Actual method that will call</param>
        /// <param name="catchSenario">Method will use for catch</param>
        /// <returns>Actual method's return Value</returns>
        public static TR Catch<TR, T>(ExceptionableMethod<TR> actualMethod, CatchSenario<T> catchSenario = null) where T : Exception
        {
            if (actualMethod == null) return default (TR);
            
            try
            {
                return actualMethod.Invoke();
            }
            catch(Exception e)
            {
                if (!(e is T)) throw;

                if (catchSenario != null)
                    catchSenario.Invoke(e as T);
            }

            return default (TR);
        }

        /// <summary>
        /// Exception handling catch method overload for handling the try/catch block for void return.
        /// </summary>
        /// <typeparam name="T">Exception type</typeparam>
        /// <param name="actualMethod">Actual method that will call</param>
        /// <param name="catchSenario">Method will use for catch</param>
        public static void Catch<T>(ExceptionableMethod actualMethod, CatchSenario<T> catchSenario = null) where T : Exception
        {
            if (actualMethod == null) return;

            try
            {
                actualMethod.Invoke();
            }
            catch (Exception e)
            {
                if (!(e is T)) throw;

                if (catchSenario != null)
                    catchSenario.Invoke(e as T);
            }
        }
    }
}
