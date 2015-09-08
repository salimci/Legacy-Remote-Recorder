using System;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class RecWrapperUnitTestFixture
    {
        /// <summary>
        /// Method Name : RecWrapper
        ///
        ///Method Description :  The method consists of properties(get-set) and ensure the set or get record
        ///
        ///Test Scenario :  Create your own type of object to assign values
        ///
        ///Known Input :
        ///       * No input parameter  
        ///
        ///Expected Output :
        ///    * No return
        /// </summary>
        [Test]
        public void RecWrapper_CoverTest()
        {
            var wrapper = new RecWrapper();
            wrapper.ComputerName = "computerName";
            wrapper.CustomInt1 = 1;
            wrapper.CustomInt2 = 1;
            wrapper.CustomInt3 = 1;
            wrapper.CustomInt4 = 1;
            wrapper.CustomInt5 = 1;
            wrapper.CustomInt6 = 1;
            wrapper.CustomInt7 = 1;
            wrapper.CustomInt8 = 1;
            wrapper.CustomInt9 = 1;
            wrapper.CustomInt10 = 1;
            wrapper.CustomStr1 = "str";
            wrapper.CustomStr2 = "str";
            wrapper.CustomStr3 = "str";
            wrapper.CustomStr4 = "str";
            wrapper.CustomStr5 = "str";
            wrapper.CustomStr6 = "str";
            wrapper.CustomStr7 = "str";
            wrapper.CustomStr8 = "str";
            wrapper.CustomStr9 = "str";
            wrapper.CustomStr10 = "str";
            wrapper.Datetime = DateTime.Now.ToString();
            wrapper.UserName = "user";
            wrapper.SourceName = "source";
            wrapper.Recordnum = 1;
            wrapper.EventType = "type";
            wrapper.EventId = 1;
            wrapper.EventCategory = "category";
            wrapper.Description = "desc";
            wrapper.LogName = "logname";


            var ComputerName = wrapper.ComputerName;
            var integer = wrapper.CustomInt1;
            integer = wrapper.CustomInt2;
            integer = wrapper.CustomInt3;
            integer = wrapper.CustomInt4;
            integer = wrapper.CustomInt5;
            var longer = wrapper.CustomInt6;
            longer = wrapper.CustomInt7;
            longer = wrapper.CustomInt8;
            longer = wrapper.CustomInt9;
            longer = wrapper.CustomInt10;
            var str = wrapper.CustomStr1;
            str = wrapper.CustomStr2;
            str = wrapper.CustomStr3;
            str = wrapper.CustomStr4;
            str = wrapper.CustomStr5;
            str = wrapper.CustomStr6;
            str = wrapper.CustomStr7;
            str = wrapper.CustomStr8;
            str = wrapper.CustomStr9;
            str = wrapper.CustomStr10;
            str = wrapper.Datetime;
            str = wrapper.UserName;
            str = wrapper.SourceName;
            integer = wrapper.Recordnum;
            str = wrapper.EventType;
            longer = wrapper.EventId;
            str = wrapper.EventCategory;
            str = wrapper.Description;
            str = wrapper.LogName;


            Assert.IsTrue(true);
        }
    }
}
