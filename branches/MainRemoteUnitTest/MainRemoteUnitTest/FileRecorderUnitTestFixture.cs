using Microsoft.VisualStudio.TestTools.UnitTesting;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.PaloAltoUrlUnifiedRecorder;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for FileRecorderUnitTestFixture
    /// </summary>
    [TestClass]
    public class FileRecorderUnitTestFixture
    {
        private FileRecorder _fileRecorder;
// ReSharper disable once UnassignedField.Compiler
        private RecorderBase _base;
        private FileRecorderContext _context;



        /// <summary>
        /// Create a FileRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _fileRecorder = new PaloAltoUrlUnifiedRecorder();
            _context = new FileLineRecorderContext(_base); 
        }

        /// <summary>
        /// Clear FileRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _fileRecorder.Clear();
            _fileRecorder = null;
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CompareFiles_IfRecorderFileSystemInfoIsNull_ReturnZero()
        {
            //Arrange
            RecorderFileSystemInfo l = null;
            RecorderFileSystemInfo r = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, int>("CompareFiles", _fileRecorder, new object[] { l,r });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(actual, null);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CompareFiles_IfRecorderFileSystemInfoIsNotNull_ReturnZero()
        {
            //Arrange

            RecorderFileSystemInfo l = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum","lorem");
            RecorderFileSystemInfo r = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum", "lorem");
            
            
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, int>("CompareFiles", _fileRecorder, new object[] { l, r });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(actual, null);
        }
        
            
        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CompareFilesByModDate_IfRecorderFileSystemInfoIsNotNull_ReturnZero()
        {
            //Arrange

            RecorderFileSystemInfo l = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum","lorem");
            RecorderFileSystemInfo r = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum", "lorem");
            
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, int>("CompareFilesByModDate", _fileRecorder, new object[] { l, r });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CompareFilesByModDate_IfRecorderFileSystemInfoIsDiffrentEachOther_ReturnZero()
        {
            //Arrange

            RecorderFileSystemInfo l = new TerminalRemoteFileSystemInfo(_context, "lorem", "ipsum");
            RecorderFileSystemInfo r = new TerminalRemoteFileSystemInfo(_context, "lorem ipsum", "lorem");

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, int>("CompareFilesByModDate", _fileRecorder, new object[] { l, r });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CompareFilesByDayName_IfRecorderFileSystemInfoIsNotNull_ReturnZero()
        {
            //Arrange

            RecorderFileSystemInfo l = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum","lorem");
            RecorderFileSystemInfo r = new TerminalRemoteFileSystemInfo(_context,"lorem ipsum", "lorem");
            
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, int>("CompareFilesByDayName", _fileRecorder, new object[] { l, r });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 0);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CreateFileSystemInfo_IfContextAndFullNameIsNull_ReturnRecorderFileSystemInfo()
        {
            //Arrange
            RecorderContext context = null;
            const string fullName = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, RecorderFileSystemInfo>("CreateFileSystemInfo", _fileRecorder, new object[] { context, fullName });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void CreateFileSystemInfo_IfContextAndFullNameIsNotNull_ReturnRecorderFileSystemInfo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext();
            const string fullName = "lorem ipsum";

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, RecorderFileSystemInfo>("CreateFileSystemInfo", _fileRecorder, new object[] { context, fullName });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void DoLogic_IfContextIsNull_ReturnNextInstructionAbort()
        {
            //Arrange
            FileRecorderContext context = null;
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, NextInstruction>("DoLogic", _fileRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void DoLogic_IfContextIsNotNull_ReturnNextInstructionAbort()
        {
            //Arrange

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, NextInstruction>("DoLogic", _fileRecorder, new object[] { _context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, 8);
        }

        
        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void DoLogic_IfFileRecorderContextRecordSentEqualZero_ReturnNextInstructionAbort()
        {
            //Arrange
            _context.RecordSent = 0;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, NextInstruction>("DoLogic", _fileRecorder, new object[] { _context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void DoLogic_IfFileRecorderContextNotNull_ReturnNextInstructionAbort()
        {
            //Arrange
            FileRecorderContext cnt = new FileLineRecorderContext();
            _context.RecordSent = 100;
            _context.CurrentFile = new TerminalRemoteFileSystemInfo(_context);

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, NextInstruction>("DoLogic", _fileRecorder, new object[] { cnt });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description : 
        ///
        ///Test Scenario : 
        ///
        ///Known Input :
        ///     * 
        ///Expected Output :
        ///	    * 
        /// 
        /// </summary>
        [Test(Description = "Create context instance")]
        public void DoLogic_IfRecorderContextNotNull_ReturnNextInstructionAbort()
        {
            //Arrange
            _context.RecordSent = 10;
            RecorderContext rc = new FileLineRecorderContext();
            _context.CurrentFile = new TerminalRemoteFileSystemInfo(rc, "", "");
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<FileRecorder, NextInstruction>("DoLogic", _fileRecorder, new object[] { _context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }
    }
}
