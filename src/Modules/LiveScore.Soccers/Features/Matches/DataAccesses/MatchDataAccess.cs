namespace LiveScore.Soccers.Features.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Database;
    using LiveScore.Shared.Database.Entities;
    using Newtonsoft.Json;

    public class MatchDataAccess : IMatchDataAccess
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly ILogger logger;

        public MatchDataAccess(
            IDynamicRepository dynamicRepository,
            ILogger logger)
        {
            this.dynamicRepository = dynamicRepository;
            this.logger = logger;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language)
        {
            var matchEntities = await dynamicRepository.Query<MatchEntity>
            (
                x => x.SportId == sportId &&
                x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                x.EventDate.ToUniversalTime() >= utcFrom &&
                x.EventDate.ToUniversalTime() <= utcTo
            );

            return matchEntities.Select(m => JsonConvert.DeserializeObject<Match>(m.Value));
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matchEntities = await dynamicRepository.Query<LiveMatchEntity>(m =>
                    m.SportId == sportId &&
                    m.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

            return matchEntities.Select(m => JsonConvert.DeserializeObject<Match>(m.Value));
        }

        public async Task<IMatch> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false)
        {
            try
            {
                var matchEntity = await dynamicRepository.GetById<MatchEntity>(matchId, language);
                var match = JsonConvert.DeserializeObject<Match>(matchEntity.Value);
                match.MatchResult = pushEvent.MatchResult;
                match.TimeLines = pushEvent.TimeLines;
                match.LatestTimeline = pushEvent.TimeLines.OrderBy(x => x.Time).Last();
                matchEntity.Value = JsonConvert.SerializeObject(match);

                if (isLive)
                {
                    await AddOrUpdateLiveMatch(matchId, language, matchEntity, match);
                }
                else
                {
                    dynamicRepository.Update(matchEntity);

                    //TODO delete live match???
                }

                await dynamicRepository.Save();

                return match;
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"AddOrUpdateMatch {matchId} - {isLive}", ex);

                return null;
            }
        }

        private async Task AddOrUpdateLiveMatch(string matchId, string language, MatchEntity matchEntity, Match match)
        {
            var liveMatchEntity = await dynamicRepository.GetById<LiveMatchEntity>(matchId, language);

            if (liveMatchEntity != null)
            {
                liveMatchEntity.Value = JsonConvert.SerializeObject(match);
                liveMatchEntity.CreatedTime = DateTime.UtcNow;
                liveMatchEntity.ModifiedTime = DateTime.UtcNow;

                dynamicRepository.Update(liveMatchEntity);
            }
            else
            {
                liveMatchEntity = new LiveMatchEntity
                {
                    Id = matchEntity.Id,
                    SportId = matchEntity.SportId,
                    Language = matchEntity.Language,
                    Value = matchEntity.Value,
                    LeagueId = matchEntity.LeagueId,
                    EventDate = matchEntity.EventDate.ToUniversalTime(),
                    CreatedTime = DateTime.UtcNow,
                    ModifiedTime = DateTime.UtcNow
                };

                await dynamicRepository.Insert(liveMatchEntity);
            }
        }

        public async Task<bool> DeleteMatch(string matchId, string language, bool isLive = false)
        {
            try
            {
                if (isLive)
                {
                    return await dynamicRepository.Delete<LiveMatchEntity>(matchId, language) > 0;
                }

                return await dynamicRepository.Delete<MatchEntity>(matchId, language) > 0;
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"DeleteMatch {matchId}", ex);

                return false;
            }
        }

        public async Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines, bool isDeleteExisting = false)
        {
            if (isDeleteExisting)
            {
                await DeleteAllTimelines(matchId);
            }

            var matchTimelineEntities = timelines.Select(
                t => new MatchTimelineEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(t),
                    CreatedTime = DateTime.Now
                });

            await dynamicRepository.InsertRange(matchTimelineEntities);

            return (await dynamicRepository.Save()) > 0;
        }

        private async Task DeleteAllTimelines(string matchId)
        {
            var eixstingTimelines = (await dynamicRepository.Query<MatchTimelineEntity>(x => x.MatchId.Equals(matchId, StringComparison.OrdinalIgnoreCase))).Select(x => x.Id);

            if (eixstingTimelines != null && eixstingTimelines.Any())
            {
                foreach (var existing in eixstingTimelines)
                {
                    await dynamicRepository.Delete<MatchTimelineEntity>(existing);
                }
            }
        }

        public async Task<bool> AddResult(string matchId, IMatchResult matchResult)
        {
            await dynamicRepository.Insert(
                new MatchResultEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(matchResult)
                });

            return (await dynamicRepository.Save()) > 0;
        }

        public async Task<bool> UpdateLiveMatches(IEnumerable<IMatch> matches, string language)
        {
            var matchEntities = new List<LiveMatchEntity>();

            foreach (var match in matches)
            {
                var matchEntity = await dynamicRepository.GetById<LiveMatchEntity>(match.Id, language);
                matchEntity.Value = JsonConvert.SerializeObject(match);
                matchEntity.EventDate = match.EventDate.ToUniversalTime();
                matchEntity.ModifiedTime = DateTime.UtcNow;
                matchEntities.Add(matchEntity);
            }

            await dynamicRepository.UpdateRange(matchEntities);

            return true;
        }

        public async Task<IMatch> GetMatch(int sportId, string matchId, string language)
        {
            var matchEntity = await dynamicRepository.GetById<MatchEntity>(matchId, language);

            return matchEntity == null
                    ? null
                    : JsonConvert.DeserializeObject<Match>(matchEntity.Value);
        }

        public async Task<IEnumerable<ITimeline>> GetTimelines(int sportId, string region, string matchId, string language)
        {
            var timelines = (await dynamicRepository
                .Query<MatchTimelineEntity>(x => x.MatchId.Equals(matchId, StringComparison.OrdinalIgnoreCase)))
                .Select(x => JsonConvert.DeserializeObject<Timeline>(x.Value))
                .Where(x => EventTypes.BasicSoccerEventTypes.Contains(x.Type))
                .ToList();

            return timelines;
        }

        public async Task<bool> AddCommentaries(string matchId, IEnumerable<ITimeline> timelines, string language)
        {
            var commentaries = timelines
                .Where(x => x.Commentaries != null && x.Commentaries.Any())
                .Select(x => new CommentaryEntity
                {
                    Id = x.Id.ToString(),
                    Value = JsonConvert.SerializeObject(x.Commentaries),
                    MatchId = matchId,
                    Language = language,
                    CreatedTime = DateTime.UtcNow
                }).ToList();

            await dynamicRepository.InsertRange(commentaries);

            return (await dynamicRepository.Save()) > 0;
        }
    }
}
