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
        private const string DefaultTimeZoneId = "SE Asia Standard Time";

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

            var matches = new List<IMatch>();
            var matchDataAccess = instanceFactory.CreateMatchDataAccess(sportId);

            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();
            const int dayIncrementIndex = 1;

            for (var date = fromUtc.Date; date.Date <= toUtc.Date; date = date.AddDays(dayIncrementIndex))
            {
                matches.AddRange(await matchDataAccess.GetMatches(sportId, date.Date, language));
            }

            return matches
                .Where(match => match.EventDate > from && match.EventDate < to)
                .Select(match =>
                {
                    match.ChangeEventDateByTimeZone(timeZone);
                    return match;
                });
        }
    }
}