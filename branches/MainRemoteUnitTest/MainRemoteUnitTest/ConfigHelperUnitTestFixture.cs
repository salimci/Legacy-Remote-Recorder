using System;
using Natek.Helpers.Config;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class ConfigHelperUnitTestFixture
    {

        private bool OnUnhandledKeywordValueTrueCase(string keyword, bool quotedKeyword, string value, bool quotedValue, bool keywordValueError, ref int touchCount, ref Exception error)
        {
            return true;
        }

        private bool OnWhitespaceTrueCase(string ws, ref System.Exception error)
        {
            return true;
        }

        private bool OnSeparatorValueTrueCase(string separator, ref System.Exception error)
        {
            return true;
        }

        private bool OnKeywordValueTrueCase(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref System.Exception error)
        {
            return true;
        }

        private bool OnUnhandledKeywordValueFalseCase(string keyword, bool quotedKeyword, string value, bool quotedValue, bool keywordValueError, ref int touchCount, ref Exception error)
        {
            return false;
        }

        private bool OnWhitespaceFalseCase(string ws, ref System.Exception error)
        {
            return false;
        }

        private bool OnSeparatorValueFalseCase(string separator, ref System.Exception error)
        {
            return false;
        }

        private bool OnKeywordValueFalseCase(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref System.Exception error)
        {
            return false;
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If Keyword is null
        ///
        ///Known Input :
        ///     * keywords = null
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should true
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfKeywordIsNull_ReturnTrue()
        {
            //Arrange
            string keywords = null;
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueTrueCase, OnSeparatorValueTrueCase,
                OnWhitespaceTrueCase, OnUnhandledKeywordValueTrueCase, ref error);
            
            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If keyword is empty
        ///
        ///Known Input :
        ///     * keywords = string.Empty
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should true
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfKeywordIsEmpty_ReturnTrue()
        {
            //Arrange
            var keywords = string.Empty;
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueTrueCase, OnSeparatorValueTrueCase,
                OnWhitespaceTrueCase, OnUnhandledKeywordValueTrueCase, ref error);

            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex not match and last index less then keyword length and on keyword value is null
        ///
        ///Known Input :
        ///     * keywords = "Lorem"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexNotMatchAndLastIndexLessThenKeywordLengthAndOnKeywordValueIsNull_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem";
            Exception error = null;
            
            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, null, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex match and last index less then keyword length and on keyword value is null
        ///
        ///Known Input :
        ///     *  keywords = "Lorem=ipsum;Lorem=ipsum;"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexMatchAndLastIndexLessThenKeywordLengthAndOnKeywordValueIsNull_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem=ipsum;Lorem=ipsum;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, null, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex match and last index less then keyword length
        ///
        ///Known Input :
        ///     * keywords = "Lorem=ipsum;Lorem=ipsum;"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexMatchAndLastIndexLessThenKeywordLength_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem=ipsum;Lorem=ipsum;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueTrueCase, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex not match and last index less then keyword length
        ///
        ///Known Input :
        ///     *  keywords = "Lorem;"
        ///     *  error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexNotMatchAndLastIndexLessThenKeywordLength_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueTrueCase, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert
            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex not match and last index less then keyword length false on whitespace
        ///
        ///Known Input :
        ///     * keywords = "Lorem;"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexNotMatchAndLastIndexLessThenKeywordLengthFalseOnWhitespace_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueTrueCase, OnSeparatorValueTrueCase, OnWhitespaceFalseCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex match and last index less then keyword length negative
        ///
        ///Known Input :
        ///     * keywords = "Lorem=ipsum;Lorem=ipsum;"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexMatchAndLastIndexLessThenKeywordLengthNegative_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem=ipsum;Lorem=ipsum;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueFalseCase, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : ParseKeywords
        ///
        ///Method Description : The method parsed keywords
        ///
        ///Test Scenario : If regex not match and last index less then keyword length negative
        ///
        ///Known Input :
        ///     * keywords = "Lorem;"
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test]
        public void ParseKeywords_IfRegexNotMatchAndLastIndexLessThenKeywordLengthNegative_ReturnFalse()
        {
            //Arrange
            const string keywords = "Lorem;";
            Exception error = null;

            //Act
            var actual = ConfigHelper.ParseKeywords(keywords, OnKeywordValueFalseCase, OnSeparatorValueTrueCase, OnWhitespaceTrueCase,
                OnUnhandledKeywordValueTrueCase, ref error);

            //Assert

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : Unescape
        ///
        ///Method Description : The method unescape to given data
        ///
        ///Test Scenario : If input string is empty
        ///
        ///Known Input :
        ///     * inputString = string.Empty
        /// 
        ///Expected Output :
        ///	    * Return should input string
        /// 
        /// </summary>
        [Test]
        public void Unescape_IfEmpty_ReturnInputString()
        {
            //Arrange
            var inputString = string.Empty;

            //Act
            var actual = ConfigHelper.Unescape(inputString);

            //Assert
            Assert.AreEqual(inputString, actual);
        }

        /// <summary>
        /// Method Name : Unescape
        ///
        ///Method Description : The method unescape to given data
        ///
        ///Test Scenario :  If input string is null
        ///
        ///Known Input :
        ///     * inputString = null
        /// 
        ///Expected Output :
        ///	    * Return should input string
        /// 
        /// </summary>
        [Test]
        public void Unescape_IfNull_ReturnInputString()
        {
            //Arrange
            string inputString = null;

            //Act
            var actual = ConfigHelper.Unescape(inputString);

            //Assert
            Assert.AreEqual(inputString, actual);
        }

        /// <summary>
        /// Method Name : Unescape
        ///
        ///Method Description : The method unescape to given data
        ///
        ///Test Scenario : Unescape Slash T, Unescape Slash T, Unescape Slash N, Unescape Slash R
        ///
        ///Known Input :
        ///     * lorem\\t
        ///     * lorem\\b
        ///     * lorem\\n
        ///     * lorem\\r
        /// 
        ///Expected Output :
        ///     * lorem\\t
        ///     * lorem\\b
        ///     * lorem\\n
        ///     * lorem\\r
        /// 
        /// </summary>
        [TestCase("lorem\\t", Result = "lorem\t", TestName = "UnescapeSlashT")]
        [TestCase("lorem\\b", Result = "lorem\b", TestName = "UnescapeSlashT")]
        [TestCase("lorem\\n", Result = "lorem\n", TestName = "UnescapeSlashN")]
        [TestCase("lorem\\r", Result = "lorem\r", TestName = "UnescapeSlashR")]
        public string Unescape_To_EscapedChars(string inputString)
        {
            //Act
            return ConfigHelper.Unescape(inputString);
        }
    }
}
