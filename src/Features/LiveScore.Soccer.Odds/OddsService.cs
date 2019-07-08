namespace LiveScore.Features.Odds
{
    using System.Threading.Tasks;
    using LiveScore.Features.Odds.Models;

    public interface OddsService
    {
        Task<MatchOdds> GetOdds(string matchId, int betTypeId);

        Task<bool> InsertOdds(MatchOdds matchOdds);
    }

    public class OddsServiceImpl : OddsService
    {
        private readonly OddsDataAccess oddsDataAccess;

        public OddsServiceImpl(OddsDataAccess oddsDataAccess)
        {
            this.oddsDataAccess = oddsDataAccess;
        }

        public async Task<MatchOdds> GetOdds(string matchId, int betTypeId)
            => new MatchOdds
            {
                MatchId = matchId,
                BetTypeOddsList = await oddsDataAccess.GetBetTypeOddsList(matchId, betTypeId)
            };

        public Task<bool> InsertOdds(MatchOdds matchOdds)
            => oddsDataAccess.InsertOdds(matchOdds);
    }
}