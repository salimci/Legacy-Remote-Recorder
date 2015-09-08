using Natek.Helpers;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class HttpHelperUnitTestFixture
    {
        /// <summary>
        /// Method Name : UrlDecode
        ///
        ///Method Description :  The method decode the Url
        ///
        ///Test Scenario :  If Url Is Null
        ///
        ///Known Input :
        ///    * url = null;
        ///    
        ///
        ///Expected Output :
        ///    Return should null
        /// </summary>
        [Test]
        public void UrlDecode_IfUrlIsNull_ReturnNull()
        {
            //Arrange
            string url = null;
            
            //Act
// ReSharper disable once ExpressionIsAlwaysNull
            var actual = HttpHelper.UrlDecode(url);

            //Assert
            Assert.IsNull(actual);
        }
    }
}
