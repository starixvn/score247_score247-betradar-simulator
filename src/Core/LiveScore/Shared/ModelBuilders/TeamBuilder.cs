namespace LiveScore.Shared.ModelBuilders
{
    using LiveScore.SportRadar.Dtos;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static class TeamBuilder
    {
        private const string Home = "home";

        public static List<LiveScore.Features.Teams.Models.Team> BuildTeams(Sport_Event sportEventDto)
        {
            var teams = new List<LiveScore.Features.Teams.Models.Team>();

            foreach (var competitor in sportEventDto.competitors)
            {
                teams.Add(new LiveScore.Features.Teams.Models.Team
                {
                    Id = competitor.id,
                    Country = competitor.country,
                    CountryCode = competitor.country_code,
                    Name = competitor.name,
                    Abbreviation = competitor.abbreviation,
                    IsHome = string.Compare(competitor.qualifier, Home, true, CultureInfo.InvariantCulture) == 0
                });
            }

            return teams.OrderBy(team => team.IsHome ? 0 : 1).ToList();
        }
    }
}