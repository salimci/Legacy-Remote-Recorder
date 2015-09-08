using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.TMGUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
// ReSharper disable InconsistentNaming
    public class TMGUnifiedRecorderUnitTest
// ReSharper restore InconsistentNaming
    {
// ReSharper disable InconsistentNaming
        private static RecorderBase _TMGUnifiedRecorder;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Create a TMGUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _TMGUnifiedRecorder = new TmgUnifiedRecorder();
        }

        /// <summary>
        /// Clear TMGUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _TMGUnifiedRecorder.Clear();
            _TMGUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder>("Convert2Date", _TMGUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If values string array has one item
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = "2014-09-09"
        ///     * data = TMGUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  IndexOutOfRangeException should occure
        /// </summary>
        [Test(Description = "If values string array has one item")]
        public void Convert2Date_IfValuesHasOneItem_ReturnIndexOutOfRangeException()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "2014-09-09" };
            object data = new TmgUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder>("Convert2Date", _TMGUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy-MM-dd HH:mm:ss", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09", "08:54:15"
        ///     * data = TMGUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is yyyy-M-d H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = { "2014-09-09", "08:54:15" };
            object data = new TmgUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, object>("Convert2Date", _TMGUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
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
        ///     * values = "2014/09/09", "08:54:15"
        ///     * data = TMGUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If date time format is not expected, return String.Empty")]
        public void Convert2Date_IfDateTimeFormatIsNotCorrect_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "2014/09/09", "08:54:15" };
            object data = new TmgUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, object>("Convert2Date", _TMGUnifiedRecorder, new[] { rec, field, values, data });
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
            var context = new FileLineRecorderContext(_TMGUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, NextInstruction>("OnFieldMatch", _TMGUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_TMGUnifiedRecorder);
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, NextInstruction>("OnFieldMatch", _TMGUnifiedRecorder, new object[] { context, field, match });
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
            MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, NextInstruction>("OnFieldMatch", _TMGUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_TMGUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, NextInstruction>("OnFieldMatch", _TMGUnifiedRecorder, new object[] { context, field, match });
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
            var match = Regex.Match("THKUS05	2012-12-23	00:00:02	ICMP	95.183.207.253:1280	95.183.207.254:256	95.183.207.253	External	Local Host	Denied	0xc004000d	Default rule		Unidentified IP Traffic	0	0	0	0	-	-	-	-	0	0	-	-	4		-	0	-", "^(?<SERVERNAME>[^\\t]+)\\t+(?<DATE>[^\\t]+)\\t+(?<TIME>[^\\t]+)\\t+(?<PROTOCOL>[^\\t]+)\\t+(?<SOURCE_IP>[^:]+):(?<SOURCE_PORT>[^\\t]+)\\t+(?<DESTINATION_IP>[^:]+):(?<DESTINATION_PORT>[^\\t]+)\\t+(?<ORIGINAL_CLIENT_IP>[^\\t]+)\\t+(?<SOURCE_NETWORK>[^\\t]+)\\t+(?<DESTINATION_NETWORK>[^\\t]+)\\t+(?<ACTION>[^\\t]+)\\t(?<RESULT_CODE>[^\\t]+)\\t+(?<RULE>[^\\t]+)\\t+(?<APPLICATION_PROTOCOL>[^\\t]+)\\t+(?<BIDIRECTIONAL>[^\\t]+)\\t+(?<BYTES_SENT>[^\\t]+)\\t+(?<BYTES_RECEIVED>[^\\t]+)\\t+(?<CONNECTION_TIME>[^\\t]+)\\t+(?<DESTINATION_NAME>[^\\t]+)\\t+(?<CLIENT_USERNAME>[^\\t]+)\\t+(?<CLIENT_AGENT>[^\\t]+)\\t+(?<SESSION_ID>[^\\t]+)\\t+(?<CONNECTION_ID>[^\\t]+)\\t+(?<INTERFACE>[^\\t]+)\\t+(?<IPHEADER>[^\\t]+)\\t+(?<PAYLOAD>[^\\t]+)\\t+(?<GMT_TIME>[^\\t]+)\\t+(?<IPS_SCANRESULT>[^\\t]+)\\t+(?<IPS_SIGNATURE>[^\\t]+)\\t+(?<NAT_ADDRESS>[^(\\t|\\n)]+)$");
            string field = null;
            var context = new FileLineRecorderContext(_TMGUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, NextInstruction>("OnFieldMatch", _TMGUnifiedRecorder, new object[] { context, field, match });
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
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, string>("GetHeaderText", _TMGUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_TMGUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_TMGUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, string>("GetHeaderText", _TMGUnifiedRecorder, new object[] { context });
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
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, Regex>("CreateHeaderSeparator", _TMGUnifiedRecorder, new object[] { });

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

            var expected = new Regex("^(?<SERVERNAME>[^\\t]+)\\t+(?<DATE>[^\\t]+)\\t+(?<TIME>[^\\t]+)\\t+(?<PROTOCOL>[^\\t]+)\\t+(?<SOURCE_IP>[^:]+):(?<SOURCE_PORT>[^\\t]+)\\t+(?<DESTINATION_IP>[^:]+):(?<DESTINATION_PORT>[^\\t]+)\\t+(?<ORIGINAL_CLIENT_IP>[^\\t]+)\\t+(?<SOURCE_NETWORK>[^\\t]+)\\t+(?<DESTINATION_NETWORK>[^\\t]+)\\t+(?<ACTION>[^\\t]+)\\t(?<RESULT_CODE>[^\\t]+)\\t+(?<RULE>[^\\t]+)\\t+(?<APPLICATION_PROTOCOL>[^\\t]+)\\t+(?<BIDIRECTIONAL>[^\\t]+)\\t+(?<BYTES_SENT>[^\\t]+)\\t+(?<BYTES_RECEIVED>[^\\t]+)\\t+(?<CONNECTION_TIME>[^\\t]+)\\t+(?<DESTINATION_NAME>[^\\t]+)\\t+(?<CLIENT_USERNAME>[^\\t]+)\\t+(?<CLIENT_AGENT>[^\\t]+)\\t+(?<SESSION_ID>[^\\t]+)\\t+(?<CONNECTION_ID>[^\\t]+)\\t+(?<INTERFACE>[^\\t]+)\\t+(?<IPHEADER>[^\\t]+)\\t+(?<PAYLOAD>[^\\t]+)\\t+(?<GMT_TIME>[^\\t]+)\\t+(?<IPS_SCANRESULT>[^\\t]+)\\t+(?<IPS_SIGNATURE>[^\\t]+)\\t+(?<NAT_ADDRESS>[^(\\t|\\n)]+)$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, Regex>("CreateHeaderSeparator", _TMGUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, Regex>("CreateFieldSeparator", _TMGUnifiedRecorder, new object[] { });

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

            var expected = new Regex("^(?<SERVERNAME>[^\\t]+)\\t+(?<DATE>[^\\t]+)\\t+(?<TIME>[^\\t]+)\\t+(?<PROTOCOL>[^\\t]+)\\t+(?<SOURCE_IP>[^:]+):(?<SOURCE_PORT>[^\\t]+)\\t+(?<DESTINATION_IP>[^:]+):(?<DESTINATION_PORT>[^\\t]+)\\t+(?<ORIGINAL_CLIENT_IP>[^\\t]+)\\t+(?<SOURCE_NETWORK>[^\\t]+)\\t+(?<DESTINATION_NETWORK>[^\\t]+)\\t+(?<ACTION>[^\\t]+)\\t(?<RESULT_CODE>[^\\t]+)\\t+(?<RULE>[^\\t]+)\\t+(?<APPLICATION_PROTOCOL>[^\\t]+)\\t+(?<BIDIRECTIONAL>[^\\t]+)\\t+(?<BYTES_SENT>[^\\t]+)\\t+(?<BYTES_RECEIVED>[^\\t]+)\\t+(?<CONNECTION_TIME>[^\\t]+)\\t+(?<DESTINATION_NAME>[^\\t]+)\\t+(?<CLIENT_USERNAME>[^\\t]+)\\t+(?<CLIENT_AGENT>[^\\t]+)\\t+(?<SESSION_ID>[^\\t]+)\\t+(?<CONNECTION_ID>[^\\t]+)\\t+(?<INTERFACE>[^\\t]+)\\t+(?<IPHEADER>[^\\t]+)\\t+(?<PAYLOAD>[^\\t]+)\\t+(?<GMT_TIME>[^\\t]+)\\t+(?<IPS_SCANRESULT>[^\\t]+)\\t+(?<IPS_SIGNATURE>[^\\t]+)\\t+(?<NAT_ADDRESS>[^(\\t|\\n)]+)$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, Regex>("CreateFieldSeparator", _TMGUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new TMGUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return TMGUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return TMGUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TmgUnifiedRecorder, RecorderContext>("CreateContextInstance", _TMGUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
