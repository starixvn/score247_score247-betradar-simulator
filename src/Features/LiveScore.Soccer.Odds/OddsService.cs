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

    public interface OddsService
    {
        Task<MatchOdds> GetOdds(string matchId, int betTypeId);

        Task<bool> InsertOdds(MatchOdds matchOdds);
    }

    public class OddsServiceImpl : OddsService
    {
        private readonly IDynamicRepository dynamicRepository;

        public OddsServiceImpl(
            IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<MatchOdds> GetOdds(string matchId, int betTypeId)
            => new MatchOdds
            {
                MatchId = matchId,
                BetTypeOddsList = await GetBetTypeOddsList(matchId, betTypeId)
            };

        private Task<IEnumerable<BetTypeOdds>> GetBetTypeOddsList(string matchId, int betTypeId)
        {
            var oddsEntities = GetOddsData(matchId, betTypeId)
                .Select(m => JsonConvert.DeserializeObject<BetTypeOdds>(m.Value))
                .GroupBy(o => o.Bookmaker.Id);

            var betTypeOdssList = oddsEntities
                .Select(group => {
                    var first = group.First();
                    var last = group.Last();

                    first.AssignOpeningData(last.BetOptions);

                    return first;
                })
                .OrderBy(bto => bto.Bookmaker.Name)
                .AsEnumerable();

            return Task.FromResult(betTypeOdssList);
        }

        private IList<OddsEntity> GetOddsData(string matchId, int betTypeId)
            => dynamicRepository
                            .Query<OddsEntity>(
                                o => o.MatchId == matchId
                                    && o.BetTypeId == betTypeId
                            )
                            .OrderByDescending(o => o.ModifiedTime)
                            .ToList();

        public async Task<bool> InsertOdds(MatchOdds matchOdds)
        {
            var insertOddsList = new List<BetTypeOdds>();

            foreach (var betTypeOdds in matchOdds.BetTypeOddsList)
            {
                var lastOddsList = GetOddsData(matchOdds.MatchId, betTypeOdds.Id);
                var lastBookMakerOdds = lastOddsList.FirstOrDefault(odds => odds.BookmakerId == betTypeOdds.Bookmaker.Id);

                if (lastBookMakerOdds == null
                    || (lastBookMakerOdds.ModifiedTime < matchOdds.LastUpdated
                        && !JsonConvert.DeserializeObject<BetTypeOdds>(lastBookMakerOdds.Value).Equals(betTypeOdds)))
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

        private async Task<bool> InsertOddsByBetType(
            string matchId, 
            IEnumerable<BetTypeOdds> BetTypeOddsList, 
            DateTime? lastUpdate)
            => await dynamicRepository.InsertRange(BetTypeOddsList.Select(betTypeOdds
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