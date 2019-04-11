namespace LiveScore.Features.Leagues.Models
{
    using System.Collections.Generic;
    using LiveScore.Features.Matches.Models;

    public interface ILeagueRound
    {
        LeagueRoundTypes Type { get; }

        string Name { get; }

        int Number { get; }

        string Phase { get; }

        IEnumerable<Match> Matches { get; }
    }

    public class LeagueRound : ILeagueRound
    {
        public LeagueRoundTypes Type { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Phase { get; set; }

        public IEnumerable<Match> Matches { get; set; }
    }
}