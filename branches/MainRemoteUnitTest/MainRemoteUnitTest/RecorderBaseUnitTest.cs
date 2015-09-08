using System;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class RecorderBaseUnitTest
    {
        private RecorderBase _recorderBase;

        /// <summary>
        /// Method Name : Convert2Int64
        ///
        ///Method Description : Convert string to int 64
        ///
        ///Test Scenario : If values null
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = null
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If values null")]
        public void Convert2Int64_IfValuesNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<RecorderBase>("Convert2Int64", _recorderBase, new[] {record, field,values,data});
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : Convert2Int64
        ///
        ///Method Description : Convert string to int 64
        ///
        ///Test Scenario : If Int 64 max value
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = Int64.MaxValue
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  Int64.MaxValue should occurence
        /// </summary>
        [Test(Description = "If Int 64 max value")]
        public void Convert2Int64_IfInt64MaxVal_ReturnInt64Max()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = { Int64.MaxValue.ToString() };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, Int64.MaxValue);
        }

        /// <summary>
        /// Method Name : Convert2Int64
        ///
        ///Method Description : Convert string to int 64
        ///
        ///Test Scenario : If values is string
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = "lorem ipsum"
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  0 should return
        /// </summary>
        [Test(Description = "If values is string")]
        public void Convert2Int64_IfValuesIsString_Return0()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = { "lorem ipsum" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : Convert2Int64
        ///
        ///Method Description : Convert string to int 64
        ///
        ///Test Scenario : If values not null
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = "12"
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  12 should occurence
        /// </summary>
        [Test(Description = "If values not null")]
        public void Convert2Int64_IfValuesNotNull_Return12()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = {"12"};
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 12);
        }

        /// <summary>
        /// Method Name : Convert2Int32
        ///
        ///Method Description : Convert string to int 32
        ///
        ///Test Scenario : If values null
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = null
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If values null")]
        public void Convert2Int32_IfValuesNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<RecorderBase>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : Convert2Int32
        ///
        ///Method Description : Convert string to int 32
        ///
        ///Test Scenario : If Int 32 max value
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = Int32.MaxValue
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    * Int32.MaxValue should occurence
        /// </summary>
        [Test(Description = "If Int 32 max value")]
        public void Convert2Int32_IfInt64MaxVal_ReturnInt32MaxValue()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = { Int32.MaxValue.ToString() };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, Int32.MaxValue);
        }

        /// <summary>
        /// Method Name : Convert2Int32
        ///
        ///Method Description : Convert string to int 32
        ///
        ///Test Scenario : If values is string
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = "lorem ipsum"
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  0 should return
        /// </summary>
        [Test(Description = "If values is string")]
        public void Convert2Int32_IfValuesIsString_Return0()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = { "lorem ipsum" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : Convert2Int32
        ///
        ///Method Description : Convert string to int 32
        ///
        ///Test Scenario : If values not null
        ///
        ///Known Input :
        ///     * record = null
        ///     * field = null
        ///     * values = "12"
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    *  12 should occurence
        /// </summary>
        [Test(Description = "If values not null")]
        public void Convert2Int32_IfValuesNotNull_Return12()
        {
            //Arrange
            RecWrapper record = null;
            string field = null;
            string[] values = { "12" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderBase, object>("Convert2Int64", _recorderBase, new[] { record, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 12);
        }

        /// <summary>
        /// Method Name : ClearRecord
        ///
        ///Method Description : Clear Record
        ///
        ///Test Scenario : If rec is not null
        ///
        ///Known Input :
        ///     * rec = RecWrapper
        ///    
        ///Expected Output :
        ///	    No Return
        /// </summary>
        [Test(Description = "If rec is not null")]
        public void ClearRecord_IfRecIsNotNull_NotReturn()
        {
            //Arrange
            var rec = new RecWrapper();
            
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<RecorderBase>("ClearRecord", _recorderBase, new object[] { rec });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            
        }
    }
}
