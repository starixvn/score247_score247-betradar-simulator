namespace LiveScore.Soccers.Features.Leagues
{
    using System.IO;
    using System.Threading.Tasks;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Models.Dtos;
    using Newtonsoft.Json;

    public class StaticLeagueApi : ILeagueApi
    {
        private readonly IAppSettings appSettings;

        public StaticLeagueApi(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<DailyScheduleDto> GetDailySchedules(
            string sportName,
            string group,
            string lang,
            string date,
            string key)
        {
            // Temporary for hardcode, please ignore it
            var jsonData = File.ReadAllText($"{appSettings.AppPath}/App_Data/staticdata/daily-schedule.json");

            return Task.FromResult(JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData));
        }
    }
}