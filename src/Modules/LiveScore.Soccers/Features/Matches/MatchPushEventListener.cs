namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Shared;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.SportRadarApi.Models;

    public class MatchPushEventListener : IMatchPushEventListener
    {
        private const string FeatureKey = "Push";
        private const int MillisecondsTimeout = 5 * 1000;
        private readonly PushHandlerFactory handlerFactory;
        private readonly IAppSettings appSettings;

        public MatchPushEventListener(
            PushHandlerFactory handlerFactory,
            IAppSettings appSettings)
        {
            this.handlerFactory = handlerFactory;
            this.appSettings = appSettings;
        }

        public async Task<bool> Start(byte sportId)
        {

            var sportSetting = ValidateSettings(1, FeatureKey);

            var endpointSettings = sportSetting
                .EndPoints
                .FirstOrDefault(x => x.Name.Equals(FeatureKey))
                .EndPointSettings;

            var regions = sportSetting.Regions
                .Where(x => x.Name.Equals("other")); // TODO: remove specific other region

            //TODO loop for endpoints and regions

            var endpoint = endpointSettings.First();
            var region = regions.First();

            //TODO ensure only one push channel created
            var formattedEndpoint = $"{appSettings.DataProviderSettings.ApiUrl}{string.Format(endpoint.Path, region.Name, region.PushKey)}";

            var req = (HttpWebRequest)WebRequest.Create(new Uri(formattedEndpoint));
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
                req = (HttpWebRequest)WebRequest.Create(new Uri(reply.Headers["Location"]));
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

        public SportSetting ValidateSettings(int sportId, string key)
        {
            var sportSetting = appSettings
                .DataProviderSettings
                .Sports.FirstOrDefault(sport => sport.Id == sportId);

            if (sportSetting == null)
            {
                throw new InvalidOperationException($"Missing Setting for sportId {sportId}");
            }

            if (!sportSetting.EndPoints.Any(x => x.Name.Equals(key)))
            {
                throw new InvalidOperationException($"Missing endpoint settings for {key}");
            }

            return sportSetting;
        }
    }
}
