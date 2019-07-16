namespace LiveScore.WebApi.Features.Odds
{
    using System.Threading.Tasks;
    using LiveScore.Features.Odds;
    using LiveScore.Features.Odds.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/soccer/")]
    [ApiController]
    public class OddsController : ControllerBase
    {
        private readonly OddsService oddsService;

        public OddsController(
            OddsService oddsService)
        {
            this.oddsService = oddsService;
        }

        [HttpGet("{lang}/odds/{matchId}/{betTypeId}")]
        public async Task<MatchOdds> Get(string matchId, int betTypeId, string lang = "en-US")
            => await oddsService.GetOdds(matchId, betTypeId);

        [HttpGet("{lang}/odds-movement/{matchId}/{betTypeId}/{format}")]
        public async Task<MatchOdds> OddsMovement(string matchId, int betTypeId, string lang = "en-US", string format = "dec")
            => await oddsService.GetOdds(matchId, betTypeId);
    }
}