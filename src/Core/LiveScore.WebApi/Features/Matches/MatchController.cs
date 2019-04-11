namespace LiveScore.WebApi.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly MatchService matchService;

        public MatchController(MatchService matchService)
        {
            this.matchService = matchService;
        }

        [HttpGet("GetMatches")]
        public async Task<IEnumerable<Match>> GetMatches(
            int sportId,
            DateTime from,
            DateTime to,
            string language = "en")
        {
            return await matchService.GetMatches(sportId, from, to, language);
        }
    }
}