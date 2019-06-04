namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.SportRadarApi.Models;

    public class MatchPushEventListener : IMatchPushEventListener
    {
        private const string FeatureKey = "Push";
        private const int MillisecondsTimeout = 5 * 1000;
        private readonly IMatchPushEventHandler pushHandler;
        private readonly IAppSettings appSettings;

        public MatchPushEventListener(
            IMatchPushEventHandler pushHandler,
            IAppSettings appSettings)
        {
            this.pushHandler = pushHandler;
            this.appSettings = appSettings;
        }

        public async Task<bool> Start(byte sportId)
        {

            var sportSetting = ValidateSettings(sportId, FeatureKey);

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

            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;            
            var _httpClient = new HttpClient(handler);

            HttpResponseMessage clientResponse = await _httpClient.GetAsync(new Uri(formattedEndpoint));

            if (clientResponse.StatusCode == HttpStatusCode.Found)
            {
                var req = (HttpWebRequest)WebRequest.Create(clientResponse.Headers.Location);
                req.KeepAlive = true;
                var reply = (HttpWebResponse)req.GetResponse();

                var stream = reply.GetResponseStream();
                var reader = new StreamReader(stream);
                var pushEvent = string.Empty;

                do
                {
                    pushEvent = reader.ReadLine();
                    await pushHandler.OnReceive(pushEvent);
                    await Task.Delay(MillisecondsTimeout);

                } while (pushEvent.Length > 0);
            }

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
