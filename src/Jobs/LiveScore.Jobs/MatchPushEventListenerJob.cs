namespace LiveScore.Jobs
{
    using LiveScore.Shared;
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {        
        private readonly PushEventListenerFactory pushEventListenerFactory;
        private readonly IHubContext<MatchHub> hubContext;

        public MatchPushEventListenerJob(PushEventListenerFactory pushEventListenerFactory, IHubContext<MatchHub> hubContext)
        {
            
            this.pushEventListenerFactory = pushEventListenerFactory;
            this.hubContext = hubContext;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory.CreatePushEventListener(sportId);

            //TODO call fetch live match results job after retry failure job when ListenPushEvent was force stop
            await ListenPushEvent(sportId, pushEventListener);            
        }

        private async Task<bool> ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => await pushEventListener.Start(sportId, hubContext);
    }
}
