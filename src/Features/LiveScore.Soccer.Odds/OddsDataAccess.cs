namespace LiveScore.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Database.Entities;
    using LiveScore.Features.Odds.Models;
    using LiveScore.Soccer.Database;
    using Newtonsoft.Json;

    public interface OddsDataAccess
    {
        Task<IEnumerable<BetTypeOdds>> GetBetTypeOddsList(string matchId, int betTypeId);

        Task<bool> InsertBetTypeOdds(string matchId, IEnumerable<BetTypeOdds> betTypeOddsList, DateTimeOffset? lastUpdateTime = null);
    }

    public class OddsDataAccessImpl : OddsDataAccess
    {
        private readonly IDynamicRepository dynamicRepository;

        public OddsDataAccessImpl(
            IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public Task<IEnumerable<BetTypeOdds>> GetBetTypeOddsList(string matchId, int betTypeId)
        {
            var oddsEntities = dynamicRepository
                .Query<OddsEntity>(
                    o => o.MatchId == matchId
                        && o.BetTypeId == betTypeId
                )
                .OrderByDescending(o => o.ModifiedTime)
                .Select(m => JsonConvert.DeserializeObject<BetTypeOdds>(m.Value));

            return Task.FromResult(oddsEntities);
        }

        public async Task<bool> InsertBetTypeOdds(string matchId, IEnumerable<BetTypeOdds> betTypeOddsList, DateTimeOffset? lastUpdateTime = null)
        {
            return await dynamicRepository.InsertRange(betTypeOddsList.Select(betTypeOdds
                => new OddsEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    BetTypeId = betTypeOdds.Id,
                    BookmakerId = betTypeOdds.Bookmaker.Id,
                    MatchId = matchId,
                    ModifiedTime = lastUpdateTime.HasValue ? lastUpdateTime.Value : DateTimeOffset.Now,
                    Value = JsonConvert.SerializeObject(betTypeOdds),
                    CreatedTime = DateTimeOffset.Now
                }));
        }
    }
}