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
    using Fanex.Caching;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Enumerations;

    public class MatchServiceTests
    {
        private const int SoccerSportId = 1;
        private readonly MatchService matchServiceImpl;
        private readonly IMatchDataAccess matchDataAccess;
        private readonly IMatchDataAccess sportRadarDataAccess;
        private readonly ICacheService cacheService;
        private readonly IAppSettings appSettings;
        private readonly Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory;

        public MatchServiceTests()
        {
            cacheService = new CacheService();

            matchDataAccess = Substitute.For<IMatchDataAccess>();
            sportRadarDataAccess = Substitute.For<IMatchDataAccess>();
            cacheService = Substitute.For<ICacheService>();
            appSettings = Substitute.For<IAppSettings>();
            matchDataAccessFactory = (sportId, provider) =>
            {
                switch (sportId)
                {
                    case SoccerSportId when provider == DataProvider.Database:
                        return matchDataAccess;

                    default:
                        return sportRadarDataAccess;
                }
            };

            appSettings.NumberOfTopMatches.Returns(int.MaxValue);
            matchServiceImpl = new MatchService(cacheService, appSettings, matchDataAccessFactory);
        }

        #region GetMatches

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
                new Match { Id ="2", Name = "B", EventDate = DateTime.Parse("2018-11-11T03:04:06+00:00"), MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed } },
                new Match { Id ="3", Name = "C", EventDate = DateTime.Parse("2018-11-11T03:04:07+00:00"), MatchResult = new MatchResult {  EventStatus = MatchStatus.Closed } }
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

        #endregion GetMatches

        #region GetMatch

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
                "en-US",
                TimeZoneInfo.Local.GetUtcOffset(DateTime.Today))));

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
                "en-US",
                TimeZoneInfo.Local.GetUtcOffset(DateTime.Today));

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
                "en-US",
                TimeZoneInfo.Local.GetUtcOffset(DateTime.Today));

            Assert.NotEmpty(actualMatch.TimeLines);
            Assert.Equal(2, actualMatch.TimeLines.Count());
            Assert.Equal(EventTypes.MatchStarted.Value, actualMatch.TimeLines.First().Type);
        }

        #endregion GetMatch

        #region AddFullTimelines

        [Fact]
        public async Task AddFullTimelines_Always_GetTimelinesFromSportRadar()
        {
            await matchServiceImpl.AddFullTimelines(
                1,
                "sr:match:1",
                "en-US",
                "eu");

            await sportRadarDataAccess.Received(1).GetTimelines(1, "eu", "sr:match:1", "en");
        }

        [Fact]
        public async Task AddFullTimelines_NoTimelines_ShouldNotAddCommentaries()
        {
            await matchServiceImpl.AddFullTimelines(
                1,
                "sr:match:1",
                "en-US",
                "eu");

            await matchDataAccess.DidNotReceive().AddCommentaries(Arg.Any<string>(), Arg.Any<List<Timeline>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task AddFullTimelines_NoTimelines_ShouldNotAddTimelines()
        {
            await matchServiceImpl.AddFullTimelines(
                1,
                "sr:match:1",
                "en-US",
                "eu");

            await matchDataAccess.DidNotReceive().AddTimelines(Arg.Any<string>(), Arg.Any<IEnumerable<ITimeline>>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task AddFullTimelines_HasTimelines_AddCommentaries()
        {
            sportRadarDataAccess.GetTimelines(1, "eu", "sr:match:1", "en").Returns(new List<Timeline>
            {
                new Timeline{ Type = EventTypes.MatchStarted.Value, Commentaries=new List<Commentary>{ new Commentary { Text="Match has started" } } }
            });

            await matchServiceImpl.AddFullTimelines(
                1,
                "sr:match:1",
                "en-US",
                "eu");

            await matchDataAccess.Received(1).AddCommentaries("sr:match:1", Arg.Any<List<Timeline>>(), "en-US");
        }

        [Fact]
        public async Task AddFullTimelines_HasTimelines_AddTimelinesExcludeCommetaries()
        {
            sportRadarDataAccess.GetTimelines(1, "eu", "sr:match:1", "en").Returns(new List<Timeline>
            {
                new Timeline{ Type = EventTypes.MatchStarted.Value, Commentaries=new List<Commentary>{ new Commentary { Text="Match has started" } } }
            });

            await matchServiceImpl.AddFullTimelines(
                1,
                "sr:match:1",
                "en-US",
                "eu");

            await matchDataAccess.Received(1).AddTimelines("sr:match:1", Arg.Any<IEnumerable<ITimeline>>(), isDeleteExisting: true);
        }

        #endregion AddFullTimelines

        #region AddTimelines

        [Fact]
        public async Task AddTimelines_Null_ShouldNotAdd()
        {
            await matchServiceImpl.AddTimelines(
                1,
                "sr:match:1",
                null);

            await matchDataAccess.DidNotReceive().AddTimelines("sr:match:1", Arg.Any<IEnumerable<ITimeline>>());
        }

        [Fact]
        public async Task AddTimelines_HasTimelines_AddTimelines()
        {
            var timelines = new List<Timeline>
            {
                new Timeline{ Type = EventTypes.MatchStarted.Value, Commentaries=new List<Commentary>{ new Commentary { Text="Match has started" } } }
            };

            await matchServiceImpl.AddTimelines(
                1,
                "sr:match:1",
                timelines);

            await matchDataAccess.Received(1).AddTimelines("sr:match:1", Arg.Any<IEnumerable<ITimeline>>());
        }

        #endregion AddTimelines

        #region AddTimelines

        [Fact]
        public async Task AddResult_Null_ShouldNotAddResult()
        {
            await matchServiceImpl.AddResult(
                1,
                "sr:match:1",
                null);

            await matchDataAccess.DidNotReceive().AddResult("sr:match:1", Arg.Any<MatchResult>());
        }

        [Fact]
        public async Task AddResult_HasTimelines_AddResult()
        {
            var result = new MatchResult();

            await matchServiceImpl.AddResult(
                1,
                "sr:match:1",
                result);

            await matchDataAccess.Received(1).AddResult("sr:match:1", Arg.Any<MatchResult>());
        }

        #endregion AddTimelines
    }
}