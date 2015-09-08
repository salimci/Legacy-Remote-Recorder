using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.MssqlErrorUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class MssqlErrorUnifiedRecorderUnitTest
    {
        private static RecorderBase _mssqlErrorUnifiedRecorder;

        /// <summary>
        /// Create a MssqlErrorUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _mssqlErrorUnifiedRecorder = new MssqlErrorUnifiedRecorder();
        }

        /// <summary>
        /// Clear MssqlErrorUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _mssqlErrorUnifiedRecorder.Clear();
            _mssqlErrorUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder>("Convert2Date", _mssqlErrorUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * data = MssqlErrorUnifiedRecorder
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
            object data = new MssqlErrorUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder>("Convert2Date", _mssqlErrorUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException
        }
        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy-M-d" + "H:m:s", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09", "08:54:15.50"
        ///     * data = MssqlErrorUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is yyyy-MM-dd HH:mm:ss.ff, return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = String.Empty;
            string[] values = { "2014-09-09", "08:54:15.50" };
            object data = new MssqlErrorUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, object>("Convert2Date", _mssqlErrorUnifiedRecorder, new[] { rec, field, values, data });
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
        ///     * data = MssqlErrorUnifiedRecorder
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
            object data = new MssqlErrorUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, object>("Convert2Date", _mssqlErrorUnifiedRecorder, new[] { rec, field, values, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, object>("Convert2Date", _mssqlErrorUnifiedRecorder, new[] { rec, field, values, data });
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
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _mssqlErrorUnifiedRecorder, new object[] { context, field, match });
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
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _mssqlErrorUnifiedRecorder, new object[] { context, field, match });
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
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _mssqlErrorUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _mssqlErrorUnifiedRecorder, new object[] { context, field, match });
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
            var match = Regex.Match("2014-07-10 03:24:38.08 spid3s      Resource governor reconfiguration succeeded.", @"^(?<DATE>[^\s]+)\s+(?<TIME>[^\s]+)\s+(?<EVENT>[^\s]+)\s+(?<DESCRIPTION>[^\n]+)$");
            string field = null;
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _mssqlErrorUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
            MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, RecordInputType>("InputTextType", _mssqlErrorUnifiedRecorder, new object[] { context, error });
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
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, RecordInputType>("InputTextType", _mssqlErrorUnifiedRecorder, new object[] { context, error });
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
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText is null")]
        public void InputTextType_IfContextInputRecordRecordTextIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, RecordInputType>("InputTextType", _mssqlErrorUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = text };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, RecordInputType>("InputTextType", _mssqlErrorUnifiedRecorder, new object[] { context, error });
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
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, string>("GetHeaderText", _mssqlErrorUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_ftp2012UnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_mssqlErrorUnifiedRecorder);
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, string>("GetHeaderText", _mssqlErrorUnifiedRecorder, new object[] { context });
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
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, Regex>("CreateHeaderSeparator", _mssqlErrorUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"^(?<DATE>[^\s]+)\s+(?<TIME>[^\s]+)\s+(?<EVENT>[^\s]+)\s+(?<DESCRIPTION>[^\n]+)$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, Regex>("CreateHeaderSeparator", _mssqlErrorUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, Regex>("CreateFieldSeparator", _mssqlErrorUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"^(?<DATE>[^\s]+)\s+(?<TIME>[^\s]+)\s+(?<EVENT>[^\s]+)\s+(?<DESCRIPTION>[^\n]+)$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<MssqlErrorUnifiedRecorder, Regex>("CreateFieldSeparator", _mssqlErrorUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }
    }
}
