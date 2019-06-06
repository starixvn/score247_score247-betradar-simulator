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
        private readonly IMatchDataAccess sportRadarDataAccess;
        private readonly ICacheService cacheService;

        public MatchServiceTests()
        {
            cacheService = new CacheService();

            matchDataAccess = Substitute.For<IMatchDataAccess>();
            sportRadarDataAccess = Substitute.For<IMatchDataAccess>();
            instanceFactory = Substitute.For<DataAccessFactory>();
            cacheService = Substitute.For<ICacheService>();
            instanceFactory.CreateMatchDataAccess(SoccerSportId).Returns(matchDataAccess);
            instanceFactory.CreateMatchDataAccess(SoccerSportId, DataProvider.ThirdPartyApi).Returns(sportRadarDataAccess);

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
                new Match {
                    EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"),
                    MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed }
                }
            };

            cacheService.GetAsync<IEnumerable<IMatch>>(Arg.Any<string>()).Returns(matches as IEnumerable<IMatch>);

            matchDataAccess
                .GetMatches(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en")
                .Returns(Task.FromResult(matches as IEnumerable<IMatch>));

            var actualMatches = (await matchServiceImpl.GetMatches(
                1,
                DateTime.Today,
                DateTime.Today.AddDays(1),
                new TimeSpan(1, 0, 0),
                "en")).ToList();

            Assert.True(actualMatches.Any());
            Assert.Equal("04:04:05", actualMatches.First().EventDate.ToString("HH:mm:ss"));
        }

        [Fact]
        public async Task GetMatches_HasLiveMatches_ShouldPrioritizeLiveMatches()
        {
            var matches = new List<Match>
            {
                new Match { Id ="1", Name = "A", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed }},
                new Match { Id ="2", Name = "B", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed } },
                new Match { Id ="3", Name = "C", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed } }
            };

            var liveMatches = new Dictionary<string, IMatch>
            {
                { "1", new Match { Id ="1", Name = "A", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult{ MatchTime = 3 } } }
            };

            cacheService.GetAsync<IDictionary<string, IMatch>>("LiveMatch_1_en").Returns(liveMatches);
            cacheService.GetAsync<IEnumerable<IMatch>>(Arg.Any<string>()).Returns(matches as IEnumerable<IMatch>);

            matchDataAccess
                .GetMatches(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en")
                .Returns(Task.FromResult(matches as IEnumerable<IMatch>));

            var actualMatches = (await matchServiceImpl.GetMatches(
                1,
                DateTime.Today,
                DateTime.Today.AddDays(1),
                TimeZoneInfo.Local.GetUtcOffset(DateTime.Today),
                "en")).ToList();

            Assert.True(actualMatches.Any());
            Assert.Equal(3, actualMatches.Count);
            Assert.Equal(3, actualMatches.First(x => x.Id.Equals("1")).MatchResult.MatchTime);
        }

        [Theory]
        [InlineData(0, null)]
        [InlineData(0, "sr:match:1")]
        [InlineData(1, "")]
        [InlineData(1, "   ")]
        public async Task GetMatch_InvalidSportOrMatchId_ShouldThrowsException(int sportId, string matchId)
        {

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => (matchServiceImpl.GetMatch(
                sportId,
                matchId,
                "en-US")));

            Assert.Equal("Missing sportId or matchId argument", exception.Message);
        }

        [Fact]
        public async Task GetMatch_ReturnAssignedValues()
        {
            var match = new Match { Id = "sr:match:1", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult { EventStatus = MatchStatus.Closed } };

            matchDataAccess
                .GetMatch(1, Arg.Any<string>(), "en-US")
                .Returns(match);

            var actualMatch = await matchServiceImpl.GetMatch(
                1,
                "sr:match:1",
                "en-US");

            Assert.NotNull(actualMatch);
            Assert.Equal(match.Id, actualMatch.Id);
            Assert.Equal(match.MatchResult.EventStatus, actualMatch.MatchResult.EventStatus);
        }

        [Fact]
        public async Task GetMatch_HasTimelines_ReturnAssignedTimelines()
        {
            var match = new Match { Id = "sr:match:1", EventDate = DateTime.Parse("2018-11-11T03:04:05+00:00"), MatchResult = new MatchResult { EventStatus = MatchStatus.Closed }, Region = "eu" };
            var timelines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.MatchStarted.Value },
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1 }
            };

            matchDataAccess
                .GetMatch(1, "sr:match:1", "en-US")
                .Returns(match);

            matchDataAccess
                .GetTimelines(1, "eu", "sr:match:1", "en-US")
                .Returns(timelines);

            var actualMatch = await matchServiceImpl.GetMatch(
                1,
                "sr:match:1",
                "en-US");

            Assert.NotEmpty(actualMatch.TimeLines);
            Assert.Equal(2, actualMatch.TimeLines.Count());
            Assert.Equal(EventTypes.MatchStarted.Value, actualMatch.TimeLines.First().Type);
        }
    }
}