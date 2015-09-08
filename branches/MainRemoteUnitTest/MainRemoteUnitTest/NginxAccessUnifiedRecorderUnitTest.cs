using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.NginxAccessUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class NginxAccessUnifiedRecorderUnitTest
    {
        private static RecorderBase _nginxAccessUnifiedRecorder;

        /// <summary>
        /// Create a NginxAccessUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _nginxAccessUnifiedRecorder = new NginxAccessUnifiedRecorder();
        }

        /// <summary>
        /// Clear NginxAccessUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _nginxAccessUnifiedRecorder.Clear();
            _nginxAccessUnifiedRecorder = null;
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If fieldvalues is null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If fieldvalues is null")]
        public void Convert2Date_IfFieldValuesIsNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder>("Convert2Date", _nginxAccessUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If fieldvalues is empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If fieldvalues is empty")]
        public void Convert2Date_IfFieldValuesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { String.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, object>("Convert2Date", _nginxAccessUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is wrong
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = "11/09/2014 10:32:05"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If date time format is wrong")]
        public void Convert2Date_IfDateTimeFormatIsWrong_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { "11/09/2014 10:32:05" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, object>("Convert2Date", _nginxAccessUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is true
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = "23/Aug/2010:03:50:59 +0000"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "2010/08/23 06:50:59" should return
        /// </summary>
        [Test(Description = "If date time format is true")]
        public void Convert2Date_IfDateTimeFormatIsTrue_ReturnYYYY_MM_dd()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { "23/Aug/2010:03:50:59 +0000" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, object>("Convert2Date", _nginxAccessUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2010/08/23 06:50:59");
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
        ///    * Return NextInstruction.Return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_nginxAccessUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxAccessUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is not success
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnSkip()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", "\\[+");
            string field = null;
            var context = new FileLineRecorderContext(_nginxAccessUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxAccessUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
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
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNull_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxAccessUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
            var context = new FileLineRecorderContext(_nginxAccessUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxAccessUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.Collections.Generic.KeyNotFoundException
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
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, string>("GetHeaderText", _nginxAccessUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_nginxAccessUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_nginxAccessUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, string>("GetHeaderText", _nginxAccessUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
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
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, Regex>("CreateHeaderSeparator", _nginxAccessUnifiedRecorder, new object[] { });

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

            var expected = new Regex("(?<REMOTE_ADDRESS>[^\\s]+)\\s+(?<HYPHEN>[^\\s]+)\\s+(?<REMOTE_USER>[^\\s]+)\\s+\\[(?<TIME_LOCAL>[^\\]]+)\\s*\\]\\s*\"(?<METHOD>[^\\s]+)\\s+(?<URI>[^\\s]+)\\s+(?<PROTO>[^\"]+)\"\\s*(?<STATUS>[0-9]+)\\s*(?<BODY_BYTES_SENT>[0-9]+)\\s*\"(?<HTTP_REFERER>.[^\"]+)\"\\s*\"(?<HTTP_USER_AGENT>.[^\"]+)\"\\s*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, Regex>("CreateHeaderSeparator", _nginxAccessUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, Regex>("CreateFieldSeparator", _nginxAccessUnifiedRecorder, new object[] { });

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

            var expected = new Regex("(?<REMOTE_ADDRESS>[^\\s]+)\\s+(?<HYPHEN>[^\\s]+)\\s+(?<REMOTE_USER>[^\\s]+)\\s+\\[(?<TIME_LOCAL>[^\\]]+)\\s*\\]\\s*\"(?<METHOD>[^\\s]+)\\s+(?<URI>[^\\s]+)\\s+(?<PROTO>[^\"]+)\"\\s*(?<STATUS>[0-9]+)\\s*(?<BODY_BYTES_SENT>[0-9]+)\\s*\"(?<HTTP_REFERER>.[^\"]+)\"\\s*\"(?<HTTP_USER_AGENT>.[^\"]+)\"\\s*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, Regex>("CreateFieldSeparator", _nginxAccessUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new NginxAccessUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return NginxAccessUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return NginxAccessUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxAccessUnifiedRecorder, RecorderContext>("CreateContextInstance", _nginxAccessUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
