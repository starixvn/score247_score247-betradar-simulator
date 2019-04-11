namespace LiveScore.WebApi.Features.Leagues
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Leagues;
    using LiveScore.Features.Leagues.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        private readonly LeagueService leagueService;

        public LeagueController(LeagueService leagueService)
        {
            this.leagueService = leagueService;
        }

        [HttpGet("GetLeaguesByDate")]
#pragma warning disable RECS0154 // Parameter is never used
        public async Task<IEnumerable<ILeague>> GetLeaguesByDate(int sportId, DateTime from, DateTime to, string language)
#pragma warning restore RECS0154 // Parameter is never used
        {
            return await leagueService.GetLeagues(1, DateTime.Now, DateTime.Now.AddDays(1));
        }

        [HttpGet("GetLeagues")]
#pragma warning disable RECS0154 // Parameter is never used
        public async Task<IEnumerable<ILeague>> GetLeagues(int sportId, string language)
#pragma warning restore RECS0154 // Parameter is never used
        {
            return await leagueService.GetLeagues(1, DateTime.Now, DateTime.Now.AddDays(1));
        }
    }
}