namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Soccers.Features.Matches;
    using NSubstitute;

    public class PenaltyEventHandlerTests
    {
        private readonly PenaltyEventHandler penaltyEventHandler;
        private readonly ICacheService cacheService;

        public PenaltyEventHandlerTests()
        {
            cacheService = Substitute.For<ICacheService>();
            penaltyEventHandler = new PenaltyEventHandler(cacheService);
        }

        public async Task Handle_NoPenaltyShootOutBefore_ReturnDefaultTimeline()
        {
            var matchId = "matchId";
            var matchEventsCacheKey = $"Penalty_Match_matchId";
            var latestEventCacheKey = $"Penalty_Match_matchId_Latest_Event";

        }

        private Timeline StubTimeline(bool isHome, bool isScored, string playerName)
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
