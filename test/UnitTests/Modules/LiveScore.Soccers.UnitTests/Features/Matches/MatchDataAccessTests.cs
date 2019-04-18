namespace LiveScore.Soccers.UnitTests.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches;
    using LiveScore.Shared.Models.Dtos;
    using LiveScore.Soccers.Features.Matches;
    using LiveScore.Soccers.Features.Matches.DataProviders;
    using NSubstitute;
    using Xunit;

    public class MatchDataAccessTests
    {
        private readonly MatchDataAccess matchDataAccess;
        private readonly IMatchApi matchApi;

        public MatchDataAccessTests()
        {
            matchApi = Substitute.For<IMatchApi>();
            matchApi
                .GetDailySchedules(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), "en")
                .Returns(Task.FromResult(StubDailySchedule()));
            matchDataAccess = new MatchDataAccessImpl(matchApi);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnTeams()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var home = match.Teams.ElementAt(0);
            var away = match.Teams.ElementAt(1);

            Assert.Equal(2, match.Teams.Count());

            Assert.Equal("sr:competitor:142194", home.Id);
            Assert.Equal("Kaya FC-Iloilo", home.Name);
            Assert.Equal("Philippines", home.Country);
            Assert.Equal("PHL", home.CountryCode);
            Assert.Equal("KAY", home.Abbreviation);
            Assert.True(home.IsHome);

            Assert.Equal("sr:competitor:135864", away.Id);
            Assert.Equal("PSM Makassar", away.Name);
            Assert.Equal("Indonesia", away.Country);
            Assert.Equal("IDN", away.CountryCode);
            Assert.Equal("MAK", away.Abbreviation);
            Assert.False(away.IsHome);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueInformation()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var league = match.League;

            Assert.Equal("sr:tournament:668", league.Id);
            Assert.Equal("AFC Cup", league.Name);
            Assert.Equal("sr:category:393", league.Category.Id);
            Assert.Equal("International Clubs", league.Category.Name);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnLeagueRound()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var leagueRound = match.LeagueRound;

            Assert.Equal("RoundLeague", leagueRound.Name);
            Assert.Equal("H", leagueRound.Group);
            Assert.Equal(4, leagueRound.Number);
            Assert.Equal("phase", leagueRound.Phase);
            Assert.Equal("group", leagueRound.Type.Value);
        }

        [Fact]
        public async Task GetMatches_Always_ReturnMatchResult()
        {
            var matches = await matchDataAccess.GetMatches(
                1,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                "en");

            var match = matches.First();
            var matchResult = match.MatchResult;
            var firstMatchPeriod = matchResult.MatchPeriods.First();

            Assert.Equal("live", matchResult.EventStatus.Value);
            Assert.Equal("1st_half", matchResult.MatchStatus.Value);
            Assert.Equal(0, matchResult.HomeScore);
            Assert.Equal(2, matchResult.AwayScore);
            Assert.Equal(1, matchResult.Period);
            Assert.Equal("29:17", matchResult.MatchTime);
            Assert.NotEmpty(matchResult.MatchPeriods);
            Assert.Equal(0, firstMatchPeriod.HomeScore);
            Assert.Equal(2, firstMatchPeriod.AwayScore);
            Assert.Equal("regular_period", firstMatchPeriod.PeriodType.Value);
            Assert.Equal(1, firstMatchPeriod.Number);
        }

        private DailyScheduleDto StubDailySchedule()
        {
            return new DailyScheduleDto
            {
                generated_at = new DateTime(2019, 1, 1),
                schema = "schema",
                results = new List<Result>
                {
                    new Result
                    {
                        sport_event = new Sport_Event
                        {
                            tournament_round = new Tournament_Round
                            {
                                    type = "group",
                                    number = 4,
                                    group = "H",
                                    name = "RoundLeague",
                                    phase = "phase"
                            },
                            tournament = new Tournament
                            {
                                id = "sr:tournament:668",
                                name = "AFC Cup",
                                sport = new Sport
                                {
                                    id = "sr:sport:1",
                                    name = "Soccer"
                                },
                                category = new Category
                                {
                                    id = "sr:category:393",
                                    name = "International Clubs"
                                }
                            },
                            season = new Season
                            {
                                id= "sr:season:62299",
                                  name= "AFC Cup 2019",
                                  start_date = "2019-02-05",
                                  end_date = "2019-11-03",
                                  year = "2019",
                                  tournament_id = "sr:tournament:668"
                            },
                            competitors = new List<Competitor>
                            {
                                new Competitor
                                {
                                    id = "sr:competitor:142194",
                                    name = "Kaya FC-Iloilo",
                                    country = "Philippines",
                                    country_code = "PHL",
                                    abbreviation = "KAY",
                                    qualifier = "home"
                                },
                                new Competitor
                                {
                                    id = "sr:competitor:135864",
                                    name = "PSM Makassar",
                                    country = "Indonesia",
                                    country_code = "IDN",
                                    abbreviation = "MAK",
                                    qualifier = "away"
                                }
                            },
                            venue = new Venue
                            {
                              id= "sr:venue:38696",
                              name= "Iloilo Sports Complex",
                              capacity= 7000,
                              city_name= "Iloilo",
                              country_name= "Philippines",
                              map_coordinates= "10.72, 122.57",
                              country_code= "PHL"
                            }
                        },
                        sport_event_status = new Sport_Event_Status
                        {
                            status = "live",
                            match_status = "1st_half",
                            home_score = 0,
                            away_score = 2,
                            period = 1,
                            clock = new Clock { match_time = "29:17" },
                            period_scores = new List<Period_Scores>
                            {
                                new Period_Scores
                                {
                                    home_score = 0,
                                    away_score = 2,
                                    type = "regular_period",
                                    number = 1
                                }
                            }
                        }
                    }
                }
            };
        }
    }


}
