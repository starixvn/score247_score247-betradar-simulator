namespace LiveScore.Soccers.UnitTests.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Database.Entities;
    using LiveScore.Features.Odds;
    using LiveScore.Soccer.Configurations;
    using LiveScore.Soccer.Database;
    using LiveScore.Soccer.Matches;
    using LiveScore.Soccer.Matches.Models;
    using LiveScore.Soccer.Odds.Models;
    using Newtonsoft.Json;
    using NSubstitute;
    using Xunit;

    [Trait("LiveScore.Soccer", "Odds")]
    public class OddsServiceTests
    {
        private readonly OddsServiceImpl oddsServiceImpl;
        private readonly IDynamicRepository dynamicRepository;
        private readonly IMatchService matchService;
        private readonly IAppSettings appSettings;

        private const string bookMakerId = "sr:book:202";
        private const string matchId = "matchId1";
        private const string language = "en-US";
        private DateTime eventDate = new DateTime(2019, 2, 3);

        public OddsServiceTests()
        {
            dynamicRepository = Substitute.For<IDynamicRepository>();
            matchService = Substitute.For<IMatchService>();
            appSettings = Substitute.For<IAppSettings>();
            oddsServiceImpl = new OddsServiceImpl(dynamicRepository, matchService, appSettings);
        }

        [Fact]
        public async Task GetOdds_Always_ReturnMatchId()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var betTypeId = 2;

            var matchOdds = await oddsServiceImpl.GetOdds(matchId, betTypeId, string.Empty);

            Assert.Equal(matchId, matchOdds.MatchId);
        }

        [Fact]
        public async Task GetOdds_Always_ReturnBetTypeOddsList()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());

            var actualBetTypeOddsList = await oddsServiceImpl.GetOdds("matchId", 1, string.Empty);

            var firstBetTypeOdds = actualBetTypeOddsList.BetTypeOddsList.First();
            var secondBetTypeOdds = actualBetTypeOddsList.BetTypeOddsList.ElementAt(1);

            Assert.Equal(2, actualBetTypeOddsList.BetTypeOddsList.Count());
            //Order by Bookmaker Name
            Assert.Equal("sr:book:201", firstBetTypeOdds.Bookmaker.Id);
            Assert.Equal("sr:book:202", secondBetTypeOdds.Bookmaker.Id);
            //Get last odds for each bookmaker
            Assert.Equal(1.3m, firstBetTypeOdds.BetOptions.ElementAt(0).LiveOdds);
            Assert.Equal(1.4m, secondBetTypeOdds.BetOptions.ElementAt(0).LiveOdds);
        }

        [Fact]
        public async Task GetBetTypeOddsList_IsNotNewOdds_DontCallInsertRange()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var matchOdds = StubMatchOdds();

            await oddsServiceImpl.InsertOdds(matchOdds);

            await dynamicRepository.DidNotReceive().InsertRange(Arg.Any<IEnumerable<OddsEntity>>());
        }

        [Fact]
        public async Task GetBetTypeOddsList_IsNewOdds_CallInsertRange()
        {
            var lastUpdatedTime = new DateTimeOffset(2019, 4, 2, 0, 0, 0, new TimeSpan(7, 0, 0)).DateTime;
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());
            var matchOdds = StubMatchOdds(lastUpdatedTime);

            await oddsServiceImpl.InsertOdds(matchOdds);

            await dynamicRepository.Received().InsertRange(
                Arg.Is<IEnumerable<OddsEntity>>(
                    entities => entities.Count() == 2
                                && entities.First().BookmakerId == "sr:book:201"
                                && entities.First().MatchId == "matchId1"
                                && entities.ElementAt(1).BookmakerId == "sr:book:202"
                ));
        }

        [Fact]
        public async Task GetBetTypeOddsList_Always_AssignOpeningOdds()
        {
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(StubBetTypeOddsEntities());

            var actualBetTypeOddsList = await oddsServiceImpl.GetOdds("matchId", 1, string.Empty);

            var firstBetTypeOdds = actualBetTypeOddsList.BetTypeOddsList.First();

            Assert.Equal(1.5m, firstBetTypeOdds.BetOptions.First().OpeningOdds);
            Assert.Equal(1.3m, firstBetTypeOdds.BetOptions.First().LiveOdds);
            Assert.Equal(1.4m, firstBetTypeOdds.BetOptions.ElementAt(1).OpeningOdds);
            Assert.Equal(1.6m, firstBetTypeOdds.BetOptions.ElementAt(2).OpeningOdds);
        }

        [Fact]
        public async Task GetOddsMovement_NotFoundBookmaker_ReturnEmpty()
        {
            var emptyOddEntities = new List<OddsEntity>();
            dynamicRepository
                .Query(Arg.Any<Func<OddsEntity, bool>>())
                .Returns(emptyOddEntities);

            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement("matchId", 1, "bookmarker", "en-US");

            Assert.Null(matchOddsMovement.Bookmaker);
            Assert.Null(matchOddsMovement.OddsMovements);
        }

        [Fact]
        public async Task GetOddsMovement_BookMakerHasData_BookMakerInfo()
        {
            var oddsEntities = StubBetTypeOddsEntities(bookMakerId);
            dynamicRepository
                .Query(Arg.Any<Func<OddsEntity, bool>>())
                .Returns(oddsEntities);

            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement("matchId1", 1, bookMakerId, "en-US");

            Assert.Equal("matchId1", matchOddsMovement.MatchId);
            Assert.Equal(bookMakerId, matchOddsMovement.Bookmaker.Id);
        }

        [Fact]
        public async Task GetOddsMovement_MatchDoesNotExist_ReturnEmptyOddsMovement()
        {
            var oddsEntities = StubBetTypeOddsEntities(bookMakerId);
            dynamicRepository
                .Query(Arg.Any<Func<OddsEntity, bool>>())
                .Returns(oddsEntities);
            matchService.GetMatch(matchId, language).Returns(Task.FromResult<Match>(null));

            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement("matchId1", 1, bookMakerId, "en-US");

            Assert.Empty(matchOddsMovement.OddsMovements);
        }

        [Fact]
        public async Task GetOddsMovement_MatchDidNotStart_ReturnOddsMovementBeforeMatchStarted()
        {
            // Arrange
            var oddsEntities = StubBetTypeOddsEntitiesForOddsMovement();
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            StubMatch(matchId);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(5, matchOddsMovement.OddsMovements.Count());

            var openingOdds = matchOddsMovement.OddsMovements.Last();
            Assert.Equal("Opening", openingOdds.MatchTime);
            Assert.False(openingOdds.IsMatchStarted);
            Assert.Equal(1.5m, openingOdds.BetOptions.First().LiveOdds);
            Assert.Equal(1.5m, openingOdds.BetOptions.First().OpeningOdds);

            var firstOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("Live", firstOdds.MatchTime);
            Assert.False(firstOdds.IsMatchStarted);
            Assert.Equal(1.35m, firstOdds.BetOptions.First().LiveOdds);

            Assert.Equal("Live", matchOddsMovement.OddsMovements.ElementAt(1).MatchTime);
        }

        [Fact]
        public async Task GetOddsMovement_MatchJustKickedOff_ReturnOddsWithKickoffInfo()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(-10), 0.15m, 0.1m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(-5), 0.15m, 0.1m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m)
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(3, matchOddsMovement.OddsMovements.Count());

            var kickoffOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("KO", kickoffOdds.MatchTime);
            Assert.True(kickoffOdds.IsMatchStarted);
            Assert.Equal(0, kickoffOdds.HomeScore);
            Assert.Equal(0, kickoffOdds.AwayScore);
            Assert.Equal(1.36m, kickoffOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Up, kickoffOdds.BetOptions.First().OddsTrend);
        }

        [Fact]
        public async Task GetOddsMovement_ScoreChangeInFirstHalf_ReturnScoreChangeOdds()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.ScoreChange.Value, HomeScore = 1, AwayScore = 1, MatchTime = 20, Time = eventDate.AddMinutes(20) }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(-10), 0.15m, 0.1m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(20), 0.14m)
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(3, matchOddsMovement.OddsMovements.Count());

            var firstHalfScoreOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("20'", firstHalfScoreOdds.MatchTime);
            Assert.True(firstHalfScoreOdds.IsMatchStarted);
            Assert.Equal(1, firstHalfScoreOdds.HomeScore);
            Assert.Equal(1, firstHalfScoreOdds.AwayScore);
            Assert.Equal(1.34m, firstHalfScoreOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Down, firstHalfScoreOdds.BetOptions.First().OddsTrend);
        }

        [Fact]
        public async Task GetOddsMovement_OddChangeInHalfTimeBreak_ReturnHalfTimeOdds()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.BreakStart.Value, PeriodType = "pause", Time = eventDate.AddMinutes(55) }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(55), 0.474m)
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(3, matchOddsMovement.OddsMovements.Count());

            var halfTimeOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("HT", halfTimeOdds.MatchTime);
            Assert.True(halfTimeOdds.IsMatchStarted);
            Assert.Equal(0, halfTimeOdds.HomeScore);
            Assert.Equal(0, halfTimeOdds.AwayScore);
            Assert.Equal(1.674m, halfTimeOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Up, halfTimeOdds.BetOptions.First().OddsTrend);
        }

        [Fact]
        public async Task GetOddsMovement_SecondHalfStart_ReturnSecondHalfStartOdds()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.ScoreChange.Value, HomeScore = 1, AwayScore = 1, MatchTime = 20, Time = eventDate.AddMinutes(20) },
                new Timeline { Type = EventTypes.BreakStart.Value, PeriodType = "pause", Time = eventDate.AddMinutes(55) },
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 2, Time = eventDate.AddMinutes(70), PeriodType = PeriodType.RegularPeriod.Value }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(20), 0.14m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(55), 0.474m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(70), 0.574m)
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(5, matchOddsMovement.OddsMovements.Count());

            var secondHalfStartOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("46'", secondHalfStartOdds.MatchTime);
            Assert.True(secondHalfStartOdds.IsMatchStarted);
            Assert.Equal(1, secondHalfStartOdds.HomeScore);
            Assert.Equal(1, secondHalfStartOdds.AwayScore);
            Assert.Equal(1.774m, secondHalfStartOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Up, secondHalfStartOdds.BetOptions.First().OddsTrend);
        }

        [Fact]
        public async Task GetOddsMovement_ScoreChangeInSecondHalf_ReturnScoreChangeOdds()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.BreakStart.Value, PeriodType = "pause", Time = eventDate.AddMinutes(55) },
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 2, Time = eventDate.AddMinutes(70), PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.ScoreChange.Value, HomeScore = 2, AwayScore = 1, MatchTime = 56, Time = eventDate.AddMinutes(80) }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(55), 0.474m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(70), 0.574m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(80), 0.14m),
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(5, matchOddsMovement.OddsMovements.Count());

            var secondHalfStartOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("56'", secondHalfStartOdds.MatchTime);
            Assert.True(secondHalfStartOdds.IsMatchStarted);
            Assert.Equal(2, secondHalfStartOdds.HomeScore);
            Assert.Equal(1, secondHalfStartOdds.AwayScore);
            Assert.Equal(1.34m, secondHalfStartOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Down, secondHalfStartOdds.BetOptions.First().OddsTrend);
        }

        [Fact]
        public async Task GetOddsMovement_OddsChangeInSecondHalf_ReturnOddsChange()
        {
            // Arrange
            var timeLines = new List<Timeline>
            {
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 1, Time = eventDate, PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.BreakStart.Value, PeriodType = "pause", Time = eventDate.AddMinutes(55) },
                new Timeline { Type = EventTypes.PeriodStart.Value, Period = 2, Time = eventDate.AddMinutes(70), PeriodType = PeriodType.RegularPeriod.Value },
                new Timeline { Type = EventTypes.ScoreChange.Value, HomeScore = 2, AwayScore = 1, MatchTime = 56, Time = eventDate.AddMinutes(80) }
            };
            var match = StubMatch(matchId, eventDate, timeLines);

            var oddsEntities = new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate, 0.16m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(55), 0.474m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(70), 0.574m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(80), 0.14m),
                StubOddEntity(bookMakerId, "bookMakerName", match.EventDate.AddMinutes(90), 0.12m),
            };
            dynamicRepository.Query(Arg.Any<Func<OddsEntity, bool>>()).Returns(oddsEntities);

            // Act
            var matchOddsMovement = await oddsServiceImpl.GetOddsMovement(matchId, 1, bookMakerId, language);

            // Assert
            Assert.Equal(6, matchOddsMovement.OddsMovements.Count());

            var secondHalfStartOdds = matchOddsMovement.OddsMovements.First();
            Assert.Equal("66'", secondHalfStartOdds.MatchTime);
            Assert.True(secondHalfStartOdds.IsMatchStarted);
            Assert.Equal(2, secondHalfStartOdds.HomeScore);
            Assert.Equal(1, secondHalfStartOdds.AwayScore);
            Assert.Equal(1.32m, secondHalfStartOdds.BetOptions.First().LiveOdds);
            Assert.Equal(OddsTrend.Down, secondHalfStartOdds.BetOptions.First().OddsTrend);
        }

        public Match StubMatch(
            string matchId = "matchId1",
            DateTime? eventDate = null,
            IEnumerable<Timeline> timelines = null)
        {
            var match = new Match
            {
                Id = matchId,
                EventDate = eventDate.HasValue
                    ? eventDate.Value
                    : new DateTime(2019, 2, 3),
                TimeLines = timelines
            };

            matchService.GetMatch(matchId, "en-US").Returns(Task.FromResult(match));

            return match;
        }

        private static MatchOdds StubMatchOdds(DateTime? lastUpdatedTime = null)
            => new MatchOdds
            {
                MatchId = "matchId1",
                LastUpdated = lastUpdatedTime ?? new DateTimeOffset(2019, 1, 2, 0, 0, 0, new TimeSpan(7, 0, 0)).DateTime,
                BetTypeOddsList = new List<BetTypeOdds>
                {
                    StubOneXTwoBetTypeOdds("sr:book:201", "bookmakername 1", 0.2m, 0.2m),
                    StubOneXTwoBetTypeOdds("sr:book:202", "bookmakername 2", 0.3m, 0.3m)
                }
            };

        private static BetTypeOdds StubOneXTwoBetTypeOdds(
            string bookMakerId = "sr:book:201",
            string bookMarkerName = "bookmakername 1",
            decimal oddsOffset = 0m,
            decimal openingOddsOffset = 0m)
            => new BetTypeOdds
            {
                Id = 1,
                Name = "1x2",
                Bookmaker = new Bookmaker { Id = bookMakerId, Name = bookMarkerName },
                BetOptions = new List<BetOptionOdds>
                        {
                            new BetOptionOdds { LiveOdds = 1.2m + oddsOffset, OpeningOdds = 1.5m + openingOddsOffset, Type = "Home" },
                            new BetOptionOdds { LiveOdds = 1.3m + oddsOffset, OpeningOdds = 1.4m + openingOddsOffset, Type = "Away" },
                            new BetOptionOdds { LiveOdds = 1.1m + oddsOffset, OpeningOdds = 1.6m + openingOddsOffset, Type = "Draw" },
                        }
            };

        private static List<OddsEntity> StubBetTypeOddsEntitiesForOddsMovement(
            bool returnOddsAfterMatchStarted = false,
            DateTime? eventDate = null,
            string bookMakerId = "sr:book:201",
            string bookMakerName = "bookmakername 1")
        {
            var matchStartTime = eventDate.HasValue ? eventDate.Value : new DateTime(2019, 1, 1);

            return new List<OddsEntity>
            {
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(-120), 0.11m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(-80), 0.12m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(-40), 0.13m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(-20), 0.14m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(-10), 0.15m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime, 0.15m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(10), 0.16m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(20), 0.17m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(50), 0.18m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(55), 0.19m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(62), 0.20m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(75), 0.21m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(85), 0.22m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(88), 0.23m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(92), 0.24m),
                StubOddEntity(bookMakerId, bookMakerName, matchStartTime.AddMinutes(98), 0.25m)
            }
            .Where(entity => returnOddsAfterMatchStarted || entity.ModifiedTime < matchStartTime)
            .ToList();
        }

        private static OddsEntity StubOddEntity(
            string bookMakerId,
            string bookMakerName,
            DateTime lastUpdatedTime,
            decimal oddsOffset = 0m,
            decimal openingOddsOffset = 0m)
            => new OddsEntity
            {
                Value = JsonConvert.SerializeObject(StubOneXTwoBetTypeOdds(bookMakerId, bookMakerName, oddsOffset, openingOddsOffset)),
                BetTypeId = 1,
                MatchId = "matchId1",
                BookmakerId = bookMakerId,
                ModifiedTime = lastUpdatedTime
            };

        private static IEnumerable<OddsEntity> StubBetTypeOddsEntities(string bookMakerId = "")
        {
            var odds1 = StubOneXTwoBetTypeOdds("sr:book:201", "bookmakername 1", 0.1m, 0.2m);
            var odds2 = StubOneXTwoBetTypeOdds("sr:book:202", "bookmakername 2", 0.2m, 0.3m);

            var oddsEntities = new List<OddsEntity>
            {
                new OddsEntity
                {
                    Value = JsonConvert.SerializeObject(odds2),
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:202",
                    ModifiedTime = new DateTimeOffset(2019, 2, 2, 0, 0, 0, new TimeSpan(6, 0 ,0))
                },
                new OddsEntity
                {
                    Value = JsonConvert.SerializeObject(StubOneXTwoBetTypeOdds()),
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:201",
                    ModifiedTime = new DateTimeOffset(2019, 1, 2, 0, 0, 0, new TimeSpan(7, 0 ,0))
                },
                new OddsEntity
                {
                    Value = JsonConvert.SerializeObject(odds1),
                    BetTypeId = 1,
                    MatchId = "matchId1",
                    BookmakerId = "sr:book:201",
                    ModifiedTime = new DateTimeOffset(2019, 1, 3, 0, 0, 0, new TimeSpan(7, 0 ,0))
                }
            };

            return oddsEntities
                    .Where(entity => bookMakerId == "" || entity.BookmakerId == bookMakerId)
                    .ToList();
        }
    }
}