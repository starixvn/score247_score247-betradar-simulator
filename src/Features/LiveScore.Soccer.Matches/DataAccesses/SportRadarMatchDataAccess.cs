namespace LiveScore.Soccers.Matches.DataAccesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Soccer.Dtos;
    using LiveScore.Soccer.Matches.DataAccesses;
    using LiveScore.Soccer.Matches.Handlers;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Shared.Configurations;
    using LiveScore.Soccer.Shared.Helpers;
    using LiveScore.Soccer.Shared.ModelBuilders;

    public class SportRadarMatchDataAccess : IMatchDataAccess
    {
        private const string SportRadarDateFormat = "yyyy-MM-dd";
        private const string FeatureKey = "MatchSchedule";
        private const string LiveMatchFeatureKey = "LiveMatch";
        private readonly IAppSettings appSettings;
        private readonly IRestClient restClient;
        private readonly IPenaltyEventHandler penaltyEventHandler;

        public SportRadarMatchDataAccess(
            IAppSettings appSettings,
            IRestClient restClient,
            IPenaltyEventHandler penaltyEventHandler)
        {
            this.appSettings = appSettings;
            this.restClient = restClient;
            this.penaltyEventHandler = penaltyEventHandler;
        }

        public async Task<IEnumerable<Match>> GetMatches(int sportId, DateTime utcFrom, DateTime utcTo, string language)
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

            foreach (var endPoint in endpointSettings)
            {
                foreach (var region in sportSetting.Regions)
                {
                    var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                    var dailyResult = await restClient.ExecuteGetAsync<DailyResultDto>(formattedEndpoint);

                    if (dailyResult != null
                        && dailyResult.results != null
                        && dailyResult.results.Any())
                    {
                        dailyResult.results.ToList().ForEach(x => matches.Add(MatchBuilder.BuildMatch(x.sport_event, x.sport_event_status, region.Name)));
                    }
                }
            }

            return matches;
        }

        private async Task<List<Match>> FetchSchedule(string language, SportSetting sportSetting, DateTime date)
        {
            var matches = new List<Match>();

            var endpointSettings = sportSetting.GetEndPointSetting(FeatureKey).Where(x => !x.IsPast);

            foreach (var endPoint in endpointSettings)
            {
                foreach (var region in sportSetting.Regions)
                {
                    var formattedEndpoint = string.Format(endPoint.Path, region.Name, language, date.ToString(SportRadarDateFormat), region.Key);
                    var dailySchedule = await restClient.ExecuteGetAsync<DailyScheduleDto>(formattedEndpoint);

                    if (dailySchedule != null
                        && dailySchedule.sport_events != null
                        && dailySchedule.sport_events.Any())
                    {
                        dailySchedule.sport_events.ToList().ForEach(x => matches.Add(MatchBuilder.BuildMatch(x, null, region.Name)));
                    }
                }
            }

            return matches;
        }

        //TODO combine live match and daily results
        public async Task<IEnumerable<Match>> GetLiveMatches(int sportId, string language)
        {
            var matches = new List<Match>();

            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpointSettings = sportSetting.GetEndPointSetting(LiveMatchFeatureKey);
            var regions = sportSetting.Regions;

            foreach (var endpoint in endpointSettings)
            {
                foreach (var region in regions)
                {
                    var formattedEndpoint = string.Format(endpoint.Path, region.Name, language, region.Key);
                    var dailyResult = await restClient.ExecuteGetAsync<DailyResultDto>(formattedEndpoint);

                    if (dailyResult != null
                        && dailyResult.results != null
                        && dailyResult.results.Any())
                    {
                        dailyResult.results.ToList()
                            .ForEach(x => matches.Add(MatchBuilder.BuildMatch(x.sport_event, x.sport_event_status, region.Name)));
                    }
                }
            }

            return matches;
        }

        public Task<Match> AddOrUpdateMatch(string matchId, string language, MatchPushEvent pushEvent, bool isLive = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteLiveMatch(string matchId, string language)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateLiveMatches(IEnumerable<Match> matches, string language)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddTimelines(string matchId, IEnumerable<Timeline> timelines, bool isDeleteExisting = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddResult(string matchId, MatchResult matchResult)
        {
            throw new NotImplementedException();
        }

        public Task<Match> GetMatch(int sportId, string matchId, string language)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Timeline>> GetTimelines(int sportId, string region, string matchId, string language)
        {
            const string Feature = "Timeline";

            var timelines = new List<Timeline>();

            var sportSetting = appSettings.DataProviderSettings.GetSportSetting(sportId);
            var endpoint = sportSetting.GetEndPointSetting(Feature).First();
            var key = sportSetting.Regions.FirstOrDefault(x => x.Name.Equals(region)).Key;

            if (!string.IsNullOrWhiteSpace(key))
            {
                var formattedEndpoint = string.Format(endpoint.Path, region, language, matchId, key);
                var matchTimeline = await restClient.ExecuteGetAsync<MatchTimelineDto>(formattedEndpoint);

                if (matchTimeline != null && matchTimeline.timeline != null && matchTimeline.timeline.Any())
                {
                    await TransformTimelines(matchId, timelines, matchTimeline);
                }
            }

            return timelines;
        }

        private async Task TransformTimelines(string matchId, List<Timeline> timelines, MatchTimelineDto matchTimeline)
        {
            foreach (var rawTimeline in matchTimeline.timeline)
            {
                var timeline = MatchBuilder.BuildTimeLine(rawTimeline);

                timeline = await BuildPenaltyEvent(matchId, timeline);

                if (timeline != default(Timeline))
                {
                    timelines.Add(timeline);
                }
            }
        }

        private async Task<Timeline> BuildPenaltyEvent(string matchId, Timeline timeline)
        {
            if (timeline.PeriodType == PeriodType.Penalties.Value)
            {
                if (timeline.Type == EventTypes.PenaltyShootout.Value)
                {
                    return await penaltyEventHandler.Handle(matchId, timeline);
                }

                if (timeline.Type == EventTypes.ScoreChange.Value)
                {
                    return default(Timeline);
                }
            }

            return timeline;
        }

        public Task AddCommentaries(string matchId, IEnumerable<Timeline> timelines, string language)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MatchFunction>> GetMatchFunctions(int sportId, string matchId, string language)
        {
            throw new NotImplementedException();
        }
    }
}