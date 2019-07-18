namespace LiveScore.Soccer.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Soccer.Models;

    public class OddsMovement : Entity<int, string>
    {
        public IEnumerable<BetOptionOdds> BetOptions { get; set; }

        public string MatchTime { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public DateTimeOffset UpdateTime { get; set; }
    }
}