namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using LiveScore.BetRadarSimulator.Api.Shared;
    using LiveScore.Providers.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    [ApiController]
    public class MatchesController : Controller
    {
        private const string FormatDate = "MM-dd-yyyy";

        private readonly IMatchService matchService;
        private readonly IFileService fileService;

        public MatchesController(IMatchService matchService, IFileService fileService)
        {
            this.matchService = matchService;
            this.fileService = fileService;
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/matches/{matchId}/timeline.json")]
        [HttpGet]
        public Task<MatchTimelineDto> Timeline(string sportType, string accessLevel, string region, string lang, string matchId)
        {
            var formattedMatchId = matchId.Replace(':', '-');
            var path = $"timelines/{formattedMatchId}/match-timelines.json";

            return matchService.GetTimeline(matchId, path);
        }

        [Route(nameof(Timelines)), HttpPost]
        public async Task<JsonResult> Timelines(string matchId, MatchTimelineDto resultDto)
        {
            var formattedMatchId = matchId.Replace(':', '-');
            var resultFolder = $"timelines/{formattedMatchId}";

            resultDto.generated_at = DateTime.Now;

            var path = $"{resultFolder}/match-timelines.json";
            var json = JsonConvert.SerializeObject(resultDto);

            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/live/results.json")]
        [HttpGet]
        public Task<DailyResultDto> LiveResults(string sportType, string accessLevel, string region, string lang)
        {
            var path = $"live/{DateTime.Now.ToString(FormatDate)}.json";

            return matchService.GetLiveMatches(path);
        }

        [Route(nameof(LiveResults)), HttpPost]
        public async Task<JsonResult> LiveResults(DailyResultDto resultDto)
        {
            var resultFolder = $"live";
            resultDto.generated_at = DateTime.Now;

            var path = $"{resultFolder}/{DateTime.Now.ToString(FormatDate)}.json";
            var json = JsonConvert.SerializeObject(resultDto);

            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/{date}/schedule.json")]
        [HttpGet]
        public Task<DailyScheduleDto> DailySchedule(string sportType, string accessLevel, string region, string lang, DateTime date)
        {
            var path = $"schedule/{date.ToString(FormatDate)}.json";

            return matchService.GetMatchSchedule(date, path);
        }

        [Route(nameof(DailySchedule)), HttpPost]
        public async Task<JsonResult> DailySchedule(DateTime date, DailyScheduleDto scheduleDto)
        {
            var scheduleFolder = $"schedule";
            scheduleDto.generated_at = DateTime.Now;

            var path = $"{scheduleFolder}/{date.ToString(FormatDate)}.json";
            var json = JsonConvert.SerializeObject(scheduleDto);
            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/schedules/{date}/results.json")]
        [HttpGet]
        public Task<DailyResultDto> DailyResults(string sportType, string accessLevel, string region, string lang, DateTime date)
        {
            var path = $"results/{date.ToString(FormatDate)}.json";

            return matchService.GetMatchResults(date, path);
        }

        [Route(nameof(DailyResults)), HttpPost]
        public async Task<JsonResult> DailyResults(DateTime date, DailyResultDto resultDto)
        {
            var resultFolder = $"results";
            System.IO.Directory.CreateDirectory(resultFolder);
            resultDto.generated_at = DateTime.Now;

            var path = $"{resultFolder}/{date.ToString(FormatDate)}.json";
            var json = JsonConvert.SerializeObject(resultDto);

            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route(nameof(Lineups)), HttpPost]
        public async Task<JsonResult> Lineups(LineUpsDto lineUpsDto)
        {
            var matchId = lineUpsDto.sport_event.id;
            var formattedMatchId = matchId.Replace(':', '-');
            var lineupsFolder = $"lineups/{formattedMatchId}";

            lineUpsDto.generated_at = DateTime.Now;

            var path = $"{lineupsFolder}/match-lineups.json";
            var json = JsonConvert.SerializeObject(lineUpsDto);

            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/matches/{matchId}/lineups.json")]
        [HttpGet]
        public Task<LineUpsDto> LineUps(string sportType, string accessLevel, string region, string lang, string matchId)
        {
            var formattedMatchId = matchId.Replace(':', '-');
            var path = $"lineups/{formattedMatchId}/match-lineups.json";

            return matchService.GetMatchLineups(path);
        }

        [Route(nameof(CleanMatch)), HttpPost]
        public OkResult CleanMatch()
        {
            var folders = new[] {
                $"results",
                $"schedule"
            };

            foreach (var folder in folders)
            {
                if (System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }

            return Ok();
        }

        [Route(nameof(CleanTimeline)), HttpPost]
        public OkResult CleanTimeline()
        {
            var folders = new[] {
                $"timelines",
                $"lineups",
                $"h2h"
            };

            foreach (var folder in folders)
            {
                if (System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }

            return Ok();
        }
    }
}