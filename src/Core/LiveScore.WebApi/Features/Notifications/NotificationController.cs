namespace LiveScore.WebApi.Features.Notifications
{
    using LiveScore.Features.Matches.Models;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly PushService pushService;

        public NotificationController(PushService pushService)
        {
            this.pushService = pushService;
        }

        [HttpPost("PushMessage")]
        public async Task PushMessage(
            string text)
        {
            await pushService.Connect();

            await pushService.Send(text);

            await pushService.Disconnect();
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
