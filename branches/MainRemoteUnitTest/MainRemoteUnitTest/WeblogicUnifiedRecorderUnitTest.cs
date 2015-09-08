using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.WeblogicUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class WeblogicUnifiedRecorderUnitTest
    {
        private static RecorderBase _weblogicUnifiedRecorder;

        /// <summary>
        /// Create a WeblogicUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _weblogicUnifiedRecorder = new WeblogicUnifiedRecorder();
        }

        /// <summary>
        /// Clear WeblogicUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _weblogicUnifiedRecorder.Clear();
            _weblogicUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder>("Convert2Date", _weblogicUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "MMM dd, yyyy HH:mm:ss ee zzzz", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "Apr 3, 2013 11:07:49 AM EEST"
        ///     * data = WeblogicUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is MMM dd, yyyy HH:mm:ss ee zzzz, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsdd_MMM_yyyy_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = { "Apr 3, 2013 11:07:49 AM EEST" };
            object data = new WeblogicUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, object>("Convert2Date", _weblogicUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2013/04/03 11:07:49");
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
        ///     * data = WeblogicUnifiedRecorder
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
            string[] values = { "2014/09/09 08:54:15" };
            object data = new WeblogicUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, object>("Convert2Date", _weblogicUnifiedRecorder, new[] { rec, field, values, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, Regex>("CreateHeaderSeparator", _weblogicUnifiedRecorder, new object[] { });

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

            var expected = new Regex("<(?<DATE_TIME>[^>]+)>\\s+<(?<SEVERITY>[^>]+)>\\s+<(?<EVENT>[^>]+)>\\s+(<(?<EVENT_ID>[^>]+)>)?\\s+<(?<DESCRIPTION>[^>]+)>");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, Regex>("CreateHeaderSeparator", _weblogicUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, Regex>("CreateFieldSeparator", _weblogicUnifiedRecorder, new object[] { });

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

            var expected = new Regex("<(?<DATE_TIME>[^>]+)>\\s+<(?<SEVERITY>[^>]+)>\\s+<(?<EVENT>[^>]+)>\\s+(<(?<EVENT_ID>[^>]+)>)?\\s+<(?<DESCRIPTION>[^>]+)>");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, Regex>("CreateFieldSeparator", _weblogicUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, string>("GetHeaderText", _weblogicUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_weblogicUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, string>("GetHeaderText", _weblogicUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
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
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, NextInstruction>("OnFieldMatch", _weblogicUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, NextInstruction>("OnFieldMatch", _weblogicUnifiedRecorder, new object[] { context, field, match });
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
            MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, NextInstruction>("OnFieldMatch", _weblogicUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, NextInstruction>("OnFieldMatch", _weblogicUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.Collections.Generic.KeyNotFoundException
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
            MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, RecordInputType>("InputTextType", _weblogicUnifiedRecorder, new object[] { context, error });
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
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, RecordInputType>("InputTextType", _weblogicUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord is empty
        ///
        ///Known Input :
        ///    * context = RecorderContext with empty InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is empty")]
        public void InputTextType_IfContextInputRecordIsEmpty_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = string.Empty };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, RecordInputType>("InputTextType", _weblogicUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputText string start with "#Fields:" 
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord that initiate with string
        ///         string = "#Fields:---"
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Header should return
        /// </summary>
        [Test(Description = "If not context.InputText string begin with #Fields:, # and not null and string is not empty")]
        public void InputTextType_IfContextInputRecordRecordTextIsRecordLikeString_ReturnRecord()
        {
            //Arrange

            const string stringtest = "Lorem ipsum dolor sit amet, consectetur adipisicing elit";
            var context = new FileLineRecorderContext(_weblogicUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringtest };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WeblogicUnifiedRecorder, RecordInputType>("InputTextType", _weblogicUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }
    }
}
