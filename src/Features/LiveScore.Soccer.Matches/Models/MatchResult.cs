namespace LiveScore.Soccer.Matches.Models
{
    using System.Collections.Generic;

    public class MatchResult
    {
        public MatchStatus MatchStatus { get; set; }

        public MatchStatus EventStatus { get; set; }

        public int Period { get; set; }

        public IEnumerable<MatchPeriod> MatchPeriods { get; set; }

        public int MatchTime { get; set; }

        public string WinnerId { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public int AggregateHomeScore { get; set; }

        public int AggregateAwayScore { get; set; }

        public string AggregateWinnerId { get; set; }
    }
}