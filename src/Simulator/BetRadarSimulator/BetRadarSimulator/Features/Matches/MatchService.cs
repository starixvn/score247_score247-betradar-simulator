namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using Newtonsoft.Json;


    public interface IMatchService
    {
        Task<MatchTimelineDto> GetTimeline(string matchId);

        Task<DailyResultDto> GetLiveMatches();

        Task<DailyScheduleDto> GetMatchSchedule(DateTime date);

        Task<DailyResultDto> GetMatchResults(DateTime date);
    }

    public class MatchService : IMatchService
    {
        public async Task<DailyResultDto> GetLiveMatches()
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/live-results.json";
            var jsonData = await File.ReadAllTextAsync(path);
            var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

            resultDto.generated_at = DateTime.Now;

            var index = 0;

            foreach (var result in resultDto.results)
            {
                index++;

                var matchId = $"auto:match{DateTime.Now.Month}{DateTime.Now.Day}:1000{index}";
                result.sport_event.id = matchId;
            }

            return resultDto;
        }

        public async Task<DailyResultDto> GetMatchResults(DateTime date)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/results.json";
            var jsonData = await File.ReadAllTextAsync(path);

            if (date < DateTime.Now)
            {
                var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);
                var index = 0;

                foreach (var result in resultDto.results)
                {
                    index++;

                    var matchId = $"auto:match{date.Month}{date.Day}:5000{index}";
                    var startHour = result.sport_event.scheduled.Hour;
                    var startMinute = result.sport_event.scheduled.Minute;

                    result.sport_event.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                    result.sport_event.id = matchId;
                }

                return resultDto;
            }
            else
            {
                return new DailyResultDto { generated_at = date, results = new List<Result>() };
            }
        }

        public async Task<DailyScheduleDto> GetMatchSchedule(DateTime date)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/schedule.json";
            var jsonData = await File.ReadAllTextAsync(path);
            var scheduleDto = JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData);
            var index = 0;

            foreach (var sportEvent in scheduleDto.sport_events)
            {
                index++;
                var matchId = $"auto:match{date.Month}{date.Day}:1000{index}";
                var startHour = sportEvent.scheduled.Hour;
                var startMinute = sportEvent.scheduled.Minute;

                sportEvent.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                sportEvent.id = matchId;
            }

            return scheduleDto;
        }

        public async Task<MatchTimelineDto> GetTimeline(string matchId)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/match-timelines.json";
            var jsonData = await File.ReadAllTextAsync(path);
            var timelineDto = JsonConvert.DeserializeObject<MatchTimelineDto>(jsonData);

            timelineDto.generated_at = DateTime.Now;
            timelineDto.sport_event.id = matchId;

            return timelineDto;
        }
    }
}
