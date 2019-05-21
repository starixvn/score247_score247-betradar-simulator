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

            using (dynamicRepository)
            {
                var matchEntities = await dynamicRepository.Find<MatchEntity>
                (
                    x => x.SportId == sportId &&
                    x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                    x.EventDate >= fromUtc.Date &&
                    x.EventDate <= toUtc.Date
                );

                foreach (var entity in matchEntities)
                {
                    var match = JsonConvert.DeserializeObject<Match>(entity.Value);

                    var lastTimeLine = await dynamicRepository.Last<MatchTimelineEntity>(t => t.MatchId == entity.Id);
                    var result = await dynamicRepository.Last<MatchResultEntity>(t => t.MatchId == entity.Id);

                    if (lastTimeLine != null)
                    {
                        match.TimeLines = new List<Timeline> { JsonConvert.DeserializeObject<Timeline>(lastTimeLine.Value) };
                    }

                    if (result != null)
                    {
                        match.MatchResult = JsonConvert.DeserializeObject<MatchResult>(result.Value);
                    }

                    matches.Add(match);
                }

                return matches;
            }
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            return (await GetMatches(sportId, DateTime.Today, DateTime.Today, language))
                .Where(m => m.MatchResult.EventStatus.IsLive());
        }

        public async Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines)
        {
            var matchTimelineEntities = timelines.Select(t => new MatchTimelineEntity
            {
                MatchId = matchId,
                Value = JsonConvert.SerializeObject(t)
            });

            await dynamicRepository.InsertRange(matchTimelineEntities);

            return (await dynamicRepository.Save()) > 0;
        }

        public async Task<bool> AddResult(string matchId, IMatchResult matchResult)
        {
            await dynamicRepository.Insert(
                new MatchResultEntity
                {
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(matchResult)
                });

            return (await dynamicRepository.Save()) > 0;
        }
    }
}
