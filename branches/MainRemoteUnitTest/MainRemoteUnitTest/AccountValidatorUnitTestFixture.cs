using System;
using Natek.Helpers.Security.Logon;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class AccountValidatorUnitTestFixture
    {
        /// <summary>
        /// Method Name : ValidateAccount
        ///
        ///Method Description : The method validate the account according to the username and password
        ///
        ///Test Scenario : Without domain incorrectect userName and Password
        ///
        ///Known Input :
        ///      *  username = "test"
        ///      *  password = "test"
        ///      *  error = null
        /// 
        ///Expected Output :
        ///	    * Return should null
        /// 
        /// </summary>
        [Test]
        public void ValidateAccount_WithoutDomainWithoutCorrectectUserName_ReturnNull()
        {
            //Arrange
            const string username = "test";
            const string password = "test";
            Exception error = null;

            //Act
            var arrange = AccountValidator.ValidateAccount(username, password, ref error);

            //Assert
            Assert.IsNull(arrange);
        }

        /// <summary>
        /// Method Name : ValidateAccount
        ///
        ///Method Description : The method validate the account according to the username and password
        ///
        ///Test Scenario : Method parameter username and password null or empty 
        ///
        ///Known Input :
        ///     * error = null
        ///     * username = "" or username = null
        ///     * password = "" or password = null
        /// 
        ///Expected Output :
        ///	    * Return should null
        /// </summary>
        [TestCase(null,"", Result = null, TestName = "WithUsernameIsNullReturnNull")]
        [TestCase("", "", Result = null, TestName = "WithUsernameIsEmptyReturnNull")]
        [TestCase("test", null, Result = null, TestName = "WithPasswordIsNullReturnNull")]
        public ImpersonationContext ValidateAccount_TestForNullAndEmptyParameters(string username, string password)
        {
            Exception error = null;
            return AccountValidator.ValidateAccount(username, password, ref error);
        }

        /// <summary>
        /// Method Name : SplitUserDomain
        ///
        ///Method Description :The method split the user domain for "//" 
        ///
        ///Test Scenario : User name with empty domain
        ///
        ///Known Input :
        ///     *  username = "TESTDOMAIN\\testUser"
        ///     *  domain = string.Empty
        ///
        ///Expected Output :
        ///	    * Return domain name
        /// 
        /// </summary>
        [Test]
        public void SplitUserDomain_UserNameWithEmptyDomain_RefDomianName()
        {
            //Arrange
            var username = "TESTDOMAIN\\testUser";
            var domain = string.Empty;

            //Act
            AccountValidator.SplitUserDomain(ref username, ref domain);

            //Assert
            Assert.AreEqual("TESTDOMAIN",domain);
        }

        /// <summary>
        /// Method Name : SplitUserDomain
        ///
        ///Method Description : The method split the user domain for "//" 
        ///
        ///Test Scenario : User name with empty domain
        ///
        ///Known Input :
        ///     *   username = "TESTDOMAIN\\testUser"
        ///     *   domain = string.Empty
        /// 
        ///Expected Output :
        ///	    * Return username
        /// 
        /// </summary>
        [Test]
        public void SplitUserDomain_UserNameWithEmptyDomain_RefUserName()
        {
            //Arrange
            var username = "TESTDOMAIN\\testUser";
            var domain = string.Empty;

            //Act
            AccountValidator.SplitUserDomain(ref username, ref domain);

            //Assert
            Assert.AreEqual("testUser", username);
        }


        /// <summary>
        /// Method Name : SplitUserDomain
        ///
        ///Method Description : The method split the user domain for "//" 
        ///
        ///Test Scenario : Domain name exist but no user name
        ///
        ///Known Input :
        ///     * username = "TESTDOMAIN\\" 
        ///     * domain = string.Empty
        /// 
        ///Expected Output :
        ///	    * Return exception with message "Domain found but no user:[TESTDOMAIN\\"
        /// 
        /// </summary>
        [Test]
        [ExpectedException(ExpectedException = typeof(Exception), ExpectedMessage = "Domain found but no user:[TESTDOMAIN\\")]
        public void SplitUserDomain_DomainNameExistButNoUserName_ExpectedException()
        {
            //Arrange
            var username = "TESTDOMAIN\\"; 
            var domain = string.Empty;

            //Act
            AccountValidator.SplitUserDomain(ref username, ref domain);
        }


        /// <summary>
        /// Method Name : Decrypt Password
        ///
        ///Method Description : The method decrypt the password
        ///
        ///Test Scenario : Password and key are empty
        ///
        ///Known Input :
        ///     * password = string.Empty
        ///     * key = string.Empty
        /// 
        ///Expected Output :
        ///	    * Return empty password
        /// 
        /// </summary>
        [Test]
        public void DecryptPassword_PasswordIsEmptyWithEmptyKey_RefPasswordEmpty()
        {
            //Arrange
            var password = string.Empty;
            var key = string.Empty;

            //Act
            AccountValidator.DecryptPassword(key,ref password);

            //Assert
            Assert.IsEmpty(password);
        }

        /// <summary>
        /// Method Name : Decrypt Password
        ///
        ///Method Description : The method decrypt the password
        ///
        ///Test Scenario : Password is not nul but key is empty
        ///
        ///Known Input :
        ///     * password = "lorem";
        ///     * key = string.Empty
        /// 
        ///Expected Output :
        ///	    * Return empty password
        /// 
        /// </summary>
        [Test]
        public void DecryptPassword_PasswordIsNotEmptyWithEmptyKey_RefPasswordEmpty()
        {
            //Arrange
            var password = "lorem";
            var key = string.Empty;

            //Act
            AccountValidator.DecryptPassword(key, ref password);

            //Assert
            Assert.IsEmpty(password);
        }
    }
}
