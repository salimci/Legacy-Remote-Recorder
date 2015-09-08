using System;
using Natek.Helpers;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class ExceptionHelperUnitTest
    {
        /// <summary>
        /// Method Name : Catch
        /// 
        /// Method Desciption : Catch exceptions
        /// 
        /// Test Scenerio : If actual method is null
        /// 
        /// Known Input :
        ///     * actualMethod = actualIsNull
        ///     * cathscenario = null
        /// 
        /// Expected Output : 
        ///     * null should return
        /// </summary>
        [Test(Description = "If actual method is null")]
        public void Catch_IfActualMethodIsNull_ReturnNull()
        {
            //Act
            ExceptionHelper.ExceptionableMethod<Exception> actualMethod = ActualIsNull;
            ExceptionHelper.CatchSenario <Exception> cathscenario = null;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = ExceptionHelper.Catch(actualMethod, cathscenario);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        private static Exception ActualIsNull(object[] args)
        {
            return null;
        }

        /// <summary>
        /// Method Name : Catch
        /// 
        /// Method Desciption : Catch exceptions
        /// 
        /// Test Scenerio : If actual method is not null
        /// 
        /// Known Input :
        ///     * actualMethod = actualIsNotNull
        ///     * cathscenario = null
        /// 
        /// Expected Output : 
        ///     * "lorem ipsum" should return
        /// </summary>
        [Test(Description = "If actual method is null")]
        public void Catch_IfActualMethodIsNotNull_CheckMessage()
        {
            //Act
            ExceptionHelper.ExceptionableMethod<Exception> actualMethod = ActualIsNotNull;
            ExceptionHelper.CatchSenario<Exception> cathscenario = null;

            //Arrange
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = ExceptionHelper.Catch(actualMethod, cathscenario);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual.Message, "lorem ipsum");
        }

        private static Exception ActualIsNotNull(object[] args)
        {
            return new Exception("lorem ipsum");
        }
    }
}
