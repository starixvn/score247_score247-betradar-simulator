namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Shared.Models;

    public interface ILeagueCategory : IEntity<string, string>
    {
        string CountryCode { get; }
    }

    public class LeagueCategory : Entity<string, string>, ILeagueCategory
    {
        public string CountryCode { get; set; }
    }
}