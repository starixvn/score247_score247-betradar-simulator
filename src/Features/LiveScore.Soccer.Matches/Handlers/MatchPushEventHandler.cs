namespace LiveScore.Soccer.Matches.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Caching;
    using Fanex.Logging;
    using LiveScore.Soccer.Dtos;
    using LiveScore.Soccer.Matches.Hubs.EventHandling;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Shared;
    using LiveScore.Soccer.Shared.ModelBuilders;
    using LiveScore.Soccer.Shared.Models;
    using Newtonsoft.Json;

    public interface IMatchPushEventHandler
    {
        Task OnReceive(string eventData);

        Task OnReceive(MatchPushEventDto matchPushEventDto);
    }

    public class MatchPushEventHandler : IMatchPushEventHandler
    {
        private static readonly CacheItemOptions EventCacheOptions =
            new CacheItemOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
            };

        private static readonly IDictionary<string, int> PeriodStartTimes =
            new Dictionary<string, int>
            {
                { MatchStatus.FirstHaft.Value, 1 },
                { MatchStatus.SecondHaft.Value, 46 },
                { MatchStatus.FirstHaftExtra.Value, 91 },
                { MatchStatus.SecondHaftExtra.Value, 106 }
            };

        private readonly IMatchService matchService;
        private readonly ILogger logger;
        private readonly IPenaltyEventHandler penaltyEventHandler;
        private readonly IPushEventHandler pushHandler;
        private readonly ICacheService cacheService;

        public MatchPushEventHandler(
            IMatchService matchService,
            ILogger logger,
            IPenaltyEventHandler penaltyEventHandler,
            IPushEventHandler pushHandler,
            ICacheService cacheService)

        {
            this.matchService = matchService;
            this.logger = logger;
            this.penaltyEventHandler = penaltyEventHandler;
            this.pushHandler = pushHandler;
            this.cacheService = cacheService;
        }

        public async Task OnReceive(string eventData)
        {
            try
            {
                var pushEventDto = JsonConvert.DeserializeObject<MatchPushEventDto>(eventData);

                await OnReceive(pushEventDto);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Error occurs OnReceive {eventData}", ex);
            }
        }

        public async Task OnReceive(MatchPushEventDto matchPushEventDto)
        {
            if (matchPushEventDto.payload != null)
            {
                var pushEvents = MatchBuilder.BuildPushEvents(matchPushEventDto);

                if (await IsEventValid(pushEvents))
                {
                    await Handle(pushEvents);
                }
            }
        }

        private async Task<bool> IsEventValid(Dictionary<string, MatchPushEvent> pushEvents)
        {
            foreach (var pushEvent in pushEvents)
            {
                var cacheKey = $"MatchPushEvent_Match_{pushEvent.Key}";
                var cacheEvents = cacheService.Get<IEnumerable<Timeline>>(cacheKey);

                if (cacheEvents == null)
                {
                    cacheEvents = await matchService.GetTimelines(SportConstants.SoccerId, string.Empty, pushEvent.Key, string.Empty);
                }

                if (cacheEvents.Any(e => pushEvent.Value.TimeLines.Any(tl => tl.Id == e.Id)))
                {
                    return false;
                }

                cacheEvents = cacheEvents.Concat(pushEvent.Value.TimeLines);
                await cacheService.SetAsync(cacheKey, cacheEvents, EventCacheOptions);
            }

            return true;
        }

        public async Task Handle(Dictionary<string, MatchPushEvent> pushEvents)
        {
            if (pushEvents == null)
            {
                return;
            }

            foreach (var matchId in pushEvents.Keys)
            {
                GenerateMatchTimeForLiveMatch(pushEvents[matchId]);
            }

            var tasks = new List<Task>
            {
                InsertToDatabase(pushEvents),
                PushBasicEvents(pushEvents)
            };

            await Task.WhenAll(tasks);
        }

        private async Task PushBasicEvents(Dictionary<string, MatchPushEvent> pushEvents)
        {
            var basicPushEvents = await GetBasicPushEvents(pushEvents);

            if (basicPushEvents.Keys.Count > 0)
            {
                await pushHandler.Handle(basicPushEvents);

                foreach (var matchId in basicPushEvents.Keys)
                {
                    var pushEvent = basicPushEvents[matchId];
                    await UpdateMatch(matchId, pushEvent);
                }
            }
        }

        private async Task InsertToDatabase(Dictionary<string, MatchPushEvent> pushEvents)
        {
            foreach (var matchId in pushEvents.Keys)
            {
                var pushEvent = pushEvents[matchId];
                await matchService.AddResult(SportConstants.SoccerId, matchId, pushEvent.MatchResult);
                await matchService.AddTimelines(SportConstants.SoccerId, matchId, pushEvent.TimeLines);
            }
        }

        private async Task<Dictionary<string, MatchPushEvent>> GetBasicPushEvents(Dictionary<string, MatchPushEvent> pushEvents)
        {
            var basicPushEvents = new Dictionary<string, MatchPushEvent>();

            foreach (var matchId in pushEvents.Keys)
            {
                var pushEvent = pushEvents[matchId];
                var timelines = pushEvent.TimeLines.ToList();
                var timeline = timelines.FirstOrDefault(x => EventTypes.BasicSoccerEventTypes.Contains(x.Type));

                if (timeline != null)
                {
                    await BuildPushEvent(basicPushEvents, matchId, pushEvent, timeline);
                }
            }

            return basicPushEvents;
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types

        private async Task BuildPushEvent(
            Dictionary<string, MatchPushEvent> basicPushEvents,
            string matchId,
            MatchPushEvent pushEvent,
            Timeline timeline)
        {
            var penaltyScoreChangeEvent = pushEvent.TimeLines.FirstOrDefault(
                _ => timeline.PeriodType == PeriodType.Penalties.Value
                    && timeline.Type == EventTypes.ScoreChange.Value);

            if (penaltyScoreChangeEvent != default(Timeline))
            {
                return;
            }

            if (pushEvent.TimeLines.FirstOrDefault(
                _ => timeline.PeriodType == PeriodType.Penalties.Value
                    && timeline.Type == EventTypes.PenaltyShootout.Value) is Timeline shootoutEvent)
            {
                var penaltyEvent = await penaltyEventHandler.Handle(matchId, shootoutEvent);

                if (penaltyEvent != default(Timeline))
                {
                    pushEvent.TimeLines = new List<Timeline> { penaltyEvent };
                    basicPushEvents[matchId] = pushEvent;
                }
            }
            else
            {
                basicPushEvents[matchId] = pushEvent;
            }
        }

#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

        private async Task UpdateMatch(string matchId, MatchPushEvent pushEvent)
        {
            var timeline = pushEvent.TimeLines.LastOrDefault();

            foreach (var language in Enumeration.GetAll<Language>())
            {
                if (timeline.Type == EventTypes.MatchEnded.Value)
                {
                    await matchService.DeleteLiveMatch(SportConstants.SoccerId, language.Value, matchId);
                    await matchService.AddOrUpdateMatch(SportConstants.SoccerId, language.Value, matchId, pushEvent);
                }
                else
                {
                    await matchService.AddOrUpdateMatch(SportConstants.SoccerId, language.Value, matchId, pushEvent, isLive: true);
                }
            }
        }

        private static void GenerateMatchTimeForLiveMatch(MatchPushEvent pushEvent)
        {
            if (pushEvent.MatchResult.EventStatus.IsLive())
            {
                var timeline = pushEvent.TimeLines.LastOrDefault();
                var matchTime = timeline.MatchTime;
                var matchStatus = pushEvent.MatchResult.MatchStatus.Value;

                if (timeline.Type == EventTypes.PeriodStart.Value
                    && PeriodStartTimes.ContainsKey(matchStatus))
                {
                    matchTime = PeriodStartTimes[matchStatus];
                }

                if (!string.IsNullOrEmpty(timeline.StoppageTime))
                {
                    matchTime += int.Parse(timeline.StoppageTime);
                }

                if (matchTime > 0)
                {
                    pushEvent.MatchResult.MatchTime = matchTime;
                }
            }
        }
    }
}