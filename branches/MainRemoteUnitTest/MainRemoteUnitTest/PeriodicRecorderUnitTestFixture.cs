using System.Timers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Linux.Ssh;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.PaloAltoUrlUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for PeriodicRecorderUnitTestFixture
    /// </summary>
    [TestFixture]
    public class PeriodicRecorderUnitTestFixture
    {
        private PeriodicRecorder _periodic = new LinuxHistoryRecorder();

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "")]
        public void ValidateGlobalParameters_IfParametersAreNull_ReturnNextInstructionDo()
        {
            //Arrange

            //Act
// ReSharper disable once ExpressionIsAlwaysNull
// ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<PeriodicRecorder, NextInstruction>("ValidateGlobalParameters", _periodic, new object[] {""});
// ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }
    }
}
