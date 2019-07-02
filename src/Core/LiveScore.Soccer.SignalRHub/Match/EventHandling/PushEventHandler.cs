namespace LiveScore.Soccer.SignalRHub.Match.EventHandling
{
    using Fanex.Logging;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Soccer.Shared;
    using Microsoft.AspNetCore.SignalR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IPushEventHandler
    {
        Task Handle(Dictionary<string, MatchPushEvent> pushEvents);
    }
    public class PushEventHandler: IPushEventHandler
    {
        private readonly ILogger logger;
        private readonly IHubContext<MatchHub> hubContext;

        public PushEventHandler(
            ILogger logger,            
            IHubContext<MatchHub> hubContext)
        {
            this.logger = logger;
            this.hubContext = hubContext;
        }

        public async Task Handle(Dictionary<string, MatchPushEvent> pushEvents)
        {
            var firstEvent = pushEvents.First().Value.TimeLines.First();
            await logger.InfoAsync($"{ DateTime.Now.ToString("MMMM dd HH:mm:ss") } Pushing {pushEvents.First().Key} - {firstEvent.Type}");
            await hubContext.Clients.All.SendAsync("PushMatchEvent", SportConstants.SoccerId.ToString(), pushEvents);
        }
    }
}
