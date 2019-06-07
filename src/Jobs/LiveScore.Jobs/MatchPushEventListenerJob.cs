namespace LiveScore.Jobs
{
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId, IMatchPushEventListener pushEventListener);
    }

    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly IBackgroundJobClient backgroundJob;

        public MatchPushEventListenerJob(IBackgroundJobClient backgroundJob)
        {
            this.backgroundJob = backgroundJob;

        }

        public async Task StartListeningPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
        {
            var isFail = await ListenPushEvent(sportId, pushEventListener);

            if (isFail)
            {
                //TODO call fetch live match results job
                backgroundJob.Enqueue(() => StartListeningPushEvent(sportId, pushEventListener));
            }
        }

        private static async Task<bool> ListenPushEvent(byte sportId, IMatchPushEventListener pushEventListener)
            => await pushEventListener.Start(sportId);
    }
}
