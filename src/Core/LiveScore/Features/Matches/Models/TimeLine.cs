namespace LiveScore.Features.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;

    public interface ITimeline : IEntity<long, string>
    {
        string Type { get; }

        string Time { get; }

        int MatchTime { get; }

        string MatchClock { get; }

        string Team { get; }

        int Period { get; }

        string PeriodType { get; }

        int HomeScore { get; }

        int AwayScore { get; }

        GoalScorer GoalScorer { get; }

        Player Assist { get; }

        Player PlayerOut { get; }

        Player PlayerIn { get; }

        Player Player { get; }

        int InjuryTimeAnnounced { get; }

        string Description { get; }

        string Outcome { get; }

        IEnumerable<Commentary> Commentaries { get; }
    }

    public class Timeline : Entity<long, string>, ITimeline
    {
        public string Type { get; set; }

        public string Time { get; set; }

        public int MatchTime { get; set; }

        public string MatchClock { get; set; }

        public string Team { get; set; }

        public int Period { get; set; }

        public string PeriodType { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public GoalScorer GoalScorer { get; set; }

        public IEnumerable<Commentary> Commentaries { get; set; }

        public Player Assist { get; set; }

        public Player PlayerOut { get; set; }

        public Player PlayerIn { get; set; }

        public Player Player { get; set; }

        public int InjuryTimeAnnounced { get; set; }

        public string Description { get; set; }

        public string Outcome { get; set; }
    }

    public class Commentary
    {
        public string Text { get; set; }
    }
}