namespace LiveScore.Features.Odds.Models
{
    using System;
    using LiveScore.Soccer.Models;

    public class BetOptionOdds : Entity<int, string>
    {
        public string Type { get; set; }

        public decimal LiveOdds { get; set; }

        public decimal OpeningOdds { get; set; }

        public string OptionValue { get; set; }

        public OddsTrend OddsTrend
            => OpeningOdds == LiveOdds
                ? OddsTrend.Neutral
#pragma warning disable S3358 // Ternary operators should not be nested
                : OpeningOdds > LiveOdds
                    ? OddsTrend.Down
                    : OddsTrend.Up;
#pragma warning restore S3358 // Ternary operators should not be nested

        public DateTime LastUpdatedTime { get; set; }

        public string MatchTime { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }
    }
}