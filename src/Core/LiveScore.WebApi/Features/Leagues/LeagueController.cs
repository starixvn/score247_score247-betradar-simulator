namespace LiveScore.WebApi.Features.Leagues
{
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Features.Matches;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        private readonly ILeagueService leagueService;

        public LeagueController(ILeagueService leagueService)
        {
            this.leagueService = leagueService;
        }

        [HttpGet("GetLeagues")]
        public async Task<IEnumerable<ILeague>> GetLeagues(
            int sportId,
            string language = "en")
        {
            return await leagueService.GetLeagues(sportId, language);
        }
    }
}