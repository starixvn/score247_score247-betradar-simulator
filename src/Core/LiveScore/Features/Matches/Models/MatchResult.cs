namespace LiveScore.Features.Matches.Models
{
    using System.Collections.Generic;

    public interface IMatchResult
    {
        MatchStatus MatchStatus { get; }

        MatchStatus EventStatus { get; }

        int HomeScore { get; }

        int AwayScore { get; }

        int Period { get; }

        string MatchTime { get; set; }

        IEnumerable<MatchPeriod> MatchPeriods { get; }

        string WinnerId { get; }
    }

    public class MatchResult : IMatchResult
    {
        public MatchStatus MatchStatus { get; set; }

        public MatchStatus EventStatus { get; set; }

        public int Period { get; set; }

        public IEnumerable<MatchPeriod> MatchPeriods { get; set; }

        public string MatchTime { get; set; }

        public string WinnerId { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }
    }
}