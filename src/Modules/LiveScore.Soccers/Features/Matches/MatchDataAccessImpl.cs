namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Soccers.Features.Matches.DataProviders;

    public class MatchDataAccessImpl : MatchDataAccess
    {        
        private readonly IMatchApi matchApi;

        public MatchDataAccessImpl(IMatchApi matchApi)
        {
            this.matchApi = matchApi;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language)
        {
            var matchEntities = await matchApi.GetDailySchedules(
                sportId,
                from,
                to,
                language);

            return matchEntities.ToList();
        }

        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            return await matchApi.GetLiveSchedules(sportId, language);
        }
    }
}