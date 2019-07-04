namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Soccer.Shared.Models;

    public class LeagueCategory : Entity<string, string>
    {
        public string CountryCode { get; set; }
    }
}