namespace LiveScore.Features.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;

    public interface ITimeLine : IEntity<long, string>
    {
        string Type { get; }

        DateTime Time { get; }

        int MatchTime { get; }

        string MatchClock { get; }

        string Team { get; }

        int Period { get; }

        string PeriodType { get; }

        int HomeScore { get; }

        int AwayScore { get; }

        Player GoalScorer { get; }

        Player Assist { get; }

        Player PlayerOut { get; }

        Player PlayerIn { get; }

        int InjuryTimeAnnounced { get; }

        string Description { get; }

        string Outcome { get; }

        IEnumerable<Commentary> Commentaries { get; }
    }

    public class TimeLine : Entity<long, string>, ITimeLine
    {
        public string Type { get; set; }

        public DateTime Time { get; set; }

        public int MatchTime { get; set; }

        public string MatchClock { get; set; }

        public string Team { get; set; }

        public int Period { get; set; }

        public string PeriodType { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public Player GoalScorer { get; set; }

        public IEnumerable<Commentary> Commentaries { get; set; }

        public Player Assist { get; set; }

        public Player PlayerOut { get; set; }

        public Player PlayerIn { get; set; }

        public int InjuryTimeAnnounced { get; set; }

        public string Description { get; set; }

        public string Outcome { get; set; }
    }

    public class Commentary
    {
        public string Text { get; set; }
    }
}