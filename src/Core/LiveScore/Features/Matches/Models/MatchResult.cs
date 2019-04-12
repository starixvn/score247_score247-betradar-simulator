namespace LiveScore.Features.Matches.Models
{
    using System.Collections.Generic;

    public interface IMatchResult
    {
        MatchStatus MatchStatus { get; }

        MatchStatus EventStatus { get; }

        IEnumerable<int> HomeScores { get; }

        IEnumerable<int> AwayScores { get; }

        int Period { get; }

        string MatchTime { get; }

        IEnumerable<MatchPeriod> MatchPeriods { get; }

        string WinnerId { get; }
    }

    public class MatchResult : IMatchResult
    {
        public MatchStatus MatchStatus { get; set; }

        public MatchStatus EventStatus { get; set; }

        public IEnumerable<int> HomeScores { get; set; }

        public IEnumerable<int> AwayScores { get; set; }

        public int Period { get; set; }

        public IEnumerable<MatchPeriod> MatchPeriods { get; set; }

        public string MatchTime { get; set; }

        public string WinnerId { get; set; }
    }
}