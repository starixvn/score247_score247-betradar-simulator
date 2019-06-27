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
        private const int MaxAttempts = 10;
        private const int MillisecondsTimeout = 10 * 1000;

        private readonly IMatchPushEventHandler pushHandler;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private Dictionary<string, StreamReader> readers;
        private int attempts;

        public MatchPushEventListener(
            IMatchPushEventHandler pushHandler,
            IAppSettings appSettings,
            ILogger logger)
        {
            this.pushHandler = pushHandler;
            this.appSettings = appSettings;
            this.logger = logger;

            readers = new Dictionary<string, StreamReader>();
            attempts = 0;
        }

        public async Task Start(byte sportId)
        {
            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(FeatureKey).First();

            var regions = sportSetting.Regions;

            if (regions.Any())
            {
                while (attempts < MaxAttempts)
                {
                    attempts += 1;
                    await logger.InfoAsync($"Start Listening Attempts: {attempts} at {DateTime.Now}");

                    //TODO call fetch live match results job after retry failure job when ListenPushEvent was force stop

                    await StartListening(sportId, sportSetting, endpoint);

                    readers = new Dictionary<string, StreamReader>();

                    await Task.Delay(MillisecondsTimeout);
                }
            }
            else
            {
                await logger.InfoAsync("Cannot find any specific region to listen");
            }
        }

        private async Task<bool> StartListening(byte sportId, SportSetting sportSetting, EndPointSetting endpoint)
        {
            foreach (var regionSetting in sportSetting.Regions)
            {
                if (!readers.ContainsKey(regionSetting.Name))
                {
                    var req = await CreateSubscribeRequest(endpoint, regionSetting);

                    StreamReader reader = CreateStreamReader(req);

                    readers.Add(regionSetting.Name, reader);
                }
            }

            var isFailed = false;

            while (!isFailed)
            {
                foreach (var reader in readers)
                {
                    try
                    {
                        await ProcessPayloads(reader.Value);
                    }
                    catch (Exception ex)
                    {
                        await logger.ErrorAsync($"{ DateTime.Now } Error while listening feed for sportId {sportId} - region {reader.Key}", ex);
                        isFailed = true;
                    }
                }

                if (!isFailed)
                {
                    await Task.Delay(MillisecondsTimeout);
                }
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

        private async Task ProcessPayloads(StreamReader reader)
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
                    await pushHandler.OnReceive(line);
                }
            } while (!string.IsNullOrWhiteSpace(line));
        }
    }
}