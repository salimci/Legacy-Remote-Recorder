using Natek.Helpers.IO.Readers;
using NUnit.Framework;
using System;
using Exception = System.Exception;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for FileLineRecorderContextUnitTestFixture
    /// </summary>
    [TestFixture]
    public class FileLineRecorderContextUnitTestFixture
    {
       private FileLineRecorderContext _filecontext = new FileLineRecorderContext();

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
           var actual = _filecontext.SetOffset(offset,ref error);
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
           _filecontext.Reader = new BufferedLineReader("add");
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
       public void ReadRecord_IfErrorIsNull_ReturnRecordSizeInBytes()
       {
           //Arrange
           Exception error = null;

           //Act
           var actual = _filecontext.ReadRecord(ref error);

           //Assert
           Assert.AreEqual(actual, 0);
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
       public void ReadRecord_IfErrorIsNotNull_ReturnRecordSizeInBytes()
       {
           //Arrange
           var error = new Exception();

           //Act
           var actual = _filecontext.ReadRecord(ref error);
           
           //Assert
           Assert.AreEqual(actual, 1);
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
           var actual = _filecontext.CreateReader(ref error);

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
       [Test(Description = "If Error is not null")]
       public void CreateReader_IfErrorNotNull_ReturnTrue()
       {
           //Arrange
           var error = new Exception();
        
           //Act
           var actual = _filecontext.CreateReader(ref error);

           //Assert
           Assert.AreEqual(actual, false);
       }

    }
}
