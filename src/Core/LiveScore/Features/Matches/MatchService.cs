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
    }

    public class MatchServiceImpl : MatchService
    {
        private readonly InstanceFactory instanceFactory;

        public MatchServiceImpl(
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
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

            return (await matchDataAccess
                .GetMatches(sportId, from, to, language))
                .Select(match =>
                {
                    match.ChangeEventDateByTimeZone(timeZone);

                    return match;
                })
                .ToList();
        }
    }
}