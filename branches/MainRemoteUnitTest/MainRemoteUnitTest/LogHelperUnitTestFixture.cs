using Log;
using NUnit.Framework;
using Natek.Helpers.Log;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class LogHelperUnitTestFixture
    {

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the logtype write header and message where 2 parameters this method
        ///
        ///Test Scenario : If Log parameters are null
        ///
        ///Known Input :
        ///             const LogType logtype = new LogType()
        ///             const LogLevel loglevel = new LogLevel()
        ///             string header = null
        ///             string message = null
        /// 
        ///Expected Output :
        ///             * No Return
        /// </summary>
        [Test(Description = "If Log parameters are null")]
        public void Log_IfParameterAreNull_NotReturn()
        {
            //Arrange
            const LogType logtype = new LogType();
            const LogLevel loglevel = new LogLevel();
            string header = null;
            string message = null;
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
                LogHelper.Log(logtype, loglevel, header, message);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            
        }

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the logtype write header and message where 2 parameters this method
        ///
        ///Test Scenario : If Log parameters are not null
        ///
        ///Known Input :
        ///            const LogType logtype = new LogType();
        ///            LogType.EVENTLOG.Equals(logtype);
        ///            const LogLevel loglevel = new LogLevel();
        ///            LogLevel.NONE.Equals(loglevel);
        ///            var header = "lorem ipsum";
        ///            var message = "sit amet";
        ///
        ///Expected Output :
        ///              * No Return   
        /// </summary>
        [Test(Description = "If Log parameters are not null")]
        public void Log_IfParameterAreNotNull_NotReturn()
        {
            //Arrange
            const LogType logtype = new LogType();
            LogType.EVENTLOG.Equals(logtype);
            const LogLevel loglevel = new LogLevel();
            LogLevel.NONE.Equals(loglevel);

            var header = "lorem ipsum";
            var message = "sit amet";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            LogHelper.Log(logtype, loglevel, header, message);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the Clogger write header and message where 2 parameters this method
        ///
        ///Test Scenario : If Log parameters are null
        ///
        ///Known Input :
        ///            * const CLogger logger = null;
        ///            * const LogType logtype = new LogType();
        ///            * const LogLevel loglevel = new LogLevel();
        ///            * string header = null;
        ///            * string message = null;
        ///
        ///
        ///Expected Output :
        ///                 * No Return
        /// </summary>
        [Test(Description = "If Log parameters are null")]
        public void Log_IfParameterAreNullForClogger_NotReturn()
        {
            //Arrange
            const CLogger logger = null;
            const LogType logtype = new LogType();
            const LogLevel loglevel = new LogLevel();
            string header = null;
            string message = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            LogHelper.Log(logger,logtype, loglevel, header, message);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the Clogger write header and message where 2 parameters this method
        ///
        ///Test Scenario : If Log parameters are not null
        ///
        ///Known Input :
        ///          * const CLogger logger = null;
        ///          * const LogType logtype = new LogType();
        ///          * const LogLevel loglevel = new LogLevel();
        ///          * var header = "lorem ipsum";
        ///          * var message = "sit amet";
        ///
        ///
        ///Expected Output :
        ///                 * No Return
        /// </summary>
        [Test(Description = "If Log parameters are not null")]
        public void Log_IfParameterAreNotNullForClogger_NotReturn()
        {
            //Arrange
            const CLogger logger = null;
            const LogType logtype = new LogType();
            const LogLevel loglevel = new LogLevel();
            var header = "lorem ipsum";
            var message = "sit amet";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            LogHelper.Log(logger, logtype, loglevel, header, message);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the Clogger write header and message where 2 parameters this method and extra logtype parameter
        ///
        ///Test Scenario : If Log parameters are null
        ///
        ///Known Input :
        ///             const CLogger logger = null;
        ///             const LogType logtype = new LogType();
        ///             const LogLevel loglevel = new LogLevel();
        ///             string header = null;
        ///             string message = null;
        ///
        ///Expected Output :
        ///                 * No Return
        /// </summary>
        [Test(Description = "If Log parameters are null")]
        public void Log_IfParameterAreNullForLogType_NotReturn()
        {
            //Arrange
            const CLogger logger = null;
            const LogType logtype = new LogType();
            const LogLevel loglevel = new LogLevel();
            string header = null;
            string message = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            LogHelper.Log(logger, logtype, loglevel, header, message, logtype);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }

        /// <summary>
        /// Method Name : Log
        ///
        ///Method Description : The method according to the Clogger write header and message where 2 parameters this method and extra logtype parameter
        ///
        ///Test Scenario : If Log parameters are not null
        ///
        ///Known Input :
        ///             const CLogger logger = null;
        ///             const LogType logtype = new LogType();
        ///             const LogLevel loglevel = new LogLevel();
        ///             var header = "lorem ipsum";
        ///             var message = "sit amet";
        ///
        ///Expected Output :
        ///                 * No Return
        /// </summary>
        [Test(Description = "If Log parameters are not null")]
        public void Log_IfParameterAreNotNullForLogType_NotReturn()
        {
            //Arrange
            const CLogger logger = null;
            const LogType logtype = new LogType();
            const LogLevel loglevel = new LogLevel();
            var header = "lorem ipsum";
            var message = "sit amet";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            LogHelper.Log(logger, logtype, loglevel, header, message, logtype);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }

    }
}
