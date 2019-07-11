namespace LiveScore.Soccers.UnitTests.Features.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LiveScore.Providers.SportRadar.Dtos;
    using LiveScore.Soccers.Shared.ModelBuilders;
    using Xunit;

    [Trait("LiveScore.Soccer", "Odds")]
    public class SportRadarOddsBuilderTests
    {
        [Fact]
        public void BuildMatchOddsList_DtoIsNull_ReturnEmptyList()
        {
            var matchOdds = SportRadarOddsBuilder.BuildMatchOddsList(null);

            Assert.Empty(matchOdds);
        }

        [Fact]
        public void BuildMatchOddsList_NoEvent_ReturnEmptyList()
        {
            var matchOdds = SportRadarOddsBuilder.BuildMatchOddsList(new OddsScheduleDto());

            Assert.Empty(matchOdds);
        }

        [Fact]
        public void BuildMatchOddsList_1X2BetType_ReturnListOddsByMatch()
        {
            var matchOdds = SportRadarOddsBuilder.BuildMatchOddsList(StubOddsScheduleDto());

            Assert.Equal(2, matchOdds.Count());
            Assert.Equal("match_id_1", matchOdds.ElementAt(0).MatchId);
            Assert.Equal("match_id_2", matchOdds.ElementAt(1).MatchId);
        }

        [Fact]
        public void BuildBetTypeOddsList_BetTypeIsNotSupport_ReturnEmptyOddsList()
        {
            var oddsList = SportRadarOddsBuilder.BuildBetTypeOddsList(StubSportEvent("match_id_1", "4way"));

            Assert.Empty(oddsList);
        }

        [Fact]
        public void BuildBetTypeOddsList_1x2Data_ReturnOddsList()
        {
            var oddsList = SportRadarOddsBuilder.BuildBetTypeOddsList(StubSportEvent("match_id_1", "3way"));

            Assert.Equal(3, oddsList.Count());
            Assert.Equal(3, oddsList.Count());
            var firstBetTypeOdds = oddsList.FirstOrDefault();

            Assert.Equal(1, firstBetTypeOdds.Id);
            Assert.Equal("1x2", firstBetTypeOdds.Name);
            Assert.Equal("sr:book:1", firstBetTypeOdds.Bookmaker.Id);
            Assert.Equal("Unibet", firstBetTypeOdds.Bookmaker.Name);
            Assert.Equal(3, firstBetTypeOdds.BetOptions.Count());

            var firstBetOptionOdds = firstBetTypeOdds.BetOptions.FirstOrDefault();
            Assert.Equal("home", firstBetOptionOdds.Type);
            Assert.Equal(4.75m, firstBetOptionOdds.LiveOdds);
            Assert.Equal(3.96m, firstBetOptionOdds.OpeningOdds);
            Assert.Equal("1", firstBetOptionOdds.Name);
            Assert.Equal("2", firstBetTypeOdds.BetOptions.ElementAt(1).Name);
            Assert.Equal("x", firstBetTypeOdds.BetOptions.ElementAt(2).Name);
        }

        private static OddsScheduleDto StubOddsScheduleDto()
            => new OddsScheduleDto
            {
                sport_events = new List<SportEvent>
                {
                    StubSportEvent("match_id_1", "3way"),
                    StubSportEvent("match_id_2", "3way")
                }
            };

        private static SportEvent StubSportEvent(string matchId, string betTypeName)
        {
            return new SportEvent
            {
                id = matchId,
                markets = new List<Market>
                        {
                            new Market
                            {
                                odds_type_id = 2,
                                name = betTypeName,
                                group_name = "regular",
                                books = new List<Book>
                                {
                                    StubBook("sr:book:1", "Unibet"),
                                    StubBook("sr:book:2", "Unibet 2"),
                                    StubBook("sr:book:3", "Unibet 3")
                                }
                            }
                        }

            };
        }

        private static Book StubBook(string id, string name)
            => new Book
            {
                id = id,
                name = name,
                outcomes = new List<Outcome>
                {
                    new Outcome { type = "home", odds = "4.751", opening_odds = "3.9555" },
                    new Outcome { type = "away", odds = "1.75", opening_odds = "2.950" },
                    new Outcome { type = "draw", odds = "2.75", opening_odds = "1.950" }
                }
            };
    }
}
