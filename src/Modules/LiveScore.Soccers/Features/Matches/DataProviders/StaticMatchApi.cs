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
        private static Random random = new Random();
        private readonly IAppSettings appSettings;

        public StaticMatchApi(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<DailyScheduleDto> GetDailySchedules(
            int sportId,
            DateTime date,
            string language)
        {
            // Temporary for hardcode, please ignore it
            var jsonData = File.ReadAllText($"{appSettings.AppPath}/App_Data/staticdata/daily-schedule.json");
            var dailyScheduleDto = JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData);

            foreach (var result in dailyScheduleDto.results)
            {
                result.sport_event.scheduled = new DateTime(
                    date.Year, 
                    date.Month, 
                    date.Day,
                    result.sport_event.scheduled.Hour,
                    result.sport_event.scheduled.Minute,
                    result.sport_event.scheduled.Second);

                foreach (var competitor in result.sport_event.competitors)
                {
                    competitor.name = $"{competitor.name} - {CreateString(5)}";
                }

                if(date > DateTime.Now.ToUniversalTime())
                {
                    result.sport_event_status.status = "not_started";
                    result.sport_event_status.match_status = "not_started";
                }

                if (date < DateTime.Now.ToUniversalTime().Date)
                {
                    result.sport_event_status.status = "closed";
                    result.sport_event_status.match_status = "ended";
                }
            }

            return Task.FromResult(dailyScheduleDto);
        }

        internal static string CreateString(int stringLength)
        {
            
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}