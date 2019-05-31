namespace LiveScore.Jobs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Hangfire;
    using LiveScore.Shared;

    public interface IMatchPushEventListenerJob
    {
        Task StartListeningPushEvent(byte sportId);
    }

    public class MatchPushEventListenerJob : IMatchPushEventListenerJob
    {
        private const int MillisecondsTimeout = 30 * 1000;
        private readonly IBackgroundJobClient backgroundJob;
        private readonly InstanceFactory instanceFactory;

        public MatchPushEventListenerJob(
            IBackgroundJobClient backgroundJob,
            InstanceFactory instanceFactory)
        {
            this.backgroundJob = backgroundJob;
            this.instanceFactory = instanceFactory;
        }

        public async Task StartListeningPushEvent(byte sportId)
        {
            var isFail = await ListenPushEvent(sportId);

            if (isFail)
            {
                backgroundJob.Enqueue(() => StartListeningPushEvent(sportId));
            }
        }

        private async Task<bool> ListenPushEvent(byte sportId)
        {
            // TODO: generate request for each sport
            var req = (HttpWebRequest)WebRequest.Create("https://api.sportradar.com/soccer-xt3/other/stream/events/subscribe?api_key=npc9md73nrwhykuepets3nqf&format=json");
            req.AllowAutoRedirect = false;
            WebResponse reply;
            var pushEventHandler = instanceFactory.CreatePushEventHandler(sportId);

            try
            {
                reply = (HttpWebResponse)req.GetResponse();

            }
            catch (WebException e)
            {

                reply = e.Response;
                req = (HttpWebRequest)WebRequest.Create(reply.Headers["Location"]);
                req.KeepAlive = true;
                reply = (HttpWebResponse)req.GetResponse();
            }

            var stream = reply.GetResponseStream();
            var reader = new StreamReader(stream);
            var pushEvent = string.Empty;

            do
            {
                pushEvent = reader.ReadLine();
                Debug.WriteLine(pushEvent);
                Console.WriteLine(pushEvent);
                await pushEventHandler.OnReceive(pushEvent);
                System.Threading.Thread.Sleep(MillisecondsTimeout);

            } while (pushEvent.Length > 0);

            return true;
        }
    }
}
