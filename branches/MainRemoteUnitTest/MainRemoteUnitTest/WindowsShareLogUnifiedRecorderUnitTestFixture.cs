using System;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Microsoft.Share;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class WindowsShareLogUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _windowsSharelog;

        /// <summary>
        /// Create a WindowsShareLogUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
             _windowsSharelog = new WindowsShareLogUnifiedRecorder();
        }

        /// <summary>
        /// Clear WindowsShareLogUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _windowsSharelog.Clear();
            _windowsSharelog = null;
        }

        /// <summary>
        /// Method Name : InputTextType
        /// 
        /// Method Desciption : Determined the input record type
        /// 
        /// Test Scenerio : If context is null
        /// 
        /// Known Input :
        ///     * context = null
        ///     * error = null
        /// 
        /// Expected Output : 
        ///     * NullReferenceException should occure
        /// </summary>
        [Test(Description = "InputTextType tested if context is null")]
        public void InputTextType_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, RecordInputType>("InputTextType", _windowsSharelog, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert

            Assert.AreEqual(actual, RecordInputType.Comment);
        }
        
        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord is null")]
        public void InputTextType_IfContextInputRecordIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_windowsSharelog) { InputRecord = null };
            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, RecordInputType>("InputTextType", _windowsSharelog, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context.InputRecord.RecordText is null
        ///
        ///Known Input :
        ///    * context = RecorderContext with null InputRecord.RecordText
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.comment should return
        /// </summary>
        [Test(Description = "If context.InputRecord.RecordText is null")]
        public void InputTextType_IfContextInputRecordRecordTextIsNull_ReturnComment()
        {
            //Arrange
            var context = new FileLineRecorderContext(_windowsSharelog);
            var inputTextRecord = new TextRecord { RecordText = null };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, RecordInputType>("InputTextType", _windowsSharelog, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Comment);
        }

        /// <summary>
        /// Method Name : InputTextType
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context is true
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord that initiate
        ///    * error = null
        ///
        ///Expected Output :
        ///    * RecordInputType.Record should return
        /// </summary>
        [Test(Description = "If context is true")]
        public void InputTextType_IfContextInputRecordRecordTextIsRecordLikeString_ReturnRecord()
        {
            //Arrange

            const string text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit";
            var context = new FileLineRecorderContext(_windowsSharelog);
            var inputTextRecord = new TextRecord { RecordText = text };
            context.InputRecord = inputTextRecord;

            Exception error = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, RecordInputType>("InputTextType", _windowsSharelog, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Return string.Empty
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * context = null
        ///
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetHeaderText_IfContextIsNull_ReturnStringEmpty()
        {
            //Arrange

            RecorderContext context = null;


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, string>("GetHeaderText", _windowsSharelog, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Return string.Empty
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * context = FileLineRecorderContext(_ftp2012UnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_windowsSharelog);


            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, string>("GetHeaderText", _windowsSharelog, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }
        
        /// <summary>
        /// Method Name : ExtractUsername
        ///
        ///Method Description : Split the user name from input
        ///
        ///Test Scenario : If all parameters are null
        ///
        ///Known Input :
        ///       *  RecWrapper rec = null;
        ///       *  string field = null;
        ///       *  string[] values = null;
        ///       *  object data = null;
        /// 
        ///Expected Output :
        ///    * string.Empty should return
        /// 
        /// </summary>
        [Test(Description = " If all parameters are null")]
        public void ExtractUsername_IfValuesNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values= null;
            object data = null;
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, object>("ExtractUsername", _windowsSharelog, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        /// Method Name : ExtractUsername
        ///
        ///Method Description : Split the user name from input
        ///
        ///Test Scenario : If all parameters are null
        ///
        ///Known Input :
        ///       *  RecWrapper rec = null;
        ///       *  string field = null;
        ///       *  string[] values = null;
        ///       *  object data = null;
        /// 
        ///Expected Output :
        ///    * string.Empty should return
        /// 
        /// </summary>
        [Test(Description = " If all parameters are null")]
        public void ExtractUsername_IfValuesNotNull_ReturnValue()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] values = {"lorem ipsum","sit","amet"};
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, object>("ExtractUsername", _windowsSharelog, new[] { rec, field, values, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }

        /// <summary>
        /// Method Name : OnBeforeSetData
        ///
        ///Method Description : Determined the input record type
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * context = RecorderContext with InputRecord that initiate
        ///    
        ///Expected Output :
        ///    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void OnBeforeSetData_IfContextNotNull_ReturnNextInstructionDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext(_windowsSharelog);
            context.Record.Datetime = "2014-09-15 14:12:53";
            const string text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit";
            var inputTextRecord = new TextRecord { RecordText = text };
            context.InputRecord = inputTextRecord;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, NextInstruction>("OnBeforeSetData", _windowsSharelog, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : GetLastProcessedFile
        ///
        ///Method Description : Get last process file
        ///
        ///Test Scenario : If context is null
        ///
        ///Known Input :
        ///    * FileLineRecorderContext context = null;
        ///    
        ///Expected Output :
        ///    * false should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetLastProcessedFile_IfContextIsNull_ReturnFalse()
        {
            //Arrange

            FileLineRecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, bool>("GetLastProcessedFile", _windowsSharelog, new object[] { context, false });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }
        
        /// <summary>
        /// Method Name : GetLastProcessedFile
        ///
        ///Method Description : Get last process file
        ///
        ///Test Scenario : If context is not null
        ///
        ///Known Input :
        ///    * FileLineRecorderContext context = null;
        ///   
        ///Expected Output :
        ///    * true should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetLastProcessedFile_IfContextIsNull_ReturnTrue()
        {
            //Arrange;
            var context = new FileLineRecorderContext(_windowsSharelog);
            context.LastFile = "asdas";
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<WindowsShareLogUnifiedRecorder, bool>("GetLastProcessedFile", _windowsSharelog, new object[] { context, true });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }
    }
}
