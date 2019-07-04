namespace LiveScore.Features.Leagues.Models
{
    using System.Collections.Generic;

    public class LeagueDetail : League
    {
        public IEnumerable<LeagueRound> Rounds { get; set; }
    }
}
