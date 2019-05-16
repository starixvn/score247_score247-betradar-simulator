namespace LiveScore.WebApi.Features.Notifications
{
    using System.Collections.Generic;
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

        public NotificationController(IHubContext<MatchHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpPost("PushMatchEvent")]
        public async Task PushMatchEvent(
            int sportId,
            Dictionary<string, MatchPayload> payloads)
        {
            if (sportId > 0 && payloads != null)
            {
                await hubContext.Clients.All.SendAsync("PushMatchEvent", sportId, payloads);
            }
        }
    }
}
