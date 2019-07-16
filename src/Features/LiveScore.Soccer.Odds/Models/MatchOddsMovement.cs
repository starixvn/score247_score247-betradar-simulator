namespace LiveScore.Soccer.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Odds.Models;

    public class MatchOddsMovement
    {
        public string MatchId { get; set; }

        public Bookmaker Bookmaker { get; set; }

        public DateTime? LastUpdated { get; set; }

        public IEnumerable<OddsMovement> OddsMovements { get; set; }
    }
}