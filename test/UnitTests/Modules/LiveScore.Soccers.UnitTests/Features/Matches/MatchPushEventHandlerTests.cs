namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Soccer.SignalRHub.Match;
    using LiveScore.Soccer.SignalRHub.Match.EventHandling;
    using LiveScore.SportRadar.Dtos;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using Xunit;

    public class MatchPushEventHandlerTests
    {
        private readonly MatchPushEventHandler eventHandler;

        private readonly PayloadStreams payloads;
        private readonly IHubContext<MatchHub> hubContext;
        private readonly IMatchService matchService;
        private readonly ILogger logger;
        private readonly IPenaltyEventHandler penaltyEventHandler;

        public MatchPushEventHandlerTests()
        {
            hubContext = Substitute.For<IHubContext<MatchHub>>();
            matchService = Substitute.For<IMatchService>();
            logger = Substitute.For<ILogger>();
            penaltyEventHandler = Substitute.For<IPenaltyEventHandler>();

            var testJsonData = File.ReadAllText(@"..\netcoreapp2.2\TestData\payloads.json");
            payloads = JsonConvert.DeserializeObject<PayloadStreams>(testJsonData);

            eventHandler = new MatchPushEventHandler(matchService, logger, penaltyEventHandler, hubContext);
        }

        [Fact]
        public async Task OnReceive_HeartBeatStream()
        {
            var eventStr = "{'heartbeat':{'interval':5000}}";

            await eventHandler.OnReceive(eventStr);

            await matchService.DidNotReceive().DeleteLiveMatch(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
            await matchService.DidNotReceive().AddOrUpdateMatch(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<MatchPushEvent>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task OnReceive_InjuryTimeShow()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.injury_time_shown);
            var matchId = payloads.injury_time_shown.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.InjuryTimeShown.Value)) != null &&
                    x.MatchResult.MatchTime == 46),
                true);
        }

        [Fact]
        public async Task OnReceive_BreakStart()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.break_start);
            var matchId = payloads.break_start.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.BreakStart.Value)) != null),
                true);
        }

        [Fact]
        public async Task OnReceive_YellowCard()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.yellow_card);
            var matchId = payloads.yellow_card.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.YellowCard.Value) && !string.IsNullOrWhiteSpace(t.Player.Name)) != null &&
                    x.MatchResult.MatchTime == 40),
                true);
        }

        [Fact]
        public async Task OnReceive_YellowRedCard()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.yellow_red_card);
            var matchId = payloads.yellow_red_card.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.YellowRedCard.Value) && !string.IsNullOrWhiteSpace(t.Player.Name)) != null),
                true);
        }

        [Fact]
        public async Task OnReceive_RedCard()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.red_card);
            var matchId = payloads.red_card.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.RedCard.Value) && !string.IsNullOrWhiteSpace(t.Player.Name)) != null),
                true);
        }

        [Fact]
        public async Task OnReceive_ScoreChange()
        {
            var eventType = payloads.score_change;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.ScoreChange.Value) && !string.IsNullOrWhiteSpace(t.GoalScorer.Name)) != null),
                true);
        }

        [Fact]
        public async Task OnReceive_ScoreChangeWithAssist()
        {
            var eventType = payloads.score_change_assist;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t =>
                        t.Type.Equals(EventTypes.ScoreChange.Value) &&
                        !string.IsNullOrWhiteSpace(t.GoalScorer.Name) &&
                        !string.IsNullOrWhiteSpace(t.Assist.Name)) != null),
                true);
        }

        [Fact]
        public async Task OnReceive_MatchEnded()
        {
            var eventType = payloads.match_ended;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            // MatchTime == 0 ?!?
            await matchService.Received(1).DeleteLiveMatch(
               Arg.Any<int>(),
               Arg.Any<string>(),
               matchId);

            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.MatchEnded.Value)) != null &&
                    x.MatchResult.EventStatus == MatchStatus.Ended &&
                    x.MatchResult.MatchStatus == MatchStatus.Ended),
                false);
        }

        [Fact]
        public async Task OnReceive_MatchStarted()
        {
            var eventType = payloads.match_started;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            await matchService.Received(1).AddOrUpdateMatch(
               Arg.Any<int>(),
               Arg.Any<string>(),
               matchId,
               Arg.Is<MatchPushEvent>(x =>
                   x.TimeLines.First(t => t.Type.Equals(EventTypes.MatchStarted.Value)) != null &&
                   x.MatchResult.EventStatus == MatchStatus.Live &&
                   x.MatchResult.MatchStatus == MatchStatus.FirstHaft),
               true);
        }

        [Fact]
        public async Task OnReceive_FirstPeriodStart()
        {
            var eventType = payloads.first_period_start;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            await matchService.Received(1).AddOrUpdateMatch(
               Arg.Any<int>(),
               Arg.Any<string>(),
               matchId,
               Arg.Is<MatchPushEvent>(x =>
                   x.TimeLines.First(t => t.Type.Equals(EventTypes.PeriodStart.Value)) != null &&
                   x.MatchResult.Period == 1 &&
                   x.MatchResult.EventStatus == MatchStatus.Live &&
                   x.MatchResult.MatchStatus == MatchStatus.FirstHaft &&
                   x.MatchResult.MatchTime == 1),
               true);
        }

        [Fact]
        public async Task OnReceive_SecondPeriodStart()
        {
            var eventType = payloads.second_period_start;
            var eventStream = JsonConvert.SerializeObject(eventType);
            var matchId = eventType.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream);

            await matchService.Received(1).AddOrUpdateMatch(
               Arg.Any<int>(),
               Arg.Any<string>(),
               matchId,
               Arg.Is<MatchPushEvent>(x =>
                   x.TimeLines.First(t => t.Type.Equals(EventTypes.PeriodStart.Value)) != null &&
                   x.MatchResult.Period == 2 &&
                   x.MatchResult.EventStatus == MatchStatus.Live &&
                   x.MatchResult.MatchStatus == MatchStatus.SecondHaft &&
                   x.MatchResult.MatchTime == 46),
               true);
        }

        [Fact]
        public async Task Handle_PushEventNull()
        {
            await eventHandler.Handle(null);
        }

        [Fact]
        public async Task OnReceive_ThrowsException_ShouldLogError()
        {
            var eventType = payloads.second_period_start;
            var eventStream = JsonConvert.SerializeObject(eventType);

            matchService.AddOrUpdateMatch(
               Arg.Any<int>(),
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<MatchPushEvent>(),
               true).Throws(new ArgumentNullException());

            await eventHandler.OnReceive(eventStream);

            await logger.Received(1).ErrorAsync(Arg.Any<string>(), Arg.Any<ArgumentNullException>());
        }

        [Fact]
        public async Task OnReceive_EventIsPenalty_CallPenaltyEventHandler()
        {
            var eventType = payloads.penalty;
            var eventStream = JsonConvert.SerializeObject(eventType);

            await eventHandler.OnReceive(eventStream);

            await penaltyEventHandler.Received(1).Handle(
                eventType.metadata.sport_event_id,
                Arg.Is<Timeline>(timeline =>
                    timeline.IsHome
                    && !timeline.IsHomeShootoutScored
                    && timeline.Player.Name == "Azpilicueta, Cesar, h1"));
        }
    }

    public class PayloadStreams
    {
        public MatchPushEventDto break_start { get; set; }

        public MatchPushEventDto period_score { get; set; }

        public MatchPushEventDto injury_time_shown { get; set; }

        public MatchPushEventDto yellow_card { get; set; }

        public MatchPushEventDto score_change { get; set; }

        public MatchPushEventDto yellow_red_card { get; set; }

        public MatchPushEventDto red_card { get; set; }

        public MatchPushEventDto score_change_assist { get; set; }

        public MatchPushEventDto match_ended { get; set; }

        public MatchPushEventDto match_started { get; set; }

        public MatchPushEventDto first_period_start { get; set; }

        public MatchPushEventDto second_period_start { get; set; }

        public MatchPushEventDto penalty { get; set; }
    }
}