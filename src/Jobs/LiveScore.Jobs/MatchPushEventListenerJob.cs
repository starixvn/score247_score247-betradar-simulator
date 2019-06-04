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
        private readonly PushHandlerFactory handlerFactory;

        public MatchPushEventListenerJob(IBackgroundJobClient backgroundJob, PushHandlerFactory handlerFactory)
        {
            this.backgroundJob = backgroundJob;
            this.handlerFactory = handlerFactory;
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
            // TODO: generate request for each sport
            // TODO: generate request for regions
            var host = "https://api.sportradar.com";
            var apiSetting = "soccer-xt3/other/stream/events/subscribe?format=json&api_key=";
            var key = "npc9md73nrwhykuepets3nqf";

            var req = (HttpWebRequest)WebRequest.Create($"{host}/{apiSetting}{key}");
            req.AllowAutoRedirect = false;
            WebResponse reply;
            var pushEventHandler = handlerFactory.CreateMatchPushEventHandler(sportId);

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
                await pushEventHandler.OnReceive(pushEvent);
                System.Threading.Thread.Sleep(MillisecondsTimeout);

            } while (pushEvent.Length > 0);

            //TODO write logs for exception

            return true;
        }
    }
}
