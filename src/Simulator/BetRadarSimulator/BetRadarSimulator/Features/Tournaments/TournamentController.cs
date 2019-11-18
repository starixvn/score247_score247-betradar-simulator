using System;
using System.Threading.Tasks;
using LiveScore.BetRadarSimulator.Api.Shared;
using LiveScore.Providers.SportRadar.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LiveScore.BetRadarSimulator.Api.Features.Tournaments
{
    [ApiController]
    public class TournamentController : Controller
    {
        private readonly ITournamentService tournamentService;
        private readonly IFileService fileService;

        public TournamentController(ITournamentService tournamentService, IFileService fileService)
        {
            this.tournamentService = tournamentService;
            this.fileService = fileService;
        }

        [Route(nameof(TournamentStandings)), HttpPost]
        public async Task<JsonResult> TournamentStandings(TournamentStandingDto tournamentStandingDto)
        {
            var tournamentId = tournamentStandingDto.tournament.id;

            tournamentStandingDto.generated_at = DateTime.Now;

            var path = $"standing/{tournamentId}.json";
            var json = JsonConvert.SerializeObject(tournamentStandingDto);

            await fileService.SaveFile(path, json);

            return Json(new
            {
                message = "OK!",
                data = json
            });
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/tournaments/{tournamentId}/live_standings.json")]
        [HttpGet]
        public Task<TournamentStandingDto> TournamentStandings(string sportType, string accessLevel, string region, string lang, string tournamentId)
        {
            var path = $"standing/{tournamentId}.json";

            return tournamentService.GetTournamentStandings(path);
        }
    }
}