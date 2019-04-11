namespace LiveScore.Features.Leagues
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;

    public interface LeagueDataAccess
    {
        Task<IEnumerable<ILeague>> GetLeagues(int sportId, DateTime date);
    }
}