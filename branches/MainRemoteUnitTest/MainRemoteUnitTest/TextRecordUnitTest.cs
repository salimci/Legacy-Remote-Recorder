using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class TextRecordUnitTest
    {
        private TextRecord _textRecord;

        /// <summary>
        /// Create a TextRecord object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _textRecord = new TextRecord();
        }

        /// <summary>
        /// TextRecord set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _textRecord = null;
        }

        /// <summary>
        /// Method Name : ToString
        ///
        ///Method Description : Return given string to string
        ///
        ///Test Scenario : Return RecordText
        ///
        ///Known Input :
        ///
        ///Expected Output :
        ///	    *  RecordText = "lorem ipsum"
        /// </summary>
        [Test(Description = "Return RecordText")]
        public void ToString_ReturnRecordText_ReturnRecordText()
        {
            //Arrange
            _textRecord .RecordText= "lorem ipsum";

            //Act
            var actual = _textRecord.ToString();
            
            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }

        /// <summary>
        /// Method Name : SetValue
        ///
        ///Method Description : Set RecordText's value with given paramaeter
        ///
        ///Test Scenario : If value is null
        ///
        ///Known Input :
        ///     * value = null
        ///Expected Output :
        ///	    *  RecordText = null
        /// </summary>
        [Test(Description = "If value is null")]
        public void SetValue_IfValueIsNull_NotReturn()
        {
            //Arrange
            _textRecord.SetValue(null);

            //Act
            var actual = _textRecord.RecordText;

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : SetValue
        ///
        ///Method Description : Set RecordText's value with given paramaeter
        ///
        ///Test Scenario : Check RecordText is equal given parameter
        ///
        ///Known Input :
        ///     * value = "lorem ipsum"
        /// 
        ///Expected Output :
        ///	    *  RecordText = "lorem ipsum"
        /// </summary>
        [Test(Description = "Check RecordText is equal given parameter")]
        public void SetValue_CheckRecordTextIsEqualGivenParameter_NotReturn()
        {
            //Arrange
            _textRecord.SetValue( "lorem ipsum");

            //Act
            var actual = _textRecord.RecordText;

            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }
    }
}
