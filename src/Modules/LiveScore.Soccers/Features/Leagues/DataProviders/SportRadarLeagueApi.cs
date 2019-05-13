namespace LiveScore.Soccers.Features.Leagues.DataProviders
{
    using System;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;

    public class SportRadarLeagueApi : ILeagueApi
    {
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarLeagueApi(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public Task<Tournaments> GetLeagues(int sportId, string language)
        {
            throw new NotImplementedException();
        }
    }
}
