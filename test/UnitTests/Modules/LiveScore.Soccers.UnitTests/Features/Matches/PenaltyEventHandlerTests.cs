namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Fanex.Caching;
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


    }
}
