namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Helpers;

    public interface IMatchService
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan clientTimeZone, string language);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);

        Task<bool> AddOrUpdateMatch(int sportId, string language, string matchId, MatchPushEvent pushEvent, bool isLive = false);

        Task<bool> DeleteMatch(int sportId, string language, string matchId, bool isLive = false);

        Task UpdateLiveMatches(int sportId, string language, IEnumerable<IMatch> matches);

        Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<ITimeline> timelines);

        Task<bool> AddResult(int sportId, string matchId, IMatchResult matchResult);
    }

    public class MatchService : IMatchService
    {
        private static readonly CacheItemOptions LiveMatchCacheOption = new CacheItemOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        private readonly InstanceFactory instanceFactory;
        private readonly ICacheService cacheService;

        public MatchService(InstanceFactory instanceFactory, ICacheService cacheService)
        {
            this.instanceFactory = instanceFactory;
            this.cacheService = cacheService;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(
            int sportId,
            DateTime from,
            DateTime to,
            TimeSpan clientTimeZone,
            string language)
        {
            if (sportId <= 0
                || from > to
                || from == DateTime.MinValue
                || to == DateTime.MinValue)
            {
                return await Task.FromResult(Enumerable.Empty<IMatch>());
            }

            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var utcFrom = from.ConvertToUtc(clientTimeZone);
            var utcTo = to.ConvertToUtc(clientTimeZone);
            var normalMatchCacheKey = $"Match_{sportId}_{language}_{utcFrom}_{utcTo}";

            var normalMatches = await await cacheService.GetOrSetAsync(normalMatchCacheKey, async () =>
            {
                return await matchDataAccess.GetMatches(sportId, utcFrom, utcTo, language);
            });

            var liveMatches = await GetLiveMatches(sportId, language);

            var matches = liveMatches
                .Union(normalMatches, new MatchComparer())
                .Select(match => match.ChangeEventDateByTimeZone(clientTimeZone));

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);

            var liveMatches = await await cacheService.GetOrSetAsync(
                    liveMatchCacheKey,
                    async () => (await matchDataAccess.GetLiveMatches(sportId, language)).ToDictionary(m => m.Id),
                    LiveMatchCacheOption);

            return liveMatches.Values;
        }

        public async Task UpdateLiveMatches(int sportId, string language, IEnumerable<IMatch> matches)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);

            await cacheService.SetAsync(liveMatchCacheKey, matches.ToDictionary(m => m.Id), LiveMatchCacheOption);

            await matchDataAccess.UpdateLiveMatches(matches, language);
        }

        public async Task<bool> AddOrUpdateMatch(int sportId, string language, string matchId, MatchPushEvent pushEvent, bool isLive = false)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var match = await matchDataAccess.AddOrUpdateMatch(matchId, language, pushEvent, isLive: true);

            if (match == null)
            {
                return false;
            }

            if (isLive)
            {
                var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
                var liveMatches = await cacheService.GetOrSetAsync(
                        liveMatchCacheKey, () => new Dictionary<string, IMatch>());

                liveMatches[matchId] = match;

                await cacheService.SetAsync(liveMatchCacheKey, liveMatches, LiveMatchCacheOption);
            }

            return true;
        }

        public async Task<bool> DeleteMatch(int sportId, string language, string matchId, bool isLive = false)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            if (isLive)
            {
                var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
                var liveMatches = await cacheService.GetAsync<IDictionary<string, IMatch>>(liveMatchCacheKey);

                if (liveMatches != null && liveMatches.ContainsKey(matchId))
                {
                    liveMatches.Remove(matchId);
                    await cacheService.SetAsync(liveMatchCacheKey, liveMatches);
                }
            }

            return await matchDataAccess.DeleteMatch(matchId, language, isLive);
        }

        public async Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<ITimeline> timelines)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            return await matchDataAccess.AddTimelines(matchId, timelines);
        }

        public async Task<bool> AddResult(int sportId, string matchId, IMatchResult matchResult)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            return await matchDataAccess.AddResult(matchId, matchResult);
        }

        private static string GetLiveMatchCacheKey(int sportId, string language) => $"LiveMatch_{sportId}_{language}";
    }
}