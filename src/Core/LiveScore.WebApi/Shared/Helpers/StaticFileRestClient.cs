namespace LiveScore.WebApi.Shared.Helpers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;
    using Newtonsoft.Json;

    public class StaticFileRestClient : IRestClient
    {
        private readonly IAppSettings appSettings;

        public StaticFileRestClient(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<T> ExecuteGetAsync<T>(string endPoint, object data = null)
        {
            if (typeof(T) == typeof(DailyResultDto))
            {
                var filePath = $"{appSettings.AppPath}/App_Data/staticdata/daily-schedule.json";
                var jsonData = File.ReadAllText(filePath);
                var dailyScheduleDto = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                //dailyScheduleDto.results = dailyScheduleDto.results.Where(result
                //    => result.sport_event.scheduled >= from && result.sport_event.scheduled <= to);

                return Task.FromResult(dailyScheduleDto as dynamic);
            }

            return Task.FromResult(default(T));
        }

        public Task<T> ExecutePostAsync<T>(string endPoint, object data = null)
        {
            throw new NotImplementedException();
        }
    }
}