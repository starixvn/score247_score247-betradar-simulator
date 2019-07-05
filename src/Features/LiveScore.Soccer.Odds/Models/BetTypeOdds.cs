namespace LiveScore.Features.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Soccer.Models;

    public class BetTypeOdds : Entity<int, string>
    {
        public Bookmaker Bookmaker { get; set; }

        public IEnumerable<BetOptionOdds> BetOptions { get; set; }
    }
}