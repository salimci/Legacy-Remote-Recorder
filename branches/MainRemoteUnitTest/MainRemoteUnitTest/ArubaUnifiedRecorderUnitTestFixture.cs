using System.Net.Sockets;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.ArubaUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class ArubaUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _aruba;

        /// <summary>
        /// Create a ArubaUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _aruba = new ArubaUnifiedRecorder();
        }

        /// <summary>
        /// Clear ArubaUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _aruba.Clear();
            _aruba = null;
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, "192.168.1.25")
            {
                HeaderInfo = null
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _aruba, new object[] { context });
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, "192.168.1.25")
            {
                HeaderInfo = new DataMappingInfo()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnBeforeProcessRecordInput", _aruba, new object[] { context });
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnFieldMatch", _aruba, new object[] { context, field, match });
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnFieldMatch", _aruba, new object[] { context, field, match });
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
        /// 
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnFieldMatch", _aruba, new object[] { context, field, match });
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
            var context = new SyslogRecorderContext(_aruba, ProtocolType.Udp, syslogAddress: "192.168.1.25");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ArubaUnifiedRecorder, NextInstruction>("OnFieldMatch", _aruba, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }
    }
}
