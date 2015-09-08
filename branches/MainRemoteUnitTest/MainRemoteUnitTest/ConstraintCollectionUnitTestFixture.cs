using System.Reflection;
using Natek.Helpers.Execution;
using Natek.Helpers.Limit;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class ConstraintCollectionUnitTestFixture
    {
        private ConstraintCollection<RecWrapper> _constraintCollection = null;

        [SetUp]
        public void SetUp()
        {
            _constraintCollection = new ConstraintCollection<RecWrapper>();
        }

        [TearDown]
        public void TearDown()
        {
            _constraintCollection = null;
        }

        /// <summary>
        /// Method Name : RemoveConstraint
        ///
        ///Method Description : The method remove constraits
        ///
        ///Test Scenario : If constraints not contain constrait
        ///
        ///Known Input :
        ///     * constraint = new TextSizeConstraint
        /// 
        ///Expected Output :
        ///	    * Return should null
        /// 
        /// </summary>
        [Test]
        public void RemoveConstraint_IfConstraintsNotContainConstrait_ReturnNull()
        {
            //Arrange
            var constraint = new TextSizeConstraint<RecWrapper>();

            //Act
            var actual = _constraintCollection.RemoveConstraint(constraint);

            //Assert
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : RemoveConstraint
        ///
        ///Method Description : The method remove constraits
        ///
        ///Test Scenario : If constraints contain constrait
        ///
        ///Known Input :
        ///     * Constraint constraint = new TextSizeConstraint { Property = new TextProperty() };
        ///     * _constraintCollection.AddConstraint(constraint)
        /// 
        ///Expected Output :
        ///	    * Return should constraint
        /// 
        /// </summary>
        [Test]
        public void RemoveConstraint_IfConstraintsContainConstrait_ReturnConstraint()
        {
            //Arrange
            Constraint<RecWrapper> constraint = new TextSizeConstraint<RecWrapper> { Property = new TextProperty() };
            _constraintCollection.AddConstraint(constraint);

            //Act
            var actual = _constraintCollection.RemoveConstraint(constraint);

            //Assert
            Assert.AreEqual(constraint, actual);

        }

        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check the constrait if constrait is not null call the apply method
        ///
        ///Test Scenario : If constraints is null
        ///
        ///Known Input :
        ///     *  FieldTestHelper.SetInstanceFieldValue("Constraints", _constraintCollection, null)
        ///     *  recWrapper = new RecWrapper()
        ///     *  context = null
        /// 
        ///Expected Output :
        ///	    * Return should next instruction do
        /// 
        /// </summary>
        [Test]
        public void Apply_IfConstraintsIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            FieldTestHelper.SetInstanceFieldValue("Constraints", _constraintCollection, null);
            var recWrapper = new RecWrapper();
            object context = null;
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = _constraintCollection.Apply(recWrapper, context);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);

        }

        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check the constrait if constrait is not null call the apply method
        ///
        ///Test Scenario : If target is null
        ///
        ///Known Input :
        ///     * RecWrapper target = null
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * Return should next instruction do
        /// 
        /// </summary>
        [Test]
        public void Apply_IfTargetIsNull_ReturnNextInsturctionDo()
        {
            //Arrange
            RecWrapper target = null;
            object context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = _constraintCollection.Apply(target, context);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }

        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check the constrait if constrait is not null call the apply method
        ///
        ///Test Scenario : If constraints are empty
        ///
        ///Known Input :
        ///     * target = new RecWrapper()
        ///     * context = null
        /// 
        /// 
        ///Expected Output :
        ///	    * Return should next instruction do
        /// 
        /// </summary>
        [Test]
        public void Apply_IfConstraintsAreEmpty_ReturnNextInstructionDo()
        {
            //Arrange
            var target = new RecWrapper();
            object context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = _constraintCollection.Apply(target, context);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);
        }

        /// <summary>
        /// Method Name : Apply
        ///
        ///Method Description : The method check the constrait if constrait is not null call the apply method
        ///
        ///Test Scenario : If T type target is not null and object is null
        ///
        ///Known Input :
        ///     * var target = new RecWrapper()
        ///     * context = null
        ///     * constraint = new TextSizeConstraint{ Property = new TextProperty() { PropertyInfo = target.GetType().GetProperty("ComputerName", BindingFlags.Public | BindingFlags.Instance) } }
        ///     * constraint.Property.SetValue(target, context, "value")
        ///     * _constraintCollection.AddConstraint(constraint)
        /// 
        ///Expected Output :
        ///	    * Return should next instruction do
        /// 
        /// </summary>
        [Test]
        public void Apply_IfTTypeTargetIsNotNullAndObjectIsNull_ReturnNextInstructionDo()
        {
            //Arrange
            var target = new RecWrapper();
            object context = null;
            var constraint = new TextSizeConstraint<RecWrapper> { Property = new TextProperty() { PropertyInfo = target.GetType().GetProperty("ComputerName", BindingFlags.Public | BindingFlags.Instance) } };
            constraint.Property.SetValue(target, context, "value");
            _constraintCollection.AddConstraint(constraint);

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = _constraintCollection.Apply(target, context);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(NextInstruction.Do, actual);

        }
    }
}
