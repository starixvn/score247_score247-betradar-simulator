namespace LiveScore.Features.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;

    public interface IMatch : IEntity<string, string>
    {
        DateTime EventDate { get; }

        ILeague League { get; }

        ILeagueRound LeagueRound { get; }

        IEnumerable<ITeam> Teams { get; }

        bool IsLive { get; }

        IMatchResult MatchResult { get; }

        ITimeLine TimeLine { get; }

        IMatchCondition MatchCondition { get; }
    }

    public class Match : Entity<string, string>, IMatch
    {
        public DateTime EventDate { get; set; }

        public IEnumerable<ITeam> Teams { get; set; }

        public IMatchResult MatchResult { get; set; }

        public ITimeLine TimeLine { get; set; }

        public IMatchCondition MatchCondition { get; set; }

        public bool IsLive { get; set; }

        public ILeague League { get; set; }

        public ILeagueRound LeagueRound { get; set; }
    }
}