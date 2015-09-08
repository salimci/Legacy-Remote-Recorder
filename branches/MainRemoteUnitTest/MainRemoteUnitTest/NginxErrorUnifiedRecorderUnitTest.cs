using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.NginxErrorUnifiedRecorder;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class NginxErrorUnifiedRecorderUnitTest
    {
        private static RecorderBase _nginxErrorUnifiedRecorder;

        /// <summary>
        /// Create a NginxErrorUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _nginxErrorUnifiedRecorder = new NginxErrorUnifiedRecorder();
        }

        /// <summary>
        /// Clear NginxErrorUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _nginxErrorUnifiedRecorder.Clear();
            _nginxErrorUnifiedRecorder = null;
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If fieldvalues is null
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = null
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  NullReferenceException should occure
        /// </summary>
        [Test(Description = "If fieldvalues is null")]
        public void Convert2Date_IfFieldValuesIsNull_ReturnNullReferenceException()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = null;
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder>("Convert2Date", _nginxErrorUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If fieldvalues is not empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = "19.11.2008 08.08.08"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If fieldvalues is not empty")]
        public void Convert2Date_IfFieldValuesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { "19.11.2008 08.08.08" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, object>("Convert2Date", _nginxErrorUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "19.11.2008 08.08.08");
        }


        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is success but context does not include sourceheaderinfo
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * Return NextInstruction.Return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_nginxErrorUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxErrorUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is not success
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Skip should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is not success")]
        public void OnFieldMatch_IfMatchIsNotSuccess_ReturnSkip()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", "\\[+");
            string field = null;
            var context = new FileLineRecorderContext(_nginxErrorUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxErrorUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Skip);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If OnFieldMatch is success but context is null
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNull_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxErrorUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is success, for wrong input
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success, for wrong input")]
        public void OnFieldMatch_IfMatchIsSuccessForWrongInput_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_nginxErrorUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, NextInstruction>("OnFieldMatch", _nginxErrorUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.Collections.Generic.KeyNotFoundException
            Assert.AreEqual(actual, NextInstruction.Return);
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
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, string>("GetHeaderText", _nginxErrorUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_nginxErrorUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_nginxErrorUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, string>("GetHeaderText", _nginxErrorUnifiedRecorder, new object[] { context });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, string.Empty);
        }
        
        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is wrong for header
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * Expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for header")]
        public void CreateHeaderSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, Regex>("CreateHeaderSeparator", _nginxErrorUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateHeaderSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for header
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * return expected regex
        /// </summary>
        [Test(Description = "If regex is true for header")]
        public void CreateHeaderSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("(?<TIME_LOCAL>[0-9\\/\\s:]+)[^\"]+\"(?<ADDRESS>[^\"]+)\"\\s*[^:]+:\\s*(?<ERROR>[^\\)]+)\\)\\s*,\\s*client:\\s*(?<CLIENT>[^,]+)\\s*,\\s*server:\\s*(?<SERVER>[^,]+)\\s*,\\s*request:\\s*\"(?<REQUEST>[^\"]+)\"\\s*,\\s*host:\\s*\"(?<HOST>[^(\\n|\")]+)\"((\\s*,\\s*referrer:\\s*\"(?<REFERER>[^(\\n|\")]+))|())");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, Regex>("CreateHeaderSeparator", _nginxErrorUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create field separator with regex
        ///
        ///Test Scenario : If regex is wrong for field
        /// 
        ///Known Input : 
        ///         regex = "^([^\s]+)\s*$"
        ///Expected Output :
        ///    * Expected and actual values are not equal
        /// </summary>
        [Test(Description = "If regex is wrong for field")]
        public void CreateFieldSeparator_IfRegexIsWrong_Return()
        {
            //Arrange
            var expected = new Regex(@"^([^\s]+)\s*$");

            //Act
            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, Regex>("CreateFieldSeparator", _nginxErrorUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreNotEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateFieldSeparator
        ///
        ///Method Description : Create header separator with regex
        ///
        ///Test Scenario : If regex is true for field
        /// 
        ///Known Input : Expected regex
        ///Expected Output :
        ///    * Return expected regex
        /// </summary>
        [Test(Description = "If regex is true for field")]
        public void CreateFieldSeparator_IfRegexIsTrue_ReturnRegex()
        {
            //Arrange

            var expected = new Regex("(?<TIME_LOCAL>[0-9\\/\\s:]+)[^\"]+\"(?<ADDRESS>[^\"]+)\"\\s*[^:]+:\\s*(?<ERROR>[^\\)]+)\\)\\s*,\\s*client:\\s*(?<CLIENT>[^,]+)\\s*,\\s*server:\\s*(?<SERVER>[^,]+)\\s*,\\s*request:\\s*\"(?<REQUEST>[^\"]+)\"\\s*,\\s*host:\\s*\"(?<HOST>[^(\\n|\")]+)\"((\\s*,\\s*referrer:\\s*\"(?<REFERER>[^(\\n|\")]+))|())");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, Regex>("CreateFieldSeparator", _nginxErrorUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new NginxErrorUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return NginxErrorUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return NginxErrorUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = { String.Empty };

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<NginxErrorUnifiedRecorder, RecorderContext>("CreateContextInstance", _nginxErrorUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
