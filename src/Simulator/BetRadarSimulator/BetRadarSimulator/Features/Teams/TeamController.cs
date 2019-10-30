namespace LiveScore.BetRadarSimulator.Api.Features.Teams
{
    using LiveScore.Providers.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    public class TeamController : Controller
    {
        private readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";

        private readonly ITeamService teamService;

        public TeamController(ITeamService teamService)
        {
            this.teamService = teamService;
        }

        [Route(nameof(H2H)), HttpPost]
        public async Task<JsonResult> H2H(H2HDto h2HDto)
        {
            var teamIds = h2HDto.teams.Select(team => team.id).OrderBy(teamId => teamId).ToList();
            var teamIdsFilename = string.Join("vs", teamIds).Replace(':', '-');
            Directory.CreateDirectory($"{DirectoryRoot}/h2h");
            var h2hFolder = $"{DirectoryRoot}/h2h/{teamIdsFilename}";

            h2HDto.generated_at = DateTime.Now;

            var path = $"{h2hFolder}/h2h.json";
            var json = JsonConvert.SerializeObject(h2HDto);

            Directory.CreateDirectory(h2hFolder);
            await System.IO.File.WriteAllTextAsync(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/teams/{homeId}/versus/{awayId}/matches.json")]
        [HttpGet]
        public Task<H2HDto> H2H(string sportType, string accessLevel, string region, string lang, string homeId, string awayId)
        {
            var teamIds = (new List<string> { homeId, awayId }).OrderBy(teamId => teamId);
            var h2hTeamIds = string.Join("vs", teamIds).Replace(':', '-');
            var path = $"{DirectoryRoot}/h2h/{h2hTeamIds}/h2h.json";

            return teamService.GetH2H(path);
        }

        [Route(nameof(Results)), HttpPost]
        public async Task<JsonResult> TeamResults(TeamResultDto teamResultDto)
        {
            var teamId = teamResultDto.team.id;
            var teamIdFilename = teamId.Replace(':', '-');
            Directory.CreateDirectory($"{DirectoryRoot}/teams");
            var teamFolder = $"{DirectoryRoot}/teams/{teamIdFilename}";

            teamResultDto.generated_at = DateTime.Now;

            var path = $"{teamFolder}/results.json";
            var json = JsonConvert.SerializeObject(teamResultDto);

            Directory.CreateDirectory(teamFolder);
            await System.IO.File.WriteAllTextAsync(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/teams/{teamId}/results.json")]
        [HttpGet]
        public Task<TeamResultDto> TeamResult(string sportType, string accessLevel, string region, string lang, string teamId)
        {
            var teamFilename = teamId.Replace(':', '-');
            var path = $"{DirectoryRoot}/teams/{teamFilename}/results.json";

            return teamService.GetResults(path);
        }
    }
}