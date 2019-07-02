namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System.Threading.Tasks;
    using LiveScore.SportRadar.Dtos;
    using Microsoft.AspNetCore.Mvc;


    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService matchService;

        public MatchesController(IMatchService matchService)
        {
            this.matchService = matchService;
        }

        [Route("~/api/{sportType}-{accessLevel}/{region}/{lang}/matches/{matchId}/timeline/")]
        [HttpGet(nameof(Timeline))]
        public Task<MatchTimelineDto> Timeline(string sportType, string accessLevel, string region, string lang, string matchId)
        {
            return matchService.GetTimeline(matchId);
        }
    }
}