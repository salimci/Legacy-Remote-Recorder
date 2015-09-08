using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Natek.Helpers;
using NUnit.Framework;


namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class DateCompareHelperUnitTestFixture
    {
        [Test]
        public void CreateDayIndex_WithEmptyParams_ReturnMapAccordingCurrentLocale()
        {
            //Arrange
            var expectedDayList = new[] {"PAZ", "PZT", "SAL", "ÇAR", "PER", "CUM", "CMT", "PAZAR", "PAZARTESI", "SALı", "ÇARŞAMBA", "PERŞEMBE", "CUMA", "CUMARTESI"};

            //Act
            var actual = DateCompareHelper.CreateDayIndex();
            var actualDayList = actual.Keys.ToArray();

            //Assert
            Assert.AreEqual(expectedDayList,actualDayList);
            
        }

        [Test]
        public void CreateDayIndex_WithGivenListOfDays_ReturnMapAccordingToGivenList()
        {
            //Arrange
            var expectedDayList = new[] { "LOREM","PAZ", "PZT", "SAL", "ÇAR", "PER", "CUM", "CMT", "PAZAR", "PAZARTESI", "SALı", "ÇARŞAMBA", "PERŞEMBE", "CUMA", "CUMARTESI" };
            var indexMap = "LOREM:0";

            //Act
            var actual = DateCompareHelper.CreateDayIndex(null, indexMap);
            var actualDayList = actual.Keys.ToArray();

            //Assert
            Assert.AreEqual(expectedDayList, actualDayList);
        }

        [TestCase("PAZ", "PZT", Result = -1, TestName = "CompareDateTimeDay_PazPzt_ReturnNegative")]
        [TestCase("PZT", "PAZ", Result = 1, TestName = "CompareDateTimeDay_PztPaz_ReturnPositive")]
        [TestCase("PAZ", "PAZ", Result = 0, TestName = "CompareDateTimeDay_PazPaz_ReturnZero")]
        public int CompareDateTimeDay_CompareDates(string left, string right)
        {
            //Act 
            return DateCompareHelper.CompareDateTimeDay(left, right, DateCompareHelper.CreateDayIndex(), false);
        }
    }
}
