﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.PaloAltoUrlUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class PaloAltoUrlUnifiedRecorderUnitTest
    {
        private static RecorderBase _paloAltoUrlUnifiedRecorder;

        /// <summary>
        /// Create a PaloAltoUrlUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _paloAltoUrlUnifiedRecorder = new PaloAltoUrlUnifiedRecorder();
        }

        /// <summary>
        /// Clear PaloAltoUrlUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _paloAltoUrlUnifiedRecorder.Clear();
            _paloAltoUrlUnifiedRecorder = null;
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
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _paloAltoUrlUnifiedRecorder, new object[] { context, field, match });
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
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder);
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _paloAltoUrlUnifiedRecorder, new object[] { context, field, match });
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
            MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _paloAltoUrlUnifiedRecorder, new object[] { context, field, match });
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
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _paloAltoUrlUnifiedRecorder, new object[] { context, field, match });
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
            var match = Regex.Match(@"1,2011/01/25 01:26:40,0004C100832,THREAT,url,1,2011/01/25 01:26:39,10.10.9.42,5.126.182.187,193.189.142.3,75.126.182.187,URL_Default,tpe\guvenlik,,web-browsing,vsys1,LAN,Internet,ethernet1/6,ethernet1/1,,2011/01/25 01:26:40,29394,1,4125,80,63495,80,0x40,tcp,block-url,srv.sayyac.net/sa.js?_salogin=ztporno&_sav=4.3,(9999),malware-sites,informational,client-to-server", @"^(?<DOMAIN>[^,]*),(?<RECEIVE_TIME>[^,]*),(?<SERIAL>[^,]*),(?<TYPE>[^,]*),(?<CONTENT_TYPE>[^,]*),(?<CONFIG_VERSION>[^,]*),(?<GENERATE_TIME>[^,]*),(?<SOURCE_ADDRESS>[^,]*),(?<DESTINATION_ADDRESS>[^,]*),(?<NAT_SOURCE_IP>[^,]*),(?<NAT_DESTINATION_IP>[^,]*),(?<RULE>[^,]*),(?<SOURCE_USER>[^,]*),(?<DESTINATION_USER>[^,]*),(?<APPLICATION>[^,]*),(?<VIRTUAL_SYSTEM>[^,]*),(?<SOURCE_ZONE>[^,]*),(?<DESTINATION_ZONE>[^,]*),(?<INBOUND_INTERFACE>[^,]*),(?<OUTBOUND_INTERFACE>[^,]*),(?<LOG_ACTION>[^,]*),(?<TIME_LOGGED>[^,]*),(?<SESSION_ID>[^,]*),(?<REPEAT_COUNT>[^,]*),(?<SOURCE_PORT>[^,]*),(?<DESTINATION_PORT>[^,]*),(?<NAT_SOURCE_PORT>[^,]*),(?<NAT_DESTINATION_PORT>[^,]*),(?<FLAGS>[^,]*),(?<IP_PROTOCOL>[^,]*),(?<ACTION>[^,]*),(((?<BYTES>[^,]*),(?<BYTES_RCV>[^,]*),(?<BYTES_SEND>[^,]*),(?<PACKETS>[^,]*),(?<START_TIME>[^,]*),(?<ELAPSED>[^,]*),(?<URL_CATEGORY>[^,]*),(?<PADDING>[^\,]*))|((?<URL>[^,]*),(?<CONTENT_NAME>[^,]*),(?<CATEGORY>[^,]*),(?<SEVERITY>[^,]*),(?<DIRECTION>[^\,]*)))$");
            string field = null;
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _paloAltoUrlUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
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
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _paloAltoUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"^(?<DOMAIN>[^,]*),(?<RECEIVE_TIME>[^,]*),(?<SERIAL>[^,]*),(?<TYPE>[^,]*),(?<CONTENT_TYPE>[^,]*),(?<CONFIG_VERSION>[^,]*),(?<GENERATE_TIME>[^,]*),(?<SOURCE_ADDRESS>[^,]*),(?<DESTINATION_ADDRESS>[^,]*),(?<NAT_SOURCE_IP>[^,]*),(?<NAT_DESTINATION_IP>[^,]*),(?<RULE>[^,]*),(?<SOURCE_USER>[^,]*),(?<DESTINATION_USER>[^,]*),(?<APPLICATION>[^,]*),(?<VIRTUAL_SYSTEM>[^,]*),(?<SOURCE_ZONE>[^,]*),(?<DESTINATION_ZONE>[^,]*),(?<INBOUND_INTERFACE>[^,]*),(?<OUTBOUND_INTERFACE>[^,]*),(?<LOG_ACTION>[^,]*),(?<TIME_LOGGED>[^,]*),(?<SESSION_ID>[^,]*),(?<REPEAT_COUNT>[^,]*),(?<SOURCE_PORT>[^,]*),(?<DESTINATION_PORT>[^,]*),(?<NAT_SOURCE_PORT>[^,]*),(?<NAT_DESTINATION_PORT>[^,]*),(?<FLAGS>[^,]*),(?<IP_PROTOCOL>[^,]*),(?<ACTION>[^,]*),(((?<BYTES>[^,]*),(?<BYTES_RCV>[^,]*),(?<BYTES_SEND>[^,]*),(?<PACKETS>[^,]*),(?<START_TIME>[^,]*),(?<ELAPSED>[^,]*),(?<URL_CATEGORY>[^,]*),(?<PADDING>[^\,]*))|((?<URL>[^,]*),(?<CONTENT_NAME>[^,]*),(?<CATEGORY>[^,]*),(?<SEVERITY>[^,]*),(?<DIRECTION>[^\,]*)))$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _paloAltoUrlUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _paloAltoUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"^(?<DOMAIN>[^,]*),(?<RECEIVE_TIME>[^,]*),(?<SERIAL>[^,]*),(?<TYPE>[^,]*),(?<CONTENT_TYPE>[^,]*),(?<CONFIG_VERSION>[^,]*),(?<GENERATE_TIME>[^,]*),(?<SOURCE_ADDRESS>[^,]*),(?<DESTINATION_ADDRESS>[^,]*),(?<NAT_SOURCE_IP>[^,]*),(?<NAT_DESTINATION_IP>[^,]*),(?<RULE>[^,]*),(?<SOURCE_USER>[^,]*),(?<DESTINATION_USER>[^,]*),(?<APPLICATION>[^,]*),(?<VIRTUAL_SYSTEM>[^,]*),(?<SOURCE_ZONE>[^,]*),(?<DESTINATION_ZONE>[^,]*),(?<INBOUND_INTERFACE>[^,]*),(?<OUTBOUND_INTERFACE>[^,]*),(?<LOG_ACTION>[^,]*),(?<TIME_LOGGED>[^,]*),(?<SESSION_ID>[^,]*),(?<REPEAT_COUNT>[^,]*),(?<SOURCE_PORT>[^,]*),(?<DESTINATION_PORT>[^,]*),(?<NAT_SOURCE_PORT>[^,]*),(?<NAT_DESTINATION_PORT>[^,]*),(?<FLAGS>[^,]*),(?<IP_PROTOCOL>[^,]*),(?<ACTION>[^,]*),(((?<BYTES>[^,]*),(?<BYTES_RCV>[^,]*),(?<BYTES_SEND>[^,]*),(?<PACKETS>[^,]*),(?<START_TIME>[^,]*),(?<ELAPSED>[^,]*),(?<URL_CATEGORY>[^,]*),(?<PADDING>[^\,]*))|((?<URL>[^,]*),(?<CONTENT_NAME>[^,]*),(?<CATEGORY>[^,]*),(?<SEVERITY>[^,]*),(?<DIRECTION>[^\,]*)))$");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _paloAltoUrlUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
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
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, string>("GetHeaderText", _paloAltoUrlUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_paloAltoUrlUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, string>("GetHeaderText", _paloAltoUrlUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
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
            MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, RecordInputType>("InputTextType", _paloAltoUrlUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, RecordInputType>("InputTextType", _paloAltoUrlUnifiedRecorder, new object[] { context, error });
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
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, RecordInputType>("InputTextType", _paloAltoUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Unknown);
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
            var context = new FileLineRecorderContext(_paloAltoUrlUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = text };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PaloAltoUrlUnifiedRecorder, RecordInputType>("InputTextType", _paloAltoUrlUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }
    }
}
