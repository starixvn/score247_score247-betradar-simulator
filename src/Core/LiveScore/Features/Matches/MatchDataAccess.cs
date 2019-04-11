namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface MatchDataAccess
    {
        Task<IEnumerable<Match>> GetMatches(int sportId, DateTime date, string language);
    }
}