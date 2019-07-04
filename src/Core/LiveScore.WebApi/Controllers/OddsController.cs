namespace LiveScore.WebApi.Features.Odds
{
    using System.Threading.Tasks;
    using LiveScore.Features.Odds;
    using LiveScore.Features.Odds.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/soccer/[controller]")]
    [ApiController]
    public class OddsController : ControllerBase
    {
        private readonly OddsService oddsService;

        public OddsController(
            OddsService oddsService)
        {
            this.oddsService = oddsService;
        }

        [HttpGet(nameof(Get))]
        public async Task<MatchOdds> Get(string matchId, int betTypeId)
            => await oddsService.GetOdds(matchId, betTypeId);
    }
}