using System;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Microsoft.Iis;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class IisUnifiedRecorderUnitTest
    {
        private static RecorderBase _iisUnifiedRecorder;

        /// <summary>
        /// Create a IisUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _iisUnifiedRecorder = new IisUnifiedRecorder();
        }

        /// <summary>
        /// Clear IisUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _iisUnifiedRecorder.Clear();
            _iisUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_iisUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Unknown);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.RecordText.Length == 0
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord.RecordText that initiate with empty string
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText.Length is 0")]
        public void InputTextType_IfContextInputRecordRecordTextIsStringEmpty_ReturnUnknown()
        {
            //Arrange
            var context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = string.Empty };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputText start with '#'
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord that initiate with string
        ///         string = "#---"
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText start with sharp")]
        public void InputTextType_IfContextInputRecordRecordTextIsStartWithSharp_ReturnUnknown()
        {
            //Arrange

            const string stringBeginWithSharp = "#---";
            var context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
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
        [Test(Description = "If context.InputRecord.RecordText start with '#Fields:'")]
        public void InputTextType_IfContextInputRecordRecordTextIsStartWithSharpFields_ReturnHeader()
        {
            //Arrange

            const string stringBeginWithSharp = "#Fields: ---";
            var context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Header);
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

            const string stringBeginWithSharp = "Lorem ipsum dolor sit amet, consectetur adipisicing elit";
            var context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        /// 
        /// Method Desciption : Get header line except first eight characters 
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "GetHeaderText tested if context is null")]
        public void GetHeaderText_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, string>("GetHeaderText", _iisUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Asset

            //Unhandled Exception
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///
        ///Expected Output :
        ///    * NullReferenceException should occure
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void GetHeaderText_IfContextInputRecordIsNull_NullReferenceException()
        {
            //Arrange
            var context = new FileLineRecorderContext(_iisUnifiedRecorder) { InputRecord = null };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, string>("GetHeaderText", _iisUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled Exception
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If context.InputRecord.length() is smaller than eight
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///
        ///Expected Output :
        ///    * ArgumentOutOfRangeException should occure
        /// </summary>
        [Test(Description = "If context.InputRecord.length() is smaller than eight")]
        public void GetHeaderText_IfContextInputRecordLengthIsSmallerThanEigth_ArgumentOutOfRangeException()
        {
            //Arrange
            var context = new FileLineRecorderContext(_iisUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "field" };
            context.InputRecord = inputTextRecord;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, string>("GetHeaderText", _iisUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled ArgumentOutOfRangeException
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If all cases is true
        ///                 string = "#Fields: ---"
        ///Known Input : RecorderContext with InputRecord that initiate
        ///
        ///Expected Output :
        ///    * " ---" should return
        /// </summary>
        [Test(Description = "If all cases is true")]
        public void GetHeaderText_IfContextInputRecordIsTrue_ReturnExpectedString()
        {
            //Arrange
            var context = new FileLineRecorderContext(_iisUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "#Fields: ---" };
            context.InputRecord = inputTextRecord;
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, string>("GetHeaderText", _iisUnifiedRecorder, new object[] { context });


            //Assert
            Assert.AreEqual(actual, " ---");
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
        /// 
        ///Expected Output :
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for header")]
        public void CreateHeaderSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, Regex>("CreateHeaderSeparator", _iisUnifiedRecorder, new object[] { });

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
        /// 
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for header")]
        public void CreateHeaderSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex(" ");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, Regex>("CreateHeaderSeparator", _iisUnifiedRecorder, new object[] { });

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
        /// 
        ///Expected Output :
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for field")]
        public void CreateFieldSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, Regex>("CreateFieldSeparator", _iisUnifiedRecorder, new object[] { });

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
        /// 
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for field")]
        public void CreateFieldSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex(" ");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, Regex>("CreateFieldSeparator", _iisUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        /// 
        /// Method Desciption : Ready for process data
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "OnBeforeSetData tested if context is null")]
        public void OnBeforeSetData_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, RecordInputType>("InputTextType", _iisUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Asset

            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.ComputerName is empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        /// 
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.ComputerName is empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndComputerNameIsEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var contextRecord = new RecWrapper { ComputerName = string.Empty };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, NextInstruction>("OnBeforeSetData", _iisUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.ComputerName is not empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        ///    * context.Record.ComputerName = "ComputerName"
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.ComputerName is not empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndComputerNameIsNotEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var contextRecord = new RecWrapper { ComputerName = "ComputerName" };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, NextInstruction>("OnBeforeSetData", _iisUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.CustomStr10 is empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.CustomStr10 is empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndCustomStr10IsEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var contextRecord = new RecWrapper { CustomStr10 = string.Empty };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, NextInstruction>("OnBeforeSetData", _iisUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.CustomStr10 is not empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        ///    * context.Record.ComputerName = "CustomStr10"
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.CustomStr10 is not empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndCustomStr10IsNotEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_iisUnifiedRecorder);
            var contextRecord = new RecWrapper { CustomStr10 = "CustomStr10" };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, NextInstruction>("OnBeforeSetData", _iisUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnArgParsed
        ///
        ///Method Description : Arguman can parsable
        ///
        ///Test Scenario : If keyword is not 'T'
        ///
        ///Known Input :
        ///   	* keyword = 'F'
        ///     * quotedKeyword = False
        ///     * value = String.Empty
        ///     * quotedValue = False
        ///     * touchCount = 0
        ///     * exception = null
        ///
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = " If keyword is not 'T'")]
        public void OnArgParsed_IfKeywordIsNotT_ReturnTrue()
        {
            //Arrange
            const string keyword = "F";
            const bool quotedKeyword = false;
            var value = string.Empty;
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception exception = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, bool>("OnArgParsed", _iisUnifiedRecorder, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, exception });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : OnArgParsed
        ///
        ///Method Description : Arguman can parsable
        ///
        ///Test Scenario : If keyword is "T"
        ///
        ///Known Input :
        ///   	* keyword = "T"
        ///     * quotedKeyword = False
        ///     * value = String.Empty
        ///     * quotedValue = False
        ///     * touchCount = 0
        ///     * exception = null
        ///
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = " If keyword is 'T'")]
        public void OnArgParsed_IfKeywordIsT_ReturnTrue()
        {
            //Arrange
            const string keyword = "T";
            const bool quotedKeyword = false;
            var value = string.Empty;
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception exception = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, bool>("OnArgParsed", _iisUnifiedRecorder, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, exception });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : CustomStr5Splitter
        ///
        ///Method Description : Return CustomStr5's 900 bytes
        ///
        ///Test Scenario : If values is null
        ///
        ///Known Input :
        ///   	* recorder = RecWrapper
        ///     * field = String.Empty
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If values is null")]
        public void CustomStr5Splitter_IfValuesIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var recorder = new RecWrapper();
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder>("CustomStr5Splitter", _iisUnifiedRecorder, new[] { recorder, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : CustomStr5Splitter
        ///
        ///Method Description : Return CustomStr5's 900 bytes
        ///
        ///Test Scenario : If values.length() is smaller than 900 bytes
        ///
        ///Known Input :
        ///   	* recorder = RecWrapper
        ///     * field = String.Empty
        ///     * values = "Lorem ipsum dolor sit amet", "consectetur adipisicing elit"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  return values[0]
        /// </summary>
        [Test(Description = "If values.length() is smaller than 900 bytes")]
        public void CustomStr5Splitter_IfValuesIsSmallerThan900bytes_ReturnValues0()
        {
            //Arrange
            var recorder = new RecWrapper();
            string field = null;
            string[] values = {"Lorem ipsum dolor sit amet", "consectetur adipisicing elit"};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("CustomStr5Splitter", _iisUnifiedRecorder, new[] { recorder, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem ipsum dolor sit amet");
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
            var  rec= new RecWrapper();
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * data = IisUnifiedRecorder
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
            string[] values = { "2014-09-09"};
            object data = new IisUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException
        }
        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy-M-d H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09", "08:54:15"
        ///     * data = IisUnifiedRecorder
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
            object data = new IisUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
        }
        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "y-M-d H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09", "08:54:15"
        ///     * data = IisUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is y-M-d H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIs_Y_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = {"2014-09-09", "08:54:15"};
            object data = new IisUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * data = IisUnifiedRecorder
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
            string[] values = {"2014/09/09", "08:54:15" };
            object data = new IisUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If recorder is null, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = "2014/09/09", "08:54:15"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If recorder is null, return string.Empty")]
        public void Convert2Date_IfDataIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "2014/09/09", "08:54:15" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("Convert2Date", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : VersionConcat
        ///
        ///Method Description : Return given version informations concaniate and convert urldecode
        ///
        ///Test Scenario : If values is null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = String.Empty
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If values is null")]
        public void VersionConcat_IfValuesIsNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper rec = null;
            string field = String.Empty;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder>("VersionConcat", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : VersionConcat
        ///
        ///Method Description : Return given version informations concaniate and convert urldecode
        ///
        ///Test Scenario : If values[0] is empty and values[1] is null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = String.Empty,null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Null should return
        /// </summary>
        [Test(Description = "If values[0] is empty and values[1] is null")]
        public void VersionConcat_IfValues0IsEmptyAndValues1IsNull_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = {String.Empty,null};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("VersionConcat", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : VersionConcat
        ///
        ///Method Description : Return given version informations concaniate and convert urldecode
        ///
        ///Test Scenario : If values[0] is empty and values[1] is not null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = String.Empty,"Mozilla/5.0+"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  HttpHelper.UrlDecode(values[1]) should return
        /// </summary>
        [Test(Description = "If values[0] is empty and values[1] is not null")]
        public void VersionConcat_IfValues0IsEmptyAndValues1IsNotEmpty_ReturnUrlDecode()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { String.Empty, "Mozilla/5.0+" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("VersionConcat", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, HttpHelper.UrlDecode(values[1]));
        }

        /// <summary>
        /// Method Name : VersionConcat
        ///
        ///Method Description : Return given version informations concaniate and convert urldecode
        ///
        ///Test Scenario : If values[0] and values[1] are is not empty or null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values ="HTTP/1.1","Mozilla/5.0+"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    * HttpHelper.UrlDecode(values[0] + " " + values[1] ) should return
        /// </summary>
        [Test(Description = "If values[0] and values[1] are is not empty or null")]
        public void VersionConcat_IfValuesIsNotEmpty_ReturnUrlDecode()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "HTTP/1.1", "Mozilla/5.0+" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("VersionConcat", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, HttpHelper.UrlDecode(values[0] + " " + values[1]));
        }

        /// <summary>
        /// Method Name : VersionConcat
        ///
        ///Method Description : Return given version informations concaniate and convert urldecode
        ///
        ///Test Scenario : If values[1] is empty and values[0] is not null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = "Mozilla/5.0+", String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  HttpHelper.UrlDecode(values[0]) should return
        /// </summary>
        [Test(Description = "If values[1] is empty and values[0] is not null")]
        public void VersionConcat_IfValues1IsEmptyAndValues0IsNotEmpty_ReturnUrlDecode()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "Mozilla/5.0+", String.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("VersionConcat", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, HttpHelper.UrlDecode(values[0]));
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return CustomStr1's 900 bytes
        ///
        ///Test Scenario : If values[0] is null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If values[0] is null")]
        public void CustomStr1Splitter_IfValues0IsNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder>("CustomStr1Splitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return CustomStr1's 900 bytes
        ///
        ///Test Scenario : If values[0] is empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If values[0] is empty")]
        public void CustomStr1Splitter_IfValues0IsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = {string.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("CustomStr1Splitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return CustomStr1's 900 bytes
        ///
        ///Test Scenario : If values[0] is not empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = "/nsmgui"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  HttpHelper.UrlDecode(values[0]) should return
        /// </summary>
        [Test(Description = "If values[0] is not empty")]
        public void CustomStr1Splitter_IfValues0IsNotEmpty_ReturnUrlDecodedString()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "/nsmgui" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("CustomStr1Splitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, HttpHelper.UrlDecode(values[0]));
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return CustomStr1's 900 bytes
        ///
        ///Test Scenario : If values[0].length is bigger than 1800 bytes
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor
        ///     sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///    consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ipsum dolor sit amet consectetur adipisicing elit"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem O" should return
        /// </summary>
        [Test(Description = "If values[0].length is bigger than 1800 bytes")]
        public void CustomStr1Splitter_IfValues0IsNotEmptyAndLengthIsBigger1800Bytes_ReturnUrlDecodedString()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolorsit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consecteturadipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Loremipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ipsum dolor sit amet consectetur adipisicing elit" };
            object data = null;
            const string expected = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem O";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("CustomStr1Splitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return CustomStr1's 900 bytes
        ///
        ///Test Scenario : If values[0] is not empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem 
        ///      ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ipsum dolor sit amet consectetur adipisicing elit "
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  HttpHelper.UrlDecode(values[0]) should return
        /// </summary>
        [Test(Description = "If values[0] is not empty and length is bigger than 900 bytes")]
        public void CustomStr1Splitter_IfValues0IsNotEmptyAndLengthIsBigger900Bytes_ReturnUrlDecodedString()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit " };
            object data = null;
            const string expected = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lore";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("CustomStr1Splitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Split given description according to comma separator
        ///
        ///Test Scenario : If values[0] is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  string.Empty should return
        /// </summary>
        [Test(Description = "If values[0] is null")]
        public void DescriptionSplitter_IfValues0IsNull_ReturndStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = {null};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("DescriptionSplitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Split given description according to comma separator
        ///
        ///Test Scenario : If values[0] is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = string.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  string.Empty should return
        /// </summary>
        [Test(Description = "If values[0] is empty")]
        public void DescriptionSplitter_IfValues0IsEmpty_ReturndStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { string.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("DescriptionSplitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Split given description according to comma separator
        ///
        ///Test Scenario : If values[0] is not empty or null, split given description
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem; ipsum; dolor"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Lorem should return
        /// </summary>
        [Test(Description = "If values[0] is not empty or null, split given description")]
        public void DescriptionSplitter_IfValues0IsThreePart_ReturndFirstSplittedPart()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = {"Lorem; ipsum; dolor"};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("DescriptionSplitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Split given description according to comma separator
        ///
        ///Test Scenario : If values[0] is not empty or null, split given description
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem ipsum dolor"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Lorem ipsum dolor should return
        /// </summary>
        [Test(Description = "If values[0] is not empty or null, split given description")]
        public void DescriptionSplitter_IfValues0IsNotIncludeCommaSeperator_ReturndString()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem ipsum dolor" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<IisUnifiedRecorder, object>("DescriptionSplitter", _iisUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem ipsum dolor");
        }
    }
}
