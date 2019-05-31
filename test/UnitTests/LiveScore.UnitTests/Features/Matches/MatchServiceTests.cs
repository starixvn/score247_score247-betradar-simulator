namespace LiveScore.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using NSubstitute;
    using Xunit;

    public class MatchServiceTests
    {
        private const int SoccerSportId = 1;
        private readonly MatchService matchServiceImpl;
        private readonly InstanceFactory instanceFactory;
        private readonly IMatchDataAccess matchDataAccess;

        public MatchServiceTests()
        {
            matchDataAccess = Substitute.For<IMatchDataAccess>();
            instanceFactory = Substitute.For<InstanceFactory>();
            instanceFactory.CreateMatchDataAccess(SoccerSportId).Returns(matchDataAccess);

            matchServiceImpl = new MatchService(instanceFactory, null);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetMatches_SportIdIsZeroOrNegative_ReturnEmptyMatches(int sportId)
        {
            var matches = await matchServiceImpl.GetMatches(sportId, DateTime.Now, DateTime.Now.AddDays(1), new TimeSpan(1, 0, 0), "en");

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_ToDateIsLowerThanFromDate_ReturnEmptyMatches()
        {
            var matches = await matchServiceImpl.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(-1),
                new TimeSpan(1, 0, 0),
                "en");

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_ToDateIsMinValue_ReturnEmptyMatches()
        {
            var matches = await matchServiceImpl.GetMatches(
                1,
                DateTime.MinValue,
                DateTime.Now.AddDays(-1),
                new TimeSpan(1, 0, 0),
                "en");

            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnMatchesWithTimezone()
        {
            var matches = new List<Match>
            {
                new Match { EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00") }
            };

            matchDataAccess
                .GetMatches(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en", Arg.Any<TimeSpan>())
                .Returns(Task.FromResult(matches as IEnumerable<IMatch>));

            var actualMatches = (await matchServiceImpl.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                new TimeSpan(1, 0, 0),
                "en")).ToList();

            Assert.True(actualMatches.Any());
            Assert.Equal("11/11/2018 4:04:05 AM", actualMatches.First().EventDate.ToString());
        }
    }
}