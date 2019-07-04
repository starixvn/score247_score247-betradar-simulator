namespace LiveScore.Soccers.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer.Matches.DataAccesses;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Database;
    using LiveScore.Soccer.Database.Entities;
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

        public async Task<IEnumerable<Match>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language)
        {
            var matchEntities = dynamicRepository.Query<MatchEntity>
            (
                x => x.SportId == sportId &&
                x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                x.EventDate.ToUniversalTime() >= utcFrom &&
                x.EventDate.ToUniversalTime() <= utcTo
            );

            return matchEntities.Select(m => JsonConvert.DeserializeObject<Match>(m.Value));
        }

        public async Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language)
        {
            var matchEntities = dynamicRepository.Query<LiveMatchEntity>(m =>
                    m.SportId == sportId &&
                    m.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

            return matchEntities.Select(m => JsonConvert.DeserializeObject<Match>(m.Value));
        }

        public async Task<Match> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false)
        {
            try
            {
                var matchEntity = await dynamicRepository.GetById<MatchEntity>(matchId, language);

                if (matchEntity == null)
                {
                    throw new KeyNotFoundException($"NotFound match {matchId} language {language}");
                }

                var match = JsonConvert.DeserializeObject<Match>(matchEntity.Value);
                match.MatchResult = pushEvent.MatchResult;
                match.TimeLines = pushEvent.TimeLines;
                match.LatestTimeline = pushEvent.TimeLines.OrderBy(x => x.Time).Last();

                matchEntity.Value = JsonConvert.SerializeObject(match);
                matchEntity.EventDate = match.EventDate;

                if (isLive)
                {
                    await AddOrUpdateLiveMatch(matchId, language, matchEntity, match);
                }
                else
                {
                    await dynamicRepository.Update(matchEntity);
                }

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

                await dynamicRepository.Update(liveMatchEntity);
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
                    EventDate = matchEntity.EventDate.ToUniversalTime()
                };

                await dynamicRepository.Insert(liveMatchEntity);
            }
        }

        public async Task<bool> DeleteLiveMatch(string matchId, string language)
        {
            try
            {
                return await dynamicRepository.Delete<LiveMatchEntity>(matchId, language) > 0;
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"DeleteMatch {matchId}", ex);

                return false;
            }
        }

        public async Task<bool> AddTimelines(string matchId, IEnumerable<Timeline> timelines, bool isDeleteExisting = false)
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

            return await dynamicRepository.InsertRange(matchTimelineEntities);
        }

        private async Task DeleteAllTimelines(string matchId)
        {
            var eixstingTimelines = (dynamicRepository.Query<MatchTimelineEntity>(x => x.MatchId.Equals(matchId, StringComparison.OrdinalIgnoreCase)));

            if (eixstingTimelines != null && eixstingTimelines.Any())
            {
                await dynamicRepository.DeleteRange(eixstingTimelines);
            }
        }

        public async Task<bool> AddResult(string matchId, MatchResult matchResult)
        {
            return await dynamicRepository.Insert(
                new MatchResultEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    MatchId = matchId,
                    Value = JsonConvert.SerializeObject(matchResult)
                }) > 0;
        }

        public async Task<bool> UpdateLiveMatches(IEnumerable<Match> matches, string language)
        {
            var liveMatchEntities = new List<LiveMatchEntity>();

            foreach (var match in matches)
            {
                var liveMatchEntity = await dynamicRepository.GetById<LiveMatchEntity>(match.Id, language);
                liveMatchEntity.Value = JsonConvert.SerializeObject(match);
                liveMatchEntity.EventDate = match.EventDate.ToUniversalTime();
                liveMatchEntity.ModifiedTime = DateTime.UtcNow;
                liveMatchEntities.Add(liveMatchEntity);
            }

            return (await dynamicRepository.UpdateRange(liveMatchEntities)) > 0;
        }

        public async Task<Match> GetMatch(int sportId, string matchId, string language)
        {
            var matchEntity = await dynamicRepository.GetById<MatchEntity>(matchId, language);

            return matchEntity == null
                    ? null
                    : JsonConvert.DeserializeObject<Match>(matchEntity.Value);
        }

        public async Task<IEnumerable<Timeline>> GetTimelines(int sportId, string region, string matchId, string language)
        {
            var timelines = (dynamicRepository
                .Query<MatchTimelineEntity>(x => x.MatchId.Equals(matchId, StringComparison.OrdinalIgnoreCase)))
                .Select(x => JsonConvert.DeserializeObject<Timeline>(x.Value))
                .Where(x => EventTypes.BasicSoccerEventTypes.Contains(x.Type))
                .ToList();

            return timelines;
        }

        public async Task AddCommentaries(string matchId, IEnumerable<Timeline> timelines, string language)
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

            if (commentaries.Any())
            {
                await dynamicRepository.InsertRange(commentaries);
            }
        }

        public Task<IEnumerable<MatchFunction>> GetMatchFunctions(
            int sportId,
            string matchId,
            string language)
        {
            // TODO: Hardcode first
            var functions = new List<MatchFunction>
            {
                    new MatchFunction { Abbreviation = "Odds", Name = "Odds" },
                    new MatchFunction { Abbreviation = "Info", Name = "Match Info" },
                    new MatchFunction { Abbreviation = "Tracker", Name = "Tracker" },
                    new MatchFunction { Abbreviation = "Stats", Name = "Statistics" },
                    new MatchFunction { Abbreviation = "Line-ups", Name = "Line-ups" },
                    new MatchFunction { Abbreviation = "H2H", Name = "Head to Head" },
                    new MatchFunction { Abbreviation = "Table", Name = "Table" },
                    new MatchFunction { Abbreviation = "Social", Name = "Social" },
                    new MatchFunction { Abbreviation = "TV", Name = "TV Schedule" }
            };

            return Task.FromResult(functions.Select(function =>
            {
                function.Id = function.Abbreviation.Replace(" ", "_");
                return function;
            }));
        }
    }
}