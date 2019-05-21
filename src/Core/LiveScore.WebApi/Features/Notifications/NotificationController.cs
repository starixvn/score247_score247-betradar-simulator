namespace LiveScore.WebApi.Features.Notifications
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<MatchHub> hubContext;
        private readonly ICacheService cacheService;
        private readonly IMatchService matchService;

        public NotificationController(
            IHubContext<MatchHub> hubContext,
            ICacheService cacheService,
            IMatchService matchService)
        {
            this.hubContext = hubContext;
            this.cacheService = cacheService;
            this.matchService = matchService;
        }

        [HttpPost("PushMatchEvent")]
        public async Task PushMatchEvent(
            int sportId,
            [FromBody]Dictionary<string, MatchPayload> payloads)
        {
            if (sportId > 0 && payloads != null)
            {
                foreach (var matchId in payloads.Keys)
                {
                    var payload = payloads[matchId];

                    if (payload.MatchResult.EventStatus.IsLive())
                    {
                        var timeline = payload.TimeLines.LastOrDefault();
                        var matchTime = timeline.MatchTime;

                        if (timeline.Type == EventTypes.PeriodStart.Value)
                        {
                            if (timeline.Period == 1)
                            {
                                matchTime = 1;
                            }
                            else
                            {
                                matchTime = 46;
                            }
                        }

                        payload.MatchResult.MatchTime = matchTime;
                    }

                    await matchService.AddTimelines(sportId, matchId, payload.TimeLines);
                    await matchService.AddResult(sportId, matchId, payload.MatchResult);
                }

                await hubContext.Clients.All.SendAsync("PushMatchEvent", sportId, payloads);
            }
        }
    }
}
