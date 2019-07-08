namespace LiveScore.Features.Odds.Models
{
    using System;
    using System.Collections.Generic;

    public class MatchOdds
    {
        public string MatchId { get; set; }

        public DateTime? LastUpdated { get; set; }

        public IEnumerable<BetTypeOdds> BetTypeOddsList { get; set; }
    }
}