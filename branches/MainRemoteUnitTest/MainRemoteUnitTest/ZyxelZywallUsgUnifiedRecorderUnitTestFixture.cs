﻿using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.ZyxelZywallUsgUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for ZyxelZywallUsgUnifiedRecorderUnitTestFixture
    /// </summary>
    [TestFixture]
    public class ZyxelZywallUsgUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _zyxelZywall;

        /// <summary>
        /// Create a ZyxelZywallUsgUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _zyxelZywall = new ZyxelZywallUsgUnifiedRecorder();
        }

        /// <summary>
        /// Clear ZyxelZywallUsgUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _zyxelZywall.Clear();
            _zyxelZywall = null;
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
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, object>("Convert2Date", _zyxelZywall, new[] { rec, field, fieldValues, data });
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
            string[] fieldValues = { "Feb 17 14:28:25 2014" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, object>("Convert2Date", _zyxelZywall, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("2014/02/17 14:28:25",actual);

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
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, object>("Convert2Date", _zyxelZywall, new[] { rec, field, fieldValues, data });
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
            var actual = (int)MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, object>("Convert2Date", _zyxelZywall, new[] { rec, field, fieldvalues, data });
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
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, object>("Convert2Date", _zyxelZywall, new[] { rec, field, fieldValues, data });
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
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, "192.168.1.25")
            {
                HeaderInfo = null
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _zyxelZywall, new object[] { context });
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
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, syslogAddress: "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _zyxelZywall, new object[] { context });
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
            var match = Regex.Match("deneme", "[\"]+");
            string field = null;
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnFieldMatch", _zyxelZywall, new object[] { context, field, match });
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
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnFieldMatch", _zyxelZywall, new object[] { context, field, match });
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
            var match = Regex.Match(null, ".*");
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnFieldMatch", _zyxelZywall, new object[] { context, field, match });
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
            var match = Regex.Match("deneme", null);
            // ReSharper restore AssignNullToNotNullAttribute
            string field = null;
            var context = new SyslogRecorderContext(_zyxelZywall, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ZyxelZywallUsgUnifiedRecorder, NextInstruction>("OnFieldMatch", _zyxelZywall, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

    }
}
