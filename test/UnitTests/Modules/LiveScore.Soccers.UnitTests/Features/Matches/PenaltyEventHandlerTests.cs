namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using NSubstitute;
    using Xunit;

    [Trait("LiveScore.Soccer", "PenaltyEventHandler")]
    public class PenaltyEventHandlerTests
    {
        private readonly PenaltyEventHandler penaltyEventHandler;
        private readonly ICacheService cacheService;

        public PenaltyEventHandlerTests()
        {
            cacheService = Substitute.For<ICacheService>();
            penaltyEventHandler = new PenaltyEventHandler(cacheService);
        }

        [Fact]
        public async Task Handle_ShootoutEventIsNull_ReturnDefaultTimeline()
        {
            var timeline = await penaltyEventHandler.Handle("matchId", null);

            Assert.Null(timeline);
        }

        [Fact]
        public async Task Handle_ShootoutEventIsNotTaken_ReturnDefaultTimeline()
        {
            var shootoutEvent = new Timeline
            {
                PenaltyStatus = "not_taken_yet"
            };

            var timeline = await penaltyEventHandler.Handle("matchId", shootoutEvent);

            Assert.Null(timeline);
        }

        [Fact]
        public async Task Handle_SetFirstShootForFirstEvent_ReturnFirstShootEvent()
        {
            var shootoutEvent = StubTimeline(true, true, "playerName");

            var timeline = await penaltyEventHandler.Handle("firstShootEvent", shootoutEvent);

            Assert.True(timeline.IsFirstShoot);
        }

        [Fact]
        public async Task Handle_SetHomePlayer_ReturnShootEventWithHomePlayer()
        {
            var isHome = true;
            var shootoutEvent = StubTimeline(isHome, true, "homePlayerName");

            var timeline = await penaltyEventHandler.Handle("firstShootEvent", shootoutEvent);

            Assert.Equal("homePlayerName", timeline.HomeShootoutPlayer.Name);
        }

        [Fact]
        public async Task Handle_SetAwayPlayer_ReturnShootEventWithAwayPlayer()
        {
            var isHome = false;
            var shootoutEvent = StubTimeline(isHome, true, "awayPlayerName");

            var timeline = await penaltyEventHandler.Handle("firstShootEvent", shootoutEvent);

            Assert.Equal("awayPlayerName", timeline.AwayShootoutPlayer.Name);
        }

        [Fact]
        public async Task Handle_CountTotalScore_ReturnShootoutEventWithTotalScore()
        {
            var matchId = "CountTotalScore";
            var shootoutEvent = StubTimeline(true, true);
            cacheService.GetAsync<IList<Timeline>>($"Penalty_Match_{matchId}").Returns(
                Task.FromResult(new List<Timeline>
                {
                    StubTimeline(true, true),
                    StubTimeline(false, true),
                    StubTimeline(true, false),
                    StubTimeline(false, false)
                } as IList<Timeline>));

            var timeline = await penaltyEventHandler.Handle(matchId, shootoutEvent);

            Assert.Equal(2, timeline.ShootoutHomeScore);
            Assert.Equal(1, timeline.ShootoutAwayScore);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task Handle_Always_SetCacheForNewShootoutEvent()
#pragma warning restore S2699 // Tests should include assertions
        {
            var matchId = "SetCacheForNewShootoutEvent";
            var cacheKeyEvent = "Penalty_Match_SetCacheForNewShootoutEvent";
            var shootoutEvent = StubTimeline(true, true, "ricky");
            cacheService.GetAsync<IList<Timeline>>(cacheKeyEvent).Returns(Task.FromResult(new List<Timeline>() as IList<Timeline>));

            await penaltyEventHandler.Handle(matchId, shootoutEvent);

            await cacheService.Received().SetAsync(
                "Penalty_Match_SetCacheForNewShootoutEvent",
                Arg.Is<IList<Timeline>>(
                    timelines => timelines.Count == 1
                        && timelines.First().HomeShootoutPlayer.Name == "ricky"
                        && timelines.First().IsHomeShootoutScored),
                Arg.Any<CacheItemOptions>());
        }

        [Fact]
        public async Task Handle_LastEventExistsInCache_ReturnCombineShootEventAndRemoveCache()
        {
            var matchId = "CombineShootEvent";
            var latestEventCacheKey = $"Penalty_Match_{matchId}_Latest_Event";
            var shootoutEvent = StubTimeline(false, false, "awayPlayerName");
            var latestShootoutEvent = StubTimeline(true, true, "homePlayerName");
            latestShootoutEvent.HomeShootoutPlayer = latestShootoutEvent.Player;
            cacheService.GetAsync<Timeline>(latestEventCacheKey)
                .Returns(Task.FromResult(latestShootoutEvent));

            var timeline = await penaltyEventHandler.Handle(matchId, shootoutEvent);

            Assert.Equal("awayPlayerName", timeline.AwayShootoutPlayer.Name);
            Assert.False(timeline.IsAwayShootoutScored);

            Assert.Equal("homePlayerName", timeline.HomeShootoutPlayer.Name);
            Assert.True(timeline.IsHomeShootoutScored);

            await cacheService.Received().RemoveAsync(latestEventCacheKey);
        }

        private Timeline StubTimeline(bool isHome, bool isScored, string playerName = "playerName")
            => new Timeline
            {
                Team = isHome ? "home" : "away",
                IsHomeShootoutScored = isHome && isScored,
                IsAwayShootoutScored = !isHome && isScored,
                Player = new Player
                {
                    Id = "player_id",
                    Name = playerName
                }
            };
    }
}
