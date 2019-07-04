namespace LiveScore.Soccer.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Soccer.Matches.Models;

    public interface IMatchDataAccess
    {
        Task<IEnumerable<Match>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language);

        Task<Match> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false);

        Task<bool> DeleteLiveMatch(string matchId, string language);

        Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language);

        Task<bool> UpdateLiveMatches(IEnumerable<Match> matches, string language);

        Task<bool> AddTimelines(string matchId, IEnumerable<Timeline> timelines, bool isDeleteExisting = false);

        Task<bool> AddResult(string matchId, MatchResult matchResult);

        Task<Match> GetMatch(int sportId, string matchId, string language);

        Task<IEnumerable<Timeline>> GetTimelines(int sportId, string region, string matchId, string language);

        Task AddCommentaries(string matchId, IEnumerable<Timeline> timelines, string language);

        Task<IEnumerable<MatchFunction>> GetMatchFunctions(int sportId, string matchId, string language);
    }
}