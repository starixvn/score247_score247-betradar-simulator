namespace LiveScore.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface IMatchDataAccess
    {
        Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language);

        Task<IMatch> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false);

        Task<bool> DeleteMatch(string matchId, string language, bool isLive = false);

        Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language);

        Task<bool> UpdateLiveMatches(IEnumerable<IMatch> matches, string language);

        Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines);

        Task<bool> AddResult(string matchId, IMatchResult matchResult);

        Task<IMatch> GetMatch(int sportId, string matchId, string language);

        Task<IEnumerable<ITimeline>> GetTimelines(int sportId, string region, string matchId, string language);
    }
}