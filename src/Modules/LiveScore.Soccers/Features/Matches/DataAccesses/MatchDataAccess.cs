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
            var liveMatches = new List<Match>();
            var formattedFromUtcTime = from.ConvertToUtc(timeZone);
            var formattedToUtcTime = to.ConvertToUtc(timeZone);

            var matchEntities = await dynamicRepository.Query<MatchEntity>
            (
                x => x.SportId == sportId &&
                x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                x.EventDate.ToUniversalTime() >= formattedFromUtcTime &&
                x.EventDate.ToUniversalTime() <= formattedToUtcTime
            );

            var liveMatchEntities = await dynamicRepository.Query<LiveMatchEntity>
               (
                   x => x.SportId == sportId &&
                   x.Language.Equals(language, StringComparison.OrdinalIgnoreCase) &&
                   x.EventDate.ToUniversalTime() >= formattedFromUtcTime &&
                   x.EventDate.ToUniversalTime() <= formattedToUtcTime
               );

            foreach (var entity in liveMatchEntities)
            {
                if (matches.Any(m => m.Id == entity.Id && m.MatchResult.EventStatus.IsClosed()))
                {
                    continue;
                }

                var match = JsonConvert.DeserializeObject<Match>(entity.Value);

                liveMatches.Add(match);
            }

            foreach (var entity in matchEntities)
            {
                if (!liveMatches.Any(m => m.Id == entity.Id))
                {
                    var match = JsonConvert.DeserializeObject<Match>(entity.Value);

                    matches.Add(match);
                }
            }

            return liveMatches.Concat(matches);
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matches = new List<Match>();
            var matchEntities = await dynamicRepository.GetAll<LiveMatchEntity>();

            foreach (var entity in matchEntities)
            {
                var match = JsonConvert.DeserializeObject<Match>(entity.Value);

                if (match.MatchResult.EventStatus.IsLive())
                {
                    matches.Add(match);
                }
            }

            return matches;
        }

        public async Task<IMatch> AddOrUpdateMatch(string matchId, MatchPushEvent pushEvent, bool isLive = false)
        {
            try
            {
                var matchEntity = await dynamicRepository.GetById<MatchEntity>(matchId);
                var match = JsonConvert.DeserializeObject<Match>(matchEntity.Value);
                match.MatchResult = pushEvent.MatchResult;
                match.TimeLines = pushEvent.TimeLines;
                matchEntity.Value = JsonConvert.SerializeObject(match);

                if (isLive)
                {
                    var liveMatchEntity = new LiveMatchEntity
                    {
                        Id = matchEntity.Id,
                        SportId = matchEntity.SportId,
                        Language = matchEntity.Language,
                        Value = matchEntity.Value,
                        LeagueId = matchEntity.LeagueId,
                        EventDate = matchEntity.EventDate.ToUniversalTime()
                    };

                    await dynamicRepository.InsertOrUpdate(matchId, liveMatchEntity);
                }
                else
                {
                    await dynamicRepository.InsertOrUpdate(matchId, matchEntity);
                }

                return match;
            }
            catch
            {
                // Write log
                return null;
            }
        }

        public async Task<bool> DeleteMatch(string matchId, bool isLive = false)
        {
            try
            {
                if (isLive)
                {
                    return await dynamicRepository.Delete<LiveMatchEntity>(matchId).Save() > 0;
                }

                return await dynamicRepository.Delete<MatchEntity>(matchId).Save() > 0;
            }
            catch
            {
                // Write log
                return false;
            }
        }

        public async Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines)
        {
            var matchTimelineEntities = timelines.Select(t => new MatchTimelineEntity
            {
                Id = Guid.NewGuid().ToString(),
                MatchId = matchId,
                Value = JsonConvert.SerializeObject(t),
                CreatedTime = DateTime.Now
            });

            await dynamicRepository.InsertRange(matchTimelineEntities);

            return (await dynamicRepository.Save()) > 0;
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

        public async Task<bool> UpdateLiveMatches(IEnumerable<IMatch> matches)
        {
            foreach (var match in matches)
            {
                var matchEntity = await dynamicRepository.GetById<MatchEntity>(match.Id);
                matchEntity.Value = JsonConvert.SerializeObject(match);
                matchEntity.EventDate = match.EventDate.ToUniversalTime();

                await dynamicRepository.Update(matchEntity).Save();
            }

            return true;
        }
    }
}
