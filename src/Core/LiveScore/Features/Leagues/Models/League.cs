namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Shared.Models;

    public interface ILeague : IEntity<string, string>
    {
        int Order { get; }

        string Flag { get; }

        LeagueCategory Category { get; }
    }

    public class League : Entity<string, string>, ILeague
    {
        public int Order { get; set; }

        public string Flag { get; set; }

        public LeagueCategory Category { get; set; }
    }
}