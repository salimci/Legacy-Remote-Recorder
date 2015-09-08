using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.WebwasherAuditUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class WebWasherAuditUnifiedRecorderUnitTest
    {
        private static RecorderBase _webWasherAuditUnifiedRecorder;

        /// <summary>
        /// Create a WebwasherAuditUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _webWasherAuditUnifiedRecorder = new WebwasherAuditUnifiedRecorder();
        }

        /// <summary>
        /// Clear WebwasherAuditUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _webWasherAuditUnifiedRecorder.Clear();
            _webWasherAuditUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder>("Convert2Date", _webWasherAuditUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If data is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    * String.Empty should return
        /// </summary>
        [Test(Description = "If data is empty")]
        public void Convert2Date_IfDataIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = {String.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, object >("Convert2Date", _webWasherAuditUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "dd/MMM/yyyy:HH:mm:ss zzzzz", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "20/Oct/2010:08:08:10 +0000"
        ///     * data = WebwasherAuditUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return  2010/10/20 08:08:10
        /// </summary>
        [Test(Description = "If date time format is dd/MMM/yyyy:HH:mm:ss zzzzz, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsdd_MMM_yyyy_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            var field = String.Empty;
            string[] values = { "20/Oct/2010:08:08:10 +0000" };
            object data = new WebwasherAuditUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, object>("Convert2Date", _webWasherAuditUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2010/10/20 11:08:10");
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
        ///     * data = WebwasherAuditUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If date time format is not expected, return String.Empty")]
        public void Convert2Date_IfDateTimeFormatIsNotCorrect_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "2014/09/09 08:54:15" };
            object data = new WebwasherAuditUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, object>("Convert2Date", _webWasherAuditUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
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
        [Test(Description = "If OnFieldMatch is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnNextInstructionSkip()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", "[\"]+");
            string field = null;
            var context = new FileLineRecorderContext(_webWasherAuditUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, NextInstruction>("OnFieldMatch", _webWasherAuditUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_webWasherAuditUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, NextInstruction>("OnFieldMatch", _webWasherAuditUnifiedRecorder, new object[] { context, field, match });
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
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, NextInstruction>("OnFieldMatch", _webWasherAuditUnifiedRecorder, new object[] { context, field, match });
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
        [Test(Description = "If OnFieldMatch is success, for wrong input")]
        public void OnFieldMatch_IfMatchIsSuccessForWrongInput_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_webWasherAuditUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, NextInstruction>("OnFieldMatch", _webWasherAuditUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, Regex>("CreateHeaderSeparator", _webWasherAuditUnifiedRecorder, new object[] { });

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

            var expected = new Regex("\\[(?<DATE_TIME>[^\\]]+)\\]\\s*\\[(?<PROTO>[^\\]]+)\\]\\s*(?<USER>[^\\s]+)\\s*(?<KEY>[^\\s]+)\\s*(?<IP>[^\\s]+)\\s*\"(?<ACT>[^\"]+)\"\\s*\"(?<STATUS>[^\"]+)\"\\s*(?<SEVERITY>[^\\s]+)\\s*\"(?<NO_1>[^\"]+)\"\\s*\"(?<NO_2>[^\"]+)\"\\s*(?<PATH>[^\\s]+)\\s*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, Regex>("CreateHeaderSeparator", _webWasherAuditUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, Regex>("CreateFieldSeparator", _webWasherAuditUnifiedRecorder, new object[] { });

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

            var expected = new Regex("\\[(?<DATE_TIME>[^\\]]+)\\]\\s*\\[(?<PROTO>[^\\]]+)\\]\\s*(?<USER>[^\\s]+)\\s*(?<KEY>[^\\s]+)\\s*(?<IP>[^\\s]+)\\s*\"(?<ACT>[^\"]+)\"\\s*\"(?<STATUS>[^\"]+)\"\\s*(?<SEVERITY>[^\\s]+)\\s*\"(?<NO_1>[^\"]+)\"\\s*\"(?<NO_2>[^\"]+)\"\\s*(?<PATH>[^\\s]+)\\s*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, Regex>("CreateFieldSeparator", _webWasherAuditUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
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
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, string>("GetHeaderText", _webWasherAuditUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_webWasherAuditUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_webWasherAuditUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, string>("GetHeaderText", _webWasherAuditUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new WebwasherAuditUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return WebwasherAuditUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return WebwasherAuditUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<WebwasherAuditUnifiedRecorder, RecorderContext>("CreateContextInstance", _webWasherAuditUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
