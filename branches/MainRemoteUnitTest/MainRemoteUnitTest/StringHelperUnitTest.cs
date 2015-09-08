using System;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class StringHelperUnitTest
    {
        private static StringHelper _stringHelper;

        /// <summary>
        /// Create a StringHelper object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _stringHelper = new StringHelper(new StringComparison());
        }

        /// <summary>
        /// Method Name : MakeSureLength
        /// 
        /// Method Desciption : Check string length is expected
        /// 
        /// Test Scenerio : If s is null
        /// 
        /// Known Input :
        ///     * s = null
        ///     * maxLen = 0
        /// 
        /// Expected Output : 
        ///     * null should return
        /// </summary>
        [Test(Description = "If s is null")]
        public void MakeSureLength_IfSisNull_ReturnNull()
        {
            //Act
            string s = null;
            const int maxLen = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, string>("MakeSureLength", _stringHelper, new object[]{s, maxLen});
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : MakeSureLength
        /// 
        /// Method Desciption : Check string length is expected
        /// 
        /// Test Scenerio : If s is empty
        /// 
        /// Known Input :
        ///     * s = string.Empty
        ///     * maxLen = 0
        /// 
        /// Expected Output : 
        ///     * string.Empty should return
        /// </summary>
        [Test(Description = "If s is empty")]
        public void MakeSureLength_IfSisEmpty_ReturnEmpty()
        {
            //Act
            var s = string.Empty;
            const int maxLen = 0;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, string>("MakeSureLength", _stringHelper, new object[] { s, maxLen });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }
        
        /// <summary>
        /// Method Name : MakeSureLength
        /// 
        /// Method Desciption : Check string length is expected
        /// 
        /// Test Scenerio : If maxLen is negative
        /// 
        /// Known Input :
        ///     * s = lorem ipsum
        ///     * maxLen = -1
        /// 
        /// Expected Output : 
        ///     * "lorem ipsum" should return
        /// </summary>
        [Test(Description = "If maxLen is negative")]
        public void MakeSureLength_IfMaxLenIsNegative_ReturnS()
        {
            //Act
            var s = "lorem ipsum";
            const int maxLen = -1;

            //Arrange
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, string>("MakeSureLength", _stringHelper, new object[] { s, maxLen });

            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }

        /// <summary>
        /// Method Name : MakeSureLength
        /// 
        /// Method Desciption : Check string length is expected
        /// 
        /// Test Scenerio : If S.length is smaller than maxLen
        /// 
        /// Known Input :
        ///     * s = lorem ipsum
        ///     * maxLen = 25
        /// 
        /// Expected Output : 
        ///     * "lorem ipsum" should return
        /// </summary>
        [Test(Description = "If S.length is smaller than maxLen")]
        public void MakeSureLength_IfSlengthIsSmallerThanMaxLen_ReturnS()
        {
            //Act
            var s = "lorem ipsum";
            const int maxLen = 25;

            //Arrange
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, string>("MakeSureLength", _stringHelper, new object[] { s, maxLen });

            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }

        /// <summary>
        /// Method Name : MakeSureLength
        /// 
        /// Method Desciption : Check string length is expected
        /// 
        /// Test Scenerio : If S.length is bigger than maxLen
        /// 
        /// Known Input :
        ///     * s = lorem ipsum
        ///     * maxLen = 5
        /// 
        /// Expected Output : 
        ///     * "lorem" should return
        /// </summary>
        [Test(Description = "If S.length is smaller than maxLen")]
        public void MakeSureLength_IfSlengthIsBiggerThanMaxLen_ReturnSubstring()
        {
            //Act
            var s = "lorem ipsum";
            const int maxLen = 5;

            //Arrange
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, string>("MakeSureLength", _stringHelper, new object[] { s, maxLen });

            //Assert
            Assert.AreEqual(actual, "lorem");
        }

        /// <summary>
        /// Method Name : Equals
        /// 
        /// Method Desciption : Check the given strings are same
        /// 
        /// Test Scenerio : If the given strings are equals 
        /// 
        /// Known Input :
        ///     * x = lorem ipsum
        ///     * y = lorem ipsum
        /// 
        /// Expected Output : 
        ///     * true should return
        /// </summary>
        [Test(Description = "If the given strings are equals ")]
        public void Equals_IfTheGivenStringsAreEquals_ReturnTrue()
        {
            //Act
            const string x = "lorem ipsum";
            const string y = "lorem ipsum";

            //Arrange
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("Equals", _stringHelper, new object[] { x,y });

            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : GetHashCode
        /// 
        /// Method Desciption : Get given string's hash code
        /// 
        /// Test Scenerio : Get given string's hash code
        /// 
        /// Known Input :
        ///     * obj = lorem ipsum
        /// 
        /// Expected Output : 
        ///     * 10 should return
        /// </summary>
        [Test(Description = "Get given string's hash code")]
        public void GetHashCode_GetgivenStringshashcode_Returnhashcode()
        {
            //Act
            var obj = "lorem ipsum";

            //Arrange
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, int>("GetHashCode", _stringHelper, new object[] { obj });

            //Assert
            Assert.AreEqual(actual, 10);
        }

        /// <summary>
        /// Method Name : NullEmptyEquals
        /// 
        /// Method Desciption : Check null or empty equality given strings
        /// 
        /// Test Scenerio : If first and second parameters are null
        /// 
        /// Known Input :
        ///     * l = null
        ///     * r = null
        ///     * stringComparison =  new StringComparison()
        /// 
        /// Expected Output : 
        ///     * true should return
        /// </summary>
        [Test(Description = "If first and second parameter are null")]
        public void NullEmptyEquals_IfFirstAndSecondParametersAreNull_ReturnTrue()
        {
            //Act
            string l = null;
            string r = null;
            const StringComparison stringComparison = new StringComparison();

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("NullEmptyEquals", _stringHelper, new object[] { l, r, stringComparison });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : NullEmptyEquals
        /// 
        /// Method Desciption : Check null or empty equality given strings
        /// 
        /// Test Scenerio : If first parameter is null, second parameter is empty
        /// 
        /// Known Input :
        ///     * l = null
        ///     * r = String.Empty
        ///     * stringComparison =  new StringComparison()
        /// 
        /// Expected Output : 
        ///     * true should return
        /// </summary>
        [Test(Description = "If first parameter is null, second parameter is empty")]
        public void NullEmptyEquals_IfFirstParameterIsNullSecondParameterIsEmpty_ReturnTrue()
        {
            //Act
            string l = null;
            var r = string.Empty;
            const StringComparison stringComparison = new StringComparison();

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("NullEmptyEquals", _stringHelper, new object[] { l, r, stringComparison });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : NullEmptyEquals
        /// 
        /// Method Desciption : Check null or empty equality given strings
        /// 
        /// Test Scenerio : If second parameter is null, first parameter is empty
        /// 
        /// Known Input :
        ///     * l = String.Empty
        ///     * r = null
        ///     * stringComparison =  new StringComparison()
        /// 
        /// Expected Output : 
        ///     * true should return
        /// </summary>
        [Test(Description = "If second parameter is null, first parameter is empty")]
        public void NullEmptyEquals_IfSecondParameterIsNullFirstParameterIsEmpty_ReturnTrue()
        {
            //Act
            var l = string.Empty;
            string r = null;
            const StringComparison stringComparison = new StringComparison();

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("NullEmptyEquals", _stringHelper, new object[] { l, r, stringComparison });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : NullEmptyEquals
        /// 
        /// Method Desciption : Check null or empty equality given strings
        /// 
        /// Test Scenerio : If second parameter is null, first parameter is not empty
        /// 
        /// Known Input :
        ///     * l = lorem ipsum
        ///     * r = null
        ///     * stringComparison =  new StringComparison()
        /// 
        /// Expected Output : 
        ///     * false should return
        /// </summary>
        [Test(Description = "If second parameter is null, first parameter is not empty")]
        public void NullEmptyEquals_IfSecondParameterIsNullFirstParameterIsNotEmpty_ReturnFalse()
        {
            //Act
            const string l = "lorem ipsum";
            string r = null;
            const StringComparison stringComparison = new StringComparison();

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("NullEmptyEquals", _stringHelper, new object[] { l, r, stringComparison });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : NullEmptyEquals
        /// 
        /// Method Desciption : Check null or empty equality given strings
        /// 
        /// Test Scenerio : If first and second parameters are not null or empty
        /// 
        /// Known Input :
        ///     * l = lorem ipsum
        ///     * r = lorem ipsum
        ///     * stringComparison =  new StringComparison()
        /// 
        /// Expected Output : 
        ///     * true should return
        /// </summary>
        [Test(Description = "If first and second parameters are not null or empty")]
        public void NullEmptyEquals_IfFirstAndSecondParametersAreNotNullOrEmpty_ReturnTrue()
        {
            //Act
            const string l = "lorem ipsum";
            const string r = "lorem ipsum";
            const StringComparison stringComparison = new StringComparison();

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<StringHelper, bool>("NullEmptyEquals", _stringHelper, new object[] { l, r, stringComparison });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }
    }
}
