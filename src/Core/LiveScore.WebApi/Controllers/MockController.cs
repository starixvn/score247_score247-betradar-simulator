namespace LiveScore.Soccer.Api.Notifications
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Odds;
    using LiveScore.Providers.SportRadar.Dtos;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer.Matches.Handlers;
    using LiveScore.Soccers.Shared.ModelBuilders;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MockController : ControllerBase
    {
        private readonly MatchPushEventHandler soccerPushEventHandler;
        private readonly MatchService matchService;
        private readonly OddsService oddsService;

        public MockController(
            MatchPushEventHandler soccerPushEventHandler,
            MatchService matchService,
            OddsService oddsService)
        {
            this.soccerPushEventHandler = soccerPushEventHandler;
            this.matchService = matchService;
            this.oddsService = oddsService;
        }

        [HttpPost(nameof(PushMatchEvent))]
        public async Task PushMatchEvent(
            [FromBody]MatchPushEventDto matchPushEventDto)
        {
            await soccerPushEventHandler.OnReceive(matchPushEventDto);
        }

        [HttpPost(nameof(PushMatchEvents))]
        public async Task PushMatchEvents(
            [FromBody]IEnumerable<MatchPushEventDto> matchPushEventDtos)
        {
            foreach (var matchPushEventDto in matchPushEventDtos)
            {
                await soccerPushEventHandler.OnReceive(matchPushEventDto);
            }
        }

        [HttpGet(nameof(AddFullTimeline))]
        public async Task<string> AddFullTimeline(int sportId, string matchId, string language = "en", string region = "eu")
        {
            await matchService.AddFullTimelines(sportId, matchId, language, region);

            return "Done";
        }

        [HttpPost(nameof(InsertOdds))]
        public async Task InsertOdds(
            [FromBody]OddsScheduleDto oddsSchedule)
        {
            var matchOddsList = OddsBuilder.BuildMatchOddsList(oddsSchedule);

            foreach (var matchOdds in matchOddsList)
            {
                await oddsService.InsertBetTypeOdds(matchOdds.MatchId, matchOdds.BetTypeOddsList);
                //oddsService.InsertBetTypeOdds()
            }
        }

        ////[HttpPost(nameof(AddResult))]
        ////public async Task<string> AddResult(int sportId, string matchId, [FromBody]MatchResult matchResult)
        ////{
        ////    await matchService.AddResult(sportId, matchId, matchResult);

        ////    return "Done";
        ////}
    }
}