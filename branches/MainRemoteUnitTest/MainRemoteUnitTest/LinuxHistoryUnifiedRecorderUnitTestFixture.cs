using System;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Linux.Ssh;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class LinuxHistoryUnifiedRecorderUnitTestFixture
    {
        private RecorderBase _linuxhistory;

        /// <summary>
        /// Create a LinuxHistoryRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _linuxhistory = new LinuxHistoryRecorder();
        }

        /// <summary>
        /// Clear LinuxHistoryRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _linuxhistory.Clear();
            _linuxhistory = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, object>("Convert2Date", _linuxhistory, new[] { rec, field, fieldValues, data });
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
        ///		* fieldvalues = 2014-09-08 14:33:58
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * Input date have been converted
        /// </summary>
        [TestCase("2014-09-08","14:33:58", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsCorrect")]
        [TestCase("2014-111-20 14:33:58","", Result = null, TestName = "IfTimeFormatIsWrong")]
        [Test(Description = "Convert2Date tested if fieldvalues is not null")]
        public object Convert2Date_IfFieldvaluesNotNull_returnDate(string fieldvalue1,string fieldvalue2)
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { fieldvalue1,fieldvalue2 };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, object>("Convert2Date", _linuxhistory, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, object>("Convert2Date", _linuxhistory, new[] { rec, field, fieldValues, data });
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
        [TestCase("09/09/14","2:55:PM", Result = null, TestName = "IfDateFormatIsWrong")]
        [TestCase("9 EYL 2014", " 14:55:53", Result = null, TestName = "IfDateFormatIsTurkish")]
        [Test]
        public int Convert2Date_IfDateFormatIsWrong_ReturnStringEmpty(string fieldvalue1, string fieldvalue2)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue1,fieldvalue2 };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, object>("Convert2Date", _linuxhistory, new[] { rec, field, fieldvalues, data });
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
        [Test(Description = "Convert2Date tested if fieldvalues are 1 values")]
        public void Convert2Date_IfFieldvaluesNotGivenFormat_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 09 10", "14:33:58", "PM" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, object>("Convert2Date", _linuxhistory, new[] { rec, field, fieldValues, data });
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
        [Test(Description = "If context is null")]
        public void GetHeaderInfo_IfContextIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            TerminalRecorderContext context = null;
            Exception e = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("GetHeaderInfo", _linuxhistory, new object[] { context, e });
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
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("GetHeaderInfo", _linuxhistory, new object[] { context, e });
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
        ///    *  TerminalRecorderContext context = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, string>("GetHeaderText", _linuxhistory, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        /// Method Name : PrepareKeywords
        ///
        ///Method Description : The method set the keyword
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    *  TerminalRecorderContext context = null
        ///    * buffer = null
        ///
        ///Expected Output :
        ///    * null should return
        /// </summary>
        /// 
        [Test(Description = "If context is null")]
        public void PrepareKeywords_IfContextIsNull_ReturnNull()
        {
            //Arrange
            TerminalRecorderContext context = null;
            object buffer = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, string>("PrepareKeywords", _linuxhistory, new[] { context,buffer });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        /// Method Name : PrepareKeywords
        ///
        ///Method Description : The method set the keyword
        ///
        ///Test Scenario : If buffer is not null
        ///
        ///Known Input :
        ///    *  TerminalRecorderContext context = null
        ///    * buffer = "lorem ipsum"
        ///
        ///Expected Output :
        ///    * null should return
        /// </summary>
        /// 
        [Test(Description = "If context is null")]
        public void PrepareKeywords_IfBufferIsNotNull_ReturnNull()
        {
            //Arrange
            TerminalRecorderContext context = null;
            object buffer = "lorem ; ipsum";
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, string>("PrepareKeywords", _linuxhistory, new[] { context, buffer });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : The method set the value record in context
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * (TerminalRecorderContext)context = null
        ///
        ///Expected Output :
        ///    * null should return
        /// </summary>
        /// 
        [Test(Description = "If context is null")]
        public void OnBeforeSetData_IfContextIsNull_ReturnNull()
        {
            //Arrange
            TerminalRecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("OnBeforeSetData", _linuxhistory, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(null, actual);
        }
        
        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : The method set the value record in context
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * TerminalRecorderContext context = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        /// 
        [Test(Description = "If context is not null")]
        public void OnBeforeSetData_IfContextIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            TerminalRecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("OnBeforeSetData", _linuxhistory, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * TerminalRecorderContext context = null;
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Error should return
        /// </summary>
        [Test (Description = "If the context and Error are null")]
        public void InputTextType_IfContexIsNull_ReturnRecordInputTypeError()
        {
            //Arrange
            TerminalRecorderContext context = null;
            Exception e = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.Error, actual);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * TerminalRecorderContext context 
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Error should return
        /// </summary>
        [Test(Description = "If the context is not null Error is null")]
        public void InputTextType_IfContexIsNotNull_ReturnRecordInputTypeEndOfStream()
        {
            //Arrange
            var context = new LinuxHistoryContext();
            Exception e = null;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.EndOfStream, actual);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If the context.waitbegin is true, Error is null
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context 
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Comment should return
        /// </summary>
        [Test(Description = "If the context.waitbegin is true, Error is null")]
        public void InputTextType_IfContextWaitBeginIsTrue_ReturnRecordInputTypeComment()
        {
            //Arrange
            var context = new LinuxHistoryContext();
            Exception e = null;
            context.WaitBegin = true;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.Comment, actual);
        }
        
        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If the context.waitbegin is false, Error is null
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context 
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Comment should return
        /// </summary>
        [Test(Description = "If the context.waitbegin is false, Error is null")]
        public void InputTextType_IfContextWaitBeginIsFalse_ReturnRecordInputTypeComment()
        {
            //Arrange
            var context = new LinuxHistoryContext();
            Exception e = null;
            context.WaitBegin = false;
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.EndOfStream, actual);
        }
        
        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If the context.InpurRecord has a string value,Context.WaitBegin false, Error is null
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context 
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Record should return
        /// </summary>
        [Test(Description = "If the context.InputRecord has a string value,Context.WaitBegin false, Error is null")]
        public void InputTextType_IfContextInputRecordHasStringValue_ReturnRecordInputTypeRecord()
        {
            //Arrange
            var context = new LinuxHistoryContext();
            Exception e = null;
            context.WaitBegin = false;
            context.InputRecord.SetValue("ali");
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.Record, actual);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : The method set input text type
        ///
        ///Test Scenario : If the context.InpurRecord has a integer value,Context.WaitBegin true, Error is null
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context 
        ///    * Exception e = null;
        ///
        ///Expected Output :
        ///    * RecortInputType.Comment should return
        /// </summary>
        [Test(Description = "If the context.InpurRecord has a integer value,Context.WaitBegin true, Error is null")]
        public void InputTextType_IfContextInputRecordHasIntegerValue_ReturnRecordInputTypeComment()
        {
            //Arrange
            var context = new LinuxHistoryContext();
            Exception e = null;
            context.WaitBegin = true;

            context.InputRecord.SetValue(123);
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, RecordInputType>("InputTextType", _linuxhistory, new object[] { context, e });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(RecordInputType.Comment, actual);
        }

        /// <summary>
        /// Method Name : GetInputName
        ///
        ///Method Description : The method get input name
        ///
        ///Test Scenario : If the context is null 
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context = null
        ///
        ///Expected Output :
        ///    * LinuxHistory should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetInputName_IfContextIsNotNull_ReturnLinuxHistory()
        {
            //Arrange
            LinuxHistoryContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, string>("GetInputName", _linuxhistory, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("LinuxHistory", actual);
        }

        /// <summary>
        /// Method Name : GetInputName
        ///
        ///Method Description : The method get input name
        ///
        ///Test Scenario : If the context is not null 
        ///
        ///Known Input :
        ///    * LinuxHistoryContext context 
        ///
        ///Expected Output :
        ///    * LinuxHistory should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetInputName_IfContextIsNull_ReturnLinuxHistory()
        {
            //Arrange
            var context = new LinuxHistoryContext();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, string>("GetInputName", _linuxhistory, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("LinuxHistory", actual);
        }
        
        /// <summary>
        /// Method Name : OnArgParsed
        ///
        ///Method Description : The method parsed according to the keyword
        ///
        ///Test Scenario :  Given  different value to keyword
        ///
        ///Known Input :
        ///           * string keyword,
        ///           * bool quotedKeyword,
        ///           * string value,
        ///           * bool quotedValue,
        ///           * int touchCount,
        ///           * Exception error
        ///
        ///Expected Output :
        ///     * return true
        /// </summary>
        [TestCase(null, Result = false, TestName = "IfAllParameterNull")]
        [TestCase("Port", Result = true, TestName = "IfKeywordPortValueNull")]
        [TestCase("ReadTimeout", Result = true, TestName = "IfKeywordReadTimeoutValueNull")]
        [TestCase("Pattern", Result = true, TestName = "IfKeywordPatternValueNull")]
        [Test(Description = "Given  different value to keyword")]
        public bool OnArgParsed_IfKeywordIsDifferentValue_Returnboolean(string keyword)
        {
            //Arrange
            object[] fieldvalues = { "lorem", "ipsum" };
            const string value = "lorem ipsum";
            const int touchCount = 1;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, bool>("OnArgParsed", _linuxhistory, new object[] { keyword, false, value, fieldvalues, touchCount, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Text2Header
        ///
        ///Method Description : The method convert text to header
        ///
        ///Test Scenario : If the context is null 
        ///
        ///Known Input :
        ///    * RecorderContext ctxFile
        ///    * string headerTex = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void Text2Header_IfContextIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext ctxFile = null;
            string headerTex = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("Text2Header", _linuxhistory, new object[] { ctxFile, headerTex });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }

        /// <summary>
        /// Method Name : Text2Header
        ///
        ///Method Description : The method convert text to header
        ///
        ///Test Scenario : If the context is not null 
        ///
        ///Known Input :
        ///    * RecorderContext ctxFile = new LinuxHistoryContext()
        ///    * string headerTex = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void Text2Header_IfContextIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext ctxFile = new LinuxHistoryContext();
            string headerTex = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("Text2Header", _linuxhistory, new object[] { ctxFile, headerTex });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }

        /// <summary>
        /// Method Name : InputText2RecordField
        ///
        ///Method Description : The method convert input text to record field
        ///
        ///Test Scenario : If the context is null 
        ///
        ///Known Input :
        ///    * RecorderContext context = null
        ///    * string[] fields = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void InputText2RecordField_IfContextIsNull_ReturnNextInstructionSkip()
        {
            //Arrange
            RecorderContext ctxFile = null;
            string[] fields = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("InputText2RecordField", _linuxhistory, new object[] { ctxFile, fields });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Skip, actual);
        }

        /// <summary>
        /// Method Name : InputText2RecordField
        ///
        ///Method Description : The method convert input text to record field
        ///
        ///Test Scenario : If the context is not null 
        ///
        ///Known Input :
        ///    * RecorderContext ctxFile = new LinuxHistoryContext()
        ///    * string[] fields = null
        ///
        ///Expected Output :
        ///    *  throw Exception "Context is not LinuxHistoryContext or null" should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void InputText2RecordField_IfContextIsNotNull_ReturnThrowException()
        {
            //Arrange
            RecorderContext ctxFile = new LinuxHistoryContext();
            string[] fields = null;
            ctxFile.InputRecord.SetValue("lorem ipsum");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<LinuxHistoryRecorder, NextInstruction>("InputText2RecordField", _linuxhistory, new object[] { ctxFile, fields });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do,actual);
        }

    }
}
