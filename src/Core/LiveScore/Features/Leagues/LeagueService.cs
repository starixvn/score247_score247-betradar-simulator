namespace LiveScore.Features.Leagues
{
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Shared;    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface LeagueService
    {
        Task<IEnumerable<ILeague>> GetLeagues(int sportId, string language);
    }

    public class LeagueServiceImpl : LeagueService
    {

        private readonly InstanceFactory instanceFactory;

        public LeagueServiceImpl(
            InstanceFactory instanceFactory)
        {            
            this.instanceFactory = instanceFactory;
        }

        public async Task<IEnumerable<ILeague>> GetLeagues(int sportId, string language)
        {
            if (sportId <= 0)
            {
                return await Task.FromResult(Enumerable.Empty<ILeague>());
            }

            var leagueDataAccess = instanceFactory.CreateLeagueDataAccess(sportId);

            var leagues = (await leagueDataAccess
                .GetLeagues(sportId, language))
                .OrderBy(x=>x.Order);         

            return leagues;
        }
    }
}
