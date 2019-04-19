namespace LiveScore.Features.Leagues.Models
{
    using System.Collections.Generic;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;

    public interface ILeague : IEntity<string, string>
    {
        int Order { get; }

        string Flag { get; }

        LeagueCategory Category { get; }

        IEnumerable<LeagueRound> Rounds { get; }

        IEnumerable<ITeam> Teams { get; }
    }

    public class League : Entity<string, string>, ILeague
    {
        public int Order { get; set; }

        public string Flag { get; set; }

        public LeagueCategory Category { get; set; }

        public IEnumerable<LeagueRound> Rounds { get; set; }

        public IEnumerable<ITeam> Teams { get; set; }
    }
}