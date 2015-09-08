using Natek.Helpers.Patterns;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class DisposablePatternUnitTest
    {
        private DisposablePattern _disposablePattern;

        /// <summary>
        /// Create a DisposablePattern object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _disposablePattern = new DisposablePattern();
        }

        /// <summary>
        /// Method Name : Dispose
        /// 
        /// Method Desciption : Dispose
        /// 
        /// Test Scenerio : If disposing is true
        /// 
        /// Known Input :
        /// 
        /// Expected Output : 
        ///    
        /// </summary>
        [Test(Description = "If disposing is true")]
        public void Dispose_IfdisposingTrue_ChangeDisposedTrue()
        {
            //Act
           
            //Arrange
            var actual = (bool)FieldTestHelper.GetInstanceFieldValue("disposed", _disposablePattern);

            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : Dispose
        /// 
        /// Method Desciption : Dispose
        /// 
        /// Test Scenerio : If disposing is false
        /// 
        /// Known Input :
        /// 
        /// Expected Output : 
        ///    
        /// </summary>
        [Test(Description = "If disposing is false")]
        public void Dispose_IfdisposingFalse_ChangeDisposedTrue()
        {
            //Act

            //Arrange
            MethodTestHelper.RunInstanceMethod<DisposablePattern>("Dispose", _disposablePattern, new object[] { false });
            var actual = (bool)FieldTestHelper.GetInstanceFieldValue("disposed", _disposablePattern);

            //Assert
            Assert.IsTrue(actual);
        }
    }
}
