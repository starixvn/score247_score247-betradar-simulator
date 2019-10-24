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
        Task<MatchTimelineDto> GetTimeline(string matchId, string path);

        Task<DailyResultDto> GetLiveMatches(string path);

        Task<DailyScheduleDto> GetMatchSchedule(DateTime date, string path);

        Task<DailyResultDto> GetMatchResults(DateTime date, string path);

        Task<LineUpsDto> GetMatchLineups(string path);
    }

    public class MatchService : IMatchService
    {
        public async Task<DailyResultDto> GetLiveMatches(string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                resultDto.generated_at = DateTime.Now;

                return resultDto;
            }
            else
            {
                return new DailyResultDto { generated_at = DateTime.Now, results = new List<Result>() };
            }
        }

        public async Task<LineUpsDto> GetMatchLineups(string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var timelineDto = JsonConvert.DeserializeObject<LineUpsDto>(jsonData);
                timelineDto.generated_at = DateTime.Now;

                return timelineDto;
            }
            else
            {
                return new LineUpsDto { generated_at = DateTime.Now, schema = "", message = "No lineups." };
            }
        }

        public async Task<DailyResultDto> GetMatchResults(DateTime date, string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                foreach (var result in resultDto.results)
                {
                    var startHour = result.sport_event.scheduled.Hour;
                    var startMinute = result.sport_event.scheduled.Minute;

                    result.sport_event.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                }

                return resultDto;
            }
            else
            {
                return new DailyResultDto { generated_at = date, results = new List<Result>() };
            }
        }

        public async Task<DailyScheduleDto> GetMatchSchedule(DateTime date, string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var scheduleDto = JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData);

                foreach (var sportEvent in scheduleDto.sport_events)
                {
                    var startHour = sportEvent.scheduled.Hour;
                    var startMinute = sportEvent.scheduled.Minute;

                    sportEvent.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                }

                return scheduleDto;
            }
            else
            {
                return new DailyScheduleDto { generated_at = date, sport_events = new List<Sport_Event>() };
            }
        }

        public async Task<MatchTimelineDto> GetTimeline(string matchId, string path)
        {
            if (File.Exists(path))
            {
                var jsonData = await File.ReadAllTextAsync(path);
                var timelineDto = JsonConvert.DeserializeObject<MatchTimelineDto>(jsonData);

                timelineDto.generated_at = DateTime.Now;
                timelineDto.sport_event.id = matchId;

                return timelineDto;
            }
            else
            {
                //var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/match-timelines.json";
                return new MatchTimelineDto { generated_at = DateTime.Now, sport_event = new Sport_Event { id = matchId } };
            }
        }
    }
}