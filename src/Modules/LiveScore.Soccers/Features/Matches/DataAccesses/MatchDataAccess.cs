namespace LiveScore.Soccers.Features.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using Newtonsoft.Json;

    public class MatchDataAccess : IMatchDataAccess
    {
        private readonly IDynamicRepository dynamicRepository;

        public MatchDataAccess(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language)
        {
            var matches = new List<Match>();
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var matchEntities = await dynamicRepository.Find<MatchEntity>
                (
                    x => x.SportId == sportId &&
                    x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                    x.EventDate >= fromUtc.Date &&
                    x.EventDate <= toUtc.Date
                );

            foreach (var entity in matchEntities)
            {
                matches.Add(JsonConvert.DeserializeObject<Match>(entity.Value));
            }

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            return (await GetMatches(sportId, DateTime.Today, DateTime.Today, language))
                .Where(m => m.MatchResult.EventStatus.IsLive());
        }

        public async Task AddTimeline(string matchId, ITimeline timeline)
        {
            await dynamicRepository.Insert(
                new MatchTimelineEntity
                {
                    Id = timeline.Id,
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(timeline)
                });
        }

        public async Task AddResult(string matchId, IMatchResult matchResult)
        {
            await dynamicRepository.Insert(
                new MatchResultEntity
                {
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(matchResult)
                });
        }
    }
}
