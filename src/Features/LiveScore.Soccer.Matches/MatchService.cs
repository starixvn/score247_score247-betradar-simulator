namespace LiveScore.Soccer.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Soccer.Matches.DataAccesses;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Shared;
    using LiveScore.Soccer.Shared.Configurations;
    using LiveScore.Soccer.Shared.Enumerations;
    using LiveScore.Soccer.Shared.Helpers;
    using LiveScore.Soccer.Shared.Models;
    using Newtonsoft.Json;

    public interface IMatchService
    {
        Task<IEnumerable<Match>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan clientTimeZone, string language);

        Task<IEnumerable<Match>> GetNonLiveMatches(int sportId, string language, DateTime utcFrom, DateTime utcTo);

        Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language);

        Task<bool> AddOrUpdateMatch(int sportId, string language, string matchId, MatchPushEvent pushEvent, bool isLive = false);

        Task<bool> DeleteLiveMatch(int sportId, string language, string matchId);

        Task UpdateLiveMatches(int sportId, string language, IEnumerable<Match> matches);

        Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<Timeline> timelines);

        Task<bool> AddResult(int sportId, string matchId, MatchResult matchResult);

        Task<Match> GetMatch(int sportId, string matchId, string language, TimeSpan clientTimeZone);

        Task AddFullTimelines(int sportId, string matchId, string language, string region);

        Task<IEnumerable<Timeline>> GetTimelines(int sportId, string region, string matchId, string language);
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

        private readonly ICacheService cacheService;
        private readonly IAppSettings appSettings;
        private readonly Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory;

        public MatchService(
            ICacheService cacheService,
            IAppSettings appSettings,
            Func<int, DataProvider, IMatchDataAccess> matchDataAccessFactory)
        {
            this.cacheService = cacheService;
            this.appSettings = appSettings;
            this.matchDataAccessFactory = matchDataAccessFactory;
        }

        public async Task<IEnumerable<Match>> GetMatches(
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
                return Enumerable.Empty<Match>();
            }

            var utcFrom = from.ConvertToUtc(clientTimeZone);
            var utcTo = to.ConvertToUtc(clientTimeZone);
            var nonLiveMatches = await GetNonLiveMatches(sportId, language, utcFrom, utcTo);
            var liveMatches = await GetLiveMatches(sportId, language, utcFrom, utcTo);

            var matches = liveMatches.Any()
                      ? liveMatches.Union(nonLiveMatches, new MatchComparer())
                      : nonLiveMatches;

            return matches
                    .Select(m => m.ChangeEventDateByTimeZone(clientTimeZone))
                    .OrderBy(m => m.EventDate)
                    .Take(appSettings.NumberOfTopMatches);
        }

        private async Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language, DateTime utcFrom, DateTime utcTo)
        {
            if (utcFrom <= DateTime.Today.ToUniversalTime()
                && utcTo >= DateTime.Today.ToUniversalTime())
            {
                return await GetLiveMatches(sportId, language);
            }

            return Enumerable.Empty<Match>();
        }

        public async Task<IEnumerable<Match>> GetNonLiveMatches(int sportId, string language, DateTime utcFrom, DateTime utcTo)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

            var normalMatchCacheKey = $"Match_{sportId}_{language}_{utcFrom}_{utcTo}";
            var normalMatches = await cacheService.GetAsync<IEnumerable<Match>>(normalMatchCacheKey);

            if (normalMatches == null)
            {
                normalMatches = await matchDataAccess.GetMatches(sportId, utcFrom, utcTo, language);
                await cacheService.SetAsync(normalMatchCacheKey, JsonConvert.SerializeObject(normalMatches), NormalMatchCacheOptions);
            }

            return normalMatches.Where(m => !m.MatchResult.EventStatus.IsLive());
        }

        public async Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
            var liveMatches = await cacheService.GetAsync<IDictionary<string, Match>>(liveMatchCacheKey);

            if (liveMatches == null || liveMatches.Count == 0)
            {
                liveMatches = (await matchDataAccess.GetLiveMatches(sportId, language)).ToDictionary(m => m.Id);
                await cacheService.SetAsync(liveMatchCacheKey, JsonConvert.SerializeObject(liveMatches), LiveMatchCacheOptions);
            }

            return liveMatches.Values;
        }

        public async Task UpdateLiveMatches(int sportId, string language, IEnumerable<Match> matches)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);
            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);

            cacheService.Set(liveMatchCacheKey, JsonConvert.SerializeObject(matches.ToDictionary(m => m.Id)), LiveMatchCacheOptions);

            await matchDataAccess.UpdateLiveMatches(matches, language);
        }

        public async Task<bool> AddOrUpdateMatch(int sportId, string language, string matchId, MatchPushEvent pushEvent, bool isLive = false)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);
            var match = await matchDataAccess.AddOrUpdateMatch(matchId, language, pushEvent, isLive);

            if (match == null)
            {
                return false;
            }

            if (isLive)
            {
                var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
                var liveMatches = await cacheService.GetAsync<IDictionary<string, Match>>(liveMatchCacheKey);

                if (liveMatches == null)
                {
                    liveMatches = new Dictionary<string, Match>();
                }

                liveMatches[matchId] = match;

                //TODO update only new match
                cacheService.Set(liveMatchCacheKey, JsonConvert.SerializeObject(liveMatches));
            }

            return true;
        }

        public async Task<bool> DeleteLiveMatch(int sportId, string language, string matchId)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

            var liveMatchCacheKey = GetLiveMatchCacheKey(sportId, language);
            var liveMatches = cacheService.Get<IDictionary<string, Match>>(liveMatchCacheKey);

            if (liveMatches != null && liveMatches.ContainsKey(matchId))
            {
                liveMatches.Remove(matchId);
                cacheService.Set(liveMatchCacheKey, JsonConvert.SerializeObject(liveMatches), LiveMatchCacheOptions);
            }

            return await matchDataAccess.DeleteLiveMatch(matchId, language);
        }

        public async Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<Timeline> timelines)
        {
            var result = false;

            if (timelines != null && timelines.Any())
            {
                var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

                result = await matchDataAccess.AddTimelines(matchId, timelines);
            }

            return result;
        }

        public async Task<bool> AddResult(int sportId, string matchId, MatchResult matchResult)
        {
            var result = false;

            if (matchResult != null)
            {
                var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

                result = await matchDataAccess.AddResult(matchId, matchResult);
            }

            return result;
        }

        private static string GetLiveMatchCacheKey(int sportId, string language) => $"LiveMatch_{sportId}_{language}";

        public async Task<Match> GetMatch(int sportId, string matchId, string language, TimeSpan clientTimeZone)
        {
            if (sportId <= 0
                || string.IsNullOrWhiteSpace(matchId))
            {
                throw new ArgumentException("Missing sportId or matchId argument");
            }

            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

            var liveMatches = await GetLiveMatches(sportId, language);
            var match = liveMatches.Any(m => m.Id == matchId)
                  ? liveMatches.FirstOrDefault(m => m.Id == matchId)
                  : await matchDataAccess.GetMatch(sportId, matchId, language);

            if (match != null)
            {
                if (!match.MatchResult.EventStatus.IsNotStartOrDelay())
                {
                    //TODO: Need review immutable here
                    match.TimeLines = await GetTimelines(sportId, match.Region, matchId, language);
                }

                match.Functions = await matchDataAccess.GetMatchFunctions(sportId, matchId, language);

                match.ChangeEventDateByTimeZone(clientTimeZone);
            }

            return match;
        }

        public async Task AddFullTimelines(int sportId, string matchId, string language, string region)
        {
            var dataAccess = matchDataAccessFactory(sportId, DataProvider.ThirdPartyApi);
            var timelines = await dataAccess.GetTimelines(sportId, region, matchId, Enumeration.FromValue<Language>(language).DisplayName.ToLowerInvariant());

            if (timelines?.Any() == true)
            {
                var dbDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);
                await dbDataAccess.AddCommentaries(matchId, timelines, language);

                var timelinesExcludeComment = timelines.Select(x => RemoveCommentary(x)).ToList();
                await dbDataAccess.AddTimelines(matchId, timelinesExcludeComment, isDeleteExisting: true);
            }
        }

        private static Timeline RemoveCommentary(Timeline timeline)
        {
            timeline.Commentaries = null;

            return timeline;
        }

        public async Task<IEnumerable<Timeline>> GetTimelines(int sportId, string region, string matchId, string language)
        {
            var matchDataAccess = matchDataAccessFactory(sportId, DataProvider.Database);

            return await matchDataAccess.GetTimelines(sportId, region, matchId, language);
        }
    }
}