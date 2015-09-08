using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.JuniperSyslogV6UnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for JuniperSyslogV6UnifiedRecorderUnitTestFixture
    /// </summary>
    [TestFixture]
    public class JuniperSyslogV6UnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _junipersyslog;

        /// <summary>
        /// Create a JuniperSyslogV6UnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _junipersyslog = new JuniperSyslogV6UnifiedRecorder();
        }

        /// <summary>
        /// Clear JuniperSyslogV6UnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _junipersyslog.Clear();
            _junipersyslog = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, object>("Convert2Date", _junipersyslog, new[] { rec, field, fieldValues, data });
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
            string[] fieldValues = { "2014 09 10", "14:33:58" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, object>("Convert2Date", _junipersyslog, new[] { rec, field, fieldValues, data });
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
            string[] fieldValues = { "2014-09-10 14:33:58" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, object>("Convert2Date", _junipersyslog, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, object>("Convert2Date", _junipersyslog, new[] { rec, field, fieldValues, data });
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
            var actual =(int)MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, object>("Convert2Date", _junipersyslog, new[] { rec, field, fieldvalues, data });
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
        ///    * field = null
        ///    * context = syslogContext
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnFieldMatch", _junipersyslog, new object[] { context, field, match });
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
        ///    * field = null
        ///    * context = syslogContext
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnFieldMatch", _junipersyslog, new object[] { context, field, match });
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
        ///    * context = syslogContext
        /// 
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnFieldMatch", _junipersyslog, new object[] { context, field, match });
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
        ///    * context = syslogContext
        /// 
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnFieldMatch", _junipersyslog, new object[] { context, field, match });
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
        ///    * match = at the moment pattern
        ///    * field = at the moment pattern
        ///    * context = syslogContext
        /// 
        /// 
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is null for pattern")]
        public void OnFieldMatch_IfMatchIsSuccuss_ReturnNextInstructionSkip()
        {
            //Arrange

            // ReSharper disable AssignNullToNotNullAttribute
            var match = Regex.Match("192.0.0.110:29813 : local0.notice TUYAP_MERKEZ_FW: NetScreen device_id=TUYAP_MERKEZ_FW  [Root]system-notification-00257(traffic): start_time=\"2011-05-27 11:01:53\" duration=0 policy_id=29 service=proto:41/port:1 proto=41 srczone=Trust dstzone=ADSL action=Deny sent=0 rcvd=0 src=192.0.0.72 dst=192.88.99.1 session_id=0 ", "(?<SOURCE_NAME>[0-9\\.:]+)\\s*:\\s*[\\w\\.]+\\s*(?<FIREWALL_NAME>[^:]+):\\s*[a-zA-Z]+\\s*device_id=(?<DEVICE_ID>[^\\s]+)\\s*.[^\\]]+\\](?<EVENT_TYPE>[^:]+):\\s*(start_time=\"(?<START_TIME>[0-9-]+\\s*[0-9:]+)\")?\\s*(duration=(?<DURATION>[0-9]+))?\\s*(policy_id=(?<POLICY_ID>[0-9]+))?\\s*(service=(?<SERVICE>[^\\s]+))?\\s*(proto=(?<PROTO>[0-9]+))?\\s*(srczone=(?<SRC_ZONE>[^\\s]+))?\\s*(dstzone=(?<DST_ZONE>[^\\s]+))?\\s*(src_port=(?<SRC_PORT>[0-9]+))?\\s*(dst_port=(?<DST_PORT>[0-9]+))?\\s*(action=(?<ACTION>[^\\s]+))?\\s*\r\n(sent=(?<SENT>[0-9]+))?\\s*(rcvd=(?<RCVD>[0-9]+))?\\s*\r\n(icmp\\stype=(?<ICMP_TYPE>[0-9]+))?\\s*(src=(?<SRC>[0-9\\.]+))?\\s*\r\n(dst=(?<DST>[0-9\\.]+))?\\s*(src-xlated-ip=(?<SRC_XLATED>[^\\s]+))?\\s*\r\n(dst-xlated-ip=(?<DST_XLATED>[^\\s]+))?\\s*(session_id=(?<SESSION_ID>[0-9]+))?\\s");
            // ReSharper restore AssignNullToNotNullAttribute
            const string field = "192.0.0.110:29813 : local0.notice TUYAP_MERKEZ_FW: NetScreen device_id=TUYAP_MERKEZ_FW  [Root]system-notification-00257(traffic): start_time=\"2011-05-27 11:01:53\" duration=0 policy_id=29 service=proto:41/port:1 proto=41 srczone=Trust dstzone=ADSL action=Deny sent=0 rcvd=0 src=192.0.0.72 dst=192.88.99.1 session_id=0 ";
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                SourceHeaderInfo = new Dictionary<string, int> { { "source_name", 0 }, { " firewall_name", 1 }, { "device_id", 2 } }
            };


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnFieldMatch", _junipersyslog, new object[] { context, field, match });
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = null
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _junipersyslog, new object[] { context });
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
            var context = new SyslogRecorderContext(_junipersyslog, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<JuniperSyslogV6UnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _junipersyslog, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }




    }
}
