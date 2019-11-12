namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.BetRadarSimulator.Api.Shared;
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
        private readonly IFileService fileService;

        public MatchService(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public async Task<DailyResultDto> GetLiveMatches(string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new DailyResultDto { generated_at = DateTime.Now, results = new List<Result>() };
            }
            else
            {
                var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(fileContent);

                resultDto.generated_at = DateTime.Now;

                return resultDto;
            }
        }

        public async Task<LineUpsDto> GetMatchLineups(string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new LineUpsDto { generated_at = DateTime.Now, schema = "", message = "No lineups." };
            }
            else
            {
                var timelineDto = JsonConvert.DeserializeObject<LineUpsDto>(fileContent);
                timelineDto.generated_at = DateTime.Now;

                return timelineDto;
            }
        }

        public async Task<DailyResultDto> GetMatchResults(DateTime date, string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new DailyResultDto { generated_at = date, results = new List<Result>() };
            }
            else
            {
                var resultDto = JsonConvert.DeserializeObject<DailyResultDto>(fileContent);
                foreach (var result in resultDto.results)
                {
                    var startHour = result.sport_event.scheduled.Hour;
                    var startMinute = result.sport_event.scheduled.Minute;

                    result.sport_event.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                }

                return resultDto;
            }
        }

        public async Task<DailyScheduleDto> GetMatchSchedule(DateTime date, string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new DailyScheduleDto { generated_at = date, sport_events = new List<Sport_Event>() };
            }
            else
            {
                var scheduleDto = JsonConvert.DeserializeObject<DailyScheduleDto>(fileContent);

                foreach (var sportEvent in scheduleDto.sport_events)
                {
                    var startHour = sportEvent.scheduled.Hour;
                    var startMinute = sportEvent.scheduled.Minute;

                    sportEvent.scheduled = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0);
                }

                return scheduleDto;
            }
        }

        public async Task<MatchTimelineDto> GetTimeline(string matchId, string path)
        {
            var fileContent = await fileService.GetFile(path);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return new MatchTimelineDto { generated_at = DateTime.Now, sport_event = new Sport_Event { id = matchId } };
            }
            else
            {
                var timelineDto = JsonConvert.DeserializeObject<MatchTimelineDto>(fileContent);

                timelineDto.generated_at = DateTime.Now;
                timelineDto.sport_event.id = matchId;

                return timelineDto;
            }
        }
    }
}