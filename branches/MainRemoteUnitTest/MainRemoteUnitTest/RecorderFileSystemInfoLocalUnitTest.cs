using System.Collections.Generic;
using System.IO;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class RecorderFileSystemInfoLocalUnitTest
    {
        private RecorderFileSystemInfoLocal _recorderFileSystemInfoLocal;

        /// <summary>
        /// Create a RecorderFileSystemInfoLocal object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            FileSystemInfo fileSystemInfo = new FileInfo(@"C:\Users\enise.kilavuz\Desktop\tst\cisco_asa");
            _recorderFileSystemInfoLocal = new RecorderFileSystemInfoLocal(fileSystemInfo);
        }

        /// <summary>
        /// RecorderFileSystemInfoLocal set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _recorderFileSystemInfoLocal = null;
        }

        /// <summary>
        /// Method Name : Refresh
        ///
        ///Method Description : Refresh instance
        ///
        ///Test Scenario : If instance is null
        ///
        ///Known Input :
        ///     * instance = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should return
        /// </summary>
        [Test(Description = "If instance is null")]
        public void Refresh_CreateContextInstance_ReturnFileLineRecorderContext()
        {
            //Arrange
            FieldTestHelper.SetInstanceFieldValue("instance", _recorderFileSystemInfoLocal, null);
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<RecorderFileSystemInfoLocal>("Refresh", _recorderFileSystemInfoLocal, null);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : GetFileSystemInfos
        ///
        ///Method Description : Get file system infos directory or file
        ///
        ///Test Scenario : If system is file
        ///
        ///Known Input :
        ///     * instance = FileSystemInfo
        /// 
        ///Expected Output :
        ///	    * null should return
        /// </summary>
        [Test(Description = " If system is file")]
        public void GetFileSystemInfos_IfFileSystemIsFile_ReturnNull()
        {
            //Arrange
           
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderFileSystemInfoLocal, IEnumerable<RecorderFileSystemInfo>>("GetFileSystemInfos", _recorderFileSystemInfoLocal, null);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : GetFileSystemInfos
        ///
        ///Method Description : Get file system infos directory or file
        ///
        ///Test Scenario : If system is directory
        ///
        ///Known Input :
        ///     * instance = DirectoryInfo
        /// 
        ///Expected Output :
        ///	    * notnull should return
        /// </summary>
        [Test(Description = " If system is directory")]
        public void GetFileSystemInfos_IfFileSystemIsDirectory_ReturnInformations()
        {
            //Arrange
            FileSystemInfo fileSystemInfo = new DirectoryInfo(@"C:\Users\enise.kilavuz\Desktop\tst\");
            _recorderFileSystemInfoLocal = new RecorderFileSystemInfoLocal(fileSystemInfo);

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<RecorderFileSystemInfoLocal, IEnumerable<RecorderFileSystemInfo>>("GetFileSystemInfos", _recorderFileSystemInfoLocal, null);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
