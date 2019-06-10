namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using LiveScore.Soccers.UnitTests.Shared;
    using NSubstitute;
    using Xunit;

    public class MatchDataAccessTests
    {
        private readonly MatchDataAccess matchDataAccess;
        private readonly IDynamicRepository dynamicRepository;

        public MatchDataAccessTests()
        {
            dynamicRepository = Substitute.For<IDynamicRepository>();
            dynamicRepository.Query(Arg.Any<Func<MatchEntity, bool>>()).Returns(new List<MatchEntity> { StubMatch.StubMatchEntity() });
            dynamicRepository.Query(Arg.Any<Func<LiveMatchEntity, bool>>()).Returns(new List<LiveMatchEntity> { StubMatch.StubLiveMatchEntity() });

            var logger = Substitute.For<ILogger>();

            matchDataAccess = new MatchDataAccess(dynamicRepository, logger);
        }

        #region GetMatches

        [Fact]
        public async Task GetMatches_Always_ReturnTeams()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var home = match.Teams.ElementAt(0);
            var away = match.Teams.ElementAt(1);

            Assert.Equal(2, match.Teams.Count());

            Assert.Equal("sr:competitor:559146", home.Id);
            Assert.Equal("Denton Diablos FC", home.Name);
            Assert.Equal("USA", home.Country);
            Assert.Equal("USA", home.CountryCode);
            Assert.Equal("DEN", home.Abbreviation);
            Assert.True(home.IsHome);

            Assert.Equal("sr:competitor:559156", away.Id);
            Assert.Equal("Dallas City FC", away.Name);
            Assert.Equal("USA", away.Country);
            Assert.Equal("USA", away.CountryCode);
            Assert.Equal("DAL", away.Abbreviation);
            Assert.False(away.IsHome);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueInformation()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:28424", league.Id);
            Assert.Equal("National Premier Soccer League", league.Name);
            Assert.Equal("sr:category:26", league.Category.Id);
            Assert.Equal("USA", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueRound()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var leagueRound = match.LeagueRound;

            Assert.Equal("regular season", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnMatchResult()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var matchResult = match.MatchResult;

            Assert.Equal("not_started", matchResult.EventStatus.Value);
            Assert.Equal("not_started", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(0, matchResult.AwayScore);
            Assert.Equal(0, matchResult.Period);
            Assert.Equal(0, matchResult.MatchTime);
        }

        [Fact]
        public async Task GetMatches_Live_ReturnMatchResult()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchEntity, bool>>()).Returns(new List<MatchEntity> { StubMatch.StubMatchEntityWithLiveResult() });

            var matches = await matchDataAccess.GetMatches(
               1,
               DateTime.Now,
               DateTime.Now.AddDays(1),
               "en");

            var match = matches.First();
            var matchResult = match.MatchResult;

            Assert.Equal("live", matchResult.EventStatus.Value);
            Assert.Equal("1st_half", matchResult.MatchStatus.Value);
            Assert.Equal(1, matchResult.HomeScore);
            Assert.Equal(0, matchResult.AwayScore);
            Assert.Equal(22, matchResult.MatchTime);
        }

        [Fact]
        public async Task GetMatches_Live_ReturnMatchTimeline()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchEntity, bool>>()).Returns(new List<MatchEntity> { StubMatch.StubMatchEntityWithLiveResult() });

            var matches = await matchDataAccess.GetMatches(
               1,
               DateTime.Now,
               DateTime.Now.AddDays(1),
               "en");

            var match = matches.First();
            var timeline = match.TimeLines.First();

            Assert.NotEmpty(match.TimeLines);
            Assert.Equal("injury_time_shown", timeline.Type);
            Assert.Equal(2, timeline.InjuryTimeAnnounced);
            Assert.Equal(1, timeline.Period);
            Assert.Equal(45, timeline.MatchTime);
            Assert.Equal("45:00", timeline.MatchClock);
        }

        #endregion

        #region AddCommentaries

        [Fact]
        public async Task AddCommentaries_Empty_ShouldNotInsertRange()
        {
            var timelines = new List<Timeline> { new Timeline { Type = EventTypes.MatchStarted.Value } };

            var result = await matchDataAccess.AddCommentaries("sr:match:1", timelines, "en-US");

            await dynamicRepository.DidNotReceive().InsertRange(Arg.Any<IEnumerable<CommentaryEntity>>());
            Assert.False(result);
        }

        [Fact]
        public async Task AddCommentaries_HasCommentary_ShouldInsertRangeAssignedValue()
        {
            var timelines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.MatchStarted.Value, Commentaries = new List<Commentary> { new Commentary { Text = "match started" } } }
            };

            dynamicRepository.Save().Returns(1);

            var result = await matchDataAccess.AddCommentaries("sr:match:1", timelines, "en-US");

            await dynamicRepository
                .Received(1)
                .InsertRange(Arg.Is<IEnumerable<CommentaryEntity>>(x => x.First().MatchId.Equals("sr:match:1")));

            Assert.True(result);
        }

        #endregion

        #region GetTimelines

        [Fact]
        public async Task GetTimelines_Empty_ReturnEmpty()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchTimelineEntity, bool>>()).Returns(new List<MatchTimelineEntity> { });

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en-US");

            Assert.Empty(timelines);
        }

        [Fact]
        public async Task GetTimelines_HasScoreChange_ReturnGoalScorer()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchTimelineEntity, bool>>()).Returns(new List<MatchTimelineEntity>
            {
                StubTimeline.StubScoreChangeTimelineEntity("sr:match:1")
            });

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en-US");

            Assert.NotEmpty(timelines);

            var timeline = timelines.First();
            Assert.Equal(EventTypes.ScoreChange.Value, timeline.Type);
            Assert.NotNull(timeline.GoalScorer);
        }

        [Fact]
        public async Task GetTimelines_PenaltyScoreChange_ReturnMethod()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchTimelineEntity, bool>>()).Returns(new List<MatchTimelineEntity>
            {
                StubTimeline.StubScoreChangePenaltyEntity("sr:match:1")
            });

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en-US");

            Assert.NotEmpty(timelines);

            var timeline = timelines.First();
            Assert.Equal(EventTypes.ScoreChange.Value, timeline.Type);
            Assert.NotNull(timeline.GoalScorer);
            Assert.Equal("penalty", timeline.GoalScorer.Method);
        }

        [Fact]
        public async Task GetTimelines_HasValues_ReturnOnlyBasicEvents()
        {
            dynamicRepository.Query(Arg.Any<Func<MatchTimelineEntity, bool>>()).Returns(new List<MatchTimelineEntity>
            {
                StubTimeline.StubScoreChangePenaltyEntity("sr:match:1"),
                StubTimeline.StubSubstitutionEntity("sr:match:1")
            });

            var timelines = await matchDataAccess.GetTimelines(
                1,
                "eu",
                "sr:match:1",
                "en-US");

            Assert.NotEmpty(timelines);
            Assert.Single(timelines);

            var timeline = timelines.First();
            Assert.Equal(EventTypes.ScoreChange.Value, timeline.Type);
        }

        #endregion

        #region AddTimelines

        [Fact]
        public async Task AddTimelines_NotDeleteExisting_ShouldNotDeleteEntity()
        {
            var timelines = new List<Timeline>
            {
                StubTimeline.StubScoreChangePenalty()
            };

            await matchDataAccess.AddTimelines("sr:match:1", timelines, isDeleteExisting: false);

            await dynamicRepository.DidNotReceive().Delete<MatchTimelineEntity>(Arg.Any<object[]>());
            await dynamicRepository.Received(1).InsertRange(Arg.Any<IEnumerable<MatchTimelineEntity>>());
        }

        [Fact]
        public async Task AddTimelines_DeleteExisting_ShouldDeleteExistingEntity()
        {
            var timelines = new List<Timeline>
            {
                StubTimeline.StubScoreChangePenalty()
            };

            dynamicRepository.Query(Arg.Any<Func<MatchTimelineEntity, bool>>()).Returns(new List<MatchTimelineEntity>
            {
                StubTimeline.StubScoreChangeTimelineEntity("sr:match:1")
            });

            await matchDataAccess.AddTimelines("sr:match:1", timelines, isDeleteExisting: true);

            await dynamicRepository.Received(1).Delete<MatchTimelineEntity>(Arg.Any<object[]>());
            await dynamicRepository.Received(1).InsertRange(Arg.Any<IEnumerable<MatchTimelineEntity>>());
        }

        #endregion

        #region GetMatch

        [Fact]
        public async Task GetMatch_NotExist_ReturnNull()
        {
            var match = await matchDataAccess.GetMatch(1, "sr:match:1", "en-US");

            Assert.Null(match);
        }

        [Fact]
        public async Task GetMatch_HasValue_ReturnAssigned()
        {
            dynamicRepository.GetById<MatchEntity>("sr:match:17809194", "en-US").Returns(StubMatch.StubMatchEntity());

            var match = await matchDataAccess.GetMatch(1, "sr:match:17809194", "en-US");

            Assert.NotNull(match);
            Assert.Equal("sr:tournament:28424", match.League.Id);
        }

        #endregion

        #region GetLiveMatches

        [Fact]
        public async Task GetLiveMatches_Always_ReturnTeams()
        {
            var matches = await matchDataAccess.GetLiveMatches(
                1,
                "en-US");

            var match = matches.First();
            var home = match.Teams.ElementAt(0);
            var away = match.Teams.ElementAt(1);

            Assert.Equal(2, match.Teams.Count());

            Assert.Equal("sr:competitor:559146", home.Id);
            Assert.Equal("Denton Diablos FC", home.Name);
            Assert.Equal("USA", home.Country);
            Assert.Equal("USA", home.CountryCode);
            Assert.Equal("DEN", home.Abbreviation);
            Assert.True(home.IsHome);

            Assert.Equal("sr:competitor:559156", away.Id);
            Assert.Equal("Dallas City FC", away.Name);
            Assert.Equal("USA", away.Country);
            Assert.Equal("USA", away.CountryCode);
            Assert.Equal("DAL", away.Abbreviation);
            Assert.False(away.IsHome);
        }

        #endregion

        #region DeleteMatch



        #endregion
    }
}
