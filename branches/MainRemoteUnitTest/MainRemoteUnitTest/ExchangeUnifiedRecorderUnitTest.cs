using System;
using System.Text.RegularExpressions;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Microsoft.Exchange;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class ExchangeUnifiedRecorderUnitTest
    {
        private static RecorderBase _exchangeUnifiedRecorder;

        /// <summary>
        /// Create a ExchangeUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _exchangeUnifiedRecorder = new ExchangeUnifiedRecorder();
        }

        /// <summary>
        /// Clear ExchangeUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _exchangeUnifiedRecorder.Clear();
            _exchangeUnifiedRecorder = null;
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If data is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If data is null")]
        public void Convert2Date_IfDataIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder>("Convert2Date", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is "yyyy-M-d'T'H:m:s.fff'Z'", return "yyyy/MM/dd HH:mm:ss" this date format
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "2014-09-09T08:54:15.000Z"
        ///     * data = NetscalerUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return 2014/09/09 08:54:15
        /// </summary>
        [Test(Description = "If date time format is yyyy-M-d'T'H:m:s.fff'Z', return yyyy/MM/dd HH:mm:ss this date format")]
        public void Convert2Date_IfDateTimeFormatIsYYYY_M_d_Return_YYYY_MM_dd()
        {
            //Arrange
            var rec = new RecWrapper();
            var field = String.Empty;
            string[] values = { "2014-09-09T08:54:15.000Z" };
            object data = new ExchangeUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("Convert2Date", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2014/09/09 08:54:15");
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is not expected, return String.Empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = "2014/09/09 08:54:15"
        ///     * data = NetscalerUnifiedRecorder
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If date time format is not expected, return String.Empty")]
        public void Convert2Date_IfDateTimeFormatIsNotCorrect_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "2014/09/09 08:54:15" };
            object data = new ExchangeUnifiedRecorder();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("Convert2Date", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If recorder is null, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = "2014/09/09T08:54:15.000Z"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If recorder is null, return string.Empty")]
        public void Convert2Date_IfDataIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { "2014/09/09T08:54:15.000Z" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("Convert2Date", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario : If values[0] is empty, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = {String.Empty}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If values[0] is empty, return string.Empty")]
        public void CustomStr1Splitter_IfValuesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = { string.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario : If values[0] is null, return string.Empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return String.Empty
        /// </summary>
        [Test(Description = "If values[0] is null, return string.Empty")]
        public void CustomStr1Splitter_IfValuesIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario : If values[0].legth() is smaller than 900 bytes, return string
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = String.Empty
        ///     * values = {"Lorem"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return "Lorem"
        /// </summary>
        [Test(Description = " If values[0].legth() is smaller than 900 bytes, return string")]
        public void CustomStr1Splitter_IfValuesLengthIsSmallerThan900Bytes_ReturnString()
        {
            //Arrange
            RecWrapper rec = null;
            var field = String.Empty;
            string[] values = { "Lorem" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario : If values[0].length() is bigger than 900 bytes but smaller than 1800 bytes, return string
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = String.Empty
        ///     * values = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem 
        ///      ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ipsum dolor sit amet consectetur adipisicing elit "
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  Return first 900 bytes
        /// </summary>
        [Test(Description = " If values[0].length() is bigger than 900 bytes but smaller than 1800 bytes, return string")]
        public void CustomStr1Splitter_IfValuesLengthIsBiggerThan900BytesButSmallerThan1800Bytes_ReturnStringFirst900Bytes()
        {
            //Arrange
            RecWrapper rec = null;
            var field = String.Empty;
            string[] values = { "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit " };
            object data = null;
            const string expected = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lore";


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario :  If values[0] is bigger than 1800 bytes, return string first 900 bytes
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = String.Empty
        ///     * values = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor
        ///     sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///    consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ipsum dolor sit amet consectetur adipisicing elit"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem O" should return
        /// </summary>
        [Test(Description = "  If values[0] is bigger than 1800 bytes, return string first 900 bytes")]
        public void CustomStr1Splitter_IfValuesLengthIsBiggerThan1800Bytes_ReturnStringFirst900Bytes()
        {
            //Arrange
            var rec = new RecWrapper();
            var field = String.Empty;
            string[] values = { "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolorsit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consecteturadipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Loremipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ipsum dolor sit amet consectetur adipisicing elit" };
            object data = null;
            const string expected = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem O";


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        /// Method Name : CustomStr1Splitter
        ///
        ///Method Description : Return given customStr1 value's first 900 bytes
        ///
        ///Test Scenario :  If values[0].length() is bigger than 1800 bytes and recorder null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = String.Empty
        ///     * values = "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor
        ///     sit amet consectetur adipisicing elit Lorem 
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///    consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///      adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor 
        ///     sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit 
        ///     Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit
        ///     amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur 
        ///     adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem
        ///     ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet
        ///     consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing
        ///     elit Lorem ipsum dolor sit amet consectetur adipisicing elit"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  string.Empty
        ///         /// </summary>
        [Test(Description = "  If values[0].length() is bigger than 1800 bytes and recorder null")]
        public void CustomStr1Splitter_IfValuesLengthIsBiggerThan1800BytesAndRecorderNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            var field = String.Empty;
            string[] values =
            {
                "Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ORTANOKTA ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolorsit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consecteturadipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sitamet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicing elit Loremipsum dolor sit amet consectetur adipisicing elit Lorem ipsum dolor sit ametconsectetur adipisicing elit Lorem ipsum dolor sit amet consectetur adipisicingelit Lorem ipsum dolor sit amet consectetur adipisicing elit"
            };
            object data = null;


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("CustomStr1Splitter",
                _exchangeUnifiedRecorder, new[] {rec, field, values, data});
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  IndexOutOfRangeException  should return
        /// </summary>
        [Test(Description = "If values is null")]
        public void DescriptionSplitter_IfValuesIsNull_ReturnIndexOutOfRangeException()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { null };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRangeException 
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values[0] is empty, return values[1]
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {string.Empty,"Lorem"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[0] is empty, return values[1]")]
        public void DescriptionSplitter_IfValues0IsEmpty_ReturnValues1()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = {string.Empty, "Lorem"};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values[0] is null, return values[1]
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {null,"Lorem"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[0] is null, return values[1]")]
        public void DescriptionSplitter_IfValues0IsNull_ReturnValues1()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = {null, "Lorem" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values[1] is empty, return values[0]
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", string.Empty}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[1] is empty, return values[0]")]
        public void DescriptionSplitter_IfValues1IsEmpty_ReturnValues0()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", string.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values[1] is null, return values[0]
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", null}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[1] is null, return values[0]")]
        public void DescriptionSplitter_IfValues1IsNull_ReturnValues0()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", null };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : DescriptionSplitter
        ///
        ///Method Description : Concaniate given description with - character
        ///
        ///Test Scenario : If values[0] is not empty or null, concaniate given description
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", "ipsum", "dolor"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lore-ipsum" should return
        /// </summary>
        [Test(Description = "If values[0] is not empty or null, concaniate given description")]
        public void DescriptionSplitter_IfValues0IsThreePart_ReturnConcaniatedString()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", "ipsum", "dolor" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("DescriptionSplitter", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem-ipsum");
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If values is null")]
        public void SetIpAddress_IfValuesIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = new[] {string.Empty}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  IndexOutOfRange should occurence
        /// </summary>
        [Test(Description = "If values is empty")]
        public void SetIpAddress_IfValuesIsEmpty_ReturnIndexOutOfRange()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            var values = new[] {string.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled IndexOutOfRange
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values[1] is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {String.Empty, String.Empty}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should occurence
        /// </summary>
        [Test(Description = "If values[1] is empty")]
        public void SetIpAddress_IfValues0and1AreEmpty_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { String.Empty, String.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values[1] is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {String.Empty, null}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If values[1] and values[0] are is empty")]
        public void SetIpAddress_IfValues0isEmptyand1isNull_ReturnStringEmpty()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { String.Empty, null };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values[1] is null
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", null}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[1] is null")]
        public void SetIpAddress_IfValues1isNull_ReturnValues0()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", null };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values[1] is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", String.Empty}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "Lorem" should return
        /// </summary>
        [Test(Description = "If values[1] is empty")]
        public void SetIpAddress_IfValues1isEmpty_ReturnValues0()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", String.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "Lorem");
        }

        /// <summary>
        /// Method Name : SetIpAddress
        ///
        ///Method Description : Determine ip address
        ///
        ///Test Scenario : If values[1] is empty
        ///
        ///Known Input :
        ///   	* rec = RecWrapper
        ///     * field = null
        ///     * values = {"Lorem", "ipsum"}
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  "ipsum" should return
        /// </summary>
        [Test(Description = "If values[1] is not empty")]
        public void SetIpAddress_IfValues1isNotEmpty_ReturnValues1()
        {
            //Arrange
            var rec = new RecWrapper();
            string field = null;
            string[] values = { "Lorem", "ipsum" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, object>("SetIpAddress", _exchangeUnifiedRecorder, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "ipsum");
        }

        /// <summary>
        /// Method Name : InputTextType
        /// 
        /// Method Desciption : Determined the input record type
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        ///     * error = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "InputTextType tested if context is null")]
        public void InputTextType_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            //Unhandled exception
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.toString().length is equal zero
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordLengthIsZero_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = string.Empty };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord starts with "#Lorem"
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord starts with #Lorem")]
        public void InputTextType_IfContextInputRecordStartsWithSharp_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = "#Lorem" };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord starts with "#Fields: "
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Header should return
        /// </summary>
        [Test(Description = "If context.InputRecord starts with #Fields: ")]
        public void InputTextType_IfContextInputRecordStartsWithSharpFields_ReturnHeader()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = "#Fields: " };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Header);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord is other posibilities
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Record should return
        /// </summary>
        [Test(Description = "If context.InputRecord is other posibilities")]
        public void InputTextType_IfContextInputRecord_ReturnRecord()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder);
            var inputTextRecord = new TextRecord { RecordText = "Lorem ipsum dolor" };
            context.InputRecord = inputTextRecord;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, RecordInputType>("InputTextType", _exchangeUnifiedRecorder, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        /// 
        /// Method Desciption : Get header line except first eight characters 
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "GetHeaderText tested if context is null")]
        public void GetHeaderText_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, string>("GetHeaderText", _exchangeUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Asset

            //Unhandled Exception
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///
        ///Expected Output :
        ///    * NullReferenceException should occure
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void GetHeaderText_IfContextInputRecordIsNull_NullReferenceException()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder) { InputRecord = null };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, string>("GetHeaderText", _exchangeUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled Exception
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If context.InputRecord.length() is smaller than eight
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///
        ///Expected Output :
        ///    * ArgumentOutOfRangeException should occure
        /// </summary>
        [Test(Description = "If context.InputRecord.length() is smaller than eight")]
        public void GetHeaderText_IfContextInputRecordLengthIsSmallerThanEigth_ArgumentOutOfRangeException()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "field" };
            context.InputRecord = inputTextRecord;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, string>("GetHeaderText", _exchangeUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled ArgumentOutOfRangeException
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Get header line except first eight characters 
        ///
        ///Test Scenario : If all cases is true
        ///                 string = "#Fields: ---"
        /// 
        ///Known Input : RecorderContext with InputRecord that initiate
        ///
        ///Expected Output :
        ///    * " ---" should return
        /// </summary>
        [Test(Description = "If all cases is true")]
        public void GetHeaderText_IfContextInputRecordIsTrue_ReturnExpectedString()
        {
            //Arrange
            var context = new FileLineRecorderContext(_exchangeUnifiedRecorder) { InputRecord = null };
            var inputTextRecord = new TextRecord { RecordText = "#Fields: ---" };
            context.InputRecord = inputTextRecord;
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, string>("GetHeaderText", _exchangeUnifiedRecorder, new object[] { context });


            //Assert
            Assert.AreEqual(actual, " ---");
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is wrong for header
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for header")]
        public void CreateHeaderSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, Regex>("CreateHeaderSeparator", _exchangeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for header
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for header")]
        public void CreateHeaderSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("([^,\"]*\"([^\"]*)\"[^,]*|([^,]*)),?");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, Regex>("CreateHeaderSeparator", _exchangeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create field separator with regex
        ///
        ///Test Scenario : If regex is wrong for field
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for field")]
        public void CreateFieldSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, Regex>("CreateFieldSeparator", _exchangeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for field
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for field")]
        public void CreateFieldSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("([^,\"]*\"([^\"]*)\"[^,]*|([^,]*)),?");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<ExchangeUnifiedRecorder, Regex>("CreateFieldSeparator", _exchangeUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }
    }
}
