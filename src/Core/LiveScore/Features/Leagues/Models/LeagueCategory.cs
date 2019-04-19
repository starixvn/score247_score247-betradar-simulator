namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Shared.Models;

    public class LeagueCategory : Entity<string, string>
    {
        public string CountryCode { get; set; }
    }
}