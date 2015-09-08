using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.CyberroamUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class CyberroamUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _cyberroam;

        /// <summary>
        /// Create a CyberroamUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _cyberroam = new CyberroamUnifiedRecorder();
        }

        /// <summary>
        /// Clear CyberroamUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _cyberroam.Clear();
            _cyberroam = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, object>("Convert2Date", _cyberroam, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

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
            string[] fieldValues = {"2014 09 10", "14:33:58"};
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, object>("Convert2Date", _cyberroam, new[] { rec, field, fieldValues, data });
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
        ///		* fieldvalues = 2014-09-08 14:33:53 
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
            string[] fieldValues = { "2014-09-10","14:33:58" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, object>("Convert2Date", _cyberroam, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull


            //Assert
            Assert.AreEqual(actual, "2014/09/10 14:33:58");

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
        ///     * return string.empty
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
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, object>("Convert2Date", _cyberroam, new[] { rec, field, fieldValues, data });
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
            var actual = (int)MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, object>("Convert2Date", _cyberroam, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

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
            var match = Regex.Match("deneme", "[\"]+");
            string field = null;
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnFieldMatch", _cyberroam, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnFieldMatch", _cyberroam, new object[] { context, field, match });
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
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for input")]
        public void OnFieldMatch_IfMatchIsNullInput_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match(null, "(.*)");
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnFieldMatch", _cyberroam, new object[] { context, field, match });
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
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for pattern")]
        public void OnFieldMatch_IfMatchIsNullPattern_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match("deneme", null);
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnFieldMatch", _cyberroam, new object[] { context, field, match });
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
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for pattern")]
        public void OnFieldMatch_IfMatchIsSuccuss_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match("date=2012-11-29 time=12:29:57 timezone=\"EET\" device_name=\"CR1000i\" device_id=C010001334-8KGA74 log_id=050901616001 log_type=\"Content Filtering\" log_component=\"HTTP\" log_subtype=\"Allowed\" status=\"a\" priority=Information fw_rule_id=0 user_name=\"12503202@ogr.gsu.edu.tr\" user_gp=\"UG_Ogrenci\" iap=12 category=\"SearchEngines\" category_type=\"Neutral\" url=\"sp.ask.com/toolbar/config/partner/vanilla/default_1.12wid/ie/PopupWidgetGamesFF.xml\" contenttype=\"text/xml\" httpresponsecode=\"\" src_ip=10.1.3.46 dst_ip=195.175.70.24 protocol=\"TCP\" src_port=2611 dst_port=80  sent_bytes=0 recv_bytes=6925 domain=sp.ask.com", "(date=(?<DATE>[0-9\\-][^\\s]*))?\\s*(time=(?<TIME>[0-9\\:][^\\s]*))?\\s*(timezone=\"(?<TIMEZONE>(.)|()[^\\\"]*)\")?\\s*(device_name=\"(?<DEVICE_NAME>(.)|()[^\\\"]*)\")?\\s*(device_id=(?<DEVICE_ID>.[^\\s]*))?\\s*(log_id=(?<LOG_ID>.[^\\s]*))?\\s*(log_type=\"(?<LOG_TYPE>(.)|()[^\\\"]*)\")?\\s*(log_component=\"(?<LOG_COMPONENT>(.)|()[^\\\"]*)\")?\\s*(log_subtype=\"(?<LOG_SUBTYPE>(.)|()[^\\\"]*)\")?\\s*(status=\"(?<STATUS>(.)|()[^\\\"]*)\")?\\s*(priority=(?<PRIORITY>.[^\\s]*))?\\s*(fw_rule_id=(?<FW_RULE_ID>.[^\\s]*))?\\s*(user_name=\"(?<USER_NAME>(.)|()[^\\\"]*)\")?\\s*(user_gp=\"(?<USER_GP>(.)|()[^\\\"]*)\")?\\s*(iap=(?<IAP>.[^\\s]*))?\\s*(category=\"(?<CATEGORY>(.)|()[^\\\"]*)\")?\\s*(category_type=\"(?<CATEGORY_TYPE>(.)|()[^\\\"]*)\")?\\s*(url=\"(?<URL>(.)|()[^\\\"]*)\")?\\s*(contenttype=\"(?<CONTENT_TYPE>(.)|()[^\\\"]*)\")?\\s*(httpresponsecode=\"(?<HTTP_RESPONSE_CODE>(.)|()[^\\\"]*)\")?\\s*(src_ip=(?<SRC_IP>[0-9\\.][^\\s]*))?\\s*(dst_ip=(?<DST_IP>[0-9\\.][^\\s]*))?\\s*(protocol=\"(?<PROTOCOL>(.)|()[^\\\"]*)\")?\\s*(src_port=(?<SRC_PORT>[0-9][^\\s]*))?\\s*(dst_port=(?<DST_PORT>[0-9][^\\s]*))?\\s*(sent_bytes=(?<SEND_BYTES>[0-9][^\\s]*))?\\s*(recv_bytes=(?<RECV_BYTES>[0-9][^\\s]*))?\\s*(domain=(?<DOMAIN>.[^\\n]*))?");
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                SourceHeaderInfo = new Dictionary<string, int> { { "date", 0 }, { " time", 1 }, { "timezone", 2 } }
            };
            

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnFieldMatch", _cyberroam, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
        public void OnBeforeProcessRecordInput_IfContextHeaderInfoIsNull_ReturnNextInstruction()
        {
            //Arrange
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = null
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _cyberroam, new object[] { context });
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
            var context = new SyslogRecorderContext(_cyberroam, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CyberroamUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _cyberroam, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

    }
}
