namespace LiveScore.Features.Leagues
{
    using LiveScore.Features.Leagues.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface LeagueDataAccess
    {
        Task<IEnumerable<ILeague>> GetLeagues(int sportId, string language);
    }
}
