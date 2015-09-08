using System.IO;
using Natek.Helpers.IO.Readers;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class BufferedSteamReaderUnitTestFixture
    {

        private BufferedStreamReader _bufferedStreamReader;

        [SetUp]
        public void SetUp()
        {
            const string testString = "lorem ipsum";
            _bufferedStreamReader = new BufferedStreamReader(new MemoryStream(ConverterHelper.GetBytes(testString)));
        }

        /// <summary>
        /// Method Name : Reset
        ///
        ///Method Description : Reset the buffer
        ///
        ///Test Scenario : _bufferedStreamReader call the Reset method 
        ///
        ///Known Input :
        ///     * 
        /// 
        ///Expected Output :
        ///	    * Buffer should be reset
        /// 
        /// </summary>
        [Test]
        public void Reset_ResetTheBuffer()
        {
            _bufferedStreamReader.Reset();
        }

        /// <summary>
        /// Method Name : Reset
        ///
        ///Method Description : Read the buffer
        ///
        ///Test Scenario : _bufferedStreamReader call the Read method
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * Buffer should be read
        /// 
        /// </summary>
        [Test]
        public void Read_ReadTheBuffer()
        {
            _bufferedStreamReader.Read();
        }
    }
}
