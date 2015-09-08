using System;
using System.Net.Sockets;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;
using Natek.Recorders.Remote.StreamBased.Terminal.Ssh.Apache;
using Natek.Recorders.Remote.Test.UnitTestHelper;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
 [TestFixture]
    public class ApacheAccessUnifiedRecorderUnitTestFixture
    {
        private static RecorderBase _apacheaccess;

        /// <summary>
        /// Create a ApacheAccessUnifiedRecorder object for testing.
        /// </summary>
        [SetUp]
        public void TestFixtureSetup()
        {
            _apacheaccess = new ApacheAccessUnifiedRecorder();
        }

        /// <summary>
        /// Clear ApacheAccessUnifiedRecorder object and set it null for dispose.
        /// </summary>
        [TearDown]
        public void TestFixtureTearDown()
        {
            _apacheaccess.Clear();
            _apacheaccess = null;
        }


        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = null 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return string.empty
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is null")]

        public void Convert2Date_IfFieldvaluesIsNull_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2Date", _apacheaccess, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }


        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : Convert2Date tested if fieldvalues is not null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = 08 SEP 2014:14:33:58 1
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * Input date have been converted
        /// </summary>
        [TestCase("08/SEP/2014:14:33:58 1", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsOneDecimal")] 
        [TestCase("08/SEP/2014:14:33:58 11", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsTwoDecimal")]
        [TestCase("08/SEP/2014:14:33:58 111", Result = "2014/09/08 14:33:58", TestName = "IfTimeFormatIsThreeDecimal")]
        [Test(Description = "Convert2Date tested if fieldvalues is not null")]
        public object Convert2Date_IfFieldvaluesNotNull_returnDate(string fieldvalue)
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { fieldvalue };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2Date", _apacheaccess, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull


            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : Convert2Date tested if fieldvalues is null
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = string.empty 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return should string.empty 
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is null")]
        public void Convert2Date_IfFieldvaluesNull_returnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { string.Empty };
            object data = null;

            //Act

            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2Date", _apacheaccess, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull


            //Assert
            Assert.AreEqual(string.Empty, actual);

        }

        /// <summary>
        /// Method Name : Convert2Date
        ///
        ///Method Description : Convert to the input date 
        ///
        ///Test Scenario : If date format is wrong
        ///
        ///Known Input :
        ///     * rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"09/09/14 2:55:PM"} 
        ///		* data = null
        ///
        ///Expected Output :
        ///     * return String Empty
        /// </summary>
        [TestCase("09/09/14 2:55:PM", Result = null, TestName = "IfDateFormatIsWrong")]
        [TestCase("9 EYL 2014 14:55:53", Result = null, TestName = "IfDateFormatIsTurkish")]
        [Test(Description = "If date format is wrong")]
        public int Convert2Date_IfDateFormatIsWrong_ReturnStringEmpty(string fieldvalue)
        {
            //Arrange
            String[] fieldvalues = { fieldvalue };
            string field = null;
            RecWrapper rec = null;
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = (int)MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2Date", _apacheaccess, new[] { rec, field, fieldvalues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            return actual;

        }

        /// <summary>
        /// Method Name : Convert2Date
        /// 
        /// Method Desciption : Convert to input date
        /// 
        /// Test Scenerio : If date is not the given format
        /// 
        /// Known Input :
        ///     
        ///		* rec = null 
        ///		* field = null 
        ///		* fieldvalues = {"2014 09 10", "14:33:58"} 
        ///		* data = null
        /// 
        /// Expected Output : 
        ///     * return string.empty
        /// </summary>
        [Test(Description = "Convert2Date tested if fieldvalues is null")]
        public void Convert2Date_IfFieldvaluesNotGivenFormat_ReturnStringEmpty()
        {
            //Arrange
            RecWrapper rec = null;
            string field = null;
            string[] fieldValues = { "2014 09 10", "14:33:58" };
            object data = null;

            //Act
            // ReSharper disable ExpressionIsAlwaysNull
            var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2Date", _apacheaccess, new[] { rec, field, fieldValues, data });
            // ReSharper restore ExpressionIsAlwaysNull

            //Assert
            Assert.AreEqual(string.Empty, actual);

        }

     /// <summary>
     /// Method Name : Convert2IpAddressSplit
     /// 
     /// Method Desciption : Split the ip address
     /// 
     /// Test Scenerio : Convert2IpAddressSplit tested if fieldvalues is null
     /// 
     /// Known Input :
     ///     
     ///		* rec = null 
     ///		* field = null 
     ///		* fieldvalues = null
     ///		* data = null
     /// 
     /// Expected Output : 
     ///     * return null
     /// </summary>
     [Test(Description = "Convert2IpAddressSplit tested if fieldvalues is null")]
     public void Convert2IpAddressSplit_IfFieldvaluesNull_ReturnNull()
     {
         //Arrange
         RecWrapper rec = null;
         string field = null;
         string[] fieldValues = null;
         object data = null;

         //Act
         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2IpAddressesSplit",_apacheaccess, new[] {rec, field, fieldValues, data});
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(null, actual);
     }

     /// <summary>
     /// Method Name : Convert2IpAddressSplit
     /// 
     /// Method Desciption : Split the ip address
     /// 
     /// Test Scenerio : Convert2IpAddressSplit tested if fieldvalues is string.Empty
     /// 
     /// Known Input :
     ///     
     ///		* rec = null 
     ///		* field = null 
     ///		* fieldvalues = {string.Empty}
     ///		* data = null
     /// 
     /// Expected Output : 
     ///     * return string.Empty
     /// </summary>
     [Test(Description = "Convert2IpAddressSplit tested if fieldvalues is string.Empty")]

     public void Convert2IpAddressSplit_IfFieldvaluesStringEmpty_ReturnStringEmpty()
     {
         //Arrange
         RecWrapper rec = null;
         string field = null;
         string[] fieldValues = {string.Empty};
         object data = null;

         //Act
         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2IpAddressesSplit",
             _apacheaccess, new[] { rec, field, fieldValues, data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(string.Empty, actual);
     }

     /// <summary>
     /// Method Name : Convert2IpAddressSplit
     /// 
     /// Method Desciption : Split the ip address
     /// 
     /// Test Scenerio : Convert2IpAddressSplit tested if fieldvalues is not null
     /// 
     /// Known Input :
     ///     
     ///		* rec = null 
     ///		* field = null 
     ///		* fieldvalues = {"1111.222.333,123.123.123"}
     ///		* data = null
     /// 
     /// Expected Output : 
     ///     * return splited index of value[0] data 
     /// </summary>
     [Test(Description = "Convert2IpAddressSplit tested if fieldvalues is not null")]

     public void Convert2IpAddressSplit_IfFieldvaluesNotNull_ReturnValueFirstData()
     {
         //Arrange
         RecWrapper rec = null;
         string field = null;
         string[] fieldValues = { "1111.222.333 , 123.123.123"};
         object data = null;

         //Act
         // ReSharper disable ExpressionIsAlwaysNull
         var actual = (string)MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2IpAddressesSplit",
             _apacheaccess, new[] { rec, field, fieldValues, data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual("1111.222.333", actual);
     }

     /// <summary>
     /// Method Name : Convert2IpAddressSplit
     /// 
     /// Method Desciption : Split the ip address
     /// 
     /// Test Scenerio : Convert2IpAddressSplit tested if fieldvalues is wrong format
     /// 
     /// Known Input :
     ///     
     ///		* rec = null 
     ///		* field = null 
     ///		* fieldvalues = {"ipsum lorem"}
     ///		* data = null
     /// 
     /// Expected Output : 
     ///     * return index of value[0] data 
     /// </summary>
     [Test(Description = "Convert2IpAddressSplit tested if fieldvalues is wrong format")]

     public void Convert2IpAddressSplit_IfFieldvaluesWrongFormat_ReturnValueFirstData()
     {
         //Arrange
         RecWrapper rec = null;
         string field = null;
         string[] fieldValues = { "ipsum , lorem" };
         object data = null;

         //Act
         // ReSharper disable ExpressionIsAlwaysNull
         var actual = (string)MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2IpAddressesSplit",
             _apacheaccess, new[] { rec, field, fieldValues, data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual("ipsum", actual);
     }


     /// <summary>
     /// Method Name : GetHeaderInfo
     ///
     ///Method Description : The method check is there a header
     ///
     ///Test Scenario : If context.HeaderInfo is null
     ///
     ///Known Input :
     ///    * context.HeaderInfo = null
     ///
     ///Expected Output :
     ///    * NextInstruction.Do should return
     /// </summary>
     /// 
     [Test(Description = "If context.HeaderInfo is null")]
     public void GetHeaderInfo_IfContextHeaderInfoIsNull_ReturnNextInstructionDo()
     {
         //Arrange
         var context = new SyslogRecorderContext(_apacheaccess, ProtocolType.Udp, "192.168.1.25")
         {
             HeaderInfo = null
         };
         Exception e = null;
         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, NextInstruction>("GetHeaderInfo", _apacheaccess, new object[] { context, e });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(NextInstruction.Do, actual);
     }


     /// <summary>
     /// Method Name : GetHeaderInfo
     ///
     ///Method Description : The method check is there a header
     ///
     ///Test Scenario : If context.HeaderInfo is not null
     ///
     ///Known Input :
     ///    * context.HeaderInfo = DataMappingInfo
     ///
     ///Expected Output :
     ///    * NextInstruction.Do should return
     /// </summary>
     /// 
     [Test(Description = "If context.HeaderInfo is not null")]
     public void GetHeaderInfo_IfContextHeaderInfoIsNotNull_ReturnNextInstructionDo()
     {
         //Arrange
         var context = new SyslogRecorderContext(_apacheaccess, ProtocolType.Udp, "192.168.1.25")
         {
             HeaderInfo = new DataMappingInfo()
         };
         Exception e = null;
         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, NextInstruction>("GetHeaderInfo", _apacheaccess, new object[] { context, e });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(actual, NextInstruction.Do);
     }


     /// <summary>
     /// Method Name : OnBeforeSetData
     ///
     ///Method Description : The method set the value record in context
     ///
     ///Test Scenario : If context is null
     ///
     ///Known Input :
     ///    * TerminalRecorderContext context = null
     ///
     ///Expected Output :
     ///    * null should return
     /// </summary>
     /// 
     [Test(Description = "If context is null")]
     public void OnBeforeSetData_IfContextIsNull_ReturnNull()
     {
         //Arrange
         TerminalRecorderContext context = null;
          
         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, NextInstruction>("OnBeforeSetData", _apacheaccess, new object[] { context });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(null, actual);
     }


     /// <summary>
     /// Method Name : OnBeforeSetData
     ///
     ///Method Description : The method set the value record in context
     ///
     ///Test Scenario : If context is not null
     ///
     ///Known Input :
     ///    * RecorderContext context = new ApacheAccessRecorderContext();
     ///
     ///Expected Output :
     ///    * NextInstruction.Do should return
     /// </summary>
     /// 
     [Test(Description = "If context is not null")]
     public void OnBeforeSetData_IfContextIsNotNull_ReturnNextInstructionDo()
     {
         //Arrange
         RecorderContext context = new ApacheAccessRecorderContext();

         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, NextInstruction>("OnBeforeSetData", _apacheaccess, new object[] { context });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(NextInstruction.Do, actual);
     }


     /// <summary>
     /// Method Name : GetHeaderText
     ///
     ///Method Description : The method get the header text
     ///
     ///Test Scenario : If context is null
     ///
     ///Known Input :
     ///    * TerminalRecorderContext context = null
     ///
     ///Expected Output :
     ///    * null should return
     /// </summary>
     /// 
     [Test(Description = "If context is null")]
     public void GetHeaderText_IfContextIsNull_ReturnNull()
     {
         //Arrange
         TerminalRecorderContext context = null;

         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, string>("GetHeaderText", _apacheaccess, new object[] { context });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(string.Empty, actual);
     }


     /// <summary>
     /// Method Name : GetHeaderText
     ///
     ///Method Description : The method get the header text
     ///
     ///Test Scenario : If context is not null
     ///
     ///Known Input :
     ///    *  RecorderContext context = new ApacheAccessRecorderContext()
     ///
     ///Expected Output :
     ///    * string.Empty should return
     /// </summary>
     /// 
     [Test(Description = "If context is not null")]
     public void GetHeaderText_IfContextIsNotNull_ReturnStringEmpty()
     {
         //Arrange
         RecorderContext context = new ApacheAccessRecorderContext();
      
         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, string>("GetHeaderText", _apacheaccess, new object[] { context });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(string.Empty, actual);
     }


     /// <summary>
     /// Method Name : Convert2MethodUriProtocol
     ///
     ///Method Description : The method convert data to uri protocol
     ///
     ///Test Scenario : If fieldvalue is null
     ///
     ///Known Input :
     ///    * string[] fieldvalues = null;
     ///    * RecWrapper rec = null;
     ///    * string field = null;
     ///    * object data = null;
     ///
     ///Expected Output :
     ///    * null should return
     /// </summary>
     /// 
     [Test(Description = "If fieldvalue is null")]

     public void Convert2MethodUriProtocol_IfFieldValueIsNull_ReturnNull()
     {
         //Arrange
         string[] fieldvalues = null;
         RecWrapper rec = null;
         string field = null;
         object data = null;

         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2MethodUriProtocol", _apacheaccess, new[] { rec,field,fieldvalues,data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(null, actual);
     }

     /// <summary>
     /// Method Name : Convert2MethodUriProtocol
     ///
     ///Method Description : The method convert data to uri protocol
     ///
     ///Test Scenario : If fieldvalue is string.Empty
     ///
     ///Known Input :
     ///    * string[] fieldvalues = {string.Empty};
     ///    * RecWrapper rec = null;
     ///    * string field = null;
     ///    * object data = null;
     ///
     ///Expected Output :
     ///    * String.Empty should return
     /// </summary>
     /// 
     [Test(Description = "If fieldvalue is string.Empty")]

     public void Convert2MethodUriProtocol_IfFieldValueIsStringEmpty_ReturnStringEmpty()
     {
         //Arrange
         string[] fieldvalues = {string.Empty};
         RecWrapper rec = null;
         string field = null;
         object data = null;

         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2MethodUriProtocol", _apacheaccess, new[] { rec, field, fieldvalues, data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual(string.Empty, actual);
     }

     /// <summary>
     /// Method Name : Convert2MethodUriProtocol
     ///
     ///Method Description : The method convert data to uri protocol
     ///
     ///Test Scenario : If fieldvalue is not null
     ///
     ///Known Input :
     ///    * string[] fieldvalues = {"lorem", "ipsum", "dolor", "sit", "amet"};
     ///    * RecWrapper rec = null;
     ///    * string field = null;
     ///    * object data = null;
     ///
     ///Expected Output :
     ///    *  "ipsum" should return
     /// </summary>
     /// 
     [Test(Description = "If fieldvalue is not null")]

     public void Convert2MethodUriProtocol_IfFieldValueIsNotNull_ReturnFirstDataGroup()
     {
         //Arrange
         string[] fieldvalues = { "lorem", "ipsum", "dolor", "sit", "amet" };
         RecWrapper rec = null;
         string field = null;
         object data = null;

         //Act

         // ReSharper disable ExpressionIsAlwaysNull
         var actual = MethodTestHelper.RunInstanceMethod<ApacheAccessUnifiedRecorder, object>("Convert2MethodUriProtocol", _apacheaccess, new[] { rec, field, fieldvalues, data });
         // ReSharper restore ExpressionIsAlwaysNull

         //Assert
         Assert.AreEqual("lorem", actual);
     }
  }
}
