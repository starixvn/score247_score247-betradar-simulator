namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;

    public interface IMatchService
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan timeZone, string language);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);

        Task<bool> AddTimelines(int sportId, string matchId, IEnumerable<ITimeline> timelines);

        Task<bool> AddResult(int sportId, string matchId, IMatchResult matchResult);

        Task UpdateLiveMatches(int sportId, IEnumerable<IMatch> matches);
    }

    public class MatchService : IMatchService
    {
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
            TimeSpan timeZone,
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

            var matches = (await matchDataAccess
                .GetMatches(sportId, from, to, language, timeZone))
                .Select(match =>
                {
                    match.ChangeEventDateByTimeZone(timeZone);

                    return match;
                });


            return matches;
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            return await matchDataAccess.GetLiveMatches(sportId, language);
        }

        public async Task UpdateLiveMatches(int sportId, IEnumerable<IMatch> matches)
        {
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            await matchDataAccess.UpdateLiveMatches(matches);
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
    }
}