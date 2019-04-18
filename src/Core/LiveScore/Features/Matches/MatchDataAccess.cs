namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface MatchDataAccess
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime from, DateTime to, string language);
    }
}