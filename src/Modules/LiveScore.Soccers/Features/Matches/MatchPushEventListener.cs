namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.SportRadarApi.Models;
    using Microsoft.AspNetCore.SignalR;
    using LiveScore.Shared.Hubs;

    public class MatchPushEventListener : IMatchPushEventListener
    {
        private const string FeatureKey = "Push";
        private const int MillisecondsTimeout = 5 * 1000;

        private readonly IMatchPushEventHandler pushHandler;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private readonly Dictionary<string, HttpWebRequest> httpRequests;

        public MatchPushEventListener(
            IMatchPushEventHandler pushHandler,
            IAppSettings appSettings,
            ILogger logger)
        {
            this.pushHandler = pushHandler;
            this.appSettings = appSettings;
            this.logger = logger;

            httpRequests = new Dictionary<string, HttpWebRequest>();
        }

        public async Task<bool> Start(byte sportId, IHubContext<MatchHub> hubContext)
        {
            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(FeatureKey).First();

            var regions = sportSetting.Regions
                .Where(x => x.Name.Equals("other")); // TODO: remove specific other region

            if (!regions.Any())
            {
                await logger.InfoAsync("Cannot find any specific region to listen");
                return false;
            }

            //TODO loop for regions            
            var region = regions.First();

            return await StartListening(sportId, endpoint, region, hubContext);
        }

        private async Task<bool> StartListening(byte sportId, EndPointSetting endpoint, Region region, IHubContext<MatchHub> hubContext)
        {
            var req = await CreateSubscribeRequest(endpoint, region);

            try
            {
                var reply = (HttpWebResponse)req.GetResponse();
                var stream = reply.GetResponseStream();
                var reader = new StreamReader(stream);

                while (true)
                {
                    await ProcessPayloads(reader, hubContext);

                    await Task.Delay(MillisecondsTimeout);
                }
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Error while listening feed for sportId {sportId} - region {region.Name}", ex);
            }

            return true;
        }

        private async Task<HttpWebRequest> CreateSubscribeRequest(EndPointSetting endpoint, Region region)
        {
            if (!httpRequests.ContainsKey(region.Name))
            {
                var formattedEndpoint = $"{appSettings.DataProviderSettings.ApiUrl}{string.Format(endpoint.Path, region.Name, region.PushKey)}";

                if (appSettings.EnabledListenLogging)
                {
                    await logger.InfoAsync($"listen {formattedEndpoint}");
                }

                var req = (HttpWebRequest)HttpWebRequest.Create(new Uri(formattedEndpoint));
                req.AllowAutoRedirect = false;

                WebResponse reply;

                try
                {
                    req.GetResponse();

                }
                catch (WebException e)
                {

                    reply = e.Response;
                    var redirectUrl = reply.Headers["Location"];
                    req = (HttpWebRequest)HttpWebRequest.Create(new Uri(redirectUrl));
                    req.KeepAlive = true;

                    httpRequests.Add(region.Name, req);
                }
            }

            return httpRequests[region.Name];
        }

        private async Task ProcessPayloads(StreamReader reader, IHubContext<MatchHub> hubContext)
        {
            string line;

            do
            {
                line = reader.ReadLine();

                if (appSettings.EnabledListenLogging)
                {
                    await logger.InfoAsync($"{DateTime.Now} Receiving: {line}");
                }

                if (!string.IsNullOrWhiteSpace(line) && !line.Contains("heartbeat"))
                {
                    await pushHandler.OnReceive(line, hubContext);
                }
            } while (!string.IsNullOrWhiteSpace(line));
        }
    }
}
