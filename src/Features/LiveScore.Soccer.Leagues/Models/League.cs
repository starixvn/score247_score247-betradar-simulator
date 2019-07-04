namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Soccer.Shared.Models;

    public class League : Entity<string, string>
    {
        public int Order { get; set; }

        public string Flag { get; set; }

        public LeagueCategory Category { get; set; }
    }
}