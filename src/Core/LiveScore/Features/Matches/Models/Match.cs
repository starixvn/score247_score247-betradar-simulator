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

        League League { get; }

        LeagueRound LeagueRound { get; }

        IEnumerable<Team> Teams { get; }

        MatchResult MatchResult { get; }

        void ChangeEventDateByTimeZone(TimeSpan timeZone);
    }

    public class Match : Entity<string, string>, IMatch
    {
        public DateTime EventDate { get; set; }

        public IEnumerable<Team> Teams { get; set; }

        public MatchResult MatchResult { get; set; }

        public League League { get; set; }

        public LeagueRound LeagueRound { get; set; }

        public void ChangeEventDateByTimeZone(TimeSpan timeZone)
        {
            EventDate = EventDate.ToUniversalTime() + timeZone;
        }
    }
}