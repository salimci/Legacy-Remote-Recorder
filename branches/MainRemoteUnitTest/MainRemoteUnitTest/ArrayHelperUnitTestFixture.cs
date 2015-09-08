using Natek.Helpers.CSharp;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class ArrayHelperUnitTestFixture
    {
        /// <summary>
        /// Method Name : AssureLength
        ///
        ///Method Description : The method give the length according to the parameter 
        ///
        ///Test Scenario : If arrays is null with default allow null
        ///
        ///Known Input :
        ///     * testArray = null
        ///     * minLength = 0
        ///
        ///Expected Output :
        ///	    * Return should false
        /// 
        /// </summary>
        [Test(Description = "If arrays is null with default allow null")]
        public void AssureLength_IfArraysIsNullWithDefaultAllowNull_ReturnFalse()
        {
            //Arrange
            int[] testArray = null;
            const int minLength = 0;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = ArrayHelper.AssureLength(testArray, minLength);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : AssureLength
        ///
        ///Method Description : The method give the length according to the parameter
        ///
        ///Test Scenario : If array is null with allow null is true
        ///
        ///Known Input :
        ///     * testArray = null
        ///     * minLength = 0
        ///     * allowNull = true
        ///Expected Output :
        ///	    * Return should true
        /// 
        /// </summary>
        [Test(Description = "If array is null with allow null is true")]
        public void AssureLength_IfArrayIsNullWithAllowNullIsTrue_ReturnTrue()
        {
            int[] testArray = null;
            const int minLength = 0;
            const bool allowNull = true;

            //Act
            
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var actual = ArrayHelper.AssureLength(testArray, minLength, allowNull);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsTrue(actual);
        }


        /// <summary>
        /// Method Name : AssureLength
        ///
        ///Method Description : The method give the length according to the parameter
        ///
        ///Test Scenario : Check with varius length of test array
        ///
        ///Known Input :
        ///     * new[] { 1, 2 }, 1
        ///     * new[] { 1, 2 }, 2
        ///     * new[] { 1, 2 }, 3
        ///Expected Output :
        ///	    * Return should true 
        ///     * Return should true
        ///     * Return should false
        /// </summary>
        [TestCase(new[] { 1, 2 }, 1, Result = true, TestName = "IfArrayLengthGreatherThenMinLength", Description = "if input arrays length greather than min lenght return true")]
        [TestCase(new[] { 1, 2 }, 2, Result = true, TestName = "IfArrayLengthEqualMinLength", Description = "if input arrays length equal  min lenght return true")]
        [TestCase(new[] { 1, 2 }, 3, Result = false, TestName = "IfArrayLengthLessThenMinLength", Description = "if input arrays length less than min lenght return false")]
        public bool AssureLength_CheckWithVariusLengthOfTestArray_ReturnResultOfComperasion(int[] testArray, int minLength)
        {
            return ArrayHelper.AssureLength(testArray, minLength);
        }
    }
}
