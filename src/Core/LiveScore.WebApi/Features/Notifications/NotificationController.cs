using Fanex.Caching;
namespace LiveScore.WebApi.Features.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

        public NotificationController(IHubContext<MatchHub> hubContext, ICacheService cacheService)
        {
            this.hubContext = hubContext;
            this.cacheService = cacheService;
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
                    if (payloads[matchId].MatchResult.EventStatus.IsLive())
                    {
                        var cacheKey = "MatchTime-" + matchId;
                        cacheService.Set(cacheKey, payloads[matchId].TimeLines.FirstOrDefault().MatchTime);
                    }
                }

                await hubContext.Clients.All.SendAsync("PushMatchEvent", sportId, payloads);
            }
        }
    }
}
