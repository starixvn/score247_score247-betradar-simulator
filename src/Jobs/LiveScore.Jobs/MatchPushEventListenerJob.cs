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

    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 1)]
    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly PushEventListenerFactory pushEventListenerFactory;

        public MatchPushEventListenerJob(PushEventListenerFactory pushEventListenerFactory)
        {
            this.pushEventListenerFactory = pushEventListenerFactory;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory.CreatePushEventListener(sportId);

            
            var isFail = await ListenPushEvent(sportId, pushEventListener);

            if (isFail)
            {
                //TODO call fetch live match results job after retry failure job when ListenPushEvent was force stop
                BackgroundJob.Schedule<IMatchPushEventListenerJob>(job => job.StartListeningPushEvent(SportConstants.SoccerId), TimeSpan.FromSeconds(5));

                throw new InvalidOperationException("Push feed was force closed!");                
            }
        }

        private async Task<bool> ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => await pushEventListener.Start(sportId);
    }
}