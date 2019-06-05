namespace LiveScore.Jobs
{
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Features.Matches;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly IBackgroundJobClient backgroundJob;

        public MatchPushEventListenerJob(IBackgroundJobClient backgroundJob, IMatchPushEventListener pushEventListener)
        {
            this.backgroundJob = backgroundJob;
            PushEventListener = pushEventListener;
        }

        public IMatchPushEventListener PushEventListener { get; set; }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var isFail = await ListenPushEvent(sportId);

            if (isFail)
            {
                //TODO call fetch live match results job
                backgroundJob.Enqueue(() => StartListeningPushEvent(sportId));
            }
        }

        private async Task<bool> ListenPushEvent(byte sportId)
        {
            return await PushEventListener.Start(sportId);
        }
    }
}
