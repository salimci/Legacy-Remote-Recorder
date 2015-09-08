using Natek.Recorders.Remote.Helpers.Mapping;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class MappedDataHelperUnitTestFixture
    {
        /// <summary>
        /// Method Name : ClearRecord
        ///
        ///Method Description :  The method clear the record
        ///
        ///Test Scenario :  Set value in recWrapper
        ///
        ///Known Input :
        ///    *  rec = new RecWrapper()
        ///    *  rec.ComputerName = "lorem Ipsum"
        ///    
        ///
        ///Expected Output :
        ///    * Return should null
        /// </summary>
        [Test]
        public void ClearRecord_SetValueInRecWrapper_ClearSettedValue()
        {
            //Arrange
            var rec = new RecWrapper {ComputerName = "lorem Ipsum"};

            //Act
            MappedDataHelper.ClearRecord(rec);

            //Assert
            Assert.IsNull(rec.ComputerName);
        }
    }
}
