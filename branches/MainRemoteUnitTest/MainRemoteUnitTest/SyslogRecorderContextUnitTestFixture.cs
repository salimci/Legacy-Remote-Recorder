using System;
using System.Net.Sockets;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for SyslogRecorderContextUnitTestFixture
    /// </summary>
    [TestFixture]
    public class SyslogRecorderContextUnitTestFixture
    {
// ReSharper disable once UnassignedField.Compiler
// ReSharper disable InconsistentNaming
        private RecorderBase recontx;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Method Name : ReadRecord 
        ///
        ///Method Description :  The method read record
        ///
        ///Test Scenario : If error is null but context is not null
        ///
        ///Known Input :
        ///    * context = new SyslogRecorderContext(recontx, ProtocolType.Udp, "192.168.1.25")
        ///    * Exception error = null
        ///    
        ///
        ///Expected Output :
        ///    Return should zero
        /// </summary>
        /// 
        [Test(Description = "If error is null but context is not null")]
        public void ReadRecord_IfErrorIsNullButContextIsNotNull_ReturnZero()
        {
            //Arrange
            var context = new SyslogRecorderContext(recontx, ProtocolType.Udp, "192.168.1.25");
            Exception error = null;

            //Act
            var actual = context.CreateReader(ref error);
            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : ReadRecord
        ///
        ///Method Description :  The method read record
        ///
        ///Test Scenario :  If error and context is not null
        ///
        ///Known Input :
        ///    * context = new SyslogRecorderContext(recontx, ProtocolType.Udp, "192.168.1.25")
        ///    * error = new Exception()
        ///    
        ///
        ///Expected Output :
        ///    Return should zero
        /// </summary>
        /// 
        [Test(Description = " If error and context is not null")]
        public void ReadRecord_IfErrorAndContextIsNotNull_ReturnZero()
        {
            //Arrange
            var context = new SyslogRecorderContext(recontx, ProtocolType.Udp, "192.168.1.25");
            var error = new Exception();

            //Act
            var actual = context.CreateReader(ref error);
            //Assert
            Assert.AreEqual(actual, 0);
        }


    }
}
