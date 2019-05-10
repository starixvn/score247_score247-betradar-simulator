namespace LiveScore.WebApi.Features.Notifications
{
    using LiveScore.Features.Matches.Models;
    using LiveScore.WebApi.Hubs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
            IEnumerable<Dictionary<string, MatchPayload>> payloads)
        {
            await hubContext.Clients.All.SendAsync("PushMatchEvent", sportId, payloads);
        }
    }
}
