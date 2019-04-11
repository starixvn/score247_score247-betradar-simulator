namespace LiveScore.Soccers.Features.Matches.DataProviders
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Models.Dtos;
    using Newtonsoft.Json;

    public class StaticMatchApi : IMatchApi
    {
        private readonly IAppSettings appSettings;

        public StaticMatchApi(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<DailyScheduleDto> GetDailySchedules(
            int sportId,
            DateTime date,
            string lang)
        {
            // Temporary for hardcode, please ignore it
            var jsonData = File.ReadAllText($"{appSettings.AppPath}/App_Data/staticdata/daily-schedule.json");
            var dailyScheduleDto = JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData);

            foreach (var result in dailyScheduleDto.results)
            {
                result.sport_event.scheduled = date;
            }

            return Task.FromResult(dailyScheduleDto);
        }
    }
}