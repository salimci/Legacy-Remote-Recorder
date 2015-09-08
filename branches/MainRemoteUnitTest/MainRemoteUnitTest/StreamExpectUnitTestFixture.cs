using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Natek.Helpers.IO.Reader;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class StreamExpectUnitTestFixture
    {
        private StreamExpect _streamExpect;

        [SetUp]
        public void SetUp()
        {
            _streamExpect = new StreamExpect();
        }

        [TearDown]
        public void TearDown()
        {
            _streamExpect = null;
        }


        /// <summary>
        /// Method Name : Next
        ///
        ///Method Description :  The method 
        ///
        ///Test Scenario : If buffer and error are null 
        ///
        ///Known Input :
        ///    * StringBuilder buffer = null
        ///    * int nl
        ///    * Exception error
        ///    
        ///
        ///Expected Output :
        ///    Return should stream expect result error
        /// </summary>
        [Test]
        public void Next_WithNullBuffer_ReturnStreamExpectResultError()
        {
            //Arrange
            StringBuilder buffer = null;
            int nl;
            Exception error;

            //Act
            var actual = _streamExpect.Next(buffer, out nl, out error);

            //Assert
            Assert.AreEqual(StreamExpectResult.Error, actual);
        }


        /// <summary>
        /// Method Name : Next
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
        [Test]
        public void Next_WithEmptyStream_ReturnStreamExpectedResultError()
        {
            //Arrange
            var buffer = new StringBuilder();
            int nl;
            Exception error;

            //Act
            var actual = _streamExpect.Next(buffer, out nl, out error);

            //Assert
            Assert.AreEqual(StreamExpectResult.Error, actual);
        }

        /// <summary>
        /// Method Name : Next
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
        [Test]
        public void Next_StreamWithoutEndLine_ReturnStreamExpectedResultEof()
        {
            //Arrange
            const string testString = "lorem ipsum";
            _streamExpect.Stream = new MemoryStream(ConverterHelper.GetBytes(testString));
            var buffer = new StringBuilder();
            int nl;
            Exception error;

            //Act
            var actual = _streamExpect.Next(buffer, out nl, out error);

            //Assert
            Assert.AreEqual(StreamExpectResult.Eof, actual);
        }


        /// <summary>
        /// Method Name : Next
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
        [Test]
        public void Next_StreamEndLineInTheMiddle_ReturnStreamExpectedResultLine()
        {
            //Arrange
            const string testString = "lorem ipsum";
            byte[] bytes = ConverterHelper.GetBytes(testString);
            bytes[4] = 10;
            _streamExpect.Stream = new MemoryStream(bytes);
            var buffer = new StringBuilder();
            int nl;
            Exception error;

            //Act
            var actual = _streamExpect.Next(buffer, out nl, out error);

            //Assert
            Assert.AreEqual(StreamExpectResult.Line, actual);
        }


        /// <summary>
        /// Method Name : Next
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
        [Test]
        public void Next_TwoThreadReadSameStream()
        {
            //Arrange
            const string testString = "lorem ipsum";
            byte[] bytes = ConverterHelper.GetBytes(testString);
            bytes[4] = 10;
            _streamExpect.Stream = new MemoryStream(bytes);
            var buffer = new StringBuilder();
            int nl;
            Exception error;
            var threadPool = new List<Thread>();

            for(var i = 0; i < 10; i++)
            {
                 threadPool.Add(new Thread(() => _streamExpect.Next(buffer, out nl, out  error)));
            }

            foreach (var thread in threadPool)
            {
                thread.Start();
            }


        }
    }
}
