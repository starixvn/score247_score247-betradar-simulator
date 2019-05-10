namespace LiveScore.Features.Leagues.Models
{
    using LiveScore.Features.Teams.Models;
    using System.Collections.Generic;

    public interface ILeagueDetail
    {
        IEnumerable<LeagueRound> Rounds { get; }

        IEnumerable<ITeam> Teams { get; }
    }
    public class LeagueDetail : League, ILeagueDetail
    {
        public IEnumerable<LeagueRound> Rounds { get; set; }

        public IEnumerable<ITeam> Teams { get; set; }
    }
}
