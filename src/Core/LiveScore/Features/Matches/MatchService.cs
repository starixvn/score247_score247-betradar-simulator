using Fanex.Caching;
namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;

    public interface MatchService
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, TimeSpan timeZone, string language);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);
    }

    public class MatchServiceImpl : MatchService
    {
        private readonly InstanceFactory instanceFactory;
        private readonly ICacheService cacheService;

        public MatchServiceImpl(InstanceFactory instanceFactory, ICacheService cacheService)
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
                .GetMatches(sportId, from, to, language))
                .Select(match =>
                {
                    if (match.MatchResult.EventStatus.IsLive())
                    {
                        var matchTime = cacheService.Get<int>("MatchTime-" + match.Id);

                        if (matchTime > 0)
                        {
                            match.MatchResult.MatchTime = $"{matchTime}:00";
                        }
                    }

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
    }
}