namespace LiveScore.Soccers.Features.Matches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared;
    using LiveScore.Shared.Hubs;
    using LiveScore.Soccers.Features.Matches.DataAccesses;
    using LiveScore.Soccers.Shared.Dtos;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;

    public class PushEventHandler : IPushEventHandler
    {
        private static readonly IDictionary<string, int> PeriodStartTimes = new Dictionary<string, int>
        {
            { MatchStatus.FirstHaft.Value, 1 },
            { MatchStatus.SecondHaft.Value, 46 },
            { MatchStatus.FirstHaftExtra.Value, 91 },
            { MatchStatus.SecondHaftExtra.Value, 106 }
        };

        private readonly IMatchDataAccess matchDataAccess;
        private readonly IHubContext<PushMatchEventHub> hubContext;

        public PushEventHandler(
            IMatchDataAccess matchDataAccess,
            IHubContext<PushMatchEventHub> hubContext)
        {
            this.matchDataAccess = matchDataAccess;
            this.hubContext = hubContext;
        }

        public async Task OnReceive(string eventData)
        {
            try
            {
                var pushEventDto = JsonConvert.DeserializeObject<PushEventDto>(eventData);
                var pushEvents = BuildPushEvents(pushEventDto);

                await Handle(pushEvents);
            }
            catch
            {
                return;
            }
        }

        public async Task Handle(Dictionary<string, PushEvent> pushEvents)
        {
            if (pushEvents != null)
            {
                foreach (var matchId in pushEvents.Keys)
                {
                    var payload = pushEvents[matchId];
                    GenerateMatchTimeForLiveMatch(payload);

                    await matchDataAccess.AddTimelines(matchId, payload.TimeLines);
                    await matchDataAccess.AddResult(matchId, payload.MatchResult);
                }

                await hubContext.Clients.All.SendAsync("PushMatchEvent", SportConstants.SoccerId, pushEvents);
            }
        }

        private Dictionary<string, PushEvent> BuildPushEvents(PushEventDto pushEventDto)
        {
            var pushEvents = new Dictionary<string, PushEvent>();
            var matchId = pushEventDto.metadata.sport_event_id;
            var pushEvent = new PushEvent
            {
                MatchResult = SportRadarMatchDataAccess.BuildMatchStatus(pushEventDto.payload.sport_event_status),
                TimeLines = new List<Timeline> { BuildTimeLine(pushEventDto.payload.timeline) }
            };

            pushEvents.Add(matchId, pushEvent);

            return pushEvents;
        }

        private static Timeline BuildTimeLine(TimelineDto timelineDto)
        {
            var timeline = new Timeline();

            if (timelineDto != null)
            {
                timeline.Id = timelineDto.id;
                timeline.Type = timelineDto.type;
                timeline.MatchTime = timelineDto.match_time;
                timeline.Team = timelineDto.team;
                timeline.InjuryTimeAnnounced = timelineDto.injury_time_announced;
                timeline.PeriodType = timelineDto.period_type;
                timeline.Period = timelineDto.period;
            }

            return timeline;
        }



        private static void GenerateMatchTimeForLiveMatch(PushEvent payload)
        {
            if (payload.MatchResult.EventStatus.IsLive())
            {
                var timeline = payload.TimeLines.LastOrDefault();
                var matchTime = timeline.MatchTime;
                var matchStatus = payload.MatchResult.MatchStatus.Value;

                if (timeline.Type == EventTypes.PeriodStart.Value)
                {
                    if (PeriodStartTimes.ContainsKey(matchStatus))
                    {
                        matchTime = PeriodStartTimes[matchStatus];
                    }
                }

                payload.MatchResult.MatchTime = matchTime;
            }
        }
    }
}
