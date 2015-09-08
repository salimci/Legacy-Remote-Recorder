using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Dhcp;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class DhcpUnifiedRecorderUnitTest
    {
        private static RecorderBase _dhcpUnifiedRecorder;

        /// <summary>
        /// Create a DhcpUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _dhcpUnifiedRecorder = new DhcpUnifiedRecorder();
        }

        /// <summary>
        /// Clear DhcpUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _dhcpUnifiedRecorder.Clear();
            _dhcpUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder>("Convert2Date", _dhcpUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * data = DhcpUnifiedRecorder
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
            object data = new DhcpUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder>("Convert2Date", _dhcpUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "M/d/yyyy H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "05/23/14", "12:10:44"
        ///     * data = DhcpUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/05/23 12:10:44
        /// </summary>
        [Test(Description = "If date time format is M/d/yyyy H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            var field = String.Empty;
            string[] values = { "05/23/14", "12:10:44" };
            object data = new DhcpUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, object>("Convert2Date", _dhcpUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/05/23 12:10:44");
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
        ///     * data = DhcpUnifiedRecorder
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
            object data = new DhcpUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, object>("Convert2Date", _dhcpUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
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
        [Test(Description = "If context is null")]
        public void InputTextType_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled NullReferenceException
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = string.Empty };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = stringBeginWithSharp };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        /// 
        /// Method Desciption : Get header line except first eight characters 
        /// 
        /// Test Scenerio : GetHeaderText tested if context is null
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
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, string>("GetHeaderText", _dhcpUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled NullReferenceException
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder) { InputRecord = null };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, string>("GetHeaderText", _dhcpUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "field" };
            context.InputRecord = inputTextRecord;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, string>("GetHeaderText", _dhcpUnifiedRecorder, new object[] { context });
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
            var context = new FileLineRecorderContext(_dhcpUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "#Fields: ---" };
            context.InputRecord = inputTextRecord;
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, string>("GetHeaderText", _dhcpUnifiedRecorder, new object[] { context });


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
        ///Expected Output :
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for header")]
        public void CreateHeaderSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, Regex>("CreateHeaderSeparator", _dhcpUnifiedRecorder, new object[] { });

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

            var expected = new Regex(",");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, Regex>("CreateHeaderSeparator", _dhcpUnifiedRecorder, new object[] { });

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
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for field")]
        public void CreateFieldSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, Regex>("CreateFieldSeparator", _dhcpUnifiedRecorder, new object[] { });

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
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for field")]
        public void CreateFieldSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex(",");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, Regex>("CreateFieldSeparator", _dhcpUnifiedRecorder, new object[] { });

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
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        /// 
        /// Method Desciption : Ready for process data
        /// 
        /// Test Scenerio : If context.record is null
        /// 
        /// Known Input :
        ///     * context.record = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "OnBeforeSetData tested if context.record is null")]
        public void OnBeforeSetData_IfContextRecordIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            context.Record = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("InputTextType", _dhcpUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.CustomStr1 is empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.CustomStr1 is empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndCustomStr1IsEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var contextRecord = new RecWrapper { CustomStr1 = string.Empty };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, NextInstruction>("OnBeforeSetData", _dhcpUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Ready for process data
        ///
        ///Test Scenario : If context is not null and context.Record.CustomStr1 is not empty
        ///
        ///Known Input :
        ///    * context = RecorderContext
        ///    * context.Record.CustomStr1 = "CustomStr1"
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = " If context is not null and context.Record.CustomStr1 is not empty")]
        public void OnBeforeSetData_IfContextIsNotNullAndCustomStr1IsNotEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var contextRecord = new RecWrapper { CustomStr1 = "CustomStr1" };
            context.Record = contextRecord;

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, NextInstruction>("OnBeforeSetData", _dhcpUnifiedRecorder, new object[] { context });

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        /// 
        /// Method Desciption : Get Header Info
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
        [Test(Description = "If context is null")]
        public void GetHeaderInfo_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, RecordInputType>("GetHeaderInfo", _dhcpUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        /// 
        /// Method Desciption : Get Header Info
        /// 
        /// Test Scenerio : Error is new exception
        /// 
        /// Known Input :
        ///     * context = dhcpUnifiedRecorder
        ///     * error = new Exception()
        /// 
        /// Expected Output : 
        ///     * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "Error is new exception")]
        public void GetHeaderInfo_IfErrorIsInvalid_ReturnNextInstructionAbort()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_dhcpUnifiedRecorder);
            var error = new Exception();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DhcpUnifiedRecorder, NextInstruction>("GetHeaderInfo", _dhcpUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }
    }
}
