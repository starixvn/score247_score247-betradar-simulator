namespace LiveScore.Jobs
{   
    using System.Threading.Tasks;
    using Hangfire; 
    using LiveScore.Shared.Factories;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private readonly IBackgroundJobClient backgroundJob;
        private readonly PushListenerFactory listenerFactory;

        public MatchPushEventListenerJob(IBackgroundJobClient backgroundJob, PushListenerFactory listenerFactory)
        {
            this.backgroundJob = backgroundJob;
            this.listenerFactory = listenerFactory;
        }

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
            return await listenerFactory.CreatePushEventListener(sportId).Start(sportId);
        }
    }
}
