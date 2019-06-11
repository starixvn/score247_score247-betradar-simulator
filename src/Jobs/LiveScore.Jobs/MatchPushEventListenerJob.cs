using LiveScore.Shared;
namespace LiveScore.Jobs
{
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
        private readonly IBackgroundJobClient backgroundJob;
        private readonly PushEventListenerFactory pushEventListenerFactory;
        private readonly IHubContext<MatchHub> hubContext;

        public MatchPushEventListenerJob(IBackgroundJobClient backgroundJob, PushEventListenerFactory pushEventListenerFactory, IHubContext<MatchHub> hubContext)
        {
            this.backgroundJob = backgroundJob;
            this.pushEventListenerFactory = pushEventListenerFactory;
            this.hubContext = hubContext;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory.CreatePushEventListener(sportId);
            var isFail = await ListenPushEvent(sportId, pushEventListener);

            if (isFail)
            {
                //TODO call fetch live match results job after retry failure job
                backgroundJob.Enqueue(() => StartListeningPushEvent(sportId));
            }
        }

        private async Task<bool> ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => await pushEventListener.Start(sportId, hubContext);
    }
}
