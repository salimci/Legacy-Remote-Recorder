using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.TrendMicroSafeSync;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class TrendMicroSafeSyncUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _trendmicrounified;
        /// <summary>
        /// Create a TrendMicroUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _trendmicrounified = new TrendMicroSafeSyncUnifiedRecorder();
        }


        /// <summary>
        /// Clear TrendMicroUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _trendmicrounified.Clear();
            _trendmicrounified = null;
        }

        /// <summary>
        /// Method Name : Concatinate
        /// 
        /// Method Desciption : Integration to fieldvalues string array data
        /// 
        /// Test Scenerio : If fieldvalues is null
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
        [Test(Description = "Concatinate tested if fieldvalues is null")]

        public void Concatinate_IfFieldvaluesIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Concatinate", _trendmicrounified, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);
        }


        /// <summary>
        /// Method Name : Concatinate
        /// 
        /// Method Desciption : Integration to fieldvalues string array data
        /// 
        /// Test Scenerio : If fieldvalues is empty
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = {string.empty} 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return string.empty
        /// </summary>
        [Test(Description = "Concatinate tested if fieldvalues is empty")]

        public void Concatinate_IfFieldvaluesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { string.Empty };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Concatinate", _trendmicrounified, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        /// Method Name : Concatinate
        /// 
        /// Method Desciption : Integration to fieldvalues string array data
        /// 
        /// Test Scenerio : If fieldvalues is not null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"ali!","ayse?"} 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return ali!ayse?
        /// </summary>
        [TestCase(123, 456, Result = 123456, TestName = "IfFieldvaluesAreNumbers")]
        [TestCase("ali!", "ayse?", Result = "ali!ayse?", TestName = "IfFieldvaluesIsString")]
        [TestCase("123", "abc", Result = "123abc", TestName = "IfFieldvaluesIsAlfaDecimal")]
        [Test]

        public object Concatinate_IfFieldvaluesIsNotNull_ReturnConcatinate(string fieldvalue1, string fieldvalue2)
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { fieldvalue1,fieldvalue2 };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Concatinate", _trendmicrounified, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;
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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Convert2Date", _trendmicrounified, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Convert2Date", _trendmicrounified, new[] { rec, field, fieldValues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Convert2Date", _trendmicrounified, new[] { rec, field, fieldValues, data });
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
            var actual = (int)MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, object>("Convert2Date", _trendmicrounified, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

        }
        
        /// <summary>
        /// Method Name : OnBeforeProcessRecordInput
        /// 
        /// Method Desciption : Is there a header information in context 
        /// 
        /// Test Scenerio : If Context.HeaderInfo is null
        /// 
        /// Known Input :
        ///     * Context.HeaderInfo = null
        /// 
        /// Expected Output : 
        ///     * return NextInstruction.Do
        /// </summary>
        [Test(Description = "OnBeforeProcessRecordInput tested if Context.HeaderInfo is null")]
        public void OnBeforeProcessRecordInput_IfHeaderInfoIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            var context = new SyslogRecorderContext(_trendmicrounified, ProtocolType.Udp, "192.168.1.25")
            {
                HeaderInfo = null
            };
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _trendmicrounified, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeProcessRecordInput
        /// 
        /// Method Desciption : Is there a header information in context
        /// 
        /// Test Scenerio : If Context.HeaderInfo is not null
        /// 
        /// Known Input :
        ///     * Context.HeaderInfo = new DataMappingInfo()
        /// 
        /// Expected Output : 
        ///     * return NextInstruction.Do
        /// </summary>
        [Test(Description = "OnBeforeProcessRecordInput tested if Context.HeaderInfo is not null")]
        public void OnBeforeProcessRecordInput_IfHeaderInfoIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var context = new SyslogRecorderContext(_trendmicrounified, ProtocolType.Udp, "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _trendmicrounified, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnBeforeProcessRecordInput
        /// 
        /// Method Desciption : Is there a header information in context
        /// 
        /// Test Scenerio : If match is not success
        /// 
        /// Known Input :
        ///     * match = !success
        /// 
        /// Expected Output : 
        ///     * return NextInstruction.Skip
        /// </summary>
        [Test(Description = "OnFieldMatch tested if match is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnNextInstructionSkip()
        {
            //Arrange
            var context = new SyslogRecorderContext(_trendmicrounified, ProtocolType.Udp, syslogAddress: "192.168.1.25");
            var match = Regex.Match("deneme", "[\"]+");
            string field = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, NextInstruction>("OnFieldMatch", _trendmicrounified, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
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
            var context = new SyslogRecorderContext(_trendmicrounified, ProtocolType.Udp, syslogAddress: "192.168.1.25");
            context.FieldBuffer.Add("deneme");
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, NextInstruction>("OnFieldMatch", _trendmicrounified, new object[] { context, field, match });
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
        ///    * context = "deneme";
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success, context is not null")]
        public void OnFieldMatch_IfMatchAndContextSuccess_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Jun  5 15:28:06 SyslogAlertForwarder: 2012-06-05 15:28:05 EEST | P2P: Windows Mesh Traffic Detected | Medium | 65.55.236.179:443 | 10.1.1.11:25949 |PolicyViolation ", @"^.*?:\s*(?<DATE>[0-9\-]*\s*[0-9\:]*\s*[a-zA-Z]*)\s*\|\s*((?<THREAD_CODE>[^:]*):\s*(?<THREAT>[^\|]*))\s*\|\s*(?<SEVERITY>[^\|]*)\s*\|\s*((?<TARGET_IP>[^:]*):(?<TARGET_PORT>[^\|]*))\s*\|\s*((?<SRC_IP>[^:]*):(?<SRC_PORT>[^\|]*))\s*\|\s*(?<RULE>.[^\|]*)\s*");
            const string field = "Jun  5 15:28:06 SyslogAlertForwarder: 2012-06-05 15:28:05 EEST | P2P: Windows Mesh Traffic Detected | Medium | 65.55.236.179:443 | 10.1.1.11:25949 |PolicyViolation";
            var context = new SyslogRecorderContext(_trendmicrounified, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                SourceHeaderInfo = new Dictionary<string, int> { { "DATE", 0 }, { "THREAD_CODE", 1 }, { "THREAT", 2 } }
            };


            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<TrendMicroSafeSyncUnifiedRecorder, NextInstruction>("OnFieldMatch", _trendmicrounified, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

    }
}
