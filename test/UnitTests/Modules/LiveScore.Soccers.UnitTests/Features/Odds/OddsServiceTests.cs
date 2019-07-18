namespace LiveScore.Soccers.UnitTests.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Database.Entities;
    using LiveScore.Features.Odds;
    using LiveScore.Soccer.Odds.Models;
    using LiveScore.Soccer.Database;
    using NSubstitute;
    using Xunit;
    using LiveScore.Soccer.Matches;

    [Trait("LiveScore.Soccer", "Odds")]
    public class OddsServiceTests
    {
        private readonly OddsServiceImpl oddsServiceImpl;
        private readonly IDynamicRepository dynamicRepository;
        private readonly IMatchService matchService;
        

        public OddsServiceTests()
        {
            dynamicRepository = Substitute.For<IDynamicRepository>();
            matchService = Substitute.For<IMatchService>();
            oddsServiceImpl = new OddsServiceImpl(dynamicRepository, matchService);
        }

        [Fact]
        public async Task GetOdds_Always_ReturnMatchId()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var matchId = "mathcId";
            var betTypeId = 2;

            var matchOdds = await oddsServiceImpl.GetOdds(matchId, betTypeId, string.Empty);

            Assert.Equal(matchId, matchOdds.MatchId);
        }

        [Fact]
        public async Task GetOdds_Always_ReturnBetTypeOddsList()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());

            var actualBetTypeOddsList = await oddsServiceImpl.GetOdds("matchId", 1, string.Empty);

            var firstBetTypeOdds = actualBetTypeOddsList.BetTypeOddsList.First();
            var secondBetTypeOdds = actualBetTypeOddsList.BetTypeOddsList.ElementAt(1);

            Assert.Equal(2, actualBetTypeOddsList.BetTypeOddsList.Count());
            //Order by Bookmaker Name
            Assert.Equal("sr:book:201", firstBetTypeOdds.Bookmaker.Id);
            Assert.Equal("sr:book:202", secondBetTypeOdds.Bookmaker.Id);
            //Get last odds for each bookmaker
            Assert.Equal(1.75m, firstBetTypeOdds.BetOptions.ElementAt(0).LiveOdds);
            Assert.Equal(3.75m, secondBetTypeOdds.BetOptions.ElementAt(0).LiveOdds);
        }

        [Fact]
        public async Task GetBetTypeOddsList_IsNotNewOdds_DontCallInsertRange()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var matchOdds = StubMatchOdds();

            await oddsServiceImpl.InsertOdds(matchOdds);

            await dynamicRepository.DidNotReceive().InsertRange(Arg.Any<IEnumerable<OddsEntity>>());
        }

        [Fact]
        public async Task GetBetTypeOddsList_IsNewOdds_CallInsertRange()
        {
            var lastUpdatedTime = new DateTimeOffset(2019, 4, 2, 0, 0, 0, new TimeSpan(7, 0, 0)).DateTime;
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var matchOdds = StubMatchOdds(lastUpdatedTime);

            await oddsServiceImpl.InsertOdds(matchOdds);

            await dynamicRepository.Received().InsertRange(
                Arg.Is<IEnumerable<OddsEntity>>(
                    entities => entities.Count() == 2
                                && entities.First().BookmakerId == "sr:book:201"
                                && entities.First().MatchId == "matchId1"
                                && entities.ElementAt(1).BookmakerId == "sr:book:202"
                ));
        }

        private static MatchOdds StubMatchOdds(DateTime? lastUpdatedTime = null)
            => new MatchOdds
            {
                MatchId = "matchId1",
                LastUpdated = lastUpdatedTime ?? new DateTimeOffset(2019, 1, 2, 0, 0, 0, new TimeSpan(7, 0, 0)).DateTime,
                BetTypeOddsList = new List<BetTypeOdds>
                {
                    new BetTypeOdds
                    {
                        Id = 1,
                        Name = "1x2",
                        Bookmaker = new Bookmaker { Id = "sr:book:201", Name = "bookmakername 1" },
                        BetOptions = new List<BetOptionOdds>
                        {
                            new BetOptionOdds { LiveOdds = 1.2m, OpeningOdds = 1.5m, Type = "Home" },
                            new BetOptionOdds { LiveOdds = 1.3m, OpeningOdds = 1.4m, Type = "Away" },
                            new BetOptionOdds { LiveOdds = 1.1m, OpeningOdds = 1.6m, Type = "Draw" },
                        }
                    },
                    new BetTypeOdds
                    {
                        Id = 1,
                        Name = "1x2",
                        Bookmaker = new Bookmaker { Id = "sr:book:202", Name = "bookmakername 2" },
                        BetOptions = new List<BetOptionOdds>
                        {
                            new BetOptionOdds { LiveOdds = 2.2m, OpeningOdds = 2.5m, Type = "Home" },
                            new BetOptionOdds { LiveOdds = 2.3m, OpeningOdds = 2.4m, Type = "Away" },
                            new BetOptionOdds { LiveOdds = 2.1m, OpeningOdds = 2.6m, Type = "Draw" },
                        }
                    }
                }
            };

        private static List<OddsEntity> StubBetTypeOddsEntities()
            => new List<OddsEntity>
            {
                new OddsEntity
                {
                    Value = "{\"Bookmaker\":{\"Id\":\"sr:book:201\",\"Name\":\"Bet200\"},"
                        + "\"BetOptions\":[{\"Type\":\"home\",\"LiveOdds\":3.75,\"OpeningOdds\":3.95,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Down\",\"Value\":\"down\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":3,\"Name\":\"1\"},"
                        + "{\"Type\":\"away\",\"LiveOdds\":2.40,\"OpeningOdds\":1.57,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Up\",\"Value\":\"up\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":5,\"Name\":\"2\"},"
                        + "{\"Type\":\"draw\",\"LiveOdds\":4.90,\"OpeningOdds\":4.90,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Neutral\",\"Value\":\"neutral\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":4,\"Name\":\"x\"}],\"Id\":1,\"Name\":\"1x2\"}",
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:201",
                    ModifiedTime = new DateTimeOffset(2019, 1, 2, 0, 0, 0, new TimeSpan(7, 0 ,0))
                },
                new OddsEntity
                {
                    Value = "{\"Bookmaker\":{\"Id\":\"sr:book:201\",\"Name\":\"Bet201\"},"
                        + "\"BetOptions\":[{\"Type\":\"home\",\"LiveOdds\":1.75,\"OpeningOdds\":1.95,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Down\",\"Value\":\"down\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":3,\"Name\":\"1\"},"
                        + "{\"Type\":\"away\",\"LiveOdds\":1.40,\"OpeningOdds\":1.57,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Up\",\"Value\":\"up\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":5,\"Name\":\"2\"},"
                        + "{\"Type\":\"draw\",\"LiveOdds\":1.90,\"OpeningOdds\":1.90,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Neutral\",\"Value\":\"neutral\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":4,\"Name\":\"x\"}],\"Id\":1,\"Name\":\"1x2\"}",
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:201",
                    ModifiedTime = new DateTimeOffset(2019, 1, 3, 0, 0, 0, new TimeSpan(7, 0 ,0))
                },
                new OddsEntity
                {
                     Value = "{\"Bookmaker\":{\"Id\":\"sr:book:202\",\"Name\":\"Bet202\"},"
                        + "\"BetOptions\":[{\"Type\":\"home\",\"LiveOdds\":3.75,\"OpeningOdds\":3.95,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Down\",\"Value\":\"down\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":3,\"Name\":\"1\"},"
                        + "{\"Type\":\"away\",\"LiveOdds\":2.40,\"OpeningOdds\":1.57,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Up\",\"Value\":\"up\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":5,\"Name\":\"2\"},"
                        + "{\"Type\":\"draw\",\"LiveOdds\":4.90,\"OpeningOdds\":4.90,\"OptionValue\":null,\"OddsTrend\":{\"DisplayName\":\"Neutral\",\"Value\":\"neutral\"},\"LastUpdatedTime\":\"0001-01-01T00:00:00\",\"MatchTime\":null,\"HomeScore\":0,\"AwayScore\":0,\"Id\":4,\"Name\":\"x\"}],\"Id\":1,\"Name\":\"1x2\"}",
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:202",
                    ModifiedTime = new DateTimeOffset(2019, 2, 2, 0, 0, 0, new TimeSpan(6, 0 ,0))
                }
            };
    }
}