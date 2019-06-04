namespace LiveScore.Soccers.Features.Matches
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;

    public class MatchPushEventListener : IMatchPushEventListener
    {

        private const int MillisecondsTimeout = 5 * 1000;        
        private readonly PushHandlerFactory handlerFactory;

        public MatchPushEventListener(PushHandlerFactory handlerFactory)
        {            
            this.handlerFactory = handlerFactory;
        }


        public async Task<bool> Start(byte sportId)
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
                await Task.Delay(MillisecondsTimeout);

            } while (pushEvent.Length > 0);

            //TODO write logs for exception

            return true;
        }
    }
}
