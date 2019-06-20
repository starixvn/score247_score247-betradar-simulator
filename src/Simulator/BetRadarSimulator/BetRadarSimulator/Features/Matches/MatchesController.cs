namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/soccer-t3/other/en/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService matchService;

        public MatchesController(IMatchService matchService)
        {
            this.matchService = matchService;
        }

        [HttpGet(nameof(Timeline))]
        public Task<MatchTimelineDto> Timeline(string matchId)
        {
            return matchService.GetTimeline(matchId);
        }
    }
}