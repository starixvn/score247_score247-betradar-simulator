namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using LiveScore.Shared.Models;
    using LiveScore.Soccers.Shared.Dtos;
    using LiveScore.Soccers.Shared.ModelBuilders;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;

    public interface IMatchPushEventHandler
    {
        Task OnReceive(string eventData, IHubContext<MatchHub> hubContext);

        Task OnReceive(MatchPushEventDto matchPushEventDto, IHubContext<MatchHub> hubContext);
    }

    public class MatchPushEventHandler : IMatchPushEventHandler
    {
        private static readonly IDictionary<string, int> PeriodStartTimes = new Dictionary<string, int>
        {
            { MatchStatus.FirstHaft.Value, 1 },
            { MatchStatus.SecondHaft.Value, 46 },
            { MatchStatus.FirstHaftExtra.Value, 91 },
            { MatchStatus.SecondHaftExtra.Value, 106 }
        };

        private readonly IMatchService matchService;
        private readonly ILogger logger;
        private readonly IPenaltyEventHandler penaltyEventHandler;

        public MatchPushEventHandler(
            IMatchService matchService,
            ILogger logger,
            IPenaltyEventHandler penaltyEventHandler)

        {
            this.matchService = matchService;
            this.logger = logger;
            this.penaltyEventHandler = penaltyEventHandler;
        }

        public async Task OnReceive(string eventData, IHubContext<MatchHub> hubContext)
        {
            try
            {
                var pushEventDto = JsonConvert.DeserializeObject<MatchPushEventDto>(eventData);

                await OnReceive(pushEventDto, hubContext);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Error occurs OnReceive {eventData}", ex);
            }
        }

        public async Task OnReceive(MatchPushEventDto matchPushEventDto, IHubContext<MatchHub> hubContext)
        {
            if (matchPushEventDto.payload != null)
            {
                var pushEvents = MatchBuilder.BuildPushEvents(matchPushEventDto);

                await Handle(pushEvents, hubContext);
            }
        }

        public async Task Handle(Dictionary<string, MatchPushEvent> pushEvents, IHubContext<MatchHub> hubContext)
        {
            if (pushEvents == null)
            {
                return;
            }

            foreach (var matchId in pushEvents.Keys)
            {
                GenerateMatchTimeForLiveMatch(pushEvents[matchId]);
            }

            var basicPushEvents = await GetBasicPushEvents(pushEvents);

            if (!basicPushEvents.Keys.Any())
            {
                return;
            }

            await PushToClients(basicPushEvents, hubContext);

            foreach (var matchId in basicPushEvents.Keys)
            {
                var pushEvent = basicPushEvents[matchId];
                await UpdateMatch(matchId, pushEvent);
            }

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

        private async Task BuildPushEvent(
            Dictionary<string, MatchPushEvent> basicPushEvents,
            string matchId,
            MatchPushEvent pushEvent,
            ITimeline timeline)
        {
#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
            var shootoutEvent = pushEvent.TimeLines.FirstOrDefault(
                                    tl => timeline.PeriodType == PeriodType.Penalties.Value
                                        && timeline.Type == EventTypes.PenaltyShootout.Value) as Timeline;
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

            var penaltyScoreChangeEvent = pushEvent.TimeLines.FirstOrDefault(
                tl => timeline.PeriodType == PeriodType.Penalties.Value
                    && timeline.Type == EventTypes.ScoreChange.Value);

            if (penaltyScoreChangeEvent != default(ITimeline))
            {
                return;
            }

            if (shootoutEvent != null)
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

        private async Task PushToClients(Dictionary<string, MatchPushEvent> pushEvents, IHubContext<MatchHub> hubContext)
        {
            var firstEvent = pushEvents.First().Value.TimeLines.First();
            await logger.InfoAsync($"{ DateTime.Now.ToString("MMMM dd HH:mm:ss") } Pushing {pushEvents.First().Key} - {firstEvent.Type}");

            await hubContext.Clients.All.SendAsync("PushMatchEvent", SportConstants.SoccerId.ToString(), pushEvents);
        }


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