namespace LiveScore.Features.Teams.Models
{
    using System.Collections.Generic;
    using LiveScore.Shared.Models;

    public interface ITeam : IEntity<string, string>
    {
        string Country { get; }

        string CountryCode { get; }

        string Flag { get; }

        bool IsHome { get; }

        //"formation": "4-2-3-1",
        string Formation { get; }

        string Abbreviation { get; }

        ITeamStatistic Statistic { get; }

        ICoach Coach { get; }

        IEnumerable<IPlayer> Players { get; }

        IEnumerable<IPlayer> Substitutions { get; }
    }

    public class Team : Entity<string, string>, ITeam
    {
        public string Country { get; set; }

        public string CountryCode { get; set; }

        public string Flag { get; set; }

        public bool IsHome { get; set; }

        public IEnumerable<IPlayer> Players { get; set; }

        public ITeamStatistic Statistic { get; set; }

        public ICoach Coach { get; set; }

        public string Formation { get; set; }

        public string Abbreviation { get; set; }

        public IEnumerable<IPlayer> Substitutions { get; set; }
    }
}