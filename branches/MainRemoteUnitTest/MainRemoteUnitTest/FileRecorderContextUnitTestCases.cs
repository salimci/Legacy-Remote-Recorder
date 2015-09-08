using Natek.Helpers.Execution;
using NUnit.Framework;
using System;
namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for FileRecorderContextUnitTestCases
    /// </summary>
    [TestFixture]
    public class FileRecorderContextUnitTestCases
    {
        private FileRecorderContext _filecontext = new FileLineRecorderContext();

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
        [Test(Description = "If Error and Offset is null")]
        public void SetOffset_IfErrorAndOffsetIsNull_ReturnFalse()
        {
            //Arrange
            long offset = 0;
            Exception error = null;

            //Act
            var actual = _filecontext.SetOffset(offset, ref error);
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
        [Test(Description = "If Error and Offset is not null")]
        public void SetOffset_IfErrorAndOffsetIsNotNull_ReturnTrue()
        {
            //Arrange
            const long offset = 123;
            Exception error = new ArgumentNullException();

            //Act
            var actual = _filecontext.SetOffset(offset, ref error);

            //Assert
            Assert.AreEqual(actual, true);
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
            var actual = _filecontext.CreateStream(ref error);

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
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
        [Test(Description = "If Error is not null")]
        public void CreateStream_IfErrorIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var error = new Exception();

            //Act
            var actual = _filecontext.CreateStream(ref error);

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
        [Test(Description = "If Error is not null")]
        public void CloseStream_IfErrorIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            Exception error = new Exception();

            //Act
            var actual = _filecontext.CloseStream(ref error);

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
        public void CloseStream_IfErrorIsNull_ReturnNextInstructionAbort()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _filecontext.CloseStream(ref error);

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
        [Test(Description = "If Error is not null")]
        public void FixOffsets_IfErrorIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            const NextInstruction nextInstruction = NextInstruction.Break;
            const long offset = 10;
            var headerOff = new long[5];
            var error = new Exception();

            //Act
// ReSharper disable once ExpressionIsAlwaysNull
            var actual = _filecontext.FixOffsets(nextInstruction, offset, headerOff, ref error);

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }



    }
}
