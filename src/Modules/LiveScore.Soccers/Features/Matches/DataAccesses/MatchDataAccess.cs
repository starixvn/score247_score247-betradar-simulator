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
    using LiveScore.Shared.Helpers;
    using Newtonsoft.Json;

    public class MatchDataAccess : IMatchDataAccess
    {
        private readonly IDynamicRepository dynamicRepository;

        public MatchDataAccess(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language, TimeSpan timeZone)
        {
            var matches = new List<Match>();

            var formattedFromUtcTime = from.ConvertToUtc(timeZone);
            var formattedToUtcTime = to.ConvertToUtc(timeZone);

            var matchEntities = await dynamicRepository.Query<MatchEntity>
            (
                x => x.SportId == sportId &&
                x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                x.EventDate.ToUniversalTime() >= formattedFromUtcTime &&
                x.EventDate.ToUniversalTime() <= formattedToUtcTime
            );

            foreach (var entity in matchEntities)
            {
                var match = await BuildMatch(entity);

                matches.Add(match);
            }

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matches = new List<Match>();
            var matchEntities = await dynamicRepository.GetAll<MatchEntity>();

            foreach (var entity in matchEntities)
            {
                var match = await BuildMatch(entity);

                if (match.MatchResult.EventStatus.IsLive())
                {
                    matches.Add(match);
                }
            }

            return matches;
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

        private async Task<Match> BuildMatch(MatchEntity entity)
        {
            var match = JsonConvert.DeserializeObject<Match>(entity.Value);

            var lastTimeLine = await dynamicRepository.Last<MatchTimelineEntity>(t => t.MatchId == entity.Id);

            // TODO Enhance query later
            var result = (await dynamicRepository.Query<MatchResultEntity>(t => t.MatchId == entity.Id))
                    .OrderBy(r => r.Id).LastOrDefault();

            if (lastTimeLine != null)
            {
                match.TimeLines = new List<Timeline> { JsonConvert.DeserializeObject<Timeline>(lastTimeLine.Value) };
            }

            if (result != null)
            {
                match.MatchResult = JsonConvert.DeserializeObject<MatchResult>(result.Value);
            }

            return match;
        }

    }
}
