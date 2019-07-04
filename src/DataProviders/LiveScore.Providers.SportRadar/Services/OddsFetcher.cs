namespace LiveScore.Providers.SportRadar.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccer.Helpers;

    public interface IOddsFetcher
    {
        Task<OddsScheduleDto> FetchDaily();

        Task<OddsChangeLogDto> FetchChangeLog();
    }

    public class OddsFetcher : IOddsFetcher
    {
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public OddsFetcher(IAppSettings appSettings, IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public Task<OddsChangeLogDto> FetchChangeLog()
        {
            throw new NotImplementedException();
        }

        public Task<OddsScheduleDto> FetchDaily()
        {
            throw new NotImplementedException();
        }
    }
}
