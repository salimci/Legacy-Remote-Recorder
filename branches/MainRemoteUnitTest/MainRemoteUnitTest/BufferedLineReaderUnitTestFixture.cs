using System.IO;
using System.Text;
using Natek.Helpers.IO.Readers;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class BufferedLineReaderUnitTestFixture
    {
        /// <summary>
        /// Method Name : Reset
        ///
        ///Method Description : The method reset the buffer
        /// 
        ///Test Scenario : If test string and buffered line reader iare not null
        ///
        ///Known Input :
        ///     * testString = "lorem ipsum"
        ///     * bytes = ConverterHelper.GetBytes(testString)
        ///     * bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes))
        ///Expected Output :
        ///	    * Reset the buffer
        /// 
        /// </summary>
        [Test]
        public void Reset_ResetTheBuffer()
        {
            const string testString = "lorem ipsum";
            var bytes = ConverterHelper.GetBytes(testString);
            bytes[10] = 10;
            var bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes));

            bufferedLineReader.Reset();
        }

        /// <summary>
        /// Method Name : ReadLine
        ///
        ///Method Description : The method read the buffer 
        ///
        ///Test Scenario : If test string and buffered line reader are not null
        ///
        ///Known Input :
        ///     *    const string testString = "lorem ipsum";
        ///     *    bytes = ConverterHelper.GetBytes(testString);
        ///     *    bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes));
        ///    
        ///Expected Output :
        ///	    * Buffer should read 
        /// 
        /// </summary>
        [Test]
        public void ReadLine_ReadTheBuffer()
        {
            const string testString = "lorem ipsum";
            var bytes = ConverterHelper.GetBytes(testString);
            bytes[10] = (int)'\n';
            var bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes));
            int nl = 0;

            bufferedLineReader.ReadLine(new UTF8Encoding(), ref nl);
        }

        /// <summary>
        /// Method Name :  ReadLine
        ///
        ///Method Description : The method read the buffer 
        ///
        ///Test Scenario : Read the buffer with slash r
        ///
        ///Known Input :
        ///     * var bytes = ConverterHelper.GetBytes(testString)
        ///     * bytes[9] = (int) '\r'
        ///     * bytes[10] = (int) '\n'
        ///     * bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes))
        /// 
        ///Expected Output :
        ///	    * Buffer should read 
        /// 
        /// </summary>
        [Test]
        public void ReadLine_ReadTheBufferWithSlashR()
        {
            const string testString = "lorem ipsum";
            var bytes = ConverterHelper.GetBytes(testString);
            bytes[9] = (int) '\r';
            bytes[10] = (int) '\n';
            var bufferedLineReader = new BufferedLineReader(new MemoryStream(bytes));
            int nl = 0;

            bufferedLineReader.ReadLine(new UTF8Encoding(), ref nl);
        }
    }
}
