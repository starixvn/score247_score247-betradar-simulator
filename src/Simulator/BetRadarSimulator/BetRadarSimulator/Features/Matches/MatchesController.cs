namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;


    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService matchService;

        public MatchesController(IMatchService matchService)
        {
            this.matchService = matchService;
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/matches/{matchId}/timeline.json")]
        [HttpGet]
        public Task<MatchTimelineDto> Timeline(string sportType, string accessLevel, string region, string lang, string matchId)
        {
            return matchService.GetTimeline(matchId);
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/live/results.json")]
        [HttpGet]
        public Task<DailyResultDto> LiveMatches(string sportType, string accessLevel, string region, string lang)
        {
            return matchService.GetLiveMatches();
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/{date}/schedule.json")]
        [HttpGet]
        public Task<DailyScheduleDto> DailySchedule(string sportType, string accessLevel, string region, string lang, DateTime date)
        {
            return matchService.GetMatchSchedule(date);
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/{date}/results.json")]
        [HttpGet]
        public Task<DailyResultDto> DailyResults(string sportType, string accessLevel, string region, string lang, DateTime date)
        {
            return matchService.GetMatchResults(date);
        }
    }
}