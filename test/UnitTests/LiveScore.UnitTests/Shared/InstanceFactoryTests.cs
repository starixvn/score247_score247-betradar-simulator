namespace LiveScore.UnitTests.Shared
{
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;    
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class InstanceFactoryTests
    {
        private readonly MatchDataAccess matchDataAccess;        
        private readonly LeagueDataAccess leagueDataAccess;        

        private readonly InstanceFactory instanceFactory;

        public InstanceFactoryTests()
        {  
            matchDataAccess = Substitute.For<MatchDataAccess>();
            leagueDataAccess = Substitute.For<LeagueDataAccess>();

            var matches = new Dictionary<int, MatchDataAccess>
            {
                { SportConstants.SoccerId, matchDataAccess },
                { SportConstants.BasketballId, null }
            };

            var leagues = new Dictionary<int, LeagueDataAccess>
            {
                { SportConstants.SoccerId, leagueDataAccess },
                { SportConstants.BasketballId, null }
            };


            instanceFactory = new InstanceFactoryImpl(matches, leagues);
        }

        [Fact]
        public void CreateLeagueDataAccess_SportIdNotExist_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => instanceFactory.CreateLeagueDataAccess(3));
        }

        [Fact]
        public void CreateLeagueDataAccess_HasContainer_ReturnsDataAccess()
        {
           var actualDataAccess = instanceFactory.CreateLeagueDataAccess(SportConstants.SoccerId);

            Assert.NotNull(actualDataAccess);
        }

        [Fact]
        public void CreateMatchDataAccess_SportIdNotExist_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => instanceFactory.CreateMatchDataAccess(3));
        }
       
        [Fact]
        public void CreateMatchDataAccess_HasContainer_ReturnsDataAccess()
        {
            var actualDataAccess = instanceFactory.CreateMatchDataAccess(SportConstants.SoccerId);

            Assert.NotNull(actualDataAccess);
        }
    }
}
