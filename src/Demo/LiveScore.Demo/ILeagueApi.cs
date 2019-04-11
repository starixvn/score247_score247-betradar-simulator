namespace LiveScore.Features.Leagues
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues.Models;
    using Refit;

    public interface ILiveScoreLeagueApi
    {
        [Get("/League/GetLeaguesByDate?sportId={sportId}&from={fromDate}&to={toDate}")]
        Task<IEnumerable<League>> GetLeagues(int sportId, string fromDate, string toDate);
    }
}