using System.Data.Common;
using MySql.Data.MySqlClient;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Database;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Database.Oracle;
using NUnit.Framework;
using Exception = System.Exception;
using System;

namespace Natek.Recorders.Remote.Test
{
    /// <summary>
    /// Summary description for DbRecorderContextUnitTestFixture
    /// </summary>
    [TestFixture]
    public class DbRecorderContextUnitTestFixture
    {
        private readonly DbRecorderContext _dbcontext = new OracleRecorderContext();


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error is null")]
        public void ReadRecord_IfErrorIsNull_ReturnZero()
        {
            //Arrange
            Exception error = null;

            //Act
            var actual = _dbcontext.ReadRecord(ref error);
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
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error is not null")]
        public void ReadRecord_IfErrorIsNotNull_ReturnZero()
        {
            //Arrange
            Exception error = new ArgumentNullException();

            //Act
            var actual = _dbcontext.ReadRecord(ref error);
            
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
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and offset is null")]
        public void SetOffset_IfParametersAreNull_ReturnTrue()
        {
            //Arrange
            Exception error = null;
            const long offset = 0;

            //Act
            var actual = _dbcontext.SetOffset(offset,ref error);

            //Assert
            Assert.AreEqual(actual, true);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and offset is not null")]
        public void SetOffset_IfParametersAreNotNull_ReturnTrue()
        {
            //Arrange
            var error = new Exception();
            const long offset = 123546521;

            //Act
            var actual = _dbcontext.SetOffset(offset, ref error);

            //Assert
            Assert.AreEqual(actual, true);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and offset is not null")]
        public void PrepareConnection_IfParametersNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;
            DbConnection con = null;

            //Act
// ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareConnection", _dbcontext,new object[] { con, error});
// ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and offset is not null")]
        public void PrepareConnection_IfParametersNotNull_ReturnConnection()
        {
            //Arrange
            var error = new Exception();
            DbConnection con = new MySqlConnection();
            con.BeginTransaction();
            con.Open();
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareConnection", _dbcontext, new object[] { con, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, con);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareQuery_IfParametersAreNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;
            var query = "";
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareQuery", _dbcontext, new object[] { query, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareQuery_IfParametersAreNotNull_ReturnTrue()
        {
            //Arrange
            var error = new Exception();
            const string query = "lorem ipsum";
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareQuery", _dbcontext, new object[] { query, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareQuery_IfParametersAreNotNullExtraContextkeyHasValue_ReturnTrue()
        {
            //Arrange
            _dbcontext.ContextKeys.Add("1", "QUERY_STRING");
            var error = new Exception();
            const string query = "lorem ipsum";
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareQuery", _dbcontext, new object[] { query, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareQuery_IfParametersAreNotNullExtraContextVariableHasValue_ReturnTrue()
        {
            //Arrange
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            _dbcontext.ContextVariables.ContainsKey("QUERY_STRING");
            var error = new Exception();
            const string query = "lorem ipsum";
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareQuery", _dbcontext, new object[] { query, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareCommand_IfParametersAreNull_ReturnFalse()
        {
            //Arrange
            DbConnection con = null;
            string query = null;
            DbCommand cmd = null;
            Exception error = null;


            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareCommand", _dbcontext, new object[] { con, query, cmd, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareCommand_IfParametersAreNull_ReturnTrue()
        {
            //Arrange
            DbConnection con = new MySqlConnection();
            const string query = "lorem ipsum";
            DbCommand cmd = new MySqlCommand();
            Exception error = null;
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            _dbcontext.ContextVariables.ContainsKey("CMD_TIMEOUT");

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareCommand", _dbcontext, new object[] { con, query, cmd, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }


        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareReader_IfParametersAreNull_ReturnFalse()
        {
            //Arrange
            DbConnection con = null;
            DbCommand cmd = null;
            Exception error = null;


            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareReader", _dbcontext, new object[] { con, cmd, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void PrepareReader_IfParametersAreNotNull_ReturnFalse()
        {
            //Arrange
            DbConnection con = new MySqlConnection();
            DbCommand cmd = new MySqlCommand();
            var error = new Exception();


            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("PrepareReader", _dbcontext, new object[] { con, cmd, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        
        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void CreateReader_IfParametersAreNull_ReturnFalse()
        {
            //Arrange
            Exception error = null;
// ReSharper disable once UnusedVariable
            DbConnection con = new MySqlConnection();

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("CreateReader", _dbcontext, new object[] { error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void CreateReader_IfParametersAreNotNull_ReturnFalse()
        {
            //Arrange
            var error = new Exception();
// ReSharper disable once UnusedVariable
            DbConnection con = new MySqlConnection();

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, bool>("CreateReader", _dbcontext, new object[] { error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, false);
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void ReplaceVariables_IfParametersAreNull_ReturnFalse()
        {
            //Arrange
            string query = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, string>("ReplaceVariables", _dbcontext, new object[] { query });
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
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and query is not null")]
        public void ReplaceVariables_IfParametersAreNotNull_ReturnFalse()
        {
            //Arrange
            const string query = "lorem ipsum";

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderContext, string>("ReplaceVariables", _dbcontext, new object[] { query });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "lorem ipsum");
        }

        /// <summary>
        /// Method Name : 
        ///
        ///Method Description :  
        ///
        ///Test Scenario :  
        ///
        ///Known Input :
        ///    
        ///    
        ///
        ///Expected Output :
        ///    
        /// </summary>
        /// 
        [Test(Description = "If Error and offset is null")]
        public void FixOffsets_IfParametersAreNull_ReturnNextInstructionDo()
        {
            //Arrange
            const NextInstruction nextInstruction = NextInstruction.Return;
            const long offset = 0;
            long[] headerOff = null;
            Exception error = null;

            //Act
// ReSharper disable once ExpressionIsAlwaysNull
            var actual = _dbcontext.FixOffsets(nextInstruction, offset, headerOff,ref error);

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

    }
}
