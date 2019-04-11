namespace LiveScore.Features.Leagues.Models
{
    using System.Collections.Generic;
    using LiveScore.Features.Teams.Models;
    using LiveScore.Shared.Models;

    public interface ILeague : IEntity<string, string>
    {
        int Order { get; }

        string Flag { get; }

        ILeagueCategory Category { get; }

        IEnumerable<ILeagueRound> Rounds { get; }

        IEnumerable<ITeam> Teams { get; }
    }

    public class League : Entity<string, string>, ILeague
    {
        public int Order { get; set; }

        public string Flag { get; set; }

        public ILeagueCategory Category { get; set; }

        public IEnumerable<ILeagueRound> Rounds { get; set; }

        public IEnumerable<ITeam> Teams { get; set; }
    }
}