namespace LiveScore.Soccers.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using LiveScore.Soccers.Shared.Dtos;
    using LiveScore.Soccers.Shared.ModelBuilders;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using Fanex.Caching;

    public class MatchPushEventHandler : IMatchPushEventHandler
    {
        private const int LiveMatchSlidingExpirationMinute = 130;
        private static readonly IDictionary<string, int> PeriodStartTimes = new Dictionary<string, int>
        {
            { MatchStatus.FirstHaft.Value, 1 },
            { MatchStatus.SecondHaft.Value, 46 },
            { MatchStatus.FirstHaftExtra.Value, 91 },
            { MatchStatus.SecondHaftExtra.Value, 106 }
        };

        private readonly IMatchDataAccess matchDataAccess;
        private readonly IHubContext<MatchHub> hubContext;
        private readonly ICacheService cacheService;

        public MatchPushEventHandler(
            IMatchDataAccess matchDataAccess,
            IHubContext<MatchHub> hubContext,
            ICacheService cacheService)
        {
            this.matchDataAccess = matchDataAccess;
            this.hubContext = hubContext;
            this.cacheService = cacheService;
        }

        public async Task OnReceive(string eventData)
        {
            try
            {
                var pushEventDto = JsonConvert.DeserializeObject<MatchPushEventDto>(eventData);
                var pushEvents = MatchBuilder.BuildPushEvents(pushEventDto);

                await Handle(pushEvents);
            }
            catch (Exception ex)
            {

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

                await HandleMatchEvent(matchId, pushEvent);
                await matchDataAccess.AddResult(matchId, pushEvent.MatchResult);
                await matchDataAccess.AddTimelines(matchId, pushEvent.TimeLines);
            }
        }

        private async Task PushChangeToClients(Dictionary<string, MatchPushEvent> pushEvents)
        {
            await hubContext.Clients.All.SendAsync("PushMatchEvent", SportConstants.SoccerId, pushEvents);
        }

        private async Task HandleMatchEvent(string matchId, MatchPushEvent pushEvent)
        {
            var timeline = pushEvent.TimeLines.FirstOrDefault();

            if (timeline.Type == EventTypes.MatchEnded.Value)
            {
                await cacheService.RemoveAsync(matchId);
                await matchDataAccess.DeleteMatch(matchId, isLive: true);
                await matchDataAccess.AddOrUpdateMatch(matchId, pushEvent);
            }
            else
            {
                var match = await matchDataAccess.AddOrUpdateMatch(matchId, pushEvent, isLive: true);

                if (match != null)
                {
                    var liveMatches = await cacheService.GetOrSetAsync(
                        $"LiveMatch_{SportConstants.SoccerId}", () => new Dictionary<string, IMatch>());

                    liveMatches[matchId] = match;

                    await cacheService.SetAsync($"LiveMatch_{SportConstants.SoccerId}", liveMatches, new CacheItemOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(LiveMatchSlidingExpirationMinute)
                    });
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
