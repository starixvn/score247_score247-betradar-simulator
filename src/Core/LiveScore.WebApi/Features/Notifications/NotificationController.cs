namespace LiveScore.WebApi.Features.Notifications
{
    using LiveScore.Features.Matches.Models;
    using LiveScore.WebApi.Hubs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly PushService pushService;
        private readonly IHubContext<MatchHub> hubContext;

        public NotificationController(PushService pushService, IHubContext<MatchHub> hubContext)
        {
            this.pushService = pushService;
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
            string matchId, 
            TimeLine timelineEvent)
        {
            await pushService.Connect();

            await pushService.Send(matchId, timelineEvent);

            await pushService.Disconnect();
        }
    }
}
