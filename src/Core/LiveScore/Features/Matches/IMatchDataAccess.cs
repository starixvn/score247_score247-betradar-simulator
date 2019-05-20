namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface IMatchDataAccess
    {
        Task<IEnumerable<IMatch>> GetMatches(
            int sportId,
            DateTime from,
            DateTime to,
            string language);

        Task<IEnumerable<IMatch>> GetLiveMatches(
          int sportId,
          string language);

        Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines);

        Task<bool> AddResult(string matchId, IMatchResult matchResult);
    }
}