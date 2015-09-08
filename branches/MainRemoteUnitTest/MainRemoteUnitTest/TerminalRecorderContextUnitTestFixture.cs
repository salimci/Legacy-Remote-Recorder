using System;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh.Apache;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for TerminalRecorderContextUnitTestFixture
    /// </summary>
    [TestFixture]
    public class TerminalRecorderContextUnitTestFixture
    {
        private readonly TerminalRecorderContext _terminalcntx = new ApacheAccessRecorderContext();

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
        [Test(Description = "If Error is null")]
        public void ReadRecord_IfErrorIsNull_ReturnNull()
        {
            //Arrange
            Exception error = null;

            //Act
           var actual = _terminalcntx.ReadLine(ref error);

            //Assert
            Assert.AreEqual(actual,null);
        }

        
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
        [Test(Description = "If Error is null")]
        public void ReadRecord_IfErrorIsNull_ReturnZero()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _terminalcntx.ReadRecord(ref error);

            //Assert
            Assert.AreEqual(actual,0);
        }

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
        [Test(Description = "If Error is null")]
        public void SendCommand_IfErrorAndCommandIsNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;
            const string command = "";
            

            //Act
            var actual = _terminalcntx.SendCommand(command,ref error);

            //Assert
            Assert.AreEqual(actual, false);
        }


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
        [Test(Description = "If Error and Command is not null")]
        public void SendCommand_IfErrorAndCommandNotNull_ReturnFalse()
        {
            //Arrange
            Exception error = new Exception();
            const string command = "lorem ipsum sit amet";


            //Act
            var actual = _terminalcntx.SendCommand(command, ref error);

            //Assert
            Assert.AreEqual(actual, false);
        }

        
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
        [Test(Description = "If Error and Command is not null")]
        [ExpectedException(typeof(Exception))]
        public void ExecuteRemoteCommand_IfErrorAndCommandNotNull_ReturnFalse()
        {
            //Arrange
            const string command = "lorem";
            TerminalRecorderContext.OnRecordReadDelegate onRecordRead = delegate(object[] objects) {};
            object[] args = null;
            


            //Act
            _terminalcntx.ExecuteRemoteCommand(command, onRecordRead, args);

            //Assert
            
        }


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
        [Test(Description = "If Error is null")]
        public void CreateStream_IfErrorIsNull_ReturnNextInstructionAbort()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _terminalcntx.CreateStream(ref error);

            //Assert
            Assert.AreEqual(actual,NextInstruction.Abort);
        }

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
        [Test(Description = "If Error is null")]
        public void CloseStream_IfErrorIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var error = new Exception();

            //Act
            var actual = _terminalcntx.CreateStream(ref error);

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

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
        [Test(Description = "If Error is null")]
        public void CreateStream_IfErrorIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var error = new Exception();

            //Act
            var actual = _terminalcntx.CloseStream(ref error);

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        
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
        [Test(Description = "If Error is null")]
        public void CreateReader_IfErrorIsNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _terminalcntx.CreateReader(ref error);

            //Assert
            Assert.AreEqual(actual, false);
        }

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
        [Test(Description = "If Error is null")]
        public void CreateReader_IfErrorIsNotNull_ReturnFalse()
        {
            //Arrange
            var error = new Exception();

            //Act
            var actual = _terminalcntx.CreateReader(ref error);

            //Assert
            Assert.AreEqual(actual, false);
        }



    }
}
