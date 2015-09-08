using System.Net.Sockets;
using LogMgr;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.JuniperSyslogV6UnifiedRecorder;
using NUnit.Framework;
using SharpSSH.SharpSsh.java;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class SyslogRecorderBaseUnitTest
    {
        private SyslogRecorderBase _syslogRecorderBase;

        /// <summary>
        /// Create a JuniperSyslogV6UnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _syslogRecorderBase = new JuniperSyslogV6UnifiedRecorder();
        }

        /// <summary>
        /// Clear JuniperSyslogV6UnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _syslogRecorderBase.Clear();
            _syslogRecorderBase = null;
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create context instance
        ///
        ///Test Scenario : Create context instance
        ///Known Input :
        ///     * object[] ctxArgs = null
        ///Expected Output :
        ///	    * SyslogRecorderContext should return
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CreateContextInstance_CreateContextInstance_ReturnSyslogRecorderContext()
        {
            //Arrange
            object[] ctxArgs = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, object>("CreateContextInstance", _syslogRecorderBase, new object[] { ctxArgs });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(actual, null);
        }

        /// <summary>
        /// Method Name : ValidateGlobalParameters
        ///
        ///Method Description : Validate global parameters
        ///
        ///Test Scenario : Validate global parameters
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "Validate global parameters")]
        public void ValidateGlobalParameters_ValidateGlobalParameters_ReturnNextInstructionDo()
        {
            //Arrange

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, NextInstruction>("ValidateGlobalParameters", _syslogRecorderBase, new object[] {  });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : ProcessSyslogEvent
        ///
        ///Method Description : Process syslog event with arguman message
        ///
        ///Test Scenario : If args is null
        /// 
        ///Known Input :
        ///     * args = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If args is null")]
        public void ProcessSyslogEvent_IfArgsIsNull_NullReferenceException()
        {
            //Arrange
            LogMgrEventArgs args = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, NextInstruction>("ProcessSyslogEvent", _syslogRecorderBase, new object[] { args });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unnhandled NullReferenceException 
        }

        /// <summary>
        /// Method Name : GetContextPosition
        ///
        ///Method Description : Get context position
        ///
        ///Test Scenario : If context is null
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetContextPosition_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, NextInstruction>("GetContextPosition", _syslogRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unnhandled NullReferenceException 
        }
        
        /// <summary>
        /// Method Name : GetContextPosition
        ///
        ///Method Description : Get context position
        ///
        ///Test Scenario : Return last record date
        /// 
        ///Known Input :
        ///     * context = SyslogRecorderContext
        /// 
        ///Expected Output :
        ///	    * "22.09.2014" should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetContextPosition_LastRecordDate_Return()
        {
            //Arrange
            RecorderContext context = new SyslogRecorderContext(_syslogRecorderBase,ProtocolType.Udp, "", 514);
            context.LastRecordDate = "22.09.2014";

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, string>("GetContextPosition", _syslogRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "22.09.2014");
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
            MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, RecordInputType>("InputTextType", _syslogRecorderBase, new object[] { context, error });
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
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_syslogRecorderBase) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, RecordInputType>("InputTextType", _syslogRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.toString().length is equal zero
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordLengthIsZero_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_syslogRecorderBase);
            var inputTextRecord = new TextRecord { RecordText = string.Empty };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<SyslogRecorderBase, RecordInputType>("InputTextType", _syslogRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }
    }
}
