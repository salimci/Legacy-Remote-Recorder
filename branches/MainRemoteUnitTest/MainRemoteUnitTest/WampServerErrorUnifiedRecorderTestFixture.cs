using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.WamppServerErrorUnifiedRecorder;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class WampServerErrorUnifiedRecorderTestFixture
    {
        private static RecorderBase _wampServerError;

        /// <summary>
        /// Create a WampServerErrorUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _wampServerError = new WamppServerErrorUnifiedRecorder();
        }

        /// <summary>
        /// Clear WampServerErrorUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _wampServerError.Clear();
            _wampServerError = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, object>("Convert2Date", _wampServerError, new[] { rec, field, fieldValues, data });
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
        ///		* fieldvalues = 08 SEP 2014:14:33:58 11111
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * Input date have been converted
        /// </summary>
        [TestCase("Mon Jun 18 09:35:07 2012", Result = "2012/09/18 09:35:07", TestName = "IfTimeFormatIsCorrect")]
        [TestCase("08/SEP/2014:14:33:58 11", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsTwoDecimal")]
        [TestCase("[08/SEP/2014:14:33:58 +1111]", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsFiveDecimal")]
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
            var actual = (int)MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, object>("Convert2Date", _wampServerError, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, object>("Convert2Date", _wampServerError, new[] { rec, field, fieldValues, data });
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
            var actual = (int)MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, object>("Convert2Date", _wampServerError, new[] { rec, field, fieldvalues, data });
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
        [Test(Description = "Convert2Date tested if fieldvalues is not given format")]
        public void Convert2Date_IfFieldvaluesNotGivenFormat_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 09 10", "14:33:58" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, object>("Convert2Date", _wampServerError, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, NextInstruction>("GetHeaderInfo", _wampServerError, new object[] { context, e });
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, NextInstruction>("GetHeaderInfo", _wampServerError, new object[] { context, e });
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
        ///    * (syslogContext)context = null
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, string>("GetHeaderText", _wampServerError, new object[] { context });
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _wampServerError, new object[] { context, field, match });
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
            var actual = MethodTestHelper.RunInstanceMethod<WamppServerErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _wampServerError, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

    }
}
