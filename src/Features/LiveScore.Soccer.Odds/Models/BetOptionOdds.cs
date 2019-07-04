namespace LiveScore.Features.Odds.Models
{
    using System;
    using LiveScore.Soccer.Shared.Models;

    public class BetOptionOdds : Entity<int, string>
    {
        public string Type { get; set; }

        public decimal LiveOdds { get; set; }

        public decimal OpeningOdds { get; set; }

        public string OptionValue { get; set; }

        public OddsTrend OddsTrend { get; set; }
    }

    public class LiveBetOptionOdds : BetOptionOdds
    {
        public DateTime LastUpdatedTime { get; set; }

        public string MatchTime { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }
    }
}