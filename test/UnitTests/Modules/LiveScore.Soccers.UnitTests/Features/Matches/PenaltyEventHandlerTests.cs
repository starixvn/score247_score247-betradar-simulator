namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;
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
            
        }

        private Timeline StubTimeline()
            => new Timeline
            {

            };
    }
}
