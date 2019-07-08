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

        Task<OddsChangeLogDto> FetchChangeLog();
    }

    public class OddsFetcher : IOddsFetcher
    {
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;
        private readonly ILogger logger;
        private const string SportRadarDateFormat = "yyyy-MM-dd";

        public OddsFetcher(
            IAppSettings appSettings,
            IRestClient restClient,
            ILogger logger)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
            this.logger = logger;
        }

        public Task<OddsChangeLogDto> FetchChangeLog()
        {
            throw new NotImplementedException();
        }

        public async Task<OddsScheduleDto> Fetch(DateTime date)
        {
            try
            {
                var soccerSport = appSettings.DataProviderSettings.Sports.FirstOrDefault(sport => sport.Id == SportConstants.SoccerId);
                // Ricky: Temporary uses full endpoint url here, because simulator have not implemented yet
                var formattedEndpoint = string.Format(soccerSport.OddsEndPoint, date.ToString(SportRadarDateFormat));
                var oddsSchedule = await restClient.ExecuteGetAsync<OddsScheduleDto>(formattedEndpoint);

                return oddsSchedule;
            }
        }
    }
}