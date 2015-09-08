using Natek.Helpers.Execution;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for TextSizeConstraitUnitTestFixture
    /// </summary>
    [TestFixture]
    public class TextSizeConstraitUnitTestFixture
    {

        private SizeConstraint<RecWrapper> st;
            
        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check property, if property not null set value property
        ///
        ///Test Scenario :If target and context is null
        ///
        ///Known Input :
        ///    * target = null
        ///    * context = null
        ///    * pro = new TextProperty()
        ///
        ///Expected Output :
        ///    * Return should NextIInstruction.Do
        /// 
        /// </summary>
        [Test(Description = "If Type T and Context is null")]
        public void Apply_IfTargetAndContextIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            RecWrapper target = null;
            object context = null;
            var pro = new TextProperty();
            //Act

// ReSharper disable ExpressionIsAlwaysNull
            var actual = st.Apply(target, context);
// ReSharper restore ExpressionIsAlwaysNull
            //Assert
            Assert.AreEqual(actual,NextInstruction.Do);
        }


        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check property, if property not null set value property
        ///
        ///Test Scenario :If target and context is not null
        ///
        ///Known Input :
        ///    * target = new RecWrapper()
        ///    * context = "lorem ipsum"
        ///    * pro = new TextProperty()
        ///
        ///Expected Output :
        ///    * Return NextInstructionDo
        /// 
        /// </summary>
        [Test(Description = "If Type T and Context is not null")]
        public void Apply_IfTargetAndContextIsNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            var target = new RecWrapper();
            object context = "lorem ipsum";
            var pro = new TextProperty();
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = st.Apply(target, context);
            // ReSharper restore ExpressionIsAlwaysNull
            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }
    }
}
