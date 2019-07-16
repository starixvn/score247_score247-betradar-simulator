namespace LiveScore.Providers.SportRadar.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Providers.SportRadar.Dtos;
    using LiveScore.Soccer;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccer.Helpers;
    using Newtonsoft.Json;

    public interface IOddsFetcher
    {
        Task<OddsScheduleDto> Fetch(DateTime date);

        Task<OddsChangeLogDto> FetchChangeLog();
    }

    public class OddsFetcher : IOddsFetcher
    {
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private readonly ILogger logger;

        public OddsFetcher(
            IAppSettings appSettings,
            IRestClient restClient,
            ILogger logger)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
            this.logger = logger;
        }

        public async Task<OddsChangeLogDto> FetchChangeLog()
        {
            var soccerSport = appSettings.DataProviderSettings.Sports.FirstOrDefault(sport => sport.Id == SportConstants.SoccerId);
            var formattedEndpoint = string.Format(soccerSport.OddsChangeLogEndPoint, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var apiUrl = $"{appSettings.DataProviderSettings.ApiUrl}{formattedEndpoint}";

            try
            {
                return await restClient.ExecuteGetAsync<OddsChangeLogDto>(apiUrl);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync(
                        string.Join("\r\n",
                        apiUrl,
                        $"Exception: {ex}"),
                        ex);
            }

            return new OddsChangeLogDto();
        }

        public async Task<OddsScheduleDto> Fetch(DateTime date)
        {
            var soccerSport = appSettings.DataProviderSettings.Sports.FirstOrDefault(sport => sport.Id == SportConstants.SoccerId);
            var formattedEndpoint = string.Format(soccerSport.OddsEndPoint, date.ToString(SportRadarDateFormat));
            var apiUrl = $"{appSettings.DataProviderSettings.ApiUrl}{formattedEndpoint}";

            try
            {
                return await restClient.ExecuteGetAsync<OddsScheduleDto>(apiUrl);
            }
            catch(Exception ex)
            {
                await logger.ErrorAsync(
                        string.Join("\r\n",
                        apiUrl,
                        $"Exception: {ex}"),
                        ex);
            }

            return new OddsScheduleDto();
        }
    }
}