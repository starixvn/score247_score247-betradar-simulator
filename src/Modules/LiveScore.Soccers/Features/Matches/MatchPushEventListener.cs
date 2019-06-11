using Microsoft.AspNetCore.SignalR;
using LiveScore.Shared.Hubs;
namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
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

        private readonly Dictionary<string, HttpWebRequest> httpRequests;

        public MatchPushEventListener(
            IMatchPushEventHandler pushHandler,
            IAppSettings appSettings)
        {
            this.pushHandler = pushHandler;
            this.appSettings = appSettings;

            httpRequests = new Dictionary<string, HttpWebRequest>();
        }

        public async Task<bool> Start(byte sportId, IHubContext<MatchHub> hubContext)
        {
            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(FeatureKey).First();

            var regions = sportSetting.Regions
                .Where(x => x.Name.Equals("other")); // TODO: remove specific other region

            //TODO loop for regions            
            var region = regions.First();

            var req = await CreateSubscribeRequest(endpoint, region);
            var reply = (HttpWebResponse)req.GetResponse();

            var stream = reply.GetResponseStream();
            var reader = new StreamReader(stream);
            var pushEvent = string.Empty;

            do
            {
                pushEvent = reader.ReadLine();
                await pushHandler.OnReceive(pushEvent, hubContext);
                await Task.Delay(MillisecondsTimeout);

            } while (pushEvent.Length > 0);


            return true;
        }

        public async Task<HttpWebRequest> CreateSubscribeRequest(EndPointSetting endpoint, Region region)
        {
            if (!httpRequests.ContainsKey(region.Name))
            {
                var formattedEndpoint = $"{appSettings.DataProviderSettings.ApiUrl}{string.Format(endpoint.Path, region.Name, region.PushKey)}";

                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = true;
                var httpClient = new HttpClient(handler);

                HttpResponseMessage clientResponse = await httpClient.GetAsync(new Uri(formattedEndpoint));

                if (clientResponse.StatusCode == HttpStatusCode.Found)
                {
                    var req = (HttpWebRequest)WebRequest.Create(clientResponse.Headers.Location);
                    req.KeepAlive = true;

                    httpRequests.Add(region.Name, req);

                }
                else
                {
                    throw new InvalidOperationException($"Cannot subscribe {formattedEndpoint}. Status Code: {clientResponse.StatusCode}");
                }
            }

            return httpRequests[region.Name];
        }
    }
}
