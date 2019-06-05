namespace LiveScore.Soccers.Features.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Configurations;
    using LiveScore.Shared.Helpers;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Shared.SportRadarApi.Models;
    using LiveScore.Soccers.Shared.ModelBuilders;

    public class SportRadarMatchDataAccess : IMatchDataAccess
    {
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private const string FeatureKey = "MatchSchedule";
        private const string LiveMatchFeatureKey = "LiveMatch";
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;

        public SportRadarMatchDataAccess(
            IAppSettings appSettings,
            IRestClient restClient)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
        }

        public async Task<IEnumerable<IMatch>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language)
        {
            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);

            var matches = new List<Match>();
            var today = DateTime.Now.ToUniversalTime();
            const int dayIncrementIndex = 1;

            for (var date = utcFrom.Date; date.Date <= utcTo.Date; date = date.AddDays(dayIncrementIndex))
            {
                matches.AddRange(
                    date >= today.Date
                    ? await FetchSchedule(language, sportSetting, date)
                    : await FetchResults(language, sportSetting, date));
            }

            return matches;
        }

        private async Task<List<Match>> FetchResults(string language, SportSetting sportSetting, DateTime date)
        {
            var matches = new List<Match>();

            var endpointSettings = sportSetting.GetEndPointSetting(FeatureKey).Where(x => x.IsPast);

            var regions = sportSetting.Regions;

            foreach (var endPoint in endpointSettings)
            {
                var dailyResult = await FetchDailyResult(date, language, regions, endPoint);

                if (dailyResult.results != null && dailyResult.results.Any())
                {
                    dailyResult.results.ToList().ForEach(x => matches.Add(MatchBuilder.BuildMatch(x.sport_event, x.sport_event_status)));
                }
            }

            return matches;
        }

        private async Task<List<Match>> FetchSchedule(string language, SportSetting sportSetting, DateTime date)
        {
            var matches = new List<Match>();

            var endpointSettings = sportSetting.GetEndPointSetting(FeatureKey).Where(x => !x.IsPast);

            var regions = sportSetting.Regions;

            foreach (var endPoint in endpointSettings)
            {
                var dailySchedule = await FetchDailySchedule(date, language, regions, endPoint);

                if (dailySchedule.sport_events != null && dailySchedule.sport_events.Any())
                {
                    dailySchedule.sport_events.ToList().ForEach(x => matches.Add(MatchBuilder.BuildMatch(x)));
                }
            }

            return matches;
        }

        private async Task<DailyScheduleDto> FetchDailySchedule(DateTime date, string language, IEnumerable<Region> regions, EndPointSetting endPoint)
        {
            var finalDailySchedules = new DailyScheduleDto { generated_at = DateTime.Now.ToUniversalTime(), sport_events = new List<Sport_Event>() };

            foreach (var region in regions)
            {
                var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                var dailyResult = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndpoint);

                if (dailyResult.sport_events != null && dailyResult.sport_events.Any())
                {
                    finalDailySchedules.sport_events = finalDailySchedules.sport_events.Concat(dailyResult.sport_events);
                }
            }

            return finalDailySchedules;
        }

        public async Task<DailyResultDto> FetchDailyResult(DateTime date, string language, IEnumerable<Region> regions, EndPointSetting endPoint)
        {
            var finalDailyResults = new DailyResultDto { generated_at = DateTime.Now.ToUniversalTime(), results = new List<Result>() };

            foreach (var region in regions)
            {
                var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                var dailyResult = await restClient.ExecuteGetAsync<DailyResultDto>(formattedEndpoint);

                if (dailyResult.results != null && dailyResult.results.Any())
                {
                    finalDailyResults.results = finalDailyResults.results.Concat(dailyResult.results);
                }
            }

            return finalDailyResults;
        }

        //TODO combine live match and daily results
        public async Task<IEnumerable<IMatch>> GetLiveMatches(int sportId, string language)
        {
            var matches = new List<Match>();

            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpointSettings = sportSetting.GetEndPointSetting(LiveMatchFeatureKey);

            var regions = sportSetting.Regions;

            var liveResults = new DailyResultDto { generated_at = DateTime.Now.ToUniversalTime(), results = new List<Result>() };

            foreach (var endpoint in endpointSettings)
            {
                foreach (var region in regions)
                {
                    var formattedEndpoint = string.Format(endpoint.Path, region.Name, language, region.Key);
                    var dailyResult = await restClient.ExecuteGetAsync<DailyResultDto>(formattedEndpoint);

                    if (dailyResult.results != null && dailyResult.results.Any())
                    {
                        liveResults.results = liveResults.results.Concat(dailyResult.results);
                    }
                }
            }

            if (liveResults.results != null && liveResults.results.Any())
            {
                liveResults.results.ToList().ForEach(x => matches.Add(MatchBuilder.BuildMatch(x.sport_event, x.sport_event_status)));
            }

            return matches;
        }

        public Task<IMatch> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMatch(string matchId, string language, bool isLive = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateLiveMatches(IEnumerable<IMatch> matches, string language)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddTimelines(string matchId, IEnumerable<ITimeline> timelines)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddResult(string matchId, IMatchResult matchResult)
        {
            throw new NotImplementedException();
        }

        public Task<IMatch> GetMatch(int sportId, string matchId, string language)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ITimeline>> GetTimelines(int sportId, string region, string matchId, string language)
        {
            const string Feature = "Timeline";

            var timelines = new List<Timeline>();

            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(Feature).First();
            var key = sportSetting.Regions.FirstOrDefault(x => x.Name.Equals(region)).Key;

            var formattedEndpoint = string.Format(endpoint.Path, region, language, matchId, key);
            var matchTimeline = await restClient.ExecuteGetAsync<MatchTimelineDto>(formattedEndpoint);

            if (matchTimeline.timeline != null && matchTimeline.timeline.Any())
            {
                matchTimeline.timeline.ToList().ForEach(x => timelines.Add(MatchBuilder.BuildTimeLine(x)));
            }

            return timelines;
        }
    }
}
