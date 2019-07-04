namespace LiveScore.Soccer.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly Func<int, MatchPushEventListener> pushEventListenerFactory;

        public MatchPushEventListenerJob(Func<int, MatchPushEventListener> pushEventListenerFactory)
        {
            this.pushEventListenerFactory = pushEventListenerFactory;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var pushEventListener = pushEventListenerFactory(sportId);

            await ListenPushEvent(sportId, pushEventListener);
        }

        private static Task ListenPushEvent(byte sportId, MatchPushEventListener pushEventListener)
            => pushEventListener.Start(sportId);
    }
}