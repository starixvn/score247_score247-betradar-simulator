namespace LiveScore.Features.Leagues.Models
{
    using System.Collections.Generic;
    using LiveScore.Features.Matches.Models;

    public class LeagueRound
    {
        public LeagueRoundTypes Type { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Phase { get; set; }

        public string Group { get; set; }
    }
}