namespace LiveScore.Soccers.Features.Leagues
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Soccers.Features.Leagues.DataProviders;

    public class LeagueDataAccessImpl : LeagueDataAccess
    {
        private readonly ILeagueApi leagueApi;

        public LeagueDataAccessImpl(ILeagueApi leagueApi)
        {
            this.leagueApi = leagueApi;
        }

        public async Task<IEnumerable<ILeague>> GetLeagues(int sportId, string language)
        {
            var tournament = await leagueApi.GetLeagues(
                 sportId,
                 language);

            var leagues = new List<League>();

            foreach (var tournamentDto in tournament.tournaments)
            {
                leagues.Add(CreateLeague(tournamentDto));
            }

            return leagues;
        }

        private static League CreateLeague(Tournament leagueDto)
        {
            var league = new League
            {
                Id = leagueDto.id,
                Name = leagueDto.name,
                Category = CreateLeagueCategory(leagueDto.category)
            };

            return league;
        }

        private static LeagueCategory CreateLeagueCategory(Category category)
        {
            var leagueCategory = new LeagueCategory
            {
                Id = category.id,
                CountryCode = category.country_code,
                Name = category.name
            };

            return leagueCategory;
        }
    }
}
