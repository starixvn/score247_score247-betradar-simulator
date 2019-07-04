//namespace LiveScore.UnitTests.Features.Leagues
//{
//    using LiveScore.Features.Leagues;
//    using LiveScore.Features.Leagues.Models;
//    using LiveScore.Soccer;
//    using NSubstitute;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using Xunit;

//    public class LeagueServiceTests
//    {
//        private const int SoccerSportId = 1;
//        private readonly LeagueServiceImpl leagueServiceImpl;
//        private readonly InstanceFactory instanceFactory;
//        private readonly LeagueDataAccess leagueDataAccess;

//        public LeagueServiceTests()
//        {
//            leagueDataAccess = Substitute.For<LeagueDataAccess>();
//            instanceFactory = Substitute.For<InstanceFactory>();
//            instanceFactory.CreateLeagueDataAccess(SoccerSportId).Returns(leagueDataAccess);

//            leagueServiceImpl = new LeagueServiceImpl(instanceFactory);
//        }

//        [Theory]
//        [InlineData(0)]
//        [InlineData(-1)]
//        public async Task GetLeagues_SportIdIsZeroOrNegative_ReturnEmptyLeagues(int sportId)
//        {
//            var matches = await leagueServiceImpl.GetLeagues(sportId, "en");

//            Assert.Empty(matches);
//        }

//        [Fact]
//        public async Task GetLeagues_Always_ReturnAssignableLeagues()
//        {
//            var leagues = new List<League>
//            {
//                new League { Name="Uefa Nations League", Id = "sr:tournament:23755" }
//            };

//            leagueDataAccess
//                .GetLeagues(1, "en")
//                .Returns(Task.FromResult(leagues as IEnumerable<ILeague>));

//            var actualLeagues = (await leagueServiceImpl.GetLeagues(
//                1,               
//                "en")).ToList();

//            Assert.True(actualLeagues.Any());
//            Assert.Equal("Uefa Nations League", actualLeagues.First().Name);
//            Assert.Equal("sr:tournament:23755", actualLeagues.First().Id);
//        }
//    }
//}
