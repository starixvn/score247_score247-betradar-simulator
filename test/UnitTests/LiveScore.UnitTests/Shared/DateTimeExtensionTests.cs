namespace LiveScore.UnitTests.Shared
{
    using System;
    using LiveScore.Soccer.Shared.Helpers;
    using Xunit;

    public class DateTimeExtensionTests
    {
        [Fact]
        public void ConvertToUtc_ReturnCorrectDateTime()
        {
            var timeZone = new TimeSpan(7, 0, 0);
            var date = new DateTime(2019, 05, 21, 0, 0, 0);
            var expectedDate = new DateTime(2019, 05, 20, 17, 0, 0);

            var actualDate = date.ConvertToUtc(timeZone);

            Assert.Equal(expectedDate, actualDate);
        }
    }
}
