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

        Task<bool> InsertOdds(MatchOdds matchOdds);
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
            var oddsEntities = GetOdds(matchId, betTypeId)
                .GroupBy(o => new { o.MatchId, o.BookmakerId });

            var betTypeOdssList = oddsEntities
                .Select(group => group.First())
                .Select(m => JsonConvert.DeserializeObject<BetTypeOdds>(m.Value))
                .OrderBy(bto => bto.Bookmaker.Name)
                .AsEnumerable();

            return Task.FromResult(betTypeOdssList);
        }

        private IOrderedEnumerable<OddsEntity> GetOdds(string matchId, int betTypeId)
            => dynamicRepository
                            .Query<OddsEntity>(
                                o => o.MatchId == matchId
                                    && o.BetTypeId == betTypeId
                            )
                            .OrderByDescending(o => o.ModifiedTime);

        public async Task<bool> InsertOdds(MatchOdds matchOdds)
        {
            var insertOddsList = new List<BetTypeOdds>();

            foreach (var betTypeOdds in matchOdds.BetTypeOddsList)
            {
                var currentOdds = GetOdds(matchOdds.MatchId, betTypeOdds.Id).FirstOrDefault();

                if (currentOdds == null || currentOdds.ModifiedTime < matchOdds.LastUpdated)
                {
                    insertOddsList.Add(betTypeOdds);
                }
            }

            if (insertOddsList.Any())
            {
                return await InsertOddsByBetType(matchOdds.MatchId, insertOddsList, matchOdds.LastUpdated);
            }

            return true;
        }

        private async Task<bool> InsertOddsByBetType(string matchId, IEnumerable<BetTypeOdds> BetTypeOddsList, DateTime? lastUpdate)
            =>  await dynamicRepository.InsertRange(BetTypeOddsList.Select(betTypeOdds
                => new OddsEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    BetTypeId = betTypeOdds.Id,
                    BookmakerId = betTypeOdds.Bookmaker.Id,
                    MatchId = matchId,
                    ModifiedTime = lastUpdate.HasValue
                        ? lastUpdate.Value
                        : DateTimeOffset.Now,
                    Value = JsonConvert.SerializeObject(betTypeOdds),
                    CreatedTime = DateTimeOffset.Now
                }));
    }
}