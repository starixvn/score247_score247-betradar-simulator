namespace LiveScore.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Odds.Models;

    public interface OddsService
    {
        Task<MatchOdds> GetOdds(string matchId, int betTypeId);

        Task<bool> InsertBetTypeOdds(string matchId, IEnumerable<BetTypeOdds> betTypeOddsList, DateTimeOffset? lastUpdateTime = null);
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

        public Task<bool> InsertBetTypeOdds(string matchId, IEnumerable<BetTypeOdds> betTypeOddsList, DateTimeOffset? lastUpdateTime = null)
            => oddsDataAccess.InsertBetTypeOdds(matchId, betTypeOddsList, lastUpdateTime);
    }
}