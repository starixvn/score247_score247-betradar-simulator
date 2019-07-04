namespace LiveScore.Soccer.Api.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer.Matches.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly MatchService matchService;

        public MatchController(
            MatchService matchService)
        {
            this.matchService = matchService;
        }

        [HttpGet(nameof(GetMatches))]
        public async Task<IEnumerable<Match>> GetMatches(
            int sportId,
            DateTime from,
            DateTime to,
            TimeSpan timeZone,
            string language = "en-US")
            => await matchService.GetMatches(sportId, from, to, timeZone, language);

        [HttpGet(nameof(GetMatch))]
        public async Task<Match> GetMatch(
            int sportId,
            string matchId,
            TimeSpan timeZone,
            string language = "en-US")
        {
            var match = await matchService.GetMatch(sportId, matchId, language, timeZone);

            if (match == null)
            {
                throw new InvalidOperationException($"Not found {matchId}");
            }

            return match;
        }
    }
}