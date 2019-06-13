namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using Fanex.Logging;
    using LiveScore.Features.Matches;
    using LiveScore.Features.Matches.Models;
    using LiveScore.Shared.Hubs;
    using LiveScore.Soccers.Features.Matches;
    using LiveScore.Soccers.Shared.Dtos;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using NSubstitute;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class MatchPushEventHandlerTests
    {
        private readonly MatchPushEventHandler eventHandler;

        private readonly PayloadStreams payloads;

        private readonly IMatchService matchService;
        private readonly IHubContext<MatchHub> hubContext;
        private readonly ILogger logger;

        public MatchPushEventHandlerTests()
        {
            matchService = Substitute.For<IMatchService>();
            hubContext = Substitute.For<IHubContext<MatchHub>>();
            logger = Substitute.For<ILogger>();

            var testJsonData = File.ReadAllText(@"..\netcoreapp2.1\Data\payloads.json");
            payloads = JsonConvert.DeserializeObject<PayloadStreams>(testJsonData);

            eventHandler = new MatchPushEventHandler(matchService, hubContext, logger);
        }

        [Fact]
        public async Task OnReceive_HeartBeatStream()
        {
            var eventStr = "{'heartbeat':{'interval':5000}}";

            await eventHandler.OnReceive(eventStr, hubContext);

            await matchService.DidNotReceive().DeleteLiveMatch(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
            await matchService.DidNotReceive().AddOrUpdateMatch(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<MatchPushEvent>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task OnReceive_InjuryTimeShow()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.injury_time_shown);
            var matchId = payloads.injury_time_shown.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream, hubContext);

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

            await eventHandler.OnReceive(eventStream, hubContext);

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

            await eventHandler.OnReceive(eventStream, hubContext);

            await matchService.Received(1).AddOrUpdateMatch(
                Arg.Any<int>(),
                Arg.Any<string>(),
                matchId,
                Arg.Is<MatchPushEvent>(x =>
                    x.TimeLines.First(t => t.Type.Equals(EventTypes.YellowCard.Value) && !string.IsNullOrWhiteSpace(t.Player.Name)) != null &&
                    x.MatchResult.MatchTime == 40 ),
                true);
        }

        [Fact]
        public async Task OnReceive_YellowRedCard()
        {
            var eventStream = JsonConvert.SerializeObject(payloads.yellow_red_card);
            var matchId = payloads.yellow_red_card.metadata.sport_event_id;

            await eventHandler.OnReceive(eventStream, hubContext);

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

            await eventHandler.OnReceive(eventStream, hubContext);

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
        public async Task Handle_PushEventNull()
        {
            await eventHandler.Handle(null);
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
    }
}
