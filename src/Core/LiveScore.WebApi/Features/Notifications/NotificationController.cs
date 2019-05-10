namespace LiveScore.WebApi.Features.Notifications
{    
    using LiveScore.Soccers.Shared.TransportMessages;
    using LiveScore.WebApi.Hubs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
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

        [HttpPost("PushMessage")]
        public async Task PushMessage(
            string text)
        {
            await hubContext.Clients.All.SendAsync("Push", text);
        }

        [HttpPost("PushMatchEvent")]
        public async Task PushMatchEvent(
            MatchTransportMessage message)
        {
            await hubContext.Clients.All.SendAsync("PushMatchEvent", message);
        }
    }
}
