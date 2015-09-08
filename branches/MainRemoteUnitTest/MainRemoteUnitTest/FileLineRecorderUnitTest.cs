using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.PaloAltoUrlUnifiedRecorder;
using NUnit.Framework;
using Exception = System.Exception;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class FileLineRecorderUnitTest
    {
        private FileLineRecorder _fileLineRecorder;

        /// <summary>
        /// Create a FileLineRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _fileLineRecorder = new PaloAltoUrlUnifiedRecorder();
        }

        /// <summary>
        /// Clear FileLineRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _fileLineRecorder.Clear();
            _fileLineRecorder = null;
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create context instance for own instance
        ///
        ///Test Scenario : Create context instance
        ///
        ///Known Input :
        ///     * params object[] ctxArgs
        /// 
        ///Expected Output :
        ///	    * FileLineRecorderContext should return
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CreateContextInstance_CreateContextInstance_ReturnFileLineRecorderContext()
        {
            //Arrange
            object[] ctxArgs = null;
           
            //Act
// ReSharper disable ExpressionIsAlwaysNull
             var actual = MethodTestHelper.RunInstanceMethod<FileLineRecorder, RecorderContext>("CreateContextInstance", _fileLineRecorder, new object[]{ctxArgs});
// ReSharper restore ExpressionIsAlwaysNull
            
            //Assert
            Assert.AreNotEqual(actual, null);
        }

        /// <summary>
        /// Method Name : InitContextInstance
        /// 
        /// Method Desciption : Initialize recorder context
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        ///     * ctxArgs = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "If context is null")]
        public void InitContextInstance_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            object[] ctxArgs = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<FileLineRecorder>("InitContextInstance", _fileLineRecorder, new object[] { context, ctxArgs });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled exception
        }

        /// <summary>
        /// Method Name : InitContextInstance
        ///
        ///Method Description : Initialize recorder context
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * ctxArgs = null
        ///
        ///Expected Output :
        ///    * NullReferenceException should occure
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InitContextInstance_IfContextInputRecordIsNull_NullReferenceException()
        {
            //Arrange
            var context = new FileLineRecorderContext(_fileLineRecorder) { InputRecord = null };
            object[] ctxArgs = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<FileLineRecorder>("InitContextInstance", _fileLineRecorder, new object[] { context, ctxArgs });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            // //Unhandled exception
        }

        /// <summary>
        /// Method Name : ApplyContextMapping
        ///
        ///Method Description : Apply context mapping 
        ///
        ///Test Scenario : If fields is null
        ///
        ///Known Input :
        ///    * context = null
        ///    * fields = null
        ///    * error = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should occure
        /// </summary>
        [Test(Description = "If fields is null")]
        public void ApplyContextMapping_IfFieldsIsNull_ReturnNextInstructionSkip()
        {
            //Arrange
            RecorderContext context = null;
            string[] fields = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileLineRecorder, NextInstruction>("ApplyContextMapping", _fileLineRecorder, new object[] { context, fields, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

        /// <summary>
        /// Method Name : ApplyContextMapping
        ///
        ///Method Description : Apply context mapping 
        ///
        ///Test Scenario : If fields is empty
        ///
        ///Known Input :
        ///    * context = null
        ///    * fields = string.Empty
        ///    * error = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should occure
        /// </summary>
        [Test(Description = "If fields is empty")]
        public void ApplyContextMapping_IfFieldsIsEmpty_ReturnNextInstructionSkip()
        {
            //Arrange
            RecorderContext context = null;
            string[] fields = {};
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileLineRecorder, NextInstruction>("ApplyContextMapping", _fileLineRecorder, new object[] { context, fields, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

        /// <summary>
        /// Method Name : ApplyContextMapping
        ///
        ///Method Description : Apply context mapping 
        ///
        ///Test Scenario : If fields is not empty
        ///
        ///Known Input :
        ///    * context = FileLineRecorderContext
        ///    * fields = "lorem", "ipsum"
        ///    * error = null
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should not occure
        /// </summary>
        [Test(Description = "If fields is not empty")]
        public void ApplyContextMapping_IfFieldsIsNotEmpty_ReturnNextInstructionSkip()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext();
            string[] fields = { "lorem", "ipsum" };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileLineRecorder, NextInstruction>("ApplyContextMapping", _fileLineRecorder, new object[] { context, fields, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }
    }
}
