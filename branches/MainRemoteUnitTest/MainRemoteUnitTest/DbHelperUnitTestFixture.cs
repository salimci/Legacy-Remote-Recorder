using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Transactions;
using Natek.Helpers.Database;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{

    [TestFixture]
    public class DbHelperUnitTestFixture
    {
        TransactionScope _trans;
        DbConnection _con;


        [SetUp]
        public void TestFixtureSetup()
        {
             _trans = new TransactionScope();
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            _trans.Dispose();
        }
        
        /// <summary>
        /// Method Name : BeginTransaction
        ///
        ///Method Description : The method start the database connection
        ///
        ///Test Scenario : If dbconnection and dirty is null
        ///
        ///Known Input :
        ///    * int dirty = null
        ///    * _con = null
        ///
        ///Expected Output :
        ///    * BeginTransaction() should return
        /// </summary>
        /// 
        [Test(Description = "If dbconnection and dirty is null")]
        public void BeginTransaction_IfConnecitonIsNull_ReturnBeginTransaction()
        {
            //Arrange
             int dirty;
             _con = null;
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.BeginTransaction(_con, out dirty);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(_con.BeginTransaction(),actual);
        }
        
        /// <summary>
        /// Method Name : BeginTransaction
        ///
        ///Method Description :  The method start the database connection
        ///
        ///Test Scenario :  If dbconnection and dirty is not null
        ///
        ///Known Input :
        ///    * int dirty = 10
        ///    * _con = new SqlConnection("localhost")
        ///
        ///Expected Output :
        ///    * BeginTransaction() should return
        /// </summary>
        /// 
        [Test(Description = "If connection and dirty is not null")]
        public void BeginTransaction_IfConnecitonIsNotNull_ReturnBeginTransaction()
        {
            //Arrange
            // ReSharper disable once RedundantAssignment
            var dirty= 10;
            _con = new SqlConnection("localhost");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.BeginTransaction(_con, out dirty);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(_con.BeginTransaction(), actual);
        }

        /// <summary>
        /// Method Name : GetField
        ///
        ///Method Description : The method get the field in data reader
        ///
        ///Test Scenario : If all parameters is null
        ///
        ///Known Input :
        ///    *  DbDataReader reader = null;
        ///    *  const int index = 0;
        ///    *  object T = string.Empty;
        ///
        ///Expected Output :
        ///    * Null should return
        /// </summary>
        /// 
        [Test(Description = "If all parameters is null")]
        public void GetField_IfAllParametersNull_ReturnNull()
        {
            //Arrange
            DbDataReader reader = null;
            const int index = 0;
            object T = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.GetField(reader, index, T);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description : The method check the database is nul or not
        ///
        ///Test Scenario : If nullcompare and value are null
        ///
        ///Known Input :
        ///    *  object nullcompare = null;
        ///    * object value = "";
        ///
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        /// 
        [Test(Description = " If nullcompare and value are null")]
        public void IfDBNull_IfDatebaseIsNullFor2Parameters_ReturnNull()
        {
            //Arrange
            object nullcompare = null;
            object value = "";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value, nullcompare);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("", actual);
        }
        
        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario : If nullcompare is null and value has a data
        ///
        ///Known Input :
        ///       * object nullcompare = null;
        ///       * object value = "lorem ipsum";
        ///
        ///Expected Output :
        ///    * lorem ipsum should return
        /// </summary>
        /// 
        [Test(Description = "If nullcompare is null and value has a data")]
        public void IfDBNull_IfDatebaseIsNotNullFor2Parameters_ReturnValue()
        {
            //Arrange
            object nullcompare = null;
            object value = "lorem ipsum";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value, nullcompare);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("lorem ipsum", actual);
        }
        
        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario : If nullcompare is null and value has a integer data
        ///
        ///Known Input :
        ///    * nullcompare = null;
        ///    * value = 10
        ///
        ///Expected Output :
        ///    * 10 should return
        /// </summary>
        /// 
        [Test(Description = "If nullcompare is null and value has a integer data")]
        public void IfDBNull_IfDatebaseIsNotNullForInteger_ReturnValue()
        {
            //Arrange
            object nullcompare = null;
            const int value = 10;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value, nullcompare);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(10, actual);
        }

        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario : If nullcompare is null and value has a long data
        ///
        ///Known Input :
        ///    * nullcompare = null;
        ///    * value = 10000000000000000
        ///
        ///Expected Output :
        ///    * 10000000000000000 should return
        /// </summary>
        /// 
        [Test(Description = "If nullcompare is null and value has a long data")]
        public void IfDBNull_IfDatebaseIsNotNullForLong_ReturnValue()
        {
            //Arrange
            object nullcompare = null;
            const long value = 10000000000000000;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value, nullcompare);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(10000000000000000, actual);
        }

        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario :  If nullcompare is null and value has a data
        ///
        ///Known Input :
        ///     * object nullcompare = null;
        ///     * object value = "lorem ipsum";
        ///
        ///Expected Output :
        ///    * "lorem ipsum" should return
        /// </summary>
        /// 
        [Test(Description = "If nullcompare is null and value has a data")]
        public void IfDBNull_IfDatebaseIsNotNullForString_ReturnValue()
        {
            //Arrange
            object nullcompare = null;
            object value = "lorem ipsum";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value, nullcompare);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("lorem ipsum", actual);

        }
        
        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario : If value is string.Empty
        ///
        ///Known Input :
        ///    * object value = string.Empty;
        ///
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        /// 
        [Test(Description = "If value is string.Empty")]
        public void IfDBNull_IfDatebaseIsNullFor1Parameters_ReturnNull()
        {
            //Arrange
            object value = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        /// Method Name : IfDBNull
        ///
        ///Method Description :  The method check the database is nul or not
        ///
        ///Test Scenario : If value is string data
        ///
        ///Known Input :
        ///    * object value = "lorem ipsum";
        ///
        ///Expected Output :
        ///    * lorem ipsum should return
        /// </summary>
        /// 
        [Test(Description = "If value is string data")]
        public void IfDBNull_IfDatebaseIsNotNullFor1Parameters_ReturnValue()
        {
            //Arrange
            object value = "lorem ipsum";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.IfDBNull(value);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual("lorem ipsum", actual);
        }
        
        /// <summary>
        /// Method Name : AddParameter
        ///
        ///Method Description : The methos set the parameters values
        ///
        ///Test Scenario : If all parameters is null
        ///
        ///Known Input :
        ///     * DbCommand cmd = null;
        ///     * string name = null;
        ///     * const DbType type = DbType.Boolean;
        ///     * const ParameterDirection direction = ParameterDirection.Input;
        ///
        ///Expected Output :
        ///    * null should return
        /// </summary>
        /// 
        [Test(Description = "If all parameters is null")]
        public void AddParameter_IfParametersAreNull_ReturnParam()
        {
            //Arrange
            DbCommand cmd = null;
            string name = null;
            const DbType type = DbType.Boolean;
            const ParameterDirection direction = ParameterDirection.Input;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.AddParameter(cmd, name, type, direction);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Method Name : AddParameter
        ///
        ///Method Description : The methos set the parameters values
        ///
        ///Test Scenario : If command has data and other parameters are null
        ///
        ///Known Input :
        ///     * DbCommand cmd = new OdbcCommand("lorem");
        ///     * string name = null;
        ///     * const DbType type = DbType.Boolean;
        ///     * const ParameterDirection direction = ParameterDirection.Input; 
        ///
        ///Expected Output :
        ///    * Not null should return
        /// </summary>
        /// 
        [Test(Description = " If command has data and other parameters are null")]
        public void AddParameter_IfDbCommandIsNotNull_ReturnParam()
        {
            //Arrange
            DbCommand cmd = new OdbcCommand("lorem");
            string name = null;
            const DbType type = DbType.Boolean;
            const ParameterDirection direction = ParameterDirection.Input;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = DbHelper.AddParameter(cmd, name, type, direction);
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreNotEqual(null,actual);
        }
    
    }
}
