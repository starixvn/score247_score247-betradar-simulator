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

    public interface IOddsFetcher
    {
        Task<OddsScheduleDto> Fetch(DateTime date);

        Task<OddsScheduleDto> FetchChangeLog();
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

        public async Task<OddsScheduleDto> FetchChangeLog()
        {
            var soccerSport = appSettings.DataProviderSettings.GetSportSetting(SportConstants.SoccerId);
            var formattedEndpoint = string.Format(
                soccerSport.OddsSetting.ChangeLogEndPoint,
                DateTimeOffset.UtcNow.AddMinutes(-soccerSport.OddsSetting.GetOddsChangeInterval).ToUnixTimeSeconds());

            return await Fetch(formattedEndpoint);
        }

        private async Task<OddsScheduleDto> Fetch(string formattedEndpoint)
        {
            var apiUrl = $"{appSettings.DataProviderSettings.ApiUrl}{formattedEndpoint}";

            try
            {
                return await restClient.ExecuteGetAsync<OddsScheduleDto>(apiUrl);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync(
                        string.Join("\r\n",
                        apiUrl,
                        $"Exception: {ex}"),
                        ex);
            }

            return new OddsScheduleDto();
        }

        public async Task<OddsScheduleDto> Fetch(DateTime date)
        {
            var soccerSport = appSettings.DataProviderSettings.GetSportSetting(SportConstants.SoccerId);
            var formattedEndpoint = string.Format(soccerSport.OddsSetting.EndPoint, date.ToString(SportRadarDateFormat));

            return await Fetch(formattedEndpoint);
        }
    }
}