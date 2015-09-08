using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.WebsenseDlpUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class WebsenseDlpUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _websenseUnifiedRecorder;

        /// <summary>
        /// Create a WebsenseUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _websenseUnifiedRecorder = new WebsenseDlpUnifiedRecorder();
        }

        /// <summary>
        /// Clear WebsenseUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _websenseUnifiedRecorder.Clear();
            _websenseUnifiedRecorder = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Date", _websenseUnifiedRecorder, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty,actual);

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
        ///		* fieldvalues = 2014 SEP 08 14:33:58 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * Input date have been converted
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is not null")]
        public void Convert2Date_IfFieldvaluesNotNull_returnDate()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 SEP 08 14:33:58" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Date", _websenseUnifiedRecorder, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull


            //Assert
            Assert.AreEqual(actual, "2014/09/08 14:33:58");

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
            string[] fieldValues = {string.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Date", _websenseUnifiedRecorder, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull


            //Assert
            Assert.AreEqual(string.Empty,actual);

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
            var actual = (int)MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Date", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
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
        [Test(Description = "Convert2Date tested if fieldvalues is null")]

        public void Convert2Date_IfFieldvaluesNotGivenFormat_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 09 10", "14:33:58" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Date", _websenseUnifiedRecorder, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }
        
        /// <summary>
        /// Method Name : OnBeforeProcessRecordInput
        ///
        ///Method Description : The method check is there a header
        ///
        ///Test Scenario : If context.HeaderInfo is null
        ///
        ///Known Input :
        ///    * context.HeaderInfo = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        /// 
        [Test(Description = "If context.HeaderInfo is null")]
        public void OnBeforeProcessRecordInput_IfContextHeaderInfoIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder,ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = null
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _websenseUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }
        
        /// <summary>
        /// Method Name : OnBeforeProcessRecordInput
        ///
        ///Method Description : The method check is there a header
        ///
        ///Test Scenario : If context.HeaderInfo is not null
        ///
        ///Known Input :
        ///    * context.HeaderInfo = DataMappingInfo
        ///
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        /// 
        [Test(Description = "If context.HeaderInfo is not null")]
        public void OnBeforeProcessRecordInput_IfContextHeaderInfoIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _websenseUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
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
        ///    * field = null
        ///    * context = syslogcontext
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnNextInstructionSkip()
        {
            //Arrange
            var match = Regex.Match("deneme","[\"]+");
            string field = null;
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder, ProtocolType.Udp,syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnFieldMatch", _websenseUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual,NextInstruction.Skip);
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
        ///    * field = null
        ///    * context = syslogcontext
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success")]
        public void OnFieldMatch_IfMatchIsSuccess_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("deneme", "(.*)");
            string field = null;
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnFieldMatch", _websenseUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }
        
        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If match is null for input
        ///
        ///Known Input :
        ///    * match = null
        ///    * field = null
        ///    * context = syslogcontext
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for input")]
        public void OnFieldMatch_IfMatchIsNullInput_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match(null,".*");
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnFieldMatch", _websenseUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }
        
        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If match is null for pattern
        ///
        ///Known Input :
        ///    * match = null
        ///    * field = null
        ///    * context = syslogcontext
        /// 
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for pattern")]
        public void OnFieldMatch_IfMatchIsNullPattern_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match("deneme",null);
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_websenseUnifiedRecorder, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("OnFieldMatch", _websenseUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If fieldvalues length is zero
        ///
        ///Known Input :
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"0"} 
        ///		* data = null
        ///
        ///Expected Output :
        ///     * return 0 should occure
        /// </summary>
        [Test(Description = "If Convert2Byte_fieldvalues length is zero")]
        public void Convert2Byte_IfFieldvaluesLengthIsZero_ReturnZero()
        {
            //Arrange

            String[] fieldvalues = {"0"};
            string field = null;
            RecWrapper rec = null;
            object data = null;
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(0,actual);
        }

        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If fieldvalues length bigger than zero
        ///
        ///Known Input :
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = 1024 KB
        ///		* data = null
        ///
        ///Expected Output :
        ///     * return 0 should not occure
        /// </summary>
        [Test(Description = "If Convert2Byte_fieldvalues length bigger than zero")]
        public void Convert2Byte_IfFieldvaluesNotZero_ReturnNotZero()
        {
            //Arrange
            String[] fieldvalues = { "1024","KB" };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, NextInstruction>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(actual, 0);
        }
        
        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If match is success for 1, 100, 1024 MB
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = 1024 KB 
        ///		* data = null
        ///     * value = 1024
        ///     * unit = KB
        ///
        ///Expected Output :
        ///     * According to the unit Convert to byte
        /// </summary>

        [TestCase("1 KB", Result = 1024, TestName = "IfMatchIsSuccessFor1KB")]
        [TestCase("100 KB", Result = 102400, TestName = "IfMatchIsSuccessFor100KB")]
        [TestCase("1024 KB", Result = 1048576, TestName = "IfMatchIsSuccessFor1024KB")]
        [TestCase("1099511627776 KB", Result = 1125899906842624, TestName = "IfMatchIsSuccessFor1TB")]
        [Test]
        public int Convert2Byte_IfMatchIsSuccessForKb_ReturnConvertedValueob(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;
             
        }
        
        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If match is success for 1, 100, 1024 MB
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = 1024 MB 
        ///		* data = null
        ///     * value = 1024
        ///     * unit = MB
        ///
        ///Expected Output :
        ///     * According to the unit Convert to byte
        /// </summary>

        [TestCase("1 MB", Result = 1048576, TestName = "IfMatchIsSuccessFor1MB")]
        [TestCase("100 MB", Result = 104857600, TestName = "IfMatchIsSuccessFor100MB")]
        [TestCase("1024 MB", Result = 1073741824, TestName = "IfMatchIsSuccessFor1024MB")]
        [Test]
        public int Convert2Byte_IfMatchIsSuccessForMb_ReturnConvertedValueob(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual =(int) MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If match is success for 1, 100, 1024 GB
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = 1024 GB 
        ///		* data = null
        ///     * value = 1024
        ///     * unit = GB
        ///
        ///Expected Output :
        ///     * According to the unit Convert to byte
        /// </summary>
        [TestCase("1 GB", Result = 1073741824, TestName = "IfMatchIsSuccessFor1GB")]
        [TestCase("100 GB", Result = 107374182400, TestName = "IfMatchIsSuccessFor100GB")]
        [TestCase("1024 GB", Result = 1099511627776, TestName = "IfMatchIsSuccessFor1024GB")]
        [Test]
        public int Convert2Byte_IfMatchIsSuccessForGb_ReturnConvertedValueob(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual =(int) MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;
        }

        /// <summary>
        /// Method Name : Convert2Byte
        ///
        ///Method Description : Convert to the input data to KB, MB or GB
        ///
        ///Test Scenario : If match is not success
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = 1024 FB 
        ///		* data = null
        ///
        ///Expected Output :
        ///     * return 0
        /// </summary>
        [TestCase("100 FB", Result = 0, TestName = "IfMatchNotSuccessFor100FB")]
        [TestCase("1024 ZB", Result = 0, TestName = "IfMatchNotSuccessFor1024ZB")]
        [Test]
        public int Convert2Byte_IfMatchNotSuccess_ReturnZero(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<WebsenseDlpUnifiedRecorder, object>("Convert2Byte", _websenseUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;
        }
    }
}
