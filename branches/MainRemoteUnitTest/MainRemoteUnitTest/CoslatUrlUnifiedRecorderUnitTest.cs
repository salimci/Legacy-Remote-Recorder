using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using Natek.Recorders.Remote.Unified.CoslatUrlUnifiedRecorder;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class CoslatUrlUnifiedRecorderUnitTest
    {
        private static RecorderBase _coslatUrlUnifiedRecorder;

        /// <summary>
        /// Create a CoslatUrlUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _coslatUrlUnifiedRecorder = new CoslatUrlUnifiedRecorder();
        }

        /// <summary>
        /// Clear CoslatUrlUnifiedRecorder object and set it null for dispose
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _coslatUrlUnifiedRecorder.Clear();
            _coslatUrlUnifiedRecorder = null;
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
            MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder>("Convert2Date", _coslatUrlUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled NullReferanceException
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If fieldvalues is empty
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = String.Empty
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If fieldvalues is empty")]
        public void Convert2Date_IfFieldValuesIsEmpty_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = {String.Empty};
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, object>("Convert2Date", _coslatUrlUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is wrong
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = "11/09/2014 10:32:05"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If date time format is wrong")]
        public void Convert2Date_IfDateTimeFormatIsWrong_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { "11/09/2014 10:32:05" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, object>("Convert2Date", _coslatUrlUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, String.Empty);
        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Return given date time values as converted database format
        ///
        ///Test Scenario : If date time format is true
        ///
        ///Known Input :
        ///   	* rec = null
        ///     * field = null
        ///     * fieldvalues = "Jun/09/2013 13:05:00.555"
        ///     * data = null
        ///
        ///Expected Output :
        ///	    *  String.Empty should return
        /// </summary>
        [Test(Description = "If date time format is true")]
        public void Convert2Date_IfDateTimeFormatIsTrue_Returndd_MM_YYYY()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldvalues = { "Jun/04/2012 00:00:08.0000" };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, object>("Convert2Date", _coslatUrlUnifiedRecorder, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(actual, "2012/06/04 00:00:08");
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
        ///    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNotIncludeSourceHeaderInfo_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            var context = new FileLineRecorderContext(_coslatUrlUnifiedRecorder);
            
            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _coslatUrlUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.NullReferenceException
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
            var context = new FileLineRecorderContext(_coslatUrlUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _coslatUrlUnifiedRecorder, new object[] { context, field, match });
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
        ///    * NullReferenceException should occurence
        /// </summary>
        [Test(Description = "If OnFieldMatch is success but context does not include sourceheaderinfo")]
        public void OnFieldMatch_IfContextIsNull_ReturnNullReferenceException()
        {
            //Arrange
            var match = Regex.Match("Lorem ipsum", ".*");
            string field = null;
            RecorderContext context = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _coslatUrlUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.NullReferenceException
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
            var context = new FileLineRecorderContext(_coslatUrlUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _coslatUrlUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            //Unhandled System.Collections.Generic.KeyNotFoundException
            Assert.AreEqual(actual, NextInstruction.Return);
        }

        /// <summary>
        /// Method Name : OnFieldMatch
        ///
        ///Method Description : Matching with regex at the moment file line
        ///
        ///Test Scenario : If Match is success, for true input
        ///
        ///Known Input :
        ///    * match = at the moment pattern
        ///
        ///Expected Output :
        ///    * NextInstruction.Return should return
        /// </summary>
        [Test(Description = "If OnFieldMatch is success, for true input")]
        public void OnFieldMatch_IfMatchIsSuccessForTrueInput_ReturnNextInstructionReturn()
        {
            //Arrange
            var match = Regex.Match("Jun  4, 2012 00:00:08.000000000 192.168.20.89 00:26:22:44:1A:E3 66.220.151.77 GET 0-131.channel.facebook.com /pull?channel=p_1667955184%26seq=395%26partition=0%26clientid=75125d48%26cb=jmbc%26idle=926", @"(?<DATE>[a-zA-Z]*\s*[0-9]*,\s*[0-9]*\s*[0-9\:\.]*)\s*(?<SRC_IP>[0-9\.]*)\s*(?<SRC_MAC>.[^\s]*)\s*(?<DST_IP>[0-9\.]*)\s*(?<METHOD>.[^\s]*)\s*(?<URL>.[^\s]+)\s+(?<URI>.+)");
            string field = null;
            var context = new FileLineRecorderContext(_coslatUrlUnifiedRecorder)
            {
                SourceHeaderInfo = new Dictionary<string, int>()
            };

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, NextInstruction>("OnFieldMatch", _coslatUrlUnifiedRecorder, new object[] { context, field, match });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
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
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, string>("GetHeaderText", _coslatUrlUnifiedRecorder, new object[] { context });
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
        ///    * context = FileLineRecorderContext(_coslatUrlUnifiedRecorder)
        ///Expected Output :
        ///    * string.Empty should return
        /// </summary>
        [Test(Description = "If context is not null")]
        public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
        {
            //Arrange

            var context = new FileLineRecorderContext(_coslatUrlUnifiedRecorder);

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, string>("GetHeaderText", _coslatUrlUnifiedRecorder, new object[] { context });
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
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _coslatUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"(?<DATE>[a-zA-Z]*\s*[0-9]*,\s*[0-9]*\s*[0-9\:\.]*)\s*(?<SRC_IP>[0-9\.]*)\s*(?<SRC_MAC>.[^\s]*)\s*(?<DST_IP>[0-9\.]*)\s*(?<METHOD>.[^\s]*)\s*(?<URL>.[^\s]+)\s+(?<URI>.+)");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, Regex>("CreateHeaderSeparator", _coslatUrlUnifiedRecorder, new object[] { });

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
            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _coslatUrlUnifiedRecorder, new object[] { });

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

            var expected = new Regex(@"(?<DATE>[a-zA-Z]*\s*[0-9]*,\s*[0-9]*\s*[0-9\:\.]*)\s*(?<SRC_IP>[0-9\.]*)\s*(?<SRC_MAC>.[^\s]*)\s*(?<DST_IP>[0-9\.]*)\s*(?<METHOD>.[^\s]*)\s*(?<URL>.[^\s]+)\s+(?<URI>.+)");
            //Act

            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, Regex>("CreateFieldSeparator", _coslatUrlUnifiedRecorder, new object[] { });

            //Assert
            Assert.AreEqual(actual.ToString(), expected.ToString());
        }

        /// <summary>
        /// Method Name : CreateContextInstance
        ///
        ///Method Description : Create new CoslatUrlUnifiedRecorderContext
        ///
        ///Test Scenario : If context instance is created
        /// 
        ///Known Input : 
        ///    * object[] ctxArgs = {String.Empty};
        ///Expected Output :
        ///    * Return CoslatUrlUnifiedRecorderContext
        /// </summary>
        [Test(Description = "If context instance is created, return CoslatUrlUnifiedRecorderContext")]
        public void CreateContextInstance_InstanceIsCreated_ReturnContext()
        {
            //Arrange
            object[] ctxArgs = {String.Empty};

            //Act

            var actual = MethodTestHelper.RunInstanceMethod<CoslatUrlUnifiedRecorder, RecorderContext>("CreateContextInstance", _coslatUrlUnifiedRecorder, new object[] { ctxArgs });

            //Assert
            Assert.AreNotEqual(actual, null);
        }
    }
}
