namespace LiveScore.Features.Leagues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Shared;

    public interface LeagueService
    {
        Task<IEnumerable<ILeague>> GetLeagues(int sportId, DateTime from, DateTime to);
    }

    public class LeagueServiceImpl : LeagueService
    {
        private readonly InstanceFactory instanceFactory;

        public LeagueServiceImpl(
            InstanceFactory instanceFactory)
        {
            this.instanceFactory = instanceFactory;
        }

        public async Task<IEnumerable<ILeague>> GetLeagues(int sportId, DateTime from, DateTime to)
        {
            if (sportId == 0)
            {
                return await Task.FromResult(Enumerable.Empty<ILeague>());
            }

            var leagues = new List<ILeague>();
            var leagueDataAccess = instanceFactory.CreateLeagueDataAccess(sportId);

            for (DateTime date = from.Date; date.Date < to.Date; date = date.AddDays(1))
            {
                leagues.AddRange(await leagueDataAccess.GetLeagues(1, date.Date));
            }

            return leagues;
        }
    }
}