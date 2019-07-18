namespace LiveScore.Soccer.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Soccer.Odds.Models;

    public class MatchOddsMovement
    {
        public string MatchId { get; set; }

        public Bookmaker Bookmaker { get; set; }

        public IEnumerable<OddsMovement> OddsMovements { get; set; }
    }
}