////namespace LiveScore.Providers.SportRadar.Services
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Text;
////    using System.Threading.Tasks;
////    using LiveScore.Providers.SportRadar.Dtos;
////    using LiveScore.Soccer.Configurations;
////    using LiveScore.Soccer.Helpers;

////    public interface IOddsFetcher
////    {
////        Task<OddsScheduleDto> FetchDaily();

////        Task<OddsChangeLogDto> FetchChangeLog();
////    }

////    public class OddsFetcher : IOddsFetcher
////    {
////        private readonly IAppSettings appSettings;
////        private readonly IRestClient restClient;
////        private readonly Func<DateTime> getCurrentTimeFunc;

////        public OddsFetcher(
////            IAppSettings appSettings, 
////            IRestClient restClient,
////            Func<DateTime> getCurrentTimeFunc)
////        {
////            this.appSettings = appSettings;
////            this.restClient = restClient;
////            this.getCurrentTimeFunc = getCurrentTimeFunc;
////        }

////        public Task<OddsChangeLogDto> FetchChangeLog()
////        {
////            throw new NotImplementedException();
////        }

////        public Task<OddsScheduleDto> FetchDaily()
////        {
////            //var apiUrl = appSettings.DataProviderSettings.ApiUrl;
////            //var numOfFetchOddsDay = appSettings.DataProviderSettings.Sports.


////            throw new NotImplementedException();
////        }
////    }
////}
