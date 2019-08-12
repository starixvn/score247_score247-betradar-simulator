namespace LiveScore.BetRadarSimulator.Api.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [ApiController]
    public class OddsController : Controller
    {
        private const string FormatDate = "MM-dd-yyyy";
        private static object ReadOddsByMatchIdLockObject = new object();

        [Route("~/api/oddscomparison-rowt1/en/eu/sports/sr:sport:1/{scheduleDate}/schedule.json"), HttpGet]
        public async Task<OddsScheduleDto> ScheduleOdds(DateTime scheduleDate)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{scheduleDate.ToString(FormatDate)}.json";

            return await ReadAndDeserializeJson(path);
        }

        [Route("~/api/oddscomparison-rowt1/en/eu/sport_events/{unixTimeStamp}/changelog.json"), HttpGet]
        public async Task<OddsScheduleDto> OddChangeLog(long unixTimeStamp)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{DateTime.Now.ToString(FormatDate)}-odds-changes.json";

            return await ReadAndDeserializeJson(path);
        }

        [Route("~/api/oddscomparison-rowt1/en/eu/sport_events/{matchId}/markets.json"), HttpGet]
        public async Task<MatchOddsDto> MatchOdds(string matchId)
        {
            try
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{DateTime.Now.ToString(FormatDate)}-match-{matchId.Replace(":", "_")}-odds.json";

                var oddsScheduleDto = await ReadAndDeserializeJson(path);
                var matchOddsDto = new MatchOddsDto();

                if (oddsScheduleDto.sport_events != null
                    && oddsScheduleDto.sport_events.Any())
                {
                    var firstOddsByMatch = oddsScheduleDto
                        .sport_events
                        .Where(se => se.id == matchId)
                        .OrderBy(se => se.markets_last_updated)
                        .FirstOrDefault();

                    if (firstOddsByMatch != null)
                    {

                        matchOddsDto.sport_event = firstOddsByMatch;

                        oddsScheduleDto.sport_events.RemoveAll(sp => sp.id == matchId && sp.markets_last_updated == firstOddsByMatch.markets_last_updated);
                    }
                }

                await SaveAndReturnOKMessage(oddsScheduleDto, path);

                return matchOddsDto;
            }
            catch(Exception ex)
            {
                return new MatchOddsDto();
            }
        }

        private static async Task<OddsScheduleDto> ReadAndDeserializeJson(string path)
        {
            if (System.IO.File.Exists(path))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(path);

                return JsonConvert.DeserializeObject<OddsScheduleDto>(jsonData);
            }

            return new OddsScheduleDto();
        }

        [Route(nameof(MatchOdds)), HttpPost]
        public async Task<JsonResult> MatchOdds(MatchOddsDto matchOddsDto)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{DateTime.Now.ToString(FormatDate)}-match-{matchOddsDto.sport_event.id.Replace(":", "_")}-odds.json";
            var oddsScheduleDto = new OddsScheduleDto
            {
                sport_events = new List<SportEvent> { matchOddsDto.sport_event }
            };

            if (System.IO.File.Exists(path))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(path);

                var oldData = JsonConvert.DeserializeObject<OddsScheduleDto>(jsonData);

                oddsScheduleDto.sport_events.AddRange(oldData.sport_events);
            }

            CreateOddsFolder();

            return await SaveAndReturnOKMessage(oddsScheduleDto, path);
        }

        private async Task<JsonResult> SaveAndReturnOKMessage(OddsScheduleDto oddsScheduleDto, string path)
        {
            CreateOddsFolder();
            var json = JsonConvert.SerializeObject(oddsScheduleDto);
            await System.IO.File.WriteAllTextAsync(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route(nameof(OddsChange)), HttpPost]
        public async Task<JsonResult> OddsChange(OddsScheduleDto oddsScheduleDto)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{DateTime.Now.ToString(FormatDate)}-odds-changes.json";

            if (System.IO.File.Exists(path))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(path);

                var oldData = JsonConvert.DeserializeObject<OddsScheduleDto>(jsonData);

                oddsScheduleDto.sport_events.AddRange(oldData.sport_events);
            }

            CreateOddsFolder();

            return await SaveAndReturnOKMessage(oddsScheduleDto, path);
        }

        [Route(nameof(ScheduleOdds)), HttpPost]
        public async Task<JsonResult> ScheduleOdds(OddsScheduleDto oddsScheduleDto)
        {
            var oddsFolder = CreateOddsFolder();

            var eventDate = oddsScheduleDto.sport_events.FirstOrDefault()?.scheduled;

            if (eventDate.HasValue)
            {
                var path = $"{oddsFolder}/{eventDate.Value.ToString(FormatDate)}.json";

                return await SaveAndReturnOKMessage(oddsScheduleDto, path);
            }

            return Json(new
            {
                message = "Not Found Event Date"
            });
        }

        private static string CreateOddsFolder()
        {
            var oddsFolder = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/odds";
            System.IO.Directory.CreateDirectory(oddsFolder);
            return oddsFolder;
        }
    }
}