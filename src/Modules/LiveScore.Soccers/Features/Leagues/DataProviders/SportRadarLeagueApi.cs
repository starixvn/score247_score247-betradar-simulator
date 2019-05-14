namespace LiveScore.Soccers.Features.Leagues.DataProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;

    public class SportRadarLeagueApi : ILeagueApi
    {
        private const string FeatureKey = "League";

        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarLeagueApi(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<Tournaments> GetLeagues(int sportId, string language)
        {
            var sportSetting = appSettings
               .DataProviderSettings
               .Sports.FirstOrDefault(sport => sport.Id == sportId)
               ?.EndPoints.FirstOrDefault(x => x.Name.Equals(FeatureKey));

            if (sportSetting == null)
            {
                throw new InvalidOperationException("Please config Sport Setting before using");
            }

            var leagueDtoSum = new Tournaments { generated_at = DateTime.Now.ToUniversalTime(), tournaments = new List<Tournament>() };

            foreach (var endPoint in sportSetting.EndPointSettings)
            {
                if (string.IsNullOrWhiteSpace(endPoint.Path))
                {
                    throw new InvalidOperationException($"Missing function format for endpoint {endPoint.Path}");
                }
                        
                var formattedEndPoint = FormatEndPoint(endPoint.Path, language);
                var leagueDto = await restClient.ExecuteGetAsync<Tournaments>(formattedEndPoint);

                if (leagueDto.tournaments != null
                    && leagueDto.tournaments.Any())
                {                    
                    leagueDtoSum.tournaments = leagueDtoSum.tournaments.Concat(leagueDto.tournaments);
                }
            }

            return await Task.FromResult(leagueDtoSum);
        }

        private static string FormatEndPoint(string endPoint, string language)
            => string.Format(endPoint, language);
    }
}
