using System;
using System.IO;
using Natek.Helpers.IO;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class FileSystemHelperUnitTestFixture
    {
        /// <summary>
        /// Method Name : Compare Files Ignore Case
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesIgnoreCase parameters are null
        ///
        ///Known Input :
        ///         * var l = string.Empty;
        ///         * var r = string.Empty;
        /// 
        ///Expected Output :
        ///         * Return should zero
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are null")]
        public void CompareFilesIgnoreCase_IfParameterIsNullForString_ReturnZero()
        {
            //Arrange
            var l = string.Empty;
            var r = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(0,actual);
        }
        
        /// <summary>
        /// Method Name : Compare Files Ignore Case
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesIgnoreCase parameters are not null
        ///
        ///Known Input :
        ///         * const string l = "lorem ipsum";
        ///         * const string r = "sit amet";
        /// 
        ///Expected Output :
        ///         * Return should 1
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are not null")]
        public void CompareFilesIgnoreCase_IfParameterNotNullForString_ReturnOne()
        {
            //Arrange
            const string l = "lorem ipsum";
            const string r = "sit amet";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(-7, actual);
        }
        
        /// <summary>
        /// Method Name : Compare Files No Ignore Case
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesNoIgnoreCase parameters are null
        ///
        ///Known Input :
        ///             * var l = string.Empty;
        ///             * var r = string.Empty;
        /// 
        ///Expected Output :
        ///             * return should 0
        /// </summary>
        [Test(Description = "If CompareFilesNoIgnoreCase parameters are null")]
        public void CompareFilesNoIgnoreCase_IfParameterIsNullForString_ReturnZero()
        {
            //Arrange
            var l = string.Empty;
            var r = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesNoIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(0, actual);
        }

        /// <summary>
        /// Method Name : Compare Files No Ignore Case
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesNoIgnoreCase parameters are not null
        ///
        ///Known Input :
        ///         * const string l = "lorem ipsum"
        ///         * const string r = "sit amet"
        /// 
        ///Expected Output :
        ///         * Return sholuld -7 
        /// </summary>
        [Test(Description = "If CompareFilesNoIgnoreCase parameters are not null")]
        public void CompareFilesNoIgnoreCase_IfParameterNotNullForString_ReturnOne()
        {
            //Arrange
            const string l = "lorem ipsum";
            const string r = "sit amet";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(-7, actual);
        }


        /// <summary>
        /// Method Name : CompareFilesIgnoreCase
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesIgnoreCase parameters are null
        ///
        ///Known Input :
        ///             * FileSystemInfo l = null;
        ///             * FileSystemInfo r = null;
        /// 
        ///Expected Output :
        ///             * return should zero
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are null")]
        public void CompareFilesIgnoreCase_IfCompareFilesIgnoreCaseParametersAreNullForFile_ReturnZero()
        {
            //Arrange
            FileSystemInfo l = null;
            FileSystemInfo r = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(0, actual);
        }


        /// <summary>
        /// Method Name : CompareFilesIgnoreCase
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : "If CompareFilesIgnoreCase parameters are not null
        ///
        ///Known Input :
        ///         * FileSystemInfo l = new FileInfo("");
        ///         * FileSystemInfo r = new FileInfo("");
        /// 
        ///Expected Output :
        ///         * Return should 1
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are not null")]
        public void CompareFilesIgnoreCase_IfCompareFilesIgnoreCaseParametersAreNotNullForFile_Returnone()
        {
            //Arrange
            FileSystemInfo l = new FileInfo("");
            FileSystemInfo r = new FileInfo("");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(1, actual);
        }

        /// <summary>
        /// Method Name : CompareFilesNoIgnoreCase
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesIgnoreCase parameters are null
        ///
        ///Known Input :
        ///            * FileSystemInfo l = null;
        ///            * FileSystemInfo r = null;
        /// 
        ///Expected Output :
        ///            * Return should 0
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are null")]
        public void CompareFilesNoIgnoreCase_IfCompareFilesIgnoreCaseParametersAreNullForFile_ReturnZero()
        {
            //Arrange
            FileSystemInfo l = null;
            FileSystemInfo r = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesNoIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(0, actual);
        }

        /// <summary>
        /// Method Name : CompareFilesNoIgnoreCase
        ///
        ///Method Description : The method takes 2 parameter and call the compare file method
        ///
        ///Test Scenario : If CompareFilesIgnoreCase parameters are not null
        ///
        ///Known Input :
        ///         * FileSystemInfo l = new FileInfo("");
        ///         * FileSystemInfo r = new FileInfo("");
        ///
        ///Expected Output :
        ///         *    return should 1
        /// </summary>
        [Test(Description = "If CompareFilesIgnoreCase parameters are not null")]
        public void CompareFilesNoIgnoreCase_IfCompareFilesIgnoreCaseParametersAreNotNullForFile_Returnone()
        {
            //Arrange
            FileSystemInfo l = new FileInfo("");
            FileSystemInfo r = new FileInfo("");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CompareFilesNoIgnoreCase(l, r);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(1, actual);
        }

        /// <summary>
        /// Method Name : CreateDirectory
        ///
        ///Method Description : The method create directory
        ///
        ///Test Scenario : If CreateDirectory parameters are empty
        ///
        ///Known Input :
        ///         * var path = ""
        ///         * Exception error = null
        ///
        ///Expected Output :
        ///         * Return should true
        /// </summary>
        [Test(Description = "If CreateDirectory parameters are null")]
        public void CreateDirectory_IfParameterAreEmpty_ReturnTrue()
        {
            //Arrange
            var path = "";
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateDirectory(path, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        /// Method Name : CreateDirectory
        ///
        ///Method Description : The method create directory
        ///
        ///Test Scenario : If CreateDirectory parameter is wrong
        ///
        ///Known Input :
        ///         * var path = @"C:\Users\yunus.cogurcu\Desktop\tst\a"
        ///         * Exception error = null
        ///
        ///Expected Output :
        ///         * return should false
        /// </summary>
        [Test(Description = "If CreateDirectory parameter is wrong")]
        public void CreateDirectory_IfParameterIsWrong_ReturnFalse()
        {
            //Arrange
            var path = @"C:\Users\yunus.cogurcu\Desktop\tst\a";
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateDirectory(path, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(false, actual);
        }
        
        /// <summary>
        /// Method Name : CreateDirectory
        ///
        ///Method Description : The method create directory
        ///
        ///Test Scenario : If CreateDirectory parameters are null
        ///
        ///Known Input :
        ///         * string path = null;
        ///         * Exception error;
        ///
        ///Expected Output :
        ///         * return should false
        /// </summary>
        [Test(Description = "If CreateDirectory parameters are null")]
        public void CreateDirectory_IfParameterAreNull_ReturnFalse()
        {
            //Arrange
            string path = null;
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateDirectory(path, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(false, actual);
        }
        
        /// <summary>
        /// Method Name : CreateDirectoryOf
        ///
        ///Method Description : The method create directory from file
        ///
        ///Test Scenario : If CreateDirectoryOf parameters are null
        ///
        ///Known Input :
        ///         * string file = null
        ///         * Exception error = null
        ///
        ///Expected Output :
        ///    Return should false
        /// </summary>
        [Test(Description = "If CreateDirectoryOf parameters are null")]
        public void CreateDirectoryOf_IfParameterAreNull_ReturnFalse()
        {
            //Arrange
            string file = null;
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateDirectoryOf(file, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(false, actual);
        }
        
        /// <summary>
        /// Method Name : CreateDirectoryOf
        ///
        ///Method Description : The method create directory from file
        ///
        ///Test Scenario : If CreateDirectoryOf parameters are not null
        ///
        ///Known Input :
        ///         * var file = @"C:\Users\yunus.cogurcu\Desktop\tst\b";
        ///         * Exception error;
        ///
        ///Expected Output :
        ///         * Return should true
        /// </summary>
        [Test(Description = "If CreateDirectoryOf parameters are not null")]
        public void CreateDirectoryOf_IfParameterNotNull_ReturnTrue()
        {
            //Arrange
            var file = @"C:\Users\yunus.cogurcu\Desktop\tst\b";
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateDirectoryOf(file, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        /// Method Name : CreateFileOf
        ///
        ///Method Description : The method create directory from file
        ///
        ///Test Scenario : If CreateFileOf parameters are null
        ///
        ///Known Input :
        ///            * string file = null;
        ///            * Exception error;
        ///
        ///Expected Output :
        ///            * Return should false
        /// </summary>
        [Test(Description = "If CreateFileOf parameters are null")]
        public void CreateFileOf_IfParameterAreNull_ReturnFalse()
        {
            //Arrange
            string file = null;
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateFileOf(file, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(false, actual);
        }
        
        /// <summary>
        /// Method Name : CreateFileOf
        ///
        ///Method Description : The method create directory from file
        ///
        ///Test Scenario : If CreateFileOf parameters are not null
        ///
        ///Known Input :
        ///         * var file = @"C:\Users\yunus.cogurcu\Desktop\tst\b";
        ///         * Exception error;
        ///
        ///Expected Output :
        ///         * Return should true
        /// </summary>
        [Test(Description = "If CreateFileOf parameters are not null")]
        public void CreateFileOf_IfParameterNotNull_ReturnTrue()
        {
            //Arrange
            var file = @"C:\Users\yunus.cogurcu\Desktop\tst\b";
            Exception error;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.CreateFileOf(file, out error);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(true, actual);
        }
        
        /// <summary>
        /// Method Name : FileNameOf
        ///
        ///Method Description : The method set the filename
        ///
        ///Test Scenario : If FileNameOf parameters are null
        ///
        ///Known Input :
        ///         * string fullName = null;
        ///         * string separator = null;
        ///
        ///Expected Output :
        ///         * Return sholuld null
        /// </summary>
        [Test(Description = "If FileNameOf parameters are null")]
        public void FileNameOf_IfParameterNull_ReturnFalse()
        {
            //Arrange
            string fullName = null;
            string separator = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.FileNameOf(fullName,separator);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        /// Method Name : FileNameOf
        ///
        ///Method Description : The method set the filename
        ///
        ///Test Scenario : If FileNameOf parameters are not null
        ///
        ///Known Input :
        ///         * const string fullName = "lorem ipsum";
        ///         * string separator = null;
        ///
        ///Expected Output :
        ///         * Return should lorem ipsum
        /// </summary>
        [Test(Description = "If FileNameOf parameters are not null")]
        public void FileNameOf_IfParameterNotNull_ReturnFullName()
        {
            //Arrange
            const string fullName = "lorem ipsum";
            string separator = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.FileNameOf(fullName, separator);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("lorem ipsum", actual);
        }

        /// <summary>
        /// Method Name : FileNameOf
        ///
        ///Method Description : The method set the filename
        ///
        ///Test Scenario : If FileNameOf parameters are not null
        ///
        ///Known Input :
        ///             * string fullName = "lorem ipsum.";
        ///             * string separator = ".";
        ///
        ///
        ///Expected Output :
        ///             * Return should lorem ipsum 
        /// </summary>
        [Test(Description = "If FileNameOf parameters are not null")]
        public void FileNameOf_IfParameterNull_ReturnFileName()
        {
            //Arrange
            const string fullName = "lorem ipsum.";
            const string separator = ".";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = FileSystemHelper.FileNameOf(fullName, separator);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("lorem ipsum", actual);
        }

    }
}
