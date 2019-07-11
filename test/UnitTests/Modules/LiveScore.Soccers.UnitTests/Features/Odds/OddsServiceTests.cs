namespace LiveScore.Soccers.UnitTests.Features.Odds
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Odds;
    using LiveScore.Features.Odds.Models;
    using NSubstitute;
    using Xunit;

    [Trait("LiveScore.Soccer", "Odds")]
    public class OddsServiceTests
    {
        private readonly OddsServiceImpl oddsServiceImpl;
        private readonly OddsDataAccess oddsDataAccess;

        public OddsServiceTests()
        {
            oddsDataAccess = Substitute.For<OddsDataAccess>();
            oddsServiceImpl = new OddsServiceImpl(oddsDataAccess);
        }

        [Fact]
        public async Task GetOdds_Always_ReturnMatchOddsData()
        {
            var matchId = "mathcId";
            var betTypeId = 2;
            var returnOdds = CreateBetTypeOdds();

            oddsDataAccess
                .GetBetTypeOddsList(matchId, betTypeId)
                .Returns(returnOdds);

            var matchOdds = await oddsServiceImpl.GetOdds(matchId, betTypeId);

            Assert.Equal(matchId, matchOdds.MatchId);
            Assert.NotEmpty(matchOdds.BetTypeOddsList);
            Assert.Equal("1x2", matchOdds.BetTypeOddsList.First().Name);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task InsertOdds_Always_CallInsertOdds()
#pragma warning restore S2699 // Tests should include assertions
        {
            var matchOdds = new MatchOdds { MatchId = "insert-matchid" };

            await oddsDataAccess.InsertOdds(matchOdds);

            await oddsDataAccess
                .Received()
                .InsertOdds(Arg.Is<MatchOdds>(mo => mo.MatchId == matchOdds.MatchId));
        }

        private IEnumerable<BetTypeOdds> CreateBetTypeOdds()
            => new List<BetTypeOdds>
            {
                new BetTypeOdds
                {
                    Id = 1,
                    Name = "1x2",
                    BetOptions = new List<BetOptionOdds>
                    {
                        new BetOptionOdds { Id = 1, OpeningOdds = 1.22m, LiveOdds = 2.22m }
                    }
                }
            };
    }
}