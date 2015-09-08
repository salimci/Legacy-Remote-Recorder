using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Database;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.Database.Oracle;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class DbRecorderBaseUnitTest
    {
        private DbRecorderBase _dbRecorderBase;

        /// <summary>
        /// Create a MsSqlUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _dbRecorderBase = new OracleUnifiedRecorder();
        }

        /// <summary>
        /// Clear DbRecorderBase object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _dbRecorderBase.Clear();
            _dbRecorderBase = null;
        }

        /// <summary>
        /// Method Name : Value2External
        ///
        ///Method Description : Convert value to external variables
        ///
        ///Test Scenario : If data is null
        /// 
        ///Known Input :
        ///     * rec = null
        ///     * field = null
        ///     * fieldValues = null
        ///     * data = null
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data is null")]
        public void Value2External_IfDataIsNull_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Value2External", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
         }

        /// <summary>
        /// Method Name : Value2External
        ///
        ///Method Description : Convert value to external variables
        ///
        ///Test Scenario : If data's length is smaller than third
        /// 
        ///Known Input :
        ///     * rec = null
        ///     * field = null
        ///     * fieldValues = null
        ///     * data = "lo"
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data's length is smaller than third")]
        public void Value2External_IfDatasLengthIsSmallerThanThird_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            object data = "lo";

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Value2External", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Value2External
        ///
        ///Method Description : Convert value to external variables
        ///
        ///Test Scenario : If data is not dbRecorderContext
        /// 
        ///Known Input :
        ///     * rec = null
        ///     * field = null
        ///     * fieldValues = null
        ///     * data = not null
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data is not dbRecorderContext")]
        public void Value2External_IfDataIsNotDbRecorderContext_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;

            var array1 = new object[5];
            array1[0] = new StringBuilder();
            array1[1] = 99;
            array1[2] = "String literal";
            array1[3] = null;

            object data = array1;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Value2External", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Value2External
        ///
        ///Method Description : Convert value to external variables
        ///
        ///Test Scenario : If data is dbRecorderContext
        /// 
        ///Known Input :
        ///     * rec = null
        ///     * field = null
        ///     * fieldValues = null
        ///     * data = not null
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data is dbRecorderContext")]
        public void Value2External_IfDataIsDbRecorderContext_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            var context = new OracleRecorderContext {ExternalVariables = new Dictionary<string, string>()};
            var array1 = new object[5];
            array1[0] = context;
            array1[1] = 99;
            array1[2] = "String literal";
            array1[3] = null;
           
            object data = array1;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Value2External", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Value2External
        ///
        ///Method Description : Convert value to external variables
        ///
        ///Test Scenario : If all inputs are true
        /// 
        ///Known Input :
        ///     * rec = null
        ///     * field = null
        ///     * fieldValues = null
        ///     * data = not null
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If all inputs are true")]
        public void Value2External_IfTrueInput_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            var context = new OracleRecorderContext { ExternalVariables = new Dictionary<string, string>() };
            context.ExternalVariables.Add("@99",null);
            var array1 = new object[5];
            array1[0] = context;
            array1[1] = 99;
            array1[2] = "String literal";
            array1[3] = null;

            object data = array1;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Value2External", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : OnKeywordParsed
        ///
        ///Method Description : If keyword parsed, return true and increase touchCount
        ///
        ///Test Scenario : If keyword is null
        /// 
        ///Known Input :
        ///     * keyword = null
        ///     * quotedKeyword = false
        ///     * value = null
        ///     * quotedValue = false
        ///     * touchCount = 0
        ///     * error = null
        ///Expected Output :
        ///	    * System.ArgumentNullException should occurence
        /// </summary>
        [Test(Description = "If keyword is null")]
        public void OnKeywordParsed_IfKeywordIsNull_SystemArgumentNullException()
        {
            //Arrange
            string keyword = null;
            const bool quotedKeyword = false;
            string value = null;
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("OnKeywordParsed", _dbRecorderBase, new object[] { keyword, quotedKeyword, value, quotedValue,touchCount,error });
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.ArgumentNullException
        }

        /// <summary>
        /// Method Name : OnKeywordParsed
        ///
        ///Method Description : If keyword parsed, return true and increase touchCount
        ///
        ///Test Scenario : If keyword is not null but  value null
        /// 
        ///Known Input :
        ///     * keyword = "HOff";
        ///     * quotedKeyword = false
        ///     * value = null
        ///     * quotedValue = false
        ///     * touchCount = 2147483647
        ///     * error = null
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If keyword is not null but  value null")]
        public void OnKeywordParsed_IfValueIsNull_NullReferenceException()
        {
            //Arrange
            const string keyword = "HOff";
            const bool quotedKeyword = false;
            string value = null;
            const bool quotedValue = false;
            const int touchCount = 2147483647;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("OnKeywordParsed", _dbRecorderBase, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, error });
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //NullReferenceException
        }

        /// <summary>
        /// Method Name : OnKeywordParsed
        ///
        ///Method Description : If keyword parsed, return true and increase touchCount
        ///
        ///Test Scenario : If keyword is HOff
        /// 
        ///Known Input :
        ///     * keyword = "HOff";
        ///     * quotedKeyword = false
        ///     * value = "a|b|c"
        ///     * quotedValue = false
        ///     * touchCount = 0
        ///     * error = null
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = "If keyword is HOff")]
        public void OnKeywordParsed_IfKeywordIsHOff_ReturnTrue()
        {
            //Arrange
            const string keyword = "HOff";
            const bool quotedKeyword = false;
            const string value = "a|b|c";
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("OnKeywordParsed", _dbRecorderBase, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, error });
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : OnKeywordParsed
        ///
        ///Method Description : If keyword parsed, return true and increase touchCount
        ///
        ///Test Scenario : If keyword is FMdf
        /// 
        ///Known Input :
        ///     * keyword = "FMdf";
        ///     * quotedKeyword = false
        ///     * value = "a|b|c"
        ///     * quotedValue = false
        ///     * touchCount = 0
        ///     * error = null
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = "If keyword is FMdf")]
        public void OnKeywordParsed_IfKeywordIsFMdf_ReturnTrue()
        {
            //Arrange
            const string keyword = "FMdf";
            const bool quotedKeyword = false;
            const string value = "a|b|c";
            const bool quotedValue = false;
            const int touchCount = 0;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("OnKeywordParsed", _dbRecorderBase, new object[] { keyword, quotedKeyword, value, quotedValue, touchCount, error });
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, true);
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create regex for seperating processing to header
        ///
        ///Test Scenario : true regex
        /// 
        ///Known Input :
        ///    
        ///Expected Output :
        ///	    * ^$
        /// </summary>
        [Test(Description = "true regex")]
        public void CreateHeaderSeparator_TrueRegex_ReturnTrueRegex()
        {
            //Arrange
            var expected = new Regex("^$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, Regex>("CreateHeaderSeparator", _dbRecorderBase, new object[] { });
            
            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create regex for seperating processing to field
        ///
        ///Test Scenario : true regex
        /// 
        ///Known Input :
        ///    
        ///Expected Output :
        ///	    * ^$
        /// </summary>
        [Test(Description = "true regex")]
        public void CreateFieldSeparator_TrueRegex_ReturnTrueRegex()
        {
            //Arrange
            var expected = new Regex("^$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, Regex>("CreateFieldSeparator", _dbRecorderBase, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : InitContextGlobals
        ///
        ///Method Description : Initialize context's globals variables
        ///
        ///Test Scenario : If dbContext is null
        /// 
        ///Known Input :
        ///     * dbContext = null
        ///     * host = null
        ///     * port = 0
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If dbContext is null")]
        public void InitContextGlobals_IfdbContextIsNull_NullReferenceException()
        {
            //Arrange
            DbRecorderContext dbContext = null;
            string host = null;
            const int port = 0;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("InitContextGlobals", _dbRecorderBase, new object[] { dbContext, host, port });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : InitContextGlobals
        ///
        ///Method Description : Initialize context's globals variables
        ///
        ///Test Scenario : If dbContext is not null but ContextKeys has no elements
        /// 
        ///Known Input :
        ///     * dbContext = OracleRecorderContext
        ///     * host = null
        ///     * port = 0
        /// 
        ///Expected Output :
        ///	    * KeyNotFoundException  should occurence
        /// </summary>
        [Test(Description = "If dbContext is not null but ContextKeys has no elements")]
        public void InitContextGlobals_IfContextKeysHasNoElements_KeyNotFoundException()
        {
            //Arrange
            DbRecorderContext dbContext = new OracleRecorderContext();
            string host = null;
            const int port = 0;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("InitContextGlobals", _dbRecorderBase, new object[] { dbContext, host, port });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled KeyNotFoundException 
        }

        /// <summary>
        /// Method Name : PrepareKeywords
        ///
        ///Method Description : Prepare keywords with some special characters
        ///
        ///Test Scenario : If keywordBuffer is null
        /// 
        ///Known Input :
        ///     * keywordBuffer = null
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If keywordBuffer is null")]
        public void PrepareKeywords_IfkeywordBufferIsNull_NullReferenceException()
        {
           //Arrange
            StringBuilder keywordBuffer = null;
            RecorderContext context = null;
            var externalVariables = new Dictionary<string, string> {{"key1", "val1"}, {"key2", "val2"}};
            
            //Act
            FieldTestHelper.SetInstanceFieldValue("externalVariables", _dbRecorderBase, externalVariables);
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("PrepareKeywords", _dbRecorderBase, new object[] { context, keywordBuffer });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : InputText2RecordField
        ///
        ///Method Description : Return NextInstruction.Do
        ///
        ///Test Scenario : Return NextInstruction.Do
        /// 
        ///Known Input :
        ///     * keywordBuffer = null
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * Return NextInstruction.Do
        /// </summary>
        [Test(Description = "Return NextInstruction.Do")]
        public void InputText2RecordField_Return_NextInstructionDo_Return_NextInstructionDo()
        {
            //Arrange
            string[] fields = null;
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("InputText2RecordField", _dbRecorderBase, new object[] { context, fields });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : InputTextType
        /// 
        ///Method Description : Return RecordInputType.Record
        ///
        ///Test Scenario : Return RecordInputType.Record
        /// 
        ///Known Input :
        ///     * error = null
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * Return RecordInputType.Record
        /// </summary>
        [Test(Description = "Return RecordInputType.Record")]
        public void InputTextType_Return_RecordInputTypeRecord_Return_RecordInputTypeRecord()
        {
            //Arrange
            Exception error = null;
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, RecordInputType>("InputTextType", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, RecordInputType.Record);
        }

        /// <summary>
        /// Method Name : GetHeaderText
        ///
        ///Method Description : Return location
        ///
        ///Test Scenario : Return location
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * Return RecordInputType.Record
        /// </summary>
        [Test(Description = "Return location")]
        public void GetHeaderText_ReturnLocation_ReturnLocation()
        {
            //Arrange
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, RecordInputType>("GetHeaderText", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual,"");
        }
        
        /// <summary>
        /// Method Name : GetExternal
        ///
        ///Method Description : Get external variables from externalVariables with parameters
        ///
        ///Test Scenario : If dbContext is null
        /// 
        ///Known Input :
        ///     * dbContext = null
        ///     * varName = string.Empty
        ///     * extension = string.Empty
        ///     * varExtension = string.Empty
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If dbContext is null")]
        public void GetExternal_IfdbContextIsNull_NullReferenceException()
        {
            //Arrange
            DbRecorderContext dbContext = null;
            var varName = string.Empty;
            var  extension = string.Empty;
            var varExtension = string.Empty;
            var externalVariables = new Dictionary<string, string> { { "@_", "val1" }, { "@_1", "val2" } };

            //Act
            FieldTestHelper.SetInstanceFieldValue("externalVariables", _dbRecorderBase, externalVariables);
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("GetExternal", _dbRecorderBase, new object[] { dbContext, varName, extension, varExtension });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : InitActiveParameters
        ///
        ///Method Description : Initialize active parameters
        ///
        ///Test Scenario : If dbContext is null
        /// 
        ///Known Input :
        ///     * dbContext = null
        ///     * query = string.Empty
        ///     * queryExtension = string.Empty
        ///     * queryString = string.Empty
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If dbContext is null")]
        public void InitActiveParameters_IfdbContextIsNull_NullReferenceException()
        {
            //Arrange
            DbRecorderContext dbContext = null;
            var query = string.Empty;
            var queryExtension = string.Empty;
            var queryString = string.Empty;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("InitActiveParameters", _dbRecorderBase, new object[] { dbContext, query, queryExtension, queryString });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : InitActiveParameters
        ///
        ///Method Description : Initialize active parameters
        ///
        ///Test Scenario : If dbContext is not null but ContextKeys has no elements
        /// 
        ///Known Input :
        ///     * dbContext = OracleRecorderContext
        ///     * query = string.Empty
        ///     * queryExtension = string.Empty
        ///     * queryString = string.Empty
        /// 
        ///Expected Output :
        ///	    * KeyNotFoundException  should occurence
        /// </summary>
        [Test(Description = "If dbContext is not null but ContextKeys has no elements")]
        public void InitActiveParameters_IfContextKeysHasNoElements_KeyNotFoundException()
        {
            //Arrange
            DbRecorderContext dbContext = new OracleRecorderContext();
            var query = string.Empty;
            var queryExtension = string.Empty;
            var queryString = string.Empty;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("InitActiveParameters", _dbRecorderBase, new object[] { dbContext, query, queryExtension, queryString });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled KeyNotFoundException 
        }

        /// <summary>
        /// Method Name : InitActiveParameters
        ///
        ///Method Description : Initialize active parameters
        ///
        ///Test Scenario : If dbContext is not null but ContextVariables has no elements
        /// 
        ///Known Input :
        ///     * dbContext = OracleRecorderContext
        ///     * query = string.Empty
        ///     * queryExtension = string.Empty
        ///     * queryString = string.Empty
        /// 
        ///Expected Output :
        ///	    * KeyNotFoundException  should occurence
        /// </summary>
        [Test(Description = "If dbContext is not null but ContextVariables has no elements")]
        public void InitActiveParameters_IfContextVariablesHasNoElements_KeyNotFoundException()
        {
            //Arrange
            DbRecorderContext dbContext = new OracleRecorderContext();
            dbContext.ContextKeys.Add("QUERY", "QUERY");
            var query = string.Empty;
            var queryExtension = string.Empty;
            var queryString = string.Empty;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("InitActiveParameters", _dbRecorderBase, new object[] { dbContext, query, queryExtension, queryString });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled KeyNotFoundException 
        }

        /// <summary>
        /// Method Name : OnBeforeSetReg
        ///
        ///Method Description : OnBeforeSetReg
        ///
        ///Test Scenario : If context is null
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If context is null")]
        public void OnBeforeSetReg_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("OnBeforeSetReg", _dbRecorderBase, new object[] { context});
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : OnBeforeSetReg
        ///
        ///Method Description : OnBeforeSetReg
        ///
        ///Test Scenario : If context is not null
        /// 
        ///Known Input :
        ///     * context = FileLineRecorderContext
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void OnBeforeSetReg_IfContextIsNull_ReturnDo()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext();

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("OnBeforeSetReg", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : OnAfterSetData
        ///
        ///Method Description : OnAfterSetData
        ///
        ///Test Scenario : If context is null
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If context is null")]
        public void OnAfterSetData_IfContextIsNull_NullReferenceException()
        {
            //Arrange
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("OnAfterSetData", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : OnAfterSetData
        ///
        ///Method Description : OnAfterSetData
        ///
        ///Test Scenario : If context is not DbRecorderContext
        /// 
        ///Known Input :
        ///     * context = FileLineRecorderContext
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If context is not DbRecorderContext")]
        public void OnAfterSetData_IfContextFileLineRecorderContext_ReturnAbort()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext();

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("OnAfterSetData", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : OnAfterSetData
        ///
        ///Method Description : OnAfterSetData
        ///
        ///Test Scenario : If context is not DbRecorderContext
        /// 
        ///Known Input :
        ///     * context = OracleRecorderContext
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is not DbRecorderContext")]
        public void OnAfterSetData_IfContextDbRecorderContext_ReturnDo()
        {
            //Arrange
            DbRecorderContext context = new OracleRecorderContext();
            context.Record = new RecWrapper();
            context.ContextKeys.Add("QUERY_EXT","lorem");
            context.ContextVariables.Add("lorem","ipsum");
            var externalVariables = new Dictionary<string, string> { { "@RECORDNUM_lorem", "val1" }, { "@RECORDDATE_lorem", "val2" } };

            //Act
            FieldTestHelper.SetInstanceFieldValue("externalVariables", _dbRecorderBase, externalVariables);
            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("OnAfterSetData", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : DoLogic
        ///
        ///Method Description : DoLogic
        ///
        ///Test Scenario : If context is null
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void DoLogic_IfContextIsNull_ReturnAbort()
        {
            //Arrange
            RecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("DoLogic", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : DoLogic
        ///
        ///Method Description : DoLogic
        ///
        ///Test Scenario : If DbRecorderContext is null
        /// 
        ///Known Input :
        ///     * context = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void DoLogic_IfDbRecorderContextIsNull_ReturnAbort()
        {
            //Arrange
            DbRecorderContext context = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("DoLogic", _dbRecorderBase, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : GetRecordPropertyDictionary
        ///
        ///Method Description : Get record property dictionary
        ///
        ///Test Scenario : If type is null
        /// 
        ///Known Input :
        ///     * type = null
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If type is null")]
        public void GetRecordPropertyDictionary_IfTypeIsNull_NullReferenceException()
        {
            //Arrange
            Type type = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<DbRecorderBase>("GetRecordPropertyDictionary", _dbRecorderBase, new object[] { type });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferenceException
        }

        /// <summary>
        /// Method Name : GetRecordPropertyDictionary
        ///
        ///Method Description : Get record property dictionary
        ///
        ///Test Scenario : If type is not null
        /// 
        ///Known Input :
        ///     * type = TypeDelegator(typeof(RecWrapper))
        /// 
        ///Expected Output :
        ///	    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If type is not null")]
        public void GetRecordPropertyDictionary_IfTypeIsNotNull_ReturnDictionary()
        {
            //Arrange
            Type type = new TypeDelegator(typeof(RecWrapper));

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, Dictionary<string, PropertyInfo>>("GetRecordPropertyDictionary", _dbRecorderBase, new object[] { type });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, GetRecordPropertyDictionary(type));
        }

        private static Dictionary<string, PropertyInfo> GetRecordPropertyDictionary(Type type)
        {
            var propDict = new Dictionary<string, PropertyInfo>();
            foreach (var prop in type.GetProperties())
                propDict[prop.Name.ToLowerInvariant()] = prop;
            return propDict;
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : If recorder context is null
        /// 
        ///Known Input :
        ///     * context = null
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If recorder context is null")]
        public void GetHeaderInfo_IfRecorderContextIsNull_ReturnAbort()
        {
            //Arrange
            RecorderContext context = null;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : If headerInfo is null
        /// 
        ///Known Input :
        ///     * context = FileLineRecorderContext
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetHeaderInfo_IfHeaderInfoIsNull_ReturnAbort()
        {
            //Arrange
            RecorderContext context = new FileLineRecorderContext();
            context.HeaderInfo = null;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : If headerInfo is not null
        /// 
        ///Known Input :
        ///     * context = OracleRecorderContext
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If context is null")]
        public void GetHeaderInfo_IfHeaderInfoIsNotNull_ReturnDo()
        {
            //Arrange
            RecorderContext context = new OracleRecorderContext();
            context.HeaderInfo = new DataMappingInfo();
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : If DbRecorderContext  is null
        /// 
        ///Known Input :
        ///     * context = DbRecorderContext = null
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Abort should return
        /// </summary>
        [Test(Description = "If DbRecorderContext  is null")]
        public void GetHeaderInfo_IfDbRecorderContextIsNull_ReturnAbort()
        {
            //Arrange
            DbRecorderContext context = null;
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Abort);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : If dbContext.Readers  is null
        /// 
        ///Known Input :
        ///     * context = OracleRecorderContext
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "If dbContext.Readers is null")]
        public void GetHeaderInfo_IfReadersIsNull_ReturnDo()
        {
            //Arrange
            DbRecorderContext context = new OracleRecorderContext();
            context.Readers.Add("DATA_READER", null);
            context.ContextKeys.Add("DATA_READER", "DATA_READER");
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : GetHeaderInfo
        ///
        ///Method Description : Get header info
        ///
        ///Test Scenario : Do header process correctly
        /// 
        ///Known Input :
        ///     * context = OracleRecorderContext
        ///     * error = null
        /// 
        ///Expected Output :
        ///	    * NextInstruction.Do should return
        /// </summary>
        [Test(Description = "Do header process correctly")]
        public void GetHeaderInfo_IfAllParametersTrue_ReturnDo()
        {
            //Arrange
            DbRecorderContext context = new OracleRecorderContext();
            var table = new DataTable();
            table.Columns.Add("Patient", typeof(string));
            DbDataReader reader = new DataTableReader(table);
            context.Readers.Add("DATA_READER", reader);
            context.ContextKeys.Add("DATA_READER", "DATA_READER");
            Exception error = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, NextInstruction>("GetHeaderInfo", _dbRecorderBase, new object[] { context, error });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Do);
        }

        /// <summary>
        /// Method Name : DisposeActiveData
        ///
        ///Method Description : Dispose active data
        ///
        ///Test Scenario : If dbContext is null
        /// 
        ///Known Input :
        ///     * dbContext = null
        /// 
        ///Expected Output :
        ///	    * False should return
        /// </summary>
        [Test(Description = "If dbContext is null")]
        public void DisposeActiveData_IfdbContextIsNull_ReturnFalse()
        {
            //Arrange
            DbRecorderContext dbContext = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("DisposeActiveData", _dbRecorderBase, new object[] { dbContext });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : DisposeActiveData
        ///
        ///Method Description : Dispose active data
        ///
        ///Test Scenario : If dbContext is OracleRecorderContext but dbContext.ContextKeys has no element
        /// 
        ///Known Input :
        ///     * dbContext = OracleRecorderContext
        /// 
        ///Expected Output :
        ///	    * False should return
        /// </summary>
        [Test(Description = "If dbContext is OracleRecorderContext but dbContext.ContextKeys has no element")]
        public void DisposeActiveData_IfdbContextKeysHasNoElements_ReturnFalse()
        {
            //Arrange
            DbRecorderContext dbContext = new OracleRecorderContext();

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("DisposeActiveData", _dbRecorderBase, new object[] { dbContext });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Method Name : DisposeActiveData
        ///
        ///Method Description : Dispose active data
        ///
        ///Test Scenario : If dbContext is OracleRecorderContext and dbContext.ContextKeys has element
        /// 
        ///Known Input :
        ///     * dbContext = OracleRecorderContext
        /// 
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = "If dbContext is OracleRecorderContext and dbContext.ContextKeys has element")]
        public void DisposeActiveData_IfdbContextKeysHasElements_ReturnTrue()
        {
            //Arrange
            DbRecorderContext dbContext = new OracleRecorderContext();
            dbContext.ContextKeys.Add("DATA_READER", "DATA_READER");
            dbContext.ContextKeys.Add("COMMAND", "COMMAND");
            dbContext.ContextKeys.Add("CONNECTION", "CONNECTION");

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("DisposeActiveData", _dbRecorderBase, new object[] { dbContext });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : DisposeActiveData
        ///
        ///Method Description : Dispose active data
        ///
        ///Test Scenario : Return true
        /// 
        ///Known Input :
        ///     * dictionary = null
        ///     * key = string.Empty
        /// 
        ///Expected Output :
        ///	    * True should return
        /// </summary>
        [Test(Description = "Return true")]
        public void DisposeActiveData_AlwaysTrue_ReturnTrue()
        {
            //Arrange
            Dictionary<string, IDisposable> dictionary = null;
            var key = string.Empty;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, bool>("DisposeActiveData", _dbRecorderBase, new object[] { dictionary, key });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Method Name : Object2Property
        ///
        ///Method Description : Convert type of object to property
        ///
        ///Test Scenario : If data is null
        /// 
        ///Known Input :
        ///     * rec =null
        ///     * field = string.Empty
        ///     * fieldValues =null
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data is null")]
        public void Object2Property_IfDataIsNull_ReturnNull()
        {
            //Arrange
            RecWrapper rec =null;
            var field = string.Empty;
            string[] fieldValues =null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Object2Property", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Object2Property
        ///
        ///Method Description : Convert type of object to property
        ///
        ///Test Scenario : If data is not null and lenth is smaller than third
        /// 
        ///Known Input :
        ///     * rec =null
        ///     * field = string.Empty
        ///     * fieldValues =null
        ///     * data = null
        /// 
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data is not null and lenth is smaller than third")]
        public void Object2Property_IfDataIsNotNullAndSmallerThanThird_ReturnNull()
        {
            //Arrange
            RecWrapper rec = null;
            var field = string.Empty;
            string[] fieldValues = null;
            object data = "lo";

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Object2Property", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Object2Property
        ///
        ///Method Description : Convert type of object to property
        ///
        ///Test Scenario : If data 1 is not propertyInfo, return null
        /// 
        ///Known Input :
        ///     * rec =null
        ///     * field = string.Empty
        ///     * fieldValues =null
        ///     * data = not null
        /// 
        ///Expected Output :
        ///	    * Null should return
        /// </summary>
        [Test(Description = "If data 1 is not propertyInfo, return null")]
        public void Object2Property_Ifdata1IsNotProperyInfo_ReturnNull()
        {
            //Arrange
            var textProperty = new TextProperty();

            RecWrapper rec = null;
            var field = string.Empty;
            string[] fieldValues = null;

            var array1 = new object[5];
            array1[0] = new object();
            array1[1] = FieldTestHelper.GetInstanceFieldValue("PropertyInfo", textProperty);
            array1[2] = "String literal";
            array1[3] = 3;
            array1[4] = null;

            object data = array1;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Object2Property", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, null);
        }

        /// <summary>
        /// Method Name : Object2Property
        ///
        ///Method Description : Convert type of object to property
        ///
        ///Test Scenario : If data 0's type is DbRecorderContext and value is null return data2
        ///Known Input :
        ///     * rec =null
        ///     * field = string.Empty
        ///     * fieldValues =null
        ///     * data = not null
        /// 
        ///Expected Output :
        ///	    * data2 should return
        /// </summary>
        [Test(Description = "If data 0's type is DbRecorderContext and value is null return data2")]
        public void Object2Property_Ifdata0IsDbRecorderContextIsNull_ReturnData2()
        {
            //Arrange
            var textProperty = new TextProperty();

            RecWrapper rec = null;
            var field = string.Empty;
            string[] fieldValues = null;

            var array1 = new object[5];
            array1[0] = new OracleRecorderContext();
            array1[1] = FieldTestHelper.GetInstanceFieldValue("PropertyInfo", textProperty);
            array1[2] = "String literal";
            array1[3] = 3;
            array1[4] = null;

            object data = array1;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<DbRecorderBase, object>("Object2Property", _dbRecorderBase, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "String literal");
        }
    }
}
