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
        private readonly IHubContext<MatchHub> hubContext;
        private readonly ILogger logger;

        public MatchPushEventHandler(
            IMatchService matchService,
            IHubContext<MatchHub> hubContext,
            ILogger logger)
        {
            this.matchService = matchService;
            this.hubContext = hubContext;
            this.logger = logger;
        }

        public async Task OnReceive(string eventData)
        {
            try
            {
                var pushEventDto = JsonConvert.DeserializeObject<MatchPushEventDto>(eventData);

                if (pushEventDto.payload != null)
                {                    
                    var pushEvents = MatchBuilder.BuildPushEvents(pushEventDto);
                    await Handle(pushEvents);
                }                
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Error occurs OnReceive {eventData}", ex);
            }
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

            var pushChangeToClientsTask = Task.Run(() => PushChangeToClients(pushEvents));
            var updateDatabaseTask = Task.Run(() => UpdateDatabase(pushEvents));

            await Task.WhenAll(pushChangeToClientsTask, updateDatabaseTask);
        }

        private async Task UpdateDatabase(Dictionary<string, MatchPushEvent> pushEvents)
        {
            foreach (var matchId in pushEvents.Keys)
            {
                var pushEvent = pushEvents[matchId];

                await UpdateMatch(matchId, pushEvent);
                await matchService.AddResult(SportConstants.SoccerId, matchId, pushEvent.MatchResult);
                await matchService.AddTimelines(SportConstants.SoccerId, matchId, pushEvent.TimeLines);
            }
        }

        private async Task PushChangeToClients(Dictionary<string, MatchPushEvent> pushEvents)
        {
            //TODO only push events: period_start, period_end, injury_time_shown, score_change
            await hubContext.Clients.All.SendAsync("PushMatchEvent", SportConstants.SoccerId, pushEvents);
        }

        private async Task UpdateMatch(string matchId, MatchPushEvent pushEvent)
        {
            var timeline = pushEvent.TimeLines.FirstOrDefault();

            foreach (var language in Enumeration.GetAll<Language>())
            {
                if (timeline.Type == EventTypes.MatchEnded.Value)
                {
                    await matchService.DeleteMatch(SportConstants.SoccerId, language.Value, matchId, isLive: true);
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
                var timeline = pushEvent.TimeLines.FirstOrDefault();
                var matchTime = timeline.MatchTime;
                var matchStatus = pushEvent.MatchResult.MatchStatus.Value;

                if (timeline.Type == EventTypes.PeriodStart.Value && PeriodStartTimes.ContainsKey(matchStatus))
                {
                    matchTime = PeriodStartTimes[matchStatus];
                }

                pushEvent.MatchResult.MatchTime = matchTime;
            }
        }
    }
}
