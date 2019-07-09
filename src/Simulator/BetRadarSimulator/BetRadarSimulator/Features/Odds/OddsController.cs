namespace LiveScore.BetRadarSimulator.Api.Features.Odds
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [ApiController]
    public class OddsController : Controller
    {
        private const string FormatDate = "MM-dd-yyyy";

        [Route("~/api/oddscomparison-rowt1/en/eu/sports/sr:sport:1/{scheduleDate}/schedule.json"), HttpGet]
        public async Task<OddsScheduleDto> ScheduleOdds(DateTime scheduleDate)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/odds/{scheduleDate.ToString(FormatDate)}.json";

            if (System.IO.File.Exists(path))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(path);

                return JsonConvert.DeserializeObject<OddsScheduleDto>(jsonData);
            }

            return new OddsScheduleDto();
        }

        [Route(nameof(ScheduleOdds)), HttpPost]
        public async Task<JsonResult> ScheduleOdds(OddsScheduleDto oddsScheduleDto)
        {
            var oddsFolder = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/odds";
            var eventDate = oddsScheduleDto.sport_events.FirstOrDefault()?.scheduled;

            if (eventDate.HasValue)
            {
                var path = $"{oddsFolder}/{eventDate.Value.ToString(FormatDate)}.json";
                var json = JsonConvert.SerializeObject(oddsScheduleDto);

                System.IO.Directory.CreateDirectory(oddsFolder);
                await System.IO.File.WriteAllTextAsync(path, json);

                return Json(new
                        {
                            message = "OK!",
                            data = json
                        });
            }

            return Json(new
                {
                    message = "Not Found Event Date"
                });
        }
    }
}