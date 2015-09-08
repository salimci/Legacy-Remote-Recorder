using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.WebWasherUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class WebwasherUnifiedRecorderUnitTestFixture
    {
        private RecorderBase _webwasher;

        /// <summary>
        /// Create a WebwasherUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _webwasher = new WebwasherUnifiedRecorder();
        }

        /// <summary>
        /// Clear WebwasherUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _webwasher.Clear();
            _webwasher = null;
        }
        
        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = null 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return string.empty
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is null")]

        public void Convert2Date_IfFieldvaluesIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, object>("Convert2Date", _webwasher, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }

        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is not null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = 08/SEP/2014:14:33:58 1111
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * Input date have been converted
        /// </summary>
        [TestCase("08/SEP/2014:14:33:58 1111", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsCorrect")]
        [TestCase("2014-111-20 14:33:58", Result = null, TestName = "IfTimeFormatIsWrong")]
        [Test(Description = "Convert2Date tested if fieldvalues is not null")]
        public object Convert2Date_IfFieldvaluesNotNull_returnDate(string fieldvalue)
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { fieldvalue };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (string)MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, object>("Convert2Date", _webwasher, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull
            
            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is string.empty
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = string.empty 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return should string.empty 
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is null")]
        public void Convert2Date_IfFieldvaluesNull_returnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { string.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, object>("Convert2Date", _webwasher, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Convert to the input date 
        ///
        ///Test Scenario : If date format is wrong
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"09/09/14 2:55:PM"} 
        ///		* data = null
        ///
        ///Expected Output :
        ///     * return String Empty
        /// </summary>
        [TestCase("09/09/14 2:55:PM", Result = null, TestName = "IfDateFormatIsWrong")]
        [TestCase("9 EYL 2014 14:55:53", Result = null, TestName = "IfDateFormatIsTurkish")]
        [Test]
        public int Convert2Date_IfDateFormatIsWrong_ReturnStringEmpty(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, object>("Convert2Date", _webwasher, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is not the given format
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"2014 09 10", "14:33:58"} 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return string.empty
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is not the given format")]
        public void Convert2Date_IfFieldvaluesNotGivenFormat_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 09 10", "14:33:58" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, object>("Convert2Date", _webwasher, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : The method check is there a header
        ///
        ///Test Scenario : If context.HeaderInfo is null
        ///
        ///Known Input :
        ///    * TerminalRecorderContext context = null
        ///    * error = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        /// 
        [Test(Description = "If context.HeaderInfo is null")]
        public void GetHeaderInfo_IfContextIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            TerminalRecorderContext context = null;
            Exception e = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, NextInstruction>("GetHeaderInfo", _webwasher, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }
        
        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : The method check is there a header
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * context.HeaderInfo = DataMappingInfo
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        /// 
        [Test(Description = "If context is not null")]
        public void GetHeaderInfo_IfContextIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            TerminalRecorderContext context = null;
            // ReSharper disable once PossibleNullReferenceException
            context.HeaderInfo = new DataMappingInfo();
            // ReSharper restore once PossibleNullReferenceException
            Exception e = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, NextInstruction>("GetHeaderInfo", _webwasher, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : The method get the header text
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    *  TerminalRecorderContext context = null
        ///
        ///Expected Output :
        ///    * null should return
        /// </summary>
        /// 
        [Test(Description = "If context is null")]
        public void GetHeaderText_IfContextIsNull_ReturnNull()
        {
            //Arrange
            TerminalRecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, string>("GetHeaderText", _webwasher, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is success
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///    * context = null
        ///    * field = null
        /// 
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success")]
        public void OnFieldMatch_IfMatchIsSuccess_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("lorem ipsum", @"(\w+)(\s.*)");
            string field = null;
            TerminalRecorderContext context = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, NextInstruction>("OnFieldMatch", _webwasher, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is not success
        ///
        ///Known Input :
        ///    * match = null
        ///    * context = null
        ///    * field = null
        /// 
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is not success")]
        public void OnFieldMatch_IfMatchNotSuccess_ReturnNextInstructionSkip()
        {
            //Arrange
            var match = Regex.Match("lorem ipsum", @"(\d)");
            string field = null;
            TerminalRecorderContext context = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, NextInstruction>("OnFieldMatch", _webwasher, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
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
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, RecordInputType>("InputTextType", _webwasher, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            Assert.AreEqual(actual, RecordInputType.Comment);
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
            var context = new FileLineRecorderContext(_webwasher) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, RecordInputType>("InputTextType", _webwasher, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
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
        ///    * RecordInputType.comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText is null")]
        public void InputTextType_IfContextInputRecordRecordTextIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_webwasher);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, RecordInputType>("InputTextType", _webwasher, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context is true
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord that initiate
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Record should return
        /// </summary>
        [Test(Description = "If context is true")]
        public void InputTextType_IfContextInputRecordRecordTextIsRecordLikeString_ReturnRecord()
        {
            //Arrange

            const string text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit";
            var context = new FileLineRecorderContext(_webwasher);
            var inputTextRecord = new TextRecord { RecordText = text };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebwasherUnifiedRecorder, RecordInputType>("InputTextType", _webwasher, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

    }
}
