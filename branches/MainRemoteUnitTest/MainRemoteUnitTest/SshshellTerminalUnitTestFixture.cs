using System;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for SshshellTerminalUnitTestFixture
    /// </summary>
    [TestFixture]
    public class SshshellTerminalUnitTestFixture
    {
        private static Terminal _ssh;
        private static string host = "lorem";
        private static string name = "ipsum";
        private static string pass = "asda";

        [SetUp]
        public void TestFixtureSetup()
        {
            _ssh = new SshShellTerminal(host,name,pass);
        }

        /// <summary>
        /// Method Name : Connect
        ///
        ///Method Description :  The method provides connect to given port
        ///
        ///Test Scenario :  If error is null
        ///
        ///Known Input :
        ///    * error = null
        ///    
        ///Expected Output :
        ///    * Return should false
        /// </summary>
        [Test(Description = "If error is null")]
        public void Connect_IfErrorIsNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _ssh.Connect( ref error);
            //Assert

            Assert.AreEqual(actual,false);
        }

        /// <summary>
        /// Method Name : Connect
        ///
        ///Method Description : The method provides connect to given port
        ///
        ///Test Scenario : If error is not null
        ///
        ///Known Input :
        ///    * error = new Exception("lorem ipsum")
        ///    * port = 655366324
        ///    
        ///
        ///Expected Output :
        ///    * Return should false
        /// </summary>
        [Test(Description = "If error is not null")]
        public void Connect_IfErrorIsNotNull_ReturnFalse()
        {
            //Arrange
            var error = new Exception("lorem ipsum");
            const int port = 655366324;

            //Act
            var actual =_ssh.Connect(port, ref error);
         
            //Assert

            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : IsConnected
        ///
        ///Method Description :  The method check the connection 
        ///
        ///Test Scenario :  Call the is connected method
        ///
        ///Known Input :
        ///    * No input parameter
        ///    
        ///Expected Output :
        ///    Return should false
        /// </summary>
        [Test]
        public void IsConnected_CallTheIsConnectedMethod_ReturnFalse()
        {
            //Arrange

            //Act
            var actual = _ssh.IsConnected();

            //Assert

            Assert.AreEqual(actual,false);
        }

        /// <summary>
        /// Method Name : CanRead
        ///
        ///Method Description :  The method read the stream
        ///
        ///Test Scenario :  Call the CanRead method
        ///
        ///Known Input :
        ///    * No input parameter
        ///    
        ///
        ///Expected Output :
        ///    * Return should null
        /// </summary>
        [Test]
        public void CanRead_CallTheCanReadMethod_ReturnNull()
        {
            //Arrange

            //Act
            var actual = _ssh.CanRead();

            //Assert
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : CanWrite
        ///
        ///Method Description :  The method write the stream
        ///
        ///Test Scenario :  Call the CanWrite method 
        ///
        ///Known Input :
        ///    * No input parameter
        ///    
        ///
        ///Expected Output :
        ///    * Return should null
        /// </summary>
        [Test]
        public void CanWrite_CallTheCanWriteMethod_ReturnNull()
        {
            //Arrange

            //Act
            var actual = _ssh.CanWrite();

            //Assert

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : Write
        ///
        ///Method Description :  The method write the stream according to the parameter
        ///
        ///Test Scenario :  If buffer is null, offset and length are zero
        ///
        ///Known Input :
        ///    * buffer = null
        ///    * offset = 0
        ///    * length = 0
        ///    
        ///
        ///Expected Output :
        ///    * Return should length zero 
        /// </summary>
        [Test(Description = "If buffer is null, offset and length are zero")]
        public void Write_IfBufferIsNullOffsetAndLengthAreZero_ReturnLengthZero()
        {
            //Arrange
            byte[] buffer = null;
            var offset = 0;
            var length = 0;

            //Act
// ReSharper disable once ExpressionIsAlwaysNull
            var actual = _ssh.Write(buffer, offset, length);

            //Assert
            Assert.AreEqual(actual,0);
        }

        /// <summary>
        /// Method Name : Write
        ///
        ///Method Description : The method write the stream according to the parameter
        ///
        ///Test Scenario : If buffer is not null and offset and length have values 
        ///
        ///Known Input :
        ///    * buffer = new byte[10]
        ///    * offset = 10
        ///    * length = 10
        ///    
        ///
        ///Expected Output :
        ///    * Return should 10
        /// </summary>
        [Test(Description = "If buffer is not null and offset and length have values")]
        public void Write_IfBufferNotNull_ReturnZero()
        {
            //Arrange
            byte[] buffer = new byte[10];
            var offset = 10;
            const int length = 10;


            //Act
            var actual = _ssh.Write(buffer, offset, length);

            //Assert

            Assert.AreEqual(actual, 10);
        }

        /// <summary>
        /// Method Name : ReadByte
        ///
        ///Method Description :  The method read the stream by byte
        ///
        ///Test Scenario : Call the ReadByte method 
        ///
        ///Known Input :
        ///    * No input parameter
        ///    
        ///
        ///Expected Output :
        ///    * Return should null 
        /// </summary>
        [Test]
        public void ReadByte_CallTheReadByteMethod_ReturnReadByteStream()
        {
            //Arrange

            //Act
            var actual = _ssh.ReadByte();

            //Assert

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : Read
        ///
        ///Method Description : The method read the stream according to the parameter 
        ///
        ///Test Scenario : If buffer is null and offset and length are zero 
        ///
        ///Known Input :
        ///    * buffer = null
        ///    * offset = 0
        ///    * length = 0
        ///    
        ///
        ///Expected Output :
        ///    Return should null
        /// </summary>
        [Test(Description = " If buffer not null and offset and length are zero")]
        public void Read_IfBufferNotNull_ReturnNull()
        {
            //Arrange
            byte[] buffer = null;
            var offset = 0;
            const int length = 0;

            //Act
// ReSharper disable once ExpressionIsAlwaysNull
            var actual = _ssh.Read(buffer, offset, length);

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Read
        ///
        ///Method Description :   The method read the stream according to the parameter
        ///
        ///Test Scenario :  If buffer not null and offset and length have value
        ///
        ///Known Input :
        ///    * buffer = new byte[10]
        ///    * offset = 10
        ///    * length = 10 
        ///
        ///Expected Output :
        ///    * Return should 10
        /// </summary>
        [Test(Description = "If buffer is not null")]
        public void Read_IfBufferNotNull_Return()
        {
            //Arrange
            byte[] buffer = new byte[10];
            var offset = 10;
            const int length = 10;

            //Act
            var actual = _ssh.Read(buffer, offset, length);

            //Assert
            Assert.AreEqual(actual, 10);
        }

        /// <summary>
        /// Method Name : GetInputStream
        ///
        ///Method Description :  The method return the input stream
        ///
        ///Test Scenario :  If error is null
        ///
        ///Known Input :
        ///    * error = null
        ///    
        ///
        ///Expected Output :
        ///    * Return should null
        /// </summary>
        [Test(Description = "If error is null")]
        public void GetInputStream_IfPortAndWithinIsNotNull_ReturnNull()
        {
            //Arrange
            Exception error = null;
            //Act
            var actual = _ssh.GetInputStream(ref error);

            //Assert

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : GetOutputStream
        ///
        ///Method Description :  The method give the output stream
        ///
        ///Test Scenario :  If error is not null
        ///
        ///Known Input :
        ///    * error = new Exception()
        ///    
        ///
        ///Expected Output :
        ///    Return the stream 
        /// </summary>
        [Test(Description = "If error is not null")]
        public void GetOutputStream_IfErrorIsNotNull_ReturnStream()
        {
            //Arrange
            var error = new Exception();
            //Act
            var actual = _ssh.GetOutputStream(ref error);

            //Assert

            Assert.IsNull(actual);
        }



    }
}
