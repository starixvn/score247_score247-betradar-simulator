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
        private const int MillisecondsTimeout = 10 * 1000;

        private readonly IMatchPushEventHandler pushHandler;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private readonly Dictionary<string, StreamReader> readers;

        public MatchPushEventListener(
            IMatchPushEventHandler pushHandler,
            IAppSettings appSettings,
            ILogger logger)
        {
            this.pushHandler = pushHandler;
            this.appSettings = appSettings;
            this.logger = logger;

            readers = new Dictionary<string, StreamReader>();
        }

        public async Task<bool> Start(byte sportId, IHubContext<MatchHub> hubContext)
        {
            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(FeatureKey).First();

            var regions = sportSetting.Regions;

            if (!regions.Any())
            {
                await logger.InfoAsync("Cannot find any specific region to listen");
                return false;
            }

            return await StartListening(sportId, hubContext, sportSetting, endpoint, regions);
        }

        private async Task<bool> StartListening(byte sportId, IHubContext<MatchHub> hubContext, SportSetting sportSetting, EndPointSetting endpoint, IEnumerable<Region> regions)
        {
            foreach (var regionSetting in sportSetting.Regions)
            {
                if (!readers.ContainsKey(regionSetting.Name))
                {
                    var req = await CreateSubscribeRequest(endpoint, regionSetting);

                    StreamReader reader = CreateStreamReader(req);
                    readers.Add(regionSetting.Key, reader);
                }
            }

            var isFailed = false;

            while (!isFailed)
            {
                foreach (var reader in readers)
                {
                    try
                    {
                        await ProcessPayloads(reader.Value, hubContext);
                    }
                    catch (Exception ex)
                    {
                        await logger.ErrorAsync($"Error while listening feed for sportId {sportId} - region {reader.Key}", ex);
                        isFailed = true;
                    }
                }

                await Task.Delay(MillisecondsTimeout);
            }

            return isFailed;
        }

        private static StreamReader CreateStreamReader(HttpWebRequest req)
        {
            var reply = (HttpWebResponse)req.GetResponse();
            var stream = reply.GetResponseStream();
            var reader = new StreamReader(stream);

            return reader;
        }

        private async Task<HttpWebRequest> CreateSubscribeRequest(EndPointSetting endpoint, Region region)
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


            }

            return req;
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
