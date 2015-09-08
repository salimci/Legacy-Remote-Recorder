using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.TrendMicroUrlUnifiedRecorder;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class TrendMicroUrlUnifiedRecorderUnitTest
    {
        private static RecorderBase _trendMicroUrlUnifiedRecorder;

        /// <summary>
        /// Create a TrendMicroUrlUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _trendMicroUrlUnifiedRecorder = new TrendMicroUrlUnifiedRecorder();
        }

        /// <summary>
        /// Clear TrendMicroUrlUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _trendMicroUrlUnifiedRecorder.Clear();
            _trendMicroUrlUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder>("Convert2Date", _trendMicroUrlUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy/MM/dd HH:mm:ss", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014/09/09 08:54:15"
        ///     * data = TrendMicroUrlUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is yyyy/MM/dd HH:mm:ss, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_MM_dd_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = { "2014/09/09 08:54:15" };
            object data = new TrendMicroUrlUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, object>("Convert2Date", _trendMicroUrlUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * values = "2014-09-09 08:54:15"
        ///     * data = TrendMicroUrlUnifiedRecorder
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
            string[] values = { "2014-09-09 08:54:15" };
            object data = new TrendMicroUrlUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, object>("Convert2Date", _trendMicroUrlUnifiedRecorder, new[] { rec, field, values, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _trendMicroUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"([\w\S]*)\s*:\t(.*)");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _trendMicroUrlUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _trendMicroUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"([\w\S]*)\s*:\t(.*)");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _trendMicroUrlUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, string>("GetHeaderText", _trendMicroUrlUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_trendMicroUrlUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_trendMicroUrlUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, string>("GetHeaderText", _trendMicroUrlUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new TrendMicroUrlUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return TrendMicroUrlUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return TrendMicroUrlUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, RecorderContext>("CreateContextInstance", _trendMicroUrlUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }

        /// <summary>
        /// Method Name : OnArgParsed
        ///
        ///Method Description : Arguman can parsable
        ///
        ///Test Scenario : If keyword is not 'DF'
        ///
        ///Known Input :
        ///   	* keyword = 'd'
        ///     * quotedKeyword = False
        ///     * value = String.Empty
        ///     * quotedValue = False
        ///     * touchCount = 0
        ///     * exception = null
        ///
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = " If keyword is not 'DF'")]
        public void OnArgParsed_IfKeywordIsNotDF_ReturnTrue()
        {
            //Arrange
            const string keyword = "d";
            const bool quotedKeyword = false;
            var value = string.Empty;
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception exception = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, bool>("OnArgParsed", _trendMicroUrlUnifiedRecorder, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, exception });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : OnArgParsed
        ///
        ///Method Description : Arguman can parsable
        ///
        ///Test Scenario : If keyword is "DF"
        ///
        ///Known Input :
        ///   	* keyword = "DF"
        ///     * quotedKeyword = False
        ///     * value = String.Empty
        ///     * quotedValue = False
        ///     * touchCount = 0
        ///     * exception = null
        ///
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = " If keyword is 'DF'")]
        public void OnArgParsed_IfKeywordIsDF_ReturnTrue()
        {
            //Arrange
            const string keyword = "DF";
            const bool quotedKeyword = false;
            var value = string.Empty;
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception exception = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, bool>("OnArgParsed", _trendMicroUrlUnifiedRecorder, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, exception });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : InputTextType
        /// 
        /// Method Desciption : Determined the input record type
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        ///     * error = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "InputTextType tested if context is null")]
        public void InputTextType_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, RecordInputType>("InputTextType", _trendMicroUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled exception
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Unknown should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnUnknown()
        {
            //Arrange
            var context = new FileLineRecorderContext(_trendMicroUrlUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, RecordInputType>("InputTextType", _trendMicroUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Unknown);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.RecordText is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord.RecordText
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Unknown should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText is null")]
        public void InputTextType_IfContextInputRecordRecordTextIsNull_ReturnUnknown()
        {
            //Arrange
            var context = new FileLineRecorderContext(_trendMicroUrlUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, RecordInputType>("InputTextType", _trendMicroUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Unknown);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.RecordText is not null
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord.RecordText
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.REcord should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText is not null")]
        public void InputTextType_IfContextInputRecordRecordTextIsNotNull_ReturnReocrd()
        {
            //Arrange
            var context = new FileLineRecorderContext(_trendMicroUrlUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = String.Empty };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, RecordInputType>("InputTextType", _trendMicroUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void OnFieldMatch_IfContextIsNull_ReturnNextInstructionAbort()
        {
            //Arrange
            Match match = null;
            var source = String.Empty;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _trendMicroUrlUnifiedRecorder, new object[] { context, source, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If source is empty
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If source is empty")]
        public void OnFieldMatch_IfsourceIsempty_ReturnNextInstructionAbort()
        {
            //Arrange
            Match match = null;
            RecorderContext context = new FileLineRecorderContext(_trendMicroUrlUnifiedRecorder);
            var source = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _trendMicroUrlUnifiedRecorder, new object[] { context, source, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }
    }
}
