using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Microsoft.Exchange;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class NetscalerUnifiedRecorderUnitTest
    {
        private static RecorderBase _netscalerUnifiedRecorder;

        /// <summary>
        /// Create a NetscalerUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _netscalerUnifiedRecorder = new NetscalerUnifiedRecorder();
        }

        /// <summary>
        /// Clear NetscalerUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _netscalerUnifiedRecorder.Clear();
            _netscalerUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }
        
        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy-M-d" +  "H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = {"2014-09-09", "08:54:15"}
        ///     * data = NetscalerUnifiedRecorder
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
            string[] values = {"2014-09-09", "08:54:15"};
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "y-M-d" +  "H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = {"2014-09-09", "08:54:15"}
        ///     * data = NetscalerUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is y-M-d H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsY_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            var field = String.Empty;
            string[] values = {"2014-09-09", "08:54:15"};
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy/M/d" +  "H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = {"2014/09/09", "08:54:15"}
        ///     * data = NetscalerUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is yyyy/M/d H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_M_dd_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = {"2014/09/09", "08:54:15"};
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "y/M/d" +  "H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = {"2014/09/09", "08:54:15"}
        ///     * data = NetscalerUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is y/M/d H:m:s, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsY_M_dd_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = {"2014/09/09", "08:54:15"};
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
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
        ///     * data = NetscalerUnifiedRecorder
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
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException
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
        ///     * values = "2014_09_09" + "08:54:15"
        ///     * data = NetscalerUnifiedRecorder
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
            string[] values = { "2014_09_09","08:54:15" };
            object data = new NetscalerUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * values = "2014/09/09" "08:54:15"
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
            string[] values = { "2014_09_09","08:54:15" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("Convert2Date", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : HttpDecode
        ///
        ///Method Description : Return given values as http decoded object
        ///
        ///Test Scenario : If values is null, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If values is null, return string.Empty")]
        public void HttpDecode_IfValuesIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("HttpDecode", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }
        
        /// <summary>
        /// Method Name : HttpDecode
        ///
        ///Method Description : Return given values as http decoded object
        ///
        ///Test Scenario : If values is empty, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If recorder is empty, return string.Empty")]
        public void HttpDecode_IfValuesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { String.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("HttpDecode", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : HttpDecode
        ///
        ///Method Description : Return given values as http decoded object
        ///
        ///Test Scenario :  If values's length is 1, return first element in values array
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = {"Lorem"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return "Lorem"
        /// </summary>
        [Test(Description = " If values's length is 1, return first element in values array")]
        public void HttpDecode_IfValuesLengthIsOne_ReturnFirstItem()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = {"Lorem"};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("HttpDecode", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : HttpDecode
        ///
        ///Method Description : Return given values as http decoded object
        ///
        ///Test Scenario :   If values's length is bigger than 1, return first element in values array
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = {"Lorem", "ipsum", "dolor"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return "Lorem"
        /// </summary>
        [Test(Description = " If values's length is bigger than 1, return first element in values array")]
        public void HttpDecode_IfValuesLengthIsBiggerThanOne_ReturnFirstItem()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "Lorem", "ipsum", "dolor" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, object>("HttpDecode", _netscalerUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
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
            MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, RecordInputType>("InputTextType", _netscalerUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_netscalerUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, RecordInputType>("InputTextType", _netscalerUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Unknown);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.Lentgh() equals zero
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.Lentgh() equals zero")]
        public void InputTextType_IfContextInputRecordLenthIsZero_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_netscalerUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = String.Empty };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, RecordInputType>("InputTextType", _netscalerUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario :  If context.InputRecord.Lentgh() is not zero
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Record should return
        /// </summary>
        [Test(Description = " If context.InputRecord.Lentgh() is not zero")]
        public void InputTextType_IfContextInputRecordLenthIsNotZero_ReturnRecord()
        {
            //Arrange
            var context = new FileLineRecorderContext(_netscalerUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = "#----" };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, RecordInputType>("InputTextType", _netscalerUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
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
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, string>("GetHeaderText", _netscalerUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_netscalerUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_netscalerUnifiedRecorder);


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, string>("GetHeaderText",
                _netscalerUnifiedRecorder, new object[] {context});
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
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, Regex>("CreateHeaderSeparator", _netscalerUnifiedRecorder, new object[] { });

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

            var expected = new Regex("([^ \"]*\"([^\"]*)\"[^ ]*|([^ ]*))[ ]*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, Regex>("CreateHeaderSeparator", _netscalerUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, Regex>("CreateFieldSeparator", _netscalerUnifiedRecorder, new object[] { });

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

            var expected = new Regex("([^ \"]*\"([^\"]*)\"[^ ]*|([^ ]*))[ ]*");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, Regex>("CreateFieldSeparator", _netscalerUnifiedRecorder, new object[] { });

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
            MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, NextInstruction>("OnBeforeSetData", _netscalerUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Asset

            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        /// 
        /// Method Desciption : Ready for process data
        /// 
        /// Test Scenerio : If context is not null
        /// 
        /// Known Input :
        ///     * context = NetscalerUnifiedRecorder
        /// 
        /// Expected Output : 
        ///     * NextInstruction.Do should occure
        /// </summary>
        [Test(Description = "OnBeforeSetData tested if context is not null")]
        public void OnBeforeSetData_IfContextIsNotNull_Do()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_netscalerUnifiedRecorder);
            context.Record = new RecWrapper();
         

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NetscalerUnifiedRecorder, NextInstruction>("OnBeforeSetData", _netscalerUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Asset

            Assert.AreEqual(actual, NextInstruction.Do);
        }
    }
}
