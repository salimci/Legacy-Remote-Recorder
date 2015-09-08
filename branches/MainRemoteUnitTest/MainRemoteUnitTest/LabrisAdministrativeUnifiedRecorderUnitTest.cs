using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.LabrisAdministrativeUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class LabrisAdministrativeUnifiedRecorderUnitTest
    {
        private static RecorderBase _labrisAdministrativeUnifiedRecorder;

        /// <summary>
        /// Create a LabrisAdministrativeUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _labrisAdministrativeUnifiedRecorder = new LabrisAdministrativeUnifiedRecorder();
        }

        /// <summary>
        /// Clear LabrisAdministrativeUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _labrisAdministrativeUnifiedRecorder.Clear();
            _labrisAdministrativeUnifiedRecorder = null;
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If data is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If data is null")]
        public void Convert2Date_IfDataIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder>("Convert2Date", _labrisAdministrativeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "MMM dd HH:mm:ss yyyy", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09 08:54:15"
        ///     * data = LabrisAdministrativeUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is MMM dd HH:mm:ss yyyy, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsMMM_dd_yyyy_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = { "Jul 22 20:38:14 2010" };
            object data = new LabrisAdministrativeUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, object>("Convert2Date", _labrisAdministrativeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2010/07/22 20:38:14");
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is not expected, return String.Empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = "2014/09/09 08:54:15"
        ///     * data = LabrisAdministrativeUnifiedRecorder
        ///
        ///Expected Output :
        ///	    * String.Empty should return
        /// </summary>
        [Test(Description = "If date time format is not expected, return String.Empty")]
        public void Convert2Date_IfDateTimeFormatIsNotCorrect_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "2014/09/09 08:54:15" };
            object data = new LabrisAdministrativeUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, object>("Convert2Date", _labrisAdministrativeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is wrong for header
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * Expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for header")]
        public void CreateHeaderSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, Regex>("CreateHeaderSeparator", _labrisAdministrativeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for header
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for header")]
        public void CreateHeaderSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("(?<DATE_TIME>.*?)\\s*sshd\\[(?<UNIQUE>[^\\]]+)\\]:\\s*(((?<EVENT_CATEGORY_0>.*?)\\s+for\\s+(?<USER_0>.*?)\\s+from\\s+(?<SOURCE_IP>[^\\s]+)\\s+port\\s+(?<PORT>[^\\s]+)\\s+(?<DESCRIPTION_0>[^\\s]+))|(pam_unix\\((?<DESCRIPTION_1>[^\\)]+)\\):\\s+(?<EVENT_CATEGORY_1>.*?)\\s+for\\s+user\\s+(?<USER_1>[^\\s]+)(\\s+by\\s+\\((?<UID_0>[^\\)]+)\\))?)|(pam_unix\\(sshd:auth\\):\\s*(?<DESCRIPTION_2>[^\\;]+)\\;\\s*logname=(?<LOGNAME>()|.[^\\s]*)\\s+uid=(?<UID_1>[0-9]+)\\s+euid=(?<EUID>[0-9]+)\\s+tty=(?<TTY>[\\w]+)\\s+ruser=(?<RUSER>()|.[^\\s]*)\\s+rhost=(?<RHOST>[0-9\\.]+)\\s+user=(?<USER_2>[\\w]+))|(?<DESCRIPTION_3>.[^\\n]+))");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, Regex>("CreateHeaderSeparator", _labrisAdministrativeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create field separator with regex
        ///
        ///Test Scenario : If regex is wrong for field
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * Expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for field")]
        public void CreateFieldSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, Regex>("CreateFieldSeparator", _labrisAdministrativeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for field
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * Return expected regex
        /// </summary>
        [Test(Description = "If regex is true for field")]
        public void CreateFieldSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("(?<DATE_TIME>.*?)\\s*sshd\\[(?<UNIQUE>[^\\]]+)\\]:\\s*(((?<EVENT_CATEGORY_0>.*?)\\s+for\\s+(?<USER_0>.*?)\\s+from\\s+(?<SOURCE_IP>[^\\s]+)\\s+port\\s+(?<PORT>[^\\s]+)\\s+(?<DESCRIPTION_0>[^\\s]+))|(pam_unix\\((?<DESCRIPTION_1>[^\\)]+)\\):\\s+(?<EVENT_CATEGORY_1>.*?)\\s+for\\s+user\\s+(?<USER_1>[^\\s]+)(\\s+by\\s+\\((?<UID_0>[^\\)]+)\\))?)|(pam_unix\\(sshd:auth\\):\\s*(?<DESCRIPTION_2>[^\\;]+)\\;\\s*logname=(?<LOGNAME>()|.[^\\s]*)\\s+uid=(?<UID_1>[0-9]+)\\s+euid=(?<EUID>[0-9]+)\\s+tty=(?<TTY>[\\w]+)\\s+ruser=(?<RUSER>()|.[^\\s]*)\\s+rhost=(?<RHOST>[0-9\\.]+)\\s+user=(?<USER_2>[\\w]+))|(?<DESCRIPTION_3>.[^\\n]+))");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, Regex>("CreateFieldSeparator", _labrisAdministrativeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is not success
        ///
        ///Known Input :
        ///    * match = !success
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If Match is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnNextInstructionSkip()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", "[\"]+");
            string field = null;
            var context = new FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, NextInstruction>("OnFieldMatch", _labrisAdministrativeUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is success but context does not include sourceheaderinfo
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, NextInstruction>("OnFieldMatch", _labrisAdministrativeUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.NullReferenceException
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is success but context is null
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context is null")]
        public void OnFieldMatch_IfContextIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, NextInstruction>("OnFieldMatch", _labrisAdministrativeUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.NullReferenceException
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is success, for wrong input
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If Match is success, for wrong input")]
        public void OnFieldMatch_IfMatchIsSuccessForWrongInput_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, NextInstruction>("OnFieldMatch", _labrisAdministrativeUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.Collections.Generic.KeyNotFoundException
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is success, for true input
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success, for true input")]
        public void OnFieldMatch_IfMatchIsSuccessForTrueInput_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("May 20 11:32:37 raven-lindev sshd[3092]: Server listening on :: port 22", "(?<DATE>[a-zA-Z]*\\s[0-9]*\\s[0-9\\:]+)\\s+(?<HOST_NAME>[^\\s]+)\\s+(?<EVENT>[^\\[]+)\\s*\\[\\s*(?<ID>[^\\]]+)\\]\\s*:\\s*(?<DESCRIPTION>[^\\n]+)");
            string field = null;
            var context = new FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, NextInstruction>("OnFieldMatch", _labrisAdministrativeUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Return string.Empty
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * context = null
        ///
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetHeaderText_IfContextIsNull_ReturnStringEmpty()
        {
            //Arrange

            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, string>("GetHeaderText", _labrisAdministrativeUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Return string.Empty
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * context = FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_labrisAdministrativeUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, string>("GetHeaderText", _labrisAdministrativeUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new LabrisAdministrativeUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return LabrisAdministrativeUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return LabrisAdministrativeUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<LabrisAdministrativeUnifiedRecorder, RecorderContext>("CreateContextInstance", _labrisAdministrativeUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
