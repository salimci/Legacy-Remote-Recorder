using Natek.Helpers.Providers.Ticket;
using NUnit.Framework;

namespace Natek.Recorders.Remote.Test
{
    [TestFixture]
    public class TimestampProviderUnitTestFixture
    {
        [Test]
        public void Next_GetTwoRandomValue_CheckIfTheseAreDiff()
        {
            //Arrange & Act
            var timestamp1 = TimestampProvider.Next;
            var timestamp2 = TimestampProvider.Next;

            //Assert
            Assert.AreNotEqual(timestamp1,timestamp2);
        }
    }
}
