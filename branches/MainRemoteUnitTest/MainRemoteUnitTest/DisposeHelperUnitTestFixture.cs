using Natek.Helpers;
using NUnit.Framework;
using System;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class DisposeHelperUnitTestFixture
    {
        /// <summary>
        /// Method Name : Close
        ///
        ///Method Description : The method dispose the pointer
        ///
        ///Test Scenario : If pointer is null
        ///
        ///Known Input :
        ///         *  IntPtr[] pointer = null
        ///Expected Output :
        ///         * no return
        /// </summary>
        [Test(Description = "If pointer is null")]
        public void Close_IfParameterIsNull_ReturnNull()
        {
            //Arrange
            IntPtr[] pointer = null;
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
             DisposeHelper.Close(pointer);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
              
        }

        /// <summary>
        /// Method Name : Close
        ///
        ///Method Description : The method dispose the pointer
        ///
        ///Test Scenario : If pointer is not null
        ///
        ///Known Input :
        ///         *  var pointer = new IntPtr[10];
        ///Expected Output :
        ///         * no return
        /// </summary>
        [Test(Description = "If pointer is not null")]
        public void Close_IfParameterIsNotNull_NotReturn()
        {
            //Arrange
            var pointer = new IntPtr[10];

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            DisposeHelper.Close(pointer);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
        }

        /// <summary>
        /// Method Name : Close
        ///
        ///Method Description : The method dispose parameter disposable 
        ///
        ///Test Scenario : If disposables is null
        ///
        ///Known Input :
        ///         *   IDisposable disposables = null
        ///Expected Output :
        ///         *   no return
        /// </summary>
        [Test(Description = "If disposables is null")]
        public void Close_IfDisposablesIsNull_NotReturn()
        {
            //Arrange
            IDisposable disposables = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            DisposeHelper.Close(disposables);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }
        
        /// <summary>
        /// Method Name :  Close
        ///
        ///Method Description : The method dispose parameter disposable 
        ///
        ///Test Scenario : If disposables is not null
        ///
        ///Known Input :
        ///         * var disposables = new IDisposable[100];
        ///Expected Output :
        ///         *   no return
        /// </summary>
        [Test(Description = "If disposables is not null")]
        public void Close_IfDisposablesIsNotNull_NotReturn()
        {
            //Arrange
            var disposables = new IDisposable[100];

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            DisposeHelper.Close(disposables);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }
        
        /// <summary>
        /// Method Name : CloseHandle
        ///
        ///Method Description : The method is close method handle 
        ///
        ///Test Scenario : If disposables is not null
        ///
        ///Known Input :
        ///         *  var pointer = new IntPtr(10)
        ///
        ///Expected Output :
        ///         *   No return
        /// </summary>
        [Test(Description = "If disposables is not null")]
        public void CloseHandle_IfPointersIsNotNull_NotReturn()
        {
            //Arrange
            var pointer = new IntPtr(10);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            DisposeHelper.CloseHandle(pointer);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

        }
    }
}
