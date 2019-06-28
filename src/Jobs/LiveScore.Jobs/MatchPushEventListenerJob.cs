namespace LiveScore.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly Func<int, IMatchPushEventListener> pushEventListenerFactory;

        public MatchPushEventListenerJob(Func<int, IMatchPushEventListener> pushEventListenerFactory)
        {
            this.pushEventListenerFactory = pushEventListenerFactory;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory(sportId);

            await ListenPushEvent(sportId, pushEventListener);
        }

        private static Task ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => pushEventListener.Start(sportId);
    }
}