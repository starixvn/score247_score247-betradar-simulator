using Microsoft.Extensions.Caching.Memory;
using Fanex.Caching;
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
        private readonly DataAccessFactory instanceFactory;
        private readonly IMatchDataAccess matchDataAccess;
        private readonly ICacheService cacheService;

        public MatchServiceTests()
        {
            cacheService = new CacheService();

            matchDataAccess = Substitute.For<IMatchDataAccess>();
            instanceFactory = Substitute.For<DataAccessFactory>();
            cacheService = Substitute.For<ICacheService>();
            instanceFactory.CreateMatchDataAccess(SoccerSportId).Returns(matchDataAccess);

            matchServiceImpl = new MatchService(instanceFactory, cacheService);
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
                .GetMatches(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en")
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

        [Fact]
        public async Task GetMatches_HasLiveMatches_ShouldPrioritizeLiveMatches()
        {
            var matches = new List<Match>
            {
                new Match { Id ="1", Name = "A", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00") },
                new Match { Id ="2", Name = "B", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00") },
                new Match { Id ="3", Name = "C", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00") }
            };

            var liveMatches = new Dictionary<string, IMatch>
            {
                { "1", new Match { Id ="1", Name = "A", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult{ MatchTime = 3 } } }
            };

            cacheService.GetAsync<Dictionary<string, IMatch>>("LiveMatch_1_en").Returns(liveMatches);

            matchDataAccess
                .GetMatches(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en")
                .Returns(Task.FromResult(matches as IEnumerable<IMatch>));

            var actualMatches = (await matchServiceImpl.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                new TimeSpan(1, 0, 0),
                "en")).ToList();

            Assert.True(actualMatches.Any());
            Assert.Equal(3, actualMatches.Count);
            Assert.Equal(3, actualMatches.First(x => x.Id.Equals("1")).MatchResult.MatchTime);
        }
    }
}