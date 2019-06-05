namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Helpers;
    using Fanex.Caching;

    public interface IMatchService
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan clientTimeZone, string language);

        Task<IEnumerable<IMatch>> GetNonLiveMatches(int sportId, string language, DateTime utcFrom, DateTime utcTo);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);

        Task<bool> AddOrUpdateMatch(int sportId, string language, string matchId, MatchPushEvent pushEvent, bool isLive = false);

        Task<bool> DeleteMatch(int sportId, string language, string matchId, bool isLive = false);

        Task UpdateLiveMatches(int sportId, string language, IEnumerable<IMatch> matches);

        Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<ITimeline> timelines);

        Task<bool> AddResult(int sportId, string matchId, IMatchResult matchResult);

        Task<IMatch> GetMatch(int sportId, string matchId, string language);
    }

    public class MatchService : IMatchService
    {
        private static readonly CacheItemOptions LiveMatchCacheOptions = new CacheItemOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10),
        };
        private static readonly CacheItemOptions NormalMatchCacheOptions = new CacheItemOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(120),
        };
        private readonly DataAccessFactory instanceFactory;
        private readonly ICacheService cacheService;

        public MatchService(DataAccessFactory instanceFactory, ICacheService cacheService)
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
            var matches = Enumerable.Empty<IMatch>();

            if (sportId <= 0 || from > to || from == DateTime.MinValue || to == DateTime.MinValue)
            {
                return await Task.FromResult(matches);
            }

            var utcFrom = from.ConvertToUtc(clientTimeZone);
            var utcTo = to.ConvertToUtc(clientTimeZone);
            var nonLiveMatches = await GetNonLiveMatches(sportId, language, utcFrom, utcTo);
            var liveMatches = await GetLiveMatches(sportId, language);

            matches = (liveMatches.Any()
                    ? liveMatches.Union(nonLiveMatches, new MatchComparer())
                    : nonLiveMatches)
                .Select(match => match.ChangeEventDateByTimeZone(clientTimeZone));

            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetNonLiveMatches(int sportId, string language, DateTime utcFrom, DateTime utcTo)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            var normalMatchCacheKey = $"Match_{sportId}_{language}_{utcFrom}_{utcTo}";
            var normalMatches = await cacheService.GetAsync<IEnumerable<IMatch>>(normalMatchCacheKey);

            if (normalMatches == null)
            {
                normalMatches = await matchDataAccess.GetMatches(sportId, utcFrom, utcTo, language);
                await cacheService.SetAsync(normalMatchCacheKey, normalMatches, NormalMatchCacheOptions);
            }

            return normalMatches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
            var liveMatches = await cacheService.GetAsync<IDictionary<string, IMatch>>(liveMatchCacheKey);

            if (liveMatches == null || liveMatches.Count == 0)
            {
                liveMatches = (await matchDataAccess.GetLiveMatches(sportId, language)).ToDictionary(m => m.Id);
                await cacheService.SetAsync(liveMatchCacheKey, liveMatches, LiveMatchCacheOptions);
            }

            return liveMatches.Values;
        }

        public async Task UpdateLiveMatches(int sportId, string language, IEnumerable<IMatch> matches)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);

            cacheService.Set(liveMatchCacheKey, matches.ToDictionary(m => m.Id), LiveMatchCacheOptions);

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
                var liveMatches = await cacheService.GetAsync<IDictionary<string, IMatch>>(liveMatchCacheKey);

                if (liveMatches == null)
                {
                    liveMatches = new Dictionary<string, IMatch>();
                }

                liveMatches[matchId] = match;

                //TODO update only new match
                cacheService.Set(liveMatchCacheKey, liveMatches);
            }

            return true;
        }

        public async Task<bool> DeleteMatch(int sportId, string language, string matchId, bool isLive = false)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            if (isLive)
            {
                var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
                var liveMatches = cacheService.Get<IDictionary<string, IMatch>>(liveMatchCacheKey);

                if (liveMatches != null && liveMatches.ContainsKey(matchId))
                {
                    liveMatches.Remove(matchId);
                    cacheService.Set(liveMatchCacheKey, liveMatches, LiveMatchCacheOptions);
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

        public Task<IMatch> GetMatch(int sportId, string matchId, string language)
        {
            if (sportId <= 0
                || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException("Missing sportId or matchId argument");
            }

            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            return matchDataAccess.GetMatch(sportId, matchId, language);
        }
    }
}