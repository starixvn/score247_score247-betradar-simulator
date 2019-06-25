namespace LiveScore.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using Microsoft.AspNetCore.SignalR;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly PushEventListenerFactory pushEventListenerFactory;
        private readonly IHubContext<MatchHub> hubContext;

        public MatchPushEventListenerJob(
            PushEventListenerFactory pushEventListenerFactory, 
            IHubContext<MatchHub> hubContext)
        {
            this.pushEventListenerFactory = pushEventListenerFactory;
            this.hubContext = hubContext;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory.CreatePushEventListener(sportId);

            
            await ListenPushEvent(sportId, pushEventListener);
        }

        private Task ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => pushEventListener.Start(sportId, hubContext);
    }
}