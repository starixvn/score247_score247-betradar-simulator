namespace LiveScore.Features.Leagues.Models
{
    public class LeagueRound
    {
        public LeagueRoundTypes Type { get; set; }

        public string Name { get; set; }

        public int? Number { get; set; }

        public string Phase { get; set; }

        public string Group { get; set; }
    }
}